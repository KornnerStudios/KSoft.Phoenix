
namespace KSoft.Phoenix.Runtime
{
	sealed class BParametricSplineCurve
		: IO.IEndianStreamSerializable
	{
		public sealed class UnknownData
			: IO.IEndianStreamSerializable
		{
			public ulong Unknown0, Unknown8, Unknown10, Unknown18,
				Unknown20, Unknown28;

			#region IEndianStreamSerializable Members
			public void Serialize(IO.EndianStream s)
			{
				s.Stream(ref Unknown0); s.Stream(ref Unknown8); s.Stream(ref Unknown10); s.Stream(ref Unknown18);
				s.Stream(ref Unknown20);s.Stream(ref Unknown28);
			}
			#endregion
		};

		public UnknownData A = new UnknownData();
		public bool Valid;

		#region IEndianStreamSerializable Members
		public void Serialize(IO.EndianStream s)
		{
			s.Stream(A);
			s.Stream(ref Valid);
		}
		#endregion
	};
}