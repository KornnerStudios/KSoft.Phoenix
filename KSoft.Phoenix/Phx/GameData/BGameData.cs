using System;

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
		static readonly XML.BListXmlParams kPlayerStatesXmlParams = new XML.BListXmlParams("PlayerState");
		static readonly XML.BListXmlParams kPopsXmlParams = new XML.BListXmlParams("Pop");
		static readonly XML.BListXmlParams kRefCountsXmlParams = new XML.BListXmlParams("RefCount");
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
		#endregion

		public Collections.BListAutoId<BResource> Resources { get; private set; }
		public Collections.BTypeNames Rates { get; private set; }

		#region GoodAgainstGrades
		uint[] mGoodAgainstGrades = new uint[(int)ReticleAttackGrade.kNumberOf];
		public uint[] GoodAgainstGrades { get { return mGoodAgainstGrades; } }
		#endregion

		#region DifficultyModifiers
		float[] mDifficultyModifiers = new float[(int)BDifficultyTypeModifier.kNumberOf];
		public float[] DifficultyModifiers { get { return mDifficultyModifiers; } }
		#endregion

		public Collections.BTypeNames Populations { get; private set; }
		public Collections.BTypeNames RefCounts { get; private set; }
		public Collections.BTypeNames PlayerStates { get; private set; }

		#region GarrisonDamageMultiplier
		float mGarrisonDamageMultiplier = 1.0f;
		public float GarrisonDamageMultiplier
		{
			get { return mGarrisonDamageMultiplier; }
			set { mGarrisonDamageMultiplier = value; }
		}
		#endregion

		#region ConstructionDamageMultiplier
		float mConstructionDamageMultiplier = 1.0f;
		public float ConstructionDamageMultiplier
		{
			get { return mConstructionDamageMultiplier; }
			set { mConstructionDamageMultiplier = value; }
		}
		#endregion

		#region CaptureDecayRate
		float mCaptureDecayRate;
		public float CaptureDecayRate
		{
			get { return mCaptureDecayRate; }
			set { mCaptureDecayRate = value; }
		}
		#endregion

		#region SquadLeashLength
		float mSquadLeashLength;
		public float SquadLeashLength
		{
			get { return mSquadLeashLength; }
			set { mSquadLeashLength = value; }
		}
		#endregion

		#region SquadAggroLength
		float mSquadAggroLength;
		public float SquadAggroLength
		{
			get { return mSquadAggroLength; }
			set { mSquadAggroLength = value; }
		}

		/// <summary>Engine clamps aggro length to leash length</summary>
		public bool SquadAggroLengthIsLessThanLeash { get { return SquadAggroLength < SquadLeashLength; } }
		#endregion

		#region UnitLeashLength
		float mUnitLeashLength;
		public float UnitLeashLength
		{
			get { return mUnitLeashLength; }
			set { mUnitLeashLength = value; }
		}
		#endregion

		#region MaxNumCorpses
		int mMaxNumCorpses;
		public int MaxNumCorpses
		{
			get { return mMaxNumCorpses; }
			set { mMaxNumCorpses = value; }
		}
		#endregion

		#region BurningEffectLimits
		int mDefaultBurningEffectLimit = 1;
		public int DefaultBurningEffectLimit
		{
			get { return mDefaultBurningEffectLimit; }
			set { mDefaultBurningEffectLimit = value; }
		}

		public Collections.BListArray<BBurningEffectLimit> BurningEffectLimits { get; private set; }
		#endregion

		#region Fatality
		float mFatalityTransitionScale;
		public float FatalityTransitionScale
		{
			get { return mFatalityTransitionScale; }
			set { mFatalityTransitionScale = value; }
		}

		float mFatalityMaxTransitionTime;
		public float FatalityMaxTransitionTime
		{
			get { return mFatalityMaxTransitionTime; }
			set { mFatalityMaxTransitionTime = value; }
		}

		float mFatalityPositionOffsetTolerance;
		public float FatalityPositionOffsetTolerance
		{
			get { return mFatalityPositionOffsetTolerance; }
			set { mFatalityPositionOffsetTolerance = value; }
		}

		float mFatalityOrientationOffsetTolerance;
		/// <summary>angle</summary>
		public float FatalityOrientationOffsetTolerance
		{
			get { return mFatalityOrientationOffsetTolerance; }
			set { mFatalityOrientationOffsetTolerance = value; }
		}

		float mFatalityExclusionRange;
		public float FatalityExclusionRange
		{
			get { return mFatalityExclusionRange; }
			set { mFatalityExclusionRange = value; }
		}
		#endregion

		#region GameOverDelay
		float mGameOverDelay;
		public float GameOverDelay
		{
			get { return mGameOverDelay; }
			set { mGameOverDelay = value; }
		}
		#endregion

		#region InfantryCorpseDecayTime
		float mInfantryCorpseDecayTime;
		public float InfantryCorpseDecayTime
		{
			get { return mInfantryCorpseDecayTime; }
			set { mInfantryCorpseDecayTime = value; }
		}
		#endregion

		#region CorpseSinkingSpacing
		float mCorpseSinkingSpacing;
		public float CorpseSinkingSpacing
		{
			get { return mCorpseSinkingSpacing; }
			set { mCorpseSinkingSpacing = value; }
		}
		#endregion

		#region MaxCorpseDisposalCount
		int mMaxCorpseDisposalCount;
		public int MaxCorpseDisposalCount
		{
			get { return mMaxCorpseDisposalCount; }
			set { mMaxCorpseDisposalCount = value; }
		}
		#endregion

		#region MaxSquadPathsPerFrame
		uint mMaxSquadPathsPerFrame = 10;
		public uint MaxSquadPathsPerFrame
		{
			get { return mMaxSquadPathsPerFrame; }
			set { mMaxSquadPathsPerFrame = value; }
		}
		#endregion

		#region MaxPlatoonPathsPerFrame
		uint mMaxPlatoonPathsPerFrame = 10;
		public uint MaxPlatoonPathsPerFrame
		{
			get { return mMaxPlatoonPathsPerFrame; }
			set { mMaxPlatoonPathsPerFrame = value; }
		}
		#endregion

		#region ProjectileGravity
		float mProjectileGravity;
		public float ProjectileGravity
		{
			get { return mProjectileGravity; }
			set { mProjectileGravity = value; }
		}
		#endregion

		#region ProjectileTumbleRate
		float mProjectileTumbleRate;
		/// <summary>angle</summary>
		public float ProjectileTumbleRate
		{
			get { return mProjectileTumbleRate; }
			set { mProjectileTumbleRate = value; }
		}
		#endregion

		#region TrackInterceptDistance
		float mTrackInterceptDistance;
		public float TrackInterceptDistance
		{
			get { return mTrackInterceptDistance; }
			set { mTrackInterceptDistance = value; }
		}
		#endregion

		#region StationaryTargetAttackToleranceAngle
		float mStationaryTargetAttackToleranceAngle;
		public float StationaryTargetAttackToleranceAngle
		{
			get { return mStationaryTargetAttackToleranceAngle; }
			set { mStationaryTargetAttackToleranceAngle = value; }
		}
		#endregion

		#region MovingTargetAttackToleranceAngle
		float mMovingTargetAttackToleranceAngle;
		public float MovingTargetAttackToleranceAngle
		{
			get { return mMovingTargetAttackToleranceAngle; }
			set { mMovingTargetAttackToleranceAngle = value; }
		}
		#endregion

		#region MovingTargetTrackingAttackToleranceAngle
		float mMovingTargetTrackingAttackToleranceAngle;
		public float MovingTargetTrackingAttackToleranceAngle
		{
			get { return mMovingTargetTrackingAttackToleranceAngle; }
			set { mMovingTargetTrackingAttackToleranceAngle = value; }
		}
		#endregion

		#region MovingTargetRangeMultiplier
		float mMovingTargetRangeMultiplier = 1.0f;
		public float MovingTargetRangeMultiplier
		{
			get { return mMovingTargetRangeMultiplier; }
			set { mMovingTargetRangeMultiplier = value; }
		}
		#endregion

		#region CloakingDelay
		float mCloakingDelay;
		public float CloakingDelay
		{
			get { return mCloakingDelay; }
			set { mCloakingDelay = value; }
		}
		#endregion

		#region ReCloakDelay
		float mReCloakDelay;
		public float ReCloakDelay
		{
			get { return mReCloakDelay; }
			set { mReCloakDelay = value; }
		}
		#endregion

		#region CloakDetectFrequency
		float mCloakDetectFrequency;
		public float CloakDetectFrequency
		{
			get { return mCloakDetectFrequency; }
			set { mCloakDetectFrequency = value; }
		}
		#endregion

		#region ShieldRegenDelay
		float mShieldRegenDelay;
		public float ShieldRegenDelay
		{
			get { return mShieldRegenDelay; }
			set { mShieldRegenDelay = value; }
		}
		#endregion

		#region ShieldRegenTime
		float mShieldRegenTime;
		public float ShieldRegenTime
		{
			get { return mShieldRegenTime; }
			set { mShieldRegenTime = value; }
		}
		#endregion

		#region AttackedRevealerLOS
		float mAttackedRevealerLOS;
		public float AttackedRevealerLOS
		{
			get { return mAttackedRevealerLOS; }
			set { mAttackedRevealerLOS = value; }
		}
		#endregion

		#region AttackedRevealerLifespan
		float mAttackedRevealerLifespan;
		public float AttackedRevealerLifespan
		{
			get { return mAttackedRevealerLifespan; }
			set { mAttackedRevealerLifespan = value; }
		}
		#endregion

		#region AttackRevealerLOS
		float mAttackRevealerLOS;
		public float AttackRevealerLOS
		{
			get { return mAttackRevealerLOS; }
			set { mAttackRevealerLOS = value; }
		}
		#endregion

		#region AttackRevealerLifespan
		float mAttackRevealerLifespan;
		public float AttackRevealerLifespan
		{
			get { return mAttackRevealerLifespan; }
			set { mAttackRevealerLifespan = value; }
		}
		#endregion

		#region MinimumRevealerSize
		float mMinimumRevealerSize;
		public float MinimumRevealerSize
		{
			get { return mMinimumRevealerSize; }
			set { mMinimumRevealerSize = value; }
		}
		#endregion

		#region AttackRatingMultiplier
		float mAttackRatingMultiplier = 20f;
		public float AttackRatingMultiplier
		{
			get { return mAttackRatingMultiplier; }
			set { mAttackRatingMultiplier = value; }
		}
		#endregion

		#region DefenseRatingMultiplier
		float mDefenseRatingMultiplier = 10f;
		public float DefenseRatingMultiplier
		{
			get { return mDefenseRatingMultiplier; }
			set { mDefenseRatingMultiplier = value; }
		}
		#endregion

		#region GoodAgainstMinAttackGrade
		uint mGoodAgainstMinAttackGrade = 3;
		public uint GoodAgainstMinAttackGrade
		{
			get { return mGoodAgainstMinAttackGrade; }
			set { mGoodAgainstMinAttackGrade = value; }
		}
		#endregion

		#region HeightBonusDamage
		float mHeightBonusDamage;
		public float HeightBonusDamage
		{
			get { return mHeightBonusDamage; }
			set { mHeightBonusDamage = value; }
		}
		#endregion

		#region ShieldBarColor
		System.Drawing.Color mShieldBarColor;
		public System.Drawing.Color ShieldBarColor
		{
			get { return mShieldBarColor; }
			set { mShieldBarColor = value; }
		}
		#endregion

		#region AmmoBarColor
		System.Drawing.Color mAmmoBarColor;
		public System.Drawing.Color AmmoBarColor
		{
			get { return mAmmoBarColor; }
			set { mAmmoBarColor = value; }
		}
		#endregion

		#region OpportunityDistPriFactor
		float mOpportunityDistPriFactor = 1.0f;
		public float OpportunityDistPriFactor
		{
			get { return mOpportunityDistPriFactor; }
			set { mOpportunityDistPriFactor = value; }
		}
		#endregion

		#region OpportunityBeingAttackedPriBonus
		float mOpportunityBeingAttackedPriBonus;
		public float OpportunityBeingAttackedPriBonus
		{
			get { return mOpportunityBeingAttackedPriBonus; }
			set { mOpportunityBeingAttackedPriBonus = value; }
		}
		#endregion

		#region ChanceToRocket
		float mChanceToRocket;
		public float ChanceToRocket
		{
			get { return mChanceToRocket; }
			set { mChanceToRocket = value; }
		}
		#endregion

		#region MaxDamageBankPctAdjust
		float mMaxDamageBankPctAdjust;
		public float MaxDamageBankPctAdjust
		{
			get { return mMaxDamageBankPctAdjust; }
			set { mMaxDamageBankPctAdjust = value; }
		}
		#endregion

		#region DamageBankTimer
		float mDamageBankTimer;
		public float DamageBankTimer
		{
			get { return mDamageBankTimer; }
			set { mDamageBankTimer = value; }
		}
		#endregion

		#region BuildingSelfDestructTime
		float mBuildingSelfDestructTime;
		public float BuildingSelfDestructTime
		{
			get { return mBuildingSelfDestructTime; }
			set { mBuildingSelfDestructTime = value; }
		}
		#endregion

		#region TributeAmount
		float mTributeAmount = 500f;
		public float TributeAmount
		{
			get { return mTributeAmount; }
			set { mTributeAmount = value; }
		}
		#endregion

		#region TributeCost
		float mTributeCost;
		public float TributeCost
		{
			get { return mTributeCost; }
			set { mTributeCost = value; }
		}
		#endregion

		#region UnscSupplyPadBonus
		float mUnscSupplyPadBonus;
		public float UnscSupplyPadBonus
		{
			get { return mUnscSupplyPadBonus; }
			set { mUnscSupplyPadBonus = value; }
		}
		#endregion

		#region UnscSupplyPadBreakEvenPoint
		float mUnscSupplyPadBreakEvenPoint;
		public float UnscSupplyPadBreakEvenPoint
		{
			get { return mUnscSupplyPadBreakEvenPoint; }
			set { mUnscSupplyPadBreakEvenPoint = value; }
		}
		#endregion

		#region CovSupplyPadBonus
		float mCovSupplyPadBonus;
		public float CovSupplyPadBonus
		{
			get { return mCovSupplyPadBonus; }
			set { mCovSupplyPadBonus = value; }
		}
		#endregion

		#region CovSupplyPadBreakEvenPoint
		float mCovSupplyPadBreakEvenPoint;
		public float CovSupplyPadBreakEvenPoint
		{
			get { return mCovSupplyPadBreakEvenPoint; }
			set { mCovSupplyPadBreakEvenPoint = value; }
		}
		#endregion

		#region LeaderPowerChargeResourceID
		int mLeaderPowerChargeResourceID = TypeExtensions.kNone;
		[Meta.ResourceReference]
		public int LeaderPowerChargeResourceID
		{
			get { return mLeaderPowerChargeResourceID; }
			set { mLeaderPowerChargeResourceID = value; }
		}
		#endregion

		#region LeaderPowerChargeRateID
		int mLeaderPowerChargeRateID = TypeExtensions.kNone;
		[Meta.RateReference]
		public int LeaderPowerChargeRateID
		{
			get { return mLeaderPowerChargeRateID; }
			set { mLeaderPowerChargeRateID = value; }
		}
		#endregion

		#region DamageReceivedXPFactor
		float mDamageReceivedXPFactor;
		public float DamageReceivedXPFactor
		{
			get { return mDamageReceivedXPFactor; }
			set { mDamageReceivedXPFactor = value; }
		}
		#endregion

		#region AirStrikeLoiterTime
		float mAirStrikeLoiterTime;
		public float AirStrikeLoiterTime
		{
			get { return mAirStrikeLoiterTime; }
			set { mAirStrikeLoiterTime = value; }
		}
		#endregion

		#region RecyleRefundRate
		float mRecyleRefundRate = 1.0f;
		public float RecyleRefundRate
		{
			get { return mRecyleRefundRate; }
			set { mRecyleRefundRate = value; }
		}
		#endregion

		#region BaseRebuildTimer
		float mBaseRebuildTimer;
		public float BaseRebuildTimer
		{
			get { return mBaseRebuildTimer; }
			set { mBaseRebuildTimer = value; }
		}
		#endregion

		#region ObjectiveArrowRadialOffset
		float mObjectiveArrowRadialOffset;
		public float ObjectiveArrowRadialOffset
		{
			get { return mObjectiveArrowRadialOffset; }
			set { mObjectiveArrowRadialOffset = value; }
		}
		#endregion

		#region ObjectiveArrowSwitchOffset
		float mObjectiveArrowSwitchOffset;
		public float ObjectiveArrowSwitchOffset
		{
			get { return mObjectiveArrowSwitchOffset; }
			set { mObjectiveArrowSwitchOffset = value; }
		}
		#endregion

		#region ObjectiveArrowYOffset
		float mObjectiveArrowYOffset;
		public float ObjectiveArrowYOffset
		{
			get { return mObjectiveArrowYOffset; }
			set { mObjectiveArrowYOffset = value; }
		}
		#endregion

		#region ObjectiveArrowMaxIndex
		byte mObjectiveArrowMaxIndex;
		public byte ObjectiveArrowMaxIndex
		{
			get { return mObjectiveArrowMaxIndex; }
			set { mObjectiveArrowMaxIndex = value; }
		}
		#endregion

		#region OverrunMinVel
		float mOverrunMinVel;
		public float OverrunMinVel
		{
			get { return mOverrunMinVel; }
			set { mOverrunMinVel = value; }
		}
		#endregion

		#region OverrunJumpForce
		float mOverrunJumpForce;
		public float OverrunJumpForce
		{
			get { return mOverrunJumpForce; }
			set { mOverrunJumpForce = value; }
		}
		#endregion

		#region OverrunDistance
		float mOverrunDistance;
		public float OverrunDistance
		{
			get { return mOverrunDistance; }
			set { mOverrunDistance = value; }
		}
		#endregion

		#region CoopResourceSplitRate
		float mCoopResourceSplitRate = 1.0f;
		public float CoopResourceSplitRate
		{
			get { return mCoopResourceSplitRate; }
			set { mCoopResourceSplitRate = value; }
		}
		#endregion

		#region HeroDownedLOS
		float mHeroDownedLOS;
		public float HeroDownedLOS
		{
			get { return mHeroDownedLOS; }
			set { mHeroDownedLOS = value; }
		}
		#endregion

		#region HeroHPRegenTime
		float mHeroHPRegenTime;
		public float HeroHPRegenTime
		{
			get { return mHeroHPRegenTime; }
			set { mHeroHPRegenTime = value; }
		}
		#endregion

		#region HeroRevivalDistance
		float mHeroRevivalDistance;
		public float HeroRevivalDistance
		{
			get { return mHeroRevivalDistance; }
			set { mHeroRevivalDistance = value; }
		}
		#endregion

		#region HeroPercentHPRevivalThreshhold
		float mHeroPercentHPRevivalThreshhold;
		public float HeroPercentHPRevivalThreshhold
		{
			get { return mHeroPercentHPRevivalThreshhold; }
			set { mHeroPercentHPRevivalThreshhold = value; }
		}
		#endregion

		#region MaxDeadHeroTransportDist
		float mMaxDeadHeroTransportDist;
		public float MaxDeadHeroTransportDist
		{
			get { return mMaxDeadHeroTransportDist; }
			set { mMaxDeadHeroTransportDist = value; }
		}
		#endregion

		#region Transport
		float mTransportClearRadiusScale = 1.0f;
		public float TransportClearRadiusScale
		{
			get { return mTransportClearRadiusScale; }
			set { mTransportClearRadiusScale = value; }
		}

		float mTransportMaxSearchRadiusScale = 1.0f;
		public float TransportMaxSearchRadiusScale
		{
			get { return mTransportMaxSearchRadiusScale; }
			set { mTransportMaxSearchRadiusScale = value; }
		}

		uint mTransportMaxSearchLocations = 1;
		public uint TransportMaxSearchLocations
		{
			get { return mTransportMaxSearchLocations; }
			set { mTransportMaxSearchLocations = value; }
		}

		uint mTransportBlockTime;
		public uint TransportBlockTime
		{
			get { return mTransportBlockTime; }
			set { mTransportBlockTime = value; }
		}

		uint mTransportLoadBlockTime;
		public uint TransportLoadBlockTime
		{
			get { return mTransportLoadBlockTime; }
			set { mTransportLoadBlockTime = value; }
		}
		#endregion

		#region Ambient life
		uint mALMaxWanderFrequency;
		public uint ALMaxWanderFrequency
		{
			get { return mALMaxWanderFrequency; }
			set { mALMaxWanderFrequency = value; }
		}

		uint mALPredatorCheckFrequency;
		public uint ALPredatorCheckFrequency
		{
			get { return mALPredatorCheckFrequency; }
			set { mALPredatorCheckFrequency = value; }
		}

		uint mALPreyCheckFrequency;
		public uint ALPreyCheckFrequency
		{
			get { return mALPreyCheckFrequency; }
			set { mALPreyCheckFrequency = value; }
		}

		float mALOppCheckRadius;
		public float ALOppCheckRadius
		{
			get { return mALOppCheckRadius; }
			set { mALOppCheckRadius = value; }
		}

		float mALFleeDistance;
		public float ALFleeDistance
		{
			get { return mALFleeDistance; }
			set { mALFleeDistance = value; }
		}

		float mALFleeMovementModifier;
		public float ALFleeMovementModifier
		{
			get { return mALFleeMovementModifier; }
			set { mALFleeMovementModifier = value; }
		}

		float mALMinWanderDistance;
		public float ALMinWanderDistance
		{
			get { return mALMinWanderDistance; }
			set { mALMinWanderDistance = value; }
		}

		float mALMaxWanderDistance;
		public float ALMaxWanderDistance
		{
			get { return mALMaxWanderDistance; }
			set { mALMaxWanderDistance = value; }
		}

		float mALSpawnerCheckFrequency;
		public float ALSpawnerCheckFrequency
		{
			get { return mALSpawnerCheckFrequency; }
			set { mALSpawnerCheckFrequency = value; }
		}
		#endregion

		#region Transport
		uint mTransportMaxBlockAttempts = 1;
		public uint TransportMaxBlockAttempts
		{
			get { return mTransportMaxBlockAttempts; }
			set { mTransportMaxBlockAttempts = value; }
		}

		float mTransportIncomingHeight = 60.0f;
		public float TransportIncomingHeight
		{
			get { return mTransportIncomingHeight; }
			set { mTransportIncomingHeight = value; }
		}

		float mTransportIncomingOffset = 40.0f;
		public float TransportIncomingOffset
		{
			get { return mTransportIncomingOffset; }
			set { mTransportIncomingOffset = value; }
		}

		float mTransportOutgoingHeight = 60.0f;
		public float TransportOutgoingHeight
		{
			get { return mTransportOutgoingHeight; }
			set { mTransportOutgoingHeight = value; }
		}

		float mTransportOutgoingOffset = 40.0f;
		public float TransportOutgoingOffset
		{
			get { return mTransportOutgoingOffset; }
			set { mTransportOutgoingOffset = value; }
		}

		float mTransportPickupHeight = 12.0f;
		public float TransportPickupHeight
		{
			get { return mTransportPickupHeight; }
			set { mTransportPickupHeight = value; }
		}

		float mTransportDropoffHeight = 12.0f;
		public float TransportDropoffHeight
		{
			get { return mTransportDropoffHeight; }
			set { mTransportDropoffHeight = value; }
		}

		uint mTransportMax = 3;
		public uint TransportMax
		{
			get { return mTransportMax; }
			set { mTransportMax = value; }
		}
		#endregion

		#region HitchOffset
		float mHitchOffset = 8.0f;
		public float HitchOffset
		{
			get { return mHitchOffset; }
			set { mHitchOffset = value; }
		}
		#endregion

		#region TimeFrozenToThaw
		float mTimeFrozenToThaw;
		public float TimeFrozenToThaw
		{
			get { return mTimeFrozenToThaw; }
			set { mTimeFrozenToThaw = value; }
		}
		#endregion

		#region TimeFreezingToThaw
		float mTimeFreezingToThaw;
		public float TimeFreezingToThaw
		{
			get { return mTimeFreezingToThaw; }
			set { mTimeFreezingToThaw = value; }
		}
		#endregion

		#region DefaultCryoPoints
		float mDefaultCryoPoints;
		public float DefaultCryoPoints
		{
			get { return mDefaultCryoPoints; }
			set { mDefaultCryoPoints = value; }
		}
		#endregion

		#region DefaultThawSpeed
		float mDefaultThawSpeed;
		public float DefaultThawSpeed
		{
			get { return mDefaultThawSpeed; }
			set { mDefaultThawSpeed = value; }
		}
		#endregion

		#region FreezingSpeedModifier
		float mFreezingSpeedModifier;
		public float FreezingSpeedModifier
		{
			get { return mFreezingSpeedModifier; }
			set { mFreezingSpeedModifier = value; }
		}
		#endregion

		#region FreezingDamageModifier
		float mFreezingDamageModifier;
		public float FreezingDamageModifier
		{
			get { return mFreezingDamageModifier; }
			set { mFreezingDamageModifier = value; }
		}
		#endregion

		#region FrozenDamageModifier
		float mFrozenDamageModifier;
		public float FrozenDamageModifier
		{
			get { return mFrozenDamageModifier; }
			set { mFrozenDamageModifier = value; }
		}
		#endregion

		#region SmallDotSize
		float mSmallDotSize;
		public float SmallDotSize
		{
			get { return mSmallDotSize; }
			set { mSmallDotSize = value; }
		}
		#endregion

		#region MediumDotSize
		float mMediumDotSize;
		public float MediumDotSize
		{
			get { return mMediumDotSize; }
			set { mMediumDotSize = value; }
		}
		#endregion

		public Collections.BTypeValuesString CodeProtoObjects { get; private set; }
		public Collections.BTypeValuesString CodeObjectTypes { get; private set; }
		public Collections.BListArray<BInfectionMap> InfectionMap { get; private set; }

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

		/// <summary>Get how much it costs, in total, to tribute a resource to another player</summary>
		public float TotalTributeCost { get { return (mTributeAmount * mTributeCost) + mTributeAmount; } }

		public BGameData()
		{
			Resources = new Collections.BListAutoId<BResource>();
			Rates = new Collections.BTypeNames();
			#region DifficultyModifiers
			mDifficultyModifiers[(int)BDifficultyTypeModifier.Normal] = 0.34f;
			mDifficultyModifiers[(int)BDifficultyTypeModifier.Hard] = 0.67f;
			mDifficultyModifiers[(int)BDifficultyTypeModifier.Legendary] = 1.0f;
			mDifficultyModifiers[(int)BDifficultyTypeModifier.Default] = 0.4f;
			mDifficultyModifiers[(int)BDifficultyTypeModifier.SPCAIDefault] = 0.5f;
			#endregion
			Populations = new Collections.BTypeNames();
			RefCounts = new Collections.BTypeNames();
			PlayerStates = new Collections.BTypeNames();
			BurningEffectLimits = new Collections.BListArray<BBurningEffectLimit>();
			CodeProtoObjects = new Collections.BTypeValuesString(kCodeProtoObjectsParams);
			CodeObjectTypes = new Collections.BTypeValuesString(kCodeObjectTypesParams);
			InfectionMap = new Collections.BListArray<BInfectionMap>();

			#region Nonsense
			HUDItems = new Collections.BTypeNames();
			FlashableItems = new Collections.BTypeNames();
			UnitFlags = new Collections.BTypeNames();
			SquadFlags = new Collections.BTypeNames();
			#endregion
		}

		#region ITagElementStreamable<string> Members
		/// <remarks>For streaming directly from gamedata.xml</remarks>
		internal void StreamGameData<TDoc, TCursor>(IO.TagElementStream<TDoc, TCursor, string> s)
			where TDoc : class
			where TCursor : class
		{
			var xs = s.GetSerializerInterface();

			XML.XmlUtil.Serialize(s, Resources, BResource.kBListXmlParams);
			XML.XmlUtil.Serialize(s, Rates, kRatesXmlParams);
			#region GoodAgainstReticle
			using (s.EnterCursorBookmark("GoodAgainstReticle"))
			{
				s.StreamElementOpt("NoEffect", ref mGoodAgainstGrades[(int)ReticleAttackGrade.NoEffect], Predicates.IsNotZero);
				s.StreamElementOpt("Weak",     ref mGoodAgainstGrades[(int)ReticleAttackGrade.Weak], Predicates.IsNotZero);
				s.StreamElementOpt("Fair",     ref mGoodAgainstGrades[(int)ReticleAttackGrade.Fair], Predicates.IsNotZero);
				s.StreamElementOpt("Good",     ref mGoodAgainstGrades[(int)ReticleAttackGrade.Good], Predicates.IsNotZero);
				s.StreamElementOpt("Extreme",  ref mGoodAgainstGrades[(int)ReticleAttackGrade.Extreme], Predicates.IsNotZero);
			}
			#endregion
			#region DifficultyModifiers
			s.StreamElementOpt("DifficultyEasy",         ref mDifficultyModifiers[(int)BDifficultyTypeModifier.Easy], Predicates.IsNotZero);
			// #NOTE The engine has a typo in it and looks for "DifficultyNormali"
			s.StreamElementOpt("DifficultyNormal",       ref mDifficultyModifiers[(int)BDifficultyTypeModifier.Normal], Predicates.IsNotZero);
			s.StreamElementOpt("DifficultyHard",         ref mDifficultyModifiers[(int)BDifficultyTypeModifier.Hard], Predicates.IsNotZero);
			s.StreamElementOpt("DifficultyLegendary",    ref mDifficultyModifiers[(int)BDifficultyTypeModifier.Legendary], Predicates.IsNotZero);
			s.StreamElementOpt("DifficultyDefault",      ref mDifficultyModifiers[(int)BDifficultyTypeModifier.Default], Predicates.IsNotZero);
			s.StreamElementOpt("DifficultySPCAIDefault", ref mDifficultyModifiers[(int)BDifficultyTypeModifier.SPCAIDefault], Predicates.IsNotZero);
			#endregion
			XML.XmlUtil.Serialize(s, Populations, kPopsXmlParams);
			XML.XmlUtil.Serialize(s, RefCounts, kRefCountsXmlParams);
			XML.XmlUtil.Serialize(s, PlayerStates, kPlayerStatesXmlParams);
			s.StreamElementOpt("GarrisonDamageMultiplier", ref mGarrisonDamageMultiplier, PhxPredicates.IsNotOne);
			s.StreamElementOpt("ConstructionDamageMultiplier", ref mConstructionDamageMultiplier, PhxPredicates.IsNotOne);
			s.StreamElementOpt("CaptureDecayRate", ref mCaptureDecayRate, Predicates.IsNotZero);
			s.StreamElementOpt("SquadLeashLength", ref mSquadLeashLength, Predicates.IsNotZero);
			s.StreamElementOpt("SquadAggroLength", ref mSquadAggroLength, Predicates.IsNotZero);
			s.StreamElementOpt("UnitLeashLength", ref mUnitLeashLength, Predicates.IsNotZero);
			s.StreamElementOpt("MaxNumCorpses", ref mMaxNumCorpses, Predicates.IsNotZero);
			#region BurningEffectLimits
			using (s.EnterCursorBookmark("BurningEffectLimits"))
			{
				s.StreamAttribute("DefaultLimit", ref mDefaultBurningEffectLimit);
				XML.XmlUtil.Serialize(s, BurningEffectLimits, BBurningEffectLimit.kBListXmlParams);
			}
			#endregion
			#region Fatality
			s.StreamElementOpt("FatalityTransitionScale", ref mFatalityTransitionScale, Predicates.IsNotZero);
			s.StreamElementOpt("FatalityMaxTransitionTime", ref mFatalityMaxTransitionTime, Predicates.IsNotZero);
			s.StreamElementOpt("FatalityPositionOffsetTolerance", ref mFatalityPositionOffsetTolerance, Predicates.IsNotZero);
			s.StreamElementOpt("FatalityOrientationOffsetTolerance", ref mFatalityOrientationOffsetTolerance, Predicates.IsNotZero);
			s.StreamElementOpt("FatalityExclusionRange", ref mFatalityExclusionRange, Predicates.IsNotZero);
			#endregion
			s.StreamElementOpt("GameOverDelay", ref mGameOverDelay, Predicates.IsNotZero);
			s.StreamElementOpt("InfantryCorpseDecayTime", ref mInfantryCorpseDecayTime, Predicates.IsNotZero);
			s.StreamElementOpt("CorpseSinkingSpacing", ref mCorpseSinkingSpacing, Predicates.IsNotZero);
			s.StreamElementOpt("MaxCorpseDisposalCount", ref mMaxCorpseDisposalCount, Predicates.IsNotZero);
			s.StreamElementOpt("MaxSquadPathsPerFrame", ref mMaxSquadPathsPerFrame, Predicates.IsNotZero);
			s.StreamElementOpt("MaxPlatoonPathsPerFrame", ref mMaxPlatoonPathsPerFrame, Predicates.IsNotZero);
			s.StreamElementOpt("ProjectileGravity", ref mProjectileGravity, Predicates.IsNotZero);
			s.StreamElementOpt("ProjectileTumbleRate", ref mProjectileTumbleRate, Predicates.IsNotZero);
			s.StreamElementOpt("TrackInterceptDistance", ref mTrackInterceptDistance, Predicates.IsNotZero);
			s.StreamElementOpt("StationaryTargetAttackToleranceAngle", ref mStationaryTargetAttackToleranceAngle, Predicates.IsNotZero);
			s.StreamElementOpt("MovingTargetAttackToleranceAngle", ref mMovingTargetAttackToleranceAngle, Predicates.IsNotZero);
			s.StreamElementOpt("MovingTargetTrackingAttackToleranceAngle", ref mMovingTargetTrackingAttackToleranceAngle, Predicates.IsNotZero);
			s.StreamElementOpt("MovingTargetRangeMultiplier", ref mMovingTargetRangeMultiplier, PhxPredicates.IsNotOne);
			s.StreamElementOpt("CloakingDelay", ref mCloakingDelay, Predicates.IsNotZero);
			s.StreamElementOpt("ReCloakDelay", ref mReCloakDelay, Predicates.IsNotZero);
			s.StreamElementOpt("CloakDetectFrequency", ref mCloakDetectFrequency, Predicates.IsNotZero);
			s.StreamElementOpt("ShieldRegenDelay", ref mShieldRegenDelay, Predicates.IsNotZero);
			s.StreamElementOpt("ShieldRegenTime", ref mShieldRegenTime, Predicates.IsNotZero);
			s.StreamElementOpt("AttackedRevealerLOS", ref mAttackedRevealerLOS, Predicates.IsNotZero);
			s.StreamElementOpt("AttackedRevealerLifespan", ref mAttackedRevealerLifespan, Predicates.IsNotZero);
			s.StreamElementOpt("AttackRevealerLOS", ref mAttackRevealerLOS, Predicates.IsNotZero);
			s.StreamElementOpt("AttackRevealerLifespan", ref mAttackRevealerLifespan, Predicates.IsNotZero);
			s.StreamElementOpt("MinimumRevealerSize", ref mMinimumRevealerSize, Predicates.IsNotZero);
			s.StreamElementOpt("AttackRatingMultiplier", ref mAttackRatingMultiplier, Predicates.IsNotZero);
			s.StreamElementOpt("DefenseRatingMultiplier", ref mDefenseRatingMultiplier, Predicates.IsNotZero);
			// #NOTE data has this as "GoodAgainstMinAttackRating"
			s.StreamElementOpt("GoodAgainstMinAttackRating", ref mGoodAgainstMinAttackGrade, Predicates.IsNotZero);
			s.StreamElementOpt("HeightBonusDamage", ref mHeightBonusDamage, Predicates.IsNotZero);
			s.StreamIntegerColor("ShieldBarColor", ref mShieldBarColor);
			s.StreamIntegerColor("AmmoBarColor", ref mAmmoBarColor);
			s.StreamElementOpt("OpportunityDistPriFactor", ref mOpportunityDistPriFactor, PhxPredicates.IsNotOne);
			s.StreamElementOpt("OpportunityBeingAttackedPriBonus", ref mOpportunityBeingAttackedPriBonus, Predicates.IsNotZero);
			s.StreamElementOpt("ChanceToRocket", ref mChanceToRocket, Predicates.IsNotZero);
			s.StreamElementOpt("MaxDamageBankPctAdjust", ref mMaxDamageBankPctAdjust, Predicates.IsNotZero);
			s.StreamElementOpt("DamageBankTimer", ref mDamageBankTimer, Predicates.IsNotZero);
			s.StreamElementOpt("BuildingSelfDestructTime", ref mBuildingSelfDestructTime, Predicates.IsNotZero);
			s.StreamElementOpt("TributeAmount", ref mTributeAmount, Predicates.IsNotZero);
			s.StreamElementOpt("TributeCost", ref mTributeCost, Predicates.IsNotZero);
			s.StreamElementOpt("UnscSupplyPadBonus", ref mUnscSupplyPadBonus, Predicates.IsNotZero);
			s.StreamElementOpt("UnscSupplyPadBreakEvenPoint", ref mUnscSupplyPadBreakEvenPoint, Predicates.IsNotZero);
			s.StreamElementOpt("CovSupplyPadBonus", ref mCovSupplyPadBonus, Predicates.IsNotZero);
			s.StreamElementOpt("CovSupplyPadBreakEvenPoint", ref mCovSupplyPadBreakEvenPoint, Predicates.IsNotZero);
			xs.StreamTypeName(s, "LeaderPowerChargeResource", ref mLeaderPowerChargeResourceID, GameDataObjectKind.Cost);
			xs.StreamTypeName(s, "LeaderPowerChargeRate", ref mLeaderPowerChargeRateID, GameDataObjectKind.Rate);
			s.StreamElementOpt("DamageReceivedXPFactor", ref mDamageReceivedXPFactor, PhxPredicates.IsNotInvalid);
			s.StreamElementOpt("AirStrikeLoiterTime", ref mAirStrikeLoiterTime, Predicates.IsNotZero);
			s.StreamElementOpt("RecyleRefundRate", ref mRecyleRefundRate, PhxPredicates.IsNotOne);
			s.StreamElementOpt("BaseRebuildTimer", ref mBaseRebuildTimer, Predicates.IsNotZero);
			s.StreamElementOpt("ObjectiveArrowRadialOffset", ref mObjectiveArrowRadialOffset, Predicates.IsNotZero);
			s.StreamElementOpt("ObjectiveArrowSwitchOffset", ref mObjectiveArrowSwitchOffset, Predicates.IsNotZero);
			s.StreamElementOpt("ObjectiveArrowYOffset", ref mObjectiveArrowYOffset, Predicates.IsNotZero);
			s.StreamElementOpt("ObjectiveArrowMaxIndex", ref mObjectiveArrowMaxIndex, Predicates.IsNotZero);
			s.StreamElementOpt("OverrunMinVel", ref mOverrunMinVel, Predicates.IsNotZero);
			s.StreamElementOpt("OverrunJumpForce", ref mOverrunJumpForce, Predicates.IsNotZero);
			s.StreamElementOpt("OverrunDistance", ref mOverrunDistance, Predicates.IsNotZero);
			s.StreamElementOpt("CoopResourceSplitRate", ref mCoopResourceSplitRate, PhxPredicates.IsNotOne);
			s.StreamElementOpt("HeroDownedLOS", ref mHeroDownedLOS, Predicates.IsNotZero);
			s.StreamElementOpt("HeroHPRegenTime", ref mHeroHPRegenTime, Predicates.IsNotZero);
			s.StreamElementOpt("HeroRevivalDistance", ref mHeroRevivalDistance, Predicates.IsNotZero);
			s.StreamElementOpt("HeroPercentHPRevivalThreshhold", ref mHeroPercentHPRevivalThreshhold, Predicates.IsNotZero);
			s.StreamElementOpt("MaxDeadHeroTransportDist", ref mMaxDeadHeroTransportDist, Predicates.IsNotZero);
			#region Transport
			s.StreamElementOpt("TransportClearRadiusScale", ref mTransportClearRadiusScale, PhxPredicates.IsNotOne);
			s.StreamElementOpt("TransportMaxSearchRadiusScale", ref mTransportMaxSearchRadiusScale, PhxPredicates.IsNotOne);
			s.StreamElementOpt("TransportMaxSearchLocations", ref mTransportMaxSearchLocations, PhxPredicates.IsNotOne);
			s.StreamElementOpt("TransportBlockTime", ref mTransportBlockTime, Predicates.IsNotZero);
			s.StreamElementOpt("TransportLoadBlockTime", ref mTransportLoadBlockTime, Predicates.IsNotZero);
			#endregion
			#region Ambient life
			s.StreamElementOpt("ALMaxWanderFrequency", ref mALMaxWanderFrequency, Predicates.IsNotZero);
			s.StreamElementOpt("ALPredatorCheckFrequency", ref mALPredatorCheckFrequency, Predicates.IsNotZero);
			s.StreamElementOpt("ALPreyCheckFrequency", ref mALPreyCheckFrequency, Predicates.IsNotZero);
			s.StreamElementOpt("ALOppCheckRadius", ref mALOppCheckRadius, Predicates.IsNotZero);
			s.StreamElementOpt("ALFleeDistance", ref mALFleeDistance, Predicates.IsNotZero);
			s.StreamElementOpt("ALFleeMovementModifier", ref mALFleeMovementModifier, Predicates.IsNotZero);
			s.StreamElementOpt("ALMinWanderDistance", ref mALMinWanderDistance, Predicates.IsNotZero);
			s.StreamElementOpt("ALMaxWanderDistance", ref mALMaxWanderDistance, Predicates.IsNotZero);
			s.StreamElementOpt("ALSpawnerCheckFrequency", ref mALSpawnerCheckFrequency, Predicates.IsNotZero);
			#endregion
			#region Transport
			s.StreamElementOpt("TransportMaxBlockAttempts", ref mTransportMaxBlockAttempts, PhxPredicates.IsNotOne);
			s.StreamElementOpt("TransportIncomingHeight", ref mTransportIncomingHeight, Predicates.IsNotZero);
			s.StreamElementOpt("TransportIncomingOffset", ref mTransportIncomingOffset, Predicates.IsNotZero);
			s.StreamElementOpt("TransportOutgoingHeight", ref mTransportOutgoingHeight, Predicates.IsNotZero);
			s.StreamElementOpt("TransportOutgoingOffset", ref mTransportOutgoingOffset, Predicates.IsNotZero);
			s.StreamElementOpt("TransportPickupHeight", ref mTransportPickupHeight, Predicates.IsNotZero);
			s.StreamElementOpt("TransportDropoffHeight", ref mTransportDropoffHeight, Predicates.IsNotZero);
			s.StreamElementOpt("TransportMax", ref mTransportMax, Predicates.IsNotZero);
			#endregion
			s.StreamElementOpt("HitchOffset", ref mHitchOffset, Predicates.IsNotZero);
			s.StreamElementOpt("TimeFrozenToThaw", ref mTimeFrozenToThaw, Predicates.IsNotZero);
			s.StreamElementOpt("TimeFreezingToThaw", ref mTimeFreezingToThaw, Predicates.IsNotZero);
			s.StreamElementOpt("DefaultCryoPoints", ref mDefaultCryoPoints, Predicates.IsNotZero);
			s.StreamElementOpt("DefaultThawSpeed", ref mDefaultThawSpeed, Predicates.IsNotZero);
			s.StreamElementOpt("FreezingSpeedModifier", ref mFreezingSpeedModifier, Predicates.IsNotZero);
			s.StreamElementOpt("FreezingDamageModifier", ref mFreezingDamageModifier, Predicates.IsNotZero);
			s.StreamElementOpt("FrozenDamageModifier", ref mFrozenDamageModifier, Predicates.IsNotZero);
			using (s.EnterCursorBookmark("Dot"))
			{
				s.StreamElementOpt("small", ref mSmallDotSize, Predicates.IsNotZero);
				s.StreamElementOpt("medium", ref mMediumDotSize, Predicates.IsNotZero);
			}

			XML.XmlUtil.Serialize(s, CodeProtoObjects, kCodeProtoObjectsXmlParams);
			XML.XmlUtil.Serialize(s, CodeObjectTypes, kCodeObjectTypesXmlParams);
			XML.XmlUtil.Serialize(s, InfectionMap, BInfectionMap.kBListXmlParams);

			#region Nonsense
			XML.XmlUtil.Serialize(s, HUDItems, kHUDItemsXmlParams);
			XML.XmlUtil.Serialize(s, FlashableItems, kFlashableItemsXmlParams);
			XML.XmlUtil.Serialize(s, UnitFlags, kUnitFlagsXmlParams);
			XML.XmlUtil.Serialize(s, SquadFlags, kSquadFlagsXmlParams);
			#endregion
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

	public sealed class BBurningEffectLimit
		: IO.ITagElementStringNameStreamable
		, IComparable<BBurningEffectLimit>
		, IEquatable<BBurningEffectLimit>
	{
		#region Xml constants
		public static readonly XML.BListXmlParams kBListXmlParams = new XML.BListXmlParams
		{
			ElementName = "BurningEffectLimitEntry",
		};
		#endregion

		#region Limit
		int mLimit;
		public int Limit
		{
			get { return mLimit; }
			set { mLimit = value; }
		}
		#endregion

		#region ObjectTypeID
		int mObjectTypeID = TypeExtensions.kNone;
		[Meta.UnitReference]
		public int ObjectTypeID
		{
			get { return mObjectTypeID; }
			set { mObjectTypeID = value; }
		}
		#endregion

		public bool IsInvalid { get { return PhxUtil.IsUndefinedReferenceHandleOrNone(ObjectTypeID); } }

		#region ITagElementStreamable<string> Members
		public void Serialize<TDoc, TCursor>(IO.TagElementStream<TDoc, TCursor, string> s)
			where TDoc : class
			where TCursor : class
		{
			var xs = s.GetSerializerInterface();

			s.StreamAttribute("Limit", ref mLimit);
			xs.StreamDBID(s, XML.XmlUtil.kNoXmlName, ref mObjectTypeID, DatabaseObjectKind.Unit, false, XML.XmlUtil.kSourceCursor);
		}
		#endregion

		#region IComparable Members
		public int CompareTo(BBurningEffectLimit other)
		{
			if (Limit != other.Limit)
				Limit.CompareTo(other.Limit);

			return ObjectTypeID.CompareTo(other.ObjectTypeID);
		}
		#endregion

		#region IEquatable Members
		public bool Equals(BBurningEffectLimit other)
		{
			return Limit == other.Limit
				&& ObjectTypeID == other.ObjectTypeID;
		}
		#endregion
	};
}