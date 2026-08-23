using System;
using System.Collections.Generic;

namespace KSoft.Collections
{
	using PhxUtil = KSoft.Phoenix.PhxUtil;

	public sealed class BListAutoIdParams
		: BListParams
	{
	};

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
		public BListAutoId(BListAutoIdParams? @params = null) : base(@params)
		{
			kUnregisteredMessage = BuildUnRegisteredMsg();
			mUndefinedInterface = new ProtoEnumWithUndefinedImpl(this);
		}

		public override void Clear()
		{
			base.Clear();

			mDBI?.Clear();

			mUndefinedInterface?.Clear();
		}

		#region Database interfaces
		/// <remarks>Mainly a hack for adding new items dynamically</remarks>
		void PreAdd(T item, string itemName, int id = TypeExtensions.kNone)
		{
			item.AutoId = id.IsNotNone()
				? id
				: Count;

			if (itemName != null)
			{
				item.Data = itemName;
			}
		}
		internal int DynamicAdd(T item, string itemName, int id = TypeExtensions.kNone)
		{
			if (IsFullyPreloaded)
			{
				throw new InvalidOperationException(
					$"Cannot dynamically add {typeof(T).Name} items after preloading: {itemName}");
			}

			PreAdd(item, itemName, id);
			if (mDBI != null)
			{
				if (mDBI.ContainsKey(itemName))
				{
					throw new ArgumentException(string.Format(
						"There is already a {0} named {1}",
						typeof(T).Name, itemName
						), nameof(itemName));
				}

				mDBI.Add(item.Data, item);

				if (Params != null && Params.ToLowerDataNames)
				{
					string lower_name = Phoenix.PhxUtil.ToLowerIfContainsUppercase(item.Data!)!;
					if (!object.ReferenceEquals(lower_name, item.Data))
					{
						mDBI.Add(lower_name, item);
					}
				}
			}
			base.AddItem(item);

			return item.AutoId;
		}

		Dictionary<string, T>? mDBI;
		internal void SetupDatabaseInterface()
		{
			mDBI = new Dictionary<string, T>(Params != null ? Params.InitialCapacity : BCollectionParams.kDefaultCapacity);
		}

		internal int TryGetId(string name)
		{
			int id = TypeExtensions.kNone;
			if (mDBI == null)
			{
				return id;
			}

			if (mDBI.TryGetValue(name, out T? obj))
			{
				id = obj.AutoId;
			}

			return id;
		}
		#endregion

		// #HACK_PHOENIX This was to track down a Preload vs StreamXml issue
		// e.g. BWeaponTypes streaming at the same time BDamageTypes are streaming.
		// This is reset during StreamUpdate's Preload phase.
		// This needs to be rethought in generally, but especially if we support adding new items like in an IDE
		public bool IsFullyPreloaded { get; set; }

		#region IProtoEnum Members
		public int TryGetMemberId(string memberName)
		{
			return mList.FindIndex(n => PhxUtil.StrEqualsIgnoreCase(n.Data, memberName));
		}
		public string? TryGetMemberName(int memberId)
		{
			return IsValidMemberId(memberId) ? GetMemberName(memberId) : null;
		}

		string IProtoEnum.TryGetMemberName(int memberId)
		{
			return TryGetMemberName(memberId)!;
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
			if (!IsFullyPreloaded)
			{
				// This was being hit when BWeaponTypes were being fully serialized inside the Preload method,
				// instead of the StreamXml method. This was leading to them trying to resolve BDamageTypes
				// (via the Modifiers properties) before they were fully preloaded.
				throw new InvalidOperationException(
					$"Cannot query {typeof(T).Name} items before preloading: {memberName}");
			}

			int index = TryGetMemberId(memberName);

			if (index.IsNone())
			{
				throw new ArgumentException(kUnregisteredMessage, memberName);
			}

			return index;
		}
		public string GetMemberName(int memberId)
		{
			if (!IsValidMemberId(memberId))
			{
				throw new ArgumentOutOfRangeException(nameof(memberId));
			}

			return this[memberId].Data;
		}

		public int MemberCount => Count;
		#endregion

		public override object? GetObject(int id)
		{
			if (id.IsNone())
			{
				return null;
			}

			if (PhxUtil.IsUndefinedReferenceHandle(id))
			{
				return Phoenix.TypeExtensionsPhx.GetUndefinedObject(mUndefinedInterface, id);
			}

			return base.GetObject(id);
		}

		private readonly ProtoEnumWithUndefinedImpl mUndefinedInterface;
		IProtoEnumWithUndefined IHasUndefinedProtoMemberInterface.UndefinedInterface => mUndefinedInterface;
		internal IProtoEnumWithUndefined UndefinedInterface => mUndefinedInterface;
	};
}

namespace KSoft.Phoenix
{
	partial class TypeExtensionsPhx
	{
		internal static string? TryGetName<T>(this Collections.BListAutoId<T> dbi, int id)
			where T : class, Collections.IListAutoIdObject, new()
		{
			if (dbi == null)
			{
				return null;
			}

			if (id >= 0 && id < dbi.Count)
			{
				return dbi[id].Data;
			}

			return null;
		}
	};
}