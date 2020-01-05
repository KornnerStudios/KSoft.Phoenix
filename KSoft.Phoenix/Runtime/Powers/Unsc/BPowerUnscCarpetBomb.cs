using System.Collections.Generic;

using BVector = System.Numerics.Vector4;
using BEntityID = System.Int32;
using BProtoObjectID = System.Int32;

namespace KSoft.Phoenix.Runtime
{
	sealed class BPowerUnscCarpetBomb
		: BPower
	{
		public sealed class BBombExplodeInfo
			: IO.IEndianStreamSerializable
		{
			public ulong Unknown0, Unknown8, Unknown10, Unknown18;

			#region IEndianStreamSerializable Members
			public void Serialize(IO.EndianStream s)
			{
				s.Stream(ref Unknown0); s.Stream(ref Unknown8); s.Stream(ref Unknown10); s.Stream(ref Unknown18);
			}
			#endregion
		};

		public BBombExplodeInfo[] BombExplodeInfos;
		public List<BEntityID> NudgedUnits = new List<BEntityID>();
		public BVector StartLocation, StartDirection, RightVector;
		public sbyte State;
		public bool GotStartLocation, GotStartDirection;
		public double TickLength, NextBombTime, LastBombTime;
		public uint NumBombClustersDropped;
		public BProtoObjectID ProjectileProtoID, ImpactProtoID, ExplosionProtoID, BomberProtoID;
		public float InitialDelay, FuseTime;
		public uint MaxBombs;
		public float MaxBombOffset, BombSpacing, LengthMultiplier,
			WedgeLengthMultiplier, WedgeMinOffset, NudgeMultiplier;
		public BPowerHelperBomber BomberData = new BPowerHelperBomber();
		public sbyte LOSMode;
		public bool ReactionPlayed;

		#region IEndianStreamSerializable Members
		public override void Serialize(IO.EndianStream s)
		{
			base.Serialize(s);

			BSaveGame.StreamArray(s, ref BombExplodeInfos);
			BSaveGame.StreamCollection(s, NudgedUnits);
			s.StreamV(ref StartLocation); s.StreamV(ref StartDirection); s.StreamV(ref RightVector);
			s.Stream(ref State);
			s.Stream(ref GotStartLocation); s.Stream(ref GotStartDirection);
			s.Stream(ref TickLength); s.Stream(ref NextBombTime); s.Stream(ref LastBombTime);
			s.Stream(ref NumBombClustersDropped);
			s.Stream(ref ProjectileProtoID); s.Stream(ref ImpactProtoID); s.Stream(ref ExplosionProtoID); s.Stream(ref BomberProtoID);
			s.Stream(ref InitialDelay); s.Stream(ref FuseTime);
			s.Stream(ref MaxBombs);
			s.Stream(ref MaxBombOffset); s.Stream(ref BombSpacing); s.Stream(ref LengthMultiplier);
			s.Stream(ref WedgeLengthMultiplier); s.Stream(ref WedgeMinOffset); s.Stream(ref NudgeMultiplier);
			s.Stream(BomberData);
			s.Stream(ref LOSMode);
			s.Stream(ref ReactionPlayed);
		}
		#endregion
	};
}