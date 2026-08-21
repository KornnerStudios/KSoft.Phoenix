namespace KSoft.Phoenix.Runtime
{
	partial class cSaveMarker
	{
		public const ushort
			Tactic = 0x2710,
			ProtoAction = 0x2711,
			Weapon = 0x2712
			;
	};

	sealed class BTactic
		: IO.IEndianStreamSerializable
	{
		public BWeapon[] Weapons;
		public BProtoAction[] ProtoActions;
		public bool AnimInfoLoaded;

		#region IEndianStreamSerializable Members
		public void Serialize(IO.EndianStream s)
		{
			BSaveGame.StreamArray(s, ref Weapons);
			if (Weapons.Length > BWeapon.kMaxCount)
			{
				throw new System.IO.InvalidDataException(string.Format(
					"Weapon count is {0}, maximum is {1}.",
					Weapons.Length,
					BWeapon.kMaxCount));
			}
			BSaveGame.StreamArray(s, ref ProtoActions);
			if (ProtoActions.Length > BProtoAction.kMaxCount)
			{
				throw new System.IO.InvalidDataException(string.Format(
					"Proto-action count is {0}, maximum is {1}.",
					ProtoActions.Length,
					BProtoAction.kMaxCount));
			}
			s.Stream(ref AnimInfoLoaded);
			s.StreamSignature(cSaveMarker.Tactic);
		}
		#endregion
	};
}