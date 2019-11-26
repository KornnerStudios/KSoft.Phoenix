﻿using System;
using System.IO;
#if CONTRACTS_FULL_SHIM
using Contract = System.Diagnostics.ContractsShim.Contract;
#else
using Contract = System.Diagnostics.Contracts.Contract; // SHIM'D
#endif

namespace KSoft.Phoenix.Resource
{
	sealed class GameTitleMemory0
		: IO.IEndianStreamSerializable
	{
		const byte kVersion = 5;
		public int AutomaticDifficultyMultiplier; // max: 200
		public int Unk8; // max: 100

		byte UnkLength; // 0xD in mine (latest) TU
		// 0x1...0xD
		public byte[] UnkC { get; private set; }
		// 6 6 5 2 3 3 3 0 0 0 4 0 0
		public byte[] Unk18 { get; private set; }

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

	sealed class GameTitleMemory1
		: IO.IEndianStreamSerializable
	{
		const byte kVersion = 0x1B;
		const byte kVersionTU = 0x1C;

		#region IEndianStreamSerializable Members
		public void Serialize(IO.EndianStream s)
		{
			s.StreamVersion(kVersionTU);

			Contract.Assert(false, "TODO");
		}
		#endregion
	};

	public sealed class GameTitleMemory
	{
		long mMemoryOffset0, mMemoryOffset1;
		GameTitleMemory0 mMemory0 = new GameTitleMemory0();
		GameTitleMemory1 mMemory1 = new GameTitleMemory1();

		public void SetTitleMemoryOffsets(Stream gpdBaseStream, long tm0, long tm1)
		{
			Contract.Requires<ArgumentNullException>(gpdBaseStream != null);
			Contract.Requires<ArgumentException>(gpdBaseStream.CanSeek);
			Contract.Requires<ArgumentOutOfRangeException>(tm0 >= 0 && tm0 < gpdBaseStream.Length);
			Contract.Requires<ArgumentOutOfRangeException>(tm1 >= 0 && tm1 < gpdBaseStream.Length);

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
				Contract.Assert(false, "TODO");

				using (var ms = new MemoryStream())
				using (var s = new IO.EndianStream(ms, FileAccess.Write))
				{
					s.StreamMode = FileAccess.Write;
					s.Stream(tm);

					ms.Seek(0, SeekOrigin.Begin);
					uint stream_adlr;
					int stream_size;
					CompressedStream.CompressFromStream(gpdStream.Writer, ms, out stream_adlr, out stream_size);
				}
			}
		}
		public void SerializeTitleMemory0(IO.EndianStream gpdStream)
		{
			Contract.Requires<ArgumentNullException>(gpdStream != null);
			Contract.Requires<ArgumentException>(gpdStream.BaseStream.CanSeek);

			SerializeTitleMemory(gpdStream, mMemoryOffset0, mMemory0);
		}
		public void SerializeTitleMemory1(IO.EndianStream gpdStream)
		{
			Contract.Requires<ArgumentNullException>(gpdStream != null);
			Contract.Requires<ArgumentException>(gpdStream.BaseStream.CanSeek);

			SerializeTitleMemory(gpdStream, mMemoryOffset1, mMemory1);
		}
	};
}