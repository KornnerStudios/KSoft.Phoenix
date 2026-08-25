using System;

namespace KSoft.Collections
{
	using PhxUtil = KSoft.Phoenix.PhxUtil;

	public sealed class CodeEnum<TEnum>
		: IProtoEnum
		where TEnum : struct, Enum
	{
		static readonly string[] kNames = Enum.GetNames<TEnum>();
		static readonly string kUnregisteredMessage = "Unregistered " + typeof(TEnum).Name + "!";

		#region IProtoEnum Members
		public int TryGetMemberId(string memberName)
		{
			return Array.FindIndex(kNames, n => PhxUtil.StrEqualsIgnoreCase(n, memberName));
		}
		public string? TryGetMemberName(int memberId)
		{
			return IsValidMemberId(memberId)
				? GetMemberName(memberId)
				: null;
		}
		public bool IsValidMemberId(int memberId)
		{
			return memberId >= 0 && memberId < kNames.Length;
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
			{
				throw new ArgumentException(kUnregisteredMessage, memberName);
			}

			return index;
		}
		public string GetMemberName(int memberId)
		{
			if (!IsValidMemberId(memberId))
			{
				throw new ArgumentOutOfRangeException(nameof(memberId));
			}

			return kNames[memberId];
		}

		public int MemberCount => kNames.Length;
		#endregion
	};
}