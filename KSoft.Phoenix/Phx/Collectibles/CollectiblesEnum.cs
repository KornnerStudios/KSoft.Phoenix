
namespace KSoft.Phoenix.Phx
{
	public enum BCollectibleSkullEffectType
	{
		Invalid,

		Score,
		GruntTank,
		GruntConfetti,
		Physics,
		ScarabBeam,
		MinimapDisable,
		Weakness,
		HitpointMod,
		DamageMod,
		Veterancy,
		AbilityRecharge,
		DeathExplode,
		TrainMod,
		SupplyMod,
		PowerRecharge,
		UnitModWarthog,
		UnitModWraith,
	};

	public enum BCollectibleSkullFlags
	{
		// 0x3C
		// 0
		OnFromBeginning, // 1
		// 2
		Hidden, // 3
		// 4
		SelfActive, // 5
		// 6
		Active, // 7
	};

	public enum BCollectibleSkullTarget
	{
		None,

		PlayerUnits,
		NonPlayerUnits,
		OwnerOnly,
	};
}