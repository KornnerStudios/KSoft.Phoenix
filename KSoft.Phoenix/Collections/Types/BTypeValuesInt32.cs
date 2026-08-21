using System;

namespace KSoft.Collections
{
	public sealed class BTypeValuesInt32
		: BTypeValuesBase<int>
	{
		public BTypeValuesInt32(BTypeValuesParams<int> @params) : base(@params)
		{
			ArgumentNullException.ThrowIfNull(@params);
		}
	};
}