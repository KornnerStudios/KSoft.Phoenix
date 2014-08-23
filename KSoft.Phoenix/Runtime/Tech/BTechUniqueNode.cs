
namespace KSoft.Phoenix.Runtime
{
	using BProtoTechStatusStreamer = IO.EnumBinaryStreamer<Phx.BProtoTechStatus>;

	struct BTechUniqueNode
		: ITechNode
	{
		public float ResearchPoints;
		public int ResearchBuilding;
		public Phx.BProtoTechStatus Status;
		public bool Unique;

		public int UnitID;

		#region IEndianStreamSerializable Members
		public void Serialize(IO.EndianStream s)
		{
			s.Stream(ref UnitID);
			s.Stream(ref ResearchPoints);
			s.Stream(ref ResearchBuilding);
			s.Stream(ref Status, BProtoTechStatusStreamer.Instance);
			s.Stream(ref Unique);
		}
		#endregion

		#region ITechNode Members
		float ITechNode.ResearchPoints
		{
			get { return this.ResearchPoints; }
			set { this.ResearchPoints = value; }
		}
		int ITechNode.ResearchBuilding
		{
			get { return this.ResearchBuilding; }
			set { this.ResearchBuilding = value; }
		}
		Phx.BProtoTechStatus ITechNode.Status
		{
			get { return this.Status; }
			set { this.Status = value; }
		}
		bool ITechNode.Unique
		{
			get { return this.Unique; }
			set { this.Unique = value; }
		}
		#endregion
	};
}