using System;
using System.Diagnostics.CodeAnalysis;

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

		public byte[]? CompressedData { get; private set; }
		public byte[]? UncompressedData { get; private set; }

		public CompressedStream(bool useBufferedStreaming = false)
		{
			mUseBufferedStreaming = useBufferedStreaming;
		}

		#region IDisposable Members
		public void Dispose()
		{
			if (UncompressedData != null)
			{
				UncompressedData = null;
			}

			if (CompressedData != null)
			{
				CompressedData = null;
			}
		}
		#endregion

		#region IEndianStreamSerializable Members
		void StreamCompressedData(IO.EndianStream s)
		{
			if (s.IsReading)
			{
				CompressedData = new byte[(int)mHeader.CompressedSize];
			}

			var compressedData = CompressedData;
			ArgumentNullException.ThrowIfNull(compressedData);
			s.Stream(compressedData);
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
			var compressedData = CompressedData;
			ArgumentNullException.ThrowIfNull(compressedData);

			if (chunkStart == compressedData.Length)
			{
				s.Write((ushort)0x0000);
				s.Write((ushort)0xFFFF); // ~0
				return 0;
			}

			int chunk_size = (bytesRemaining < kBufferedSize)
				? compressedData.Length % kBufferedSize
				: kBufferedSize;
			bytesRemaining -= chunk_size;

			s.Write((ushort)chunk_size);
			s.Write((ushort)~chunk_size);
			s.Write(compressedData, chunkStart, chunk_size);

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
			var compressedData = CompressedData;
			ArgumentNullException.ThrowIfNull(compressedData);

			for (int offset = 0, size, bytes_remaining = compressedData.Length;
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
				if (writing && mHeader.StreamMode != (uint)Mode.Streaming)
				{
					throw new InvalidOperationException(string.Create(KSoft.Util.InvariantCultureInfo,
						$"Unbuffered compressed streams must use mode {(uint)Mode.Streaming}; actual mode is {mHeader.StreamMode}."));
				}

				StreamCompressedData(s);
			}
			else
			{
				if (writing && mHeader.StreamMode != (uint)Mode.Buffered)
				{
					throw new InvalidOperationException(string.Create(KSoft.Util.InvariantCultureInfo,
						$"Buffered compressed streams must use mode {(uint)Mode.Buffered}; actual mode is {mHeader.StreamMode}."));
				}

				StreamCompressedDataInChunks(s);
				s.Stream(ref mHeader); // actual header appears after the chunks

				if (writing && mHeader.StreamMode != (uint)Mode.BufferedEnd)
				{
					throw new InvalidOperationException(string.Create(KSoft.Util.InvariantCultureInfo,
						$"Buffered compressed stream footer must use mode {(uint)Mode.BufferedEnd}; actual mode is {mHeader.StreamMode}."));
				}
			}
		}
		#endregion

		[MemberNotNull(nameof(UncompressedData))]
		public void ReadData(System.IO.Stream s)
		{
			UncompressedData = new byte[mHeader.UncompressedSize];
			s.ReadExactly(UncompressedData);

			mHeader.UncompressedAdler32 = Adler32.Compute(UncompressedData.AsSpan());
		}
		public void WriteData(System.IO.Stream s)
		{
			var uncompressedData = UncompressedData;
			ArgumentNullException.ThrowIfNull(uncompressedData);
			s.Write(uncompressedData, 0, uncompressedData.Length);
		}
		[MemberNotNull(nameof(UncompressedData))]
		public void InitializeFromStream(System.IO.Stream source)
		{
			ArgumentNullException.ThrowIfNull(source);
			if (!source.CanRead)
			{
				throw new ArgumentException("Stream must be readable.", nameof(source));
			}

			mHeader.UncompressedSize = (ulong)source.Length;
			ReadData(source);
		}

		[MemberNotNull(nameof(CompressedData))]
		public void Compress(int level = 5)
		{
			if (!level.IsNone() && (level < IO.Compression.ZLib.kNoCompression || level > IO.Compression.ZLib.kBestCompression))
			{
				throw new ArgumentOutOfRangeException(nameof(level));
			}

			var uncompressedData = UncompressedData;
			ArgumentNullException.ThrowIfNull(uncompressedData);

			// Assume the compressed data will be at most the same size as the uncompressed data
			var compressedData = CompressedData;
			if (compressedData == null || compressedData.Length < (int)mHeader.UncompressedSize)
			{
				compressedData = new byte[mHeader.UncompressedSize];
			}
			else
			{
				Array.Clear(compressedData, 0, compressedData.Length);
			}

			compressedData = IO.Compression.ZLib.LowLevelCompress(uncompressedData, level,
				out uint adler32/*mHeader.CompressedAdler32*/, compressedData);
			CompressedData = compressedData;

			mHeader.CompressedAdler32 = Adler32.Compute(compressedData.AsSpan());
			if (mHeader.CompressedAdler32 != adler32)
			{
#if false
				Debug.Trace.Resource.TraceInformation("ZLib.LowLevelCompress returned different adler32 ({0}) than our computations ({1}). Uncompressed adler32={2}",
					adler32.ToString("X8"),
					mHeader.CompressedAdler32.ToString("X8"),
					mHeader.UncompressedAdler32.ToString("X8"));
#endif
			}

			mHeader.CompressedSize = (ulong)compressedData.LongLength;
		}
		[MemberNotNull(nameof(UncompressedData))]
		public void Decompress()
		{
			var compressedData = CompressedData;
			ArgumentNullException.ThrowIfNull(compressedData);

			var uncompressedData = UncompressedData;
			if (uncompressedData == null || uncompressedData.Length < (int)mHeader.UncompressedSize)
			{
				uncompressedData = new byte[mHeader.UncompressedSize];
			}
			else
			{
				Array.Clear(uncompressedData, 0, uncompressedData.Length);
			}

			UncompressedData = uncompressedData;
			IO.Compression.ZLib.LowLevelDecompress(compressedData, uncompressedData);
		}
	};
}
