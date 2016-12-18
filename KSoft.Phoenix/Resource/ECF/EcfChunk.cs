
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

	enum EcfChunkResourceFlags : ushort
	{
		Contiguous,
		WriteCombined, // only valid with Contiguous
		IsDeflateStream,
		IsResourceTag,
	};

	/*public*/ class EcfChunk
		: IO.IEndianStreamSerializable
	{
		public const int kMaxCount = 32768;
		public const uint kMaxSize = 1024U * 1024U * 1024U;

		public const int kSizeOf = 0x18;
		internal const string kXmlElementStreamName = "chunk";

		const int kDefaultAlignmentBit = 2;
		const byte kCompressionTypeMask = 7;

		#region Struct fields
		public ulong EntryId;
		public Values.PtrHandle DataOffset = Values.PtrHandle.Null32; // offset within the parent block
		public int DataSize;
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
		public virtual byte[] GetBuffer(IO.EndianStream blockStream)
		{
			return GetRawBuffer(blockStream);
		}

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
		protected virtual void ReadFields(IO.XmlElementStream s, bool includeFileData)
		{
			s.ReadAttributeOpt("id", ref EntryId, NumeralBase.Hex);
			s.ReadAttributeOpt("flags", ref Flags, NumeralBase.Hex);
			s.ReadAttributeOpt("align", ref DataAlignmentBit, NumeralBase.Hex);
			if (includeFileData)
			{
				s.ReadAttributeOpt("offset", ref DataOffset.u32, NumeralBase.Hex);
				s.ReadAttributeOpt("size", ref DataSize, NumeralBase.Hex);
			}
		}
		public void Read(IO.XmlElementStream s, bool includeFileData)
		{
			ReadFields(s, includeFileData);
		}
		protected virtual void WriteFields(IO.XmlElementStream s, bool includeFileData)
		{
			s.WriteAttribute("id", EntryId.ToString("X16"));
			if (Flags != 0)
				s.WriteAttribute("flags", Flags.ToString("X1"));
			if (DataAlignmentBit != kDefaultAlignmentBit)
				s.WriteAttribute("align", DataAlignmentBit.ToString("X1"));
			if (includeFileData)
			{
				s.WriteAttribute("offset", DataOffset.u32.ToString("X8"));
				s.WriteAttribute("size", DataSize.ToString("X8"));
			}
		}
		public void Write(IO.XmlElementStream s, bool includeFileData)
		{
			using (s.EnterCursorBookmark(kXmlElementStreamName))
				WriteFields(s, includeFileData);
		}
		#endregion
	};
}