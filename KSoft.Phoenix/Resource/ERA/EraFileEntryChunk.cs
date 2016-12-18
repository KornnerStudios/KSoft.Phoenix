using Contracts = System.Diagnostics.Contracts;
using Contract = System.Diagnostics.Contracts.Contract;

namespace KSoft.Phoenix.Resource
{
	/*public*/ sealed class EraFileEntryChunk
		: ECF.EcfChunk
	{
		#region SizeOf
		public new const int kSizeOf = 0x38;
		public const int kCompresssedDataTigerHashSize = 16;
		public const uint kExtraDataSize = kSizeOf - ECF.EcfChunk.kSizeOf;

		public static int CalculateFileChunksSize(int fileCount)
		{
			return kSizeOf * fileCount;
		}
		#endregion

		#region Struct fields
		public ulong FileTime; // bit encoded file time
		public int DataUncompressedSize;
		// First 128 bits of the compressed data's Tiger hash
		public byte[] CompressedDataTiger128 = new byte[kCompresssedDataTigerHashSize];
		// actually only 24 bits, big endian
		public uint FileNameOffset;
		#endregion

		public string FileName;

		#region IEndianStreamSerializable Members
		public override void Serialize(IO.EndianStream s)
		{
			var eraUtil = s.Owner as EraFileUtil;
			long position = s.BaseStream.Position;

			base.Serialize(s);

			s.Stream(ref FileTime);
			s.Stream(ref DataUncompressedSize);

			if (s.IsWriting)
			{
				Bitwise.ByteSwap.SwapInt64(CompressedDataTiger128, sizeof(ulong) * 0);
				Bitwise.ByteSwap.SwapInt64(CompressedDataTiger128, sizeof(ulong) * 1);
			}
			s.Stream(CompressedDataTiger128);
			{
				Bitwise.ByteSwap.SwapInt64(CompressedDataTiger128, sizeof(ulong) * 0);
				Bitwise.ByteSwap.SwapInt64(CompressedDataTiger128, sizeof(ulong) * 1);
			}


			s.StreamUInt24(ref FileNameOffset);
			s.Pad8();

			if (eraUtil != null && eraUtil.DebugOutput != null)
			{
				eraUtil.DebugOutput.Write("FileEntry: {0} @{1} offset={2} end={3} size={4} dsize={5} adler={6} ",
					base.EntryId.ToString("X16"),
					position.ToString("X8"),
					base.DataOffset.u32.ToString("X8"),
					(base.DataOffset.u32 + base.DataSize).ToString("X8"),
					base.DataSize.ToString("X8"),
					DataUncompressedSize.ToString("X8"),
					base.Adler32.ToString("X8"));

				if (!string.IsNullOrEmpty(FileName))
					eraUtil.DebugOutput.Write(FileName);

				eraUtil.DebugOutput.WriteLine();
			}
		}
		#endregion

		#region Xml Streaming
		protected override void WriteFields(IO.XmlElementStream s, bool includeFileData)
		{
			if (FileTime != 0)
				s.WriteAttribute("fileTime", FileTime.ToString("X16"));

			base.WriteFields(s, includeFileData);

			// When we extract, we decode xmbs
			string fn = FileName;
			if (EraFile.IsXmbFile(fn))
			{
				bool remove_xmb_ext = true;

				var expander = s.Owner as EraFileExpander;
				if (expander != null && expander.ExpanderOptions.Test(EraFileExpanderOptions.DontTranslateXmbFiles))
					remove_xmb_ext = false;

				if (remove_xmb_ext)
					EraFile.RemoveXmbExtension(ref fn);
			}
			s.WriteAttribute("name", fn);

			if (includeFileData)
			{
				if (DataUncompressedSize != DataSize)
					s.WriteAttribute("fullSize", DataUncompressedSize.ToString("X8"));

				s.WriteAttribute("compressedDataHash",
					Text.Util.ByteArrayToString(CompressedDataTiger128));

				s.WriteAttribute("nameOffset", FileNameOffset.ToString("X6"));
			}
		}

		protected override void ReadFields(IO.XmlElementStream s, bool includeFileData)
		{
			base.ReadFields(s, includeFileData);

			s.ReadAttributeOpt("fileTime", ref FileTime, NumeralBase.Hex);

			s.ReadAttribute("name", ref FileName);

			s.ReadAttributeOpt("fullSize", ref DataUncompressedSize, NumeralBase.Hex);
			s.ReadAttributeOpt("nameOffset", ref FileNameOffset, NumeralBase.Hex);

			string hashString = null;
			if (s.ReadAttributeOpt("compressedDataHash", ref hashString))
				CompressedDataTiger128 = Text.Util.ByteStringToArray(hashString);
		}
		#endregion

		[Contracts.Pure]
		public uint ComputeAdler32(IO.EndianStream blockStream)
		{
			Contract.Requires(blockStream != null);

			SeekTo(blockStream);
			uint adler = Security.Cryptography.Adler32.Compute(blockStream.BaseStream, DataSize);
			return adler;
		}

		[Contracts.Pure]
		public void ComputeHash(IO.EndianStream blockStream, Security.Cryptography.TigerHashBase hasher)
		{
			Contract.Requires(blockStream != null);
			Contract.Requires(hasher != null);

			hasher.Initialize();
			hasher.ComputeHash(blockStream.BaseStream,
				(long)DataOffset, DataSize);
		}

		void CalculateDecompressedDataTiger(System.IO.Stream source, Security.Cryptography.TigerHashBase hasher)
		{
			hasher.ComputeHash(source, 0, (int)source.Length,
				restorePosition: true);

			ulong tiger64;
			hasher.TryGetAsTiger64(out tiger64);
			this.DecompressedDataTiger64 = tiger64;
		}

		#region Buffer Util
		byte[] DecompressFromBuffer(IO.EndianStream blockStream, byte[] buffer)
		{
			uint result_adler;
			return EraFile.Decompress(buffer, DataUncompressedSize, out result_adler);
		}
		byte[] DecompressFromStream(IO.EndianStream blockStream)
		{
			SeekTo(blockStream);
			return CompressedStream.DecompressFromStream(blockStream);
		}
		public override byte[] GetBuffer(IO.EndianStream blockStream)
		{
			byte[] result = null;

			switch (CompressionType)
			{
				case ECF.EcfCompressionType.Stored:
					result = GetRawBuffer(blockStream);
					break;

				case ECF.EcfCompressionType.DeflateRaw:
					result = GetRawBuffer(blockStream);
					result = DecompressFromBuffer(blockStream, result);
					break;

				case ECF.EcfCompressionType.DeflateStream:
					result = DecompressFromStream(blockStream);
					break;

				default:
					throw new KSoft.Debug.UnreachableException(CompressionType.ToString());
			}

			return result;
		}

		void CompressSourceToStream(IO.EndianWriter blockStream, System.IO.Stream sourceFile)
		{
			// Read the source bytes
			byte[] buffer = new byte[sourceFile.Length];
			for (int x = 0; x < buffer.Length; )
			{
				int n = sourceFile.Read(buffer, x, buffer.Length - x);
				x += n;
			}

			// Compress the source bytes into a new buffer
			byte[] result = EraFile.Compress(buffer, out base.Adler32);  // Also update this ECF's checksum
			base.DataSize = result.Length; // Update this ECF's size

			// Write the compressed bytes to the block stream
			blockStream.Write(result);
		}
		void CompressSourceToCompressionStream(IO.EndianWriter blockStream, System.IO.Stream sourceFile)
		{
			// Build a CompressedStream from the source file and write it to the block stream
			CompressedStream.CompressFromStream(blockStream, sourceFile,
				out base.Adler32, out base.DataSize);  // Update this ECF's checksum and size
		}
		public void BuildBuffer(IO.EndianStream blockStream, System.IO.Stream sourceFile,
			Security.Cryptography.TigerHashBase hasher)
		{
			blockStream.AlignToBoundry(base.DataAlignmentBit);

			sourceFile.Seek(0, System.IO.SeekOrigin.Begin);
			CalculateDecompressedDataTiger(sourceFile, hasher);

			base.DataOffset = blockStream.PositionPtr;
			this.DataUncompressedSize = (int)sourceFile.Length;

			switch (CompressionType)
			{
				case ECF.EcfCompressionType.Stored:
				{
					base.DataSize = (int)sourceFile.Length;  // Update this ECF's size
					base.Adler32 = Security.Cryptography.Adler32.Compute(sourceFile, base.DataSize); // Also update this ECF's checksum
					// Copy the source file's bytes to the block stream
					sourceFile.CopyTo(blockStream.BaseStream);
					break;
				}

				case ECF.EcfCompressionType.DeflateRaw:
					CompressSourceToStream(blockStream.Writer, sourceFile);
					break;

				case ECF.EcfCompressionType.DeflateStream:
					CompressSourceToCompressionStream(blockStream.Writer, sourceFile);
					break;

				default:
					throw new KSoft.Debug.UnreachableException(CompressionType.ToString());
			}

			ComputeHash(blockStream, hasher);
			System.Array.Copy(hasher.Hash, 0, CompressedDataTiger128, 0, CompressedDataTiger128.Length);
		}
		#endregion

		public override string ToString()
		{
			return string.Format("{0}",
				FileName);
		}

		bool ShouldUnpack(EraFileExpander expander, string path)
		{
			if (expander.ExpanderOptions.Test(EraFileExpanderOptions.DontOverwriteExistingFiles))
			{
				// it's an XMB file and the user didn't say NOT to translate them
				if (EraFile.IsXmbFile(path) && !expander.ExpanderOptions.Test(EraFileExpanderOptions.DontTranslateXmbFiles))
				{
					EraFile.RemoveXmbExtension(ref path);
				}

				if (System.IO.File.Exists(path))
				{
					return false;
				}
			}
			return true;
		}
		void Unpack(IO.EndianStream blockStream, EraFileExpander expander, string path, byte[] chunk)
		{
			const System.IO.FileAccess k_mode = System.IO.FileAccess.Read;

			byte[] buffer = chunk;

			bool translate_xmb_files = true;
			if (expander != null && expander.ExpanderOptions.Test(EraFileExpanderOptions.DontTranslateXmbFiles))
			{
				translate_xmb_files = false;
			}

			if (EraFile.IsXmbFile(path) && translate_xmb_files)
			{
				#region Read XMB chunk
				using (var xmb = new ECF.EcfFileXmb())
				using (var ms = new System.IO.MemoryStream(chunk))
				using (var es = new IO.EndianStream(ms, blockStream.ByteOrder, permissions: k_mode))
				{
					es.StreamMode = k_mode;
					xmb.Serialize(es);

					buffer = xmb.FileData;
				}
				#endregion

				EraFile.RemoveXmbExtension(ref path);

				#region Translate XMB to XML
				var vaSize = Shell.ProcessorSize.x32;
				var builtFor64Bit = expander.Options.Test(EraFileUtilOptions.x64);
				if (builtFor64Bit)
				{
					vaSize = Shell.ProcessorSize.x64;
				}

				var context = new Xmb.XmbFileContext()
				{
					PointerSize = vaSize,
				};

				using (var ms = new System.IO.MemoryStream(buffer, false))
				using (var s = new IO.EndianReader(ms, blockStream.ByteOrder))
				{
					s.UserData = context;

					using (var xmbf = new Phoenix.Xmb.XmbFile())
					{
						xmbf.Read(s);
						xmbf.ToXml(path);
					}
				}
				#endregion
			}
			else
			{
				using (var fs = System.IO.File.Create(path))
				{
					fs.Write(buffer, 0, buffer.Length);
				}

				if (EraFile.IsScaleformFile(path))
				{
					#region DecompressUIFiles
					if (expander.ExpanderOptions.Test(EraFileExpanderOptions.DecompressUIFiles))
					{
						using (var ms = new System.IO.MemoryStream(buffer, false))
						using (var s = new IO.EndianReader(ms, Shell.EndianFormat.Little))
						{
							uint buffer_signature;
							if (EraFile.IsScaleformBuffer(s, out buffer_signature))
							{
								int decompressed_size = s.ReadInt32();
								int compressed_size = (int)(ms.Length - ms.Position);

								byte[] decompressed_data = EraFile.DecompressScaleform(buffer, decompressed_size);
								using (var fs = System.IO.File.Create(path + ".bin"))
								{
									fs.Write(decompressed_data, 0, decompressed_data.Length);
								}
							}
						}
					}
					#endregion
					#region TranslateGfxFiles
					if (expander.ExpanderOptions.Test(EraFileExpanderOptions.TranslateGfxFiles))
					{
						using (var ms = new System.IO.MemoryStream(buffer, false))
						using (var s = new IO.EndianReader(ms, Shell.EndianFormat.Little))
						{
							uint buffer_signature;
							if (EraFile.IsScaleformBuffer(s, out buffer_signature))
							{
								uint swf_signature = EraFile.GfxHeaderToSwf(buffer_signature);
								using (var fs = System.IO.File.Create(path + ".swf"))
								using (var out_s = new IO.EndianWriter(fs, Shell.EndianFormat.Little))
								{
									out_s.Write(swf_signature);
									out_s.Write(buffer, sizeof(uint), buffer.Length - sizeof(uint));
								}
							}
						}
					}
					#endregion
				}
			}
		}
		public void Unpack(IO.EndianStream blockStream, string basePath)
		{
			Contract.Requires(blockStream.IsReading);

			string path = System.IO.Path.Combine(basePath, FileName);

			var expander = blockStream.Owner as EraFileExpander;
			if (expander != null && !ShouldUnpack(expander, path))
			{
				return;
			}

			string folder = System.IO.Path.GetDirectoryName(path);
			if (!System.IO.Directory.Exists(folder))
			{
				System.IO.Directory.CreateDirectory(folder);
			}

			byte[] buffer = GetBuffer(blockStream);
			Unpack(blockStream, expander, path, buffer);
		}

		public bool Pack(IO.EndianStream blockStream, string basePath,
			Security.Cryptography.TigerHashBase hasher)
		{
			Contract.Requires(blockStream.IsWriting);

#if false // only compress if there's a reasonable savings
			// in case someone fucked up the xml listing
			if (EraFile.IsXmlBasedFile(FileName))
			{
				CompressionType = ECF.EcfCompressionType.DeflateRaw;

			}
			else if (EraFile.IsXmbFile(FileName))
			{
				CompressionType = ECF.EcfCompressionType.Stored;
			}
#endif

			string path = System.IO.Path.Combine(basePath, FileName);
			if (!System.IO.File.Exists(path))
			{
				return false;
			}

			using (var fs = System.IO.File.OpenRead(path))
			{
				BuildBuffer(blockStream, fs, hasher);
			}

			return true;
		}

		// Interface really only for the ERA's internal filenames table packaging
		internal bool Pack(IO.EndianStream blockStream, System.IO.MemoryStream source,
			Security.Cryptography.TigerHashBase hasher)
		{
			Contract.Requires(blockStream.IsWriting);

			BuildBuffer(blockStream, source, hasher);

			return true;
		}
	};
}