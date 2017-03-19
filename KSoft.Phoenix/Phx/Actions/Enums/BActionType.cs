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
}