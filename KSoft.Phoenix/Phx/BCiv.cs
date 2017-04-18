
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
		public static readonly Engine.ProtoDataXmlFileInfo kProtoFileInfo = new Engine.ProtoDataXmlFileInfo(
			Engine.XmlFilePriority.ProtoData,
			kXmlFileInfo);
		#endregion

		#region IsExcludedFromAlpha
		int mAlpha = TypeExtensions.kNone;
		public bool IsExcludedFromAlpha
		{
			get { return mAlpha == 0; }
			set { mAlpha = value ? 0 : TypeExtensions.kNone; }
		}
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

		#region CommandAckObjectID
		int mCommandAckObjectID = TypeExtensions.kNone;
		[Meta.BProtoObjectReference]
		public int CommandAckObjectID
		{
			get { return mCommandAckObjectID; }
			set { mCommandAckObjectID = value; }
		}
		#endregion

		#region RallyPointObjectID
		int mRallyPointObjectID = TypeExtensions.kNone;
		[Meta.BProtoObjectReference]
		public int RallyPointObjectID
		{
			get { return mRallyPointObjectID; }
			set { mRallyPointObjectID = value; }
		}
		#endregion

		#region LocalRallyPointObjectID
		int mLocalRallyPointObjectID = TypeExtensions.kNone;
		[Meta.BProtoObjectReference]
		public int LocalRallyPointObjectID
		{
			get { return mLocalRallyPointObjectID; }
			set { mLocalRallyPointObjectID = value; }
		}
		#endregion

		#region TransportObjectID
		int mTransportObjectID = TypeExtensions.kNone;
		[Meta.BProtoObjectReference]
		public int TransportObjectID
		{
			get { return mTransportObjectID; }
			set { mTransportObjectID = value; }
		}
		#endregion

		#region TransportTriggerObjectID
		int mTransportTriggerObjectID = TypeExtensions.kNone;
		[Meta.BProtoObjectReference]
		public int TransportTriggerObjectID
		{
			get { return mTransportTriggerObjectID; }
			set { mTransportTriggerObjectID = value; }
		}
		#endregion

		#region HullExpansionRadius
		float mHullExpansionRadius;
		public float HullExpansionRadius
		{
			get { return mHullExpansionRadius; }
			set { mHullExpansionRadius = value; }
		}
		#endregion

		#region TerrainPushOffRadius
		float mTerrainPushOffRadius;
		public float TerrainPushOffRadius
		{
			get { return mTerrainPushOffRadius; }
			set { mTerrainPushOffRadius = value; }
		}
		#endregion

		#region BuildingMagnetRange
		float mBuildingMagnetRange;
		public float BuildingMagnetRange
		{
			get { return mBuildingMagnetRange; }
			set { mBuildingMagnetRange = value; }
		}
		#endregion

		#region SoundBank
		string mSoundBank;
		// .bnk
		public string SoundBank
		{
			get { return mSoundBank; }
			set { mSoundBank = value; }
		}
		#endregion

		#region LeaderMenuNameID
		int mLeaderMenuNameID = TypeExtensions.kNone;
		[Meta.LocStringReference]
		public int LeaderMenuNameID
		{
			get { return mLeaderMenuNameID; }
			set { mLeaderMenuNameID = value; }
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

		#region UIControlBackground
		string mUIControlBackground;
		[Meta.TextureReference]
		public string UIControlBackground
		{
			get { return mUIControlBackground; }
			set { mUIControlBackground = value; }
		}
		#endregion

		// Empty Civs just have a name
		public bool IsEmpty { get { return mTechID.IsNotNone(); } }

		public BCiv()
		{
			var textData = base.CreateDatabaseObjectUserInterfaceTextData();
			textData.HasDisplayNameID = true;
		}

		#region ITagElementStreamable<string> Members
		public override void Serialize<TDoc, TCursor>(IO.TagElementStream<TDoc, TCursor, string> s)
		{
			base.Serialize(s);

			var xs = s.GetSerializerInterface();

			s.StreamAttributeOpt("Alpha", ref mAlpha, Predicates.IsNotNone);
			xs.StreamDBID(s, "CivTech", ref mTechID, DatabaseObjectKind.Tech);
			xs.StreamDBID(s, "CommandAckObject", ref mCommandAckObjectID, DatabaseObjectKind.Object);
			xs.StreamDBID(s, "RallyPointObject", ref mRallyPointObjectID, DatabaseObjectKind.Object);
			xs.StreamDBID(s, "LocalRallyPointObject", ref mLocalRallyPointObjectID, DatabaseObjectKind.Object);
			xs.StreamDBID(s, "Transport", ref mTransportObjectID, DatabaseObjectKind.Object);
			xs.StreamDBID(s, "TransportTrigger", ref mTransportTriggerObjectID, DatabaseObjectKind.Object);
			s.StreamElementOpt("ExpandHull", ref mHullExpansionRadius, Predicates.IsNotZero);
			s.StreamElementOpt("TerrainPushOff", ref mTerrainPushOffRadius, Predicates.IsNotZero);
			s.StreamElementOpt("BuildingMagnetRange", ref mBuildingMagnetRange, Predicates.IsNotZero);
			s.StreamStringOpt("SoundBank", ref mSoundBank, toLower: false, type: XML.XmlUtil.kSourceElement);
			xs.StreamStringID(s, "LeaderMenuNameID", ref mLeaderMenuNameID);
			s.StreamElementOpt("PowerFromHero", ref mPowerFromHero, Predicates.IsTrue);
			s.StreamStringOpt("UIControlBackground", ref mUIControlBackground, toLower: false, type: XML.XmlUtil.kSourceElement);
		}
		#endregion
	};
}