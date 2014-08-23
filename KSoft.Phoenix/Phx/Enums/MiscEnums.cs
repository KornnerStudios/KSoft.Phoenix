
namespace KSoft.Phoenix.Phx
{
	public enum BActionStatus
	{
		NotDone,
		DoneSuccess,
		DoneFailure,
	};

	public enum BLOSType
	{
		LOSDontCare,
		LOSNormal,
		LOSFullVisible,
	};

	public enum BListPosition
	{
		First,
		Last,
		Random,
	};

	public enum BExposedAction
	{
		ExposedAction0,
		ExposedAction1,
		ExposedAction2,
	};

	public enum BDataScalar
	{
		Accuracy,
		WorkRate,
		Damage,
		LOS,
		Velocity,
		WeaponRange,
		DamageTaken,
	};

	public enum BFlareType
	{
		Look,
		Help,
		Meet,
		Attack,
	};

	public enum BBidType
	{
		Invalid,
		None,
		Squad,
		Tech,
		Building,
		Power,
	};
	public enum BBidState
	{
		Invalid,
		Inactive,
		Waiting,
		Approved,
	};

	public enum BRumbleType
	{
		Invalid,

		Fixed,
		SineWave,
		IntervalBurst,
		RandomNoise,
		Incline,
		Decline,
		BumpLRL,
	};
	public enum BRumbleMotor
	{
		Both,
		Left,
		Right,
	};

	public enum BEventType
	{
		Invalid,

		ControlTilt,
		ControlZoom,
		ControlRotate,
		ControlPan,
		ControlCircleSelect,
		ControlCircleMultiSelect,
		ControlClearAllSelections,
		ControlModifierAction,
		ControlModifierSpeed,
		ControlResetCameraSettings,
		ControlGotoRally,
		ControlGotoBase,
		ControlGotoScout,
		ControlGotoNode,
		ControlGotoHero,
		ControlGotoAlert,
		ControlGotoSelected,
		ControlGroupSelect,
		ControlGroupGoto,
		ControlGroupAssign,
		ControlGroupAddTo,
		ControlScreenSelectMilitary,
		ControlGlobalSelect,
		ControlDoubleClickSelect,
		ControlFindCrowdMilitary,
		ControlFindCrowdVillager,
		ControlSetRallyPoint,
		Flare,
		FlareHelp,
		FlareMeet,
		FlareAttack,
		MenuShowCommand,
		MenuCloseCommand,
		MenuNavCommand,
		MenuCommandHasFocus0,
		MenuCommandHasFocus1,
		MenuCommandHasFocus2,
		MenuCommandHasFocus3,
		MenuCommandHasFocus4,
		MenuCommandHasFocus5,
		MenuCommandHasFocus6,
		MenuCommandHasFocus7,
		MenuCommandHasFocusN,
		MenuCommandClickmenuN,
		MenuCommandIsMenuOpen,
		MenuCommanndIsMenuNotOpen,
		MenuShowPower,
		MenuClosePower,
		MenuPowerHasFocusN,
		MenuPowerClickmenuN,
		MenuPowerIsMenuOpen,
		MenuPowerIsMenuNotOpen,
		MenuShowSelectPower,
		MenuShowAbility,
		MenuShowTribute,
		MenuShowObjectives,
		GameEntityBuilt,
		GameEntityKilled,
		ChatShown,
		ChatRemoved,
		ChatCompleted,
		CommandBowl,
		CommandAbility,
		CommandUnpack,
		CommandDoWork,
		CommandAttack,
		CommandMove,
		CommandTrainSquad,
		CommandTrainSquadCancel,
		CommandResearch,
		CommandResearchCancel,
		CommandBuildOther,
		CommandRecycle,
		CommandRecycleCancel,
		CameraLookingAt,
		SelectUnits,
		CinematicCompleted,
		FadeCompleted,
		UsedPower,
		Timer1Sec,
		ControlCircleSelectFullyGrown,
		PowerOrbitalComplete,
		GameEntityRammed,
		GameEntityJacked,
		GameEntityKilledByNonProjectile,
	};

	public enum BGameStatePredicate
	{
		Invalid,

		SquadsSelected,
		HasSquads,
		HasBuildings,
		HasResources,
		HasTech,
		GameTime,
	};
}