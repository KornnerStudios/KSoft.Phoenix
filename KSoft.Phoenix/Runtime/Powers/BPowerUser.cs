
using BEntityID = System.Int32;

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
			switch (type)
			{
			case Phx.BPowerType.Cleansing: return new BPowerCovGlassingUser();
			case Phx.BPowerType.Orbital: return new BPowerUnscMacUser();
			case Phx.BPowerType.CarpetBombing: return new BPowerUnscCarpetBombUser();
			case Phx.BPowerType.Cryo: return new BPowerUnscCryoUser();
			case Phx.BPowerType.Rage: return new BPowerCovRageUser();
			case Phx.BPowerType.Wave: return new BPowerCovDebrisUser();
			case Phx.BPowerType.Disruption: return new BPowerUnscDisruptionUser();
			case Phx.BPowerType.Transport: return new BPowerTransportUser();
			case Phx.BPowerType.ODST: return new BPowerUnscOdstUser();
			case Phx.BPowerType.Repair: return new BPowerUnscHealUser();

			default: throw new KSoft.Debug.UnreachableException(type.ToString());
			}
		}
	};
}