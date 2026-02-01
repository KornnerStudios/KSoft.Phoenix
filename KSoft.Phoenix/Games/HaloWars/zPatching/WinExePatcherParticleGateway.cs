using System;
using System.Collections.Generic;
#if CONTRACTS_FULL_SHIM
using Contract = System.Diagnostics.ContractsShim.Contract;
#else
using Contract = System.Diagnostics.Contracts.Contract; // SHIM'D
#endif

namespace KSoft.Phoenix.Games.HaloWars.zPatching;

public sealed class WinExePatcherParticleGateway
{
	public readonly byte[] TargetAsmBytesPattern;

	public readonly short[] BytePattern;
	// BytePattern offset where the jmp relative address is stored
	public readonly int BytePatternNextJmpOffset;
	// BytePattern offset where the jmp instruction is stored
	public readonly int BytePatternModJmpOffset;

	public readonly List<int> PatternFileOffsets = new();

	// Relative to the resolved file pattern, the target asm bytes are here
	public readonly int TargetJmpRelativeOffset = -107;
	public int ModJmpFileOffset;
	public int ModJmpVa = -121;

	public WinExePatcherParticleGateway()
	{
		// the part of BParticleGateway::getOrCreateData(const char*, int&)
		// where it will assert when mNumDataSlotsInUse == cMaxDataSlots.
		// We instead want the assert logic to return in the same way as when NoParticles is defined in user.cfg.
		// r8 = the out parameter reference to set to -1
		/*
		loc_1407A78CF:
			mov     dword ptr [r8], 0FFFFFFFFh
			mov     rsi, [rsp+60h]
			add     rsp, 40h
			pop     rdi
			retn
		*/
		TargetAsmBytesPattern = [
			0x41, 0xC7, 0x00, 0xFF, 0xFF, 0xFF, 0xFF,
			0x48, 0x8B, 0x74, 0x24, 0x60,
			0x48, 0x83, 0xC4, 0x40,
			0x5F,
			0xC3,
		];

		BytePattern = [
			/*
			loc_1407A793A:
				inc     ebx
				cmp     ebx, ecx
				jb      short loc_1407A7900
			loc_1407A7940:
				cmp     ecx, 200h
				jnz     short loc_1407A7958
			; we want to change this jmp to loc_1407A78CF
			loc_1407A7948:
				jmp     loc_1407A79F6
			*/
			0xFF, 0xC3,
			0x3B, 0xD9,
			0x72, 0xC0,
			0x81, 0xF9, 0x00, 0x02, 0x00, 0x00,
			0x75, 0x10,
			0xE9, 0xA9, 0x00, 0x00, 0x00,
		];
		BytePatternNextJmpOffset = BytePattern.Length - sizeof(uint);
		BytePatternModJmpOffset = BytePatternNextJmpOffset - sizeof(byte);
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

		// loc_1407A793A
		int file_offset = PatternFileOffsets[0];
		// loc_1407A78CF
		int targetAsmBytesFileOffset = file_offset + TargetJmpRelativeOffset;

		ReadOnlySpan<byte> actualTargetAsmBytes = sourceExeBytes.Slice(targetAsmBytesFileOffset, TargetAsmBytesPattern.Length);
		if (!actualTargetAsmBytes.SequenceEqual(TargetAsmBytesPattern))
		{
			return false;
		}

		ModJmpFileOffset = file_offset + BytePatternModJmpOffset;
		Contract.Assert(sourceExeBytes[ModJmpFileOffset] == 0xE9);

		ModJmpVa = ModJmpFileOffset - targetAsmBytesFileOffset;
		// negate, as we need to jump to an earlier address
		ModJmpVa = -ModJmpVa;
		Contract.Assert(ModJmpVa == -121);

		return true;
	}

	public void ApplyModJmp(byte[] dstExeBytes)
	{
		// loc_1407A793A
		int fileOffset = PatternFileOffsets[0];
		int jmpFileOffset = fileOffset + BytePatternNextJmpOffset;

		// ModJmpFileOffset already equals 0xE9
		Bitwise.ByteSwap.ReplaceBytes(dstExeBytes, jmpFileOffset, ModJmpVa);
	}
};
