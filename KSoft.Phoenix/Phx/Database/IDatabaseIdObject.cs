
namespace KSoft.Phoenix.Phx
{
	public interface IDatabaseIdObject
		: IO.ITagElementStringNameStreamable
		, System.ComponentModel.INotifyPropertyChanged
	{
		int DbId { get; }

		string Name { get; }
	};
}