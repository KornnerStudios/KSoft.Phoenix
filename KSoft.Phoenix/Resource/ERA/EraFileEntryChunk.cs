using Contracts = System.Diagnostics.Contracts;
using Contract = System.Diagnostics.Contracts.Contract;

namespace KSoft.Phoenix.Resource
{
	/*public*/ sealed class EraFileEntryChunk
		: ECF.EcfChunk
	{
		#region SizeOf
		public new const int kSizeOf = 0x38;
		public const uint kExtraDataSize = kSizeOf - ECF.EcfChunk.kSizeOf;

		public static int CalculateFileChunksSize(int fileCount)
		{
			return kSizeOf * fileCount;
		}
		#endregion

		#region Flags
		const byte kIsCompressedFlag = 1 << 0;
		/// <summary>File data is stored in a standard compressed buffer (no header/footer/etc data, just plain old compressed bytes)</summary>
		public bool IsCompressed
		{
			get { return Bitwise.Flags.Test(base.Flags, kIsCompressedFlag); }
			set { Bitwise.Flags.Modify(value, ref base.Flags, kIsCompressedFlag); }
		}
		const byte kIsCompressedStreamFlag = 1 << 1;
		/// <summary>File data is stored in a <see cref="CompressedStream"/> buffer</summary>
		public bool IsCompressedStream
		{
			get { return Bitwise.Flags.Test(base.Flags, kIsCompressedStreamFlag); }
			set { Bitwise.Flags.Modify(value, ref base.Flags, kIsCompressedStreamFlag); }
		}
		#endregion

		public ulong FileTime; // bit encoded file time
		public int DataUncompressedSize;
		// 24 = ID calculated from Uncompressed data?
		ulong unknown24, unknown2C;
		public uint FilenameOffset;

		public string Filename;

		#region IEndianStreamSerializable Members
		public override void Serialize(IO.EndianStream s)
		{
			base.Serialize(s);

			var fname_offset = s.IsReading
				? 0
				: FilenameOffset << 8;

			s.Stream(ref FileTime);
			s.Stream(ref DataUncompressedSize);
			s.Stream(ref unknown24);
			s.Stream(ref unknown2C);
			s.Stream(ref fname_offset);

			if(s.IsReading)
				FilenameOffset = fname_offset >> 8;
		}
		#endregion

		#region Xml Streaming
		protected override void WriteFields(IO.XmlElementStream s, bool includeFileData)
		{
			base.WriteFields(s, includeFileData);

			if(FileTime != 0) s.WriteAttribute("fileTime", FileTime.ToString("X16"));
			if(unknown2C != 0) s.WriteAttribute("u2C", unknown2C.ToString("X16"));

			// When we extract, we decode xmbs
			string fn = Filename;
			if (EraFile.IsXmbFile(fn))
			{
				bool remove_xmb_ext = true;

				var expander = s.Owner as EraFileExpander;
				if (expander != null && expander.Options.HasFlag(EraFileExpanderOptions.DontTranslateXmbFiles))
					remove_xmb_ext = false;

				if(remove_xmb_ext)
					EraFile.RemoveXmbExtension(ref fn);
			}
			s.WriteAttribute("name", fn);

			if (includeFileData && DataUncompressedSize != DataSize)
				s.WriteAttribute("fullSize", DataUncompressedSize.ToString("X8"));
			if (unknown24 != EntryId)
				s.WriteAttribute("u24", unknown24.ToString("X16"));

			if (includeFileData)
				s.WriteAttribute("nameOffset", FilenameOffset.ToString("X6"));
		}

		protected override void ReadFields(IO.XmlElementStream s, bool includeFileData)
		{
			base.ReadFields(s, includeFileData);

			s.ReadAttributeOpt("fileTime", ref FileTime, NumeralBase.Hex);
			s.ReadAttributeOpt("u2C", ref unknown2C, NumeralBase.Hex);

			s.ReadAttribute("name", ref Filename);

			s.ReadAttributeOpt("fullSize", ref DataUncompressedSize, NumeralBase.Hex);
			s.ReadAttributeOpt("u24", ref unknown24, NumeralBase.Hex);
			s.ReadAttributeOpt("nameOffset", ref FilenameOffset, NumeralBase.Hex);
		}
		#endregion

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

			if (IsCompressed)
			{
				result = base.GetBuffer(blockStream);
				result = DecompressFromBuffer(blockStream, result);
			}
			else if (IsCompressedStream)
				result = DecompressFromStream(blockStream);
			else
				result = base.GetBuffer(blockStream);

			return result;
		}

		void CompressSourceToStream(IO.EndianWriter blockStream, System.IO.Stream sourceFile)
		{
			// Read the source bytes
			byte[] buffer = new byte[sourceFile.Length];
			sourceFile.Read(buffer, 0, buffer.Length);

			// Compress the source bytes into a new buffer
			byte[] result = EraFile.Compress(buffer, out base.Checksum);  // Also update this ECF's checksum
			base.DataSize = result.Length; // Update this ECF's size

			// Write the compressed bytes to the block stream
			blockStream.Write(result);
		}
		void CompressSourceToCompressionStream(IO.EndianWriter blockStream, System.IO.Stream sourceFile)
		{
			// Build a CompressedStream from the source file and write it to the block stream
			CompressedStream.CompressFromStream(blockStream, sourceFile,
				out base.Checksum, out base.DataSize);  // Update this ECF's checksum and size
		}
		public void BuildBuffer(IO.EndianStream blockStream, System.IO.Stream sourceFile)
		{
			blockStream.AlignToBoundry(base.DataAlignmentBit);

			base.DataOffset = blockStream.PositionPtr;
			this.DataUncompressedSize = (int)sourceFile.Length;

			if (IsCompressed)
				CompressSourceToStream(blockStream.Writer, sourceFile);
			else if (IsCompressedStream)
				CompressSourceToCompressionStream(blockStream.Writer, sourceFile);
			else
			{
				base.DataSize = (int)sourceFile.Length;  // Update this ECF's size
				// Copy the source file's bytes to the block stream
				sourceFile.CopyTo(blockStream.BaseStream);
			}
		}
		#endregion

		bool ShouldUnpack(EraFileExpander expander, string path)
		{
			if (expander.Options.HasFlag(EraFileExpanderOptions.DontOverwriteExistingFiles))
			{
				// it's an XMB file and the user didn't say NOT to translate them
				if (EraFile.IsXmbFile(path) && !expander.Options.HasFlag(EraFileExpanderOptions.DontTranslateXmbFiles))
					EraFile.RemoveXmbExtension(ref path);

				if (System.IO.File.Exists(path))
					return false;
			}
			return true;
		}
		void Unpack(IO.EndianStream blockStream, EraFileExpander expander, string path, byte[] chunk)
		{
			const System.IO.FileAccess k_mode = System.IO.FileAccess.Read;

			byte[] buffer = chunk;

			bool translate_xmb_files = true;
			if (expander != null && expander.Options.HasFlag(EraFileExpanderOptions.DontTranslateXmbFiles))
				translate_xmb_files = false;

			if (EraFile.IsXmbFile(path) && translate_xmb_files)
			{
				using (var xmb = new ECF.EcfFileXmb())
				using (var ms = new System.IO.MemoryStream(chunk))
				using (var es = new IO.EndianStream(ms, blockStream.ByteOrder, permissions: k_mode))
				{
					es.StreamMode = k_mode;
					xmb.Serialize(es);

					buffer = xmb.FileData;
				}

				EraFile.RemoveXmbExtension(ref path);

				using (var ms = new System.IO.MemoryStream(buffer, false))
				using (var s = new IO.EndianReader(ms, blockStream.ByteOrder))
				{
					s.VirtualAddressTranslationInitialize(Shell.ProcessorSize.x32);

					using (var xmbf = new Phoenix.Xmb.XmbFile())
					{
						xmbf.Read(s);
						xmbf.ToXml(path);
					}
				}
			}
			else
			{
				using (var fs = System.IO.File.Create(path))
					fs.Write(buffer, 0, buffer.Length);

				if (EraFile.IsScaleformFile(path))
				{
					#region DecompressUIFiles
					if (expander.Options.HasFlag(EraFileExpanderOptions.DecompressUIFiles))
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
									fs.Write(decompressed_data, 0, decompressed_data.Length);
							}
						}
					}
					#endregion
					#region TranslateGfxFiles
					if (expander.Options.HasFlag(EraFileExpanderOptions.TranslateGfxFiles))
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

			string path = System.IO.Path.Combine(basePath, Filename);

			var expander = blockStream.Owner as EraFileExpander;
			if (expander != null && !ShouldUnpack(expander, path))
					return;

			string folder = System.IO.Path.GetDirectoryName(path);
			if (!System.IO.Directory.Exists(folder))
				System.IO.Directory.CreateDirectory(folder);

			byte[] buffer = GetBuffer(blockStream);
			Unpack(blockStream, expander, path, buffer);
		}

		public bool Pack(IO.EndianStream blockStream, string basePath)
		{
			Contract.Requires(blockStream.IsWriting);

			// in case someone fucked up the xml listing
			if (EraFile.IsXmlBasedFile(Filename))
			{
				IsCompressed = true;
				IsCompressedStream = false;
			
			}
			else if (EraFile.IsXmbFile(Filename))
			{
				IsCompressed = false;
				IsCompressedStream = false;
			}

			string path = System.IO.Path.Combine(basePath, Filename);
			if (!System.IO.File.Exists(path)) return false;

			using (var fs = System.IO.File.OpenRead(path))
				BuildBuffer(blockStream, fs);

			return true;
		}

		internal bool Pack(IO.EndianStream blockStream, System.IO.MemoryStream source)
		{
			Contract.Requires(blockStream.IsWriting);

			BuildBuffer(blockStream, source);

			return true;
		}
	};
}