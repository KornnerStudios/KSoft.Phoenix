
namespace KSoft.Phoenix.Phx
{
	public sealed class BProtoPower
		: DatabaseNamedObject
	{
		#region Xml constants
		public static readonly XML.BListXmlParams kBListXmlParams = new XML.BListXmlParams("Power")
		{
			DataName = DatabaseNamedObject.kXmlAttrName,
			Flags = XML.BCollectionXmlParamsFlags.RequiresDataNamePreloading
		};
		public static readonly Engine.XmlFileInfo kXmlFileInfo = new Engine.XmlFileInfo
		{
			Directory = Engine.GameDirectory.Data,
			FileName = "Powers.xml",
			RootName = kBListXmlParams.RootName
		};

		const string kXmlElementAttributes = "Attributes";

		// can have multiple dynamic costs
//		const string kXmlElementDynamicCost = "DynamicCost"; // inner text is a float (the actual cost)
//		const string kXmlElementDynamicCostAttrObjectType = "ObjectType"; // GameObjectTypes
		// can have multiple
//		const string kXmlElementTargetEffectiveness = "TargetEffectiveness"; // inner text is an integer
//		const string kXmlElementTargetEffectivenessAttrObjectType = "ObjectType"; // GameObjectTypes
//		const string kXmlElementUIRadius = "UIRadius"; // float
		const string kXmlElementPowerType = "PowerType";
		const string kXmlElementAutoRecharge = "AutoRecharge";
		const string kXmlElementUseLimit = "UseLimit"; // integer
//		const string kXmlElementIcon = "Icon";
		// can have multiple
//		const string kXmlElementIconLocation = "IconLocation"; // string id
		// can have multiple
		const string kXmlElementTechPrereq = "TechPrereq"; // proto tech
//		const string kXmlElementChooseTextID = "ChooseTextID";
//		const string kXmlElementAction = "Action"; // BActionType
//		const string kXmlElementMinigame = "Minigame"; // BMinigameType
//		const string kXmlElementCameraZoomMin = "CameraZoomMin"; // float
//		const string kXmlElementCameraZoomMax = "CameraZoomMax"; // float
//		const string kXmlElementCameraPitchMin = "CameraPitchMin"; // float
//		const string kXmlElementCameraPitchMax = "CameraPitchMax"; // float
//		const string kXmlElementCameraEffectIn = "CameraEffectIn"; // camera effect name
//		const string kXmlElementCameraEffectOut = "CameraEffectOut"; // camera effect name
//		const string kXmlElementMinDistanceToSquad = "MinDistanceToSquad"; // float
//		const string kXmlElementMaxDistanceToSquad = "MaxDistanceToSquad"; // float
//		const string kXmlElementCameraEnableUserScroll = "CameraEnableUserScroll"; // bool
//		const string kXmlElementCameraEnableUserYaw = "CameraEnableUserYaw"; // bool
//		const string kXmlElementCameraEnableUserZoom = "CameraEnableUserZoom"; // bool
//		const string kXmlElementCameraEnableAutoZoomInstant = "CameraEnableAutoZoomInstant"; // bool
//		const string kXmlElementCameraEnableAutoZoom = "CameraEnableAutoZoom"; // bool
//		const string kXmlElementShowTargetHighlight = "ShowTargetHighlight";
//		const string kXmlElementShowTargetHighlightAttrObjectType = "ObjectType"; // GameObjectTypes
//		const string kXmlElementShowTargetHighlightAttrRelation = "Relation"; // BDiplomacy
//		const string kXmlElementShowInPowerMenu = "ShowInPowerMenu"; // bool
//		const string kXmlElementShowTransportArrows = "ShowTransportArrows"; // bool
//		const string kXmlElementChildObjects = "ChildObjects";
//		const string kXmlElementChildObjectsObject = "Object"; // proto object name
		const string kXmlElementDataLevel = "DataLevel";
		const string kXmlElementBaseDataLevel = "BaseDataLevel";

		const string kXmlElementTriggerScript = "TriggerScript";
		const string kXmlElementCommandTriggerScript = "CommandTriggerScript";
		#endregion

		public Collections.BTypeValuesSingle Cost { get; private set; }

		public Collections.BTypeValuesSingle Populations { get; private set; }

		BPowerType mPowerType = BPowerType.None;
		float mAutoRecharge = PhxUtil.kInvalidSingle;
		int mUseLimit = TypeExtensions.kNone;
//		BPowerFlags mFlags; // TODO

		string mTriggerScript, mCommandTriggerScript;

		public BProtoPower()
		{
			Cost = new Collections.BTypeValuesSingle(BResource.kBListTypeValuesParams);

			Populations = new Collections.BTypeValuesSingle(BPopulation.kBListParamsSingle);
		}

		#region ITagElementStreamable<string> Members
		void StreamFlags<TDoc, TCursor>(IO.TagElementStream<TDoc, TCursor, string> s)
			where TDoc : class
			where TCursor : class
		{
			// TODO: enabled flags are written as xml elements without any attributes or text. Eg:
			// <InfiniteUses /> means the BPowerFlags.InfiniteUses bit is set
		}
		public override void Serialize<TDoc, TCursor>(IO.TagElementStream<TDoc, TCursor, string> s)
		{
			using (s.EnterCursorBookmark(kXmlElementAttributes))
			{
				base.Serialize(s);

				XML.XmlUtil.SerializeCostHack(s, Cost);
				// DynamicCost
				// TargetEffectiveness
				XML.XmlUtil.Serialize(s, Populations, BPopulation.kBListXmlParamsSingle_LowerCase);

				s.StreamElementEnumOpt(kXmlElementPowerType, ref mPowerType, e => e != BPowerType.None);
				s.StreamElementOpt    (kXmlElementAutoRecharge, ref mAutoRecharge, PhxPredicates.IsNotInvalid);
				s.StreamElementOpt    (kXmlElementUseLimit, ref mUseLimit, Predicates.IsNotNone);
				StreamFlags(s);
			}
			s.StreamElementOpt(kXmlElementTriggerScript, ref mTriggerScript, Predicates.IsNotNullOrEmpty);
			s.StreamElementOpt(kXmlElementCommandTriggerScript, ref mCommandTriggerScript, Predicates.IsNotNullOrEmpty);
		}
		#endregion
	};
}