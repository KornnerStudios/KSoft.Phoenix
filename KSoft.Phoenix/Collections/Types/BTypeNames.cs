using System;

namespace KSoft.Collections
{
	using PhxUtil = KSoft.Phoenix.PhxUtil;

	public interface IBTypeNames
		: IBList
		, IProtoEnum
		, IHasUndefinedProtoMemberInterface
	{
	};

	public class BTypeNames
		: BListBase<string>
		, IBTypeNames
	{
		readonly string kUnregisteredMessage;

		static string BuildUnRegisteredMsg()
		{
			return "Unregistered BTypeName!";
		}
		public BTypeNames()
		{
			kUnregisteredMessage = BuildUnRegisteredMsg();
			mUndefinedInterface = new ProtoEnumWithUndefinedImpl(this);
		}

		public override void Clear()
		{
			base.Clear();

			mUndefinedInterface?.Clear();
		}

		#region IProtoEnum Members
		public virtual int TryGetMemberId(string memberName)
		{
			return mList.FindIndex(n => PhxUtil.StrEqualsIgnoreCase(n, memberName));
		}
		public virtual string? TryGetMemberName(int memberId)
		{
			return IsValidMemberId(memberId)
				? GetMemberName(memberId)
				: null;
		}
		string IProtoEnum.TryGetMemberName(int memberId)
		{
			return TryGetMemberName(memberId)!;
		}

		public bool IsValidMemberId(int memberId)
		{
			return memberId >= 0 && memberId < MemberCount;
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
			{
				throw new ArgumentException(kUnregisteredMessage, memberName);
			}

			return index;
		}
		public virtual string GetMemberName(int memberId)
		{
			if (!IsValidMemberId(memberId))
			{
				throw new ArgumentOutOfRangeException(nameof(memberId));
			}

			return this[memberId];
		}

		public virtual int MemberCount { get { return Count; } }
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
		public static string? TryGetName(this Collections.BTypeNames dbi, int id)
		{
			if (dbi == null)
			{
				return null;
			}

			if (id >= 0 && id < dbi.Count)
			{
				return dbi[id];
			}

			return null;
		}
	};
}