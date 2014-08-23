
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
}