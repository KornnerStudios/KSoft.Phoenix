﻿
namespace KSoft.Phoenix.Runtime
{
	partial class BDatabase
	{
		public sealed class Tactic
			: IO.IEndianStreamSerializable
		{
			public string[] ProtoActions, Weapons;

			#region IEndianStreamSerializable Members
			public void Serialize(IO.EndianStream s)
			{
				BSaveGame.StreamArray(s, ref ProtoActions);
				BSaveGame.StreamArray(s, ref Weapons);
			}
			#endregion
		};
		static readonly CondensedListInfo kTacticsListInfo = new CondensedListInfo()
		{
			IndexSize=sizeof(short),
		};
	};
}