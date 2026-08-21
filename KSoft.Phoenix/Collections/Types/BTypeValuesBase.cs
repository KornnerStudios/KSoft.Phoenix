using System;

namespace KSoft.Collections
{
	public abstract class BTypeValuesBase<T>
		: BListExplicitIndexBase<T>
	{
		internal BTypeValuesParams<T> TypeValuesParams { get { return Params as BTypeValuesParams<T>; } }

		protected BTypeValuesBase(BTypeValuesParams<T> @params) : base(@params)
		{
			ArgumentNullException.ThrowIfNull(@params);
		}
	};
}