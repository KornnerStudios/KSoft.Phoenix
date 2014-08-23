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
			var result = new byte[PhxHash.kResultSize];
			PhxHash.Sha1Hash("TehHaloz", result);
		}
	};
}