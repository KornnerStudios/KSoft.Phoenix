using System.Collections.Generic;

using BWaveGravityBall = System.UInt64; // #TODO real struct
// #REVIEW this should be 12-16 bytes?
// BEntityID ObjectID
// double AddTime
using BQueuedObject = System.UInt64; // idk, 8 bytes

namespace KSoft.Phoenix.Runtime
{
	class BPowerCovDebris : BPower
	{
		public double NextTickTime;
		public BWaveGravityBall RealGravityBall;
		public BVector DesiredBallPosition;
		public List<BEntityID> CapturedUnits = new();
		public float ExplodeCooldownLeft;
		public BEntityID[]? UnitsToPull;
		public BQueuedObject[]? QueuedPickupObjects;
		public BCostDatum[]? CostPerTick;
		public float TickLength;
		public BProtoObjectID BallProtoID, LightningProtoID, LightningBeamVisualProtoID,
			DebrisProtoID, ExplodeProtoID, PickupAttachmentProtoID;
		public float AudioReactionTimer;
		public uint LeaderAnimOrderID;
		public float MaxBallSpeedStagnant, MaxBallSpeedPulling, ExplodeTime,
			PullingRange, ExplosionForceOnDebris, HealthToCapture,
			NudgeStrength, InitialLateralPullStrength, CapturedRadialSpacing,
			CapturedSpringStrength, CapturedSpringDampening, CapturedSpringRestLength,
			CapturedMinLateralSpeed, RipAttachmentChancePulling, PickupObjectRate,
			DebrisAngularDamping, CurrentExplosionDamageBank, MaxPossibleExplosionDamageBank,
			MaxExplosionDamageBankPerCaptured, ExplosionDamageBankPerTick;
		public uint CommandInterval;
		public float MinBallDistance, MaxBallDistance;
		public int LightningPerTick, MaxCapturedObjects;
		public byte NudgeChancePulling, ThrowPartChancePulling, LightningChancePulling;
		public BCueIndex ExplodeSound;
		public float MinDamageBankPercentToThrow;
		public BTeamID[]? RevealedTeamIDs;
		BProtoActionTacticUnion unknown0, unknown1, unknown2;
		public bool CompletedInitialization, ThrowUnitsOnExplosion;

		#region IEndianStreamSerializable Members
		public override void Serialize(IO.EndianStream s)
		{
			base.Serialize(s);

			var owner = s.Owner;
			System.ArgumentNullException.ThrowIfNull(owner);
			var sg = KSoft.Debug.TypeCheck.CastReference<BSaveGame>(owner);

			s.Stream(ref NextTickTime);
			s.Stream(ref RealGravityBall);
			s.StreamV(ref DesiredBallPosition);
			BSaveGame.StreamCollection(s, CapturedUnits);
			s.Stream(ref ExplodeCooldownLeft);
			BEntityID[] unitsToPull = s.IsReading
				? System.Array.Empty<BEntityID>()
				: UnitsToPull ?? throw new System.ArgumentNullException(nameof(UnitsToPull));
			BQueuedObject[] queuedPickupObjects = s.IsReading
				? System.Array.Empty<BQueuedObject>()
				: QueuedPickupObjects ?? throw new System.ArgumentNullException(nameof(QueuedPickupObjects));
			BCostDatum[] costPerTick = s.IsReading
				? System.Array.Empty<BCostDatum>()
				: CostPerTick ?? throw new System.ArgumentNullException(nameof(CostPerTick));
			BSaveGame.StreamArray(s, ref unitsToPull);
			BSaveGame.StreamArray(s, ref queuedPickupObjects);
			sg.StreamBCost(s, ref costPerTick);
			UnitsToPull = unitsToPull;
			QueuedPickupObjects = queuedPickupObjects;
			CostPerTick = costPerTick;
			s.Stream(ref TickLength);
			s.Stream(ref BallProtoID); s.Stream(ref LightningProtoID); s.Stream(ref LightningBeamVisualProtoID);
			s.Stream(ref DebrisProtoID); s.Stream(ref ExplodeProtoID); s.Stream(ref PickupAttachmentProtoID);
			s.Stream(ref AudioReactionTimer);
			s.Stream(ref LeaderAnimOrderID);

			s.Stream(ref MaxBallSpeedStagnant); s.Stream(ref MaxBallSpeedPulling); s.Stream(ref ExplodeTime);
			s.Stream(ref PullingRange); s.Stream(ref ExplosionForceOnDebris); s.Stream(ref HealthToCapture);
			s.Stream(ref NudgeStrength); s.Stream(ref InitialLateralPullStrength); s.Stream(ref CapturedRadialSpacing);
			s.Stream(ref CapturedSpringStrength); s.Stream(ref CapturedSpringDampening); s.Stream(ref CapturedSpringRestLength);
			s.Stream(ref CapturedMinLateralSpeed); s.Stream(ref RipAttachmentChancePulling); s.Stream(ref PickupObjectRate);
			s.Stream(ref DebrisAngularDamping); s.Stream(ref CurrentExplosionDamageBank); s.Stream(ref MaxPossibleExplosionDamageBank);
			s.Stream(ref MaxExplosionDamageBankPerCaptured); s.Stream(ref ExplosionDamageBankPerTick);
			s.Stream(ref CommandInterval);
			s.Stream(ref MinBallDistance); s.Stream(ref MaxBallDistance);
			s.Stream(ref LightningPerTick); s.Stream(ref MaxCapturedObjects);
			s.Stream(ref NudgeChancePulling); s.Stream(ref ThrowPartChancePulling); s.Stream(ref LightningChancePulling);
			s.Stream(ref ExplodeSound);
			s.Stream(ref MinDamageBankPercentToThrow);
			BTeamID[] revealedTeamIDs = s.IsReading
				? System.Array.Empty<BTeamID>()
				: RevealedTeamIDs ?? throw new System.ArgumentNullException(nameof(RevealedTeamIDs));
			BSaveGame.StreamArray(s, ref revealedTeamIDs);
			RevealedTeamIDs = revealedTeamIDs;
			s.Stream(ref unknown0); s.Stream(ref unknown1); s.Stream(ref unknown2);
			s.Stream(ref CompletedInitialization); s.Stream(ref ThrowUnitsOnExplosion);
		}
		#endregion
	};
}
