using System;
using Contracts = System.Diagnostics.Contracts;
using Contract = System.Diagnostics.Contracts.Contract;

// http://en.wikipedia.org/wiki/Unix_File_System

namespace KSoft.Phoenix.Resource.ECF
{
	/*public*/ struct EcfHeader
		: IO.IEndianStreamSerializable
	{
		public const uint kSignature = 0xDABA7737;
		public const int kSizeOf = 0x20;

		public int HeaderSize;
		// Checksum of the TotalSize field and onward, added to the checksum of everything after the header (HeaderSize - sizeof(ECFHeader))
		public uint Checksum;
		public int TotalSize;
		public short ChunkCount;
		short unknown12;

		uint ID; // The signature of the data which this header encapsulates
		ushort ExtraDataSize;

		public void InitializeChunkInfo(uint dataId, uint dataChunkExtraDataSize = 0)
		{
			ID = dataId;
			ExtraDataSize = (ushort)dataChunkExtraDataSize;
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
			s.Stream(ref Checksum);
			s.Stream(ref TotalSize);
			s.Stream(ref ChunkCount);
			s.Stream(ref unknown12);

			if (s.IsReading && unknown12 != 0)
			{
				// TODO: trace
				System.Diagnostics.Debugger.Break();
			}

			s.Stream(ref ID);
			s.Stream(ref ExtraDataSize);
			s.Pad(sizeof(short) + sizeof(int));
		}
		#endregion
	};
}