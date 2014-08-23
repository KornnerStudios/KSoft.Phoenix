
namespace KSoft.Phoenix.Runtime
{
	interface ITechNode
		: IO.IEndianStreamSerializable
	{
		float ResearchPoints { get; set; }
		int ResearchBuilding { get; set; }
		Phx.BProtoTechStatus Status { get; set; }
		bool Unique { get; set; }
	};
}