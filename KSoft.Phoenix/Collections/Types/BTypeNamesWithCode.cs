using System;
using System.Collections.Generic;
using Contracts = System.Diagnostics.Contracts;
using Contract = System.Diagnostics.Contracts.Contract;

namespace KSoft.Collections
{
	public class BTypeNamesWithCode : BTypeNames
	{
		IProtoEnum mCodeTypes;

		public BTypeNamesWithCode(IProtoEnum CodeTypes)
		{
			Contract.Requires<ArgumentNullException>(CodeTypes != null);

			mCodeTypes = CodeTypes;
		}

		#region IProtoEnum Members
		public override int TryGetMemberId(string memberName)
		{
			int idx = base.TryGetMemberId(memberName);

			if (idx.IsNone())
			{
				idx = mCodeTypes.GetMemberId(memberName);
				if (idx.IsNotNone())
					idx += Count;
			}

			return idx;
		}
		public override string TryGetMemberName(int memberId)
		{
			string name = base.TryGetMemberName(memberId);
			
			if (name == null)
				return mCodeTypes.TryGetMemberName(memberId);

			return name;
		}

		public override string GetMemberName(int memberId)
		{
			if (memberId < Count)
				return base.GetMemberName(memberId);

			memberId -= Count;
			return mCodeTypes.GetMemberName(memberId);
		}

		public override int MemberCount { get {
			return Count + mCodeTypes.MemberCount;
		} }
		#endregion
	};
}