using System;
using System.Collections.Generic;
using Contracts = System.Diagnostics.Contracts;
using Contract = System.Diagnostics.Contracts.Contract;

namespace KSoft.Collections
{
	using PhxUtil = KSoft.Phoenix.PhxUtil;

	public sealed class BListAutoId<T>
		: BListBase<T>
		, IProtoEnum
		// For now, I don't see a reason to support struct types in AutoIds
		// If structs are needed, the streaming logic will need to be adjusted
		where T : class, IListAutoIdObject, new()
	{
		readonly string kUnregisteredMessage;

		static string BuildUnRegisteredMsg()
		{
			return string.Format("Unregistered {0}!", typeof(T).Name);
		}
		public BListAutoId()
		{
			kUnregisteredMessage = BuildUnRegisteredMsg();
			UndefinedInterface = new ProtoEnumWithUndefinedImpl(this);
		}

		#region Database interfaces
		/// <remarks>Mainly a hack for adding new items dynamically</remarks>
		void PreAdd(T item, string itemName, int id = TypeExtensions.kNone)
		{
			item.AutoId = id.IsNotNone()
				? id
				: Count;

			if (itemName != null)
				item.Data = itemName;
		}
		internal int DynamicAdd(T item, string itemName, int id = TypeExtensions.kNone)
		{
			PreAdd(item, itemName, id);
			if (mDBI != null)
				mDBI.Add(item.Data, item);
			base.AddItem(item);

			return item.AutoId;
		}

		Dictionary<string, T> mDBI;
		internal void SetupDatabaseInterface(out Dictionary<string, T> dbi)
		{
			mDBI = dbi = new Dictionary<string, T>(Params != null ? Params.InitialCapacity : BCollectionParams.kDefaultCapacity);
		}
		#endregion

		#region IProtoEnum Members
		public int TryGetMemberId(string memberName)
		{
			return mList.FindIndex(n => PhxUtil.StrEqualsIgnoreCase(n.Data, memberName));
		}
		public string TryGetMemberName(int memberId)
		{
			return IsValidMemberId(memberId) ? GetMemberName(memberId) : null;
		}
		public bool IsValidMemberId(int memberId)
		{
			return memberId >= 0 && memberId < Count;
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
		public string GetMemberName(int memberId)
		{
			return this[memberId].Data;
		}

		public int MemberCount { get { return Count; } }
		#endregion

		internal IProtoEnumWithUndefined UndefinedInterface { get; private set; }

		internal void Sort(Comparison<T> comparison)
		{
			mList.Sort(comparison);
		}
	};
}