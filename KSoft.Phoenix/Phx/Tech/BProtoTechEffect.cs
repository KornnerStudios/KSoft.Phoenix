using Contracts = System.Diagnostics.Contracts;
using Contract = System.Diagnostics.Contracts.Contract;

namespace KSoft.Phoenix.Phx
{
	// internal engine structure is only 0x34 bytes...
	public sealed partial class BProtoTechEffect
		: IO.ITagElementStringNameStreamable
	{
		#region Xml constants
		public static readonly XML.BListXmlParams kBListXmlParams = new XML.BListXmlParams("Effect")
		{
			Flags = 0
		};

		const string kXmlAttrType = "type";
		/// <remarks></remarks>
		const string kXmlAttrAllActions = "allactions";
		const string kXmlAttrAction = "action";
		const string kXmlAttrSubType = "subtype";
		const string kXmlAttrAmount = "amount";
		const string kXmlAttrRelativity = "relativity";

		// TransformProtoUnit, TransformProtoSquad
		const string kXmlTransformProto_AttrFromType = "FromType";
		const string kXmlTransformProto_AttrToType = "ToType";

		const string kXmlAttachSquadAttrType = "squadType";
		#endregion

		BProtoTechEffectType mType;
		public BProtoTechEffectType Type { get { return mType; } }

		DataUnion mDU;

		#region ObjectData
		bool mAllActions;

		string mAction;

		public BObjectDataType SubType { get { return mDU.SubType; } }

		// Amount can be negative, so use NaN as the 'invalid' value instead
		float mAmount = PhxUtil.kInvalidSingleNaN;

		BObjectDataRelative mRelativity = BObjectDataRelative.Invalid;

		#region Command
		public BProtoObjectCommandType CommandType { get {
			Contract.Requires(Type == BProtoTechEffectType.Data);
			Contract.Requires(SubType == BObjectDataType.CommandEnable || SubType == BObjectDataType.CommandSelectable);
			return mDU.CommandType;
		} }

		public int CommandDataID { get {
			Contract.Requires(Type == BProtoTechEffectType.Data);
			Contract.Requires(SubType == BObjectDataType.CommandEnable || SubType == BObjectDataType.CommandSelectable);
			return mDU.CommandData;
		} }
		public BSquadMode CommandDataSquadMode { get {
			Contract.Requires(Type == BProtoTechEffectType.Data);
			Contract.Requires(SubType == BObjectDataType.CommandEnable || SubType == BObjectDataType.CommandSelectable);
			return mDU.CommandDataSM;
		} }

		public DatabaseObjectKind CommandDataObjectKind { get {
			Contract.Requires(Type == BProtoTechEffectType.Data);
			Contract.Requires(SubType == BObjectDataType.CommandEnable || SubType == BObjectDataType.CommandSelectable);
			switch (mDU.CommandType)
			{
			case BProtoObjectCommandType.Research:		return DatabaseObjectKind.Tech;
			case BProtoObjectCommandType.TrainUnit:
			case BProtoObjectCommandType.Build:			return DatabaseObjectKind.Object;
			case BProtoObjectCommandType.TrainSquad:
			case BProtoObjectCommandType.BuildOther:	return DatabaseObjectKind.Squad;
			case BProtoObjectCommandType.Ability:		return DatabaseObjectKind.Ability;
			case BProtoObjectCommandType.Power:			return DatabaseObjectKind.Power;

			default: throw new KSoft.Debug.UnreachableException(mDU.CommandType.ToString());
			}
		} }
		#endregion
		#endregion

		public BProtoTechEffectSetAgeLevel SetAgeLevel { get { return mDU.SetAgeLevel; } }

		public Collections.BListArray<BProtoTechEffectTarget> Targets { get; private set; }
		public bool HasTargets { get { return Targets != null || Targets.Count != 0; } }

		public BProtoTechEffect()
		{
			mDU.Initialize();
		}

		#region ITagElementStreamable<string> Members
		DatabaseObjectKind TransformProtoObjectKind { get {
			return mType == BProtoTechEffectType.TransformProtoUnit ? DatabaseObjectKind.Object : DatabaseObjectKind.Squad;
		} }

		void StreamXmlTargets<TDoc, TCursor>(IO.TagElementStream<TDoc, TCursor, string> s, XML.BXmlSerializerInterface xs)
			where TDoc : class
			where TCursor : class
		{
			if (s.IsReading)
				Targets = new Collections.BListArray<BProtoTechEffectTarget>();

			XML.XmlUtil.Serialize(s, Targets, BProtoTechEffectTarget.kBListXmlParams);
		}
		void StreamXmlObjectData<TDoc, TCursor>(IO.TagElementStream<TDoc, TCursor, string> s, XML.BXmlSerializerInterface xs)
			where TDoc : class
			where TCursor : class
		{
			switch (mDU.SubType)
			{
			#region Unused
			case BObjectDataType.RateAmount:
			case BObjectDataType.RateMultiplier:
				xs.StreamTypeName(s, kXmlAttrODT_Rate, ref mDU.ID, GameDataObjectKind.Rate, false, XML.XmlUtil.kSourceAttr);
				break;
			#endregion

			case BObjectDataType.CommandEnable:
			case BObjectDataType.CommandSelectable: // Unused
				mDU.StreamCommand(s, xs);
				break;

			case BObjectDataType.Cost:
				mDU.StreamCost(s, xs);
				break;

			#region Unused
			case BObjectDataType.DamageModifier:
				mDU.StreamDamageModifier(s, xs);
				break;
			#endregion

			case BObjectDataType.PopCap:
			case BObjectDataType.PopMax:
				xs.StreamTypeName(s, kXmlAttrODT_Pop_PopType, ref mDU.ID, GameDataObjectKind.Pop, false, XML.XmlUtil.kSourceAttr);
				break;

			#region Unused
			case BObjectDataType.UnitTrainLimit:
				mDU.StreamTrainLimit(s, xs, DatabaseObjectKind.Object);
				break;
			case BObjectDataType.SquadTrainLimit:
				mDU.StreamTrainLimit(s, xs, DatabaseObjectKind.Squad);
				break;
			#endregion

			case BObjectDataType.PowerRechargeTime:
			case BObjectDataType.PowerUseLimit:
			case BObjectDataType.PowerLevel:
				xs.StreamDBID(s, kXmlAttrODT_Power_Power, ref mDU.ID, DatabaseObjectKind.Power, false, XML.XmlUtil.kSourceAttr);
				break;

			#region Ignored
 			case BObjectDataType.ImpactEffect:
 				break;
			#endregion
			#region Unused
//			case BObjectDataType.DisplayNameID:
//				break;
			#endregion
			#region Ignored
 			case BObjectDataType.TurretYawRate:
 			case BObjectDataType.TurretPitchRate:
 				break;
			#endregion

			case BObjectDataType.AbilityRecoverTime:
				xs.StreamDBID(s, kXmlAttrODT_AbilityRecoverTime_Ability, ref mDU.ID, DatabaseObjectKind.Ability, false, XML.XmlUtil.kSourceAttr);
				break;

			#region Ignored
			case BObjectDataType.HPBar:
				break;
			#endregion
			#region Unused
//			case BObjectDataType.DeathSpawn:
//				break;
			#endregion

			// assume everything else (sans ignored/unused) only uses amount
			default: //throw new KSoft.Debug.UnreachableException(mSubType.ToString());
				break;
			}
		}
		public void Serialize<TDoc, TCursor>(IO.TagElementStream<TDoc, TCursor, string> s)
			where TDoc : class
			where TCursor : class
		{
			var xs = s.GetSerializerInterface();

			s.StreamAttributeEnum(kXmlAttrType, ref mType);

			bool stream_targets = false;
			switch (mType)
			{
			case BProtoTechEffectType.Data:
				s.StreamAttributeOpt(kXmlAttrAllActions, ref mAllActions, Predicates.IsTrue);
				s.StreamStringOpt(kXmlAttrAction, ref mAction, false, intern: true);
				s.StreamAttributeEnum   (kXmlAttrSubType, ref mDU.SubType);
				// e.g., SubType==Icon and these won't be used...TODO: is Icon the only one?
				s.StreamAttributeOpt    (kXmlAttrAmount, ref mAmount, PhxPredicates.IsNotInvalidNaN);
				s.StreamAttributeEnumOpt(kXmlAttrRelativity, ref mRelativity, x => x != BObjectDataRelative.Invalid);
				StreamXmlObjectData(s, xs);
				stream_targets = true;
				break;
			case BProtoTechEffectType.TransformUnit:
			case BProtoTechEffectType.Build:
				xs.StreamDBID(s, /*xmlName:*/null, ref mDU.ToTypeID, DatabaseObjectKind.Object, false, XML.XmlUtil.kSourceCursor);
				break;
			case BProtoTechEffectType.TransformProtoUnit:
			case BProtoTechEffectType.TransformProtoSquad:
				xs.StreamDBID(s, kXmlTransformProto_AttrFromType, ref mDU.FromTypeID, TransformProtoObjectKind, false, XML.XmlUtil.kSourceAttr);
				xs.StreamDBID(s, kXmlTransformProto_AttrToType, ref mDU.ToTypeID, TransformProtoObjectKind, false, XML.XmlUtil.kSourceAttr);
				break;
			#region Unused
			case BProtoTechEffectType.SetAge:
				s.StreamCursorEnum(ref mDU.SetAgeLevel);
				break;
			#endregion
			case BProtoTechEffectType.GodPower:
				xs.StreamDBID(s, /*xmlName:*/null, ref mDU.ID, DatabaseObjectKind.Power, false, XML.XmlUtil.kSourceCursor);
				s.StreamAttribute(kXmlAttrAmount, ref mAmount);
				break;
			#region Unused
			case BProtoTechEffectType.TechStatus:
				xs.StreamDBID(s, /*xmlName:*/null, ref mDU.ID, DatabaseObjectKind.Tech, false, XML.XmlUtil.kSourceCursor);
				break;
			case BProtoTechEffectType.Ability:
				xs.StreamDBID(s, /*xmlName:*/null, ref mDU.ID, DatabaseObjectKind.Ability, false, XML.XmlUtil.kSourceCursor);
				break;
 			case BProtoTechEffectType.SharedLOS: // no extra parsed data
 				break;
			case BProtoTechEffectType.AttachSquad:
				xs.StreamDBID(s, kXmlAttachSquadAttrType, ref mDU.ID, TransformProtoObjectKind, false, XML.XmlUtil.kSourceAttr);
				stream_targets = true;
				break;
			#endregion
			}

			if (stream_targets)
				StreamXmlTargets(s, xs);
		}
		#endregion
	};
}