using System;
using System.Collections.ObjectModel;
using Contracts = System.Diagnostics.Contracts;
using Contract = System.Diagnostics.Contracts.Contract;

namespace KSoft.Collections
{
	[Contracts.ContractClass(typeof(IProtoEnumWithUndefinedContract))]
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
	[Contracts.ContractClassFor(typeof(IProtoEnumWithUndefined))]
	abstract class IProtoEnumWithUndefinedContract
		: IProtoEnumWithUndefined
	{
		#region IProtoEnum Members
		public abstract int TryGetMemberId(string memberName);
		public abstract string TryGetMemberName(int memberId);
		public abstract bool IsValidMemberId(int memberId);
		public abstract bool IsValidMemberName(string memberName);
		public abstract int GetMemberId(string memberName);
		public abstract string GetMemberName(int memberId);
		public abstract int MemberCount { get; }
		#endregion

		#region IProtoEnumWithUndefined
		public abstract int TryGetMemberIdOrUndefined(string memberName);
		public abstract int GetMemberIdOrUndefined(string memberName);
		public abstract string GetMemberNameOrUndefined(int memberId);

		int IProtoEnumWithUndefined.MemberUndefinedCount { get {
			Contract.Ensures(Contract.Result<int>() >= 0);

			throw new NotImplementedException();
		} }
		ObservableCollection<string> IProtoEnumWithUndefined.UndefinedMembers { get {
#if false
				Contract.Ensures(Contract.Result<ObservableCollection<string>>() != null);
#endif

				throw new NotImplementedException();
		} }
		#endregion
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
				return TypeExtensions.kNone;

			return dbi.UndefinedInterface.GetMemberIdOrUndefined(name);
		}
		internal static string TryGetNameWithUndefined(this Collections.IHasUndefinedProtoMemberInterface dbi, int id)
		{
			if (dbi == null)
				return null;

			return dbi.UndefinedInterface.GetMemberNameOrUndefined(id);
		}

		public static UndefinedObjectResult GetUndefinedObject(this Collections.IProtoEnumWithUndefined protoEnum, int memberId)
		{
			Contract.Requires(protoEnum != null);

			string name = protoEnum.GetMemberNameOrUndefined(memberId);

			return new UndefinedObjectResult(memberId, name);
		}
	};
}