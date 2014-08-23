
namespace KSoft.Phoenix.Phx
{
	public interface IDatabaseIdObject
		: IO.ITagElementStringNameStreamable
	{
		int DbId { get; }

		string Name { get; }
	};
}