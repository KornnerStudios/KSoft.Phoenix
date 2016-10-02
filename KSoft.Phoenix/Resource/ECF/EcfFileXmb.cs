﻿using Contracts = System.Diagnostics.Contracts;
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

			foreach (var chunk in mChunks)
			{
				if (s.IsReading)
				{
					chunk.SeekTo(s);
				}

				switch (chunk.EntryId)
				{
					case kChunkId:
						SerializeMainChunk(chunk, s);
						break;

					default:
						throw new KSoft.Debug.UnreachableException(chunk.EntryId.ToString("X16"));
				}
			}
		}

		private void SerializeMainChunk(EcfChunk chunk, IO.EndianStream s)
		{
			if (s.IsReading)
			{
				if (!chunk.IsDeflateStream)
				{
					throw new System.IO.InvalidDataException(string.Format("{0}'s is supposed to be an XMB but isn't compressed",
						chunk.EntryId.ToString("X16")));
				}

				FileData = CompressedStream.DecompressFromStream(s);
			}
			else if (s.IsWriting)
			{
				Contract.Assert(false);

				chunk.IsDeflateStream = true;
			}
		}
	};
}