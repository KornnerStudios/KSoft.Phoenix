
namespace KSoft.Phoenix.Phx
{
	public sealed class BInfectionMap
		: IO.ITagElementStringNameStreamable
	{
		#region Xml constants
		public static readonly XML.BListXmlParams kBListXmlParams = new()
		{
			RootName = "InfectionMap",
			ElementName = "InfectionMapEntry",
		};
		#endregion

		[Meta.BProtoObjectReference]
		int mBaseObjectID = TypeExtensions.kNone;
		[Meta.BProtoObjectReference]
		int mInfectedObjectID = TypeExtensions.kNone;
		[Meta.BProtoSquadReference]
		int mInfectedSquadID = TypeExtensions.kNone;

		#region ITagElementStreamable<string> Members
		public void Serialize<TDoc, TCursor>(IO.TagElementStream<TDoc, TCursor, string> s)
			where TDoc : class
			where TCursor : class
		{
			var xs = s.GetSerializerInterface();

			xs.StreamDBID(s, "base", ref mBaseObjectID, DatabaseObjectKind.Object, false, XML.XmlUtil.kSourceAttr);
			xs.StreamDBID(s, "infected", ref mInfectedObjectID, DatabaseObjectKind.Object, false, XML.XmlUtil.kSourceAttr);
			if(xs.Database.Engine.Build != Engine.PhxEngineBuild.Alpha)
			{
				xs.StreamDBID(s, "infectedSquad", ref mInfectedSquadID, DatabaseObjectKind.Squad, false, XML.XmlUtil.kSourceAttr);
			}
		}
		#endregion
	};
}