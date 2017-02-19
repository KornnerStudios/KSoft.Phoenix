
namespace KSoft.Phoenix.Phx
{
	public sealed class BCiv
		: DatabaseNamedObject
	{
		#region Xml constants
		public static readonly XML.BListXmlParams kBListXmlParams = new XML.BListXmlParams("Civ")
		{
			DataName = "Name",
			Flags = XML.BCollectionXmlParamsFlags.UseElementForData
		};
		public static readonly Engine.XmlFileInfo kXmlFileInfo = new Engine.XmlFileInfo
		{
			Directory = Engine.GameDirectory.Data,
			FileName = "Civs.xml",
			RootName = kBListXmlParams.RootName
		};
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

		#region PowerFromHero
		bool mPowerFromHero;
		public bool PowerFromHero
		{
			get { return mPowerFromHero; }
			set { mPowerFromHero = value; }
		}
		#endregion

		// Empty Civs just have a name
		public bool IsEmpty { get { return mTechID.IsNotNone(); } }

		#region ITagElementStreamable<string> Members
		public override void Serialize<TDoc, TCursor>(IO.TagElementStream<TDoc, TCursor, string> s)
		{
			base.Serialize(s);

			var xs = s.GetSerializerInterface();

			//Alpha (unused in code)
			xs.StreamDBID(s, "CivTech", ref mTechID, DatabaseObjectKind.Tech);
			//CommandAckObject
			//RallyPointObject
			//LocalRallyPointObject
			//Transport
			//TransportTrigger
			//ExpandHull
			//TerrainPushOff
			//BuildingMagnetRange
			//SoundBank
			//LeaderMenuNameID
			s.StreamElementOpt("PowerFromHero", ref mPowerFromHero, Predicates.IsTrue);
			//UIControlBackground
		}
		#endregion
	};
}