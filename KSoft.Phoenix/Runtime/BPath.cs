
using BVector = SlimMath.Vector4;

namespace KSoft.Phoenix.Runtime
{
	partial class cSaveMarker
	{
		public const ushort
			cSaveMarkerPath1 = 0x2710
			;
	};

	public sealed class BPath
		: IO.IEndianStreamSerializable
	{
		const int cMaximumWaypoints = 0x2710;

		public BVector[] Waypoints;
		public byte Flags;
		public float PathLength;
		public uint CreationTime;

		#region IEndianStreamSerializable Members
		public void Serialize(IO.EndianStream s)
		{
			BSaveGame.StreamVectorArray16(s, ref Waypoints, cMaximumWaypoints);
			s.Stream(ref Flags);
			s.Stream(ref PathLength);
			s.Stream(ref CreationTime);

			s.StreamSignature(cSaveMarker.cSaveMarkerPath1);
		}
		#endregion
	};
}