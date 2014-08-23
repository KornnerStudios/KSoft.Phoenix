
namespace KSoft.Phoenix.HaloWars
{
	public sealed partial class BDatabase
		: Phx.BDatabaseBase
	{
		static readonly Collections.CodeEnum<BCodeObjectType> kGameObjectTypes = new Collections.CodeEnum<BCodeObjectType>();
		static readonly Collections.CodeEnum<BCodeProtoObject> kGameProtoObjectTypes = new Collections.CodeEnum<BCodeProtoObject>();
		static readonly Collections.CodeEnum<BScenarioWorld> kGameScenarioWorlds = new Collections.CodeEnum<BScenarioWorld>();

		public override Collections.IProtoEnum GameObjectTypes { get { return kGameObjectTypes; } }
		public override Collections.IProtoEnum GameProtoObjectTypes { get { return kGameProtoObjectTypes; } }
		public override Collections.IProtoEnum GameScenarioWorlds { get { return kGameScenarioWorlds; } }

		public int RepairPowerID { get; private set; }
		public int RallyPointPowerID { get; private set; }
		public int HookRepairPowerID { get; private set; }
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