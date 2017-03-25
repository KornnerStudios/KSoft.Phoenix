
namespace KSoft.Phoenix.Phx
{
	public enum BAutoLockDown
	{
		None,
		LockAndUnlock,
		LockOnly,
		UnlockOnly,

		kNumberOf
	};

	public enum BGotoType
	{
		None,
		Base,
		Military,
		Infantry,
		Vehicle,
		Air,
		Civilian,
		Scout,
		Node,
		Alert,
		Army,
		Hero,

		kNumberOf
	};

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
	};

	public enum BPickPriority
	{
		None,
		Building,
		Resource,
		Unit,
		Rally,
	};

	public enum BProtoObjectClassType
	{
		Invalid = TypeExtensions.kNone,

		Object,
		Squad,
		Building,
		Unit,
		Projectile,
	};

	public enum BProtoObjectExitDirection
	{
		FromFront,
		FromFrontRight,
		FromFrontLeft,
		FromRight,
		FromLeft,
		FromBack,

		kNumberOf
	};

	public enum BProtoObjectMovementType
	{
		None,
		Land,
		Air,
		Flood,
		Scarab,
		Hover,
	};

	public enum BProtoObjectSelectType
	{
		None,

		Unit,
		Command,
		Target,
		SingleUnit,
		SingleType,
	};

	public enum BRallyPointType
	{
		Invalid = TypeExtensions.kNone,

		Military,
		Civilian,
	};

	partial class BProtoObjectChildObject
	{
		public enum ChildObjectType
		{
			Object,
			ParkingLot,
			Socket,
			Rally,
			OneTimeSpawnSquad,
			Unit,
			Foundation,
		};
	};

	partial class BProtoObjectTrainLimit
	{
		public enum LimitType
		{
			Invalid = TypeExtensions.kNone,

			Unit,
			Squad,
		};
	};
}