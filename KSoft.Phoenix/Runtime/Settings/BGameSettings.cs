using System.Collections.Generic;

namespace KSoft.Phoenix.Runtime
{
	sealed class BGameSettings
		: IO.IEndianStreamSerializable
	{
		const uint kVersion = 1;
		const int kDefaultSettingsCapacity = 113;

		public List<BGameSetting> Settings { get; private set; }

		public BGameSettings()
		{
			Settings = new List<BGameSetting>(kDefaultSettingsCapacity);
		}

		#region IEndianStreamSerializable Members
		public void Serialize(IO.EndianStream s)
		{
			// #TODO the two are not part of this class' serialization, they are from BSaveGame
			s.Pad8(); // bool multiplayer game
			s.StreamVersion(kVersion); // unused local player id (ALWAYS 1)
			BSaveGame.StreamCollection(s, Settings);
		}
		#endregion
	};
}
