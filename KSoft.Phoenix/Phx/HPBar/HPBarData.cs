using System;
#if CONTRACTS_FULL_SHIM
using Contract = System.Diagnostics.ContractsShim.Contract;
#else
using Contract = System.Diagnostics.Contracts.Contract; // SHIM'D
#endif

namespace KSoft.Phoenix.Phx
{
	[ProtoDataTypeObjectSourceKind(ProtoDataObjectSourceKind.HPData)]
	public sealed class HPBarData
		: IO.ITagElementStringNameStreamable
		, IProtoDataObjectDatabaseProvider
	{
		public ProtoDataObjectDatabase ObjectDatabase { get; private set; }

		#region Xml constants
		const string kXmlRoot = "HPBarDefinition";

		public static readonly Engine.XmlFileInfo kXmlFileInfo = new()
		{
			Directory = Engine.GameDirectory.Data,
			FileName = "HPBars.xml",
			RootName = kXmlRoot
		};
		public static readonly Engine.ProtoDataXmlFileInfo kProtoFileInfo = new(
			Engine.XmlFilePriority.GameData,
			kXmlFileInfo);
		#endregion

		public Collections.BListAutoId<BProtoHPBar> HPBars { get; private set; } = new();
		public Collections.BListAutoId<BProtoHPBarColorStages> ColorStages { get; private set; } = new();
		public Collections.BListAutoId<BProtoVeterancyBar> VeterancyBars { get; private set; } = new();
		public Collections.BListAutoId<BProtoPieProgress> PieProgress { get; private set; } = new();
		public Collections.BListAutoId<BProtoBobbleHead> BobbleHeads { get; private set; } = new();
		public Collections.BListAutoId<BProtoBuildingStrength> BuildingStrengths { get; private set; } = new();

		public HPBarData()
		{
			ObjectDatabase = new ProtoDataObjectDatabase(this, typeof(HPBarDataObjectKind));

			InitializeDatabaseInterfaces();
		}

		public void Clear()
		{
			HPBars.Clear();
			ColorStages.Clear();
			VeterancyBars.Clear();
			PieProgress.Clear();
			BobbleHeads.Clear();
			BuildingStrengths.Clear();
		}

		#region Database interfaces
		void InitializeDatabaseInterfaces()
		{
			HPBars.SetupDatabaseInterface();
			ColorStages.SetupDatabaseInterface();
			VeterancyBars.SetupDatabaseInterface();
			PieProgress.SetupDatabaseInterface();
			BobbleHeads.SetupDatabaseInterface();
			BuildingStrengths.SetupDatabaseInterface();
		}

		internal Collections.IBTypeNames GetNamesInterface(HPBarDataObjectKind kind)
		{
			Contract.Requires<ArgumentOutOfRangeException>(kind != HPBarDataObjectKind.None);

			return kind switch
			{
				HPBarDataObjectKind.HPBar => HPBars,
				HPBarDataObjectKind.ColorStages => ColorStages,
				HPBarDataObjectKind.VeterancyBar => VeterancyBars,
				HPBarDataObjectKind.PieProgress => PieProgress,
				HPBarDataObjectKind.BobbleHead => BobbleHeads,
				HPBarDataObjectKind.BuildingStrength => BuildingStrengths,
				_ => throw new KSoft.Debug.UnreachableException(kind.ToString()),
			};
		}

		internal Collections.IHasUndefinedProtoMemberInterface GetMembersInterface(HPBarDataObjectKind kind)
		{
			Contract.Requires<ArgumentOutOfRangeException>(kind != HPBarDataObjectKind.None);

			return kind switch
			{
				HPBarDataObjectKind.HPBar => HPBars,
				HPBarDataObjectKind.ColorStages => ColorStages,
				HPBarDataObjectKind.VeterancyBar => VeterancyBars,
				HPBarDataObjectKind.PieProgress => PieProgress,
				HPBarDataObjectKind.BobbleHead => BobbleHeads,
				HPBarDataObjectKind.BuildingStrength => BuildingStrengths,
				_ => throw new KSoft.Debug.UnreachableException(kind.ToString()),
			};
		}
		#endregion

		#region ITagElementStreamable<string> Members
		/// <remarks>For streaming directly from hpbars.xml</remarks>
		internal void StreamHPBarData<TDoc, TCursor>(IO.TagElementStream<TDoc, TCursor, string> s)
			where TDoc : class
			where TCursor : class
		{
			XML.XmlUtil.Serialize(s, HPBars, BProtoHPBar.kBListXmlParams);
			XML.XmlUtil.Serialize(s, ColorStages, BProtoHPBarColorStages.kBListXmlParams);
			XML.XmlUtil.Serialize(s, VeterancyBars, BProtoVeterancyBar.kBListXmlParams);
			XML.XmlUtil.Serialize(s, PieProgress, BProtoPieProgress.kBListXmlParams);
			XML.XmlUtil.Serialize(s, BobbleHeads, BProtoBobbleHead.kBListXmlParams);
			XML.XmlUtil.Serialize(s, BuildingStrengths, BProtoBuildingStrength.kBListXmlParams);
		}

		public void Serialize<TDoc, TCursor>(IO.TagElementStream<TDoc, TCursor, string> s)
			where TDoc : class
			where TCursor : class
		{
			using (s.EnterCursorBookmark(kXmlRoot))
			{
				StreamHPBarData(s);
			}
		}
		#endregion

		#region IProtoDataObjectDatabaseProvider members
		Engine.XmlFileInfo IProtoDataObjectDatabaseProvider.SourceFileReference => kXmlFileInfo;

		Collections.IBTypeNames IProtoDataObjectDatabaseProvider.GetNamesInterface(int objectKind)
		{
			var kind = (HPBarDataObjectKind)objectKind;
			return GetNamesInterface(kind);
		}

		Collections.IHasUndefinedProtoMemberInterface IProtoDataObjectDatabaseProvider.GetMembersInterface(int objectKind)
		{
			var kind = (HPBarDataObjectKind)objectKind;
			return GetMembersInterface(kind);
		}
		#endregion
	};
}