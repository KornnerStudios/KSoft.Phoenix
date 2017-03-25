
namespace KSoft.Phoenix.Phx
{
	public enum BMinigameType
	{
		None,

		OneButtonPress,
		TwoButtonPress,
		ThreeButtonPress,
	};

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

	public enum BPowerType
	{
		Invalid,

		Cleansing,
		Orbital,
		CarpetBombing,
		Cryo,
		Rage,
		Wave,
		Disruption,
		Transport,
		ODST,
		Repair,
	};

	public enum ProtoPowerDataType
	{
		Invalid,
		Float,
		Int,
		ProtoObject,
		ProtoSquad,
		Tech,
		Bool,
		[System.Obsolete]
		Cost,
		ObjectType,
		Sound,
		Texture,
	};
}