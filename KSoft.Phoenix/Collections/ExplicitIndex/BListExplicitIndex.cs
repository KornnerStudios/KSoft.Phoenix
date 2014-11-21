using System;
using System.Collections.Generic;
using Contracts = System.Diagnostics.Contracts;
using Contract = System.Diagnostics.Contracts.Contract;

namespace KSoft.Collections
{
	/// <summary>
	/// Our base interface for lists of game data, whose elements occupy an explicit index
	/// </summary>
	/// <typeparam name="T">Game Data's type</typeparam>
	public sealed class BListExplicitIndex<T>
		: BListExplicitIndexBase<T>
		where T : IEqualityComparer<T>, IO.ITagElementStringNameStreamable, new()
	{
		public BListExplicitIndex(BListExplicitIndexParams<T> @params) : base(@params)
		{
			Contract.Requires<ArgumentNullException>(@params != null);
		}
	};
}