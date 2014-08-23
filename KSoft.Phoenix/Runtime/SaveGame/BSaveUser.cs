
using BVector = SlimMath.Vector4;

namespace KSoft.Phoenix.Runtime
{
	sealed class BSaveUser
		: IO.IEndianStreamSerializable
	{
		public int CurrentPlayer, CoopPlayer;
		public BVector HoverPoint, CameraHoverPoint, CameraPosition, 
			CameraForward, CameraRight, CameraUp;
		public float CameraDefaultPitch, CameraDefaultYaw, CameraDefaultZoom,
			CameraPitch, CameraYaw, CameraZoom,
			CameraFOV, CameraHoverPointOffsetHeight;
		public bool HaveHoverPoint, DefaultCamera;

		#region IEndianStreamSerializable Members
		public void Serialize(IO.EndianStream s)
		{
			s.Stream(ref CurrentPlayer);
			s.Stream(ref CoopPlayer);
			s.StreamV(ref HoverPoint); s.StreamV(ref CameraHoverPoint); s.StreamV(ref CameraPosition);
			s.StreamV(ref CameraForward); s.StreamV(ref CameraRight); s.StreamV(ref CameraUp);
			s.Stream(ref CameraDefaultPitch); s.Stream(ref CameraDefaultYaw); s.Stream(ref CameraDefaultZoom);
			s.Stream(ref CameraPitch); s.Stream(ref CameraYaw); s.Stream(ref CameraZoom);
			s.Stream(ref CameraFOV);
			s.Stream(ref CameraHoverPointOffsetHeight);
			s.Stream(ref HaveHoverPoint);
			s.Stream(ref DefaultCamera);
			s.StreamSignature(cSaveMarker.User1);
		}
		#endregion
	};
}