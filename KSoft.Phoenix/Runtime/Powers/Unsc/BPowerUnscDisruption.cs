
using BVector = SlimMath.Vector4;
using BCueIndex = System.Int32;
using BEntityID = System.Int32;
using BProtoObjectID = System.Int32;

namespace KSoft.Phoenix.Runtime
{
	sealed class BPowerUnscDisruption
		: BPower
	{
		public BEntityID DisruptionObjectID;
		public float DisruptionRadius, DisruptionRadiusSqr, TimeRemainingSec,
			DisruptionStartTime;
		public BVector Direction, Right;
		public BProtoObjectID BomberProtoID, DisruptionObjectProtoID, 
			PulseObjectProtoID, StrikeObjectProtoID;
		public float PulseSpacing, NextPulseTime;
		public int NumPulses;
		public BPowerHelperBomber BomberData = new BPowerHelperBomber();
		public BCueIndex PulseSound;

		#region IEndianStreamSerializable Members
		public override void Serialize(IO.EndianStream s)
		{
			base.Serialize(s);

			s.Stream(ref DisruptionObjectID);
			s.Stream(ref DisruptionRadius); s.Stream(ref DisruptionRadiusSqr); s.Stream(ref TimeRemainingSec);
			s.Stream(ref DisruptionStartTime);
			s.StreamV(ref Direction); s.StreamV(ref Right);
			s.Stream(ref BomberProtoID); s.Stream(ref DisruptionObjectProtoID);
			s.Stream(ref PulseObjectProtoID); s.Stream(ref StrikeObjectProtoID);
			s.Stream(ref PulseSpacing); s.Stream(ref NextPulseTime);
			s.Stream(ref NumPulses);
			s.Stream(BomberData);
			s.Stream(ref PulseSound);
		}
		#endregion
	};
}