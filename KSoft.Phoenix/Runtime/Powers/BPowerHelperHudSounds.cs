
using BCueIndex = System.Int32;

namespace KSoft.Phoenix.Runtime
{
	sealed class BPowerHelperHudSounds
		: IO.IEndianStreamSerializable
	{
		public BCueIndex HudUpSound, HudAbortSound, HudFireSound,
			HudLastFireSound, HudStartEnvSound, HudStopEnvSound;

		#region IEndianStreamSerializable Members
		public void Serialize(IO.EndianStream s)
		{
			s.Stream(ref HudUpSound); s.Stream(ref HudAbortSound); s.Stream(ref HudFireSound);
			s.Stream(ref HudLastFireSound); s.Stream(ref HudStartEnvSound); s.Stream(ref HudStopEnvSound);
		}
		#endregion
	};
}