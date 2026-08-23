
namespace KSoft.Phoenix.Runtime
{
	partial class BDatabase
	{
		public sealed class Tactic
			: IO.IEndianStreamSerializable
		{
			public string[] ProtoActions = null!, Weapons = null!;

			#region IEndianStreamSerializable Members
			public void Serialize(IO.EndianStream s)
			{
				BSaveGame.StreamArray(s, ref ProtoActions);
				BSaveGame.StreamArray(s, ref Weapons);
			}
			#endregion
		};
		static readonly CondensedListInfo kTacticsListInfo = new()
		{
			IndexSize=sizeof(short),
		};
	};
}