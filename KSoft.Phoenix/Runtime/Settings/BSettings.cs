
namespace KSoft.Phoenix.Runtime
{
	sealed class BSettings : IO.IEndianStreamSerializable
	{
		const uint kVersion = 0;

		public BGameSettings GameSettings { get; private set; }
		public BConfigSettings ConfigSettings { get; private set; }

		public BSettings()
		{
			GameSettings = new BGameSettings();
			ConfigSettings = new BConfigSettings();
		}

		#region IEndianStreamSerializable Members
		public void Serialize(IO.EndianStream s)
		{
			s.StreamVersion(kVersion);
			s.Stream(GameSettings);
			s.Stream(ConfigSettings);
		}
		#endregion
	};
}