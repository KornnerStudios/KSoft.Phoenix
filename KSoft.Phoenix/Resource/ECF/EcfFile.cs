using System;
using System.Collections.Generic;
using Contracts = System.Diagnostics.Contracts;
using Contract = System.Diagnostics.Contracts.Contract;
using System.Collections;

namespace KSoft.Phoenix.Resource.ECF
{
	// http://en.wikipedia.org/wiki/Unix_File_System

	public class EcfFile
		: IDisposable
		, IO.IEndianStreamSerializable
		, IEnumerable<EcfChunk>
	{
		internal EcfHeader mHeader = new EcfHeader();
		protected List<EcfChunk> mChunks = new List<EcfChunk>();

		public int ChunksCount { get { return mChunks.Count; } }

		public EcfFile()
		{
			mHeader.HeaderSize = EcfHeader.kSizeOf;
		}

		public void InitializeChunkInfo(uint dataId, uint dataChunkExtraDataSize = 0)
		{
			mHeader.InitializeChunkInfo(dataId, dataChunkExtraDataSize);
		}

		public int CalculateHeaderAndChunkEntriesSize()
		{
			return
				mHeader.HeaderSize +
				mHeader.CalculateChunkEntriesSize(ChunksCount);
		}

		#region IDisposable Members
		public virtual void Dispose()
		{
			mChunks.Clear();
		}
		#endregion

		#region IEndianStreamSerializable Members
		public virtual void Serialize(IO.EndianStream s)
		{
			SerializeBegin(s);
			SerializeChunkHeaders(s);
			SerializeEnd(s);
		}

		internal void SerializeBegin(IO.EndianStream s
			, bool isFinalizing = false)
		{
			var ecfFile = s.Owner as EcfFileUtil;

			if (s.IsWriting)
			{
				mHeader.ChunkCount = (short)mChunks.Count;

				if (isFinalizing)
					mHeader.UpdateTotalSize(s.BaseStream);
			}

			long header_position = s.BaseStream.CanSeek
				? s.BaseStream.Position
				: -1;

			mHeader.BeginBlock(s);
			mHeader.Serialize(s);

			// verify or update the header checksum
			if (s.IsReading)
			{
				if (header_position != -1 &&
					!ecfFile.Options.Test(EraFileUtilOptions.SkipVerification))
				{
					var actual_adler = mHeader.ComputeAdler32(s.BaseStream, header_position);
					if (actual_adler != mHeader.Adler32)
					{
						throw new System.IO.InvalidDataException(string.Format(
							"ECF header adler32 {0} does not match actual adler32 {1}",
							mHeader.Adler32.ToString("X8"),
							actual_adler.ToString("X8")
							));
					}
				}
			}
			else if (s.IsWriting)
			{
				if (header_position != -1 && isFinalizing)
				{
					mHeader.ComputeAdler32AndWrite(s, header_position);
				}
			}
		}

		internal void SerializeChunkHeaders(IO.EndianStream s)
		{
			s.StreamListElementsWithClear(mChunks, mHeader.ChunkCount, () => new EcfChunk());
		}

		internal void SerializeEnd(IO.EndianStream s)
		{
			mHeader.EndBlock(s);
		}
		#endregion

		#region Chunk accessors
		public IEnumerator<EcfChunk> GetEnumerator()
		{
			return ((IEnumerable<EcfChunk>)mChunks).GetEnumerator();
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return ((IEnumerable<EcfChunk>)mChunks).GetEnumerator();
		}

		public EcfChunk GetChunk(int chunkIndex)
		{
			Contract.Requires<ArgumentOutOfRangeException>(chunkIndex >= 0 && chunkIndex < ChunksCount);

			return mChunks[chunkIndex];
		}
		#endregion

		public void CopyHeaderDataTo(EcfFileDefinition definition)
		{
			definition.CopyHeaderData(mHeader);
		}

		public void SetupHeaderAndChunks(EcfFileDefinition definition)
		{
			mChunks.Clear();

			definition.UpdateHeader(ref mHeader);
			foreach (var chunk in definition.Chunks)
			{
				var rawChunk = new EcfChunk();
				chunk.SetupRawChunk(rawChunk, ChunksCount);
				mChunks.Add(rawChunk);
			}
		}
	};
}