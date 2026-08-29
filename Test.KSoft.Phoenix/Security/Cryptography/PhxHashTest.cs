using System;
using System.IO;
using System.Security.Cryptography;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace KSoft.Security.Cryptography.Test
{
	[TestClass]
	public sealed class PhxHashTest
		: KSoft.Phoenix.BaseTestClass
	{
		[TestMethod]
		public void PhxHash_Sha1HashTest()
		{
			const string cKeyPhrase = PhxTEA.kKeyGameFilePhrase;

			var result = new byte[PhxHash.kResultSize];
			PhxHash.Sha1Hash(cKeyPhrase, result);

			ulong[] gameFileTeaKey = PhxTEA.CreateKeyFromPhrase(cKeyPhrase);
			Assert.IsNotNull(gameFileTeaKey);
			Assert.HasCount(PhxTEA.kKeySize, gameFileTeaKey);
			CollectionAssert.AreEqual(PhxTEA.kKeyGameFile, gameFileTeaKey);
		}

		[TestMethod]
		public void PhxHash_PrimitiveHelpers_HashBigEndianBytes()
		{
			using var sha = SHA1.Create();

			PhxHash.UInt8(sha, 0x12);
			PhxHash.UInt16(sha, 0x3456);
			PhxHash.UInt32(sha, 0x789ABCDE);
			PhxHash.UInt64(sha, 0xF0123456789ABCDE, true);

			byte[] expected_input =
			[
				0x12,
				0x34, 0x56,
				0x78, 0x9A, 0xBC, 0xDE,
				0xF0, 0x12, 0x34, 0x56, 0x78, 0x9A, 0xBC, 0xDE,
			];
			CollectionAssert.AreEqual(SHA1.HashData(expected_input), sha.Hash!);
		}

		[TestMethod]
		public void PhxHash_PrimitiveHelpers_UseIndependentBuffersAcrossConcurrentCalls()
		{
			using var barrier = new Barrier(2);
			using var first = new BarrierSha1(barrier);
			using var second = new BarrierSha1(barrier);

			Task first_task = Task.Run(() => PhxHash.UInt32(first, 0x12345678));
			Task second_task = Task.Run(() => PhxHash.UInt32(second, 0x9ABCDEF0));
			Task.WaitAll(first_task, second_task);

			CollectionAssert.AreEqual(new byte[] { 0x12, 0x34, 0x56, 0x78 }, first.Input);
			CollectionAssert.AreEqual(new byte[] { 0x9A, 0xBC, 0xDE, 0xF0 }, second.Input);
		}

		[TestMethod]
		public void PhxHash_PrimitiveHelpers_PreserveInputDuringSameThreadReentrancy()
		{
			using var sha = new ReentrantSha1();

			PhxHash.UInt32(sha, 0x12345678);

			CollectionAssert.AreEqual(new byte[] { 0x12, 0x34, 0x56, 0x78 }, sha.Input);
		}

		[TestMethod]
		public void PhxHash_Stream_ReadsFragmentedInputAndRestoresPosition()
		{
			byte[] input = { 0xDE, 0xAD, 0xBE, 0xEF, 0x12, 0x34, 0x56, 0x78 };
			using var stream = new FragmentedReadStream(input, 2);
			stream.Position = 6;
			using var sha = SHA1.Create();

			PhxHash.Stream(sha, stream, 1, 5, true);

			CollectionAssert.AreEqual(SHA1.HashData(input.AsSpan(1, 5)), sha.Hash!);
			Assert.AreEqual(6L, stream.Position);
		}

		[TestMethod]
		public void PhxHash_Stream_ThrowsEndOfStreamWhenReadReturnsZeroBeforeRangeCompletes()
		{
			using var stream = new ZeroThenDataReadStream(new byte[] { 1, 2, 3, 4 });
			using var sha = SHA1.Create();

			Assert.ThrowsExactly<EndOfStreamException>(() => PhxHash.Stream(sha, stream, 0, 4));
		}

		[TestMethod]
		public void PhxHash_Stream_InvalidRanges_ThrowExpectedArgumentNames()
		{
			using var stream = new MemoryStream(new byte[] { 1, 2, 3, 4 });
			using var sha = SHA1.Create();

			var length_exception = Assert.ThrowsExactly<ArgumentOutOfRangeException>(
				() => PhxHash.Stream(sha, stream, 0, long.MaxValue));
			Assert.AreEqual("inputLength", length_exception.ParamName);

			var offset_exception = Assert.ThrowsExactly<ArgumentOutOfRangeException>(
				() => PhxHash.Stream(sha, stream, stream.Length + 1, 1));
			Assert.AreEqual("inputOffset", offset_exception.ParamName);
		}

		[TestMethod]
		public void PhxHash_Sha1HashInvalidArguments_ThrowExpectedExceptions()
		{
			var result = new byte[PhxHash.kResultSize];

			AssertThrowsArgumentNull("str", () => PhxHash.Sha1Hash(null!, result));
			AssertThrowsArgument("str", () => PhxHash.Sha1Hash(string.Empty, result));
			AssertThrowsArgumentNull("result", () => PhxHash.Sha1Hash("test", null!));
			AssertThrowsArgumentOutOfRange("result", () => PhxHash.Sha1Hash("test", new byte[PhxHash.kResultSize - 1]));

			AssertThrowsArgumentNull("fileName", () => _ = PhxHash.Sha1HashFile(null!, result, out _));
			AssertThrowsArgument("fileName", () => _ = PhxHash.Sha1HashFile(string.Empty, result, out _));
			AssertThrowsArgumentNull("result", () => _ = PhxHash.Sha1HashFile("missing.bin", null!, out _));
			AssertThrowsArgumentOutOfRange("result", () =>
				_ = PhxHash.Sha1HashFile("missing.bin", new byte[PhxHash.kResultSize - 1], out _));
		}

		static void AssertThrowsArgument(string parameterName, Action action)
		{
			var exception = Assert.ThrowsExactly<ArgumentException>(action);

			Assert.AreEqual(parameterName, exception.ParamName);
		}

		static void AssertThrowsArgumentNull(string parameterName, Action action)
		{
			var exception = Assert.ThrowsExactly<ArgumentNullException>(action);

			Assert.AreEqual(parameterName, exception.ParamName);
		}

		static void AssertThrowsArgumentOutOfRange(string parameterName, Action action)
		{
			var exception = Assert.ThrowsExactly<ArgumentOutOfRangeException>(action);

			Assert.AreEqual(parameterName, exception.ParamName);
		}

		sealed class BarrierSha1
			: SHA1
		{
			readonly Barrier mBarrier;

			public byte[] Input { get; private set; } = Array.Empty<byte>();

			public BarrierSha1(Barrier barrier)
			{
				mBarrier = barrier;
				HashSizeValue = PhxHash.kSha1SizeOf * 8;
			}

			public override void Initialize()
			{
			}

			protected override void HashCore(byte[] array, int ibStart, int cbSize)
			{
				mBarrier.SignalAndWait();
				Input = array[ibStart..(ibStart + cbSize)];
			}

			protected override byte[] HashFinal() => Array.Empty<byte>();
		}

		sealed class FragmentedReadStream
			: MemoryStream
		{
			readonly int mMaximumReadSize;

			public FragmentedReadStream(byte[] buffer, int maximumReadSize)
				: base(buffer, writable: false)
			{
				mMaximumReadSize = maximumReadSize;
			}

			public override int Read(byte[] buffer, int offset, int count)
			{
				return base.Read(buffer, offset, System.Math.Min(count, mMaximumReadSize));
			}
		}

		sealed class ReentrantSha1
			: SHA1
		{
			bool mHasReentered;

			public byte[] Input { get; private set; } = Array.Empty<byte>();

			public ReentrantSha1()
			{
				HashSizeValue = PhxHash.kSha1SizeOf * 8;
			}

			public override void Initialize()
			{
			}

			protected override void HashCore(byte[] array, int ibStart, int cbSize)
			{
				if (!mHasReentered)
				{
					mHasReentered = true;
					PhxHash.UInt32(this, 0x9ABCDEF0);
				}

				Input = array[ibStart..(ibStart + cbSize)];
			}

			protected override byte[] HashFinal() => Array.Empty<byte>();
		}

		sealed class ZeroThenDataReadStream
			: MemoryStream
		{
			bool mHasReturnedZero;

			public ZeroThenDataReadStream(byte[] buffer)
				: base(buffer, writable: false)
			{
			}

			public override int Read(byte[] buffer, int offset, int count)
			{
				if (!mHasReturnedZero)
				{
					mHasReturnedZero = true;
					return 0;
				}

				return base.Read(buffer, offset, count);
			}
		}
	};
}
