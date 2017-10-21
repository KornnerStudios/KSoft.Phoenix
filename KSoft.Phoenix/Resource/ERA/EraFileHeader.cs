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
					var actual_adler = mHeader.ComputeAdler32(s.BaseStream, header_position);
					if (actual_adler != mHeader.Adler32)
					{
						throw new System.IO.InvalidDataException(string.Format(
							"ERA header adler32 {0} does not match actual adler32 {1}",
							mHeader.Adler32.ToString("X8"),
							actual_adler.ToString("X8")
							));
					}
				}
			}
			else if (s.IsWriting)
			{
				if (header_position != -1)
				{
					mHeader.ComputeAdler32AndWrite(s, header_position);
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