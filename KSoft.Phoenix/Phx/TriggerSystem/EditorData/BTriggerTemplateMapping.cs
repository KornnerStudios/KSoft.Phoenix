
namespace KSoft.Phoenix.Phx
{
	/*public*/ sealed class BTriggerTemplateMapping
	{
		#region Xml constants
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields", Justification = "Retains protocol binding reference data.")]
		const string kBindNameTriggerActive = "Activate";
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields", Justification = "Retains protocol binding reference data.")]
		const string kBindNameTriggerEffectsOnTrue = "Effect.True";
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields", Justification = "Retains protocol binding reference data.")]
		const string kBindNameTriggerEffectsOnFalse = "Effect.False";

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields", Justification = "Retains XML schema reference data.")]
		const string kXmlElementInputMapping = "InputMapping";
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields", Justification = "Retains XML schema reference data.")]
		const string kXmlElementOutputMapping = "OutputMapping";
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields", Justification = "Retains XML schema reference data.")]
		const string kXmlElementTriggerInput = "TriggerInput";
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields", Justification = "Retains XML schema reference data.")]
		const string kXmlElementTriggerOutput = "TriggerOutput";

		public static readonly XML.BListXmlParams kBListXmlParams = new()
		{
			RootName = "TriggerMappings",
			ElementName = "TriggerTemplateMapping",
			DataName = DatabaseNamedObject.kXmlAttrNameN,
		};

		// ID, Image, X, Y, SizeX, SizeY, GroupID, CommentOut, Obsolete, DoNotUse
		#endregion
	};
}