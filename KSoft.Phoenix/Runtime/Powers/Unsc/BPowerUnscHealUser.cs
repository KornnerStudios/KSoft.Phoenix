
namespace KSoft.Phoenix.Runtime
{
	sealed class BPowerUnscHealUser
		: BPowerUser
	{
		public BPowerHelperHudSounds HudSounds = new();

		#region IEndianStreamSerializable Members
		public override void Serialize(IO.EndianStream s)
		{
			base.Serialize(s);

			s.Stream(HudSounds);
		}
		#endregion
	};
}