
namespace KSoft.Phoenix.Runtime
{
	public sealed class FreeListInfo
		: CondensedListInfo
	{
		public ushort SaveMarker
		{
			get { return (ushort)DoneIndex; }
			set { DoneIndex = value; }
		}

		public FreeListInfo(ushort saveMarker)
		{
			SerializeCapacity = true;
			IndexSize = sizeof(short);
			MaxCount = ushort.MaxValue;
			SaveMarker = saveMarker;
		}

		public void StreamCount(IO.EndianStream s, ref int count)
		{
			StreamCapacity(s, ref count);
		}
		public void StreamSaveMarker(IO.EndianStream s)
		{
			s.StreamSignature(SaveMarker);
		}
	};
}