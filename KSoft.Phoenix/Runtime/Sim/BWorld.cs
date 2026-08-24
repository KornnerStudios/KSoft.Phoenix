using System.Collections.Generic;

namespace KSoft.Phoenix.Runtime
{
	partial class cSaveMarker
	{
		public const ushort
			World1 = 0x2710, World2 = 0x2711, World3 = 0x2712, World4 = 0x2713, World5 = 0x2714,
			Players = 0x2715,
			Teams = 0x2716,
			SimOrder = 0x2717,
			UnitOpp = 0x2718,
			PathMoveData = 0x2719,
			Platoons = 0x271A,
			Dopples = 0x271B,
			Projectiles = 0x271C,
			AirSpots = 0x271D,
			Armies = 0x271E,
			Squads = 0x271F,

			Units = 0x2720,
			ObjectiveManager = 0x2721,
			GeneralEvents = 0x2722,
			Triggers = 0x2723,
			Visibilty = 0x2724,
			ScoreManager = 0x2725,
			StoredAnimEventManager = 0x2726,
			EntityScheduler = 0x2727,
			CollectiblesManager = 0x2728,
			Objects = 0x2729
			;

		public const ushort ObjectAnimEventTagQueue_DoneIndex = 0x9C5;

		public const byte BActionController__cNumberControllers = 2;
	};

	sealed class BWorld
		: IO.IEndianStreamSerializable
	{
		public const byte cMaximumSupportedPlayers = 9,
			cMaxPlayerColorCategories = 2,
			cMaximumSupportedTeams = 5;

		public sealed class ObjectGroup
			: IO.IEndianStreamSerializable
		{
			public short Id;

			public int[]? Objects; // not sure if BProtoObjectID, etc
			public int[]? TriggeredTeams;

			#region IEndianStreamSerializable Members
			public void Serialize(IO.EndianStream s)
			{
				s.Stream(ref Id);
				int[] objects = s.IsReading
					? System.Array.Empty<int>()
					: Objects ?? throw new System.ArgumentNullException(nameof(Objects));
				int[] triggeredTeams = s.IsReading
					? System.Array.Empty<int>()
					: TriggeredTeams ?? throw new System.ArgumentNullException(nameof(TriggeredTeams));
				BSaveGame.StreamArray(s, ref objects);
				BSaveGame.StreamArray(s, ref triggeredTeams);
				Objects = objects;
				TriggeredTeams = triggeredTeams;
			}
			#endregion
		};

		public struct BExplorationGroupTimerEntry
			: IO.IEndianStreamSerializable
		{
			public uint Unknown0, Unknown4, Unknown8;

			#region IEndianStreamSerializable Members
			public void Serialize(IO.EndianStream s)
			{
				s.Stream(ref Unknown0); s.Stream(ref Unknown4); s.Stream(ref Unknown8);
			}
			#endregion
		};

		public struct PlayerColorCategory
			: IO.IEndianStreamSerializable
		{
			public uint Objects, Corpse, Selection,
				Minimap, UI;
			public byte Index;

			#region IEndianStreamSerializable Members
			public void Serialize(IO.EndianStream s)
			{
				s.Stream(ref Objects); s.Stream(ref Corpse); s.Stream(ref Selection);
				s.Stream(ref Minimap); s.Stream(ref UI);
				s.Stream(ref Index);
			}
			#endregion
		};

		ObjectGroup[]? NumExplorationGroups;
		BExplorationGroupTimerEntry[]? ActiveExplorationGroups;
		public BPlayer[]? Players;
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Performance", "CA1814:Prefer jagged arrays over multidimensional", Justification = "Fixed serialized category-by-player matrix preserves two-dimensional indexing and stream order.")]
		readonly PlayerColorCategory[,] PlayerColorCategories = new PlayerColorCategory[cMaxPlayerColorCategories, cMaximumSupportedPlayers];
		readonly List<CondensedListItem16<BSimOrder>> SimOrders = new();
		readonly List<CondensedListItem16<BUnitOpp>> UnitOpps = new();
		readonly List<CondensedListItem16<BPathMoveData>> PathMoveData = new();

		#region IEndianStreamSerializable Members
		public void Serialize(IO.EndianStream s)
		{
			var owner = s.Owner;
			System.ArgumentNullException.ThrowIfNull(owner);
			var sg = KSoft.Debug.TypeCheck.CastReference<BSaveGame>(owner);

			ObjectGroup[] numExplorationGroups = s.IsReading
				? System.Array.Empty<ObjectGroup>()
				: NumExplorationGroups ?? throw new System.ArgumentNullException(nameof(NumExplorationGroups));
			BExplorationGroupTimerEntry[] activeExplorationGroups = s.IsReading
				? System.Array.Empty<BExplorationGroupTimerEntry>()
				: ActiveExplorationGroups ?? throw new System.ArgumentNullException(nameof(ActiveExplorationGroups));
			BPlayer[] players;
			if (s.IsReading)
			{
				players = new BPlayer[sg.Players.Count];
				for(int x = 0; x < players.Length; x++)
				{
					players[x] = new BPlayer();
				}
			}
			else
			{
				players = Players ?? throw new System.ArgumentNullException(nameof(Players));
			}

			BSaveGame.StreamArray16(s, ref numExplorationGroups, isIterated:true);
			BSaveGame.StreamArray(s, ref activeExplorationGroups);
			NumExplorationGroups = numExplorationGroups;
			ActiveExplorationGroups = activeExplorationGroups;
			Players = players;
			s.StreamSignature(cSaveMarker.World1);
			foreach (var player in players)
			{
				s.Stream(player);
			}

			s.StreamSignature(cSaveMarker.Players);
			s.StreamSignature(cMaximumSupportedPlayers);
			s.StreamSignature(cMaxPlayerColorCategories);
			for (int x = 0; x < cMaxPlayerColorCategories; x++)
			{
				for (int y = 0; y < cMaximumSupportedPlayers; y++)
				{
					s.Stream(ref PlayerColorCategories[x, y]);
				}
			}

			s.StreamSignature(cSaveMarker.World2);
			BSaveGame.StreamFreeList(s, SimOrders, BSimOrder.kFreeListInfo);
			BSaveGame.StreamFreeList(s, UnitOpps, BUnitOpp.kFreeListInfo);
			BSaveGame.StreamFreeList(s, PathMoveData, BPathMoveData.kFreeListInfo);

			//...
		}
		#endregion
	};
}
