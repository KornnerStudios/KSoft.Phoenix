using System;

namespace KSoft.Collections
{
	public sealed class BTypeNamesWithCode
		: BTypeNames
	{
		readonly IProtoEnum mCodeTypes;

		public BTypeNamesWithCode(IProtoEnum CodeTypes)
		{
			ArgumentNullException.ThrowIfNull(CodeTypes);

			mCodeTypes = CodeTypes;
		}

		#region IProtoEnum Members
		public override int TryGetMemberId(string memberName)
		{
			int idx = base.TryGetMemberId(memberName);

			if (idx.IsNone())
			{
				idx = mCodeTypes.TryGetMemberId(memberName);
				if (idx.IsNotNone())
				{
					idx += Count;
				}
			}

			return idx;
		}
		public override string? TryGetMemberName(int memberId)
		{
			string? name = base.TryGetMemberName(memberId);

			if (name == null)
			{
				return mCodeTypes.TryGetMemberName(memberId);
			}

			return name;
		}

		public override string GetMemberName(int memberId)
		{
			if (memberId < Count)
			{
				return base.GetMemberName(memberId);
			}

			memberId -= Count;
			return mCodeTypes.GetMemberName(memberId);
		}

		public override int MemberCount => Count + mCodeTypes.MemberCount;
		#endregion
	};
}