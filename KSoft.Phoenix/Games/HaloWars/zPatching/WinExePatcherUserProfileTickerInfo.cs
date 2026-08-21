using System;
using System.Collections.Generic;

namespace KSoft.Phoenix.Games.HaloWars.zPatching;

/*
When mod support was added for the game supporting more, BUserProfile::setupTickerInfo
was fixed to handle the user profile containing leaders which the current BDatabase
doesn't have. I.e., someone plays a mod which adds more leaders, which gets reflected
in their profile's stats tracking, then removes that mod, and the game won't crash.

HOWEVER, this was not done for maps and modes. If more maps or modes are added via a,
mod, the user plays them, then removes the mod, the game will crash after logging in
due to trying to ask the BDatabase for the map/mode's (display) name string for the
purposes of displaying in the pregame UI ticker. E.g.
"Private skirmish games on map %1!s!: %2!d! won  %3!d! played"

This patch just nops out the call to BUserProfile::setupTickerInfo to avoid potential crashes.
 */
public sealed class WinExePatcherUserProfileTickerInfo
{
	const int kCallAsmBytesSize = sizeof(byte) + sizeof(uint);

	public readonly short[] BytePattern;
	public readonly int BytePatternCallUserProfileSetupTickerInfoOffset;

	public readonly List<int> PatternFileOffsets = new();

	public int ModCallUserProfileSetupTickerInfoFileOffset;

	public WinExePatcherUserProfileTickerInfo()
	{
		BytePattern = [
			/*
				call    sub

				mov     rcx, [rbx+198h]
				test    rcx, rcx
				jz      short loc_XXXXX
				call    sub ; BUserProfile::setupTickerInfo
				mov     rcx, [rbx+198h]
				add     rcx, 70h

				call    sub
			*/
			0x48, 0x8B, 0x8B, 0x98, 0x01, 0x00, 0x00,
			0x48, 0x85, 0xC9,
			0x74, 0x15,
			0xE8, /*0x90*/-1, /*0x0E*/-1, /*0x4E*/-1, /*0x00*/-1,
			0x48, 0x8B, 0x8B, 0x98, 0x01, 0x00, 0x00,
			0x48, 0x83, 0xC1, 0x70,
		];
		BytePatternCallUserProfileSetupTickerInfoOffset = BytePattern.Length;
		BytePatternCallUserProfileSetupTickerInfoOffset -= 4;
		BytePatternCallUserProfileSetupTickerInfoOffset -= 3 + sizeof(uint);
		BytePatternCallUserProfileSetupTickerInfoOffset -= kCallAsmBytesSize;
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

	public bool BuildPatchData(ReadOnlySpan<byte> sourceExeBytes)
	{
		ModCallUserProfileSetupTickerInfoFileOffset = TypeExtensions.kNone;

		int file_offset = PatternFileOffsets[0];

		int callFuncFileOffset = file_offset + BytePatternCallUserProfileSetupTickerInfoOffset;
		const byte cExpectedCallOpcode = 0xE8; // call...
		if (sourceExeBytes[callFuncFileOffset] != cExpectedCallOpcode) // call...
		{
			Debug.Trace.Phoenix.TraceDataSansId(System.Diagnostics.TraceEventType.Warning,
				"Failed to find the expected call instruction at file offset 0x{0:X8}, got 0x{1:X2} instead of 0x{2:X2}"
				.FormatWith(Util.InvariantCultureInfo,
					callFuncFileOffset,
					sourceExeBytes[callFuncFileOffset],
					cExpectedCallOpcode));
			return false;
		}

		ModCallUserProfileSetupTickerInfoFileOffset = callFuncFileOffset;
		return true;
	}

	public void ApplyPatches(Span<byte> dstExeBytes)
	{
		for (int x = 0; x < kCallAsmBytesSize; x++)
		{
			// nop out the call to BUserProfile::setupTickerInfo
			dstExeBytes[ModCallUserProfileSetupTickerInfoFileOffset+x] = 0x90;
		}
	}
};
