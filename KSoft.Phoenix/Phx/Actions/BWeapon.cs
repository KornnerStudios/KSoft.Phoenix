
namespace KSoft.Phoenix.Phx
{
	public sealed class BWeapon
		: Collections.BListAutoIdObject
	{
		#region Xml constants
		public static readonly XML.BListXmlParams kBListXmlParams = new()
		{
			ElementName = "Weapon",
			DataName = "Name",
			Flags = XML.BCollectionXmlParamsFlags.UseElementForData |
				XML.BCollectionXmlParamsFlags.ForceNoRootElementStreaming,
		};
		#endregion

		#region Properties
		float mDamagePerSecond = PhxUtil.kInvalidSingle;
		public float DamagePerSecond { get { return mDamagePerSecond; } }
		float mDOTRate = PhxUtil.kInvalidSingle;
		public float DOTRate { get { return mDOTRate; } }
		float mDOTDuration = PhxUtil.kInvalidSingle;
		public float DOTDuration { get { return mDOTDuration; } }

		float mAttackRate = PhxUtil.kInvalidSingle;
		public float AttackRate { get { return mAttackRate; } }
		int mProjectileObjectID = TypeExtensions.kNone;
		[Meta.BProtoObjectReference]
		public float ProjectileObjectID { get { return mProjectileObjectID; } }

		int mWeaponTypeID = TypeExtensions.kNone;
		[Meta.BWeaponTypeReference]
		public int WeaponTypeID { get { return mWeaponTypeID; } }
		int mVisualAmmo = TypeExtensions.kNone;
		public int VisualAmmo { get { return mVisualAmmo; } }

		#region TriggerScript
		string mTriggerScript;
		[Meta.TriggerScriptReference]
		public string TriggerScript
		{
			get { return mTriggerScript; }
			set { mTriggerScript = value; }
		}
		#endregion

		float mMinRange = PhxUtil.kInvalidSingle;
		public float MinRange { get { return mMinRange; } }
		float mMaxRange = PhxUtil.kInvalidSingle;
		public float MaxRange { get { return mMaxRange; } }

		float mReflectDamageFactor = PhxUtil.kInvalidSingle;
		public float ReflectDamageFactor { get { return mReflectDamageFactor; } }
		float mMovingAccuracy = PhxUtil.kInvalidSingle;
		public float MovingAccuracy { get { return mMovingAccuracy; } }
		float mMaxDeviation = PhxUtil.kInvalidSingle;
		public float MaxDeviation { get { return mMaxDeviation; } }
		float mMovingMaxDeviation = PhxUtil.kInvalidSingle;
		public float MovingMaxDeviation { get { return mMovingMaxDeviation; } }
		float mAccuracyDistanceFactor = PhxUtil.kInvalidSingle;
		public float AccuracyDistanceFactor { get { return mAccuracyDistanceFactor; } }
		float mAccuracyDeviationFactor = PhxUtil.kInvalidSingle;
		public float AccuracyDeviationFactor { get { return mAccuracyDeviationFactor; } }
		float mMaxVelocityLead = PhxUtil.kInvalidSingle;
		public float MaxVelocityLead { get { return mMaxVelocityLead; } }
		float mAirBurstSpan = PhxUtil.kInvalidSingle;
		public float AirBurstSpan { get { return mAirBurstSpan; } }

		public Collections.BTypeValues<BDamageRatingOverride> DamageOverrides { get; private set; } = new(BDamageRatingOverride.kBListParams);
		public Collections.BListArray<BTargetPriority> TargetPriorities { get; private set; } = new();

		bool mStasisSmartTargeting;
		public bool StasisSmartTargeting { get { return mStasisSmartTargeting; } }
		float mStasisHealToDrainRatio = PhxUtil.kInvalidSingle;
		public float StasisHealToDrainRatio { get { return mStasisHealToDrainRatio; } }

		sbyte mBounces = TypeExtensions.kNone;
		public sbyte Bounces { get { return mBounces; } }
		float mBounceRange = PhxUtil.kInvalidSingle;
		public float BounceRange { get { return mBounceRange; } }

		float mMaxPullRange = PhxUtil.kInvalidSingle;
		public float MaxPullRange { get { return mMaxPullRange; } }
		#endregion

		#region BListAutoIdObject Members
		public override void Serialize<TDoc, TCursor>(IO.TagElementStream<TDoc, TCursor, string> s)
		{
			var xs = s.GetSerializerInterface();

			s.StreamElementOpt("DamagePerSecond", ref mDamagePerSecond, PhxPredicates.IsNotInvalid);
			// #TODO_SUPPORT DPSRamp
			s.StreamElementOpt("DOTrate", ref mDOTRate, PhxPredicates.IsNotInvalid);
			s.StreamElementOpt("DOTduration", ref mDOTDuration, PhxPredicates.IsNotInvalid);
			// #TODO_SUPPORT DOTEffect

			// #TODO_SUPPORT Reapply
			// #TODO_SUPPORT Apply
			// #TODO_SUPPORT PostAttackCooldownMin
			// #TODO_SUPPORT PostAttackCooldownMax
			// #TODO_SUPPORT PreAttackCooldownMin
			// #TODO_SUPPORT PreAttackCooldownMax

			s.StreamElementOpt("AttackRate", ref mAttackRate, PhxPredicates.IsNotInvalid);
			xs.StreamDBID(s, "Projectile", ref mProjectileObjectID, DatabaseObjectKind.Object);

			// #TODO_SUPPORT ImpactEffect
			// #TODO_SUPPORT ImpactCameraShake
			// #TODO_SUPPORT ImpactRumble
			// #TODO_SUPPORT ImpactCameraEffect

			xs.StreamDBID(s, "WeaponType", ref mWeaponTypeID, DatabaseObjectKind.WeaponType);
			s.StreamElementOpt("VisualAmmo", ref mVisualAmmo, Predicates.IsNotNone);

			// #TODO_SUPPORT AOERadius
			// #TODO_SUPPORT AOEPrimaryTargetFactor
			// #TODO_SUPPORT AOEDistanceFactor
			// #TODO_SUPPORT AOEDamageFactor
			// #TODO_SUPPORT AOELinearDamage

			// #TODO_SUPPORT PhysicsLaunchAngleMin
			// #TODO_SUPPORT PhysicsLaunchAngleMax
			// #TODO_SUPPORT PhysicsLaunchAxial
			// #TODO_SUPPORT PhysicsForceMin
			// #TODO_SUPPORT PhysicsForceMax
			// #TODO_SUPPORT PhysicsForceMaxAngle

			// #TODO_SUPPORT BWeaponFlags
			// ThrowUnits,ThrowAliveUnits,ThrowDamageParts,FlailThrownUnits,Dodgeable,
			// Deflectable,SmallArmsDeflectable,OverridesRevive,PullUnits,UseDPSasDPA,
			// UseGroupRange,CarriedObjectAsProjectileVisual,
			// AllowFriendlyFire,EnableHeightBonusDamage,
			// UsesAmmo,
			// TargetsFootOfUnit,KeepDPSRamp,
			// StasisDrain,StasisBomb,
			// ApplyKnockback,Tentacle,
			// AOEIgnoresYAxis,
			// AirBurst

			// #TODO_SUPPORT Hardpoint, id that must exist on the ProtoObject's Hardpoints

			s.StreamElementOpt("TriggerScript", ref mTriggerScript, Predicates.IsNotNullOrEmpty);

			s.StreamElementOpt("MinRange", ref mMinRange, PhxPredicates.IsNotInvalid);
			s.StreamElementOpt("MaxRange", ref mMaxRange, PhxPredicates.IsNotInvalid);

			// #TODO_SUPPORT MaxDamagePerRam
			s.StreamElementOpt("ReflectDamageFactor", ref mReflectDamageFactor, PhxPredicates.IsNotInvalid);
			// #TODO_SUPPORT Accuracy
			s.StreamElementOpt("MovingAccuracy", ref mMovingAccuracy, PhxPredicates.IsNotInvalid);
			s.StreamElementOpt("MaxDeviation", ref mMaxDeviation, PhxPredicates.IsNotInvalid);
			s.StreamElementOpt("MovingMaxDeviation", ref mMovingMaxDeviation, PhxPredicates.IsNotInvalid);
			s.StreamElementOpt("AccuracyDistanceFactor", ref mAccuracyDistanceFactor, PhxPredicates.IsNotInvalid);
			s.StreamElementOpt("AccuracyDeviationFactor", ref mAccuracyDeviationFactor, PhxPredicates.IsNotInvalid);
			s.StreamElementOpt("MaxVelocityLead", ref mMaxVelocityLead, PhxPredicates.IsNotInvalid);
			s.StreamElementOpt("AirBurstSpan", ref mAirBurstSpan, PhxPredicates.IsNotInvalid);

			XML.XmlUtil.Serialize(s, DamageOverrides, BDamageRatingOverride.kBListXmlParams);
			XML.XmlUtil.Serialize(s, TargetPriorities, BTargetPriority.kBListXmlParams);

			// #TODO_SUPPORT CausePhysicsExplosion (particle, victimType)

			using (var bm = s.EnterCursorBookmarkOpt("Stasis", this, o => o.mStasisSmartTargeting))
			{
				if (bm.IsNotNull)
				{
					s.StreamAttribute("SmartTargeting", ref mStasisSmartTargeting);
				}
			}

			s.StreamElementOpt("StasisHealToDrainRatio", ref mStasisHealToDrainRatio, PhxPredicates.IsNotInvalid);
			// #TODO_SUPPORT ThrowOffsetAngle
			// #TODO_SUPPORT ThrowVelocity

			// #TODO_SUPPORT Daze (...)

			s.StreamElementOpt("Bounces", ref mBounces, Predicates.IsNotNone);
			s.StreamElementOpt("BounceRange", ref mBounceRange, PhxPredicates.IsNotInvalid);
			// #TODO_SUPPORT CameraRubleShakeScalarNotLocal
			s.StreamElementOpt("MaxPullRange", ref mMaxPullRange, PhxPredicates.IsNotInvalid);
		}
		#endregion
	};
}