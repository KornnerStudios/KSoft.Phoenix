
namespace KSoft.Phoenix.HaloWars
{
	public sealed partial class BDatabase
		: Phx.BDatabaseBase
	{
		static readonly Collections.CodeEnum<BCodeObjectType> kGameObjectTypes = new();
		static readonly Collections.CodeEnum<BCodeProtoObject> kGameProtoObjectTypes = new();
		static readonly Collections.CodeEnum<BScenarioWorld> kGameScenarioWorlds = new();

		public override Collections.IProtoEnum GameObjectTypes => kGameObjectTypes;
		public override Collections.IProtoEnum GameProtoObjectTypes => kGameProtoObjectTypes;
		public override Collections.IProtoEnum GameScenarioWorlds => kGameScenarioWorlds;

		[Phx.Meta.BProtoPowerReference]
		public int RepairPowerID { get; private set; }
		[Phx.Meta.BProtoPowerReference]
		public int RallyPointPowerID { get; private set; }
		[Phx.Meta.BProtoPowerReference]
		public int HookRepairPowerID { get; private set; }
		[Phx.Meta.BProtoPowerReference]
		public int UnscOdstDropPowerID { get; private set; }

		public BDatabase(Engine.PhxEngine engine) : base(engine, kGameObjectTypes)
		{
			RepairPowerID = RallyPointPowerID = HookRepairPowerID = UnscOdstDropPowerID =
				TypeExtensions.kNone;
		}

		internal void SetupDBIDs()
		{
			RepairPowerID = base.GetId(Phx.DatabaseObjectKind.Power, "_Repair");
			RallyPointPowerID = base.GetId(Phx.DatabaseObjectKind.Power, "_RallyPoint");
			HookRepairPowerID = base.GetId(Phx.DatabaseObjectKind.Power, "HookRepair");
			UnscOdstDropPowerID = base.GetId(Phx.DatabaseObjectKind.Power, "UnscOdstDrop");
		}
	};
}