
namespace KSoft.Phoenix.Phx
{
	/// <remarks>Effect's <see cref="TriggerScriptObject.ID"/> is ignored by runtime (deprecated or editor only?)</remarks>
	public sealed class BTriggerEffect
		: TriggerScriptObjectWithArgs
	{
		#region Xml constants
		public const string kXmlRootName_OnTrue = "TriggerEffectsOnTrue";
		public const string kXmlRootName_OnFalse = "TriggerEffectsOnFalse";

		public static readonly XML.BListXmlParams kBListXmlParams = new XML.BListXmlParams
		{
			RootName = null,
			ElementName = "Effect",
			DataName = kXmlAttrType,
		};
		#endregion

		public override void Serialize<TDoc, TCursor>(IO.TagElementStream<TDoc, TCursor, string> s)
		{
			base.Serialize(s);
		}
	};
}