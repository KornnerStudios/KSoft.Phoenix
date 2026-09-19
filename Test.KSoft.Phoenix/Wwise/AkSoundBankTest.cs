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
	public void Serialize_ModernStringMapping_ReadsInt8AsciiNamesWithoutConsumingNextField()
	{
		// Synthetic STID payload: type/count header and two ID + Int8-counted ASCII records.
		byte[] bytes =
		[
			.. kModernBankHeader,
			(byte)'S', (byte)'T', (byte)'I', (byte)'D',
			0x00, 0x00, 0x00, 0x17,
			0x00, 0x00, 0x00, 0x01,
			0x00, 0x00, 0x00, 0x02,
			0x01, 0x02, 0x03, 0x04, 0x02, 0x41, 0x42,
			0x05, 0x06, 0x07, 0x08, 0x03, 0x58, 0x59, 0x5A,
		];
		var bank = AkSoundBank.CreateStandalone(bytes.Length, AkVersion.k2009.Id);
		using var buffer = new MemoryStream([.. bytes, 0xCC]);
		using var stream = new IO.EndianStream(buffer, Shell.EndianFormat.Big, permissions: FileAccess.Read);
		stream.StreamMode = FileAccess.Read;

		bank.Serialize(stream);

		Assert.AreEqual((long)bytes.Length, buffer.Position);
		Assert.AreEqual((byte)0xCC, stream.Reader.ReadByte());
		// Standalone banks expose no public name lookup.
		var field = typeof(AkSoundBank).GetField("mIdToName",
			System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
		Assert.IsNotNull(field);
		var names = Assert.IsInstanceOfType<System.Collections.Generic.Dictionary<uint, string?>>(field.GetValue(bank));
		Assert.HasCount(2, names);
		Assert.AreEqual("AB", names[0x01020304]);
		Assert.AreEqual("XYZ", names[0x05060708]);
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
