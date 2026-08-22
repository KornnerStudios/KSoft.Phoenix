namespace KSoft.Collections
{
	public interface IProtoEnum
	{
		int TryGetMemberId(string memberName);
		string TryGetMemberName(int memberId);

		bool IsValidMemberId(int memberId);
		bool IsValidMemberName(string memberName);

		int GetMemberId(string memberName);
		string GetMemberName(int memberId);

		/// <summary>Number of members</summary>
		int MemberCount { get; }
	};
}

namespace KSoft.Phoenix
{
	partial class TypeExtensionsPhx
	{
		public static int TryGetId(this Collections.IProtoEnum dbi, string name)
		{
			if (dbi == null)
			{
				return TypeExtensions.kNone;
			}

			return dbi.TryGetMemberId(name);
		}
	};
}