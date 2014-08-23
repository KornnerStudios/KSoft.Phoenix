
namespace KSoft.Phoenix.Phx
{
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
}