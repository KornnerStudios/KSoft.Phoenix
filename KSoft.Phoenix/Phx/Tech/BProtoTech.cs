
namespace KSoft.Phoenix.Phx
{
	public sealed class BProtoTech
		: DatabaseIdObject
	{
		#region Xml constants
		public static readonly XML.BListXmlParams kBListXmlParams = new XML.BListXmlParams("Tech")
		{
			RootName = "TechTree",
			DataName = "name",
			Flags = //XML.BCollectionXmlParamsFlags.ToLowerDataNames |
				XML.BCollectionXmlParamsFlags.RequiresDataNamePreloading |
				XML.BCollectionXmlParamsFlags.SupportsUpdating
		};
		public static readonly Engine.XmlFileInfo kXmlFileInfo = new Engine.XmlFileInfo
		{
			Location = Engine.ContentStorage.Game,
			Directory = Engine.GameDirectory.Data,
			FileName = "Techs.xml",
			RootName = kBListXmlParams.RootName
		};
		public static readonly Engine.XmlFileInfo kXmlFileInfoUpdate = new Engine.XmlFileInfo
		{
			Location = Engine.ContentStorage.Update,
			Directory = Engine.GameDirectory.Data,
			FileName = "Techs_Update.xml",
			RootName = kBListXmlParams.RootName
		};
		#endregion

		BProtoTechStatus mStatus;

		public BProtoTechPrereqs Prereqs { get; private set; }
		public bool HasPrereqs { get { return Prereqs != null; } }

		public Collections.BListArray<BProtoTechEffect> Effects { get; private set; }

		public BProtoTech() : base(BResource.kBListTypeValuesParams, BResource.kBListTypeValuesXmlParams_CostLowercaseType)
		{
			Effects = new Collections.BListArray<BProtoTechEffect>();
		}

		#region IXmlElementStreamable Members
		protected override void StreamDbId<TDoc, TCursor>(IO.TagElementStream<TDoc, TCursor, string> s)
		{
			// This isn't always used, nor unique
			s.StreamElementOpt("DBID", ref mDbId, Predicates.IsNotNone);
		}

		bool ShouldStreamPrereqs<TDoc, TCursor>(IO.TagElementStream<TDoc, TCursor, string> s)
			where TDoc : class
			where TCursor : class
		{
			if (s.IsReading)
			{
				bool has_prereqs = s.ElementsExists(BProtoTechPrereqs.kXmlRootName);

				if (has_prereqs)
					Prereqs = new BProtoTechPrereqs();
				return has_prereqs;
			}
			else if (s.IsWriting)
				return HasPrereqs;

			return false;
		}
		public override void Serialize<TDoc, TCursor>(IO.TagElementStream<TDoc, TCursor, string> s)
		{
			base.Serialize(s);

			//Alpha
			//Flag
			//DisplayNameID
			//RolloverTextID
			//PrereqTextID
			//Cost
			//ResearchPoints
			// TODO: just check if this is "Unobtainable" and set the proper flag, don't actively use this field
			s.StreamElementEnum("Status", ref mStatus);
			//Icon
			//ResearchCompleteSound
			//ResearchAnim
			if (ShouldStreamPrereqs(s))
				Prereqs.Serialize(s);
			XML.XmlUtil.Serialize(s, Effects, BProtoTechEffect.kBListXmlParams);
			//StatsObject ProtoObject
		}
		#endregion
	};
}