using System;

namespace KSoft.Collections
{
	/// <summary>
	/// Our base interface for lists of game data, whose elements occupy an explicit index
	/// </summary>
	/// <typeparam name="T">Game Data's type</typeparam>
	public sealed class BListExplicitIndex<T>
		: BListExplicitIndexBase<T>
		where T : IO.ITagElementStringNameStreamable, new()
	{
		public BListExplicitIndex(BListExplicitIndexParams<T> @params) : base(@params)
		{
			ArgumentNullException.ThrowIfNull(@params);
		}
	};
}