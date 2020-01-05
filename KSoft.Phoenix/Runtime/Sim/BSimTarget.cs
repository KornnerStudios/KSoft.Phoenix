
using BVector = System.Numerics.Vector4;
using BAbilityID = System.Int32;
using BEntityID = System.Int32;

namespace KSoft.Phoenix.Runtime
{
	sealed class BSimTarget
		: IO.IEndianStreamSerializable
	{
		public BVector Position;
		public BEntityID ID;
		public float Range;
		public BAbilityID AbilityID;
		public bool PositionValid, RangeValid, AbilityIDValid;

		#region IEndianStreamSerializable Members
		public void Serialize(IO.EndianStream s)
		{
			s.StreamV(ref Position);
			s.Stream(ref ID);
			s.Stream(ref Range);
			s.Stream(ref AbilityID);
			s.Stream(ref PositionValid); s.Stream(ref RangeValid); s.Stream(ref AbilityIDValid);
		}
		#endregion
	};
}