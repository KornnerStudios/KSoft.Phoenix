using System;
using System.Collections.Generic;
using Contracts = System.Diagnostics.Contracts;
using Contract = System.Diagnostics.Contracts.Contract;

namespace KSoft.Collections
{
	/// <summary>
	/// Our base interface for lists of comparable objects, whose elements occupy an explicit index
	/// </summary>
	/// <typeparam name="T">Comparble object's type</typeparam>
	public abstract class BListExplicitIndexBase<T>
		: BListBase<T>
	{
		internal BListExplicitIndexParams<T> ExplicitIndexParams { get { return Params as BListExplicitIndexParams<T>; } }

		protected BListExplicitIndexBase(BListExplicitIndexParams<T> @params) : base(@params)
		{
			Contract.Requires<ArgumentNullException>(@params != null);
		}

		/// <summary>
		/// If the new count is greater than <see cref="Count"/>, adds new elements up-to <paramref name="new_count"/>,
		/// using the "invalid value" defined in the list params
		/// </summary>
		/// <param name="newCount"></param>
		/// <exception cref="ArgumentOutOfRangeException"><paramref name="newCount"/> is less than <see cref="Count"/></exception>
		internal void ResizeCount(int newCount)
		{
			if (newCount < Count)
				throw new ArgumentOutOfRangeException("new_count", newCount.ToString(),
					"For resizing to a smaller Count, use Capacity.");
			Contract.EndContractBlock();

			var eip = ExplicitIndexParams;

			for (int x = Count; x < newCount; x++)
				AddItem(eip.kTypeGetInvalid());
		}

		internal void InitializeItem(int index)
		{
			if (index < 0)
				throw new ArgumentOutOfRangeException("index", index.ToString());
			Contract.EndContractBlock();

			var eip = ExplicitIndexParams;

			if (index >= Count)
			{
				// expand the list up-to the requested index
				for (int x = Count; x <= index; x++)
					AddItem(eip.kTypeGetInvalid());
			}
			else
				base[index] = eip.kTypeGetInvalid();
		}
	};
}