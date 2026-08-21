using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace KSoft.Collections.Test
{
	[TestClass]
	public sealed class ProtoEnumTest
		: Phoenix.BaseTestClass
	{
		enum TestCode
		{
			Alpha,
			Beta,
		}

		[TestMethod]
		public void CodeEnum_GetMemberNameInvalidMember_ThrowsOutOfRange()
		{
			var code_enum = new CodeEnum<TestCode>();

			var exception = Assert.ThrowsExactly<ArgumentOutOfRangeException>(() => code_enum.GetMemberName(-1));

			Assert.AreEqual("memberId", exception.ParamName);
		}

		[TestMethod]
		public void ProtoEnumWithUndefined_NullRoot_ThrowsArgumentNull()
		{
			var exception = Assert.ThrowsExactly<ArgumentNullException>(() => new ProtoEnumWithUndefinedImpl(null!));

			Assert.AreEqual("root", exception.ParamName);
		}

		[TestMethod]
		public void ProtoEnumWithUndefined_UnregisteredUndefinedMember_ThrowsOutOfRange()
		{
			var proto_enum = new ProtoEnumWithUndefinedImpl(new CodeEnum<TestCode>());
			int undefined_id = Phoenix.PhxUtil.GetUndefinedReferenceHandle(0);

			var exception = Assert.ThrowsExactly<ArgumentOutOfRangeException>(
				() => proto_enum.GetMemberNameOrUndefined(undefined_id));

			Assert.AreEqual("memberId", exception.ParamName);
		}
	};
}
