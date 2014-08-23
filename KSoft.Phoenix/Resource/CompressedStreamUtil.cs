using System;
using Contracts = System.Diagnostics.Contracts;
using Contract = System.Diagnostics.Contracts.Contract;

using FA = System.IO.FileAccess;

namespace KSoft.Phoenix.Resource
{
	using Adler32 = Security.Cryptography.Adler32;

	partial class CompressedStream
	{
		static class InMemoryRep
		{
			const int kOffsetSourceSize = 0;
			const int kOffsetSourceAdler = kOffsetSourceSize + sizeof(ulong);
			const int kOffsetCompressedSize = kOffsetSourceAdler + sizeof(uint);
			const int kOffsetCompressedAdler = kOffsetCompressedSize + sizeof(ulong);
			const int kOffsetMode = kOffsetCompressedAdler + sizeof(uint);

			const int kSizeOf = kOffsetMode + sizeof(uint); // 0x14

			public static uint Checksum(ulong srcSize, uint srcAdler, ulong cmpSize, uint cmpAdler, 
				uint mode = 0)
			{
				var bc = Adler32.BitComputer.New;

				bc.ComputeBE(srcSize);
				bc.ComputeBE(srcAdler);
				bc.ComputeBE(cmpSize);
				bc.ComputeBE(cmpAdler);
				bc.ComputeBE(mode);

				return bc.ComputeFinish();
			}
		};

		static readonly Header kBufferedHeader = new Header() {
			HeaderCrc = 0x00330004,
			StreamMode = (uint)Mode.Buffered,
			UncompressedSize = 0,	CompressedSize = 0,
			UncompressedCrc = 1,	CompressedCrc = 1,
		};

		public static void CompressFromStream(IO.EndianWriter blockStream, System.IO.Stream source,
			out uint streamAdler, out int streamSize)
		{
			Contract.Requires<ArgumentNullException>(blockStream != null);
			Contract.Requires<ArgumentNullException>(source != null);

			using (var ms = new System.IO.MemoryStream((int)source.Length + Header.kSizeOf))
			using (var s = new IO.EndianStream(ms, Shell.EndianFormat.Big, permissions: FA.Write))
			using (var cs = new CompressedStream())
			{
				s.StreamMode = FA.Write;

				cs.InitializeFromStream(source);
				cs.Compress();

				cs.Serialize(s);

				byte[] final_buffer = ms.GetBuffer();
				streamAdler = Adler32.Compute(final_buffer);
				streamSize = final_buffer.Length;
				blockStream.Write(final_buffer);
			}
		}
		public static byte[] DecompressFromStream(IO.EndianStream blockStream)
		{
			Contract.Requires<ArgumentNullException>(blockStream != null);
			Contract.Ensures(Contract.Result<byte[]>() != null);

			byte[] buffer;
			using (var cs = new CompressedStream())
			{
				cs.Serialize(blockStream);
				cs.Decompress();
				buffer = cs.UncompressedData;
			}

			return buffer;
		}
	};
}