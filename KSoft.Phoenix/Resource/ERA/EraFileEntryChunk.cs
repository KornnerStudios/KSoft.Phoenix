using System;

namespace KSoft.Phoenix.Resource
{
	public sealed class EraFileEntryChunk
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
		private ulong mFileTimeBits; // FILETIME
		public int DataUncompressedSize;
		// First 128 bits of the compressed data's Tiger hash
		public byte[] CompressedDataTiger128 = new byte[kCompresssedDataTigerHashSize];
		// actually only 24 bits, big endian
		public uint FileNameOffset;
		#endregion

		public string? FileName;
		public System.DateTime FileDateTime
		{
			get { return DateTime.FromFileTimeUtc((long)mFileTimeBits); }
			set { mFileTimeBits = (ulong)value.ToFileTimeUtc(); }
		}

		#region IEndianStreamSerializable Members
		public override void Serialize(IO.EndianStream s)
		{
			KSoft.Debug.TypeCheck.TryCastReference(s.Owner, out EraFileUtil? eraUtil);
			long position = s.BaseStream.Position;

			base.Serialize(s);

			s.Stream(ref mFileTimeBits);
			s.Stream(ref DataUncompressedSize);

			if (s.IsWriting)
			{
				CompressedDataTiger128.AsSpan(sizeof(ulong) * 0, sizeof(ulong)).Reverse();
				CompressedDataTiger128.AsSpan(sizeof(ulong) * 1, sizeof(ulong)).Reverse();
			}
			s.Stream(CompressedDataTiger128.AsSpan());
			{
				CompressedDataTiger128.AsSpan(sizeof(ulong) * 0, sizeof(ulong)).Reverse();
				CompressedDataTiger128.AsSpan(sizeof(ulong) * 1, sizeof(ulong)).Reverse();
			}


			s.StreamUInt24(ref FileNameOffset);
			s.Pad8();

			if (eraUtil != null && eraUtil.DebugOutput != null)
			{
				eraUtil.DebugOutput.Write("FileEntry: {0} @{1} offset={2} end={3} size={4} dsize={5} adler={6} ",
					base.EntryId.ToString("X16", KSoft.Util.InvariantCultureInfo),
					position.ToString("X8", KSoft.Util.InvariantCultureInfo),
					base.DataOffset.u32.ToString("X8", KSoft.Util.InvariantCultureInfo),
					(base.DataOffset.u32 + base.DataSize).ToString("X8", KSoft.Util.InvariantCultureInfo),
					base.DataSize.ToString("X8", KSoft.Util.InvariantCultureInfo),
					DataUncompressedSize.ToString("X8", KSoft.Util.InvariantCultureInfo),
					base.Adler32.ToString("X8", KSoft.Util.InvariantCultureInfo));

				if (!string.IsNullOrEmpty(FileName))
				{
					eraUtil.DebugOutput.Write(FileName);
				}

				eraUtil.DebugOutput.WriteLine();
			}
		}
		#endregion

		#region Xml Streaming
		string FileDateTimeString
			=> FileDateTime.ToString("u"); // UniversalSorta­bleDateTimePat­tern

		protected override void WriteFields(IO.XmlElementStream s, bool includeFileData)
		{
			if (includeFileData && mFileTimeBits != 0)
			{
				s.WriteAttribute("fileTime", mFileTimeBits.ToString("X16", KSoft.Util.InvariantCultureInfo));
			}

			// only because it's interesting to have, never read back in
			s.WriteAttribute("fileDateTime", FileDateTimeString);

			base.WriteFields(s, includeFileData);

			// When we extract, we decode xmbs
			string fn = FileName!;
			if (ResourceUtils.IsXmbFile(fn))
			{
				bool remove_xmb_ext = true;

				KSoft.Debug.TypeCheck.TryCastReference(s.Owner, out EraFileExpander? expander);

				if (expander != null && expander.ExpanderOptions.Test(EraFileExpanderOptions.DontTranslateXmbFiles))
				{
					remove_xmb_ext = false;
				}

				if (remove_xmb_ext)
				{
					ResourceUtils.RemoveXmbExtension(ref fn);
				}
			}
			s.WriteAttribute("name", fn);

			if (includeFileData)
			{
				if (DataUncompressedSize != DataSize)
				{
					s.WriteAttribute("fullSize", DataUncompressedSize.ToString("X8", KSoft.Util.InvariantCultureInfo));
				}

				s.WriteAttribute("compressedDataHash",
					Text.Util.ByteArrayToString(CompressedDataTiger128));

				s.WriteAttribute("nameOffset", FileNameOffset.ToString("X6", KSoft.Util.InvariantCultureInfo));
			}

			base.WriteFlags(s);
			base.WriteResourceFlags(s);
		}

		protected override void ReadFields(IO.XmlElementStream s, bool includeFileData)
		{
			base.ReadFields(s, includeFileData);

			s.ReadAttributeOpt("fileTime", ref mFileTimeBits, NumeralBase.Hex);

			s.ReadAttribute("name", ref FileName!);

			s.ReadAttributeOpt("fullSize", ref DataUncompressedSize, NumeralBase.Hex);
			s.ReadAttributeOpt("nameOffset", ref FileNameOffset, NumeralBase.Hex);

			string? hashString = null;
			if (s.ReadAttributeOpt("compressedDataHash", ref hashString))
			{
				CompressedDataTiger128 = Text.Util.ByteStringToArray(hashString!);
			}
		}
		#endregion

		#region Buffer Util
		protected override byte[] DecompressFromBuffer(IO.EndianStream blockStream, byte[] buffer)
		{
			return ResourceUtils.Decompress(buffer, DataUncompressedSize, out uint /*result_adler*/_);
		}

		public override void BuildBuffer(IO.EndianStream blockStream, System.IO.Stream sourceFile,
			Security.Cryptography.TigerHashBase? hasher)
		{
			ArgumentNullException.ThrowIfNull(hasher);

			base.BuildBuffer(blockStream, sourceFile, hasher);

			ComputeHash(blockStream, hasher);
			Array.Copy(hasher.Hash!, 0, CompressedDataTiger128, 0, CompressedDataTiger128.Length);
		}

		protected override void CompressSourceToStream(IO.EndianWriter blockStream, System.IO.Stream sourceFile)
		{
			this.DataUncompressedSize = (int)sourceFile.Length;

			base.CompressSourceToStream(blockStream, sourceFile);
		}
		#endregion

		public override string ToString()
		{
			return FileName ?? string.Empty;
		}
	};
}
