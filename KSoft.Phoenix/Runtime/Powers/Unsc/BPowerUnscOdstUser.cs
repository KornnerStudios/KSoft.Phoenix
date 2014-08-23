
using BVector = SlimMath.Vector4;
using BProtoObjectID = System.Int32;

namespace KSoft.Phoenix.Runtime
{
	sealed class BPowerUnscOdstUser
		: BPowerUser
	{
		public string HelpString;
		public BPowerHelperHudSounds HudSounds = new BPowerHelperHudSounds();
		public int LOSMode;
		public BProtoObjectID ODSTProtoSquadID, ODSTProtoObjectID;
		public int CanFire;
		public BVector ValidDropLocation;

		#region IEndianStreamSerializable Members
		public override void Serialize(IO.EndianStream s)
		{
			base.Serialize(s);

			s.StreamPascalWideString32(ref HelpString);
			s.Stream(HudSounds);
			s.Stream(ref LOSMode);
			s.Stream(ref ODSTProtoSquadID); s.Stream(ref ODSTProtoObjectID);
			s.Stream(ref CanFire);
			s.StreamV(ref ValidDropLocation);
		}
		#endregion
	};
}