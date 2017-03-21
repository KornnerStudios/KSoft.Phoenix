
namespace KSoft.Phoenix.Phx
{
	public enum BPowerFlags
	{
		SequentialRecharge,
		InfiniteUses,
		UnitPower,
		NotDisruptable,
		MultiRechargePower,
		ShowLimit,
		//ShowTargetHighlight,
		LeaderPower,
	};

	// The following flags respect the true/false value in the XmlText
	public enum BPowerToggableFlags
	{
		CameraEnableUserScroll,
		CameraEnableUserYaw,
		CameraEnableUserZoom,
		CameraEnableAutoZoomInstant,
		CameraEnableAutoZoom,

		ShowInPowerMenu,
		ShowTransportArrows,
	};
}