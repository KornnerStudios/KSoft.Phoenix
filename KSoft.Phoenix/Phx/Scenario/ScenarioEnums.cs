
namespace KSoft.Phoenix.Phx
{
	public enum BGameType
	{
		Skirmish,
		Campaign,
		Scenario,
	};

	public enum BMapType
	{
		Unknown,

		Final,
		Playtest,
		Development,
		Campaign,

		DLC = Final,
	};

	public enum BScenarioObjectFlags
	{
		NoTieToGround,
		IncludeInSimRep,
	};

	public enum BScenarioPlayerPlacementType
	{
		Invalid,

		Grouped,
		Consecutive, // int "Spacing" attribute
		Random,
	};

	public enum BMissionType
	{
		Invalid,
		Attack,
		Defend,
		Scout,
		Claim,
		Power,
	};

	public enum BMissionState
	{
		Invalid,
		Success,
		Failure,
		Create,
		Working,
		Withdraw,
		Retreat,
	};

	public enum BMissionTargetType
	{
		Invalid,
		Area,
		KBBase,
		CaptureNode,
	};
}