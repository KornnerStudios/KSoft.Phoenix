
namespace KSoft.Phoenix.Phx
{
	public enum BObjectSoundType
	{
		None = TypeExtensions.kNone,

		Create,
		Death,
		Select,
		Work,
		Attack,
		CaptureComplete,
		Ability,
		AbilityJacked,

		StopExist,
		StepDown,
		StepUp,
		SkidOn,
		SkidOff,
		RocketStart,
		RocketEnd,
		/// <summary>Ramp up sound leading to moving sound</summary>
		StartMove,
		CorpseDeath,
		/// <summary>The ramp down sound</summary>
		StopMove,
		Jump,
		Land,
		ImpactDeath,
		PieceThrownOff,
		LandHard,

		SelectDowned,
		Unused24,

		Pain,
		Cloak,
		UnCloak,
		Exist,

		ShieldLow,
		ShieldDepleted,
		ShieldRegen,

		kNumberOf
	};
}