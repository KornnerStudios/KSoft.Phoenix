#if CONTRACTS_FULL_SHIM
using Contract = System.Diagnostics.ContractsShim.Contract;
#else
using Contract = System.Diagnostics.Contracts.Contract; // SHIM'D
#endif

using BProtoObjectID = System.Int32;
using BProtoObjectTrainLimit = System.Int32; // idk, 4 bytes

namespace KSoft.Phoenix.Runtime
{
	partial class cSaveMarker
	{
		public const ushort
			ProtoObject = 0x2710
			;
	};

	sealed class BProtoObject
		: BProtoObjectBase
	{
		const int kTrainLimitsMaxCount = 0x64;

		public BProtoObjectID ProtoID;
		public BProtoObjectTrainLimit[] TrainLimits;
		public BHardpoint[] Hardpoints;

		public int ProtoVisualIndex;
		public float DesiredVelocity, MaxVelocity;
		public float Hitpoints, Shieldpoints;
		public float LOS;
		public int SimLOS;

		public float Bounty;
		public BTactic Tactic;
		public float AmmoMax, AmmoRegenRate, RateAmount;
		public int MaxContained, DisplayNameIndex, CircleMenuIconID;
		public int DeathSpawnSquad;
		public byte[] CommandDisabled; // utbitvector; count=4
		public byte[] CommandSelectable; // utbitvector; count=4
		public bool AbilityDisabled,
			AutoCloak, CloakMove, CloakAttack,
			UniqueInstance;

		public void CommandDisabledNone()
		{
			for (int x = 0; x < CommandDisabled.Length; x++)
				CommandDisabled[x] = 0;
		}

		#region IEndianStreamSerializable Members
		public override void Serialize(IO.EndianStream s)
		{
			base.Serialize(s);
			var sg = s.Owner as BSaveGame;

			s.Stream(ref ProtoID);
			BSaveGame.StreamArray(s, ref TrainLimits);
			Contract.Assert(TrainLimits.Length <= kTrainLimitsMaxCount);
			BSaveGame.StreamArray(s, ref Hardpoints);
			Contract.Assert(Hardpoints.Length <= BHardpoint.kMaxCount);
			sg.StreamBCost(s, ref Cost);
			s.Stream(ref ProtoVisualIndex);
			s.Stream(ref DesiredVelocity); s.Stream(ref MaxVelocity);
			s.Stream(ref Hitpoints); s.Stream(ref Shieldpoints);
			s.Stream(ref LOS);
			s.Stream(ref SimLOS);
			s.Stream(ref BuildPoints);
			s.Stream(ref Bounty);
			s.StreamNotNull(ref Tactic);
			s.Stream(ref AmmoMax); s.Stream(ref AmmoRegenRate); s.Stream(ref RateAmount);
			s.Stream(ref MaxContained); s.Stream(ref DisplayNameIndex); s.Stream(ref CircleMenuIconID);
			s.Stream(ref DeathSpawnSquad);
			BSaveGame.StreamArray(s, ref CommandDisabled);
			BSaveGame.StreamArray(s, ref CommandSelectable);
			s.Stream(ref Available); s.Stream(ref Forbid); s.Stream(ref AbilityDisabled);
			s.Stream(ref AutoCloak); s.Stream(ref CloakMove); s.Stream(ref CloakAttack);
			s.Stream(ref UniqueInstance);
			s.StreamSignature(cSaveMarker.ProtoObject);
		}
		#endregion

		#region IIndentedStreamWritable Members
#if false
		public void ToStream(IO.IndentedTextWriter s)
		{
			s.WriteLine("{0} {1}{2}{3}{4}", "CommandDisabled",
				CommandDisabled[0].ToString("X2"), CommandDisabled[1].ToString("X2"),
				CommandDisabled[2].ToString("X2"), CommandDisabled[3].ToString("X2"));
			s.WriteLine("{0} {1}{2}{3}{4}", "CommandSelectable",
				CommandSelectable[0].ToString("X2"), CommandSelectable[1].ToString("X2"),
				CommandSelectable[2].ToString("X2"), CommandSelectable[3].ToString("X2"));
		}
#endif
		#endregion
	};
}