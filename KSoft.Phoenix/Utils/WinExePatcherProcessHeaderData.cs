using System;
using System.Collections.Generic;

namespace KSoft.Phoenix.zPatching
{
	// Patches? We don't need no stinkin- owait.
	// I hacked this together. I hope I never have to touch this again.

	public sealed class WinExePatcherProcessHeaderData
	{
		public short[] BytePattern;
		public int BytePatternNextJmpOffset;
		public int BytePatternModJmpOffset;

		public List<int> PatternFileOffsets = new();

		public int ModJmpFileOffset;
		public int ModJmpVa;

		public WinExePatcherProcessHeaderData()
		{
			// the part of BAsyncECFArchiveLoader::processHeaderData, after it attempts to find
			// a matching digital signature (see EraFileSignature) against hard coded public keys
			// r14 = 'this' pointer
			// ebx = public key index, compared against the number of public keys.
			//		it will be equal to the number of public keys when no match is found.
			BytePattern = [
				/*
					call    sub
					nop
					mov     edi, ebx
					cmp     ebx, [r14+20h]
					jnb     i32
				*/
				0xE8, /*0xD7*/-1, /*0xE5*/-1, 0xFF, 0xFF,
				0x90,
				/*0x8B*/-1, 0xFB,
				0x41, 0x3B, 0x5E, 0x20,
				0x0F, 0x83,   -1, 0x00, 0x00, 0x00,

				// alignment asm bytes, can't rely on these :(
				//0x66, 0x66,
				//0x0F, 0x1F, 0x84, 0x00, 0x00, 0x00, 0x00, 0x00,
			];
			BytePatternNextJmpOffset = BytePattern.Length - sizeof(uint);
			BytePatternModJmpOffset = BytePattern.Length - sizeof(uint) - sizeof(ushort);
		}

		public bool FindPatterns(ReadOnlySpan<byte> sourceExeBytes)
		{
			PatternFileOffsets.Clear();

			int offset = 0;
			while (PhxUtil.FindBytePattern(PatternFileOffsets, sourceExeBytes, ref offset, BytePattern))
			{
			}

			return PatternFileOffsets.Count == 1;
		}

		public bool CalculateModJmp(ReadOnlySpan<byte> sourceExeBytes)
		{
			ModJmpFileOffset = ModJmpVa = TypeExtensions.kNone;

			int file_offset = PatternFileOffsets[0];
			int next_jmp_index = file_offset + BytePatternNextJmpOffset;
			int next_jmp_offset_va = BitConverter.ToInt32(sourceExeBytes.Slice(next_jmp_index));
			int good_jmp_base = next_jmp_offset_va;
			good_jmp_base += sizeof(uint);
			good_jmp_base += next_jmp_index;

			// jnz short i8
			if (0x75 != sourceExeBytes[good_jmp_base])
			{
				Debug.Trace.Phoenix.TraceDataSansId(System.Diagnostics.TraceEventType.Warning,
					"Failed to find the expected jnz instruction at file offset 0x{0:X8}, got 0x{1:X2} instead of 0x{2:X2}"
					.FormatWith(Util.InvariantCultureInfo,
						good_jmp_base,
						sourceExeBytes[good_jmp_base],
						0x75));
				return false;
			}

			int good_asm_offset_va = sourceExeBytes[good_jmp_base + 1];
			int good_asm_index = (good_jmp_base + 2);
			good_asm_index += good_asm_offset_va;

			// 0xE9 .. .. .. ..
			int mod_jmp_base = file_offset + BytePatternModJmpOffset;
			mod_jmp_base += sizeof(byte) + sizeof(uint);
			int mod_jmp_offset_va = good_asm_index;
			mod_jmp_offset_va -= mod_jmp_base;

			ModJmpFileOffset = file_offset + BytePatternModJmpOffset;
			ModJmpVa = mod_jmp_offset_va;

			return true;
		}

		public void ApplyModJmp(byte[] dstExeBytes)
		{
			int index = ModJmpFileOffset;
			dstExeBytes[index++] = 0xE9;
			Bitwise.ByteSwap.ReplaceBytes(dstExeBytes, index, ModJmpVa);
		}
	};
}
