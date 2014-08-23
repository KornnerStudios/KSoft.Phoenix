
namespace KSoft.Phoenix.Runtime
{
	sealed class BTimerManager
		: IO.IEndianStreamSerializable
	{
		const int kTimerCount = 0x60 / BTimer.kSizeOf;

		public struct BTimer
			: IO.IEndianStreamSerializable
		{
			internal const int kSizeOf = 0x18; // in-memory rep size

			public uint StartTime, StopTime, CurrentTime, LastTime;
			public int ID;
			public bool CountUp, Active, Done, Paused;

			#region IEndianStreamSerializable Members
			public void Serialize(IO.EndianStream s)
			{
				s.Stream(ref StartTime); s.Stream(ref StopTime); s.Stream(ref CurrentTime); s.Stream(ref LastTime);
				s.Stream(ref ID);
				s.Stream(ref CountUp); s.Stream(ref Active); s.Stream(ref Done); s.Stream(ref Paused);
			}
			#endregion
		};
		public BTimer[] Timers = new BTimer[kTimerCount];
		public int NextTimerID;

		#region IEndianStreamSerializable Members
		public void Serialize(IO.EndianStream s)
		{
			for (int x = 0; x < Timers.Length; x++)
				s.Stream(ref Timers[x]);
			s.Stream(ref NextTimerID);
		}
		#endregion
	};
}