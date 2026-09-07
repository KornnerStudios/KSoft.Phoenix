using System;
using System.Collections.Generic;

namespace KSoft.Phoenix.Games.HaloWars.zPatching;

public sealed class WinExePatcherParticleGateway
{
	const int kJmpAsmBytesSize = sizeof(byte) + sizeof(uint);

	public readonly byte[] NewAsmBytesPattern;

	public readonly short[] BytePattern;
	// BytePattern offset where the jmp instruction is stored
	public readonly int BytePatternJmpToAssertOffset;

	public readonly List<int> PatternFileOffsets = new();

	public int ModNewAsmBytesFileOffset;

	public WinExePatcherParticleGateway()
	{
		// the part of BParticleGateway::getOrCreateData(const char*, int&)
		// where it will assert when mNumDataSlotsInUse == cMaxDataSlots.
		// We instead want the assert logic to return in the same way as when NoParticles is defined in user.cfg.
		// r8 = the out parameter reference to set to -1
		BytePattern = [
			/*
			loc_1407A793A:
				inc     ebx
				cmp     ebx, ecx
				jb      short loc_1407A7900
			loc_1407A7940:
				cmp     ecx, 200h
				jnz     short loc_1407A7958
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
		BytePatternJmpToAssertOffset = BytePattern.Length - kJmpAsmBytesSize;

		// We want to replace the asm bytes related to the assert with code that just returns, and sets the out parameter to -1 (0xFFFFFFFF).
		// r8 gets moved into rsi near the start, so we just use rsi here, before restoring the original rsi value.
		/*
			mov     dword ptr [rsi], 0FFFFFFFFh
			mov     rbx, [rsp+50h]
			mov     rbp, [rsp+58h]
			mov     rsi, [rsp+60h]
			add     rsp, 40h
			pop     rdi
			retn
		*/
		NewAsmBytesPattern = [
			0xC7, 0x06, 0xFF, 0xFF, 0xFF, 0xFF,
			0x48, 0x8B, 0x5C, 0x24, 0x50,
			0x48, 0x8B, 0x6C, 0x24, 0x58,
			0x48, 0x8B, 0x74, 0x24, 0x60,
			0x48, 0x83, 0xC4, 0x40,
			0x5F,
			0xC3,
		];
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
		ModNewAsmBytesFileOffset = TypeExtensions.kNone;

		// loc_1407A793A
		int file_offset = PatternFileOffsets[0];

		int jmpToAssertFileOffset = file_offset + BytePatternJmpToAssertOffset;
		// loc_1407A7948 has a jmp instruction we want to modify, and it should be at the expected offset from the pattern start
		const byte cExpectedJmpOpcode = 0xE9; // jmp...
		if (sourceExeBytes[jmpToAssertFileOffset] != cExpectedJmpOpcode) // jmp...
		{
			Debug.Trace.Phoenix.TraceDataSansId(System.Diagnostics.TraceEventType.Warning,
				"Failed to find the expected jmp instruction at file offset 0x{0:X8}, got 0x{1:X2} instead of 0x{2:X2}"
				.FormatWith(Util.InvariantCultureInfo,
					jmpToAssertFileOffset,
					sourceExeBytes[jmpToAssertFileOffset],
					cExpectedJmpOpcode));
			return false;
		}

		int jmpToAssertOffset = BitConverter.ToInt32(sourceExeBytes.Slice(jmpToAssertFileOffset + sizeof(byte), sizeof(uint)));
		// relative address starts after the jmp opcode and its relative address value
		int assertionAsmOffset = sizeof(byte) + sizeof(uint);
		assertionAsmOffset += jmpToAssertOffset;
		assertionAsmOffset += jmpToAssertFileOffset;

		// loc_1407A79F6
		const byte cExpectedAssertOpcode = 0x48; // lea...
		if (sourceExeBytes[assertionAsmOffset] != cExpectedAssertOpcode) // lea...
		{
			Debug.Trace.Phoenix.TraceDataSansId(System.Diagnostics.TraceEventType.Warning,
				"Failed to find the expected assert instruction at file offset 0x{0:X8}, got 0x{1:X2} instead of 0x{2:X2}"
				.FormatWith(Util.InvariantCultureInfo,
					assertionAsmOffset,
					sourceExeBytes[assertionAsmOffset],
					cExpectedAssertOpcode));
			return false;
		}

		ModNewAsmBytesFileOffset = assertionAsmOffset;
		return true;
	}

	public void ApplyModJmp(Span<byte> dstExeBytes)
	{
		// loc_1407A79F6, replace assertion asm with NoParticles-like behavior
		NewAsmBytesPattern.AsSpan().CopyTo(dstExeBytes[ModNewAsmBytesFileOffset..]);
	}
};
