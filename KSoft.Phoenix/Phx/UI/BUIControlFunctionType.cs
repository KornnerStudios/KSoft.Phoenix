
namespace KSoft.Phoenix.Phx
{
	public enum BUIControlFunctionType
	{
		Flare,		SpeedModifier,	Start,		Back,				Translation,		Pan,
		Tilt,		Zoom,			Selection,	DoubleClickSelect,	MultiSelect,		Clear,
		DoWork,		DoWorkQueue,	Ability,	AbilityQueue,		Powers,				ResetCamera,

		AssignGroup1,	AssignGroup2,	AssignGroup3,	AssignGroup4,
		SelectGroup1,	SelectGroup2,	SelectGroup3,	SelectGroup4,
		GotoGroup1,		GotoGroup2,		GotoGroup3,		GotoGroup4,
		
		GotoBase,	GotoAlert,	GotoScout,	GotoArmy,	GotoNode,	GotoHero,	GotoRally,	GotoSelected,

		ScreenSelect,		GlobalSelect,
		ScreenSelectPrev,	ScreenSelectNext,
		GlobalSelectPrev,	GlobalSelectNext,
		ScreenCyclePrev,	ScreenCycleNext,
		TargetPrev,			TargetNext,
		SubSelectPrev,		SubSelectNext,		SubSelectSquad,		SubSelectType,	SubSelectTag,
		SubSelectSelect,	SubSelectGoto,
		ModeGoto,			ModeSubSelectRight,	ModeSubSelectLeft,	ModeGroup,		ModeFlare,
		GroupAdd,			GroupNext,			GroupPrev,			GroupGoto,
		FlareLook,			FlareHelp,			FlareMeet,			FlareAttack,	MapZoom,
		
		AttackMove,
		SetRally,
		NoAction1,			NoAction2,
		ExtraInfo,
	};
}