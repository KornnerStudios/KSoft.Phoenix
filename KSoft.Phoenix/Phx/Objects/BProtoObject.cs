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

		internal const string kXmlElementAttackGradeDPS = "AttackGradeDPS";
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

		public static bool SortCommandsAfterReading = false;
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

			s.StreamAttributeOpt("id", ref mId, Predicates.IsNotNone);
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
			s.StreamElementOpt("Hitpoints", ref mHitpoints, PhxPredicates.IsNotInvalid);
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
			s.StreamElementEnumOpt("ObjectClass", ref mClassType, x => x != BProtoObjectClassType.Invalid);
			//TrainerType
			//AutoLockDown
			//Cost
			s.StreamElementOpt("CostEscalation", ref mCostEscalation, PhxPredicates.IsNotInvalid);
			//CostEscalationObject
			//CaptureCost
			s.StreamElementOpt("Bounty", ref mBounty, PhxPredicates.IsNotInvalid);
			//AIAssetValueAdjust
			s.StreamElementOpt("CombatValue", ref mCombatValue, PhxPredicates.IsNotInvalid);
			//ResourceAmount
			//PlacementRules, PlacementRules file name (sans extension)
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
			XML.XmlUtil.Serialize(s, AddResource, BResource.kBListTypeValuesXmlParams_AddResource, "Amount");
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
			((XML.BDatabaseXmlSerializerBase)xs).StreamXmlTactic(s, "Tactics", this, ref mHasTactics);
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

			if (s.IsReading)
			{
				PostDeserialize();
			}
		}

		private void PostDeserialize()
		{
			if (SortCommandsAfterReading)
			{
				SortCommands();
			}
		}
		#endregion

		private void SortCommands()
		{
			Commands.Sort(CompareCommands);
		}

		private static int CompareCommands(BProtoObjectCommand x, BProtoObjectCommand y)
		{
			if (x.Position != y.Position)
				return x.Position.CompareTo(y.Position);

			if (x.CommandType != y.CommandType)
				return ((int)x.CommandType).CompareTo((int)y.CommandType);

			// assuming Proto upgrades are defined after earlier Protos
			return x.ID.CompareTo(y.ID);
		}
	};
}