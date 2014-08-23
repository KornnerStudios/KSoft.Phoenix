
namespace KSoft.Phoenix.Phx
{
	public enum BPowerFlags
	{
		// 0xCC
		SequentialRecharge,// = 1<<0,
		LeaderPower,// = 1<<1,
		ShowTargetHighlight,// = 1<<2,
		ShowLimit,// = 1<<3,
		MultiRechargePower,// = 1<<4,
		UnitPower,// = 1<<5,
		InfiniteUses,// = 1<<6,

		// 0xCD, camera flags
		//CameraEnableUserYaw,// = 1<<0,
		//CameraEnableUserZoom,// = 1<<1,
		//CameraEnableUserScroll,// = 1<<2,
		//CameraEnableAutoZoomInstant, // 6?
		
		// 0xCE
		//CameraEnableAutoZoom, // 0
		NotDisruptable,// = 1<<4, // actually "Disruptable" in code
		//ShowTransportArrows, // 5
		//ShowInPowerMenu, // 6
	};
}