using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace KSoft.Phoenix.Xmb.Test;

[TestClass]
public sealed class XmbVariantSerializationTests
{
	[TestMethod]
	public void Write_UnsignedInt24_EncodesFlagsAndRoundTrips()
	{
		var value = new XmbVariant
		{
			Type = XmbVariantType.Int,
			IsUnsigned = true,
			Int = 0x00123456,
		};

		byte[] bytes = Serialize(value);
		CollectionAssert.AreEqual(new byte[] { 0x43, 0x12, 0x34, 0x56 }, bytes);

		var (rawData, actual) = Deserialize(bytes);
		Assert.AreEqual(0x43123456u, rawData);
		Assert.AreEqual(XmbVariantSerialization.RawVariantType.Int24,
			XmbVariantSerialization.GetTypeFromRawData(rawData));
		Assert.AreEqual(XmbVariantType.Int, actual.Type);
		Assert.IsTrue(actual.IsUnsigned);
		Assert.IsFalse(actual.IsIndirect);
		Assert.AreEqual(0x00123456u, actual.Int);
	}

	[TestMethod]
	public void Write_IndirectAnsiString_EncodesFlagsAndRoundTrips()
	{
		var value = new XmbVariant
		{
			Type = XmbVariantType.String,
			IsIndirect = true,
			Offset = 0x00123456,
		};

		byte[] bytes = Serialize(value);
		CollectionAssert.AreEqual(new byte[] { 0x88, 0x12, 0x34, 0x56 }, bytes);

		var (rawData, actual) = Deserialize(bytes);
		Assert.AreEqual(0x88123456u, rawData);
		Assert.AreEqual(XmbVariantSerialization.RawVariantType.StringAnsi,
			XmbVariantSerialization.GetTypeFromRawData(rawData));
		Assert.AreEqual(XmbVariantType.String, actual.Type);
		Assert.IsTrue(actual.IsIndirect);
		Assert.IsFalse(actual.IsUnicode);
		Assert.AreEqual(0x00123456u, actual.Offset);
	}

	[TestMethod]
	public void Write_IndirectInt_EncodesOffsetTypeAndRoundTrips()
	{
		var value = new XmbVariant
		{
			Type = XmbVariantType.Int,
			IsIndirect = true,
			Offset = 0x00123456,
		};

		byte[] bytes = Serialize(value);
		CollectionAssert.AreEqual(new byte[] { 0x84, 0x12, 0x34, 0x56 }, bytes);

		var (rawData, actual) = Deserialize(bytes);
		Assert.AreEqual(0x84123456u, rawData);
		Assert.AreEqual(XmbVariantSerialization.RawVariantType.Int,
			XmbVariantSerialization.GetTypeFromRawData(rawData));
		Assert.AreEqual(XmbVariantType.Int, actual.Type);
		Assert.IsFalse(actual.IsUnsigned);
		Assert.IsTrue(actual.IsIndirect);
		Assert.AreEqual(0x00123456u, actual.Offset);
	}

	[TestMethod]
	public void Write_IndirectSingle_EncodesOffsetTypeAndRoundTrips()
	{
		var value = new XmbVariant
		{
			Type = XmbVariantType.Single,
			IsIndirect = true,
			Offset = 0x00123456,
		};

		byte[] bytes = Serialize(value);
		CollectionAssert.AreEqual(new byte[] { 0x82, 0x12, 0x34, 0x56 }, bytes);

		var (rawData, actual) = Deserialize(bytes);
		Assert.AreEqual(0x82123456u, rawData);
		Assert.AreEqual(XmbVariantSerialization.RawVariantType.Single,
			XmbVariantSerialization.GetTypeFromRawData(rawData));
		Assert.AreEqual(XmbVariantType.Single, actual.Type);
		Assert.IsTrue(actual.IsIndirect);
		Assert.AreEqual(0x00123456u, actual.Offset);
	}

	private static byte[] Serialize(XmbVariant value)
	{
		using var stream = new MemoryStream();
		using (var writer = new IO.EndianWriter(stream, Shell.EndianFormat.Big))
		{
			XmbVariantSerialization.Write(writer, value);
		}

		return stream.ToArray();
	}

	private static (uint RawData, XmbVariant Variant) Deserialize(byte[] bytes)
	{
		using var stream = new MemoryStream(bytes);
		using var reader = new IO.EndianReader(stream, Shell.EndianFormat.Big);

		uint rawData = XmbVariantSerialization.Read(reader, out XmbVariant variant);
		return (rawData, variant);
	}
}
