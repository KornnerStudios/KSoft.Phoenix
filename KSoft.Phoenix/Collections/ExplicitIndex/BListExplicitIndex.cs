using System;
#if CONTRACTS_FULL_SHIM
using Contract = System.Diagnostics.ContractsShim.Contract;
#else
using Contract = System.Diagnostics.Contracts.Contract; // SHIM'D
#endif

namespace KSoft.Collections
{
	/// <summary>
	/// Our base interface for lists of game data, whose elements occupy an explicit index
	/// </summary>
	/// <typeparam name="T">Game Data's type</typeparam>
	public sealed class BListExplicitIndex<T>
		: BListExplicitIndexBase<T>
		where T : IO.ITagElementStringNameStreamable, new()
	{
		public BListExplicitIndex(BListExplicitIndexParams<T> @params) : base(@params)
		{
			Contract.Requires<ArgumentNullException>(@params != null);
		}
	};
}