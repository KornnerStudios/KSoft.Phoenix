
using BUnitOppID = System.Int32;

namespace KSoft.Phoenix.Runtime
{
	struct BHardpoint
		: IO.IEndianStreamSerializable
	{
		public const int kMaxCount = 0x14;

		public float YawRotationRate, PitchRotationRate;

		#region IEndianStreamSerializable Members
		public void Serialize(IO.EndianStream s)
		{
			s.Stream(ref YawRotationRate);
			s.Stream(ref PitchRotationRate);
		}
		#endregion
	};

	public struct BHardpointState
		: IO.IEndianStreamSerializable
	{
		public int OwnerAction;
		public float AutoCenteringTimer, YawSoundActivationTimer, PitchSoundActivationTimer;
		public BUnitOppID OppID;
		public bool
			AllowAutoCentering, YawSound, YawSoundPlaying,
			PitchSound, PitchSoundPlaying, SecondaryTurretScanToken
			;

		#region IEndianStreamSerializable Members
		public void Serialize(IO.EndianStream s)
		{
			s.Stream(ref OwnerAction);
			s.Stream(ref AutoCenteringTimer);	s.Stream(ref YawSoundActivationTimer);	s.Stream(ref PitchSoundActivationTimer);
			s.Stream(ref OppID);
			s.Stream(ref AllowAutoCentering);	s.Stream(ref YawSound);				s.Stream(ref YawSoundPlaying);
			s.Stream(ref PitchSound);			s.Stream(ref PitchSoundPlaying);	s.Stream(ref SecondaryTurretScanToken);
		}
		#endregion
	};
}