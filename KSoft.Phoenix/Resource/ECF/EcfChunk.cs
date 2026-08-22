using System;
using System.Diagnostics;
using System.IO;

namespace KSoft.Phoenix.Resource.ECF
{
	public enum EcfCompressionType : byte
	{
		Stored,
		/// <summary>File data is stored in a standard compressed buffer (no header/footer/etc data, just plain old compressed bytes)</summary>
		DeflateRaw,
		/// <summary>File data is stored in a <see cref="CompressedStream"/> buffer</summary>
		DeflateStream,
	};

	// eECFChunkResourceFlags
	enum EcfChunkResourceFlags : ushort
	{
		Contiguous,
		WriteCombined, // only valid with Contiguous
		IsDeflateStream,
		IsResourceTag,
	};

	// BECFChunkHeader
	[DebuggerDisplay($"{{{nameof(GetDebuggerDisplay)}(),nq}}")]
	public class EcfChunk
		: IO.IEndianStreamSerializable
	{
		public const int kMaxCount = 32768;
		public const uint kMaxSize = 1024U * 1024U * 1024U;

		public const int kSizeOf = 0x18;
		internal const string kXmlElementStreamName = "chunk";

		public const int kDefaultAlignmentBit = 2;
		const byte kCompressionTypeMask = 7;

		#region Struct fields
		public ulong EntryId;
		public Values.PtrHandle DataOffset = Values.PtrHandle.Null32; // offset within the parent block
		public int DataSize; // technically, this is uint32
		public uint Adler32;
		public byte Flags;
		public byte DataAlignmentBit = kDefaultAlignmentBit;
		private ushort mResourceFlags;
		#endregion

		public ulong DecompressedDataTiger64
		{
			get { return EntryId; }
			set { EntryId = value; }
		}

		public EcfCompressionType CompressionType
		{
			get { return (EcfCompressionType)(Flags & kCompressionTypeMask); }
			set { Flags |= (byte)( ((byte)(value)) & kCompressionTypeMask ); }
		}

		#region Resource flags
		public bool IsContiguous
		{
			get { return Bitwise.Flags.Test(mResourceFlags, 1U<<(ushort)EcfChunkResourceFlags.Contiguous); }
			set { Bitwise.Flags.Modify(value, ref mResourceFlags, (ushort)1U<<(ushort)EcfChunkResourceFlags.Contiguous); }
		}

		public bool IsWriteCombined
		{
			get { return Bitwise.Flags.Test(mResourceFlags, 1U<<(ushort)EcfChunkResourceFlags.WriteCombined); }
			set { Bitwise.Flags.Modify(value, ref mResourceFlags, (ushort)1U<<(ushort)EcfChunkResourceFlags.WriteCombined); }
		}

		public bool IsDeflateStream
		{
			get { return Bitwise.Flags.Test(mResourceFlags, 1U<<(ushort)EcfChunkResourceFlags.IsDeflateStream); }
			set { Bitwise.Flags.Modify(value, ref mResourceFlags, (ushort)1U<<(ushort)EcfChunkResourceFlags.IsDeflateStream); }
		}

		public bool IsResourceTag
		{
			get { return Bitwise.Flags.Test(mResourceFlags, 1U<<(ushort)EcfChunkResourceFlags.IsResourceTag); }
			set { Bitwise.Flags.Modify(value, ref mResourceFlags, (ushort)1U<<(ushort)EcfChunkResourceFlags.IsResourceTag); }
		}
		#endregion

		/// <summary>
		/// Flag is set but CompressionType is Stored. This will be the case with XMBs.
		/// </summary>
		public bool IsDeflateStreamButNoCompression
			=> IsDeflateStream && CompressionType == EcfCompressionType.Stored;

		public void SeekTo(IO.IKSoftBinaryStream blockStream)
		{
			blockStream.Seek((long)DataOffset);
		}
		public byte[] GetRawBuffer(IO.EndianStream blockStream)
		{
			SeekTo(blockStream);
			byte[] result = blockStream.Reader.ReadBytes(DataSize);

			return result;
		}

		#region Buffer Util
		public byte[] GetBuffer(IO.EndianStream blockStream)
		{
			byte[] result;

			var assumed_compression_type = CompressionType;

			// #NOTE CompressionType can be Stored but IsDeflateStream can be true (seen it in XMB).
			// So just handle the flag as we do EcfCompressionType.DeflateStream
			if (IsDeflateStream)
			{
				assumed_compression_type = EcfCompressionType.DeflateStream;
			}

			switch (assumed_compression_type)
			{
				case EcfCompressionType.Stored:
					result = GetRawBuffer(blockStream);
					break;

				case EcfCompressionType.DeflateRaw:
					result = GetRawBuffer(blockStream);
					result = DecompressFromBuffer(blockStream, result);
					break;

				case EcfCompressionType.DeflateStream:
					result = DecompressFromStream(blockStream);
					break;

				default:
					throw new KSoft.Debug.UnreachableException(assumed_compression_type.ToString());
			}

			return result;
		}

		protected virtual byte[] DecompressFromBuffer(IO.EndianStream blockStream, byte[] buffer)
		{
			throw new InvalidOperationException(string.Format(
				"Can't get the decompressed bytes for {0} (from {1}). Need to know the uncompressed data size",
				EntryId, blockStream.StreamName));
		}

		byte[] DecompressFromStream(IO.EndianStream blockStream)
		{
			SeekTo(blockStream);
			return CompressedStream.DecompressFromStream(blockStream);
		}

		public virtual void BuildBuffer(IO.EndianStream blockStream, Stream sourceFile
			, Security.Cryptography.TigerHashBase hasher = null)
		{
			blockStream.AlignToBoundry(DataAlignmentBit);

			sourceFile.Seek(0, SeekOrigin.Begin);
			if (hasher != null)
			{
				UpdateDecompressedDataTigerHash(sourceFile, hasher);
			}

			if (blockStream.BaseStream.Position != blockStream.BaseStream.Length)
			{
				throw new InvalidOperationException(string.Format(
					"Block stream must be positioned at the end before writing chunk data; position is {0}, length is {1}.",
					blockStream.BaseStream.Position,
					blockStream.BaseStream.Length));
			}

			DataOffset = blockStream.PositionPtr;

			// #TODO determine if compressing the sourceFile data has any savings (eg, 7% smaller)

			var assumed_compression_type = CompressionType;

			// #NOTE CompressionType can be Stored but IsDeflateStream can be true (seen it in XMB).
			// So just handle the flag as we do EcfCompressionType.DeflateStream
			if (IsDeflateStream)
			{
				assumed_compression_type = EcfCompressionType.DeflateStream;
			}

			switch (assumed_compression_type)
			{
				case EcfCompressionType.Stored:
				{
					// Update this ECF's size
					DataSize = (int)sourceFile.Length;
					// Also update this ECF's checksum
					Adler32 = Security.Cryptography.Adler32.Compute(sourceFile, DataSize, restorePosition: true);
					// Copy the source file's bytes to the block stream
					sourceFile.CopyTo(blockStream.BaseStream);
					break;
				}

				case EcfCompressionType.DeflateRaw:
					CompressSourceToStream(blockStream.Writer, sourceFile);
					break;

				case EcfCompressionType.DeflateStream:
					CompressSourceToCompressionStream(blockStream.Writer, sourceFile);
					break;

				default:
					throw new KSoft.Debug.UnreachableException(assumed_compression_type.ToString());
			}

			long expected_end_position = (long)DataOffset + DataSize;
			if (blockStream.BaseStream.Position != expected_end_position)
			{
				throw new InvalidOperationException(string.Format(
					"Chunk write ended at position {0}, expected {1}.",
					blockStream.BaseStream.Position,
					expected_end_position));
			}
		}

		protected virtual void CompressSourceToStream(IO.EndianWriter blockStream, Stream sourceFile)
		{
			// Read the source bytes
			byte[] buffer = new byte[sourceFile.Length];
			for (int x = 0; x < buffer.Length;)
			{
				int n = sourceFile.Read(buffer, x, buffer.Length - x);
				x += n;
			}

			// Compress the source bytes into a new buffer
			byte[] result = ResourceUtils.Compress(buffer, out Adler32);  // Also update this ECF's checksum
			DataSize = result.Length; // Update this ECF's size

			// Write the compressed bytes to the block stream
			blockStream.Write(result);
		}

		protected void CompressSourceToCompressionStream(IO.EndianWriter blockStream, Stream sourceFile)
		{
			// Build a CompressedStream from the source file and write it to the block stream
			CompressedStream.CompressFromStream(blockStream, sourceFile,
				out Adler32, out DataSize);  // Update this ECF's checksum and size
		}
		#endregion

		#region Hash Utils
		public uint ComputeAdler32(IO.EndianStream blockStream)
		{
			ArgumentNullException.ThrowIfNull(blockStream);

			SeekTo(blockStream);
			uint adler = Security.Cryptography.Adler32.Compute(blockStream.BaseStream, DataSize);
			return adler;
		}

		public void ComputeHash(IO.EndianStream blockStream, Security.Cryptography.TigerHashBase hasher)
		{
			ArgumentNullException.ThrowIfNull(blockStream);
			ArgumentNullException.ThrowIfNull(hasher);

			hasher.Initialize();
			hasher.ComputeHash(blockStream.BaseStream,
				(long)DataOffset, DataSize);
		}

		protected void UpdateDecompressedDataTigerHash(System.IO.Stream source, Security.Cryptography.TigerHashBase hasher)
		{
			hasher.ComputeHash(source, 0, (int)source.Length,
				restorePosition: true);

			hasher.TryGetAsTiger64(out ulong tiger64);
			DecompressedDataTiger64 = tiger64;
		}
		#endregion

		#region IEndianStreamSerializable Members
		public virtual void Serialize(IO.EndianStream s)
		{
			s.Stream(ref EntryId);
			s.StreamVirtualAddress(ref DataOffset);
			s.Stream(ref DataSize);
			s.Stream(ref Adler32);
			s.Stream(ref Flags);
			s.Stream(ref DataAlignmentBit);
			s.Stream(ref mResourceFlags);
		}
		#endregion

		#region Xml Streaming
		public void Read(IO.XmlElementStream s, bool includeFileData)
		{
			ReadFields(s, includeFileData);
		}
		public void Write(IO.XmlElementStream s, bool includeFileData)
		{
			using (s.EnterCursorBookmark(kXmlElementStreamName))
			{
				WriteFields(s, includeFileData);
			}
		}

		protected virtual void ReadFields(IO.XmlElementStream s, bool includeFileData)
		{
			s.ReadAttributeOpt("id", ref EntryId, NumeralBase.Hex);
			//s.ReadAttributeOpt("flags", ref Flags, NumeralBase.Hex);
			s.ReadAttributeOpt("align", ref DataAlignmentBit, NumeralBase.Hex);
			if (includeFileData)
			{
				s.ReadAttributeOpt("offset", ref DataOffset.u32, NumeralBase.Hex);
				s.ReadAttributeOpt("size", ref DataSize, NumeralBase.Hex);
			}

			ReadFlags(s);
			ReadResourceFlags(s);
		}
		protected virtual void WriteFields(IO.XmlElementStream s, bool includeFileData)
		{
			s.WriteAttribute("id", EntryId.ToString("X16"));
			//if (Flags != 0)
			//	s.WriteAttribute("flags", Flags.ToString("X1"));
			if (DataAlignmentBit != kDefaultAlignmentBit)
			{
				s.WriteAttribute("align", DataAlignmentBit.ToString("X1"));
			}
			if (includeFileData)
			{
				s.WriteAttribute("offset", DataOffset.u32.ToString("X8"));
				s.WriteAttribute("size", DataSize.ToString("X8"));
			}
		}

		protected void ReadFlags(IO.XmlElementStream s)
		{
			var compType = EcfCompressionType.Stored;
			if (s.ReadAttributeEnumOpt("Compression", ref compType))
			{
				CompressionType = compType;
			}
		}
		protected void WriteFlags(IO.XmlElementStream s)
		{
			if (Flags == 0)
			{
				return;
			}

			s.WriteAttributeEnum("Compression", CompressionType);
		}

		protected void ReadResourceFlags(IO.XmlElementStream s)
		{
			bool flag = false;
			if (s.ReadAttributeOpt("IsContiguous", ref flag))
			{
				IsContiguous = flag;
			}
			if (s.ReadAttributeOpt("IsWriteCombined", ref flag))
			{
				IsWriteCombined = flag;
			}
			if (s.ReadAttributeOpt("IsDeflateStream", ref flag))
			{
				IsDeflateStream = flag;
			}
			if (s.ReadAttributeOpt("IsResourceTag", ref flag))
			{
				IsResourceTag = flag;
			}
		}
		protected void WriteResourceFlags(IO.XmlElementStream s)
		{
			if (mResourceFlags == 0)
			{
				return;
			}

			if (IsContiguous)
			{
				s.WriteAttribute("IsContiguous", true);
			}
			if (IsWriteCombined)
			{
				s.WriteAttribute("IsWriteCombined", true);
			}
			if (IsDeflateStream)
			{
				s.WriteAttribute("IsDeflateStream", true);
			}
			if (IsResourceTag)
			{
				s.WriteAttribute("IsResourceTag", true);
			}
		}
		#endregion

		private string GetDebuggerDisplay()
			=> $"{EntryId:X16} {DataSize:X8} {DataOffset}";
	}
}
