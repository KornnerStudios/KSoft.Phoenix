using XmlIgnore = System.Xml.Serialization.XmlIgnoreAttribute;

namespace KSoft.Phoenix.Phx
{
	public enum BActionType : byte
	{
		#region 0x00
		[XmlIgnore] Idle, // entity
		[XmlIgnore] Listen, // entity
		Move, GaggleMove = Move, // unit...
		MoveAir,
		[XmlIgnore] MoveWarthog,
		[XmlIgnore] MoveGhost,
		RangedAttack, HandAttack = RangedAttack,
		[XmlIgnore] Building,
		[XmlIgnore] DOT,
		[XmlIgnore] UnitChangeMode,
		[XmlIgnore] Death,
		[XmlIgnore] InfectDeath,
		Garrison,
		Ungarrison,
		[XmlIgnore] ShieldRegen,
		Honk,
		#endregion
		#region 0x10
		SpawnSquad,
		Capture,
		Join,
		ChangeOwner,
		[XmlIgnore] AmmoRegen,
		Physics,
		[XmlIgnore] PlayBlockingAnimation,
		Mines,
		Detonate,
		Gather,
		CollisionAttack,
		AreaAttack,
		UnderAttack,
		SecondaryTurretAttack,
		RevealToTeam,
		AirTrafficControl,
		#endregion
		#region 0x20
		Hitch,
		Unhitch,
		SlaveTurretAttack,
		Thrown,
		Dodge,
		Deflect,
		AvoidCollisionAir,
		[XmlIgnore] PlayAttachmentAnims,
		Heal,
		Revive,
		Buff,
		Infect,
		HotDrop,
		TentacleDormant,
		[XmlIgnore] HeroDeath,
		Stasis,
		#endregion
		#region 0x30
		BubbleShield,
		Bomb,
		PlasmaShieldGen,
		Jump,
		AmbientLifeSpawner,
		JumpGather,
		JumpGarrison,
		JumpAttack,
		PointBlankAttack,
		Roar,
		EnergyShield,
		[XmlIgnore] ScaleLOS,
		Charge, // ChargedRangedAttack
		TowerWall,
		AoeHeal,
		[XmlIgnore] Attack, // squad
		#endregion
		#region 0x40
		ChangeMode, // squad...
		[XmlIgnore] Repair,
		RepairOther,
		[XmlIgnore] SquadShieldRegen,
		[XmlIgnore] SquadGarrison,
		[XmlIgnore] SquadUngarrison,
		Transport,
		[XmlIgnore] SquadPlayBlockingAnimation,
		[XmlIgnore] SquadMove,
		[XmlIgnore] Reinforce,
		[XmlIgnore] Work,
		CarpetBomb,
		AirStrike,
		[XmlIgnore] SquadHitch,
		[XmlIgnore] SquadUnhitch,
		[XmlIgnore] SquadDetonate,
		#endregion
		#region 0x50
		Wander,
		Cloak,
		CloakDetector,
		Daze,
		[XmlIgnore] SquadJump,
		AmbientLife,
		ReflectDamage,
		Cryo,
		[XmlIgnore] PlatoonMove,
		CoreSlide, // unit...
		InfantryEnergyShield,
		Dome,
		SpiritBond, // squad
		Rage, // unit
		//
		//
		#endregion

		Invalid = 0x5E
	};

	public enum BProtoActionFlags
	{
		// 0x138
		InstantAttack,
		MeleeRange,
		KillSelfOnAttack,
		DontCheckOrientTolerance,
		DontLoopAttackAnim,
		StopAttackingWhenAmmoDepleted,
		MainAttack,
		Stationary,

		// 0x139
		//Strafing, // 1<<5, set when the Strafing element is streamed
		//CanOrientOwner, // 1<<6, actually tests the inner text of the element for 'false'
		Infection, // 1<<7

		// 0x13C
		//
		//
		//
		//
		WaitForDodgeCooldown,
		MultiDeflect,
		WaitForDeflectCooldown,
		//

		// 0x13D
		AvoidOnly,
		HideSpawnUntilRelease,
		DoShakeOnAttackTag,
		SmallArms,

		// 0x13E
		DontAutoRestart, // 1<<7
	};

	public enum BWeaponFlags
	{
		// 0x58
		ThrowDamageParts,
		ThrowAliveUnits,
		ThrowUnits,
		UsesAmmo,
		PhysicsLaunchAxial,
		EnableHeightBonusDamage,
		AllowFriendlyFire,

		// 0x59
		UseGroupRange,
		//
		UseDPSasDPA,
		SmallArmsDeflectable,
		Deflectable,
		Dodgeable,
		FlailThrownUnits,
		//PulseObject, // Set automatically when the PulseObject element is streamed

		// 0x5A
		//
		//
		Tentacle,
		ApplyKnockback,
		StasisBomb,
		StasisDrain,
		//StasisSmartTargeting,
		CarriedObjectAsProjectileVisual,

		// 0x5B
		//
		//
		//
		//
		AOEIgnoresYAxis, // 1<<4
		OverridesRevive,
		//
		AOELinearDamage,

		// 0xDC
		//
		//
		//
		AirBurst, // 1<<3
		PullUnits,
		//
		KeepDPSRamp,
		TargetsFootOfUnit,
	};

	public enum BTargetRuleFlags
	{
		TargetsGround,
		ContainsUnits,
		GaiaOwned,
		MergeSquads,
		MeleeAttacker,
	};

	public enum BTargetRuleTargetStates
	{
		Unbuilt,
		Damaged,
		Capturable,
	};

	partial class BProtoAction
	{
		enum BJoinType
		{
			Follow,
			Merge,
			Board,
			FollowAttack,
		};

		enum BMergeType
		{
			None,

			Ground,
			Air,
		};
	};
}