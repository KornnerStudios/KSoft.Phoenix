
namespace KSoft.Phoenix.Phx
{
	public sealed class BTriggerProtoEffect
		: TriggerSystemProtoObject
	{
		#region Xml constants
		public static readonly XML.BListXmlParams kBListXmlParams = new XML.BListXmlParams("Effect")
		{
			DataName = DatabaseNamedObject.kXmlAttrNameN,
			Flags = 0,
		};
		#endregion

		public BTriggerProtoEffect() { }
		public BTriggerProtoEffect(BTriggerSystem root, BTriggerEffect instance) : base(root, instance)
		{
		}
	};
}