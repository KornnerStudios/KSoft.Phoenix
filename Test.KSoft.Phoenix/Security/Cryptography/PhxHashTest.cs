using System;
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
	};
}
