using System;
using System.Collections.Generic;
using Contracts = System.Diagnostics.Contracts;
using Contract = System.Diagnostics.Contracts.Contract;

namespace KSoft.Collections
{
	using PhxUtil = KSoft.Phoenix.PhxUtil;

	internal class ProtoEnumWithUndefinedImpl
		: IProtoEnumWithUndefined
	{
		IProtoEnum mRoot;
		List<string> mUndefined;

		public ProtoEnumWithUndefinedImpl(IProtoEnum root)
		{
			Contract.Requires(root != null);

			mRoot = root;
		}

		void InitializeUndefined()
		{
			if (mUndefined == null)
				mUndefined = new List<string>();
		}

		#region IProtoEnum Members
		public int TryGetMemberId(string memberName)		{ return mRoot.TryGetMemberId(memberName); }
		public string TryGetMemberName(int memberId)		{ return mRoot.TryGetMemberName(memberId); }
		public bool IsValidMemberId(int memberId)			{ return mRoot.IsValidMemberId(memberId); }
		public bool IsValidMemberName(string memberName)	{ return mRoot.IsValidMemberName(memberName); }
		public int GetMemberId(string memberName)			{ return mRoot.GetMemberId(memberName); }
		public string GetMemberName(int memberId)			{ return mRoot.GetMemberName(memberId); }
		public int MemberCount						{ get	{ return mRoot.MemberCount; } }
		#endregion

		#region IProtoEnumWithUndefined Members
		public int TryGetMemberIdOrUndefined(string memberName)
		{
			int id = TryGetMemberId(memberName);

			if (id.IsNone() && MemberUndefinedCount != 0)
			{
				id = mUndefined.FindIndex(str => PhxUtil.StrEqualsIgnoreCase(str, memberName));
				if (id.IsNotNone())
					id = PhxUtil.GetUndefinedReferenceHandle(id);
			}

			return id;
		}

		public int GetMemberIdOrUndefined(string memberName)
		{
			int id = TryGetMemberIdOrUndefined(memberName);

			if (id.IsNone())
			{
				InitializeUndefined();

				id = mUndefined.Count;
				mUndefined.Add(memberName);
				id = PhxUtil.GetUndefinedReferenceHandle(id);
			}

			return id;
		}

		public string GetMemberNameOrUndefined(int memberId)
		{
			string name;
			
			if (PhxUtil.IsUndefinedReferenceHandle(memberId))
				name = mUndefined[PhxUtil.GetUndefinedReferenceDataIndex(memberId)];
			else
				name = GetMemberName(memberId);

			return name;
		}

		public int MemberUndefinedCount { get {
			return mUndefined != null
				? mUndefined.Count
				: 0;
		} }

		public IEnumerable<string> UndefinedMembers { get { return mUndefined; } }
		#endregion
	};
}