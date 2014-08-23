
namespace KSoft.Phoenix.Phx
{
	public struct BProtoSquadUnit
		: IO.ITagElementStringNameStreamable
	{
		#region Xml constants
		public static readonly XML.BListXmlParams kBListXmlParams = new XML.BListXmlParams("Unit")
		{
			Flags = 0
		};

		const string kXmlAttrCount = "count";
		#endregion

		int mCount;
		public int Count { get { return mCount; } }
		int mUnitID;
		public int UnitID { get { return mUnitID; } }

		#region ITagElementStreamable<string> Members
		public void Serialize<TDoc, TCursor>(IO.TagElementStream<TDoc, TCursor, string> s)
			where TDoc : class
			where TCursor : class
		{
			var xs = s.GetSerializerInterface();

			s.StreamAttribute(kXmlAttrCount, ref mCount);
			xs.StreamDBID(s, /*xmlName:*/null, ref mUnitID, DatabaseObjectKind.Object, false, XML.XmlUtil.kSourceCursor);
		}
		#endregion
	};
}