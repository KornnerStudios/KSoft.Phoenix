using System.Collections.Generic;

namespace KSoft.Phoenix.Runtime
{
	public interface ICondensedListItem
	{
		int Index { get; }

		void Serialize(IO.EndianStream s, CondensedListInfo parentListInfo);
	};

	[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1815:OverrideEqualsAndOperatorEqualsOnValueTypes", Justification = "Represents packed collection storage.")]
	public struct CondensedListItem8<T>
		: ICondensedListItem
		where T : class, IO.IEndianStreamSerializable, new()
	{
		public sbyte mIndex;
		public readonly int Index => mIndex;
		public T Value;

		#region IEndianStreamSerializable Members
		public void Serialize(IO.EndianStream s, CondensedListInfo parentListInfo)
		{
			s.Stream(ref mIndex);
			if (Index != parentListInfo.DoneIndex)
			{
				if (s.IsReading)
				{
					Value = new T();
				}

				s.Stream(Value);
			}
		}
		#endregion
	};
	[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1815:OverrideEqualsAndOperatorEqualsOnValueTypes", Justification = "Represents packed collection storage.")]
	public struct CondensedListItemValue8<T>
		: ICondensedListItem
		where T : struct, IO.IEndianStreamSerializable
	{
		public sbyte mIndex;
		public readonly int Index => mIndex;
		public T Value;

		#region IEndianStreamSerializable Members
		public void Serialize(IO.EndianStream s, CondensedListInfo parentListInfo)
		{
			s.Stream(ref mIndex);
			if (Index != parentListInfo.DoneIndex)
			{
				s.Stream(ref Value);
			}
		}
		#endregion
	};

	[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1815:OverrideEqualsAndOperatorEqualsOnValueTypes", Justification = "Represents packed collection storage.")]
	public struct CondensedListItem16<T>
		: ICondensedListItem
		where T : class, IO.IEndianStreamSerializable, new()
	{
		public short mIndex;
		public readonly int Index => mIndex;
		public T Value;

		#region IEndianStreamSerializable Members
		public void Serialize(IO.EndianStream s, CondensedListInfo parentListInfo)
		{
			s.Stream(ref mIndex);
			if (Index != parentListInfo.DoneIndex)
			{
				if (s.IsReading)
				{
					Value = new T();
				}

				s.Stream(Value);
			}
		}
		#endregion
	};
	[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1815:OverrideEqualsAndOperatorEqualsOnValueTypes", Justification = "Represents packed collection storage.")]
	public struct CondensedListItemValue16<T>
		: ICondensedListItem
		where T : struct, IO.IEndianStreamSerializable
	{
		public short mIndex;
		public readonly int Index => mIndex;
		public T Value;

		#region IEndianStreamSerializable Members
		public void Serialize(IO.EndianStream s, CondensedListInfo parentListInfo)
		{
			s.Stream(ref mIndex);
			if (Index != parentListInfo.DoneIndex)
			{
				s.Stream(ref Value);
			}
		}
		#endregion
	};

	[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1815:OverrideEqualsAndOperatorEqualsOnValueTypes", Justification = "Represents packed collection storage.")]
	public struct CondensedListItem32<T>
		: ICondensedListItem
		where T : class, IO.IEndianStreamSerializable, new()
	{
		public int mIndex;
		public readonly int Index => mIndex;
		public T Value;

		#region IEndianStreamSerializable Members
		public void Serialize(IO.EndianStream s, CondensedListInfo parentListInfo)
		{
			s.Stream(ref mIndex);
			if (Index != parentListInfo.DoneIndex)
			{
				if (s.IsReading)
				{
					Value = new T();
				}

				s.Stream(Value);
			}
		}
		#endregion
	};
	[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1815:OverrideEqualsAndOperatorEqualsOnValueTypes", Justification = "Represents packed collection storage.")]
	public struct CondensedListItemValue32<T> : ICondensedListItem
			where T : struct, IO.IEndianStreamSerializable
	{
		public int mIndex;
		public readonly int Index => mIndex;
		public T Value;

		#region IEndianStreamSerializable Members
		public void Serialize(IO.EndianStream s, CondensedListInfo parentListInfo)
		{
			s.Stream(ref mIndex);
			if (Index != parentListInfo.DoneIndex)
			{
				s.Stream(ref Value);
			}
		}
		#endregion
	};

	partial class BSaveGame
	{
		public static IO.EndianStream StreamList<T>(IO.EndianStream s, List<T> list,
			CondensedListInfo info)
			// constraining it to struct instead of 'new()' generates less complex 'new T' code and avoids boxing ops
			where T : struct, ICondensedListItem
		{
			int capacity = list.Capacity;
			info.StreamCapacity(s, ref capacity);

			if (s.IsReading)
			{
				if (info.SerializeCapacity)
				{
					if (capacity > info.MaxCount)
					{
						throw new System.IO.InvalidDataException(string.Format(
							"Read list capacity {0} exceeds maximum {1}.",
							capacity,
							info.MaxCount));
					}
					list.Capacity = capacity;
				}

				var item = new T();
				for (item.Serialize(s, info); item.Index != info.DoneIndex; item.Serialize(s, info))
				{
					if (list.Count > info.MaxCount)
					{
						throw new System.IO.InvalidDataException(string.Format(
							"Read list count {0} exceeds maximum {1}.",
							list.Count,
							info.MaxCount));
					}
					list.Add(item);
				}
			}
			else if (s.IsWriting)
			{
				foreach (var obj in list)
				{
					obj.Serialize(s, info);
				}

				info.StreamDoneIndex(s);
			}

			return s;
		}

		public static IO.EndianStream StreamFreeList<T>(IO.EndianStream s, List<T> list,
			FreeListInfo info)
			where T : struct, ICondensedListItem
		{
			int capacity = list.Capacity;
			info.StreamCapacity(s, ref capacity); // highWaterMark
			int count = list.Count;
			info.StreamCapacity(s, ref count); // numAllocated

			if (s.IsReading)
			{
				if (capacity > info.MaxCount || count >= info.MaxCount)
				{
					throw new System.IO.InvalidDataException(string.Format(
						"Read free-list capacity/count {0}/{1} is outside maximum {2}.",
						capacity,
						count,
						info.MaxCount));
				}
				list.Capacity = capacity;

				for (int x = 0; x < count; x++)
				{
					var item = new T();
					item.Serialize(s, info);

					list.Add(item);
				}
			}
			else if (s.IsWriting)
			{
				foreach (var obj in list)
				{
					obj.Serialize(s, info);
				}
			}

			info.StreamSaveMarker(s);

			return s;
		}
	};
}