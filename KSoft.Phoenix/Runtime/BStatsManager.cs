using System.Collections.Generic;

namespace KSoft.Phoenix.Runtime
{
	partial class cSaveMarker
	{
		public const ushort
			StatsPlayer = 0x2710,
			StatsRecorders = 0x2711,
			StatsPowers = 0x2712,
			StatsAbilities = 0x2713
			;
	};

	static class BStatsSerialization
	{
		public static void StreamArray16<T>(IO.EndianStream s, ref T[]? values, bool isIterated = false)
			where T : IO.IEndianStreamSerializable, new()
		{
			if (s.IsReading)
			{
				T[] array = System.Array.Empty<T>();
				BSaveGame.StreamArray16(s, ref array, isIterated);
				values = array;
			}
			else
			{
				T[] array = values ?? throw new System.ArgumentNullException(nameof(values));
				BSaveGame.StreamArray16(s, ref array, isIterated);
				values = array;
			}
		}

		public static void StreamArray16(IO.EndianStream s, ref int[]? values)
		{
			if (s.IsReading)
			{
				int[] array = System.Array.Empty<int>();
				BSaveGame.StreamArray16(s, ref array);
				values = array;
			}
			else
			{
				int[] array = values ?? throw new System.ArgumentNullException(nameof(values));
				BSaveGame.StreamArray16(s, ref array);
				values = array;
			}
		}

		public static void StreamBCost(BSaveGame saveGame, IO.EndianStream s, ref BCostDatum[]? values)
		{
			if (s.IsReading)
			{
				BCostDatum[] array = System.Array.Empty<BCostDatum>();
				saveGame.StreamBCost(s, ref array);
				values = array;
			}
			else
			{
				BCostDatum[] array = values ?? throw new System.ArgumentNullException(nameof(values));
				saveGame.StreamBCost(s, ref array);
				values = array;
			}
		}

		public static void StreamNotNull<T>(IO.EndianStream s, ref T? value)
			where T : class, IO.IEndianStreamSerializable, new()
		{
			bool hasValue = value is not null;
			s.Stream(ref hasValue);
			if (s.IsReading)
			{
				if (hasValue)
				{
					var item = new T();
					s.Stream(item);
					value = item;
				}
			}
			else if (hasValue)
			{
				T item = value ?? throw new System.ArgumentNullException(nameof(value));
				s.Stream(item);
			}
		}

		public static void Stream<T>(IO.EndianStream s, ref T? value, System.Func<T> initializer)
			where T : class, IO.IEndianStreamSerializable
		{
			System.ArgumentNullException.ThrowIfNull(initializer);
			if (s.IsReading)
			{
				T item = initializer();
				s.Stream(item);
				value = item;
			}
			else
			{
				T item = value ?? throw new System.ArgumentNullException(nameof(value));
				s.Stream(item);
			}
		}
	};

	struct BStatLostDestroyed
		: IO.IEndianStreamSerializable
	{
		public uint Lost, Destroyed;

		#region IEndianStreamSerializable Members
		public void Serialize(IO.EndianStream s)
		{
			s.Stream(ref Lost);
			s.Stream(ref Destroyed);
		}
		#endregion
	};
	struct BStatLostDestroyedKeyValuePair
		: IO.IEndianStreamSerializable
	{
		public int Key;
		public BStatLostDestroyed Value;

		#region IEndianStreamSerializable Members
		public void Serialize(IO.EndianStream s)
		{
			s.Stream(ref Key);
			s.Stream(ref Value);
		}
		#endregion
	};
	sealed class BStatLostDestroyedMap
		: IO.IEndianStreamSerializable
	{
		public int Index;
		public BStatLostDestroyedKeyValuePair[]? Killers;

		#region IEndianStreamSerializable Members
		public void Serialize(IO.EndianStream s)
		{
			s.Stream(ref Index);
			BStatsSerialization.StreamArray16(s, ref Killers);
		}
		#endregion
	};

	sealed class BStatCombat
		: IO.IEndianStreamSerializable
	{
		public const ushort DoneIndex = 0x2711;

		public int[]? Levels;
		public float XP;

		#region IEndianStreamSerializable Members
		public void Serialize(IO.EndianStream s)
		{
			BStatsSerialization.StreamArray16(s, ref Levels);
			s.Stream(ref XP);
		}
		#endregion
	};
	sealed class BStatCombatWithIndex
		: IO.IEndianStreamSerializable
	{
		public int Index;
		public BStatCombat Combat = new();

		#region IEndianStreamSerializable Members
		public void Serialize(IO.EndianStream s)
		{
			s.Stream(ref Index);
			s.Stream(Combat);
		}
		#endregion
	};

	class BStatTotal
		: IO.IEndianStreamSerializable
	{
		public byte[] KillerIDs = new byte[0x20];
		public uint Built, Lost, Destroyed, Max;
		public int CombatID;
		public uint FirstTime, LastTime;

		#region IEndianStreamSerializable Members
		public virtual void Serialize(IO.EndianStream s)
		{
			s.Stream(KillerIDs);
			s.Stream(ref Built); s.Stream(ref Lost); s.Stream(ref Destroyed); s.Stream(ref Max);
			s.Stream(ref CombatID);
			s.Stream(ref FirstTime); s.Stream(ref LastTime);
		}
		#endregion
	};
	sealed class BStatEvent
		: BStatTotal
	{
		public uint Timestamp;
		public int ProtoID;

		#region IEndianStreamSerializable Members
		public override void Serialize(IO.EndianStream s)
		{
			base.Serialize(s);

			s.Stream(ref Timestamp);
			s.Stream(ref ProtoID);
		}
		#endregion
	};

	sealed class BStatTotalKeyValuePair
		: IO.IEndianStreamSerializable
	{
		public int Key;
		public BStatTotal Value = new();

		#region IEndianStreamSerializable Members
		public void Serialize(IO.EndianStream s)
		{
			s.Stream(ref Key);
			s.Stream(Value);
		}
		#endregion
	};

	#region BStats Recorders
	abstract class BStatRecorderBase
		: IO.IEndianStreamSerializable
	{
		public BStatLostDestroyedMap[]? Killers;
		public BStatCombatWithIndex[]? Combat;

		#region IEndianStreamSerializable Members
		public virtual void Serialize(IO.EndianStream s)
		{
			BStatsSerialization.StreamArray16(s, ref Killers, isIterated:true);
			BStatsSerialization.StreamArray16(s, ref Combat);
			s.StreamSignature(BStatCombat.DoneIndex);
		}
		#endregion
	};
	sealed class BStatTotalsRecorder
		: BStatRecorderBase
	{
		public const int kStatType = 1;

		public List<BStatTotalKeyValuePair> Totals { get; private set; } = new();
		public BStatTotal Total = new();
		public BStatCombat? Combat_;

		#region IEndianStreamSerializable Members
		public override void Serialize(IO.EndianStream s)
		{
			base.Serialize(s);

			BSaveGame.StreamCollection(s, Totals);
			s.Stream(Total);
			BStatsSerialization.StreamNotNull(s, ref Combat_);
		}
		#endregion
	};
	sealed class BStatEventRecorder
		: BStatRecorderBase
	{
		public const int kStatType = 2;

		public BStatTotal Total = new();
		public BStatEvent[]? Events;

		#region IEndianStreamSerializable Members
		public override void Serialize(IO.EndianStream s)
		{
			base.Serialize(s);

			s.Stream(Total);
			BStatsSerialization.StreamArray16(s, ref Events);
		}
		#endregion
	};
	sealed class BStatGraphRecorder
		: BStatRecorderBase
	{
		public const int kStatType = 3;
	};
	sealed class BStatResourceGraphRecorder
		: BStatRecorderBase
	{
		public const int kStatType = 4;
	};
	sealed class BStatPopGraphRecorder
		: BStatRecorderBase
	{
		public const int kStatType = 5;
	};
	sealed class BStatBaseGraphRecorder
		: BStatRecorderBase
	{
		public const int kStatType = 6;
	};
	sealed class BStatScoreGraphRecorder
		: BStatRecorderBase
	{
		public const int kStatType = 7;
	};

	sealed class BStatsRecorder
		: IO.IEndianStreamSerializable
	{
		public short Index;

		public byte StatType;
		public BStatRecorderBase? Stat;

		#region IEndianStreamSerializable Members
		static BStatRecorderBase FromType(int statType)
		{
			return statType switch
			{
				BStatTotalsRecorder.kStatType => new BStatTotalsRecorder(),
				BStatEventRecorder.kStatType => new BStatEventRecorder(),
#if false
				BStatGraphRecorder.kStatType => new BStatGraphRecorder(),
				BStatResourceGraphRecorder.kStatType => new BStatResourceGraphRecorder(),
				BStatPopGraphRecorder.kStatType => new BStatPopGraphRecorder(),
				BStatBaseGraphRecorder.kStatType => new BStatBaseGraphRecorder(),
				BStatScoreGraphRecorder.kStatType => new BStatScoreGraphRecorder(),
#endif

				_ => throw new KSoft.Debug.UnreachableException(statType.ToString(KSoft.Util.InvariantCultureInfo)),
			};
		}
		public void Serialize(IO.EndianStream s)
		{
			s.Stream(ref Index);
			s.Stream(ref StatType);
			BStatsSerialization.Stream(s, ref Stat,
				() => FromType(StatType));
		}
		#endregion
	};
	#endregion

	struct BStatPowerKeyValuePair
		: IO.IEndianStreamSerializable
	{
		public BProtoPowerID Key;
		public uint Value;

		#region IEndianStreamSerializable Members
		public void Serialize(IO.EndianStream s)
		{
			s.Stream(ref Key);
			s.Stream(ref Value);
		}
		#endregion
	};
	struct BStatAbilityKeyValuePair
		: IO.IEndianStreamSerializable
	{
		public int Key;
		public uint Value;

		#region IEndianStreamSerializable Members
		public void Serialize(IO.EndianStream s)
		{
			s.Stream(ref Key);
			s.Stream(ref Value);
		}
		#endregion
	};
	sealed class BStatsManager
		: IO.IEndianStreamSerializable
	{
		public BStatsRecorder[]? Recorders;
		public List<BStatPowerKeyValuePair> Powers { get; private set; } = new();
		public List<BStatAbilityKeyValuePair> Abilities { get; private set; } = new();
		public BCostDatum[]? TotalResources, MaxResources,
			GatheredResources, TributedResources;
		public BPlayerID PlayerID;
		public BTeamID TeamID;
		public uint PlayerStateTime;
		public BPlayerState PlayerState;
		public uint StrengthTime, StrengthTimer;
		public int CivID, LeaderID;
		public byte ResourcesUsed, PlayerType;
		public bool RandomCiv, RandomLeader, Resigned, Defeated, Disconnected, Won;

		#region IEndianStreamSerializable Members
		public void Serialize(IO.EndianStream s)
		{
			var owner = s.Owner;
			System.ArgumentNullException.ThrowIfNull(owner);
			var sg = KSoft.Debug.TypeCheck.CastReference<BSaveGame>(owner);

			BStatsSerialization.StreamArray16(s, ref Recorders, isIterated:true);
			s.StreamSignature(cSaveMarker.StatsRecorders);
			BSaveGame.StreamCollection(s, Powers);
			s.StreamSignature(cSaveMarker.StatsPowers);
			BSaveGame.StreamCollection(s, Abilities);
			s.StreamSignature(cSaveMarker.StatsAbilities);
			BStatsSerialization.StreamBCost(sg, s, ref TotalResources); BStatsSerialization.StreamBCost(sg, s, ref MaxResources);
			BStatsSerialization.StreamBCost(sg, s, ref GatheredResources); BStatsSerialization.StreamBCost(sg, s, ref TributedResources);
			s.Stream(ref PlayerID);
			s.Stream(ref TeamID);
			s.Stream(ref PlayerStateTime);
			s.Stream(ref PlayerState);
			s.Stream(ref StrengthTime); s.Stream(ref StrengthTimer);
			s.Stream(ref CivID); s.Stream(ref LeaderID);
			s.Stream(ref ResourcesUsed); s.Stream(ref PlayerType);
			s.Stream(ref RandomCiv); s.Stream(ref RandomLeader); s.Stream(ref Resigned);
			s.Stream(ref Defeated); s.Stream(ref Disconnected); s.Stream(ref Won);
			s.StreamSignature(cSaveMarker.StatsPlayer);
		}
		#endregion
	};
}
