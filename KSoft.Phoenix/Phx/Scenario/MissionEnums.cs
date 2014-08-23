
namespace KSoft.Phoenix.Phx
{
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