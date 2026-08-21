using System;

namespace KSoft.Collections
{
	public sealed class BTypeValuesString
		: BTypeValuesBase<string>
	{
		public BTypeValuesString(BTypeValuesParams<string> @params) : base(@params)
		{
			ArgumentNullException.ThrowIfNull(@params);
		}
	};
}