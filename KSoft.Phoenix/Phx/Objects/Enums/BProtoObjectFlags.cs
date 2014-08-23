using System;
using XmlIgnore = System.Xml.Serialization.XmlIgnoreAttribute;

namespace KSoft.Phoenix.Phx
{
	public enum BProtoObjectFlags
	{
		// 0x88
		AttackWhileCloaked, // 0
		MoveWhileCloaked, // 1
		AutoCloak, // 2
		// 3?
		AbilityDisabled, // 4

		// 0x89
		SoundBehindFOW,// = 1<<0,
		VisibleToAll,// = 1<<1,
		Doppled,// = 1<<2,
		GrayMapDoppled,// = 1<<3,
		NoTieToGround,// = 1<<4,
		ForceToGaiaPlayer,// = 1<<5,

		// 0x8A
		NoGrayMapDoppledInCampaign, // 6
		Neutral,// = 1<<7,

		// 0x348
		ManualBuild, // 0
		Build, // 1
		// HasTrainSquadCommand // 2
		SelectedRect, // 3
		OrientUnitWithGround, // 4
		NonCollideable, // 5, actually "Collideable" in code
		PlayerOwnsObstruction, // 6
		DontRotateObstruction, // 7, actually "RotateObstruction" in code

		// 0x349
		HasHPBar, // 0
		UnlimitedResources, // 1
		DieAtZeroResources, // 2
		// 3?
		// 4?
		Immoveable, // 5

		// 0x34A
		HighArc, // 0
		IsAffectedByGravity, // 1
		// 2?
		// 3?
		Invulnerable, // 4
		AutoRepair, // 5
		BlockMovement, // 6
		BlockLOS, // 7

		// 0x34B
		KillGarrisoned, // 0
		DamageGarrisoned, // 1
		Tracking, // 2
		ShowRange, // 3
		PassiveGarrisoned, // 4
		UngarrisonToGaia, // 5
		Capturable, // 6
		// 7?

		#region 0x34C
		RocketOnDeath, // 0
		VisibleForTeamOnly, // 1
		VisibleForOwnerOnly, // 2
		Destructible, // 3
		KBCreatesBase, // 4
		ExternalShield, // 5
		KBAware, // 6
		UIDecal, // 7
		#endregion

		// 0x34D
		StartAtMaxAmmo, // 0
		TargetsFootOfUnit,// = 1<<1,
		// 2?
		HasTrackMask, // 3
		NoCull, // 4
		FadeOnDeath, // 5
		DontAttackWhileMoving, // 6
		Beam, // 7

		#region 0x34E
		Repairable,// = 1<<0,
		NoRender,// = 1<<1,
		Obscurable,// = 1<<2,
		AlwaysVisibleOnMinimap,// = 1<<3,
		ForceAnimRate, // 4
		NoActionOverrideMove, // 5
		Update, // 6
		InvulnerableWhenGaia, // 7
		#endregion

		// 0x34F
		ForceCreateObstruction, // 0
		DamageLinkedSocketsFirst,// = 1<<1,
		NoBuildUnderAttack,// = 1<<2,
		// 3?
		AirMovement,// = 1<<4,
		WalkToTurn,// = 1<<5,
		ScaleBuildAnimRate, // 6
		DoNotFilterOrient, // 7, actually "FilterOrient" in code

		// 0x350
		IsSticky,// = 1<<0,
		ExpireOnTimer,// = 1<<1,
		ExplodeOnTimer,// = 1<<2,
		CommandableByAnyPlayer,// = 1<<3,
		SingleSocketBuilding,// = 1<<4,
		AlwaysAttackReviveUnits,// = 1<<5,
		DontAutoAttackMe,// = 1<<6,
		// 7?

		#region 0x351
		UseBuildingAction, // 0
		NonRotatable, // 1, actually "Rotatable" in code
		ShatterDeathReplacement, // 2
		DamagedDeathReplacement,// = 1<<3,
		HasPivotingEngines,// = 1<<4,
		OverridesRevive,// = 1<<5,
		LinearCostEscalation,// = 1<<6,
		IsNeedler,// = 1<<7,
		#endregion

		#region 0x352
		ForceUpdateContainedUnits,// = 1<<0,
		IsFlameEffect,// = 1<<1,
		SingleStick,// = 1<<2,
		DieLast, // 3
		ChildForDamageTakenScalar, // 4
		SelfParkingLot, // 5
		KillChildObjectsOnDeath, // 6
		LockdownMenu, // 7
		#endregion

		#region 0x353
		TriggersBattleMusicWhenAttacked,// = 1<<0,
		NoRenderForOwner,// = 1<<1,
		NoCorpse,// = 1<<2,
		ShowRescuedCount,// = 1<<3,
		MustOwnToSelect,// = 1<<4,
		AbilityAttacksMeleeOnly,// = 1<<5,
		RegularAttacksMeleeOnly,// = 1<<6,
		FlattenTerrain,// = 1<<7,
		#endregion

		#region 0x354
		CanSetAsRallyPoint,// = 1<<0,
		IgnoreSquadAI,// = 1<<1,
		NotSelectableWhenChildObject,// = 1<<2,
		Teleporter,// = 1<<3,
		OneSquadContainment,// = 1<<4,
		ProjectileTumbles,// = 1<<5,
		ProjectileObstructable,// = 1<<6,
		AutoExplorationGroup,// = 1<<7,
		#endregion

		#region 0x355
		SelectionDontConformToTerrain, // 0
		PhysicsDetonateOnDeath,// = 1<<1,
		ObstructsAir,// = 1<<2,
		NoRandomMoveAnimStart,// = 1<<3, actually "RandomMoveAnimStart" in code
		HideOnImpact,// = 1<<4,
		PermanentSocket,// = 1<<5,
		SelfDamage,// = 1<<6,
		SecondaryBuildingQueue,// = 1<<7,
		#endregion

		#region 0x356
		UseAutoParkingLot,// = 1<<0,
		UseBuildRotation,// = 1<<1,
		CarryNoRenderToChildren,// = 1<<2,
		UseRelaxedSpeedGroup,// = 1<<3,
		AppearsBelowDecals,// = 1<<4,
		IKTransitionToIdle,// = 1<<5,
		SyncAnimRateToPhysics,// = 1<<6,
		TurnInPlace,// = 1<<7,
		#endregion

		// 0x357
		NoStickyCam,// = 1<<3, actually "StickyCam" in code
		// 4?
		CheckLOSAgainstBase,// = 1<<5,
		//CheckPos, // 6, see: DeathSpawnSquad
		KillOnDetach,// = 1<<7,

		#region Alpha only
		[Obsolete] cFlagPhysicsControl,
		#endregion

		//[Obsolete] NonCollidable = NonCollideable, // Fixed in HW's XmlFiles.cs
		[Obsolete, XmlIgnore] NonSolid,
		[Obsolete, XmlIgnore] RenderBelowDecals,
	};
}