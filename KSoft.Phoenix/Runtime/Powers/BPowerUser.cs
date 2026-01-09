
namespace KSoft.Phoenix.Runtime
{
	abstract class BPowerUser : IO.IEndianStreamSerializable
	{
		public uint Value;
		public uint Type;
		public bool Initialized, Destroy, NoCost, CheckPowerLocation;
		public sbyte ProtoPowerID, PowerLevel;
		public BEntityID OwnerSquadID;
		public bool UsedByPrimaryUser;
		public double Elapsed;

		#region IEndianStreamSerializable Members
		public virtual void Serialize(IO.EndianStream s)
		{
			s.Stream(ref Value);
			s.Stream(ref Type);
			s.Stream(ref Initialized); s.Stream(ref Destroy); s.Stream(ref NoCost); s.Stream(ref CheckPowerLocation);
			s.Stream(ref ProtoPowerID); s.Stream(ref PowerLevel);
			s.Stream(ref OwnerSquadID);
			s.Stream(ref UsedByPrimaryUser);
			s.Stream(ref Elapsed);
		}
		#endregion

		internal static BPowerUser FromType(Phx.BPowerType type)
		{
			return type switch
			{
				Phx.BPowerType.Cleansing => new BPowerCovGlassingUser(),
				Phx.BPowerType.Orbital => new BPowerUnscMacUser(),
				Phx.BPowerType.CarpetBombing => new BPowerUnscCarpetBombUser(),
				Phx.BPowerType.Cryo => new BPowerUnscCryoUser(),
				Phx.BPowerType.Rage => new BPowerCovRageUser(),
				Phx.BPowerType.Wave => new BPowerCovDebrisUser(),
				Phx.BPowerType.Disruption => new BPowerUnscDisruptionUser(),
				Phx.BPowerType.Transport => new BPowerTransportUser(),
				Phx.BPowerType.ODST => new BPowerUnscOdstUser(),
				Phx.BPowerType.Repair => new BPowerUnscHealUser(),
				_ => throw new KSoft.Debug.UnreachableException(type.ToString()),
			};
		}
	};
}