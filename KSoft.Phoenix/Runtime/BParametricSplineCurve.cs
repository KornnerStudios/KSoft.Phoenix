
using BVector = SlimMath.Vector4;

namespace KSoft.Phoenix.Runtime
{
	sealed class BParametricSplineCurve
		: IO.IEndianStreamSerializable
	{
		public BVector A0, A1, A2;
		public bool Valid;

		#region IEndianStreamSerializable Members
		public void Serialize(IO.EndianStream s)
		{
			s.StreamV(ref A0);
			s.StreamV(ref A1);
			s.StreamV(ref A2);
			s.Stream(ref Valid);
		}
		#endregion
	};
}