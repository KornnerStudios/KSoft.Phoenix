using System;
using System.IO;

namespace KSoft.Phoenix.Resource
{
	// BHumanPlayerAITrackingData
	sealed class GameTitleMemory0
		: IO.IEndianStreamSerializable
	{
		const byte kVersion = 5;
		// AutoDifficultyLevel
		public int AutomaticDifficultyMultiplier; // max: 200
		// AutoDifficultyNumGamesPlayed
		public int Unk8; // max: 100

		// concepts count
		byte UnkLength; // 0xD in mine (latest) TU
		// ConceptIDs
		// 0x1...0xD
		public byte[] UnkC { get; private set; } = null!;
		// ConceptTimesReinforced
		// 6 6 5 2 3 3 3 0 0 0 4 0 0
		public byte[] Unk18 { get; private set; } = null!;

		#region IEndianStreamSerializable Members
		public void Serialize(IO.EndianStream s)
		{
			s.StreamVersion(kVersion);
			s.Stream(ref AutomaticDifficultyMultiplier);
			s.Stream(ref Unk8);

			s.Stream(ref UnkLength);
			if (s.IsReading)
			{
				UnkC = new byte[UnkLength];
				Unk18 = new byte[UnkLength];
			}
			s.Stream(UnkC);
			s.Stream(Unk18);
		}
		#endregion
	};

	// BUserProfile (game settings, stats, etc)
	sealed class GameTitleMemory1
		: IO.IEndianStreamSerializable
	{
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields", Justification = "Retains game-title format version reference data.")]
		const byte kVersion = 0x1B;
		const byte kVersionTU = 0x1C; // Xbox360
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields", Justification = "Retains game-title format version reference data.")]
		const byte kVersion_LatestHWDE = 43;

		#region IEndianStreamSerializable Members
		public void Serialize(IO.EndianStream s)
		{
			s.StreamVersion(kVersionTU);

			System.Diagnostics.Debug.Fail("TODO");
		}
		#endregion
	};

	public sealed class GameTitleMemory
	{
		long mMemoryOffset0, mMemoryOffset1;
		readonly GameTitleMemory0 mMemory0 = new();
		readonly GameTitleMemory1 mMemory1 = new();

		public void SetTitleMemoryOffsets(Stream gpdBaseStream, long tm0, long tm1)
		{
			ArgumentNullException.ThrowIfNull(gpdBaseStream);
			if (!gpdBaseStream.CanSeek)
			{
				throw new ArgumentException("Stream must support seeking.", nameof(gpdBaseStream));
			}
			if (tm0 < 0 || tm0 >= gpdBaseStream.Length)
			{
				throw new ArgumentOutOfRangeException(nameof(tm0));
			}
			if (tm1 < 0 || tm1 >= gpdBaseStream.Length)
			{
				throw new ArgumentOutOfRangeException(nameof(tm1));
			}

			mMemoryOffset0 = tm0;
			mMemoryOffset1 = tm1;
		}

		void SerializeTitleMemory(IO.EndianStream gpdStream, long tmOffset, IO.IEndianStreamSerializable tm)
		{
			gpdStream.Seek(tmOffset);

			if (gpdStream.IsReading)
			{
				byte[] uncompressed_data = CompressedStream.DecompressFromStream(gpdStream);
				using (var ms = new MemoryStream(uncompressed_data, false))
				using (var s = new IO.EndianStream(ms, FileAccess.Read))
				{
					s.StreamMode = FileAccess.Read;
					s.Stream(tm);
				}
			}
			else if (gpdStream.IsWriting)
			{
				System.Diagnostics.Debug.Fail("TODO");

				using (var ms = new MemoryStream())
				using (var s = new IO.EndianStream(ms, FileAccess.Write))
				{
					s.StreamMode = FileAccess.Write;
					s.Stream(tm);

					ms.Seek(0, SeekOrigin.Begin);
					CompressedStream.CompressFromStream(gpdStream.Writer, ms, out uint stream_adlr, out int stream_size);
				}
			}
		}
		public void SerializeTitleMemory0(IO.EndianStream gpdStream)
		{
			ArgumentNullException.ThrowIfNull(gpdStream);
			if (!gpdStream.BaseStream.CanSeek)
			{
				throw new ArgumentException("Stream must support seeking.", nameof(gpdStream));
			}

			SerializeTitleMemory(gpdStream, mMemoryOffset0, mMemory0);
		}
		public void SerializeTitleMemory1(IO.EndianStream gpdStream)
		{
			ArgumentNullException.ThrowIfNull(gpdStream);
			if (!gpdStream.BaseStream.CanSeek)
			{
				throw new ArgumentException("Stream must support seeking.", nameof(gpdStream));
			}

			SerializeTitleMemory(gpdStream, mMemoryOffset1, mMemory1);
		}
	};
}
