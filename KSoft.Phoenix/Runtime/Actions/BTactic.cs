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
		public BWeapon[] Weapons = null!;
		public BProtoAction[] ProtoActions = null!;
		public bool AnimInfoLoaded;

		#region IEndianStreamSerializable Members
		public void Serialize(IO.EndianStream s)
		{
			BSaveGame.StreamArray(s, ref Weapons);
			if (Weapons.Length > BWeapon.kMaxCount)
			{
				throw new System.IO.InvalidDataException(string.Create(KSoft.Util.InvariantCultureInfo,
					$"Weapon count is {Weapons.Length}, maximum is {BWeapon.kMaxCount}."));
			}
			BSaveGame.StreamArray(s, ref ProtoActions);
			if (ProtoActions.Length > BProtoAction.kMaxCount)
			{
				throw new System.IO.InvalidDataException(string.Create(KSoft.Util.InvariantCultureInfo,
					$"Proto-action count is {ProtoActions.Length}, maximum is {BProtoAction.kMaxCount}."));
			}
			s.Stream(ref AnimInfoLoaded);
			s.StreamSignature(cSaveMarker.Tactic);
		}
		#endregion
	};
}