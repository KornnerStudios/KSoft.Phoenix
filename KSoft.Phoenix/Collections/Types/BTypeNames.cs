using System;
using System.Collections.Generic;
using Contracts = System.Diagnostics.Contracts;
using Contract = System.Diagnostics.Contracts.Contract;

namespace KSoft.Collections
{
	using PhxUtil = KSoft.Phoenix.PhxUtil;

	public class BTypeNames
		: BListBase<string>
		, IProtoEnum
	{
		readonly string kUnregisteredMessage;

		static string BuildUnRegisteredMsg()
		{
			return string.Format("Unregistered {0}!", "BTypeName");
		}
		public BTypeNames()
		{
			kUnregisteredMessage = BuildUnRegisteredMsg();
			UndefinedInterface = new ProtoEnumWithUndefinedImpl(this);
		}

		#region IProtoEnum Members
		public virtual int TryGetMemberId(string memberName)
		{
			return mList.FindIndex(n => PhxUtil.StrEqualsIgnoreCase(n, memberName));
		}
		public virtual string TryGetMemberName(int memberId)
		{
			return IsValidMemberId(memberId)
				? GetMemberName(memberId)
				: null;
		}
		public bool IsValidMemberId(int memberId)
		{
			return memberId >= 0 && memberId < MemberCount;
		}
		public bool IsValidMemberName(string memberName)
		{
			int index = TryGetMemberId(memberName);

			return index.IsNotNone();
		}

		public int GetMemberId(string memberName)
		{
			int index = TryGetMemberId(memberName);

			if (index.IsNone())
				throw new ArgumentException(kUnregisteredMessage, memberName);

			return index;
		}
		public virtual string GetMemberName(int memberId)
		{
			return this[memberId];
		}

		public virtual int MemberCount { get { return Count; } }
		#endregion

		internal IProtoEnumWithUndefined UndefinedInterface { get; private set; }
	};
}