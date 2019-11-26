using System;
using System.Collections.Generic;
#if CONTRACTS_FULL_SHIM
using Contract = System.Diagnostics.ContractsShim.Contract;
#else
using Contract = System.Diagnostics.Contracts.Contract; // SHIM'D
#endif

namespace KSoft.Collections
{
	public sealed class BTypeValues<T>
		: BTypeValuesBase<T>
		where T : IEqualityComparer<T>, IO.ITagElementStringNameStreamable, new()
	{
		public BTypeValues(BTypeValuesParams<T> @params) : base(@params)
		{
			Contract.Requires<ArgumentNullException>(@params != null);
		}
	};
}