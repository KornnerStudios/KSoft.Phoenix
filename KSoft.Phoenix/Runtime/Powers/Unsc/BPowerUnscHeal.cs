
using BEntityID = System.Int32;
using BEntityTimePair = System.UInt64;
using BProtoObjectID = System.Int32;
using BObjectTypeID = System.Int32;

namespace KSoft.Phoenix.Runtime
{
	sealed class BPowerUnscHeal
		: BPower
	{
		public BEntityID[] SquadsRepairing;
		public BEntityTimePair[] IgnoreList;
		public uint NextTickTime;
		public BEntityID RepairObjectID;
		public BProtoObjectID RepairAttachmentProtoID;
		public BObjectTypeID FilterTypeID;
		public float RepairRadius;
		public uint TickDuration;
		public float RepairCombatValuePerTick;
		public uint CooldownTimeIfDamaged, TicksRemaining;
		public bool SpreadAmongSquads, AllowReinforce, IgnorePlacement,
			HealAny, NeverStops;

		#region IEndianStreamSerializable Members
		public override void Serialize(IO.EndianStream s)
		{
			base.Serialize(s);

			BSaveGame.StreamArray(s, ref SquadsRepairing);
			BSaveGame.StreamArray(s, ref IgnoreList);
			s.Stream(ref NextTickTime);
			s.Stream(ref RepairObjectID);
			s.Stream(ref RepairAttachmentProtoID);
			s.Stream(ref FilterTypeID);
			s.Stream(ref RepairRadius);
			s.Stream(ref TickDuration);
			s.Stream(ref RepairCombatValuePerTick);
			s.Stream(ref CooldownTimeIfDamaged); s.Stream(ref TicksRemaining);
			s.Stream(ref SpreadAmongSquads); s.Stream(ref AllowReinforce); s.Stream(ref IgnorePlacement);
			s.Stream(ref HealAny); s.Stream(ref NeverStops);
		}
		#endregion
	};
}