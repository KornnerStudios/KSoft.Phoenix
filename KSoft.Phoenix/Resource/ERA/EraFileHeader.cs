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
			var eraFile = s.Owner as EraFileUtil;

			if (s.IsWriting)
			{
				mHeader.UpdateTotalSize(s.BaseStream);
			}

			long header_position = s.BaseStream.CanSeek
				? s.BaseStream.Position
				: -1;

			// write the header, but it won't have the correct CRC if things have changed,
			// or if this is a fresh new archive
			mHeader.Serialize(s);
			mSignature.Serialize(s);

			var leftovers_size = mHeader.HeaderSize - s.BaseStream.Position;
			s.Pad((int)leftovers_size);

			// verify or update the header checksum
			if (s.IsReading)
			{
				if (header_position != -1 &&
					!eraFile.Options.Test(EraFileUtilOptions.SkipVerification))
				{
					long current_position = s.BaseStream.Position;
					s.BaseStream.Seek(header_position+ECF.EcfHeader.kAdler32StartOffset, System.IO.SeekOrigin.Begin);

					var actual_adler = Security.Cryptography.Adler32.Compute(s.BaseStream, mHeader.Adler32BufferLength);
					if (actual_adler != mHeader.Adler32)
					{
						throw new System.IO.InvalidDataException(string.Format(
							"ERA header adler32 {0} does not match actual adler32 {1}",
							mHeader.Adler32.ToString("X8"),
							actual_adler.ToString("X8")
							));
					}

					s.BaseStream.Seek(current_position, System.IO.SeekOrigin.Begin);
				}
			}
			else if (s.IsWriting)
			{
				if (header_position != -1)
				{
					long current_position = s.BaseStream.Position;
					s.BaseStream.Seek(header_position+ECF.EcfHeader.kAdler32StartOffset, System.IO.SeekOrigin.Begin);

					mHeader.Adler32 = Security.Cryptography.Adler32.Compute(s.BaseStream, mHeader.Adler32BufferLength);

					s.BaseStream.Seek(header_position, System.IO.SeekOrigin.Begin);
					mHeader.Serialize(s);
					s.BaseStream.Seek(current_position, System.IO.SeekOrigin.Begin);
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