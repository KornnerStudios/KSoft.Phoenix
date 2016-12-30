
namespace KSoft.Phoenix.Phx
{
	public sealed class BProtoSquad
		: DatabaseIdObject
	{
		#region Xml constants
		public static readonly XML.BListXmlParams kBListXmlParams = new XML.BListXmlParams("Squad")
		{
			DataName = DatabaseNamedObject.kXmlAttrName,
			Flags = //XML.BCollectionXmlParamsFlags.ToLowerDataNames |
				XML.BCollectionXmlParamsFlags.RequiresDataNamePreloading |
				XML.BCollectionXmlParamsFlags.SupportsUpdating
		};
		public static readonly Engine.XmlFileInfo kXmlFileInfo = new Engine.XmlFileInfo
		{
			Location = Engine.ContentStorage.Game,
			Directory = Engine.GameDirectory.Data,
			FileName = "Squads.xml",
			RootName = kBListXmlParams.RootName
		};
		public static readonly Engine.XmlFileInfo kXmlFileInfoUpdate = new Engine.XmlFileInfo
		{
			Location = Engine.ContentStorage.Update,
			Directory = Engine.GameDirectory.Data,
			FileName = "Squads_Update.xml",
			RootName = kBListXmlParams.RootName
		};

		static readonly Collections.CodeEnum<BProtoSquadFlags> kFlagsProtoEnum = new Collections.CodeEnum<BProtoSquadFlags>();
		static readonly Collections.BBitSetParams kFlagsParams = new Collections.BBitSetParams(() => kFlagsProtoEnum);

		const string kXmlElementCanAttackWhileMoving = "CanAttackWhileMoving";
		#endregion

		public Collections.BListArray<BProtoSquadUnit> Units { get; private set; }

		public Collections.BBitSet Flags { get; private set; }

		/// <summary>Is this Squad just made up of a single Unit?</summary>
		public bool SquadIsUnit { get {
			return Units.Count == 1 && Units[0].Count == 1;
		}}

		public BProtoSquad() : base(BResource.kBListTypeValuesParams, BResource.kBListTypeValuesXmlParams_CostLowercaseType)
		{
			Units = new Collections.BListArray<BProtoSquadUnit>();

			Flags = new Collections.BBitSet(kFlagsParams);
		}

		#region IXmlElementStreamable Members
		bool ShouldStreamUnits<TDoc, TCursor>(IO.TagElementStream<TDoc, TCursor, string> s)
			where TDoc : class
			where TCursor : class
		{
				 if (s.IsReading) return s.ElementsExists(BProtoSquadUnit.kBListXmlParams.RootName);
			else if (s.IsWriting) return !Units.IsEmpty;

			return false;
		}

		public override void Serialize<TDoc, TCursor>(IO.TagElementStream<TDoc, TCursor, string> s)
		{
			base.Serialize(s);

			//formationType
			//DisplayNameID
			//RolloverTextID
			//StatsNameID
			//PrereqTextID
			//RoleTextID
			//PortraitIcon
			//AltIcon
			//BuildPoints
			//Cost
			//Stance
			//TrainAnim
			//Selection
			//HPBar
			//VeterancyBar
			//AbilityRecoveryBar
			//BobbleHead
			//BuildingStrengthDisplay
			//CryoPoints
			//DazeResist
			//Birth
			if (ShouldStreamUnits(s))
				XML.XmlUtil.Serialize(s, Units, BProtoSquadUnit.kBListXmlParams);
			//SubSelectSort
			//TurnRadius
			//LeashDistance
			//LeashDeadzone
			//LeashRecallDelay
			//AggroDistance
			//MinimapScale
			XML.XmlUtil.Serialize(s, Flags, XML.BBitSetXmlParams.kFlagsSansRoot);
			//Level
			//TechLevel
			//Sound
			//RecoveringEffect
			//CanAttackWhileMoving
		}
		#endregion
	};
}