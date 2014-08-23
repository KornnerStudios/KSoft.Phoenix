
using BVector = SlimMath.Vector4;
using BEntityID = System.Int32;

namespace KSoft.Phoenix.Runtime
{
	sealed class BPowerTransportUser
		: BPowerUser
	{
		public BPowerHelperHudSounds HudSounds = new BPowerHelperHudSounds();
		public BEntityID[] SquadsToTransport;
		public BEntityID[] TargetedSquads;
		public int LOSMode;
		public bool GotPickupLocation;
		public BVector PickupLocation;

		#region IEndianStreamSerializable Members
		public override void Serialize(IO.EndianStream s)
		{
			base.Serialize(s);

			s.Stream(HudSounds);
			BSaveGame.StreamArray(s, ref SquadsToTransport);
			BSaveGame.StreamArray(s, ref TargetedSquads);
			s.Stream(ref LOSMode);
			s.Stream(ref GotPickupLocation);
			s.StreamV(ref PickupLocation);
		}
		#endregion
	};
}