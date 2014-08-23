
using BVector = SlimMath.Vector4;
using BEntityID = System.Int32;
using BProtoObjectID = System.Int32;

namespace KSoft.Phoenix.Runtime
{
	sealed class BPowerUnscCarpetBombUser
		: BPowerUser
	{
		public sbyte InputState;
		public BVector Position, DesiredForward;
		public float DesiredScale, CurrentScale;
		public double ShutdownTime;
		public BEntityID ArrowID;
		public float MaxBombOffset, LengthMultiplier;
		public sbyte LOSMode;
		public BProtoObjectID ArrowProtoID;
		public BPowerHelperHudSounds HudSounds = new BPowerHelperHudSounds();

		#region IEndianStreamSerializable Members
		public override void Serialize(IO.EndianStream s)
		{
			base.Serialize(s);

			s.Stream(ref InputState);
			s.StreamV(ref Position); s.StreamV(ref DesiredForward);
			s.Stream(ref DesiredScale); s.Stream(ref CurrentScale);
			s.Stream(ref ShutdownTime);
			s.Stream(ref ArrowID);
			s.Stream(ref MaxBombOffset); s.Stream(ref LengthMultiplier);
			s.Stream(ref LOSMode);
			s.Stream(ref ArrowProtoID);
			s.Stream(HudSounds);
		}
		#endregion
	};
}