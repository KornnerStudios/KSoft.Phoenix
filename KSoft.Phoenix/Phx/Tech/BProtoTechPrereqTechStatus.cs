
namespace KSoft.Phoenix.Phx
{
	public struct BProtoTechPrereqTechStatus
		: IO.ITagElementStringNameStreamable
	{
		#region Xml constants
		public static readonly XML.BListXmlParams kBListXmlParams = new XML.BListXmlParams
		{
			ElementName = "TechStatus",
		};

		// Not actually parsed by the engine
		const string kXmlAttrOperator = "status";
		#endregion

		BProtoTechStatus mTechStatus;// = BProtoTechStatus.Invalid;
		int mTechID;// = TypeExtensions.kNone;

		#region ITagElementStreamable<string> Members
		public void Serialize<TDoc, TCursor>(IO.TagElementStream<TDoc, TCursor, string> s)
			where TDoc : class
			where TCursor : class
		{
			var xs = s.GetSerializerInterface();

			s.StreamAttributeEnum(kXmlAttrOperator, ref mTechStatus);
			xs.StreamDBID(s, /*xmlName:*/null, ref mTechID, DatabaseObjectKind.Object, false, XML.XmlUtil.kSourceCursor);
		}
		#endregion
	};
}