
using BVector = System.Numerics.Vector4;
using BEntityID = System.Int32;
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
			switch (type)
			{
			case Phx.BPowerType.Cleansing: return new BPowerCovGlassing();
			case Phx.BPowerType.Orbital: return new BPowerUnscMac();
			case Phx.BPowerType.CarpetBombing: return new BPowerUnscCarpetBomb();
			case Phx.BPowerType.Cryo: return new BPowerUnscCryo();
			case Phx.BPowerType.Rage: return new BPowerCovRage();
			case Phx.BPowerType.Wave: return new BPowerCovDebris();
			case Phx.BPowerType.Disruption: return new BPowerUnscDisruption();
			case Phx.BPowerType.Transport: return new BPowerTransport();
			case Phx.BPowerType.ODST: return new BPowerUnscOdst();
			case Phx.BPowerType.Repair: return new BPowerUnscHeal();

			default: throw new KSoft.Debug.UnreachableException(type.ToString());
			}
		}
	};
}