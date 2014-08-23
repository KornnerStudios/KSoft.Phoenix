using Contracts = System.Diagnostics.Contracts;
using Contract = System.Diagnostics.Contracts.Contract;

namespace KSoft.Phoenix.Resource.ECF
{
	/*public*/ sealed class EcfFileXmb
		: EcfFile
	{
		const uint kSignature = 0xE43ABC00;
		const ulong kChunkId = 0x00000000A9C96500;

		public byte[] FileData;

		public EcfFileXmb()
		{
			InitializeChunkInfo(kSignature);
		}

		public override void Dispose()
		{
			base.Dispose();

			FileData = null;
		}

		public override void Serialize(IO.EndianStream s)
		{
			base.Serialize(s);

			var chunk = mChunks[0];
			if (chunk.EntryId != kChunkId)
				throw new System.IO.InvalidDataException(chunk.EntryId.ToString("X16"));

			if (s.IsReading)
			{
				chunk.SeekTo(s);
				FileData = CompressedStream.DecompressFromStream(s);
			}
			else if (s.IsWriting)
			{
				Contract.Assert(false);
			}
		}
	};
}