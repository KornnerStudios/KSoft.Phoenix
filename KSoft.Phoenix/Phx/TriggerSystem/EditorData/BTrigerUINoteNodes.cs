
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

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields", Justification = "Retains XML schema reference data.")]
		const string kXmlElementTitle = "Title";
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields", Justification = "Retains XML schema reference data.")]
		const string kXmlElementDescription = "Description";
		#endregion
	};
}