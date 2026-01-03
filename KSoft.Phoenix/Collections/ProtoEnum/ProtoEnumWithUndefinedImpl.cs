using System.Collections.ObjectModel;
#if CONTRACTS_FULL_SHIM
using Contract = System.Diagnostics.ContractsShim.Contract;
#else
using Contract = System.Diagnostics.Contracts.Contract; // SHIM'D
#endif

namespace KSoft.Collections
{
	using PhxUtil = KSoft.Phoenix.PhxUtil;

	internal sealed class ProtoEnumWithUndefinedImpl
		: IProtoEnumWithUndefined
	{
		readonly IProtoEnum mRoot;
		ObservableCollection<string> mUndefined;

		public ProtoEnumWithUndefinedImpl(IProtoEnum root)
		{
			Contract.Requires(root != null);

			mRoot = root;
		}

		void InitializeUndefined()
		{
			if (mUndefined == null)
			{
				mUndefined = new ObservableCollection<string>();
			}
		}

		public void Clear()
		{
			mUndefined?.Clear();
		}

		#region IProtoEnum Members
		public int TryGetMemberId(string memberName)		=> mRoot.TryGetMemberId(memberName);
		public string TryGetMemberName(int memberId)		=> mRoot.TryGetMemberName(memberId);
		public bool IsValidMemberId(int memberId)			=> mRoot.IsValidMemberId(memberId);
		public bool IsValidMemberName(string memberName)	=> mRoot.IsValidMemberName(memberName);
		public int GetMemberId(string memberName)			=> mRoot.GetMemberId(memberName);
		public string GetMemberName(int memberId)			=> mRoot.GetMemberName(memberId);
		public int MemberCount								=> mRoot.MemberCount;
		#endregion

		#region IProtoEnumWithUndefined Members
		public int TryGetMemberIdOrUndefined(string memberName)
		{
			int id = TryGetMemberId(memberName);

			if (id.IsNone() && MemberUndefinedCount != 0)
			{
				id = mUndefined.FindIndex(str => PhxUtil.StrEqualsIgnoreCase(str, memberName));
				if (id.IsNotNone())
				{
					id = PhxUtil.GetUndefinedReferenceHandle(id);
				}
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
			{
				Contract.Assert(mUndefined != null);
				name = mUndefined[PhxUtil.GetUndefinedReferenceDataIndex(memberId)];
			}
			else
			{
				name = GetMemberName(memberId);
			}

			return name;
		}

		public int MemberUndefinedCount => mUndefined != null
			? mUndefined.Count
			: 0;

		public ObservableCollection<string> UndefinedMembers => mUndefined;
		#endregion
	};
}