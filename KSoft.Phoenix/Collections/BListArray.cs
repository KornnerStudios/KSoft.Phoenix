
namespace KSoft.Collections
{
	/// <summary>For an array of items which have no specific order or names</summary>
	/// <typeparam name="T">Game Data's type</typeparam>
	/// <see cref="KSoft.Phoenix.Phx.BProtoTechEffect"/>
	public sealed class BListArray<T>
		: BListBase<T>
		where T : IO.ITagElementStringNameStreamable, new()
	{
	};
}