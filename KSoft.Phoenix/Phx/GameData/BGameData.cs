
namespace KSoft.Phoenix.Phx
{
	public sealed class BGameData
		: IO.ITagElementStringNameStreamable
	{
		#region Xml constants
		const string kXmlRoot = "GameData";

		public static readonly Engine.XmlFileInfo kXmlFileInfo = new Engine.XmlFileInfo
		{
			Directory = Engine.GameDirectory.Data,
			FileName = "GameData.xml",
			RootName = kXmlRoot
		};

		public static readonly Collections.BTypeValuesParams<float> kRatesBListTypeValuesParams = new
			Collections.BTypeValuesParams<float>(db => db.GameData.Rates) { kTypeGetInvalid = PhxUtil.kGetInvalidSingle };
		static readonly XML.BListXmlParams kRatesXmlParams = new XML.BListXmlParams("Rate");
		public static readonly XML.BTypeValuesXmlParams<float> kRatesBListTypeValuesXmlParams = new
			XML.BTypeValuesXmlParams<float>("Rate", "Rate"); // oiy, really? name the 'type' attribute with the same name as the element?

		const string kXmlElementDifficultyEasy = "DifficultyEasy";					// 59A0
		const string kXmlElementDifficultyNormal = "DifficultyNormal";				// 59A4
		const string kXmlElementDifficultyHard = "DifficultyHard";					// 59A8
		const string kXmlElementDifficultyLegendary = "DifficultyLegendary";		// 59AC
		const string kXmlElementDifficultyDefault = "DifficultyDefault";			// 59B0
		const string kXmlElementDifficultySPCAIDefault = "DifficultySPCAIDefault";	// 59B4
		static readonly XML.BListXmlParams kPopsXmlParams = new XML.BListXmlParams("Pop");			// 28D8
		static readonly XML.BListXmlParams kRefCountsXmlParams = new XML.BListXmlParams("RefCount");// 30F8
		static readonly XML.BListXmlParams kHUDItemsXmlParams = new XML.BListXmlParams("HUDItem");
		static readonly XML.BListXmlParams kFlashableItemsXmlParams = new XML.BListXmlParams
		{
			RootName = "FlashableItems",
			ElementName = "Item",
			Flags = XML.BCollectionXmlParamsFlags.UseInnerTextForData,
		};
		static readonly XML.BListXmlParams kUnitFlagsXmlParams = new XML.BListXmlParams("UnitFlag");
		static readonly XML.BListXmlParams kSquadFlagsXmlParams = new XML.BListXmlParams("SquadFlag");

		static readonly Collections.BTypeValuesParams<string> kCodeProtoObjectsParams = new Collections.BTypeValuesParams<string>(db => db.GameProtoObjectTypes);
		static readonly XML.BTypeValuesXmlParams<string> kCodeProtoObjectsXmlParams = new XML.BTypeValuesXmlParams<string>("CodeProtoObject", "Type",
			XML.BCollectionXmlParamsFlags.ToLowerDataNames)
		{
			RootName = "CodeProtoObjects",
		};
		static readonly Collections.BTypeValuesParams<string> kCodeObjectTypesParams = new Collections.BTypeValuesParams<string>(db => db.GameObjectTypes);
		static readonly XML.BTypeValuesXmlParams<string> kCodeObjectTypesXmlParams = new XML.BTypeValuesXmlParams<string>("CodeObjectType", "Type")
		{
			RootName = "CodeObjectTypes",
		};

		const string kXmlElementGarrisonDamageMultiplier = "GarrisonDamageMultiplier";
		const string kXmlElementConstructionDamageMultiplier = "ConstructionDamageMultiplier";
		//
		const string kXmlElementShieldRegenDelay = "ShieldRegenDelay";
		const string kXmlElementShieldRegenTime = "ShieldRegenTime";
		//
		const string kXmlElementAttackRatingMultiplier = "AttackRatingMultiplier";
		const string kXmlElementDefenseRatingMultiplier = "DefenseRatingMultiplier";
		const string kXmlElementGoodAgainstMinAttackRating = "GoodAgainstMinAttackRating";
		//
		const string kXmlElementChanceToRocket = "ChanceToRocket";
		//
		const string kXmlElementTributeAmount = "TributeAmount";
		const string kXmlElementTributeCost = "TributeCost";
		//
		const string kXmlElementDamageReceivedXPFactor = "DamageReceivedXPFactor";
		//
		const string kXmlElementHeroDownedLOS = "HeroDownedLOS";// 59B8
		const string kXmlElementHeroHPRegenTime = "HeroHPRegenTime";// 59BC
		const string kXmlElementHeroRevivalDistance = "HeroRevivalDistance";// 59C0
		const string kXmlElementHeroPercentHPRevivalThreshhold = "HeroPercentHPRevivalThreshhold";// 59C4
		const string kXmlElementMaxDeadHeroTransportDist = "MaxDeadHeroTransportDist";// 59C8
		const string kXmlElementTransportClearRadiusScale = "TransportClearRadiusScale";// 59CC
		const string kXmlElementTransportMaxSearchRadiusScale = "TransportMaxSearchRadiusScale";// 59D0
		const string kXmlElementTransportMaxSearchLocations = "TransportMaxSearchLocations";// 59D4
		const string kXmlElementTransportBlockTime = "TransportBlockTime";// 59D8
		const string kXmlElementTransportLoadBlockTime = "TransportLoadBlockTime";// 59DC

		static readonly XML.BListXmlParams kPlayerStatesXmlParams = new XML.BListXmlParams("PlayerState"); // 3918
		#endregion

		public Collections.BListAutoId<BResource> Resources { get; private set; }
		public Collections.BTypeNames Rates { get; private set; }
		public Collections.BTypeNames Populations { get; private set; }
		public Collections.BTypeNames RefCounts { get; private set; }
		#region Nonsense
		/// <remarks>Engine doesn't process these, but some trigger scripts use these dynamic types, so keep them on record</remarks>
		public Collections.BTypeNames HUDItems { get; private set; }
		/// <remarks>Engine doesn't process these, but some trigger scripts use these dynamic types, so keep them on record</remarks>
		public Collections.BTypeNames FlashableItems { get; private set; }
		/// <remarks>Engine doesn't process these, but some trigger scripts use these dynamic types, so keep them on record</remarks>
		public Collections.BTypeNames UnitFlags { get; private set; }
		/// <remarks>Engine doesn't process these, but some trigger scripts use these dynamic types, so keep them on record</remarks>
		public Collections.BTypeNames SquadFlags { get; private set; }
		#endregion
		public Collections.BTypeValuesString CodeProtoObjects { get; private set; }
		public Collections.BTypeValuesString CodeObjectTypes { get; private set; }

		#region Misc values
		float mGarrisonDamageMultiplier = PhxUtil.kInvalidSingle;
		public float GarrisonDamageMultiplier { get { return mGarrisonDamageMultiplier; } }
		float mConstructionDamageMultiplier = PhxUtil.kInvalidSingle;
		public float ConstructionDamageMultiplier { get { return mConstructionDamageMultiplier; } }
		//
		float mShieldRegenDelay = PhxUtil.kInvalidSingle;
		public float ShieldRegenDelay { get { return mShieldRegenDelay; } }
		float mShieldRegenTime = PhxUtil.kInvalidSingle;
		public float ShieldRegenTime { get { return mShieldRegenTime; } }
		//
		float mAttackRatingMultiplier = PhxUtil.kInvalidSingle;
		public float AttackRatingMultiplier { get { return mAttackRatingMultiplier; } }
		float mDefenseRatingMultiplier = PhxUtil.kInvalidSingle;
		public float DefenseRatingMultiplier { get { return mDefenseRatingMultiplier; } }
		float mGoodAgainstMinAttackRating = PhxUtil.kInvalidSingle;
		public float GoodAgainstMinAttackRating { get { return mGoodAgainstMinAttackRating; } }
		//
		float mChanceToRocket = PhxUtil.kInvalidSingle;
		public float ChanceToRocket { get { return mChanceToRocket; } }
		//
		float mTributeAmount = PhxUtil.kInvalidSingle;
		public float TributeAmount { get { return mTributeAmount; } }
		float mTributeCost = PhxUtil.kInvalidSingle;
		public float TributeCost { get { return mTributeCost; } }
		//
		public Collections.BListArray<BInfectionMap> InfectionMap { get; private set; }
		//
		public float mDamageReceivedXPFactor = PhxUtil.kInvalidSingle;
		public float DamageReceivedXPFactor { get { return mDamageReceivedXPFactor; } }
		#endregion

		public Collections.BTypeNames PlayerStates { get; private set; }

		/// <summary>Get how much it costs, in total, to tribute a resource to another player</summary>
		public float TotalTributeCost { get { return (mTributeAmount * mTributeCost) + mTributeAmount; } }

		public BGameData()
		{
			Resources = new Collections.BListAutoId<BResource>();
			Rates = new Collections.BTypeNames();
			Populations = new Collections.BTypeNames();
			RefCounts = new Collections.BTypeNames();
			#region Nonsense
			HUDItems = new Collections.BTypeNames();
			FlashableItems = new Collections.BTypeNames();
			UnitFlags = new Collections.BTypeNames();
			SquadFlags = new Collections.BTypeNames();
			#endregion
			CodeProtoObjects = new Collections.BTypeValuesString(kCodeProtoObjectsParams);
			CodeObjectTypes = new Collections.BTypeValuesString(kCodeObjectTypesParams);

			InfectionMap = new Collections.BListArray<BInfectionMap>();

			PlayerStates = new Collections.BTypeNames();
		}

		#region ITagElementStreamable<string> Members
		/// <remarks>For streaming directly from gamedata.xml</remarks>
		internal void StreamGameData<TDoc, TCursor>(IO.TagElementStream<TDoc, TCursor, string> s)
			where TDoc : class
			where TCursor : class
		{
			XML.XmlUtil.Serialize(s, Resources, BResource.kBListXmlParams);
			XML.XmlUtil.Serialize(s, Rates, kRatesXmlParams);
			XML.XmlUtil.Serialize(s, Populations, kPopsXmlParams);
			XML.XmlUtil.Serialize(s, RefCounts, kRefCountsXmlParams);
			#region Nonsense
			XML.XmlUtil.Serialize(s, HUDItems, kHUDItemsXmlParams);
			XML.XmlUtil.Serialize(s,FlashableItems, kFlashableItemsXmlParams);
			XML.XmlUtil.Serialize(s, UnitFlags, kUnitFlagsXmlParams);
			XML.XmlUtil.Serialize(s, SquadFlags, kSquadFlagsXmlParams);
			#endregion
			XML.XmlUtil.Serialize(s, CodeProtoObjects, kCodeProtoObjectsXmlParams);
			XML.XmlUtil.Serialize(s, CodeObjectTypes, kCodeObjectTypesXmlParams);

			#region Misc values
			s.StreamElementOpt(kXmlElementGarrisonDamageMultiplier, ref mGarrisonDamageMultiplier, PhxPredicates.IsNotInvalid);
			s.StreamElementOpt(kXmlElementConstructionDamageMultiplier, ref mConstructionDamageMultiplier, PhxPredicates.IsNotInvalid);
			//
			s.StreamElementOpt(kXmlElementShieldRegenDelay, ref mShieldRegenDelay, PhxPredicates.IsNotInvalid);
			s.StreamElementOpt(kXmlElementShieldRegenTime, ref mShieldRegenTime, PhxPredicates.IsNotInvalid);
			//
			s.StreamElementOpt(kXmlElementAttackRatingMultiplier, ref mAttackRatingMultiplier, PhxPredicates.IsNotInvalid);
			s.StreamElementOpt(kXmlElementDefenseRatingMultiplier, ref mDefenseRatingMultiplier, PhxPredicates.IsNotInvalid);
			s.StreamElementOpt(kXmlElementGoodAgainstMinAttackRating, ref mGoodAgainstMinAttackRating, PhxPredicates.IsNotInvalid);
			//
			s.StreamElementOpt(kXmlElementChanceToRocket, ref mChanceToRocket, PhxPredicates.IsNotInvalid);
			//
			s.StreamElementOpt(kXmlElementTributeAmount, ref mTributeAmount, PhxPredicates.IsNotInvalid);
			s.StreamElementOpt(kXmlElementTributeCost, ref mTributeCost, PhxPredicates.IsNotInvalid);
			//
			XML.XmlUtil.Serialize(s, InfectionMap, BInfectionMap.kBListXmlParams);
			//
			s.StreamElementOpt(kXmlElementDamageReceivedXPFactor, ref mDamageReceivedXPFactor, PhxPredicates.IsNotInvalid);
			#endregion

			XML.XmlUtil.Serialize(s, PlayerStates, kPlayerStatesXmlParams);
		}
		public void Serialize<TDoc, TCursor>(IO.TagElementStream<TDoc, TCursor, string> s)
			where TDoc : class
			where TCursor : class
		{
			using (s.EnterCursorBookmark(kXmlRoot))
				StreamGameData(s);
		}
		#endregion
	};
}