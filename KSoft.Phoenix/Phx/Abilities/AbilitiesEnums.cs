
namespace KSoft.Phoenix.Phx
{
	public enum BAbilityTargetType
	{
		None,
		Location,
		Unit,
		UnitOrLocation,
	};

	public enum BMovementModifierType
	{
		Ability,
		Mode,
	};

	public enum BRecoverType
	{
		Move,
		Attack,
		Ability,
	};

	public enum BSquadMode
	{
		Invalid = TypeExtensions.kNone,

		Normal = 0,
		StandGround,
		Lockdown,
		Sniper,
		HitAndRun,
		Passive,
		Cover,
		Ability,
		CarryingObject,
		Power,
		ScarabScan,
		ScarabTarget,
		ScarabKill,
	};
}