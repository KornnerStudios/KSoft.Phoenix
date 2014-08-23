
using BVector = SlimMath.Vector4;
using BEntityID = System.Int32;
using BEntityTimePair = System.UInt64;
using BProtoObjectID = System.Int32;
using BObjectTypeID = System.Int32;

namespace KSoft.Phoenix.Runtime
{
	sealed class BPowerUnscCryo
		: BPower
	{
		public BEntityTimePair[] IgnoreList;
		public uint NextTickTime;
		public BEntityID CryoObjectID;
		public BVector Direction, Right;
		public BProtoObjectID CryoObjectProtoID, BomberProtoID;
		public BObjectTypeID FilterTypeID;
		public float CryoRadius, MinCryoFalloff;
		public uint TickDuration, TicksRemaining;
		public float CryoAmountPerTick, KillableHpLeft, FreezingThawTime, FrozenThawTime;
		public BPowerHelperBomber BomberData = new BPowerHelperBomber();
		public bool ReactionPlayed;

		#region IEndianStreamSerializable Members
		public override void Serialize(IO.EndianStream s)
		{
			base.Serialize(s);

			BSaveGame.StreamArray(s, ref IgnoreList);
			s.Stream(ref NextTickTime);
			s.Stream(ref CryoObjectID);
			s.StreamV(ref Direction); s.StreamV(ref Right);
			s.Stream(ref CryoObjectProtoID); s.Stream(ref BomberProtoID);
			s.Stream(ref FilterTypeID);
			s.Stream(ref CryoRadius); s.Stream(ref MinCryoFalloff);
			s.Stream(ref TickDuration); s.Stream(ref TicksRemaining);
			s.Stream(ref CryoAmountPerTick); s.Stream(ref KillableHpLeft); s.Stream(ref FreezingThawTime); s.Stream(ref FrozenThawTime);
			s.Stream(BomberData);
			s.Stream(ref ReactionPlayed);
		}
		#endregion
	};
}