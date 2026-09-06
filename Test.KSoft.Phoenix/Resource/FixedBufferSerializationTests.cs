using System;
using System.Buffers.Binary;
using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace KSoft.Phoenix.Resource.Test;

[TestClass]
public sealed class FixedBufferSerializationTests
{
	private const int kResourceTagHeaderSize32 = 0x5F;
	private const int kSourceDigestOffset32 = 0x30;
	private const int kSourceFileSizeOffset32 = 0x44;

	private const int kCompressedDataTiger128Offset = 0x24;
	private const int kFileNameOffsetOffset = 0x34;

	[TestMethod]
	public void ResourceTagHeader_SerializeWithNullSourceDigestForWrite_ThrowsAtDigestTransfer()
	{
		byte[] expectedPrefix = SerializeResourceTagHeader(CreateResourceTagHeader())
			.AsSpan(0, kSourceDigestOffset32).ToArray();
		var header = CreateResourceTagHeader();
		header.SourceDigest = null!;
		using var stream = new MemoryStream();
		using var endianStream = new IO.EndianStream(
			stream, Shell.EndianFormat.Big, permissions: FileAccess.Write);
		endianStream.StreamMode = FileAccess.Write;

		var exception = Assert.ThrowsExactly<ArgumentNullException>(
			() => header.Serialize(endianStream));

		Assert.AreEqual(nameof(ResourceTagHeader.SourceDigest), exception.ParamName);
		Assert.AreEqual(kSourceDigestOffset32, stream.Position);
		Assert.AreEqual(kSourceDigestOffset32, stream.Length);
		CollectionAssert.AreEqual(expectedPrefix, stream.ToArray());
	}

	[TestMethod]
	public void ResourceTagHeader_SerializeWithNullSourceDigestForRead_ThrowsAtDigestTransfer()
	{
		byte[] serialized = SerializeResourceTagHeader(CreateResourceTagHeader());
		byte[] originalBytes = (byte[])serialized.Clone();
		var header = CreateResourceTagHeader();
		header.SourceDigest = null!;
		using var stream = new MemoryStream(serialized, writable: true);
		using var endianStream = new IO.EndianStream(
			stream, Shell.EndianFormat.Big, permissions: FileAccess.Read);
		endianStream.StreamMode = FileAccess.Read;

		var exception = Assert.ThrowsExactly<ArgumentNullException>(
			() => header.Serialize(endianStream));

		Assert.AreEqual(nameof(ResourceTagHeader.SourceDigest), exception.ParamName);
		Assert.AreEqual(kSourceDigestOffset32, stream.Position);
		Assert.AreEqual(kResourceTagHeaderSize32, stream.Length);
		CollectionAssert.AreEqual(originalBytes, stream.ToArray());
	}

	[TestMethod]
	public void ResourceTagHeader_Serialize_RoundTripsSourceDigestAndPreservesFollowingFieldAlignment()
	{
		byte[] expectedDigest =
		[
			0xA0, 0xA1, 0xA2, 0xA3, 0xA4,
			0xA5, 0xA6, 0xA7, 0xA8, 0xA9,
			0xAA, 0xAB, 0xAC, 0xAD, 0xAE,
			0xAF, 0xB0, 0xB1, 0xB2, 0xB3,
		];
		const ulong kExpectedSourceFileSize = 0x1122334455667788;
		const ulong kExpectedSourceFileTimeStamp = 0x8877665544332211;
		var expected = CreateResourceTagHeader();
		expected.SourceDigest = expectedDigest;
		expected.SourceFileSize = kExpectedSourceFileSize;
		expected.SourceFileTimeStamp = kExpectedSourceFileTimeStamp;

		byte[] serialized;
		using (var stream = new MemoryStream())
		{
			using var endianStream = new IO.EndianStream(
				stream, Shell.EndianFormat.Big, permissions: FileAccess.Write);
			endianStream.StreamMode = FileAccess.Write;

			expected.Serialize(endianStream);

			Assert.AreEqual(kResourceTagHeaderSize32, stream.Position);
			serialized = stream.ToArray();
		}

		Assert.AreEqual(kResourceTagHeaderSize32, serialized.Length);
		CollectionAssert.AreEqual(expectedDigest,
			serialized.AsSpan(kSourceDigestOffset32, expectedDigest.Length).ToArray());
		Assert.AreEqual(kExpectedSourceFileSize,
			BinaryPrimitives.ReadUInt64LittleEndian(
				serialized.AsSpan(kSourceFileSizeOffset32, sizeof(ulong))));

		var actual = CreateResourceTagHeader();
		byte[] originalDigestBuffer = actual.SourceDigest;
		Array.Fill(originalDigestBuffer, (byte)0xCC);
		using (var stream = new MemoryStream(serialized))
		using (var endianStream = new IO.EndianStream(
			stream, Shell.EndianFormat.Big, permissions: FileAccess.Read))
		{
			endianStream.StreamMode = FileAccess.Read;

			actual.Serialize(endianStream);

			Assert.AreEqual(serialized.Length, stream.Position);
		}

		Assert.AreSame(originalDigestBuffer, actual.SourceDigest);
		CollectionAssert.AreEqual(expectedDigest, actual.SourceDigest);
		Assert.AreEqual(kExpectedSourceFileSize, actual.SourceFileSize);
		Assert.AreEqual(kExpectedSourceFileTimeStamp, actual.SourceFileTimeStamp);
	}

	[TestMethod]
	public void EraFileEntryChunk_Serialize_RoundTripsCompressedDataTiger128AndPreservesFileNameOffsetAlignment()
	{
		byte[] expectedHash =
		[
			0x00, 0x01, 0x02, 0x03, 0x04, 0x05, 0x06, 0x07,
			0x08, 0x09, 0x0A, 0x0B, 0x0C, 0x0D, 0x0E, 0x0F,
		];
		byte[] expectedSerializedHash =
		[
			0x07, 0x06, 0x05, 0x04, 0x03, 0x02, 0x01, 0x00,
			0x0F, 0x0E, 0x0D, 0x0C, 0x0B, 0x0A, 0x09, 0x08,
		];
		const uint kExpectedFileNameOffset = 0x00A1B2C3;
		var expected = new EraFileEntryChunk
		{
			CompressedDataTiger128 = expectedHash,
			FileNameOffset = kExpectedFileNameOffset,
		};

		byte[] serialized;
		using (var stream = new MemoryStream())
		{
			using var endianStream = new IO.EndianStream(
				stream, Shell.EndianFormat.Big, permissions: FileAccess.Write);
			endianStream.StreamMode = FileAccess.Write;
			endianStream.VirtualAddressTranslationInitialize(Shell.ProcessorSize.x32);

			expected.Serialize(endianStream);

			Assert.AreEqual(EraFileEntryChunk.kSizeOf, stream.Position);
			serialized = stream.ToArray();
		}

		Assert.AreEqual(EraFileEntryChunk.kSizeOf, serialized.Length);
		CollectionAssert.AreEqual(expectedHash, expected.CompressedDataTiger128);
		CollectionAssert.AreEqual(expectedSerializedHash,
			serialized.AsSpan(kCompressedDataTiger128Offset, expectedHash.Length).ToArray());
		CollectionAssert.AreEqual(new byte[] { 0xA1, 0xB2, 0xC3 },
			serialized.AsSpan(kFileNameOffsetOffset, 3).ToArray());

		var actual = new EraFileEntryChunk();
		byte[] originalHashBuffer = actual.CompressedDataTiger128;
		Array.Fill(originalHashBuffer, (byte)0xCC);
		using (var stream = new MemoryStream(serialized))
		using (var endianStream = new IO.EndianStream(
			stream, Shell.EndianFormat.Big, permissions: FileAccess.Read))
		{
			endianStream.StreamMode = FileAccess.Read;
			endianStream.VirtualAddressTranslationInitialize(Shell.ProcessorSize.x32);

			actual.Serialize(endianStream);

			Assert.AreEqual(serialized.Length, stream.Position);
		}

		Assert.AreSame(originalHashBuffer, actual.CompressedDataTiger128);
		CollectionAssert.AreEqual(expectedHash, actual.CompressedDataTiger128);
		Assert.AreEqual(kExpectedFileNameOffset, actual.FileNameOffset);
	}

	private static byte[] SerializeResourceTagHeader(ResourceTagHeader header)
	{
		using var stream = new MemoryStream();
		using var endianStream = new IO.EndianStream(
			stream, Shell.EndianFormat.Big, permissions: FileAccess.Write);
		endianStream.StreamMode = FileAccess.Write;

		header.Serialize(endianStream);

		return stream.ToArray();
	}

	private static ResourceTagHeader CreateResourceTagHeader()
	{
		var header = new ResourceTagHeader(Shell.ProcessorSize.x32);
		// The existing constructor initializes its pointer fields but not this private-set property.
		// Set it here so the fixture remains focused on the fixed-buffer serialization contract.
		var pointerSizeSetter = typeof(ResourceTagHeader)
			.GetProperty(nameof(ResourceTagHeader.CreatorPointerSize))!
			.GetSetMethod(nonPublic: true)!;
		pointerSizeSetter.Invoke(header, [Shell.ProcessorSize.x32]);
		return header;
	}
}
