using System;
using System.Buffers;
using System.IO;
using System.Security.Cryptography;

namespace KSoft.Security.Cryptography
{
	using Debug = Phoenix.Debug;

	static class PhxHash
	{
		public const int kSha1SizeOf = 20;

		public static bool TraceSha1Hash { get; set; }

		// NOTE: data is written to the buffer in MSB order
		static void BufferFillUnicode(byte[] buffer, char unicode)
		{
			Span<byte> unicodeBytes = buffer.AsSpan(0, sizeof(ushort));
			BitConverter.TryWriteBytes(unicodeBytes, (ushort)unicode);
			if (BitConverter.IsLittleEndian)
			{
				unicodeBytes.Reverse();
			}
		}

		static byte[] GetHashResult(SHA1 sha)
		{
			ArgumentNullException.ThrowIfNull(sha);
			return sha.Hash ?? throw new CryptographicException("SHA-1 did not produce a hash result.");
		}

		public static void UInt8(SHA1 sha, uint word, bool isFinal = false)
		{
			byte[] buffer = ArrayPool<byte>.Shared.Rent(sizeof(ulong));
			try
			{
				buffer[0] = (byte)(word >> 0);

				if (isFinal)
				{
					sha.TransformFinalBlock(buffer, 0, sizeof(byte));
				}
				else
				{
					sha.TransformBlock(buffer, 0, sizeof(byte), null, 0);
				}
			}
			finally
			{
				ArrayPool<byte>.Shared.Return(buffer);
			}
		}
		public static void UInt16(SHA1 sha, uint word, bool isFinal = false)
		{
			byte[] buffer = ArrayPool<byte>.Shared.Rent(sizeof(ulong));
			try
			{
				Span<byte> wordBytes = buffer.AsSpan(0, sizeof(ushort));
				BitConverter.TryWriteBytes(wordBytes, (ushort)word);
				if (BitConverter.IsLittleEndian)
				{
					wordBytes.Reverse();
				}

				if (isFinal)
				{
					sha.TransformFinalBlock(buffer, 0, sizeof(ushort));
				}
				else
				{
					sha.TransformBlock(buffer, 0, sizeof(ushort), null, 0);
				}
			}
			finally
			{
				ArrayPool<byte>.Shared.Return(buffer);
			}
		}
		public static void UInt32(SHA1 sha, uint word, bool isFinal = false)
		{
			byte[] buffer = ArrayPool<byte>.Shared.Rent(sizeof(ulong));
			try
			{
				Span<byte> wordBytes = buffer.AsSpan(0, sizeof(uint));
				BitConverter.TryWriteBytes(wordBytes, word);
				if (BitConverter.IsLittleEndian)
				{
					wordBytes.Reverse();
				}

				if (isFinal)
				{
					sha.TransformFinalBlock(buffer, 0, sizeof(uint));
				}
				else
				{
					sha.TransformBlock(buffer, 0, sizeof(uint), null, 0);
				}
			}
			finally
			{
				ArrayPool<byte>.Shared.Return(buffer);
			}
		}
		public static void UInt64(SHA1 sha, ulong word, bool isFinal = false)
		{
			byte[] buffer = ArrayPool<byte>.Shared.Rent(sizeof(ulong));
			try
			{
				Span<byte> wordBytes = buffer.AsSpan(0, sizeof(ulong));
				BitConverter.TryWriteBytes(wordBytes, word);
				if (BitConverter.IsLittleEndian)
				{
					wordBytes.Reverse();
				}

				if (isFinal)
				{
					sha.TransformFinalBlock(buffer, 0, sizeof(ulong));
				}
				else
				{
					sha.TransformBlock(buffer, 0, sizeof(ulong), null, 0);
				}
			}
			finally
			{
				ArrayPool<byte>.Shared.Return(buffer);
			}
		}

		public static void Ascii(SHA1 sha, string str, int fixedLength = 0)
		{
			byte[] buffer = ArrayPool<byte>.Shared.Rent(sizeof(ulong));
			try
			{
				for (int x = 0; x < str.Length; x++)
				{
					buffer[0] = (byte)(str[x] >> 0);
					BufferFillUnicode(buffer, str[x]);
					sha.TransformBlock(buffer, 0, sizeof(byte), null, 0);
				}

				buffer[0] = 0;
				for (int x = 0, null_count = fixedLength - str.Length; x < null_count; x++)
				{
					BufferFillUnicode(buffer, '\0');
					sha.TransformBlock(buffer, 0, sizeof(byte), null, 0);
				}
			}
			finally
			{
				ArrayPool<byte>.Shared.Return(buffer);
			}
		}
		public static void Unicode(SHA1 sha, string str, int fixedLength = 0)
		{
			byte[] buffer = ArrayPool<byte>.Shared.Rent(sizeof(ulong));
			try
			{
				for (int x = 0; x < str.Length; x++)
				{
					BufferFillUnicode(buffer, str[x]);
					sha.TransformBlock(buffer, 0, sizeof(ushort), null, 0);
				}

				BufferFillUnicode(buffer, '\0');
				for (int x = 0, null_count = fixedLength - str.Length; x < null_count; x++)
				{
					sha.TransformBlock(buffer, 0, sizeof(ushort), null, 0);
				}
			}
			finally
			{
				ArrayPool<byte>.Shared.Return(buffer);
			}
		}

		public static void Stream(SHA1 sha
			, System.IO.Stream inputStream
			, long inputOffset
			, long inputLength
			, bool isFinal = false)
		{
			const int k_read_block_size = 4096;

			ArgumentNullException.ThrowIfNull(inputStream);
			if (!inputStream.CanSeek || !inputStream.CanRead)
			{
				throw new ArgumentException("Stream must be readable and seekable.", nameof(inputStream));
			}
			ArgumentOutOfRangeException.ThrowIfNegative(inputOffset);
			ArgumentOutOfRangeException.ThrowIfNegativeOrZero(inputLength);

			long stream_length = inputStream.Length;
			if (inputOffset > stream_length)
			{
				throw new ArgumentOutOfRangeException(nameof(inputOffset));
			}
			if (inputLength > stream_length - inputOffset)
			{
				throw new ArgumentOutOfRangeException(nameof(inputLength));
			}

			var scratch_buffer = new byte[k_read_block_size];

			using (new IO.StreamPositionContext(inputStream))
			{
				inputStream.Seek(inputOffset, System.IO.SeekOrigin.Begin);

				for (long input_bytes_read = 0; input_bytes_read < inputLength; )
				{
					long bytes_remaining = inputLength - input_bytes_read;
					int read_block_length = (int)System.Math.Min(bytes_remaining, (long)scratch_buffer.Length);

					Array.Clear(scratch_buffer, 0, scratch_buffer.Length);
					for (int actual_bytes_read = 0; actual_bytes_read < read_block_length; )
					{
						int sub_block_offset = actual_bytes_read;
						int sub_block_length = read_block_length - sub_block_offset;
						int bytes_read = inputStream.Read(scratch_buffer, sub_block_offset, sub_block_length);
						if (bytes_read == 0)
						{
							throw new EndOfStreamException("The stream ended before the requested range was read.");
						}

						actual_bytes_read += bytes_read;
					}

					sha.TransformBlock(
						scratch_buffer, 0, read_block_length,
						null, 0);
					input_bytes_read += read_block_length;
				}
			}

			if (isFinal)
			{
				sha.TransformFinalBlock(scratch_buffer, 0, 0);
			}
		}

		// #TODO_PHOENIX rename and move this into PhxTEA
		public const int kResultSize = 0x18;
		static void ValidateSha1HashResult(Span<byte> result, int requiredLength)
		{
			ArgumentOutOfRangeException.ThrowIfLessThan(result.Length, requiredLength, nameof(result));
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Cryptography", "CA5350:Do Not Use Weak Cryptographic Algorithms", Justification = "Required for compatibility with the legacy game digest format; not used for security authentication.")]
		public static void Sha1Hash(string str, Span<byte> result)
		{
			ArgumentException.ThrowIfNullOrEmpty(str);
			ValidateSha1HashResult(result, kResultSize);

			byte[] str_bytes = System.Text.Encoding.ASCII.GetBytes(str);

			using (var sha = SHA1.Create())
			{
				byte[] hash1;
				byte[] hash2;

				PhxHash.UInt32(sha, 0xA4800C14);
				//PhxHash.Ascii(sha, str);
				sha.TransformBlock(str_bytes, 0, str_bytes.Length, null, 0);
				PhxHash.UInt32(sha, 0x5AF4A9F1);
				PhxHash.UInt32(sha, 0xCA6884EC, true);
				hash1 = GetHashResult(sha);
#if DEBUG
				if (TraceSha1Hash && System.Diagnostics.Debugger.IsAttached)
				{
					Debug.Trace.Security.TraceInformation("Sha1Hash: {0} Result: {1}", str, Text.Util.ByteArrayToString(hash1));
				}
#endif // DEBUG

				sha.Initialize();
				PhxHash.UInt32(sha, 0xCB92EAEB);
				sha.TransformBlock(hash1, 0, hash1.Length, null, 0);
				PhxHash.UInt32(sha, 0x1D919BF8, true);
				hash2 = GetHashResult(sha);
#if DEBUG
				if (TraceSha1Hash && System.Diagnostics.Debugger.IsAttached)
				{
					Debug.Trace.Security.TraceInformation("Sha1Hash: {0} Final: {1}", str, Text.Util.ByteArrayToString(hash2));
				}
#endif // DEBUG

				// The engine digest is five words from hash2 followed by the first word from hash1.
				{
					const int kHash2WordCount = 5;
					int hash2ByteCount = kHash2WordCount * sizeof(uint);
					ReadOnlySpan<byte> hash2Words = hash2;
					Span<byte> hash2Destination = result[..hash2ByteCount];
					hash2Words.CopyTo(hash2Destination);

					ReadOnlySpan<byte> hash1FirstWord = hash1.AsSpan(0, sizeof(uint));
					Span<byte> hash1Destination = result.Slice(hash2ByteCount, sizeof(uint));
					hash1FirstWord.CopyTo(hash1Destination);
				}

				// we want to read the dwords of the result as big endian, as this is how the engine reads the bytes
				for (int x = 0; x < kResultSize; x += sizeof(uint))
				{
					Span<byte> resultWord = result.Slice(x, sizeof(uint));
					resultWord.Reverse();
				}
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Cryptography", "CA5350:Do Not Use Weak Cryptographic Algorithms", Justification = "Required for compatibility with the legacy game digest format; not used for security authentication.")]
		public static bool Sha1HashFile(string fileName, Span<byte> result, out long fileLength)
		{
			ArgumentException.ThrowIfNullOrEmpty(fileName);
			ValidateSha1HashResult(result, kSha1SizeOf);

			fileLength = -1;

			try
			{
				if (!File.Exists(fileName))
				{
					return false;
				}

				byte[] result_final;

				using (var fs = File.OpenRead(fileName))
				using (var sha = SHA1.Create())
				{
					result_final = sha.ComputeHash(fs, 0, fs.Length);

					fileLength = fs.Length;
				}

				result_final.AsSpan().CopyTo(result);
			}
			catch (IOException ex)
			{
				Debug.Trace.Security.TraceInformation(ex.ToString());
				return false;
			}
			catch (UnauthorizedAccessException ex)
			{
				Debug.Trace.Security.TraceInformation(ex.ToString());
				return false;
			}

			return true;
		}

		public static TigerHashBase CreateHaloWarsTigerHash()
		{
			var tiger = TigerHashBase.Create(TigerHash.kAlgorithmName);

			return tiger ?? throw new InvalidOperationException("Failed to create Halo Wars Tiger hash.");
		}
	};
}
