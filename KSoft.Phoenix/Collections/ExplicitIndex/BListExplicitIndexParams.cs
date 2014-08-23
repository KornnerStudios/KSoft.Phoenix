using System;
using System.Collections.Generic;
using Contracts = System.Diagnostics.Contracts;
using Contract = System.Diagnostics.Contracts.Contract;

namespace KSoft.Collections
{
	public class BListExplicitIndexParams<T> : BListParams
	{
		public readonly IComparer<T> kComparer = Comparer<T>.Default;

		/// <summary>Get the 'invalid' value for a value</summary>
		public Func<T> kTypeGetInvalid = () => default(T);

		public BListExplicitIndexParams() { }

		public BListExplicitIndexParams(int initialCapacity = TypeExtensions.kNone) : base()
		{
			Flags = 0;
			if (initialCapacity > 0)
				base.InitialCapacity = initialCapacity;
		}
	};
}