
using BEntityID = System.Int32;

namespace KSoft.Phoenix.Runtime
{
	sealed class BPowerUnscMacUser
		: BPowerUser
	{
		public BPowerHelperHudSounds HudSounds = new BPowerHelperHudSounds();
		public string HelpString;
		public BEntityID FakeTargettingLaserID, RealTargettingLaserID, TargettedSquadID;
		public uint ShotsRemaining;
		public float LastCommandSent, CommandInterval, LastShotSent, ShotInterval;
		public int LOSMode;
		public bool FlagLastCameraAutoZoomInstantEnabled, FlagLastCameraAutoZoomEnabled,
			FlagLastCameraZoomEnabled, FlagLastCameraYawEnabled;

		#region IEndianStreamSerializable Members
		public override void Serialize(IO.EndianStream s)
		{
			base.Serialize(s);

			s.Stream(HudSounds);
			s.StreamPascalWideString32(ref HelpString);
			s.Stream(ref FakeTargettingLaserID); s.Stream(ref RealTargettingLaserID); s.Stream(ref TargettedSquadID);
			s.Stream(ref ShotsRemaining);
			s.Stream(ref LastCommandSent); s.Stream(ref CommandInterval); s.Stream(ref LastShotSent); s.Stream(ref ShotInterval);
			s.Stream(ref LOSMode);
			s.Stream(ref FlagLastCameraAutoZoomInstantEnabled); s.Stream(ref FlagLastCameraAutoZoomEnabled);
			s.Stream(ref FlagLastCameraZoomEnabled); s.Stream(ref FlagLastCameraYawEnabled);
		}
		#endregion
	};
}