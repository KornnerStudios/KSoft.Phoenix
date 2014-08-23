
namespace KSoft.Phoenix.Phx
{
	// TODO: Nothing in HW uses this
	public struct BProtoTechPrereqTypeCount
		: IO.ITagElementStringNameStreamable
	{
		#region Xml constants
		public static readonly XML.BListXmlParams kBListXmlParams = new XML.BListXmlParams
		{
			ElementName = "TypeCount",
		};

		const string kXmlAttrUnit = "unit"; // ProtoObject
		const string kXmlAttrOperator = "operator";
		const string kXmlAttrCount = "count";
		#endregion

		int mUnitID;
		BProtoTechTypeCountOperator mOperator;
		short mCount;

		#region ITagElementStreamable<string> Members
		public void Serialize<TDoc, TCursor>(IO.TagElementStream<TDoc, TCursor, string> s)
			where TDoc : class
			where TCursor : class
		{
			var xs = s.GetSerializerInterface();

			xs.StreamDBID(s, kXmlAttrUnit, ref mUnitID, DatabaseObjectKind.Object, false, XML.XmlUtil.kSourceAttr);
			if (!s.StreamAttributeEnumOpt(kXmlAttrOperator, ref mOperator, e => e != BProtoTechTypeCountOperator.None))
				mOperator = BProtoTechTypeCountOperator.None;
			if (!s.StreamAttributeOpt(kXmlAttrCount, ref mCount, Predicates.IsNotNone))
				mCount = TypeExtensions.kNone;
		}
		#endregion
	};
}