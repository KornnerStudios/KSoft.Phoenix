
namespace KSoft.Phoenix.Phx
{
	/*public*/ sealed class BTriggerUIGroups
	{
		#region Xml constants
		public static readonly XML.BListXmlParams kBListXmlParams = new()
		{
			RootName = "Groups",
			ElementName = "GroupUI",
		};

		// X, Y, iX, iY, oX, oY, Width, Height, GroupID, InternalGroupID

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields", Justification = "Retains XML schema reference data.")]
		const string kXmlElementTitle = "Title";
		#endregion
	};
}