
namespace KSoft.Phoenix.Phx
{
	/*public*/ sealed class BTrigerUINoteNodes
	{
		#region Xml constants
		public static readonly XML.BListXmlParams kBListXmlParams = new()
		{
			RootName = "NoteNodes",
			ElementName = "NoteNodeXml",
		};

		// X, Y, Width, Height, GroupID

		const string kXmlElementTitle = "Title";
		const string kXmlElementDescription = "Description";
		#endregion
	};
}