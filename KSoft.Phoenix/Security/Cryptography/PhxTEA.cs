using System;
using System.Collections.Generic;
using System.Threading.Tasks;
namespace KSoft.Security.Cryptography
{
	// https://web.archive.org/web/20070930222335/http://www.simonshepherd.supanet.com/tea.htm
	// https://web.archive.org/web/20070920142755/http://www.simonshepherd.supanet.com/source.htm
	public sealed class PhxTEA
	{
		// the negated value of this is 0xEA10F50CCBDB701E
		public const ulong kDefaultIV = 0x15EF0AF334248FE2UL;
		const ulong kNegatedDefaultIV = unchecked(0UL - kDefaultIV);

		public ulong InitializationVector { get; set; } = kDefaultIV;

		#region Keys
		public const int kKeySize = 3;

		public const string kKeyEraPhrase = "3zDdptN*rV=qOkRbE*NAuWM6";
		/// <summary>
		/// If you feed <see cref="kKeyEraPhrase"/> to <see cref="CreateKeyFromPhrase"/>
		/// this is what you get
		/// </summary>
		public static readonly ulong[] kKeyEra = /*kKeySize*/[
			0xBC3EB6B4D0471DDB,
			0x8299E6431912BE73,
			0x4601515D43D26DF5,
		];
		public const string kKeyGameFilePhrase = "TehHaloz";
		/// <summary>
		/// If you feed <see cref="kKeyGameFilePhrase"/> to <see cref="CreateKeyFromPhrase"/>
		/// this is what you get
		/// </summary>
		public static readonly ulong[] kKeyGameFile = /*kKeySize*/[
			0x194F8D77DF360283,
			0x1385AC1E2122F575,
			0xA7392D249DC2C737,
		];
		public static ulong GameFileKey0 => kKeyGameFile[0];
		public static ulong GameFileKey1 => kKeyGameFile[1];

		ulong[]? mKey;

		public void InitializeKey(ulong[] key, ulong userKey = 0)
		{
			ArgumentNullException.ThrowIfNull(key);
			if (key.Length != kKeySize)
			{
				throw new ArgumentException("Key must contain exactly three 64-bit values.", nameof(key));
			}

			if (userKey == 0)
			{
				mKey = key;
			}
			else
			{
				mKey = [key[0], key[1], userKey];
			}
		}

		public static ulong[] CreateKeyFromPhrase(string keyPhrase)
		{
			ArgumentNullException.ThrowIfNull(keyPhrase);
			var key = new ulong[kKeySize];

			var keyBytes = new byte[PhxHash.kResultSize];
			PhxHash.Sha1Hash(keyPhrase, keyBytes.AsSpan());

			// set key elements from keyBytes, low part then high part
			{
				ulong keyLo = BitConverter.ToUInt32(keyBytes, 0 * sizeof(uint));
				ulong keyHi = BitConverter.ToUInt32(keyBytes, 1 * sizeof(uint));

				key[0] = (keyHi << Bits.kUInt32BitCount) | keyLo;
			}
			{
				ulong keyLo = BitConverter.ToUInt32(keyBytes, 2 * sizeof(uint));
				ulong keyHi = BitConverter.ToUInt32(keyBytes, 3 * sizeof(uint));

				key[1] = (keyHi << Bits.kUInt32BitCount) | keyLo;
			}
			{
				ulong keyLo = BitConverter.ToUInt32(keyBytes, 4 * sizeof(uint));
				ulong keyHi = BitConverter.ToUInt32(keyBytes, 5 * sizeof(uint));

				key[2] = (keyHi << Bits.kUInt32BitCount) | keyLo;
			}


			return key;
		}
		#endregion

		readonly IO.EndianReader mStreamIn;
		readonly IO.EndianWriter mStreamOut;
		ulong[] mBufferIn, mBufferOut;

		[System.Diagnostics.CodeAnalysis.MemberNotNull(nameof(mBufferIn), nameof(mBufferOut))]
		void InitializeBuffers()
		{
			mBufferIn = new ulong[kBlocksPerIteration];
			mBufferOut = new ulong[kBlocksPerIteration];
		}

		public PhxTEA(IO.EndianReader streamIn, IO.EndianWriter streamOut)
		{
			mStreamIn = streamIn;
			mStreamOut = streamOut;
			InitializeBuffers();
		}

		void FillBufferIn()
		{
			for (int x = 0; x < mBufferIn.Length; x++)
			{
				mStreamIn.Read(out mBufferIn[x]);
			}
		}
		void FillBufferOut()
		{
			for (int x = 0; x < mBufferOut.Length; x++)
			{
				mStreamOut.Write(mBufferOut[x]);
			}
		}

		void ProcessBuffer(long size, ProcessIterationProc proc)
		{
			ulong[] key = mKey ?? throw new InvalidOperationException("An encryption key must be initialized before processing.");

			if (size == 0)
			{
				size = mStreamIn.BaseStream.Length - mStreamIn.BaseStream.Position;
			}

			uint interation_count = GetIterationsCount(size);

			for (uint x = 0; x < interation_count; x++)
			{
				FillBufferIn();

				proc(key, mBufferIn, mBufferOut, x);

				FillBufferOut();
			}
		}
		public void Decrypt(long size = 0)
		{
			ProcessBuffer(size, DecryptIterationBlock64);
		}
		public void Encrypt(long size = 0)
		{
			ProcessBuffer(size, EncryptIterationBlock64);
		}

		#region Implementation
		delegate void ProcessIterationProc(ulong[] key, ulong[] buffIn, ulong[] buffOut, uint iteration);

		const uint kDelta = 0x9E3779B9;
		const int k16CycleCountShift = 4;
		const int k16CycleCount = 1 << k16CycleCountShift;

		// A block is 64 bytes (512 bits). Shift by 3 (8 bytes) to get chunks of a block as ulong
		const int kBlocksPerIterationShift = 3;
		const int kBlocksPerIteration = 1 << kBlocksPerIterationShift;

		static uint GetIterationsCount(long size)
		{
			const int kBlockSizeShift = 3;

			uint block_count = (uint)(size >> kBlockSizeShift);

			return block_count >> kBlocksPerIterationShift;
		}

		// used to use a hard coded 0xBCCD0923, which is (uint)(kDefaultIV >> 10)
		uint GetIterationMod(uint iteration)
		{
			uint iter_mod = iteration + (uint)(InitializationVector >> 10);
			if (iter_mod == 0)
			{
				iter_mod++;
			}

			return iter_mod;
		}

		// Linear Feedback Shift Register, with 3 tap positions
		static void LFSR3(ref uint x)
		{
			// bit 0 influenced by bit 17
			x ^= (x << 17);
			// bit 31 influenced by bit 18
			x ^= (x >> 13);
			// bit 0 influenced by bit 27
			x ^= (x << 5);
		}

		static void RevDiffDisperse(ref ulong x, ref ulong y, ref ulong z, ref ulong w)
		{
			// XOR each value with its neighbor in a forward cascade
			x ^= y;
			y ^= z;
			z ^= w;
			w ^= x;
		}
		static void RevDiffContract(ref ulong x, ref ulong y, ref ulong z, ref ulong w)
		{
			// Reverse what was done in RevDiffDisperse
			w ^= x;
			z ^= w;
			y ^= z;
			x ^= y;
		}

		static void Mod0(ref ulong q, out ulong v)
		{
			const ulong kMod = kDefaultIV;

			q = (((uint)q << 17) ^ q);
			q = ((uint)q >> 13) ^ q;
			q = ((uint)q << 5) ^ q;
			v = (uint)q + kMod;
		}
		static void Mod1(ref ulong q, out ulong v)
		{
			const ulong kMod = kNegatedDefaultIV;

			q = (((uint)q << 17) ^ q);
			q = ((uint)q >> 13) ^ q;
			q = ((uint)q << 5) ^ q;
			// or, v = q - kDefaultIV
			v = (uint)q + kMod;
		}

		static void DecryptWith16Cycles(ref ulong v, ulong key0, ulong key1)
		{
			ulong lv0 = v >> 32, lv1 = v & 0xFFFFFFFF;
			uint sum = kDelta << k16CycleCountShift;
			for (int x = 0; x < k16CycleCount; x++)
			{
				lv0 = (lv0
					- (((uint)lv1 >> 5) ^ sum))
					- (lv1 ^ key1)
					- ((uint)lv1 << 4)
					- (key1 >> 32);

				lv1 = (lv1
					- (((uint)lv0 >> 5) ^ sum))
					- (lv0 ^ key0)
					- ((uint)lv0 << 4)
					- (key0 >> 32);

				sum -= kDelta;
			}

			v = (lv0 << 32) | (lv1 & 0xFFFFFFFF);
		}
		static void EncryptWith16Cycles(ref ulong v, ulong key0, ulong key1)
		{
			ulong lv0 = v >> 32, lv1 = v & 0xFFFFFFFF;
			uint sum = 0;
			for (int x = 0; x < k16CycleCount; x++)
			{
				sum += kDelta;

				lv1 = (lv1
					+ (((uint)lv0 >> 5) ^ sum))
					+ (lv0 ^ key0)
					+ ((uint)lv0 << 4)
					+ (key0 >> 32);

				lv0 = (lv0
					+ (((uint)lv1 >> 5) ^ sum))
					+ (lv1 ^ key1)
					+ ((uint)lv1 << 4)
					+ (key1 >> 32);
			}

			v = (lv0 << 32) | (lv1 & 0xFFFFFFFF);
		}

		static void DecryptFourBlocks(ulong in0, ulong in1, ulong in2, ulong in3,
			out ulong out0, out ulong out1, out ulong out2, out ulong out3,
			ulong key0, ulong key1)
		{
			out0 = in0; DecryptWith16Cycles(ref out0, key0, key1);
			out1 = in1; DecryptWith16Cycles(ref out1, key0, key1);
			out2 = in2; DecryptWith16Cycles(ref out2, key0, key1);
			out3 = in3; DecryptWith16Cycles(ref out3, key0, key1);
		}
		static void EncryptFourBlocks(ulong in0, ulong in1, ulong in2, ulong in3,
			out ulong out0, out ulong out1, out ulong out2, out ulong out3,
			ulong key0, ulong key1)
		{
			out0 = in0; EncryptWith16Cycles(ref out0, key0, key1);
			out1 = in1; EncryptWith16Cycles(ref out1, key0, key1);
			out2 = in2; EncryptWith16Cycles(ref out2, key0, key1);
			out3 = in3; EncryptWith16Cycles(ref out3, key0, key1);
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE0018:Inline variable declaration")]
		void DecryptIterationBlock64(ulong[] key, ulong[] buffIn, ulong[] buffOut, uint iteration)
		{
			const uint index = 0;// iteration * kBlocksPerIteration;
			ulong	block0 = buffIn[index+0],	block1 = buffIn[index+1],	block2 = buffIn[index+2],	block3 = buffIn[index+3],
					block4 = buffIn[index+4],	block5 = buffIn[index+5],	block6 = buffIn[index+6],	block7 = buffIn[index+7];

			ulong t1, t2, t3, t4;
			DecryptFourBlocks(	block0, block1, block2, block3,
							out t1, out t2, out t3, out t4,		key[0], key[1] );

			ulong t5, t6, t7, t8;
			DecryptFourBlocks(	block6 ^ block7,	block4 ^ block5 ^ block7,
							block4 ^ block6,	block7 ^ block5,
							out t5, out t6, out t7, out t8,		key[1], key[2]);

			ulong t9, t10, t11, t12;
			DecryptFourBlocks(	t4 ^ block7,
							(t1 - (block6 ^ block7)) ^ t2 ^ block6 ^ block5,
							t2 ^ block6 ^ block5 ^ (t3 + (block5 ^ block4)),
							t4 ^ block7 ^ (t3 + (block5 ^ block4)),
							out t9, out t10, out t11, out t12,	key[1], key[0]);

			ulong q = GetIterationMod(iteration);
			ulong q0, q1, q2, q3, q4, q5, q6, q7;

			Mod0(ref q, out q0);	Mod1(ref q, out q1);
			Mod0(ref q, out q2);	Mod1(ref q, out q3);
			Mod0(ref q, out q4);	Mod1(ref q, out q5);
			Mod0(ref q, out q6);	Mod1(ref q, out q7);

			q0 ^= t9;
			q1 ^= t10;
			q2 ^= t11;
			q3 ^= t12;
			q4 ^= t5 ^ t4 ^ block7;
			q5 ^= t3 + (block5 ^ block4) + t6;
			q6 ^= t7 ^ t2 ^ block6 ^ block5;
			q7 ^= t8 - (t1 - (block6 ^ block7));

			buffOut[index+0] = q0; buffOut[index+1] = q1; buffOut[index+2] = q2; buffOut[index+3] = q3;
			buffOut[index+4] = q4; buffOut[index+5] = q5; buffOut[index+6] = q6; buffOut[index+7] = q7;
		}
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE0018:Inline variable declaration")]
		void EncryptIterationBlock64(ulong[] key, ulong[] buffIn, ulong[] buffOut, uint iteration)
		{
			const uint index = 0;//iteration * kBlocksPerIteration;
			ulong	block0 = buffIn[index+0],	block1 = buffIn[index+1],	block2 = buffIn[index+2],	block3 = buffIn[index+3],
					block4 = buffIn[index+4],	block5 = buffIn[index+5],	block6 = buffIn[index+6],	block7 = buffIn[index+7];

			ulong q = GetIterationMod(iteration);
			ulong q0, q1, q2, q3, q4, q5, q6, q7;

			Mod0(ref q, out q0);	Mod1(ref q, out q1);
			Mod0(ref q, out q2);	Mod1(ref q, out q3);
			Mod0(ref q, out q4);	Mod1(ref q, out q5);
			Mod0(ref q, out q6);	Mod1(ref q, out q7);

			ulong	t9 = q0 ^ block0,
					t10 = q1 ^ block1,
					t11 = q2 ^ block2,
					t12 = q3 ^ block3;

			ulong o1, o2, o3, o4;
			EncryptFourBlocks(	t9, t10, t11, t12,
							out o1, out o2, out o3, out o4, key[1], key[0]);

			ulong o5, o6, o7, o8;
			EncryptFourBlocks(	o1 ^ q4 ^ block4,
							(q5 ^ block5) - (o1 ^ o4),
							o1 ^ o3 ^ o4 ^ q6 ^ block6,
							(q7 ^ block7) + (o1 ^ o2 ^ o3 ^ o4),
							out o5, out o6, out o7, out o8, key[1], key[2]);

			ulong o9, o10, o11, o12;
			EncryptFourBlocks(	(o1 ^ o2 ^ o3 ^ o4) + o5,
							o5 ^ o8 ^ o1 ^ o3 ^ o4,
							(o1 ^ o4) - (o5 ^ o7 ^ o8),
							o5 ^ o6 ^ o7 ^ o8 ^ o1,
							out o9, out o10, out o11, out o12, key[0], key[1]);

			buffOut[index+0] = o9; buffOut[index+1] = o10; buffOut[index+2] = o11; buffOut[index+3] = o12;
			buffOut[index+4] = o6 ^ o8;
			buffOut[index+5] = o5 ^ o6 ^ o7;
			buffOut[index+6] = o6 ^ o7 ^ o8;
			buffOut[index+7] = o5 ^ o6 ^ o7 ^ o8;
		}
		#endregion

		#region variable-cycle implementation
		static void DecryptInCycles(ref ulong v, ulong key0, ulong key1, uint cycleCount)
		{
			ulong lv0 = v >> 32, lv1 = v & 0xFFFFFFFF;
			uint sum = kDelta * cycleCount;
			for (int x = 0; x < cycleCount; x++)
			{
				lv0 = (lv0
					- (((uint)lv1 >> 5) ^ sum))
					- (lv1 ^ key1)
					- ((uint)lv1 << 4)
					- (key1 >> 32);

				lv1 = (lv1
					- (((uint)lv0 >> 5) ^ sum))
					- (lv0 ^ key0)
					- ((uint)lv0 << 4)
					- (key0 >> 32);

				sum -= kDelta;
			}

			v = (lv0 << 32) | (lv1 & 0xFFFFFFFF);
		}
		static void EncryptInCycles(ref ulong v, ulong key0, ulong key1, uint cycleCount)
		{
			ulong lv0 = v >> 32, lv1 = v & 0xFFFFFFFF;
			uint sum = 0;
			for (int x = 0; x < cycleCount; x++)
			{
				sum += kDelta;

				lv1 = (lv1
					+ (((uint)lv0 >> 5) ^ sum))
					+ (lv0 ^ key0)
					+ ((uint)lv0 << 4)
					+ (key0 >> 32);

				lv0 = (lv0
					+ (((uint)lv1 >> 5) ^ sum))
					+ (lv1 ^ key1)
					+ ((uint)lv1 << 4)
					+ (key1 >> 32);
			}

			v = (lv0 << 32) | (lv1 & 0xFFFFFFFF);
		}
		#endregion

		#region 8 cycles
		public static ulong DecryptWithEightCycles(ulong v0, ulong key0, ulong key1)
		{
			ulong result = v0;
			DecryptInCycles(ref result, key0, key1, 8);
			return result;
		}
		public static ulong EncryptWithEightCycles(ulong v0, ulong key0, ulong key1)
		{
			ulong result = v0;
			EncryptInCycles(ref result, key0, key1, 8);
			return result;
		}
		public static ulong TransformWithEightCycles(CryptographyTransformType transformType, ulong v0, ulong key0, ulong key1)
		{
			return transformType switch
			{
				CryptographyTransformType.Decrypt => DecryptWithEightCycles(v0, key0, key1),
				CryptographyTransformType.Encrypt => EncryptWithEightCycles(v0, key0, key1),
				_ => throw new KSoft.Debug.UnreachableException()
			};
		}
		#endregion
	};
}
