using System;
using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace KSoft.Phoenix.Test;

[TestClass]
public sealed class PascalStringSerializationTests
{
	[TestMethod]
	[DataRow(false, "", "00000000")]
	[DataRow(false, "AB", "000000024142")]
	[DataRow(true, "", "00000000")]
	[DataRow(true, "A\u00E9", "00000002004100E9")]
	[DataRow(true, "\U0001F642", "00000002D83DDE42")]
	public void PascalHelpers_PreserveBigEndianCharacterCountsAndFollowingField(bool wide, string text, string expectedHex)
	{
		byte[] expected = Convert.FromHexString(expectedHex);
		using var buffer = new MemoryStream();
		using var stream = new IO.EndianStream(buffer, Shell.EndianFormat.Little);
		stream.StreamMode = FileAccess.Write;
		string written = text;

		if (wide)
			stream.StreamPascalWideString32(ref written);
		else
			stream.StreamPascalString32(ref written);

		CollectionAssert.AreEqual(expected, buffer.ToArray());
		stream.Writer.Write((byte)0xCC);
		buffer.Position = 0;
		stream.StreamMode = FileAccess.Read;
		string read = string.Empty;

		if (wide)
			stream.StreamPascalWideString32(ref read);
		else
			stream.StreamPascalString32(ref read);

		Assert.AreEqual(text, read);
		Assert.AreEqual((long)expected.Length, buffer.Position);
		Assert.AreEqual((byte)0xCC, stream.Reader.ReadByte());
	}
}
