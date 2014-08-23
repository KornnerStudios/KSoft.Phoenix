
namespace KSoft.Phoenix.Runtime
{
	sealed class BSavePlayer
		: IO.IEndianStreamSerializable
	{
		public string Name;
		public string DisplayName;
		public int MPID, ScenarioID, CivID, TeamID, LeaderID;
		public ushort Difficulty; // BHalfFloat
		public sbyte PlayerType;

		#region IEndianStreamSerializable Members
		public void Serialize(IO.EndianStream s)
		{
			s.StreamPascalString32(ref Name);
			s.StreamPascalWideString32(ref DisplayName);
			s.Stream(ref MPID);
			s.Stream(ref ScenarioID);
			s.Stream(ref CivID);
			s.Stream(ref TeamID);
			s.Stream(ref LeaderID);
			s.Stream(ref Difficulty);
			s.Stream(ref PlayerType);
			s.StreamSignature(cSaveMarker.SetupPlayer);
		}
		#endregion
	};
}