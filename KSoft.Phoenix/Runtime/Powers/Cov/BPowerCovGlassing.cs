
using BVector = SlimMath.Vector4;
using BCost = System.Single;
using BEntityID = System.Int32;
using BProtoObjectID = System.Int32;

namespace KSoft.Phoenix.Runtime
{
	sealed class BPowerCovGlassing
		: BPower
	{
		const int cMaximumWaypoints = 0xC8;
		const int cMaximumBeamPathLength = 0xC8;

		public BVector[] Waypoints;
		public BEntityID BeamID, AirImpactObjectID;
		public double NextDamageTime;
		public BVector DesiredBeamPosition;
		public BVector[] BeamPath;
		public int[] RevealedTeamIDs; // BTeamID
		public BCost[] CostPerTick;
		public BProtoObjectID Projectile;
		public float TickLength, MinBeamDistance, MaxBeamDistance;
		public uint CommandInterval;
		public float MaxBeamSpeed;
		public int LOSMode;
		public bool UsePath;
		public float AudioReactionTimer;

		#region IEndianStreamSerializable Members
		public override void Serialize(IO.EndianStream s)
		{
			base.Serialize(s);
			var sg = s.Owner as BSaveGame;

			BSaveGame.StreamVectorArray(s, ref Waypoints, cMaximumWaypoints);
			s.Stream(ref BeamID); s.Stream(ref AirImpactObjectID);
			s.Stream(ref NextDamageTime);
			s.StreamV(ref DesiredBeamPosition);
			BSaveGame.StreamVectorArray(s, ref BeamPath, cMaximumBeamPathLength);
			BSaveGame.StreamArray(s, ref RevealedTeamIDs);
			sg.StreamBCost(s, ref CostPerTick);
			s.Stream(ref Projectile);
			s.Stream(ref TickLength); s.Stream(ref MinBeamDistance); s.Stream(ref MaxBeamDistance);
			s.Stream(ref CommandInterval);
			s.Stream(ref MaxBeamSpeed);
			s.Stream(ref LOSMode);
			s.Stream(ref UsePath);
			s.Stream(ref AudioReactionTimer);
		}
		#endregion
	};
}