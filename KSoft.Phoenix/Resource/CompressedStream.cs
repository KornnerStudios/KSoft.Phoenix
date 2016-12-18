using System;
using Contracts = System.Diagnostics.Contracts;
using Contract = System.Diagnostics.Contracts.Contract;

namespace KSoft.Phoenix.Resource
{
	using Adler32 = Security.Cryptography.Adler32;

	public sealed partial class CompressedStream
		: IO.IEndianStreamSerializable
		, IDisposable
	{
		enum Mode : uint
		{
			Streaming,
			Buffered,
			BufferedEnd,
		};

		/// <summary>Max size of a chunk of a in a buffered stream</summary>
		const int kBufferedSize = 0x2000; // cOutBufSize

		public const uint kSignature = 0xCC34EEAD;
		const uint kSignatureEndOfStream = 0xA5D91776;

		Header mHeader;
		bool mUseBufferedStreaming;

		public byte[] CompressedData { get; private set; }
		public byte[] UncompressedData { get; private set; }

		public CompressedStream(bool useBufferedStreaming = false)
		{
			mUseBufferedStreaming = useBufferedStreaming;
		}

		#region IDisposable Members
		public void Dispose()
		{
			if (UncompressedData != null)
				UncompressedData = null;

			if (CompressedData != null)
				CompressedData = null;
		}
		#endregion

		#region IEndianStreamSerializable Members
		void StreamCompressedData(IO.EndianStream s)
		{
			if (s.IsReading)
				CompressedData = new byte[(int)mHeader.CompressedSize];

			s.Stream(CompressedData);
			s.StreamSignature(kSignatureEndOfStream);
		}

		#region Chunk buffering
		int ReadChunk(IO.EndianReader s, System.IO.MemoryStream ms)
		{
			ushort chunk_size = s.ReadUInt16(), negate_chunk_size = s.ReadUInt16();

			ushort expected_negate = (ushort)~chunk_size;
			if (expected_negate != negate_chunk_size)
			{
				throw new IO.SignatureMismatchException(s.BaseStream,
					expected_negate, negate_chunk_size);
			}

			byte[] bytes = s.ReadBytes(chunk_size);
			ms.Write(bytes, 0, bytes.Length);

			return chunk_size;
		}
		int WriteChunk(IO.EndianWriter s, int chunkStart, ref int bytesRemaining)
		{
			if (chunkStart == CompressedData.Length)
			{
				s.Write((ushort)0x0000);
				s.Write((ushort)0xFFFF); // ~0
				return 0;
			}

			int chunk_size = (bytesRemaining < kBufferedSize)
				? CompressedData.Length % kBufferedSize
				: kBufferedSize;
			bytesRemaining -= chunk_size;

			s.Write((ushort)chunk_size);
			s.Write((ushort)~chunk_size);
			s.Write(CompressedData, chunkStart, chunk_size);

			return chunk_size;
		}
		void ReadCompressedDataInChunks(IO.EndianStream s, int initBufferCapacity)
		{
			using (var ms = new System.IO.MemoryStream(initBufferCapacity))
			{
				while (ReadChunk(s.Reader, ms) != 0)
				{
				}
				s.StreamSignature(kSignatureEndOfStream);

				CompressedData = ms.ToArray();
			}
		}
		void WriteCompressedDataInChunks(IO.EndianStream s)
		{
			for (int offset = 0, size = 0, bytes_remaining = CompressedData.Length;
				(size = WriteChunk(s.Writer, offset, ref bytes_remaining)) != 0;
				offset += size)
			{
			}

			s.StreamSignature(kSignatureEndOfStream);
		}
		void StreamCompressedDataInChunks(IO.EndianStream s, int initBufferCapacity = 4096)
		{
			if (s.IsReading)
			{
				ReadCompressedDataInChunks(s, initBufferCapacity);
			}
			else if (s.IsWriting)
			{
				WriteCompressedDataInChunks(s);
			}
		}
		#endregion

		public void Serialize(IO.EndianStream s)
		{
			bool writing = s.IsWriting;

			if (s.IsReading)
			{
				s.Stream(ref mHeader);

				mHeader.UpdateHeaderCrc();
				mUseBufferedStreaming = mHeader.UseBufferedStreaming;
			}
			else if (writing)
			{
				var head = mUseBufferedStreaming
					? kBufferedHeader
					: mHeader;
				s.Stream(ref head);
			}

			if (!mUseBufferedStreaming)
			{
				Contract.Assert(!writing || mHeader.StreamMode == (uint)Mode.Streaming);

				StreamCompressedData(s);
			}
			else
			{
				Contract.Assert(!writing || mHeader.StreamMode == (uint)Mode.Buffered);

				StreamCompressedDataInChunks(s);
				s.Stream(ref mHeader); // actual header appears after the chunks

				Contract.Assert(!writing || mHeader.StreamMode == (uint)Mode.BufferedEnd);
			}
		}
		#endregion

		public void ReadData(System.IO.Stream s)
		{
			UncompressedData = new byte[mHeader.UncompressedSize];
			s.Read(UncompressedData, 0, UncompressedData.Length);

			mHeader.UncompressedAdler32 = Adler32.Compute(UncompressedData);
		}
		public void WriteData(System.IO.Stream s)
		{
			s.Write(UncompressedData, 0, UncompressedData.Length);
		}
		public void InitializeFromStream(System.IO.Stream source)
		{
			Contract.Requires(source.CanRead);

			mHeader.UncompressedSize = (ulong)source.Length;
			ReadData(source);
		}

		public void Compress(int level = 5)
		{
			Contract.Requires(level.IsNone() ||
				(level >= IO.Compression.ZLib.kNoCompression && level <= IO.Compression.ZLib.kBestCompression));

			// Assume the compressed data will be at most the same size as the uncompressed data
			if (CompressedData == null || CompressedData.Length < (int)mHeader.UncompressedSize)
			{
				CompressedData = new byte[mHeader.UncompressedSize];
			}
			else
			{
				Array.Clear(CompressedData, 0, CompressedData.Length);
			}

			uint adler32;
			CompressedData = IO.Compression.ZLib.LowLevelCompress(UncompressedData, level,
				out adler32/*mHeader.CompressedAdler32*/, CompressedData);

			mHeader.CompressedAdler32 = Adler32.Compute(CompressedData);
			if (mHeader.CompressedAdler32 != adler32)
			{
#if false
				Debug.Trace.Resource.TraceInformation("ZLib.LowLevelCompress returned different adler32 ({0}) than our computations ({1}). Uncompressed adler32={2}",
					adler32.ToString("X8"),
					mHeader.CompressedAdler32.ToString("X8"),
					mHeader.UncompressedAdler32.ToString("X8"));
#endif
			}

			mHeader.CompressedSize = (ulong)CompressedData.LongLength;
		}
		public void Decompress()
		{
			if (UncompressedData == null || UncompressedData.Length < (int)mHeader.UncompressedSize)
			{
				UncompressedData = new byte[mHeader.UncompressedSize];
			}
			else
			{
				Array.Clear(UncompressedData, 0, UncompressedData.Length);
			}

			IO.Compression.ZLib.LowLevelDecompress(CompressedData, UncompressedData);
		}
	};
}