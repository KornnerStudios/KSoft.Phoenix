
namespace KSoft.Phoenix.XML
{
	/// <summary>Various flags for <see cref="BCollectionXmlParams"/></summary>
	/// <remarks>
	/// * Intern flags should be set when certain values are strings and are used repeatedly within game data
	/// </remarks>
	[System.Flags]
	public enum BCollectionXmlParamsFlags
	{
		// Only one of these should ever be set
		UseInnerTextForData = 1<<0,

		UseElementForData = 1<<1,

		InternDataNames = 1<<2,

		ToLowerDataNames = 1<<3,
		RequiresDataNamePreloading = 1<<4,

		/// <summary>Forces the list code to not stream the root element from the xml document</summary>
		/// <remarks>Needed for when we're reading definitions from game files, but will later write to a app-specific monolithic file</remarks>
		ForceNoRootElementStreaming = 1<<5,
		SupportsUpdating = 1<<6,

		DoNotWriteUndefinedData = 1<<7,

		InternEverything = InternDataNames,
	};
}