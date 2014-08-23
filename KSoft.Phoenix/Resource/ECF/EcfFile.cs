using System;
using Contracts = System.Diagnostics.Contracts;
using Contract = System.Diagnostics.Contracts.Contract;

namespace KSoft.Phoenix.Resource.ECF
{
	// http://en.wikipedia.org/wiki/Unix_File_System
	
	/*public*/ class EcfFile
		: IDisposable
		, IO.IEndianStreamSerializable
	{
		EcfHeader mHeader = new EcfHeader();
		protected EcfChunk[] mChunks;

		public void InitializeChunkInfo(uint dataId, uint dataChunkExtraDataSize = 0)
		{
			mHeader.InitializeChunkInfo(dataId, dataChunkExtraDataSize);
		}

		#region IDisposable Members
		public virtual void Dispose()
		{
			Util.ClearAndNull(ref mChunks);
		}
		#endregion

		#region IEndianStreamSerializable Members
		public virtual void Serialize(IO.EndianStream s)
		{
			if (s.IsWriting)
			{
				mHeader.ChunkCount = (short)mChunks.Length;

				// TODO: need to build chunk position and size info for writing
				Contract.Assert(false);
			}

			mHeader.BeginBlock(s);
			mHeader.Serialize(s);

			if (s.IsReading)
				mChunks = new EcfChunk[mHeader.ChunkCount];
			s.StreamArray(mChunks, () => new EcfChunk());

			mHeader.EndBlock(s);
		}
		#endregion
	};
}