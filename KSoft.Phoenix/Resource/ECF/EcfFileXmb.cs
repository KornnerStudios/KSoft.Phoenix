#if CONTRACTS_FULL_SHIM
using Contract = System.Diagnostics.ContractsShim.Contract;
#else
using Contract = System.Diagnostics.Contracts.Contract; // SHIM'D
#endif

namespace KSoft.Phoenix.Resource.ECF
{
	public sealed class EcfFileXmb
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

					// chunk.IsResourceTag
					case ResourceTagHeader.kChunkId:
						// #TODO
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

		public static void XmbToXml(IO.EndianStream xmbStream, System.IO.Stream outputStream, Shell.ProcessorSize vaSize)
		{
			byte[] xmbBytes;

			using (var xmb = new ECF.EcfFileXmb())
			{
				xmb.Serialize(xmbStream);

				xmbBytes = xmb.FileData;
			}

			var context = new Xmb.XmbFileContext()
			{
				PointerSize = vaSize,
			};

			using (var ms = new System.IO.MemoryStream(xmbBytes, false))
			using (var s = new IO.EndianReader(ms, xmbStream.ByteOrder))
			{
				s.UserData = context;

				using (var xmbf = new Phoenix.Xmb.XmbFile())
				{
					xmbf.Read(s);
					xmbf.ToXml(outputStream);
				}
			}
		}
	};
}