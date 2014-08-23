
namespace KSoft.Phoenix.Resource.ECF
{
	/*public*/ class EcfChunk
		: IO.IEndianStreamSerializable
	{
		public const int kSizeOf = 0x18;
		internal const string kXmlElementStreamName = "chunk";

		const int kDefaultAlignmentBit = 2;

		public ulong EntryId;
		public Values.PtrHandle DataOffset = Values.PtrHandle.Null32; // offset within the parent block
		public int DataSize;
		public uint Checksum;
		public byte Flags;
		public byte DataAlignmentBit = kDefaultAlignmentBit;
		// I've seen XMB chunks have this set (eg, 4)
		ushort unknown16;

		public void SeekTo(IO.IKSoftBinaryStream blockStream)
		{
			blockStream.Seek((long)DataOffset);
		}
		public virtual byte[] GetBuffer(IO.EndianStream blockStream)
		{
			SeekTo(blockStream);
			byte[] result = blockStream.Reader.ReadBytes(DataSize);

			return result;
		}

		#region IEndianStreamSerializable Members
		public virtual void Serialize(IO.EndianStream s)
		{
			s.Stream(ref EntryId);
			s.StreamVirtualAddress(ref DataOffset);
			s.Stream(ref DataSize);
			s.Stream(ref Checksum);
			s.Stream(ref Flags);
			s.Stream(ref DataAlignmentBit);
			s.Stream(ref unknown16);
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
		public void Read(IO.XmlElementStream s, bool include_file_data)
		{
			ReadFields(s, include_file_data);
		}
		protected virtual void WriteFields(IO.XmlElementStream s, bool includeFileData)
		{
			s.WriteAttribute("id", EntryId.ToString("X16"));
			if(Flags != 0)
				s.WriteAttribute("flags", Flags.ToString("X1"));
			if(DataAlignmentBit != kDefaultAlignmentBit)
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