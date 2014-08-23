using System;
using System.Collections.Generic;
using Contracts = System.Diagnostics.Contracts;
using Contract = System.Diagnostics.Contracts.Contract;

namespace KSoft.Collections
{
	public sealed class BTypeValuesInt32
		: BTypeValuesBase<int>
	{
		public BTypeValuesInt32(BTypeValuesParams<int> @params) : base(@params)
		{
			Contract.Requires<ArgumentNullException>(@params != null);
		}
	};
}