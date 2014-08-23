
namespace KSoft.Phoenix.Runtime
{
	sealed class BSaveTeam
		: IO.IEndianStreamSerializable
	{
		public int[] Players;
		public byte[] Relations; // BRelationType

		#region IEndianStreamSerializable Members
		public void Serialize(IO.EndianStream s)
		{
			BSaveGame.StreamArray(s, ref Players);
			BSaveGame.StreamArray(s, ref Relations);
		}
		#endregion
	};
}