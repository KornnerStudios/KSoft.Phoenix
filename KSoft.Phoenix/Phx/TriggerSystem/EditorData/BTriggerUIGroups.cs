
namespace KSoft.Phoenix.Phx
{
	/*public*/ sealed class BTriggerUIGroups
	{
		#region Xml constants
		public static readonly XML.BListXmlParams kBListXmlParams = new XML.BListXmlParams
		{
			RootName = "Groups",
			ElementName = "GroupUI",
		};

		// X, Y, iX, iY, oX, oY, Width, Height, GroupID, InternalGroupID

		const string kXmlElementTitle = "Title";
		#endregion
	};
}