using Interop = System.Runtime.InteropServices;

namespace KSoft.Phoenix.Runtime
{
	using GameSettingTypeStreamer = IO.EnumBinaryStreamer<GameSettingType>;

	[Interop.StructLayout(Interop.LayoutKind.Explicit)]
	struct BGameSettingVariant
		: IO.IEndianStreamSerializable
	{
		[Interop.FieldOffset(0)] public bool Bool;
		[Interop.FieldOffset(0)] public byte Byte;
		[Interop.FieldOffset(0)] public int Int;
		[Interop.FieldOffset(0)] public float Float;
		[Interop.FieldOffset(0)] public long Long;

		[Interop.FieldOffset(8)] public GameSettingType Type;

		// #64BIT: +4 to offset 16
		[Interop.FieldOffset(12+4)] public string String;

		#region IEndianStreamSerializable Members
		public void Serialize(IO.EndianStream s)
		{
			s.Stream(ref Type, GameSettingTypeStreamer.Instance);
			switch (Type)
			{
				case GameSettingType.Float:	s.Stream(ref Float); break;
				case GameSettingType.Int:	s.Stream(ref Int); break;
				case GameSettingType.Byte:	s.Stream(ref Byte); break;
				case GameSettingType.Bool:	s.Stream(ref Bool); break;
				case GameSettingType.Long:	s.Stream(ref Long); break;
				case GameSettingType.String:s.Stream(ref String); break;
			}
		}
		#endregion
	};
}
