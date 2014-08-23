
using BVector = SlimMath.Vector4;
using BEntityID = System.Int32;

namespace KSoft.Phoenix.Runtime
{
	sealed class BPowerCovGlassingUser
		: BPowerUser
	{
		public BVector InputDir, LastUpdatePos;
		public BEntityID RealBeamID, FakeBeamID, AirImpactObjectID;
		public uint TimestampNextCommand, CommandInterval;
		public float MinBeamDistance, MaxBeamDistance, MaxBeamSpeed;
		public int LOSMode;

		#region IEndianStreamSerializable Members
		public override void Serialize(IO.EndianStream s)
		{
			base.Serialize(s);

			s.StreamV(ref InputDir); s.StreamV(ref LastUpdatePos);
			s.Stream(ref RealBeamID); s.Stream(ref FakeBeamID); s.Stream(ref AirImpactObjectID);
			s.Stream(ref TimestampNextCommand); s.Stream(ref CommandInterval);
			s.Stream(ref MinBeamDistance); s.Stream(ref MaxBeamDistance); s.Stream(ref MaxBeamSpeed);
			s.Stream(ref LOSMode);
		}
		#endregion
	};
}