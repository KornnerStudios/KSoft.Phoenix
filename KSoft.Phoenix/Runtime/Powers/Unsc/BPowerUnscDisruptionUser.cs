﻿
using BProtoObjectID = System.Int32;

namespace KSoft.Phoenix.Runtime
{
	sealed class BPowerUnscDisruptionUser
		: BPowerUser
	{
		public int LOSMode;
		public BProtoObjectID DisruptionObjectProtoID;
		public BPowerHelperHudSounds HudSounds = new BPowerHelperHudSounds();

		#region IEndianStreamSerializable Members
		public override void Serialize(IO.EndianStream s)
		{
			base.Serialize(s);

			s.Stream(ref LOSMode);
			s.Stream(ref DisruptionObjectProtoID);
			s.Stream(HudSounds);
		}
		#endregion
	};
}