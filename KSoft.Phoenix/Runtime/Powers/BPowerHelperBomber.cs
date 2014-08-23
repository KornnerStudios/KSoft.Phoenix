
using BEntityID = System.Int32;

namespace KSoft.Phoenix.Runtime
{
	sealed class BPowerHelperBomber
		: IO.IEndianStreamSerializable
	{
		public BEntityID BomberId;
		public float BomberFlyinDistance, BomberFlyinHeight, BomberBombHeight,
			BomberSpeed, BombTime, FlyoutTime, AdditionalHeight;

		#region IEndianStreamSerializable Members
		public void Serialize(IO.EndianStream s)
		{
			s.Stream(ref BomberId);
			s.Stream(ref BomberFlyinDistance); s.Stream(ref BomberFlyinHeight); s.Stream(ref BomberBombHeight);
			s.Stream(ref BomberSpeed); s.Stream(ref BombTime); s.Stream(ref FlyoutTime); s.Stream(ref AdditionalHeight);
		}
		#endregion
	};
}