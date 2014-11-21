using System;
using Contracts = System.Diagnostics.Contracts;
using Contract = System.Diagnostics.Contracts.Contract;

namespace KSoft.Phoenix.Resource
{
	/*public*/ sealed class EraFileHeader
		: IO.IEndianStreamSerializable
	{
		const uint kSiganture = 0x17FDBA9C;
		const int kHeaderSize = 0x1E00;

		public static int CalculateHeaderSize() { return kHeaderSize; }

		ECF.EcfHeader mHeader;
		EraFileSignature mSignature = new EraFileSignature();

		public int FileCount { get { return mHeader.ChunkCount; } }

		public EraFileHeader()
		{
			mHeader.InitializeChunkInfo(kSiganture, EraFileEntryChunk.kExtraDataSize);
			mHeader.HeaderSize = kHeaderSize;
		}

		public void UpdateFileCount(int count)
		{
			mHeader.ChunkCount = (short)count;
		}

		#region IEndianStreamSerializable Members
		public void Serialize(IO.EndianStream s)
		{
			if (s.IsWriting)
			{
				mHeader.UpdateTotalSize(s.BaseStream);
			}

			// write the header, but it won't have the correct CRC if things have changed, 
			// or if this is a fresh new archive
			mHeader.Serialize(s);
			mSignature.Serialize(s);

			var leftovers_size = mHeader.HeaderSize - s.BaseStream.Position;
			s.Pad((int)leftovers_size);

			if (s.IsWriting)
			{
				// Update the header if we're using a memory stream for our destination
				var ms = s.BaseStream as System.IO.MemoryStream;
				if (ms != null)
				{
					mHeader.Checksum = Security.Cryptography.Adler32.Compute(ms.GetBuffer(),
						ECF.EcfHeader.kSizeOf, kHeaderSize - ECF.EcfHeader.kSizeOf);

					ms.Seek(0, System.IO.SeekOrigin.Begin);
					mHeader.Serialize(s);
					ms.Seek(kHeaderSize, System.IO.SeekOrigin.Begin);
				}
			}
		}
		#endregion

		public static bool VerifyIsEraAndDecrypted(IO.EndianReader s)
		{
			const int k_sizeof_signature = sizeof(uint);

			Contract.Requires<InvalidOperationException>(s.BaseStream.CanRead);
			Contract.Requires<InvalidOperationException>(s.BaseStream.CanSeek);

			var base_stream = s.BaseStream;
			if ((base_stream.Length - base_stream.Position) < k_sizeof_signature)
			{
				return false;
			}

			uint sig = s.ReadUInt32();
			base_stream.Seek(-k_sizeof_signature, System.IO.SeekOrigin.Current);

			return sig == ECF.EcfHeader.kSignature;
		}
	};
}