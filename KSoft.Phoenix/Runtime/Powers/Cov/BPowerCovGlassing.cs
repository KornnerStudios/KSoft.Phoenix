
namespace KSoft.Phoenix.Runtime
{
	sealed class BPowerCovGlassing
		: BPower
	{
		const int cMaximumWaypoints = 0xC8;
		const int cMaximumBeamPathLength = 0xC8;

		public BVector[]? Waypoints;
		public BEntityID BeamID, AirImpactObjectID;
		public double NextDamageTime;
		public BVector DesiredBeamPosition;
		public BVector[]? BeamPath;
		public int[]? RevealedTeamIDs; // BTeamID
		public BCostDatum[]? CostPerTick;
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
			var owner = s.Owner;
			System.ArgumentNullException.ThrowIfNull(owner);
			var sg = KSoft.Debug.TypeCheck.CastReference<BSaveGame>(owner);

			BVector[] waypoints = s.IsReading
				? System.Array.Empty<BVector>()
				: Waypoints ?? throw new System.ArgumentNullException(nameof(Waypoints));
			BSaveGame.StreamVectorArray(s, ref waypoints, cMaximumWaypoints);
			Waypoints = waypoints;
			s.Stream(ref BeamID); s.Stream(ref AirImpactObjectID);
			s.Stream(ref NextDamageTime);
			s.StreamV(ref DesiredBeamPosition);
			BVector[] beamPath = s.IsReading
				? System.Array.Empty<BVector>()
				: BeamPath ?? throw new System.ArgumentNullException(nameof(BeamPath));
			int[] revealedTeamIDs = s.IsReading
				? System.Array.Empty<int>()
				: RevealedTeamIDs ?? throw new System.ArgumentNullException(nameof(RevealedTeamIDs));
			BCostDatum[] costPerTick = s.IsReading
				? System.Array.Empty<BCostDatum>()
				: CostPerTick ?? throw new System.ArgumentNullException(nameof(CostPerTick));
			BSaveGame.StreamVectorArray(s, ref beamPath, cMaximumBeamPathLength);
			BSaveGame.StreamArray(s, ref revealedTeamIDs);
			sg.StreamBCost(s, ref costPerTick);
			BeamPath = beamPath;
			RevealedTeamIDs = revealedTeamIDs;
			CostPerTick = costPerTick;
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
