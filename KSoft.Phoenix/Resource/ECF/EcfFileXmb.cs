using System;

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
				System.Diagnostics.Debug.Fail("TODO");

				chunk.IsDeflateStream = true;
			}
		}

		private static Phoenix.Xmb.XmbFile ReadXmbFromStream(IO.EndianStream xmbStream, Xmb.XmbFileContext xmbFileContext)
		{
			ArgumentNullException.ThrowIfNull(xmbStream);
			if (!xmbStream.CanRead)
			{
				throw new ArgumentException("Stream must be readable", nameof(xmbStream));
			}

			byte[] xmbBytes;

			using (var xmb = new ECF.EcfFileXmb())
			{
				xmb.Serialize(xmbStream);

				xmbBytes = xmb.FileData;

				// Unsure how these users hit this being null, assuming this was a problem in a previous release
				// https://github.com/HaloMods/HaloWarsDocs/issues/4
				// https://github.com/HaloMods/HaloWarsDocs/issues/5
				if (xmbBytes == null)
				{
					throw new System.IO.InvalidDataException($"Failed to find {nameof(FileData)} in {xmbStream.StreamName}");
				}
			}

			using (var ms = new System.IO.MemoryStream(xmbBytes, false))
			using (var s = new IO.EndianReader(ms, xmbStream.ByteOrder,
					name: $"{xmbStream.StreamName}:{nameof(FileData)}"))
			{
				s.UserData = xmbFileContext;

				//using (var xmbf = new Phoenix.Xmb.XmbFile())
				var xmbf = new Phoenix.Xmb.XmbFile();
				{
					xmbf.Read(s);
					return xmbf;
				}
			}
		}

		public static void XmbToXml(IO.EndianStream xmbStream, System.IO.Stream outputStream, Shell.ProcessorSize vaSize)
		{
			var xmbFileContext = new Xmb.XmbFileContext()
			{
				PointerSize = vaSize,
			};

			using (Phoenix.Xmb.XmbFile xmbf = ReadXmbFromStream(xmbStream, xmbFileContext))
			{
				xmbf.ToXml(outputStream);
			}
		}

		public static Xmb.Single24DumpInfo DumpSingle24Values(IO.EndianStream xmbStream, Shell.ProcessorSize vaSize)
		{
			var xmbFileContext = new Xmb.XmbFileContext()
			{
				PointerSize = vaSize,

				CallOnRawDataRead = true, // #HACK
			};

			var dumpInfo = new Xmb.Single24DumpInfo()
			{
				SourcePath = xmbStream.StreamName,
			};

			using (Phoenix.Xmb.XmbFile xmbf = ReadXmbFromStream(xmbStream, xmbFileContext))
			{
				if (xmbf.DumpSingle24Values(dumpInfo))
				{
					return dumpInfo;
				}
			}

			return null;
		}
	};
}
