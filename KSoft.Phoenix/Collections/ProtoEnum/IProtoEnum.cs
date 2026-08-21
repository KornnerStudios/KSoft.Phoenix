using Contracts = System.Diagnostics.Contracts;

namespace KSoft.Collections
{
	public interface IProtoEnum
	{
		[Contracts.Pure]
		int TryGetMemberId(string memberName);
		[Contracts.Pure]
		string TryGetMemberName(int memberId);

		[Contracts.Pure]
		bool IsValidMemberId(int memberId);
		[Contracts.Pure]
		bool IsValidMemberName(string memberName);

		[Contracts.Pure]
		int GetMemberId(string memberName);
		[Contracts.Pure]
		string GetMemberName(int memberId);

		/// <summary>Number of members</summary>
		[Contracts.Pure]
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