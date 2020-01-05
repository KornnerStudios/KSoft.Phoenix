
using BVector = System.Numerics.Vector4;
using BCueIndex = System.Int32;
using BEntityID = System.Int32;
using BProtoObjectID = System.Int32;

namespace KSoft.Phoenix.Runtime
{
	sealed class BPowerUnscMac
		: BPower
	{
		public sealed class BShot
			: IO.IEndianStreamSerializable
		{
			public BVector LaunchPos, TargetPos;
			public uint LaunchTime, CreateLaserTime;
			public BEntityID LaserObj;
			public bool LaserCreated;

			#region IEndianStreamSerializable Members
			public void Serialize(IO.EndianStream s)
			{
				s.StreamV(ref LaunchPos); s.StreamV(ref TargetPos);
				s.Stream(ref LaunchTime); s.Stream(ref CreateLaserTime);
				s.Stream(ref LaserObj);
				s.Stream(ref LaserCreated);
			}
			#endregion
		};

		public BEntityID RealTargettingLaserID;
		public BVector DesiredTargettingPosition;
		public BShot[] Shots;
		public bool FiredInitialShot;
		public uint ShotsRemaining, ImpactsToProcess;
		public BProtoObjectID TargetBeamID, ProjectileID, EffectProtoID,
			RockSmallProtoID, RockMediumProtoID, RockLargeProtoID;
		public BCueIndex FiredSound;
		public uint TargetingDelay, AutoShotDelay;
		public float AutoShotInnerRadius, AutoShotOuterRadius, 
			XOffset, YOffset, ZOffset;
		public int LOSMode;

		#region IEndianStreamSerializable Members
		public override void Serialize(IO.EndianStream s)
		{
			base.Serialize(s);

			s.Stream(ref RealTargettingLaserID);
			s.StreamV(ref DesiredTargettingPosition);
			BSaveGame.StreamArray(s, ref Shots);
			s.Stream(ref FiredInitialShot);
			s.Stream(ref ShotsRemaining); s.Stream(ref ImpactsToProcess);
			s.Stream(ref TargetBeamID); s.Stream(ref ProjectileID); s.Stream(ref EffectProtoID);
			s.Stream(ref RockSmallProtoID); s.Stream(ref RockMediumProtoID); s.Stream(ref RockLargeProtoID);
			s.Stream(ref FiredSound);
			s.Stream(ref TargetingDelay); s.Stream(ref AutoShotDelay);
			s.Stream(ref AutoShotInnerRadius); s.Stream(ref AutoShotOuterRadius);
			s.Stream(ref XOffset); s.Stream(ref YOffset); s.Stream(ref ZOffset);
			s.Stream(ref LOSMode);
		}
		#endregion
	};
}