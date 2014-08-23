
namespace KSoft.Phoenix.Phx
{
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
}