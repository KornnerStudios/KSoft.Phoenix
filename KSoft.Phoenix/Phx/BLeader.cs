using System.Collections.Generic;

using BProtoSquadID = System.Int32;

namespace KSoft.Phoenix.Phx
{
	public sealed class BLeader
		: DatabaseNamedObject
	{
		#region Xml constants
		public static readonly XML.BListXmlParams kBListXmlParams = new XML.BListXmlParams("Leader")
		{
			DataName = "Name",
			Flags = 0
		};
		public static readonly Engine.XmlFileInfo kXmlFileInfo = new Engine.XmlFileInfo
		{
			Directory = Engine.GameDirectory.Data,
			FileName = "Leaders.xml",
			RootName = kBListXmlParams.RootName
		};

		const string kXmlElementTech = "Tech";
		const string kXmlElementCiv = "Civ";
		const string kXmlElementPower = "Power";
		const string kXmlElementNameID = "NameID";

		// TODO: HW's FlashPortrait elements have an ending " character (sans a starting quote). Becareful!

		const string kXmlElementSupportPower = "SupportPower";
		const string kXmlElementSupportPowerAttrTechPrereq = "TechPrereq"; // proto tech
		// Can have multiple powers...
		const string kXmlElementSupportPowerElementPower = "Power";

		const string kXmlElementStartingUnit = "StartingUnit";
		const string kXmlElementStartingUnitAttrBuildOther = "BuildOther";
		const string kXmlElementStartingSquad = "StartingSquad";
		// we ignore these values. would require use to create a special BLeaderStartingSquad type
		const string kXmlElementStartingSquadAttrFlyIn = "FlyIn"; // bool
		const string kXmlElementStartingSquadAttrOffset = "Offset"; // Vector3f
		#endregion

		int mTechID = TypeExtensions.kNone;
		public int TechID { get { return mTechID; } }
		int mCivID = TypeExtensions.kNone;
		public int CivID { get { return mCivID; } }
		int mPowerID = TypeExtensions.kNone;
		public int PowerID { get { return mPowerID; } }

		int mNameID = TypeExtensions.kNone;
		public int NameID { get { return mNameID; } }

		/// <summary>Initial resources</summary>
		public Collections.BTypeValuesSingle Resources { get; private set; }

		int mStartingUnitID = TypeExtensions.kNone;
		public int StartingUnitID { get { return mStartingUnitID; } }
		int mStartingUnitBuildOtherID = TypeExtensions.kNone;
		public int StartingUnitBuildOtherID { get { return mStartingUnitBuildOtherID; } }
		public List<BProtoSquadID> StartingSquadIDs { get; private set; }

		public Collections.BTypeValues<BPopulation> Populations { get; private set; }

		// Empty Leaders just have a Civ
		public bool IsEmpty { get { return mTechID.IsNone(); } }

		public BLeader()
		{
			Resources = new Collections.BTypeValuesSingle(BResource.kBListTypeValuesParams);

			StartingSquadIDs = new List<BProtoSquadID>();

			Populations = new Collections.BTypeValues<BPopulation>(BPopulation.kBListParams);
		}

		#region ITagElementStreamable<string> Members
		public override void Serialize<TDoc, TCursor>(IO.TagElementStream<TDoc, TCursor, string> s)
		{
			base.Serialize(s);

			var xs = s.GetSerializerInterface();

			xs.StreamDBID(s, kXmlElementTech, ref mTechID, DatabaseObjectKind.Tech);
			xs.StreamDBID(s, kXmlElementCiv, ref mCivID, DatabaseObjectKind.Civ);
			xs.StreamDBID(s, kXmlElementPower, ref mPowerID, DatabaseObjectKind.Power);
			xs.StreamStringID(s, kXmlElementNameID, ref mNameID);

			XML.XmlUtil.Serialize(s, Resources, BResource.kBListTypeValuesXmlParams);
			using (var bm = s.EnterCursorBookmarkOpt(kXmlElementStartingUnit, mStartingUnitID, Predicates.IsNotNone)) if (bm.IsNotNull)
			{
				xs.StreamDBID(s, /*xmlName:*/null, ref mStartingUnitID, DatabaseObjectKind.Object, false, XML.XmlUtil.kSourceCursor);
				xs.StreamDBID(s, kXmlElementStartingUnitAttrBuildOther, ref mStartingUnitBuildOtherID, DatabaseObjectKind.Object, false, XML.XmlUtil.kSourceAttr);
			}
			s.StreamElements(kXmlElementStartingSquad, StartingSquadIDs, xs, XML.BXmlSerializerInterface.StreamSquadID);
			XML.XmlUtil.Serialize(s, Populations, BPopulation.kBListXmlParams);
		}
		#endregion
	};
}