using System;
using Contracts = System.Diagnostics.Contracts;
using Contract = System.Diagnostics.Contracts.Contract;

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

		public static readonly Engine.XmlFileInfo kXmlFileInfo = new Engine.XmlFileInfo
		{
			Directory = Engine.GameDirectory.Data,
			FileName = "HPBars.xml",
			RootName = kXmlRoot
		};
		public static readonly Engine.ProtoDataXmlFileInfo kProtoFileInfo = new Engine.ProtoDataXmlFileInfo(
			Engine.XmlFilePriority.GameData,
			kXmlFileInfo);
		#endregion

		public Collections.BListAutoId<BProtoHPBar> HPBars { get; private set; }
		public Collections.BListAutoId<BProtoHPBarColorStages> ColorStages { get; private set; }
		public Collections.BListAutoId<BProtoVeterancyBar> VeterancyBars { get; private set; }
		public Collections.BListAutoId<BProtoPieProgress> PieProgress { get; private set; }
		public Collections.BListAutoId<BProtoBobbleHead> BobbleHeads { get; private set; }
		public Collections.BListAutoId<BProtoBuildingStrength> BuildingStrengths { get; private set; }

		public HPBarData()
		{
			ObjectDatabase = new ProtoDataObjectDatabase(this, typeof(HPBarDataObjectKind));

			HPBars = new Collections.BListAutoId<BProtoHPBar>();
			ColorStages = new Collections.BListAutoId<BProtoHPBarColorStages>();
			VeterancyBars = new Collections.BListAutoId<BProtoVeterancyBar>();
			PieProgress = new Collections.BListAutoId<BProtoPieProgress>();
			BobbleHeads = new Collections.BListAutoId<BProtoBobbleHead>();
			BuildingStrengths = new Collections.BListAutoId<BProtoBuildingStrength>();

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

			switch (kind)
			{
			case HPBarDataObjectKind.HPBar:				return HPBars;
			case HPBarDataObjectKind.ColorStages:		return ColorStages;
			case HPBarDataObjectKind.VeterancyBar:		return VeterancyBars;
			case HPBarDataObjectKind.PieProgress:		return PieProgress;
			case HPBarDataObjectKind.BobbleHead:		return BobbleHeads;
			case HPBarDataObjectKind.BuildingStrength:	return BuildingStrengths;

			default: throw new KSoft.Debug.UnreachableException(kind.ToString());
			}
		}

		internal Collections.IHasUndefinedProtoMemberInterface GetMembersInterface(HPBarDataObjectKind kind)
		{
			Contract.Requires<ArgumentOutOfRangeException>(kind != HPBarDataObjectKind.None);

			switch (kind)
			{
			case HPBarDataObjectKind.HPBar:				return HPBars;
			case HPBarDataObjectKind.ColorStages:		return ColorStages;
			case HPBarDataObjectKind.VeterancyBar:		return VeterancyBars;
			case HPBarDataObjectKind.PieProgress:		return PieProgress;
			case HPBarDataObjectKind.BobbleHead:		return BobbleHeads;
			case HPBarDataObjectKind.BuildingStrength:	return BuildingStrengths;

			default: throw new KSoft.Debug.UnreachableException(kind.ToString());
			}
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
				StreamHPBarData(s);
		}
		#endregion

		#region IProtoDataObjectDatabaseProvider members
		Engine.XmlFileInfo IProtoDataObjectDatabaseProvider.SourceFileReference { get { return kXmlFileInfo; } }

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