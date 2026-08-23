using System;

namespace KSoft.Collections
{
	public abstract class BTypeValuesBase<T>
		: BListExplicitIndexBase<T>
	{
		internal BTypeValuesParams<T> TypeValuesParams { get { return Params as BTypeValuesParams<T>
			?? throw new InvalidOperationException("Type values parameters are required."); } }

		protected BTypeValuesBase(BTypeValuesParams<T> @params) : base(@params)
		{
			ArgumentNullException.ThrowIfNull(@params);
		}
	};
}