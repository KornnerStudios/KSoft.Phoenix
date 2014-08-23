using System;
using System.Collections.Generic;
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

		IEnumerable<string> UndefinedMembers { get; }
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
		IEnumerable<string> IProtoEnumWithUndefined.UndefinedMembers { get {
			Contract.Ensures(Contract.Result<IEnumerable<string>>() != null);

			throw new NotImplementedException();
		} }
		#endregion
	};
}