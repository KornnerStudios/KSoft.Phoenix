
using BProtoObjectID = System.Int32;
using BAIMissionID = System.Int32;

namespace KSoft.Phoenix.Runtime
{
	sealed class BPowerUnscOdst
		: BPower
	{
		public struct BODSTDrop
			: IO.IEndianStreamSerializable
		{
			// A BVector field?
			public ulong Unknown0, Unknown8;

			#region IEndianStreamSerializable Members
			public void Serialize(IO.EndianStream s)
			{
				s.Stream(ref Unknown0); s.Stream(ref Unknown8);
			}
			#endregion
		};

		public float SquadSpawnDelay;
		public BODSTDrop[] ActiveDrops;
		public BProtoObjectID ProjectileProtoID, ODSTProtoSquadID;
		public BAIMissionID AddToMissionID;
		public bool ReadyForShutdown;

		#region IEndianStreamSerializable Members
		public override void Serialize(IO.EndianStream s)
		{
			base.Serialize(s);

			s.Stream(ref SquadSpawnDelay);
			BSaveGame.StreamArray(s, ref ActiveDrops);
			s.Stream(ref ProjectileProtoID); s.Stream(ref ODSTProtoSquadID);
			s.Stream(ref AddToMissionID);
			s.Stream(ref ReadyForShutdown);
		}
		#endregion
	};
}