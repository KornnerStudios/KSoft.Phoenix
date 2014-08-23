using System;
using System.Collections.Generic;
using Contracts = System.Diagnostics.Contracts;
using Contract = System.Diagnostics.Contracts.Contract;

namespace KSoft.Collections
{
	public abstract class BTypeValuesBase<T>
		: BListExplicitIndexBase<T>
	{
		internal BTypeValuesParams<T> TypeValuesParams { get { return Params as BTypeValuesParams<T>; } }

		protected BTypeValuesBase(BTypeValuesParams<T> @params) : base(@params)
		{
			Contract.Requires<ArgumentNullException>(@params != null);
		}
	};
}