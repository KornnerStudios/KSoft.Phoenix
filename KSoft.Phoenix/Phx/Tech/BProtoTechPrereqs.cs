
namespace KSoft.Phoenix.Phx
{
	public sealed class BProtoTechPrereqs
		: IO.ITagElementStringNameStreamable
	{
		#region Xml constants
		public const string kXmlRootName = "Prereqs";
		#endregion

		public Collections.BListArray<BProtoTechPrereqTechStatus> TechStatus { get; private set; }
		public Collections.BListArray<BProtoTechPrereqTypeCount> TypeCounts { get; private set; }

		public BProtoTechPrereqs()
		{
			TechStatus = new Collections.BListArray<BProtoTechPrereqTechStatus>();
			TypeCounts = new Collections.BListArray<BProtoTechPrereqTypeCount>();
		}

		#region ITagElementStreamable<string> Members
		public void Serialize<TDoc, TCursor>(IO.TagElementStream<TDoc, TCursor, string> s)
			where TDoc : class
			where TCursor : class
		{
			XML.XmlUtil.Serialize(s, TechStatus, BProtoTechPrereqTechStatus.kBListXmlParams);
			XML.XmlUtil.Serialize(s, TypeCounts, BProtoTechPrereqTypeCount.kBListXmlParams);
		}
		#endregion
	};
}