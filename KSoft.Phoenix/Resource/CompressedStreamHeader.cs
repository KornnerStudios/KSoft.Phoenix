
namespace KSoft.Phoenix.Resource
{
	partial class CompressedStream
	{
		struct Header
			: IO.IEndianStreamSerializable
		{
			public const int kSizeOf = 0x24;

			internal uint HeaderAdler32;
			public uint StreamMode;
			public ulong UncompressedSize, CompressedSize;
			public uint UncompressedAdler32, CompressedAdler32;

			public bool UseBufferedStreaming { get {
				return StreamMode == (uint)Mode.Buffered;
			} }

			public void UpdateHeaderCrc()
			{
				HeaderAdler32 = InMemoryRep.Checksum(UncompressedSize, UncompressedAdler32,
					CompressedSize, CompressedAdler32,
					StreamMode);
			}

			#region IEndianStreamSerializable Members
			public void Serialize(IO.EndianStream s)
			{
				if (s.IsWriting)
					UpdateHeaderCrc();

				s.StreamSignature(kSignature);
				s.Stream(ref HeaderAdler32);
				s.Stream(ref StreamMode);
				s.Stream(ref UncompressedSize);
				s.Stream(ref CompressedSize);
				s.Stream(ref UncompressedAdler32);
				s.Stream(ref CompressedAdler32);
			}
			#endregion
		};
	};
}