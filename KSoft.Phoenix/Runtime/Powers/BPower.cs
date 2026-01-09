
using BPowerUserID = System.Int32;

namespace KSoft.Phoenix.Runtime
{
	abstract class BPower
		: IO.IEndianStreamSerializable
	{
		public int ID;
		public uint Type;
		public BPowerUserID PowerUserID;
		public sbyte ProtoPowerID, PowerLevel;
		public float MaintenanceSupplies;
		public double Elapsed;
		public sbyte PlayerID;
		public BEntityID OwnerID;
		public BVector TargetLocation;
		public bool Destroy, IgnoreAllReqs, CheckPowerLocation;

		#region IEndianStreamSerializable Members
		public virtual void Serialize(IO.EndianStream s)
		{
			s.Stream(ref ID);
			s.Stream(ref Type);
			s.Stream(ref PowerUserID);
			s.Stream(ref ProtoPowerID); s.Stream(ref PowerLevel);
			s.Stream(ref MaintenanceSupplies);
			s.Stream(ref Elapsed);
			s.Stream(ref PlayerID);
			s.Stream(ref OwnerID);
			s.StreamV(ref TargetLocation);
			s.Stream(ref Destroy); s.Stream(ref IgnoreAllReqs); s.Stream(ref CheckPowerLocation);
		}
		#endregion

		internal static BPower FromType(Phx.BPowerType type)
		{
			return type switch
			{
				Phx.BPowerType.Cleansing => new BPowerCovGlassing(),
				Phx.BPowerType.Orbital => new BPowerUnscMac(),
				Phx.BPowerType.CarpetBombing => new BPowerUnscCarpetBomb(),
				Phx.BPowerType.Cryo => new BPowerUnscCryo(),
				Phx.BPowerType.Rage => new BPowerCovRage(),
				Phx.BPowerType.Wave => new BPowerCovDebris(),
				Phx.BPowerType.Disruption => new BPowerUnscDisruption(),
				Phx.BPowerType.Transport => new BPowerTransport(),
				Phx.BPowerType.ODST => new BPowerUnscOdst(),
				Phx.BPowerType.Repair => new BPowerUnscHeal(),
				_ => throw new KSoft.Debug.UnreachableException(type.ToString()),
			};
		}
	};
}