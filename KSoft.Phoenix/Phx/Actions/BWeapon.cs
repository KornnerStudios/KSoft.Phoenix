
namespace KSoft.Phoenix.Phx
{
	public sealed class BWeapon
		: Collections.BListAutoIdObject
	{
		#region Xml constants
		public static readonly XML.BListXmlParams kBListXmlParams = new XML.BListXmlParams
		{
			ElementName = "Weapon",
			DataName = "Name",
			Flags = XML.BCollectionXmlParamsFlags.UseElementForData |
				XML.BCollectionXmlParamsFlags.ForceNoRootElementStreaming,
		};

		const string kXmlElementDamagePerSecond = "DamagePerSecond"; // float
		const string kXmlElementDOTRate = "DOTrate"; // float
		const string kXmlElementDOTDuration = "DOTduration"; // float

		const string kXmlElementAttackRate = "AttackRate"; // float
		const string kXmlElementProjectile = "Projectile"; // proto object, inner text

		const string kXmlElementWeaponType = "WeaponType"; // weapon type id, inner text
		const string kXmlElementVisualAmmo = "VisualAmmo"; // integer
		const string kXmlElementTriggerScript = "TriggerScript"; // 

		const string kXmlElementMinRange = "MinRange"; // float
		const string kXmlElementMaxRange = "MaxRange"; // float

		const string kXmlElementReflectDamageFactor = "ReflectDamageFactor"; // float
		const string kXmlElementMovingAccuracy = "MovingAccuracy"; // float
		const string kXmlElementMaxDeviation = "MaxDeviation"; // float
		const string kXmlElementMovingMaxDeviation = "MovingMaxDeviation"; // float
		const string kXmlElementAccuracyDistanceFactor = "AccuracyDistanceFactor"; // float
		const string kXmlElementAccuracyDeviationFactor = "AccuracyDeviationFactor"; // float
		const string kXmlElementMaxVelocityLead = "MaxVelocityLead"; // float
		const string kXmlElementAirBurstSpan = "AirBurstSpan"; // float


		const string kXmlElementStasis = "Stasis";
		const string kXmlElementStasisAttrSmartTargeting = "SmartTargeting"; // bool
		const string kXmlElementStasisHealToDrainRatio = "StasisHealToDrainRatio"; // float

		const string kXmlElementBounces = "Bounces"; // sbyte
		const string kXmlElementBounceRange = "BounceRange"; // float

		const string kXmlElementMaxPullRange = "MaxPullRange"; // float
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
		public float ProjectileObjectID { get { return mProjectileObjectID; } }

		int mWeaponTypeID = TypeExtensions.kNone;
		public int WeaponTypeID { get { return mWeaponTypeID; } }
		int mVisualAmmo = TypeExtensions.kNone;
		public int VisualAmmo { get { return mVisualAmmo; } }
		int mTriggerScriptID = TypeExtensions.kNone;
		public int TriggerScriptID { get { return mTriggerScriptID; } }

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

		public Collections.BTypeValues<BDamageRatingOverride> DamageOverrides { get; private set; }
		public Collections.BListArray<BTargetPriority> TargetPriorities { get; private set; }

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

		public BWeapon()
		{
			DamageOverrides = new Collections.BTypeValues<BDamageRatingOverride>(BDamageRatingOverride.kBListParams);
			TargetPriorities = new Collections.BListArray<BTargetPriority>();
		}

		#region BListAutoIdObject Members
		public override void Serialize<TDoc, TCursor>(IO.TagElementStream<TDoc, TCursor, string> s)
		{
			var xs = s.GetSerializerInterface();

			s.StreamElementOpt(kXmlElementDamagePerSecond, ref mDamagePerSecond, PhxPredicates.IsNotInvalid);
			s.StreamElementOpt(kXmlElementDOTRate, ref mDOTRate, PhxPredicates.IsNotInvalid);
			s.StreamElementOpt(kXmlElementDOTDuration, ref mDOTDuration, PhxPredicates.IsNotInvalid);

			s.StreamElementOpt(kXmlElementAttackRate, ref mAttackRate, PhxPredicates.IsNotInvalid);
			xs.StreamDBID(s, kXmlElementProjectile, ref mProjectileObjectID, DatabaseObjectKind.Object);

			xs.StreamDBID(s, kXmlElementWeaponType, ref mWeaponTypeID, DatabaseObjectKind.WeaponType);
			s.StreamElementOpt(kXmlElementVisualAmmo, ref mVisualAmmo, Predicates.IsNotNone);
			//mTriggerScriptID

			s.StreamElementOpt(kXmlElementMinRange, ref mMinRange, PhxPredicates.IsNotInvalid);
			s.StreamElementOpt(kXmlElementMaxRange, ref mMaxRange, PhxPredicates.IsNotInvalid);

			s.StreamElementOpt(kXmlElementReflectDamageFactor, ref mReflectDamageFactor, PhxPredicates.IsNotInvalid);
			s.StreamElementOpt(kXmlElementMovingAccuracy, ref mMovingAccuracy, PhxPredicates.IsNotInvalid);
			s.StreamElementOpt(kXmlElementMaxDeviation, ref mMaxDeviation, PhxPredicates.IsNotInvalid);
			s.StreamElementOpt(kXmlElementMovingMaxDeviation, ref mMovingMaxDeviation, PhxPredicates.IsNotInvalid);
			s.StreamElementOpt(kXmlElementAccuracyDistanceFactor, ref mAccuracyDistanceFactor, PhxPredicates.IsNotInvalid);
			s.StreamElementOpt(kXmlElementAccuracyDeviationFactor, ref mAccuracyDeviationFactor, PhxPredicates.IsNotInvalid);
			s.StreamElementOpt(kXmlElementMaxVelocityLead, ref mMaxVelocityLead, PhxPredicates.IsNotInvalid);
			s.StreamElementOpt(kXmlElementAirBurstSpan, ref mAirBurstSpan, PhxPredicates.IsNotInvalid);

			XML.XmlUtil.Serialize(s, DamageOverrides, BDamageRatingOverride.kBListXmlParams);
			XML.XmlUtil.Serialize(s, TargetPriorities, BTargetPriority.kBListXmlParams);

			using (var bm = s.EnterCursorBookmarkOpt(kXmlElementStasis, this, o => o.mStasisSmartTargeting)) if (bm.IsNotNull)
				s.StreamAttribute(kXmlElementStasisAttrSmartTargeting, ref mStasisSmartTargeting);

			s.StreamElementOpt(kXmlElementStasisHealToDrainRatio, ref mStasisHealToDrainRatio, PhxPredicates.IsNotInvalid);

			s.StreamElementOpt(kXmlElementBounces, ref mBounces, Predicates.IsNotNone);
			s.StreamElementOpt(kXmlElementBounceRange, ref mBounceRange, PhxPredicates.IsNotInvalid);

			s.StreamElementOpt(kXmlElementMaxPullRange, ref mMaxPullRange, PhxPredicates.IsNotInvalid);
		}
		#endregion
	};
}