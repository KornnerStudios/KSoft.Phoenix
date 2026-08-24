
namespace KSoft.Phoenix.HaloWars
{
	[System.Diagnostics.CodeAnalysis.SuppressMessage("Design", "CA1027:Mark enums with FlagsAttribute", Justification = "Values are mutually exclusive chat speaker identifiers.")]
	public enum BChatSpeaker
	{
		Serena,
		Forge,
		Cutter,
		Voice_of_God,
		Generic_Soldiers,
		Arcadian_Police,
		Civilians,
		Anders,
		RhinoCommander,
		Spartan1,
		Spartan2,
		SpartanSniper,
		SpartanRockeLauncher,

		Covenant = Anders,
		Arbiter = RhinoCommander,

		ODST = SpartanRockeLauncher+1+1 + 1,
	};
}