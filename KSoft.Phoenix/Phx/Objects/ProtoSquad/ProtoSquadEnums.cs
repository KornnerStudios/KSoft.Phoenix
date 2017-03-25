using System;

using XmlIgnore = System.Xml.Serialization.XmlIgnoreAttribute;

namespace KSoft.Phoenix.Phx
{
	[Obsolete("Only for reverse engineering documentation purposes, don't use")]
	public enum BProtoSquadDataType
	{
		Invalid = BObjectDataType.Invalid,

		Enable = BObjectDataType.Enable,
		BuildPoints = BObjectDataType.BuildPoints,
		Cost = BObjectDataType.Cost,
		Level = BObjectDataType.Level,
	};

	public enum BProtoSquadFlags
	{
		// 0x68
		KBAware, // 1<<2
		OneTimeSpawnUsed, // 1<<4

		// 0x144
		AlwaysAttackReviveUnits, // 1<<0
		InstantTrainWithRecharge, // 1<<1
		ForceToGaiaPlayer, // 1<<2,
		CreateAudioReactions, // 1<<3
		Chatter, // 1<<4
		Repairable, // 1<<5

		// 0x145
		Rebel, // 1<<0
		Forerunner, // 1<<1
		Flood, // 1<<2
		FlyingFlood, // 1<<3
		DiesWhenFrozen, // 1<<4
		OnlyShowBobbleHeadWhenContained, // 1<<5
		AlwaysRenderSelectionDecal, // 1<<6
		ScaredByRoar, // 1<<7

		// 0x146
		AlwaysShowHPBar, // 1<<3
		NoPlatoonMerge, // 1<<6

		[Obsolete, XmlIgnore] JoinAll,
	};

	public enum BProtoSquadFormationType
	{
		Standard,

		Flock,
		Gaggle,
		Line,
	};

	public enum BSquadBirthType
	{
		Invalid = TypeExtensions.kNone,

		Trained,
		FlyIn,
	};

	public enum BSquadSoundType
	{
		None = TypeExtensions.kNone,

		Exist,
		StopExist,
		MoveChatter,
		AttackChatter,
		MoveAttackChatter,
		IdleChatter,
		AllyKilled,
		KilledEnemy,
		Cheer,
		LevelUp,

		ReactBirth,
		ReactDeath,
		ReactJoinBattle,
		ReactPowCarpetBomb,
		ReactPowOrbital,
		ReactPowCleansing,
		ReactPowCryo,
		ReactPowRage,
		ReactPowWave,
		ReactPowDisruption,
		ReactFatalityUNSC,
		ReactFatalityCOV,
		ReactJacking,
		ReactCommandeer,
		ReactHotDrop,

		StartMove,
		StopMove,

		Kamikaze,

		StartJump,
		StopJump,
	};

	public enum BSquadStance
	{
		Invalid = TypeExtensions.kNone,

		Passive,
		Aggressive,
		Defensive,
	};

	public enum BUnitRole
	{
		Normal,
		Leader,
		Support,
	};
}