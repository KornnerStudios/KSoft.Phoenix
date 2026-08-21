using System;
using System.Collections.Generic;

namespace KSoft.Collections
{
	public sealed class BTypeValues<T>
		: BTypeValuesBase<T>
		where T : IEqualityComparer<T>, IO.ITagElementStringNameStreamable, new()
	{
		public BTypeValues(BTypeValuesParams<T> @params) : base(@params)
		{
			ArgumentNullException.ThrowIfNull(@params);
		}
	};
}