using System;
using System.Collections.Generic;
using Contracts = System.Diagnostics.Contracts;
using Contract = System.Diagnostics.Contracts.Contract;

namespace KSoft.Collections
{
	using PhxUtil = KSoft.Phoenix.PhxUtil;

	public sealed class BListAutoId<T>
		: BListBase<T>
		, IBTypeNames
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
			mUndefinedInterface = new ProtoEnumWithUndefinedImpl(this);
		}

		public override void Clear()
		{
			base.Clear();

			if (mDBI != null)
				mDBI.Clear();

			if (mUndefinedInterface != null)
				mUndefinedInterface.Clear();
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
			{
				mDBI.Add(item.Data, item);
			}
			base.AddItem(item);

			return item.AutoId;
		}

		Dictionary<string, T> mDBI;
		internal void SetupDatabaseInterface(out Dictionary<string, T> dbi)
		{
			SetupDatabaseInterface();
			dbi = mDBI;
		}
		internal void SetupDatabaseInterface()
		{
			mDBI = new Dictionary<string, T>(Params != null ? Params.InitialCapacity : BCollectionParams.kDefaultCapacity);
		}

		internal int TryGetId(string name)
		{
			int id = TypeExtensions.kNone;
			if (mDBI == null)
				return id;

			T obj;
			if (mDBI.TryGetValue(name, out obj))
				id = obj.AutoId;

			return id;
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

		private ProtoEnumWithUndefinedImpl mUndefinedInterface;
		IProtoEnumWithUndefined IHasUndefinedProtoMemberInterface.UndefinedInterface { get { return mUndefinedInterface; } }
		internal IProtoEnumWithUndefined UndefinedInterface { get { return mUndefinedInterface; } }
	};
}

namespace KSoft.Phoenix
{
	partial class TypeExtensionsPhx
	{
		internal static string TryGetName<T>(this Collections.BListAutoId<T> dbi, int id)
			where T : class, Collections.IListAutoIdObject, new()
		{
			if (dbi == null)
				return null;

			if (id >= 0 && id < dbi.Count)
				return dbi[id].Data;

			return null;
		}
	};
}