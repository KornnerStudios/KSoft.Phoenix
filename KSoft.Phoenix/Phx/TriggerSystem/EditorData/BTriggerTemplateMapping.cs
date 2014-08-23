
namespace KSoft.Phoenix.Phx
{
	/*public*/ sealed class BTriggerTemplateMapping
	{
		#region Xml constants
		const string kBindNameTriggerActive = "Activate";
		const string kBindNameTriggerEffectsOnTrue = "Effect.True";
		const string kBindNameTriggerEffectsOnFalse = "Effect.False";

		const string kXmlElementInputMapping = "InputMapping";
		const string kXmlElementOutputMapping = "OutputMapping";
		const string kXmlElementTriggerInput = "TriggerInput";
		const string kXmlElementTriggerOutput = "TriggerOutput";

		public static readonly XML.BListXmlParams kBListXmlParams = new XML.BListXmlParams
		{
			RootName = "TriggerMappings",
			ElementName = "TriggerTemplateMapping",
			DataName = DatabaseNamedObject.kXmlAttrNameN,
		};

		// ID, Image, X, Y, SizeX, SizeY, GroupID, CommentOut, Obsolete, DoNotUse
		#endregion
	};
}