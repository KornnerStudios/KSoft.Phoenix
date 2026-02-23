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
			Assert.IsTrue(gameFileTeaKey.Length == PhxTEA.kKeySize);
			Assert.IsTrue(gameFileTeaKey.EqualsArray(PhxTEA.kKeyGameFile));
		}
	};
}
