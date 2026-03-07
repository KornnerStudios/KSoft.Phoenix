using System;
using System.Collections.ObjectModel;
using System.Threading;
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
		, IDisposable
	{
		readonly IProtoEnum mRoot;
		ObservableCollection<string> mUndefined;
		// This is only really needed while we're loading a database.
		// Multiple files could be async loading, causing multiple threads
		// to resolve undefined members which need to be added.
		ReaderWriterLockSlim mUndefinedLock;

		public ProtoEnumWithUndefinedImpl(IProtoEnum root)
		{
			Contract.Requires(root != null);

			mRoot = root;
		}

		public void Dispose()
		{
			if (mUndefinedLock != null)
			{
				mUndefinedLock.Dispose();
				mUndefinedLock = null;
			}
		}

		void InitializeUndefined()
		{
			// this itself is probably not thread safe
			if (mUndefined == null)
			{
				mUndefined = new ObservableCollection<string>();
				mUndefinedLock = new ReaderWriterLockSlim();
			}
		}

		public void Clear()
		{
			if (mUndefined != null)
			{
				mUndefinedLock.EnterWriteLock();
				mUndefined.Clear();
				mUndefinedLock.ExitWriteLock();
			}
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
				mUndefinedLock.EnterReadLock();
				id = mUndefined.FindIndex(str => PhxUtil.StrEqualsIgnoreCase(str, memberName));
				mUndefinedLock.ExitReadLock();

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

				mUndefinedLock.EnterWriteLock();
				id = mUndefined.Count;
				mUndefined.Add(memberName);
				id = PhxUtil.GetUndefinedReferenceHandle(id);
				mUndefinedLock.ExitWriteLock();
			}

			return id;
		}

		public string GetMemberNameOrUndefined(int memberId)
		{
			string name;

			if (PhxUtil.IsUndefinedReferenceHandle(memberId))
			{
				Contract.Assert(mUndefined != null);
				mUndefinedLock.EnterReadLock();
				name = mUndefined[PhxUtil.GetUndefinedReferenceDataIndex(memberId)];
				mUndefinedLock.ExitReadLock();
			}
			else
			{
				name = GetMemberName(memberId);
			}

			return name;
		}

		public int MemberUndefinedCount { get {
			if (mUndefined != null)
			{
				mUndefinedLock.EnterReadLock();
				int count = mUndefined.Count;
				mUndefinedLock.ExitReadLock();
				return count;
			}
			return 0;
		} }

		public ObservableCollection<string> UndefinedMembers => mUndefined;
		#endregion
	};
}