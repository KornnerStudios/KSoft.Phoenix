
namespace KSoft.Phoenix.Runtime
{
	abstract class BProtoBuildableObject
		: IO.IEndianStreamSerializable
	{
		public BCostDatum[]? Cost;
		public float BuildPoints;

		public bool Forbid;

		public abstract void Serialize(IO.EndianStream s);
	};

	internal static class BSaveGameNullableSerialization
	{
		internal static void StreamArray<T>(IO.EndianStream s, ref T[]? values, int maxCount = byte.MaxValue)
			where T : IO.IEndianStreamSerializable, new()
		{
			if (s.IsReading)
			{
				T[] array = System.Array.Empty<T>();
				BSaveGame.StreamArray(s, ref array, maxCount);
				values = array;
			}
			else
			{
				T[] array = values ?? throw new System.ArgumentNullException(nameof(values));
				BSaveGame.StreamArray(s, ref array, maxCount);
				values = array;
			}
		}

		internal static void StreamArray(IO.EndianStream s, ref int[]? values)
		{
			if (s.IsReading)
			{
				int[] array = System.Array.Empty<int>();
				BSaveGame.StreamArray(s, ref array);
				values = array;
			}
			else
			{
				int[] array = values ?? throw new System.ArgumentNullException(nameof(values));
				BSaveGame.StreamArray(s, ref array);
				values = array;
			}
		}

		internal static void StreamArray(IO.EndianStream s, ref ulong[]? values)
		{
			if (s.IsReading)
			{
				ulong[] array = System.Array.Empty<ulong>();
				BSaveGame.StreamArray(s, ref array);
				values = array;
			}
			else
			{
				ulong[] array = values ?? throw new System.ArgumentNullException(nameof(values));
				BSaveGame.StreamArray(s, ref array);
				values = array;
			}
		}

		internal static void StreamArray16<T>(IO.EndianStream s, ref T[]? values, bool isIterated = false, int maxCount = ushort.MaxValue)
			where T : IO.IEndianStreamSerializable, new()
		{
			if (s.IsReading)
			{
				T[] array = System.Array.Empty<T>();
				BSaveGame.StreamArray16(s, ref array, isIterated, maxCount);
				values = array;
			}
			else
			{
				T[] array = values ?? throw new System.ArgumentNullException(nameof(values));
				BSaveGame.StreamArray16(s, ref array, isIterated, maxCount);
				values = array;
			}
		}

		internal static void StreamVectorArray(IO.EndianStream s, ref BVector[]? values, int maxCount = byte.MaxValue)
		{
			if (s.IsReading)
			{
				BVector[] array = System.Array.Empty<BVector>();
				BSaveGame.StreamVectorArray(s, ref array, maxCount);
				values = array;
			}
			else
			{
				BVector[] array = values ?? throw new System.ArgumentNullException(nameof(values));
				BSaveGame.StreamVectorArray(s, ref array, maxCount);
				values = array;
			}
		}

		internal static void StreamVectorArray16(IO.EndianStream s, ref BVector[]? values, int maxCount = ushort.MaxValue)
		{
			if (s.IsReading)
			{
				BVector[] array = System.Array.Empty<BVector>();
				BSaveGame.StreamVectorArray16(s, ref array, maxCount);
				values = array;
			}
			else
			{
				BVector[] array = values ?? throw new System.ArgumentNullException(nameof(values));
				BSaveGame.StreamVectorArray16(s, ref array, maxCount);
				values = array;
			}
		}

		internal static void StreamBCost(BSaveGame saveGame, IO.EndianStream s, ref BCostDatum[]? values)
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

		internal static void StreamPascalWideString32(IO.EndianStream s, ref string? value)
		{
			string streamValue = s.IsReading ? string.Empty : value ?? throw new System.ArgumentNullException(nameof(value));
			s.StreamPascalWideString32(ref streamValue);
			value = streamValue;
		}

		internal static void StreamString(IO.EndianStream s, ref string? value)
		{
			string streamValue = s.IsReading ? string.Empty : value ?? throw new System.ArgumentNullException(nameof(value));
			s.Stream(ref streamValue);
			value = streamValue;
		}
	};
}