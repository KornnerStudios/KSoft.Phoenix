using System;
using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace KSoft.Phoenix.Xmb.Test;

[TestClass]
public sealed class BinaryDataTreeVariantTypeTests
{
	[TestMethod]
	public void ReadArray_AllSupportedDescriptors_PopulatesDestinationAndReturnsSameArray()
	{
		foreach (var testCase in GetSupportedArrayCases())
		{
			using var stream = new MemoryStream(testCase.ExpectedBigEndianBytes, writable: false);
			using var reader = new IO.EndianReader(stream, Shell.EndianFormat.Big);
			Array destination = testCase.Descriptor.MakeArray(testCase.Source.Length)!;
			FillWithSentinel(destination);

			Array? result = testCase.Descriptor.ReadArray(reader, destination);

			Assert.AreSame(destination, result, testCase.Name);
			Assert.AreEqual(testCase.Source.GetType(), destination.GetType(), testCase.Name);
			CollectionAssert.AreEqual(GetRawBytes(testCase.Source), GetRawBytes(destination), testCase.Name);
			Assert.AreEqual((long)testCase.ExpectedBigEndianBytes.Length, stream.Position, testCase.Name);
			Assert.AreEqual((long)testCase.ExpectedBigEndianBytes.Length, stream.Length, testCase.Name);
		}
	}

	[TestMethod]
	public void WriteArray_AllSupportedDescriptors_WritesBigEndianBytesAndPreservesSourceIdentity()
	{
		foreach (var testCase in GetSupportedArrayCases())
		{
			byte[] sourceBytes = GetRawBytes(testCase.Source);
			using var stream = new MemoryStream();
			using var writer = new IO.EndianWriter(stream, Shell.EndianFormat.Big);

			Array? result = testCase.Descriptor.WriteArray(writer, testCase.Source);

			Assert.AreSame(testCase.Source, result, testCase.Name);
			CollectionAssert.AreEqual(sourceBytes, GetRawBytes(testCase.Source), testCase.Name);
			CollectionAssert.AreEqual(testCase.ExpectedBigEndianBytes, stream.ToArray(), testCase.Name);
			Assert.AreEqual((long)testCase.ExpectedBigEndianBytes.Length, stream.Position, testCase.Name);
			Assert.AreEqual((long)testCase.ExpectedBigEndianBytes.Length, stream.Length, testCase.Name);
		}
	}

	[TestMethod]
	public void ReadAndWriteArray_SupportedDescriptorsWithNullArray_ThrowBeforeIo()
	{
		foreach (var testCase in GetSupportedArrayCases())
		{
			using var readStream = new MemoryStream(testCase.ExpectedBigEndianBytes, writable: false);
			using var reader = new IO.EndianReader(readStream, Shell.EndianFormat.Big);

			var readException = Assert.ThrowsExactly<ArgumentNullException>(
				() => testCase.Descriptor.ReadArray(reader, null!),
				testCase.Name);

			Assert.AreEqual("array", readException.ParamName, testCase.Name);
			Assert.AreEqual(0L, readStream.Position, testCase.Name);
			Assert.AreEqual((long)testCase.ExpectedBigEndianBytes.Length, readStream.Length, testCase.Name);

			using var writeStream = new MemoryStream();
			using var writer = new IO.EndianWriter(writeStream, Shell.EndianFormat.Big);

			var writeException = Assert.ThrowsExactly<ArgumentNullException>(
				() => testCase.Descriptor.WriteArray(writer, null!),
				testCase.Name);

			Assert.AreEqual("array", writeException.ParamName, testCase.Name);
			Assert.AreEqual(0L, writeStream.Position, testCase.Name);
			Assert.AreEqual(0L, writeStream.Length, testCase.Name);
		}
	}

	[TestMethod]
	public void ReadAndWriteArray_NullDescriptor_ReturnNullWithoutIo()
	{
		var descriptor = BinaryDataTreeVariantTypeDesc.Null;
		using var readStream = new MemoryStream(new byte[] { 0xA5 }, writable: false);
		using var reader = new IO.EndianReader(readStream, Shell.EndianFormat.Big);

		Assert.IsNull(descriptor.ReadArray(reader, null!));
		Assert.IsNull(descriptor.ReadArray(reader, new object[1]));
		Assert.AreEqual(0L, readStream.Position);
		Assert.AreEqual(1L, readStream.Length);

		using var writeStream = new MemoryStream();
		using var writer = new IO.EndianWriter(writeStream, Shell.EndianFormat.Big);

		Assert.IsNull(descriptor.WriteArray(writer, null!));
		Assert.IsNull(descriptor.WriteArray(writer, new object[1]));
		Assert.AreEqual(0L, writeStream.Position);
		Assert.AreEqual(0L, writeStream.Length);
	}

	private static FixedArrayCase[] GetSupportedArrayCases() =>
	[
		new(
			"Bool",
			BinaryDataTreeVariantTypeDesc.Bool,
			new bool[] { false, true },
			[0x00, 0x01]),
		new(
			"UInt8",
			BinaryDataTreeVariantTypeDesc.UInt8,
			new byte[] { 0x00, 0x80, 0xFF },
			[0x00, 0x80, 0xFF]),
		new(
			"Int8",
			BinaryDataTreeVariantTypeDesc.Int8,
			new sbyte[] { sbyte.MaxValue, sbyte.MinValue, -1 },
			[0x7F, 0x80, 0xFF]),
		new(
			"UInt16",
			BinaryDataTreeVariantTypeDesc.UInt16,
			new ushort[] { 0x0000, 0x8000, 0xFFFF },
			[
				0x00, 0x00,
				0x80, 0x00,
				0xFF, 0xFF,
			]),
		new(
			"Int16",
			BinaryDataTreeVariantTypeDesc.Int16,
			new short[] { short.MaxValue, short.MinValue, -1 },
			[
				0x7F, 0xFF,
				0x80, 0x00,
				0xFF, 0xFF,
			]),
		new(
			"UInt32",
			BinaryDataTreeVariantTypeDesc.UInt32,
			new uint[] { 0x00000000, 0x80000000, 0xFFFFFFFF },
			[
				0x00, 0x00, 0x00, 0x00,
				0x80, 0x00, 0x00, 0x00,
				0xFF, 0xFF, 0xFF, 0xFF,
			]),
		new(
			"Int32",
			BinaryDataTreeVariantTypeDesc.Int32,
			new int[] { int.MaxValue, int.MinValue, -1 },
			[
				0x7F, 0xFF, 0xFF, 0xFF,
				0x80, 0x00, 0x00, 0x00,
				0xFF, 0xFF, 0xFF, 0xFF,
			]),
		new(
			"UInt64",
			BinaryDataTreeVariantTypeDesc.UInt64,
			new ulong[] { 0x0000000000000000, 0x8000000000000000, 0xFFFFFFFFFFFFFFFF },
			[
				0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
				0x80, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
				0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF,
			]),
		new(
			"Int64",
			BinaryDataTreeVariantTypeDesc.Int64,
			new long[] { long.MaxValue, long.MinValue, -1 },
			[
				0x7F, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF,
				0x80, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
				0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF,
			]),
		new(
			"Single",
			BinaryDataTreeVariantTypeDesc.Single,
			new float[]
			{
				BitConverter.Int32BitsToSingle(unchecked((int)0x80000000)),
				BitConverter.Int32BitsToSingle(0x7F7FFFFF),
				BitConverter.Int32BitsToSingle(unchecked((int)0xFFC12345)),
			},
			[
				0x80, 0x00, 0x00, 0x00,
				0x7F, 0x7F, 0xFF, 0xFF,
				0xFF, 0xC1, 0x23, 0x45,
			]),
		new(
			"Double",
			BinaryDataTreeVariantTypeDesc.Double,
			new double[]
			{
				BitConverter.Int64BitsToDouble(unchecked((long)0x8000000000000000)),
				BitConverter.Int64BitsToDouble(0x7FEFFFFFFFFFFFFF),
				BitConverter.Int64BitsToDouble(unchecked((long)0xFFF8123456789ABC)),
			},
			[
				0x80, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
				0x7F, 0xEF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF,
				0xFF, 0xF8, 0x12, 0x34, 0x56, 0x78, 0x9A, 0xBC,
			]),
	];

	private static byte[] GetRawBytes(Array array)
	{
		var bytes = new byte[Buffer.ByteLength(array)];
		Buffer.BlockCopy(array, 0, bytes, 0, bytes.Length);
		return bytes;
	}

	private static void FillWithSentinel(Array array)
	{
		var bytes = new byte[Buffer.ByteLength(array)];
		Array.Fill(bytes, (byte)0xCC);
		Buffer.BlockCopy(bytes, 0, array, 0, bytes.Length);
	}

	private sealed record FixedArrayCase(
		string Name,
		BinaryDataTreeVariantTypeDesc Descriptor,
		Array Source,
		byte[] ExpectedBigEndianBytes);
}
