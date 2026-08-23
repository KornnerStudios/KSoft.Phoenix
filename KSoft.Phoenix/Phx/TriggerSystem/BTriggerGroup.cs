
namespace KSoft.Phoenix.Phx
{
	public sealed class BTriggerGroup
		: TriggerScriptIdObject
	{
		#region Xml constants
		public static readonly XML.BListXmlParams kBListXmlParams = new()
		{
			RootName = "TriggerGroups",
			ElementName = "Group",
			DataName = DatabaseNamedObject.kXmlAttrNameN,
		};

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields", Justification = "Retains XML schema reference data.")]
		const string kXmlAttrId = "ID";
		#endregion

		//string mValue;

		public override void Serialize<TDoc, TCursor>(IO.TagElementStream<TDoc, TCursor, string> s)
		{
			base.Serialize(s);

			//s.StreamCursor(mode, ref mValue);
		}
	};
}