﻿using System;
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

		void UpdateDecompressedDataTigerHash(System.IO.Stream source, Security.Cryptography.TigerHashBase hasher)
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
			return ResourceUtils.Decompress(buffer, DataUncompressedSize, out result_adler);
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
			byte[] result = ResourceUtils.Compress(buffer, out base.Adler32);  // Also update this ECF's checksum
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
			UpdateDecompressedDataTigerHash(sourceFile, hasher);

			Contract.Assert(blockStream.BaseStream.Position == blockStream.BaseStream.Length);

			base.DataOffset = blockStream.PositionPtr;
			this.DataUncompressedSize = (int)sourceFile.Length;

			// #TODO determine if compressing the sourceFile data has any savings (eg, 7% smaller)

			switch (CompressionType)
			{
				case ECF.EcfCompressionType.Stored:
				{
					// Update this ECF's size
					base.DataSize = (int)sourceFile.Length;
					// Also update this ECF's checksum
					base.Adler32 = Security.Cryptography.Adler32.Compute(sourceFile, base.DataSize, restorePosition:true);
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

			Contract.Assert(blockStream.BaseStream.Position == ((long)DataOffset + DataSize));

			ComputeHash(blockStream, hasher);
			System.Array.Copy(hasher.Hash, 0, CompressedDataTiger128, 0, CompressedDataTiger128.Length);
		}
		#endregion

		public override string ToString()
		{
			return string.Format("{0}",
				FileName);
		}
	};
}