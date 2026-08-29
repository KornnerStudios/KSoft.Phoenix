using System;
using System.IO;
using KSoft.Wwise.SoundBank;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace KSoft.Wwise.Test;

[TestClass]
public sealed class AkSoundBankTest
{
	private static readonly byte[] kModernBankHeader =
	[
		(byte)'B', (byte)'K', (byte)'H', (byte)'D',
		0x00, 0x00, 0x00, 0x10,
		0x00, 0x00, 0x00, 0x2D,
		0x00, 0x00, 0x00, 0x01,
		0x00, 0x00, 0x00, 0x00,
		0x00, 0x00, 0x00, 0x00,
	];

	[TestMethod]
	public void Serialize_StandaloneModernBankWithSdk_ParsesHeader()
	{
		var soundBank = AkSoundBank.CreateStandalone(kModernBankHeader.Length,
			sdkVersion: AkVersion.k2009.Id);

		using var stream = new MemoryStream(kModernBankHeader);
		using var endianStream = new IO.EndianStream(
			stream, Shell.EndianFormat.Big, permissions: FileAccess.Read);
		endianStream.StreamMode = FileAccess.Read;

		soundBank.Serialize(endianStream);

		Assert.AreEqual(0x2Du, soundBank.GeneratorVersion);
		Assert.AreEqual(1u, soundBank.Id);
		Assert.AreEqual(0u, soundBank.LanguageId);
		Assert.IsFalse(soundBank.HasFeedback);
	}

	[TestMethod]
	public void Serialize_StandaloneBankWithoutSdk_ThrowsDeterministicException()
	{
		var soundBank = new AkSoundBank(kModernBankHeader.Length);

		using var stream = new MemoryStream(kModernBankHeader);
		using var endianStream = new IO.EndianStream(
			stream, Shell.EndianFormat.Big, permissions: FileAccess.Read);
		endianStream.StreamMode = FileAccess.Read;

		var exception = Assert.ThrowsExactly<InvalidOperationException>(
			() => soundBank.Serialize(endianStream));

		Assert.AreEqual("Standalone sound bank parsing requires an SDK version.",
			exception.Message);
	}
}
