using System.Collections.Generic;
#if CONTRACTS_FULL_SHIM
using Contract = System.Diagnostics.ContractsShim.Contract;
#else
using Contract = System.Diagnostics.Contracts.Contract; // SHIM'D
#endif

using BVector = System.Numerics.Vector4;
using BEntityID = System.Int32;
using BPlayerID = System.Int32;
using BTeamID = System.Int32;
using BPlayerState = System.UInt32; // states are defined in GameData.xml
using BCost = System.Single;
using BPowerLevel = System.UInt32; // idk, 4 bytes

namespace KSoft.Phoenix.Runtime
{
	using BRallyPoint = System.Numerics.Vector4; // this is only a guess

	partial class cSaveMarker
	{
		public const ushort
			Player1 = 0x2710, Player2 = 0x2711, Player3 = 0x2712, Player4 = 0x2713
			;
	};

	sealed class BPlayer
		: IO.IEndianStreamSerializable
	{
		public struct PowerInfo : IO.IEndianStreamSerializable
		{
			public uint RechargeTime;
			public int UseLimit;
			public BPowerLevel Level;
			public uint AvailableTime;

			#region IEndianStreamSerializable Members
			public void Serialize(IO.EndianStream s)
			{
				s.Stream(ref RechargeTime);
				s.Stream(ref UseLimit);
				s.Stream(ref Level);
				s.Stream(ref AvailableTime);
			}
			#endregion
		};
		public struct RateInfo
			: IO.IEndianStreamSerializable
		{
			public float Amount, Multiplier;

			#region IEndianStreamSerializable Members
			public void Serialize(IO.EndianStream s)
			{
				s.Stream(ref Amount);
				s.Stream(ref Multiplier);
			}
			#endregion
		};
		#region UnitCountInfo
		public struct UnitCountInfo
			: IO.IEndianStreamSerializable
		{
			public uint FutureUnitCount, DeadUnitCount;

			#region IEndianStreamSerializable Members
			public void Serialize(IO.EndianStream s)
			{
				s.Stream(ref FutureUnitCount);
				s.Stream(ref DeadUnitCount);
			}
			#endregion
		};
		static readonly CondensedListInfo kUnitCountsListInfo = new CondensedListInfo()
		{
			IndexSize=sizeof(short),
		};
		#endregion
		#region WeaponType
		public struct WeaponType
			: IO.IEndianStreamSerializable
		{
			public sbyte DamageType;
			public float Modifier;

			#region IEndianStreamSerializable Members
			public void Serialize(IO.EndianStream s)
			{
				s.Stream(ref DamageType);
				s.Stream(ref Modifier);
			}
			#endregion
		};
		// Index = actual WeaponType
		static readonly CondensedListInfo kWeaponTypesListInfo = new CondensedListInfo()
		{
			IndexSize=sizeof(sbyte),
			MaxCount=0x4E20,
		};
		#endregion

		#region Player1
		static readonly CondensedListInfo kProtoUnitsListInfo = new CondensedListInfo()
		{
			SerializeCapacity=true,
			IndexSize=sizeof(short),
			MaxCount=0x4E20,
		};
		static readonly CondensedListInfo kProtoTechsListInfo = new CondensedListInfo()
		{
			SerializeCapacity=true,
			IndexSize=sizeof(short),
			MaxCount=0x1388,
		};
		static readonly CondensedListInfo kProtoUniqueUnitsListInfo = new CondensedListInfo()
		{
			SerializeCapacity=true,
			IndexSize=sizeof(short),
			MaxCount=0x3E8,
		};

		public BVector LookAtPos; // Gaia's values are ints, not floats...
		public string Name;
		public BRallyPoint RallyPoint; // Gaia's values are ints, not floats...
		public BStatsManager StatsManager = new BStatsManager();
		public List<CondensedListItem16<BProtoObject>> ProtoObjects;
		public List<CondensedListItem16<BProtoSquad>> ProtoSquads;
		public List<CondensedListItem16<BProtoTech>> ProtoTechs;
		public BProtoObject[] UniqueProtoObjects;
		public BProtoSquad[] UniqueProtoSquad;
		public BPowerEntry[] PowerEntries;
		public int[] Abilities;
		public PowerInfo[] Powers;
		public BCost[] Resources;
		public RateInfo[] Rates;
		public BCost[] TotalResources;
		public BCost[] ResourceTrickleRate;
		public BPlayerPop[] Populations;
		#endregion
		#region Player2
		public BHintEngine HintEngine;
		public List<CondensedListItemValue16<UnitCountInfo>> GenericObjectCounts, SquadCounts;
		#endregion
		#region Player3
		public uint TotalFutureUnitCounts, TotalDeadUnitCounts;
		public uint TotalFutureSquadCounts, TotalDeadSquadCounts;
		public BEntityID[] GotoBases;
		public List<CondensedListItemValue8<WeaponType>> WeaponTypes;
		public float[] AbilityRecoverTimes;
		public BTechTree TechTree = new BTechTree();
		#endregion
		#region Player4
		public int MPID, ColorIndex;
		public BPlayerID ID, CoopID, ScenarioID;
		public int CivID;
		public BTeamID TeamID;
		public BPlayerState PlayerState;
		public int LeaderID, BountyResource;
		public BEntityID RallyObject;
		public int Strength;
		public float TributeCost;
		public BCost[] RepairCost;
		public float RepairTime, HandicapMultiplier, ShieldRegenRate;
		public uint ShieldRegenDelay;
		public float TotalCombatValue;
		public float Difficulty;
		public uint GamePlayedTime;
		public BPlayerID FloodPoofPlayer;
		public sbyte PlayerType, SquadSearchAttempts;
		public float WeaponPhysicsMultiplier, AIDamageMultiplier, AIDamageTakenMultiplier,
			AIBuildSpeedMultiplier;
		public bool FlagRallyPoint, FlagBountyResource, FlagMinimapBlocked,
			FlagLeaderPowersBlocked, FlagDefeatedDestroy;
		public int SquadAISearchIndex, SquadAIWorkIndex, SquadAISecondaryTurretScanIndex;
		#endregion

		#region IEndianStreamSerializable Members
		long mPositionMarker;
		public void Serialize(IO.EndianStream s)
		{
			var sg = s.Owner as BSaveGame;

			#region Init
			if (s.IsReading)
			{
				ProtoObjects = new List<CondensedListItem16<BProtoObject>>(sg.Database.GenericProtoObjects.Count);
				ProtoSquads = new List<CondensedListItem16<BProtoSquad>>(sg.Database.ProtoSquads.Count);
				ProtoTechs = new List<CondensedListItem16<BProtoTech>>(sg.Database.ProtoTechs.Count);

				Powers = new PowerInfo[sg.Database.ProtoPowers.Count];
				Rates = new RateInfo[sg.Database.Rates.Count];
				Populations = new BPlayerPop[sg.Database.Populations.Count];

				GenericObjectCounts = new List<CondensedListItemValue16<UnitCountInfo>>(sg.Database.GenericProtoObjects.Count);
				SquadCounts = new List<CondensedListItemValue16<UnitCountInfo>>(sg.Database.ProtoSquads.Count);
				WeaponTypes = new List<CondensedListItemValue8<WeaponType>>(sg.Database.WeaponTypes.Count);
			}
			#endregion

			#region Player1
			s.StreamV(ref LookAtPos);
			s.StreamPascalString32(ref Name);
			s.StreamV(ref RallyPoint);
			s.Stream(StatsManager);
			BSaveGame.StreamList(s, ProtoObjects, kProtoUnitsListInfo);
			BSaveGame.StreamList(s, ProtoSquads, kProtoUnitsListInfo);
			BSaveGame.StreamList(s, ProtoTechs, kProtoTechsListInfo);
			BSaveGame.StreamArray16(s, ref UniqueProtoObjects);
			Contract.Assert(UniqueProtoObjects.Length <= kProtoUniqueUnitsListInfo.MaxCount);
			BSaveGame.StreamArray16(s, ref UniqueProtoSquad);
			Contract.Assert(UniqueProtoSquad.Length <= kProtoUniqueUnitsListInfo.MaxCount);
			BSaveGame.StreamArray(s, ref PowerEntries);
			BSaveGame.StreamArray(s, ref Abilities);
			for (int x = 0; x < Powers.Length; x++)
				s.Stream(ref Powers[x]);
			sg.StreamBCost(s, ref Resources);
			for (int x = 0; x < Rates.Length; x++)
				s.Stream(ref Rates[x]);
			sg.StreamBCost(s, ref TotalResources);
			sg.StreamBCost(s, ref ResourceTrickleRate);
			for (int x = 0; x < Populations.Length; x++)
				s.Stream(ref Populations[x]);
			s.StreamSignature(cSaveMarker.Player1);
			#endregion
			#region Player2
			s.StreamNotNull(ref HintEngine);
			BSaveGame.StreamList(s, GenericObjectCounts, kUnitCountsListInfo);
			BSaveGame.StreamList(s, SquadCounts, kUnitCountsListInfo);
			s.StreamSignature(cSaveMarker.Player2);
			#endregion
			#region Player3
			s.Stream(ref TotalFutureUnitCounts); s.Stream(ref TotalDeadUnitCounts);
			s.Stream(ref TotalFutureSquadCounts); s.Stream(ref TotalDeadSquadCounts);
			BSaveGame.StreamArray(s, ref GotoBases);
			BSaveGame.StreamList(s, WeaponTypes, kWeaponTypesListInfo);
			BSaveGame.StreamArray16(s, ref AbilityRecoverTimes);
			s.Stream(TechTree);
			s.StreamSignature(cSaveMarker.Player3);
			#endregion
			#region Player4
			s.Stream(ref MPID); s.Stream(ref ColorIndex);
			s.Stream(ref ID); s.Stream(ref CoopID); s.Stream(ref ScenarioID);
			s.Stream(ref CivID);
			s.Stream(ref TeamID);
			s.Stream(ref PlayerState);
			s.Stream(ref LeaderID); s.Stream(ref BountyResource);
			s.Stream(ref RallyObject);
			s.Stream(ref Strength);
			s.Stream(ref TributeCost);
			sg.StreamBCost(s, ref RepairCost);
			s.Stream(ref RepairTime); s.Stream(ref HandicapMultiplier); s.Stream(ref ShieldRegenRate);
			s.Stream(ref ShieldRegenDelay);
			s.Stream(ref TotalCombatValue);
			s.Stream(ref Difficulty);
			s.Stream(ref GamePlayedTime);
			s.Stream(ref FloodPoofPlayer);
			s.Stream(ref PlayerType); s.Stream(ref SquadSearchAttempts);
			s.Stream(ref WeaponPhysicsMultiplier); s.Stream(ref AIDamageMultiplier); s.Stream(ref AIDamageTakenMultiplier);
			s.Stream(ref AIBuildSpeedMultiplier);
			s.Stream(ref FlagRallyPoint); s.Stream(ref FlagBountyResource); s.Stream(ref FlagMinimapBlocked);
			s.Stream(ref FlagLeaderPowersBlocked); s.Stream(ref FlagDefeatedDestroy);
			s.Stream(ref SquadAISearchIndex); s.Stream(ref SquadAIWorkIndex); s.Stream(ref SquadAISecondaryTurretScanIndex);
			s.StreamSignature(cSaveMarker.Player4);
			#endregion
			s.TraceAndDebugPosition(ref mPositionMarker);
		}
		#endregion
	};
}