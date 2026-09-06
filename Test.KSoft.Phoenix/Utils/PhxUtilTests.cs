using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace KSoft.Phoenix.Test;

[TestClass]
public sealed class PhxUtilTests
{
	private static readonly byte[] kInput =
	[
		0x10, 0x21, 0x32, 0x43,
		0x54, 0x65, 0x76,
	];

	[TestMethod]
	public void SuperFastHash_EmptyInput_ReturnsGoldenHash()
	{
		uint hash = PhxUtil.SuperFastHash(Array.Empty<byte>());

		Assert.AreEqual(0x00000000U, hash);
	}

	[TestMethod]
	[DataRow(4, 0x1CED2604U, DisplayName = "Remainder 0")]
	[DataRow(5, 0xDC9FDDDFU, DisplayName = "Remainder 1")]
	[DataRow(6, 0x5B4AC04BU, DisplayName = "Remainder 2")]
	[DataRow(7, 0x9CC73231U, DisplayName = "Remainder 3")]
	public void SuperFastHash_LengthCoveringRemainder_ReturnsGoldenHash(int length, uint expectedHash)
	{
		byte[] input = kInput[..length];

		uint hash = PhxUtil.SuperFastHash(input);

		Assert.AreEqual(expectedHash, hash,
			$"Length {length}: expected 0x{expectedHash:X8}, actual 0x{hash:X8}.");
	}

	[TestMethod]
	public void SuperFastHash_FullBufferAndNonzeroOffsetSlice_AreEquivalentDeterministicAndPreserveInput()
	{
		byte[] fullBuffer = (byte[])kInput.Clone();
		byte[] fullBufferBefore = (byte[])fullBuffer.Clone();
		byte[] containingBuffer =
		[
			0xA5, 0x5A,
			.. kInput,
			0xC3,
		];
		byte[] containingBufferBefore = (byte[])containingBuffer.Clone();

		uint fullHash = PhxUtil.SuperFastHash(fullBuffer);
		uint repeatedHash = PhxUtil.SuperFastHash(fullBuffer);
		uint sliceHash = PhxUtil.SuperFastHash(containingBuffer.AsSpan(2, kInput.Length));

		Assert.AreEqual(0x9CC73231U, fullHash,
			$"Expected 0x9CC73231, actual 0x{fullHash:X8}.");
		Assert.AreEqual(fullHash, repeatedHash);
		Assert.AreEqual(fullHash, sliceHash);
		CollectionAssert.AreEqual(fullBufferBefore, fullBuffer);
		CollectionAssert.AreEqual(containingBufferBefore, containingBuffer);
	}

	[TestMethod]
	public void SuperFastHash_SeededIncrementalChaining_ReturnsGoldenHash()
	{
		uint hash = PhxUtil.SuperFastHash(new byte[] { 0x10, 0x21, 0x32, 0x43 });
		hash = PhxUtil.SuperFastHash(new byte[] { 0x54, 0x65 }, hash);
		hash = PhxUtil.SuperFastHash(new byte[] { 0x76, 0x87 }, hash);
		hash = PhxUtil.SuperFastHash(new byte[] { 0x98 }, hash);

		Assert.AreEqual(0x2C15DE65U, hash,
			$"Expected 0x2C15DE65, actual 0x{hash:X8}.");
	}
}
