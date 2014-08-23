using System;

using XmlIgnore = System.Xml.Serialization.XmlIgnoreAttribute;

namespace KSoft.Phoenix.Phx
{
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
}