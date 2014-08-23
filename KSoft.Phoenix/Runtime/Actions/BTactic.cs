using Contract = System.Diagnostics.Contracts.Contract;

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
			Contract.Assert(Weapons.Length <= BWeapon.kMaxCount);
			BSaveGame.StreamArray(s, ref ProtoActions);
			Contract.Assert(ProtoActions.Length <= BProtoAction.kMaxCount);
			s.Stream(ref AnimInfoLoaded);
			s.StreamSignature(cSaveMarker.Tactic);
		}
		#endregion
	};
}