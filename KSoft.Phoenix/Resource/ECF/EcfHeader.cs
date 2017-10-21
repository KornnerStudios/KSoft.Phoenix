using System;
using System.IO;
using Contracts = System.Diagnostics.Contracts;
using Contract = System.Diagnostics.Contracts.Contract;

// http://en.wikipedia.org/wiki/Unix_File_System

namespace KSoft.Phoenix.Resource.ECF
{
	public struct EcfHeader
		: IO.IEndianStreamSerializable
	{
		public const uint kSignature = 0xDABA7737;
		public const int kSizeOf = 0x20;
		public const int kAdler32StartOffset = sizeof(uint) * 3;

		public int HeaderSize;
		// Checksum of the TotalSize field and onward, added to the checksum of everything after the header (HeaderSize - sizeof(ECFHeader))
		public uint Adler32;

		public int TotalSize;
		public short ChunkCount;
		private ushort mFlags;
		private uint mID; // The signature of the data which this header encapsulates
		private ushort mExtraDataSize;

		public int Adler32BufferLength { get { return HeaderSize - kAdler32StartOffset; } }
		public ushort ExtraDataSize { get { return mExtraDataSize; } }

		public void InitializeChunkInfo(uint dataId, uint dataChunkExtraDataSize = 0)
		{
			mID = dataId;
			mExtraDataSize = (ushort)dataChunkExtraDataSize;
		}

		public void BeginBlock(IO.IKSoftBinaryStream s)
		{
			s.VirtualAddressTranslationInitialize(Shell.ProcessorSize.x32);
			s.VirtualAddressTranslationPush(s.PositionPtr);
		}
		public void EndBlock(IO.IKSoftBinaryStream s)
		{
			s.VirtualAddressTranslationPop();
		}

		public void UpdateTotalSize(System.IO.Stream s, int startOffset = 0)
		{
			TotalSize = (int)s.Length - startOffset;
		}

		#region IEndianStreamSerializable Members
		public void Serialize(IO.EndianStream s)
		{
			s.StreamSignature(kSignature);
			s.Stream(ref HeaderSize);
			s.Stream(ref Adler32);
			s.Stream(ref TotalSize);
			s.Stream(ref ChunkCount);
			s.Stream(ref mFlags);

			if (s.IsReading && mFlags != 0)
			{
				// TODO: trace
				System.Diagnostics.Debugger.Break();
			}

			s.Stream(ref mID);
			s.Stream(ref mExtraDataSize);
			s.Pad(sizeof(short) + sizeof(int));
		}
		#endregion

		public uint ComputeAdler32(Stream stream, long headerPosition)
		{
			Contract.Requires(stream != null);
			Contract.Requires(headerPosition >= 0);

			long current_position = stream.Position;

			long adler_start_position = headerPosition + kAdler32StartOffset;
			stream.Seek(adler_start_position, SeekOrigin.Begin);
			var adler = Security.Cryptography.Adler32.Compute(stream, Adler32BufferLength);

			stream.Seek(current_position, SeekOrigin.Begin);

			return adler;
		}

		public void ComputeAdler32AndWrite(IO.EndianStream s, long headerPosition)
		{
			Contract.Requires(s != null);
			Contract.Requires(headerPosition >= 0);

			long current_position = s.BaseStream.Position;

			long adler_start_position = headerPosition + kAdler32StartOffset;
			s.BaseStream.Seek(adler_start_position, SeekOrigin.Begin);
			var adler = Security.Cryptography.Adler32.Compute(s.BaseStream, Adler32BufferLength);

			s.BaseStream.Seek(headerPosition, SeekOrigin.Begin);
			this.Serialize(s);

			s.BaseStream.Seek(current_position, SeekOrigin.Begin);
		}
	};
}