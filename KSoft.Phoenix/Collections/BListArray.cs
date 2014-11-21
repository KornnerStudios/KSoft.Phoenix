using System;
using Contracts = System.Diagnostics.Contracts;
using Contract = System.Diagnostics.Contracts.Contract;

namespace KSoft.Collections
{
	/// <summary>For an array of items which have no specific order or names</summary>
	/// <typeparam name="T">Game Data's type</typeparam>
	/// <see cref="Engine.BProtoTechEffect"/>
	public sealed class BListArray<T>
		: BListBase<T>
		where T : IO.ITagElementStringNameStreamable, new()
	{
	};
}