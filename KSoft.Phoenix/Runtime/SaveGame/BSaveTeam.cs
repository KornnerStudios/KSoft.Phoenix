
namespace KSoft.Phoenix.Runtime
{
	sealed class BSaveTeam
		: IO.IEndianStreamSerializable
	{
		public int[] Players = null!;
		public byte[] Relations = null!; // BRelationType

		#region IEndianStreamSerializable Members
		public void Serialize(IO.EndianStream s)
		{
			BSaveGame.StreamArray(s, ref Players);
			BSaveGame.StreamArray(s, ref Relations);
		}
		#endregion
	};
}