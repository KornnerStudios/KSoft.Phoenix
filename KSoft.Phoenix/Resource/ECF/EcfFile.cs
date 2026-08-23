using System;
using System.Collections;
using System.Collections.Generic;

namespace KSoft.Phoenix.Resource.ECF
{
	// http://en.wikipedia.org/wiki/Unix_File_System

	public class EcfFile
		: IDisposable
		, IO.IEndianStreamSerializable
		, IEnumerable<EcfChunk>
	{
		internal EcfHeader mHeader = new();
		protected List<EcfChunk> mChunks = new();

		public int ChunksCount => mChunks.Count;

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
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Usage", "CA1816:Dispose methods should call SuppressFinalize",
			Justification = "Not expecting any derived classes to have Finalizers")]
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
			KSoft.Debug.TypeCheck.TryCastReference(s.Owner, out EcfFileUtil? ecfFile);

			if (s.IsWriting)
			{
				mHeader.ChunkCount = (short)mChunks.Count;

				if (isFinalizing)
				{
					mHeader.UpdateTotalSize(s.BaseStream);
				}
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
					ecfFile != null &&
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
			=> ((IEnumerable<EcfChunk>)mChunks).GetEnumerator();

		IEnumerator IEnumerable.GetEnumerator()
			=> ((IEnumerable<EcfChunk>)mChunks).GetEnumerator();

		public EcfChunk GetChunk(int chunkIndex)
		{
			if (chunkIndex < 0 || chunkIndex >= ChunksCount)
			{
				throw new ArgumentOutOfRangeException(nameof(chunkIndex));
			}

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
