
namespace KSoft.Phoenix.Resource
{
	partial class CompressedStream
	{
		struct Header
			: IO.IEndianStreamSerializable
		{
			public const int kSizeOf = 0x24;

			internal uint HeaderCrc;
			public uint StreamMode;
			public ulong UncompressedSize, CompressedSize;
			public uint UncompressedCrc, CompressedCrc;

			public bool UseBufferedStreaming { get {
				return StreamMode == (uint)Mode.Buffered;
			} }

			public void UpdateHeaderCrc()
			{
				HeaderCrc = InMemoryRep.Checksum(UncompressedSize, UncompressedCrc,
					CompressedSize, CompressedCrc,
					StreamMode);
			}

			#region IEndianStreamSerializable Members
			public void Serialize(IO.EndianStream s)
			{
				if (s.IsWriting)
					UpdateHeaderCrc();

				s.StreamSignature(kSignature);
				s.Stream(ref HeaderCrc);
				s.Stream(ref StreamMode);
				s.Stream(ref UncompressedSize);
				s.Stream(ref CompressedSize);
				s.Stream(ref UncompressedCrc);
				s.Stream(ref CompressedCrc);
			}
			#endregion
		};
	};
}