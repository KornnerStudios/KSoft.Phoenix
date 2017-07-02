
namespace KSoft.Collections
{
	public interface IListAutoIdObject
		: IO.ITagElementStringNameStreamable
		, System.ComponentModel.INotifyPropertyChanged
	{
		/// <summary>Generated AutoID for this object. Used internally, not a concept in the actual Phoenix engine</summary>
		int AutoId { get; set; }

		/// <summary>Main, ie key, data</summary>
		/// <example>If this were a Resource, then this could be "Supplies"</example>
		string Data { get; set; }
	};
}