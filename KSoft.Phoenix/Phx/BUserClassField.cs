
namespace KSoft.Phoenix.Phx
{
	public sealed class BUserClassField
		: Collections.BListAutoIdObject
	{
		#region Xml constants
		public static readonly XML.BListXmlParams kBListXmlParams = new XML.BListXmlParams()
		{
			ElementName = "Fields",
			DataName = "Name",
		};

		const string kXmlAttrType = "Type";
		#endregion

		BTriggerVarType mType;

		#region IXmlElementStreamable Members
		public override void Serialize<TDoc, TCursor>(IO.TagElementStream<TDoc, TCursor, string> s)
		{
			s.StreamAttributeEnum(kXmlAttrType, ref mType);
		}
		#endregion
	};
}