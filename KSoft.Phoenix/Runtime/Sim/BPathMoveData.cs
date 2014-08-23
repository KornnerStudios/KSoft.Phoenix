
using BPathLevel = System.Byte;

namespace KSoft.Phoenix.Runtime
{
	public sealed class BPathMoveData
		: IO.IEndianStreamSerializable
	{
		internal static readonly FreeListInfo kFreeListInfo = new FreeListInfo(cSaveMarker.PathMoveData)
		{
			MaxCount = 0x4E20,
		};

		public BPath Path { get; private set; }
		public int CurrentWaypoint;
		public uint PathTime;
		public int LinkedPath = TypeExtensions.kNone;
		public BPathLevel PathLevel;

		public BPathMoveData()
		{
			Path = new BPath();
		}

		#region IEndianStreamSerializable Members
		public void Serialize(IO.EndianStream s)
		{
			s.Stream(Path);
			s.Stream(ref CurrentWaypoint);
			s.Stream(ref PathTime);
			BSaveGame.StreamFreeListItemPtr(s, ref LinkedPath);
			s.Stream(ref PathLevel);
		}
		#endregion
	};
}