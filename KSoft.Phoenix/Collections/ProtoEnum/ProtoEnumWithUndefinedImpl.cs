using System;
using System.Collections.ObjectModel;
using System.Threading;

namespace KSoft.Collections
{
	using PhxUtil = KSoft.Phoenix.PhxUtil;

	internal sealed class ProtoEnumWithUndefinedImpl
		: IProtoEnumWithUndefined
		, IDisposable
	{
		readonly IProtoEnum mRoot;
		ObservableCollection<string>? mUndefined;
		// This is only really needed while we're loading a database.
		// Multiple files could be async loading, causing multiple threads
		// to resolve undefined members which need to be added.
		ReaderWriterLockSlim? mUndefinedLock;

		public ProtoEnumWithUndefinedImpl(IProtoEnum root)
		{
			ArgumentNullException.ThrowIfNull(root);

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
			var undefined = mUndefined;
			if (undefined is not null)
			{
				var undefinedLock = mUndefinedLock ?? throw new ObjectDisposedException(nameof(ProtoEnumWithUndefinedImpl));
				undefinedLock.EnterWriteLock();
				undefined.Clear();
				undefinedLock.ExitWriteLock();
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
				var undefined = mUndefined ?? throw new InvalidOperationException("Undefined members were unexpectedly unavailable.");
				var undefinedLock = mUndefinedLock ?? throw new ObjectDisposedException(nameof(ProtoEnumWithUndefinedImpl));
				undefinedLock.EnterReadLock();
				id = undefined.FindIndex(str => PhxUtil.StrEqualsIgnoreCase(str, memberName));
				undefinedLock.ExitReadLock();

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

				var undefined = mUndefined ?? throw new InvalidOperationException("Undefined members were unexpectedly unavailable.");
				var undefinedLock = mUndefinedLock ?? throw new ObjectDisposedException(nameof(ProtoEnumWithUndefinedImpl));
				undefinedLock.EnterWriteLock();
				id = undefined.Count;
				undefined.Add(memberName);
				id = PhxUtil.GetUndefinedReferenceHandle(id);
				undefinedLock.ExitWriteLock();
			}

			return id;
		}

		public string GetMemberNameOrUndefined(int memberId)
		{
			string name;

			if (PhxUtil.IsUndefinedReferenceHandle(memberId))
			{
				int undefined_index = PhxUtil.GetUndefinedReferenceDataIndex(memberId);
				var undefined = mUndefined;
				if (undefined is null)
				{
					throw new ArgumentOutOfRangeException(nameof(memberId));
				}
				var undefinedLock = mUndefinedLock ?? throw new ObjectDisposedException(nameof(ProtoEnumWithUndefinedImpl));

				undefinedLock.EnterReadLock();
				try
				{
					if (undefined_index >= undefined.Count)
					{
						throw new ArgumentOutOfRangeException(nameof(memberId));
					}
					name = undefined[undefined_index];
				}
				finally
				{
					undefinedLock.ExitReadLock();
				}
			}
			else
			{
				name = GetMemberName(memberId);
			}

			return name;
		}

		public int MemberUndefinedCount { get {
			var undefined = mUndefined;
			if (undefined is not null)
			{
				var undefinedLock = mUndefinedLock ?? throw new ObjectDisposedException(nameof(ProtoEnumWithUndefinedImpl));
				undefinedLock.EnterReadLock();
				int count = undefined.Count;
				undefinedLock.ExitReadLock();
				return count;
			}
			return 0;
		} }

		public ObservableCollection<string> UndefinedMembers
			=> mUndefined ?? throw new InvalidOperationException("Undefined members are not available.");
		#endregion
	};
}