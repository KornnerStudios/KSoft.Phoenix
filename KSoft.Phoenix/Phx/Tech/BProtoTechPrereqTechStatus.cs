
namespace KSoft.Phoenix.Phx
{
	public sealed class BProtoTechPrereqTechStatus
		: IO.ITagElementStringNameStreamable
	{
		#region Xml constants
		public static readonly XML.BListXmlParams kBListXmlParams = new XML.BListXmlParams
		{
			ElementName = "TechStatus",
		};
		#endregion

		#region TechStatus
		BProtoTechStatus mTechStatus = BProtoTechStatus.Invalid;
		[Meta.UnusedData("Not actually parsed by the engine")]
		public BProtoTechStatus TechStatus
		{
			get { return mTechStatus; }
			set { mTechStatus = value; }
		}

		static System.Predicate<BProtoTechStatus> BProtoTechStatusIsNotInvalid = (BProtoTechStatus v) => v != BProtoTechStatus.Invalid;
		#endregion

		#region TechID
		int mTechID = TypeExtensions.kNone;
		[Meta.BProtoTechReference]
		public int TechID
		{
			get { return mTechID; }
			set { mTechID = value; }
		}
		#endregion

		#region ITagElementStreamable<string> Members
		public void Serialize<TDoc, TCursor>(IO.TagElementStream<TDoc, TCursor, string> s)
			where TDoc : class
			where TCursor : class
		{
			var xs = s.GetSerializerInterface();

			s.StreamAttributeEnumOpt("status", ref mTechStatus, BProtoTechStatusIsNotInvalid);
			xs.StreamDBID(s, XML.XmlUtil.kNoXmlName, ref mTechID, DatabaseObjectKind.Tech, false, XML.XmlUtil.kSourceCursor);
		}
		#endregion
	};
}