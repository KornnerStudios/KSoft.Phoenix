using System;

namespace KSoft.Phoenix.Phx
{
	//ChildObjectType
	//ParkingLot
	//Socket
	//Rally
	//OneTimeSpawnSquad, inner text is a proto squad, not proto object
	//Unit
	//Foundation
	//ChildObject.UserCiv
	//ChildObject.AttachBone (string)
	//ChildObject.Rotation (float)

	public sealed class BProtoObject
		: DatabaseIdObject
	{
		#region Xml constants
		public static readonly XML.BListXmlParams kBListXmlParams = new XML.BListXmlParams("Object")
		{
			DataName = DatabaseNamedObject.kXmlAttrName,
			Flags = XML.BCollectionXmlParamsFlags.ToLowerDataNames |
				XML.BCollectionXmlParamsFlags.RequiresDataNamePreloading |
				XML.BCollectionXmlParamsFlags.SupportsUpdating
		};
		public static readonly Engine.XmlFileInfo kXmlFileInfo = new Engine.XmlFileInfo
		{
			Location = Engine.ContentStorage.Game,
			Directory = Engine.GameDirectory.Data,
			FileName = "Objects.xml",
			RootName = kBListXmlParams.RootName
		};
		public static readonly Engine.XmlFileInfo kXmlFileInfoUpdate = new Engine.XmlFileInfo
		{
			Location = Engine.ContentStorage.Update,
			Directory = Engine.GameDirectory.Data,
			FileName = "Objects_Update.xml",
			RootName = kBListXmlParams.RootName
		};

		static readonly Collections.CodeEnum<BProtoObjectFlags> kFlagsProtoEnum = new Collections.CodeEnum<BProtoObjectFlags>();
		static readonly Collections.BBitSetParams kFlagsParams = new Collections.BBitSetParams(() => kFlagsProtoEnum);

		static readonly Collections.BBitSetParams kObjectTypesParams = new Collections.BBitSetParams(db => db.ObjectTypes);
		static readonly XML.BBitSetXmlParams kObjectTypesXmlParams = new XML.BBitSetXmlParams("ObjectType");

		const string kXmlAttrIs = "is"; // boolean int, only streamed when '0', only used by tools?
		const string kXmlAttrId = "id";

		const string kXmlElementObjectClass = "ObjectClass";

		const string kXmlElementCostEscalation = "CostEscalation";
		const string kXmlElementHitpoints = "Hitpoints";
		const string kXmlElementShieldpoints = "Shieldpoints";
		internal const string kXmlElementAttackGradeDPS = "AttackGradeDPS";
		const string kXmlElementCombatValue = "CombatValue";
		const string kXmlElementBounty = "Bounty";

		const string kXmlElementTactics = "Tactics";

		const string kXmlElementAddRsrcAttrAmount = "Amount";

		const string kXmlElementPlacementRules = "PlacementRules"; // PlacementRules file name (sans extension)

		const string kXmlElementCommand = "Command";
		#endregion

		int mId = TypeExtensions.kNone;
		public int Id { get { return mId; } }

		BProtoObjectClassType mClassType;
		public BProtoObjectClassType ClassType { get { return mClassType; } }

		public Collections.BListExplicitIndex<BProtoObjectVeterancy> Veterancy { get; private set; }

		float mCostEscalation = PhxUtil.kInvalidSingle;
		/// <summary>see: UNSC reactors</summary>
		// Also, CostEscalationObject and Flag.LinearCostEscalation
		public float CostEscalation { get { return mCostEscalation; } }
		public bool HasCostEscalation { get { return PhxPredicates.IsNotInvalid(CostEscalation); } }

		float mHitpoints = PhxUtil.kInvalidSingle;
		public float Hitpoints { get { return mHitpoints; } }
		float mShieldpoints = PhxUtil.kInvalidSingle;
		public float Shieldpoints { get { return mShieldpoints; } }
		float mAttackGradeDPS = PhxUtil.kInvalidSingle;
		public float AttackGradeDPS { get { return mAttackGradeDPS; } }
		float mCombatValue = PhxUtil.kInvalidSingle;
		/// <summary>Score value</summary>
		public float CombatValue { get { return mCombatValue; } }
		float mBounty = PhxUtil.kInvalidSingle;
		/// <summary>Vet XP contribution value</summary>
		public float Bounty { get { return mBounty; } }
		public Collections.BTypeValuesSingle Populations { get; private set; }
		public Collections.BTypeValuesSingle PopulationsCapAddition { get; private set; }

		internal bool mHasTactics;

		public Collections.BTypeValuesSingle Rates { get; private set; }
		public Collections.BTypeValuesSingle AddResource { get; private set; }

		public Collections.BBitSet Flags { get; private set; }
		public Collections.BBitSet ObjectTypes { get; private set; }

		public Collections.BListArray<BProtoObjectCommand> Commands { get; private set; }

		public BProtoObject() : base(BResource.kBListTypeValuesParams, BResource.kBListTypeValuesXmlParams_Cost)
		{
			Veterancy = new Collections.BListExplicitIndex<BProtoObjectVeterancy>(BProtoObjectVeterancy.kBListExplicitIndexParams);

			Populations = new Collections.BTypeValuesSingle(BPopulation.kBListParamsSingle);
			PopulationsCapAddition = new Collections.BTypeValuesSingle(BPopulation.kBListParamsSingle);

			Rates = new Collections.BTypeValuesSingle(BGameData.kRatesBListTypeValuesParams);
			AddResource = new Collections.BTypeValuesSingle(BResource.kBListTypeValuesParams);

			Flags = new Collections.BBitSet(kFlagsParams);
			ObjectTypes = new Collections.BBitSet(kObjectTypesParams);

			Commands = new Collections.BListArray<BProtoObjectCommand>();
		}

		#region ITagElementStreamable<string> Members
		public override void Serialize<TDoc, TCursor>(IO.TagElementStream<TDoc, TCursor, string> s)
		{
			base.Serialize(s);

			var xs = s.GetSerializerInterface();

			s.StreamAttributeOpt(kXmlAttrId, ref mId, Predicates.IsNotNone);
			//MovementType
			//Hardpoint
			//SingleBoneIK
			//GroundIK
			//SweetSpotIK
			//ObstructionRadiusX,ObstructionRadiusY,ObstructionRadiusZ
			//FlattenMinX0,FlattenMaxX0,FlattenMinZ0,FlattenMaxZ0,FlattenMinX1,FlattenMaxX1,FlattenMinZ1,FlattenMaxZ1
			//ParkingMinX,ParkingMaxX,ParkingMinZ,ParkingMaxZ
			//TerrainHeightTolerance
			//PhysicsInfo
			//PhysicsReplacementInfo
			//Velocity
			//MaxVelocity
			//ReverseSpeed
			//Acceleration
			//TrackingDelay
			//StartingVelocity
			//Fuel
			//PerturbanceChance
			//PerturbanceVelocity
			//PerturbanceMinTime
			//PerturbanceMaxTime
			//PerturbInitialVelocity
			//ActiveScanChance
			//TurnRate
			s.StreamElementOpt(kXmlElementHitpoints, ref mHitpoints, PhxPredicates.IsNotInvalid);
			//Shieldpoints
			//LOS
			//PickRadius
			//PickOffset
			//PickPriority
			//SelectType
			//GotoType
			//SelectedRadiusX
			//SelectedRadiusZ
			//BuildPoints
			//RepairPoints
			s.StreamElementEnumOpt(kXmlElementObjectClass, ref mClassType, x => x != BProtoObjectClassType.Invalid);
			//TrainerType
			//AutoLockDown
			//Cost
			s.StreamElementOpt(kXmlElementCostEscalation, ref mCostEscalation, PhxPredicates.IsNotInvalid);
			//CostEscalationObject
			//CaptureCost
			s.StreamElementOpt(kXmlElementBounty, ref mBounty, PhxPredicates.IsNotInvalid);
			//AIAssetValueAdjust
			s.StreamElementOpt(kXmlElementCombatValue, ref mCombatValue, PhxPredicates.IsNotInvalid);
			//ResourceAmount
			//PlacementRules
			//DeathFadeTime
			//DeathFadeDelayTime
			//TrainAnim
			//SquadModeAnim
			//RallyPoint
			//MaxProjectileHeight
			//GroundIKTilt
			//DeathReplacement
			//DeathSpawnSquad
			//SurfaceType
			//DisplayNameID
			//RolloverTextID
			//StatsNameID
			//GaiaRolloverTextID
			//EnemyRolloverTextID
			//PrereqTextID
			//RoleTextID
			//Visual
			//CorpseDeath
			//AbilityCommand
			//Power
			//Ability
			XML.XmlUtil.Serialize(s, Veterancy, BProtoObjectVeterancy.kBListExplicitIndexXmlParams);
			XML.XmlUtil.Serialize(s, AddResource, BResource.kBListTypeValuesXmlParams_AddResource, kXmlElementAddRsrcAttrAmount);
			//ExistSound
			//GathererLimit
			//BlockMovementObject
			//Lifespan
			//AmmoMax
			//AmmoRegenRate
			//NumConversions
			//NumStasisFieldsToStop
			XML.XmlUtil.Serialize(s, Flags, XML.BBitSetXmlParams.kFlagsSansRoot);
			XML.XmlUtil.Serialize(s, ObjectTypes, kObjectTypesXmlParams);
			//DamageType
			//Sound
			//ImpactDecal
			//ExtendedSoundBank
			//PortraitIcon
			//MinimapIcon
			//MinimapColor
			XML.XmlUtil.Serialize(s, Commands, BTargetPriority.kBListXmlParams);
			//TrainLimit
			//GatherLink
			//ChildObjects
			XML.XmlUtil.Serialize(s, Populations, BPopulation.kBListXmlParamsSingle);
			XML.XmlUtil.Serialize(s, PopulationsCapAddition, BPopulation.kBListXmlParamsSingle_CapAddition);
			(xs as XML.BDatabaseXmlSerializerBase).StreamXmlTactic(s, kXmlElementTactics, this, ref mHasTactics);
			//FlightLevel
			//ExitFromDirection
			//HPBar
			//HitZone
			//BeamHead
			//BeamTail
			//Level
			//LevelUpEffect
			//RecoveringEffect
			//AutoTrainOnBuilt
			//Socket
			XML.XmlUtil.Serialize(s, Rates, BGameData.kRatesBListTypeValuesXmlParams);
			//MaxContained
			//MaxFlameEffects
			//Contain
			//GarrisonSquadMode
			//BuildStatsObject
			//SubSelectSort
			s.StreamElementOpt(kXmlElementAttackGradeDPS, ref mAttackGradeDPS, PhxPredicates.IsNotInvalid);
			//RamDodgeFactor
			//HoveringRumble
			//VisualDisplayPriority
			//ChildObjectDamageTakenScalar
			//TrueLOSHeight
			//GarrisonTime
			//BuildRotation
			//BuildOffset
			//AutoParkingLot
			//BuildingStrengthDisplay
			//ShieldType
			//RevealRadius
			//TargetBeam
			//KillBeam
		}
		#endregion
	};
}