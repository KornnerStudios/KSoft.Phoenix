
namespace KSoft.Phoenix.Phx
{
	public enum BObjectDataType
	{
		Invalid = TypeExtensions.kNone,

		#region 0x00
		Enable = 0,
		Hitpoints,
		Shieldpoints,
		AmmoMax,
		LOS,
		MaximumVelocity,
		MaximumRange,
		ResearchPoints,
		ResourceTrickleRate,
		MaximumResourceTrickleRate,
		RateAmount,
		RateMultiplier,
		Resource,
		Projectile,
		Damage,
		MinRange,
		#endregion
		#region 0x10
		AOERadius,
		AOEPrimaryTargetFactor,
		AOEDistanceFactor,
		AOEDamageFactor,
		Accuracy,
		MovingAccuracy,
		MaxDeviation,
		MovingMaxDeviation,
		DataAccuracyDistanceFactor,
		AccuracyDeviationFactor,
		MaxVelocityLead,
		WorkRate,
		BuildPoints,
		Cost,
		AutoCloak,
		MoveWhileCloaked,
		#endregion
		#region 0x20
		AttackWhileCloaked,
		ActionEnable,
		CommandEnable,
		BountyResource,
		TributeCost,
		ShieldRegenRate,
		ShieldRegenDelay,
		DamageModifier,
		PopCap,
		PopMax,
		UnitTrainLimit,
		SquadTrainLimit,
		RepairCost,
		RepairTime,
		PowerRechargeTime,
		PowerUseLimit,
		#endregion
		#region 0x30
		Level,
		Bounty,
		MaxContained,
		MaxDamagePerRam,
		ReflectDamageFactor,
		AirBurstSpan,
		AbilityDisabled,
		DOTrate,
		DOTduration,
		ImpactEffect,
		AmmoRegenRate,
		CommandSelectable,
		DisplayNameID,
		Icon,
		AltIcon,
		Stasis,
		#endregion
		#region 0x40
		TurretYawRate,
		TurretPitchRate,
		PowerLevel,
		BoardTime,
		AbilityRecoverTime,
		TechLevel,
		HPBar,
		WeaponPhysicsMultiplier,
		DeathSpawn,







		#endregion
	};

	public enum BObjectDataIconType : int
	{
		Invalid = TypeExtensions.kNone,

		unit,
		building,
		misc,
		tech,
	};
}