using System;

namespace KSoft.Collections
{
	public sealed class BTypeValuesSingle
		: BTypeValuesBase<float>
	{
		public BTypeValuesSingle(BTypeValuesParams<float> @params) : base(@params)
		{
			ArgumentNullException.ThrowIfNull(@params);
		}

		public bool HasNonZeroItems { get {
			for (int x = 0; x < Count; x++)
			{
				if (this[x] != 0.0f)
				{
					return true;
				}
			}

			return false;
		} }
	};
}