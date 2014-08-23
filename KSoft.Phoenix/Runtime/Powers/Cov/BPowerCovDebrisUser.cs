
using BVector = SlimMath.Vector4;
using BWaveGravityBall = System.UInt64; // idk, 8 bytes

namespace KSoft.Phoenix.Runtime
{
	sealed class BPowerCovDebrisUser
		: BPowerUser
	{
		public BWaveGravityBall FakeGravityBall;
		public BVector HorizontalMoveInputDir, VerticalMoveInputDir, 
			LastUpdatePos, CameraFocusPoint;
		public uint TimestampNextCommand;
		public float TimeUntilHint;
		public uint CommandInterval;
		public float MinBallDistance, MaxBallDistance, MinBallHeight,
			MaxBallHeight, MaxBallSpeedStagnant, MaxBallSpeedPulling,
			CameraDistance, CameraHeight, CameraHoverPointDistance,
			CameraMaxBallAngle, PullingRange, PickupShakeDuration,
			PickupRumbleShakeStrength, PickupCameraShakeStrength, 
			ExplodeTime, DelayShutdownTimeLeft;
		public bool HintShown, HintCompleted, ShuttingDown;

		#region IEndianStreamSerializable Members
		public override void Serialize(IO.EndianStream s)
		{
			base.Serialize(s);

			s.Stream(ref FakeGravityBall);
			s.StreamV(ref HorizontalMoveInputDir); s.StreamV(ref VerticalMoveInputDir);
			s.StreamV(ref LastUpdatePos); s.StreamV(ref CameraFocusPoint);
			s.Stream(ref TimestampNextCommand);
			s.Stream(ref TimeUntilHint);
			s.Stream(ref CommandInterval);
			s.Stream(ref MinBallDistance); s.Stream(ref MaxBallDistance); s.Stream(ref MinBallHeight);
			s.Stream(ref MaxBallHeight); s.Stream(ref MaxBallSpeedStagnant); s.Stream(ref MaxBallSpeedPulling);
			s.Stream(ref CameraDistance); s.Stream(ref CameraHeight); s.Stream(ref CameraHoverPointDistance);
			s.Stream(ref CameraMaxBallAngle); s.Stream(ref PullingRange); s.Stream(ref PickupShakeDuration);
			s.Stream(ref PickupRumbleShakeStrength); s.Stream(ref PickupCameraShakeStrength);
			s.Stream(ref ExplodeTime); s.Stream(ref DelayShutdownTimeLeft);
			s.Stream(ref HintShown); s.Stream(ref HintCompleted); s.Stream(ref ShuttingDown);
		}
		#endregion
	};
}