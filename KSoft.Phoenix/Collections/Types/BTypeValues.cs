using System;
using System.Collections.Generic;
using Contracts = System.Diagnostics.Contracts;
using Contract = System.Diagnostics.Contracts.Contract;

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