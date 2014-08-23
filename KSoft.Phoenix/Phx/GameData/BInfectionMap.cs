
namespace KSoft.Phoenix.Phx
{
	public sealed class BInfectionMap
		: IO.ITagElementStringNameStreamable
	{
		#region Xml constants
		public static readonly XML.BListXmlParams kBListXmlParams = new XML.BListXmlParams
		{
			RootName = "InfectionMap",
			ElementName = "InfectionMapEntry",
		};

		const string kXmlAttrBase = "base";
		const string kXmlAttrInfected = "infected";
		const string kXmlAttrInfectedSquad = "infectedSquad";
		#endregion

		int mBaseObjectID = TypeExtensions.kNone;
		int mInfectedObjectID = TypeExtensions.kNone;
		int mInfectedSquadID = TypeExtensions.kNone;

		#region ITagElementStreamable<string> Members
		public void Serialize<TDoc, TCursor>(IO.TagElementStream<TDoc, TCursor, string> s)
			where TDoc : class
			where TCursor : class
		{
			var xs = s.GetSerializerInterface();

			xs.StreamDBID(s, kXmlAttrBase, ref mBaseObjectID, DatabaseObjectKind.Object, false, XML.XmlUtil.kSourceAttr);
			xs.StreamDBID(s, kXmlAttrInfected, ref mInfectedObjectID, DatabaseObjectKind.Object, false, XML.XmlUtil.kSourceAttr);
			if(xs.Database.Engine.Build != Engine.PhxEngineBuild.Alpha)
				xs.StreamDBID(s, kXmlAttrInfectedSquad, ref mInfectedSquadID, DatabaseObjectKind.Squad, false, XML.XmlUtil.kSourceAttr);
		}
		#endregion
	};
}