using System;
using System.Collections.Generic;
using Contracts = System.Diagnostics.Contracts;
using Contract = System.Diagnostics.Contracts.Contract;

namespace KSoft.Collections
{
	public sealed class BTypeValuesString
		: BTypeValuesBase<string>
	{
		public BTypeValuesString(BTypeValuesParams<string> @params) : base(@params)
		{
			Contract.Requires<ArgumentNullException>(@params != null);
		}
	};
}