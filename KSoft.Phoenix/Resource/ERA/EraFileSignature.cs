using System;
using SHA1 = System.Security.Cryptography.SHA1;

namespace KSoft.Phoenix.Resource
{
	using PhxHash = Security.Cryptography.PhxHash;

	/*public*/ sealed class EraFileSignature
		: IO.IEndianStreamSerializable
	{
		const uint kSignature = 0x05ABDBD8;
		const uint kSignatureMarker = 0xAAC94350;
		const byte kDefaultSizeBit = 0x13;

		const int kNonSignatureBytesSize =
			sizeof(uint) + // kSignatureMarker
			sizeof(byte) + // SizeBit, 'tree height'
				// ...hash tree nodes (SHA1)...
			sizeof(uint);  // kSignatureMarker

		const uint kSha1Salt = 0xA7F95F9C;

		// needs to be in a range of [2, 32]
		public byte SizeBit = kDefaultSizeBit;
		public byte[]? SignatureData;

		#region IEndianStreamSerializable Members
		public void Serialize(IO.EndianStream s)
		{
			bool reading = s.IsReading;

			int sig_data_length = reading || SignatureData == null
				? 0
				: SignatureData.Length;
			int size = reading
				? 0
				: kNonSignatureBytesSize + sig_data_length;

			s.StreamSignature(kSignature);
			s.Stream(ref size);
			if (size < kNonSignatureBytesSize)
			{
				throw new System.IO.InvalidDataException(size.ToString("X8", KSoft.Util.InvariantCultureInfo));
			}
			s.Pad64();

			s.StreamSignature(kSignatureMarker);
			s.Stream(ref SizeBit);
			if (reading)
			{
				Array.Resize(ref SignatureData, size - kNonSignatureBytesSize);
				sig_data_length = SignatureData.Length;
			}
			if (sig_data_length > 0)
			{
				byte[] signatureData = SignatureData ?? throw new InvalidOperationException("Signature data was unexpectedly null.");
				s.Stream(signatureData);
			}
			s.StreamSignature(kSignatureMarker);
		}
		#endregion

		/// <summary>
		/// The signature digest (SHA1) is what is digitally signed when the engine's tools when they build
		/// an ERA. The private keys are not known, but the exe has the public keys hard coded.
		/// </summary>
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Cryptography", "CA5350:Do Not Use Weak Cryptographic Algorithms", Justification = "Required for compatibility with the legacy ERA signature format.")]
		internal static byte[] ComputeSignatureDigest(System.IO.Stream chunksStream
			, long chunksOffset
			, long chunksLength
			, ECF.EcfHeader header)
		{
			ArgumentNullException.ThrowIfNull(chunksStream);
			if (!chunksStream.CanSeek || !chunksStream.CanRead)
			{
				throw new ArgumentException("Stream must be readable and seekable.", nameof(chunksStream));
			}
			ArgumentOutOfRangeException.ThrowIfNegative(chunksOffset);
			ArgumentOutOfRangeException.ThrowIfNegativeOrZero(chunksLength);

			using (var sha = SHA1.Create())
			{
				PhxHash.UInt32(sha, kSha1Salt);
				PhxHash.UInt32(sha, (uint)header.HeaderSize);
				PhxHash.UInt32(sha, (uint)header.ChunkCount);
				PhxHash.UInt32(sha, (uint)header.ExtraDataSize);
				PhxHash.UInt32(sha, (uint)header.TotalSize);

				PhxHash.Stream(sha,
					chunksStream, chunksOffset, chunksLength,
					isFinal: true);

				return sha.Hash ?? throw new System.Security.Cryptography.CryptographicException("SHA-1 did not produce a hash result.");
			}
		}
	};
}
