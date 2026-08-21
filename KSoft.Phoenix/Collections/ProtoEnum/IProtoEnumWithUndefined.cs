using System;
using System.Collections.ObjectModel;
using Contracts = System.Diagnostics.Contracts;

namespace KSoft.Collections
{
	public interface IProtoEnumWithUndefined
		: IProtoEnum
	{
		int TryGetMemberIdOrUndefined(string memberName);

		[Contracts.Pure]
		int GetMemberIdOrUndefined(string memberName);
		[Contracts.Pure]
		string GetMemberNameOrUndefined(int memberId);

		/// <summary>Number of members that are undefined</summary>
		[Contracts.Pure]
		int MemberUndefinedCount { get; }

		ObservableCollection<string> UndefinedMembers { get; }
	};

	public interface IHasUndefinedProtoMemberInterface
	{
		IProtoEnumWithUndefined UndefinedInterface { get; }
	};
}

namespace KSoft.Phoenix
{
	partial class TypeExtensionsPhx
	{
		internal static int TryGetIdWithUndefined(this Collections.IHasUndefinedProtoMemberInterface dbi, string name)
		{
			if (dbi == null)
			{
				return TypeExtensions.kNone;
			}

			return dbi.UndefinedInterface.GetMemberIdOrUndefined(name);
		}
		internal static string TryGetNameWithUndefined(this Collections.IHasUndefinedProtoMemberInterface dbi, int id)
		{
			if (dbi == null)
			{
				return null;
			}

			return dbi.UndefinedInterface.GetMemberNameOrUndefined(id);
		}

		public static UndefinedObjectResult GetUndefinedObject(this Collections.IProtoEnumWithUndefined protoEnum, int memberId)
		{
			ArgumentNullException.ThrowIfNull(protoEnum);

			string name = protoEnum.GetMemberNameOrUndefined(memberId);

			return new UndefinedObjectResult(memberId, name);
		}
	};
}