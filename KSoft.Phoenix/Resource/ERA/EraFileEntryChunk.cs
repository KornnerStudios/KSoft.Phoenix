using System;
using Contracts = System.Diagnostics.Contracts;
using Contract = System.Diagnostics.Contracts.Contract;

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

		public string FileName;
		public System.DateTime FileDateTime
		{
			get { return DateTime.FromFileTimeUtc((long)mFileTimeBits); }
			set { mFileTimeBits = (ulong)value.ToFileTimeUtc(); }
		}

		#region IEndianStreamSerializable Members
		public override void Serialize(IO.EndianStream s)
		{
			var eraUtil = s.Owner as EraFileUtil;
			long position = s.BaseStream.Position;

			base.Serialize(s);

			s.Stream(ref mFileTimeBits);
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
		string FileDateTimeString { get {
			return FileDateTime.ToString("u"); // UniversalSorta­bleDateTimePat­tern
		} }

		protected override void WriteFields(IO.XmlElementStream s, bool includeFileData)
		{
			if (includeFileData && mFileTimeBits != 0)
				s.WriteAttribute("fileTime", mFileTimeBits.ToString("X16"));

			// only because it's interesting to have, never read back in
			s.WriteAttribute("fileDateTime", FileDateTimeString);

			base.WriteFields(s, includeFileData);

			// When we extract, we decode xmbs
			string fn = FileName;
			if (ResourceUtils.IsXmbFile(fn))
			{
				bool remove_xmb_ext = true;

				var expander = s.Owner as EraFileExpander;
				if (expander != null && expander.ExpanderOptions.Test(EraFileExpanderOptions.DontTranslateXmbFiles))
					remove_xmb_ext = false;

				if (remove_xmb_ext)
					ResourceUtils.RemoveXmbExtension(ref fn);
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

			base.WriteFlags(s);
			base.WriteResourceFlags(s);
		}

		protected override void ReadFields(IO.XmlElementStream s, bool includeFileData)
		{
			base.ReadFields(s, includeFileData);

			s.ReadAttributeOpt("fileTime", ref mFileTimeBits, NumeralBase.Hex);

			s.ReadAttribute("name", ref FileName);

			s.ReadAttributeOpt("fullSize", ref DataUncompressedSize, NumeralBase.Hex);
			s.ReadAttributeOpt("nameOffset", ref FileNameOffset, NumeralBase.Hex);

			string hashString = null;
			if (s.ReadAttributeOpt("compressedDataHash", ref hashString))
				CompressedDataTiger128 = Text.Util.ByteStringToArray(hashString);
		}
		#endregion

		#region Buffer Util
		protected override byte[] DecompressFromBuffer(IO.EndianStream blockStream, byte[] buffer)
		{
			uint result_adler;
			return ResourceUtils.Decompress(buffer, DataUncompressedSize, out result_adler);
		}

		public override void BuildBuffer(IO.EndianStream blockStream, System.IO.Stream sourceFile,
			Security.Cryptography.TigerHashBase hasher)
		{
			base.BuildBuffer(blockStream, sourceFile, hasher);

			ComputeHash(blockStream, hasher);
			Array.Copy(hasher.Hash, 0, CompressedDataTiger128, 0, CompressedDataTiger128.Length);
		}

		protected override void CompressSourceToStream(IO.EndianWriter blockStream, System.IO.Stream sourceFile)
		{
			this.DataUncompressedSize = (int)sourceFile.Length;

			base.CompressSourceToStream(blockStream, sourceFile);
		}
		#endregion

		public override string ToString()
		{
			return string.Format("{0}",
				FileName);
		}
	};
}