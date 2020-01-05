
using BVector = System.Numerics.Vector4;

namespace KSoft.Phoenix.Runtime
{
	sealed class BPowerCovRageUser
		: BPowerUser
	{
		public float CameraZoom, CameraHeight;
		public uint TimestampNextCommand;
		public BVector MoveInputDir, AttackInputDir;
		public float TimeUntilHint;
		public BVector LastMovePos, LastMoveDir, LastAttackDir;
		public uint CommandInterval;
		public float ScanRadius, MovementProjectionMultiplier;
		public bool HasMoved, HasAttacked, HintShown, 
			HintCompleted, ForceCommandNextUpdate;

		#region IEndianStreamSerializable Members
		public override void Serialize(IO.EndianStream s)
		{
			base.Serialize(s);

			s.Stream(ref CameraZoom); s.Stream(ref CameraHeight);
			s.Stream(ref TimestampNextCommand);
			s.StreamV(ref MoveInputDir); s.StreamV(ref AttackInputDir);
			s.Stream(ref TimeUntilHint);
			s.StreamV(ref LastMovePos); s.StreamV(ref LastMoveDir); s.StreamV(ref LastAttackDir);
			s.Stream(ref CommandInterval);
			s.Stream(ref ScanRadius); s.Stream(ref MovementProjectionMultiplier);
			s.Stream(ref HasMoved); s.Stream(ref HasAttacked); s.Stream(ref HintShown);
			s.Stream(ref HintCompleted); s.Stream(ref ForceCommandNextUpdate);
		}
		#endregion
	};
}