using System;

namespace KSoft.Phoenix.Phx
{
	public sealed class BProtoAction
		: Collections.BListAutoIdObject
	{
		enum BJoinType
		{
			Follow,
			Merge,
			Board,
			FollowAttack,
		};
		enum BMergeType
		{
			None,

			Ground,
			Air,
		};

		static readonly Predicate<BActionType> kNotInvalidActionType = e => e != BActionType.Invalid;
		static readonly Predicate<BSquadMode> kNotInvalidSquadMode = e => e != BSquadMode.Invalid;

		#region Xml constants
		public static readonly XML.BListXmlParams kBListXmlParams = new XML.BListXmlParams
		{
			ElementName = "Action",
			DataName = "Name",
			Flags = XML.BCollectionXmlParamsFlags.UseElementForData |
				XML.BCollectionXmlParamsFlags.ForceNoRootElementStreaming,
		};

		const string kXmlElementActionType = "ActionType";
		const string kXmlElementProjectileSpread = "ProjectileSpread"; // float

		const string kXmlElementSquadType = "SquadType"; // proto squad
		const string kXmlElementWeapon = "Weapon"; // BWeapon
		const string kXmlElementLinkedAction = "LinkedAction"; // BProtoAction

		const string kXmlElementSquadMode = "SquadMode"; // BSquadMode
		const string kXmlElementNewSquadMode = "NewSquadMode"; // BSquadMode
		const string kXmlElementNewTacticState = "NewTacticState"; // BTacticState?

		const string kXmlElementWorkRate = "WorkRate"; // float
		const string kXmlElementWorkRateVariance = "WorkRateVariance"; // float
		const string kXmlElementWorkRange = "WorkRange"; // float

		const string kXmlElementDamageModifiers = "DamageModifiers";
		const string kXmlElementDamageModifiersAttrDamage = "damage"; // float
		const string kXmlElementDamageModifiersAttrDamageTaken = "damageTaken"; // float
		const string kXmlElementDamageModifiersAttrByCombatValue = "byCombatValue"; // bool

		const string kXmlElementResource = "Resource"; // ResourceID
		const string kXmlElementDefault = "Default"; // bool, if element equals 'true' this is the default action

		const string kXmlElementSlaveAttackAction = "SlaveAttackAction"; // BProtoAction
		const string kXmlElementBaseDPSWeapon = "BaseDPSWeapon"; // BWeapon

		const string kXmlElementPersistentActionType = "PersistentActionType";

		const string kXmlElementDuration = "Duration"; // float
		const string kXmlElementDurationAttrSpread = "DurationSpread"; // float

		const string kXmlElementAutoRepair = "AutoRepair";
		const string kXmlElementAutoRepairAttrIdleTime = "AutoRepairIdleTime"; // int
		const string kXmlElementAutoRepairAttrThreshold = "AutoRepairThreshold"; // float
		const string kXmlElementAutoRepairAttrSearchDistance = "AutoRepairSearchDistance"; // float
		const string kXmlElementInvalidTarget = "InvalidTarget"; // proto object

		const string kXmlElementProtoObject = "ProtoObject";
		const string kXmlElementProtoObjectAttrSquad = "Squad"; // inner text: if 0, proto object, if not, proto squad
		const string kXmlElementCount = "Count"; // StringID
		const string kXmlElementMaxNumUnitsPerformAction = "MaxNumUnitsPerformAction"; // int
		const string kXmlElementDamageCharge = "DamageCharge"; // float
		#endregion

		#region Properties
		BActionType mActionType;
		float mProjectileSpread = PhxUtil.kInvalidSingle;

		int mSquadTypeID = TypeExtensions.kNone;
		int mWeaponID = TypeExtensions.kNone;
		int mLinkedActionID = TypeExtensions.kNone;

		BSquadMode mSquadMode = BSquadMode.Invalid;
		BSquadMode mNewSquadMode = BSquadMode.Invalid;
#if false
		int mNewTacticStateID = TypeExtensions.kNone;
#endif

		float mWorkRate = PhxUtil.kInvalidSingle;
		float mWorkRateVariance = PhxUtil.kInvalidSingle;
		float mWorkRange = PhxUtil.kInvalidSingle;

		float mDamageModifiersDmg;
		float mDamageModifiersDmgTaken;
		bool mDamageModifiersByCombatValue;

		int mResourceID = TypeExtensions.kNone;
		bool mDefault;

		int mSlaveAttackActionID = TypeExtensions.kNone;
		int mBaseDPSWeaponID = TypeExtensions.kNone;

		BActionType mPersistentActionType = BActionType.Invalid;

		float mDuration = PhxUtil.kInvalidSingle;
		float mDurationSpread = PhxUtil.kInvalidSingle;

		int mAutoRepairIdleTime = TypeExtensions.kNone;
		float mAutoRepairThreshold = PhxUtil.kInvalidSingle;
		float mAutoRepairSearchDistance = PhxUtil.kInvalidSingle;
		int mInvalidTargetObjectID = TypeExtensions.kNone;

		int mProtoObjectID = TypeExtensions.kNone;
		bool mProtoObjectIsSquad;
#if false
		int mCountStringID = TypeExtensions.kNone;
#endif
		int mMaxNumUnitsPerformAction = TypeExtensions.kNone;
		float mDamageCharge = PhxUtil.kInvalidSingle;
		#endregion

		#region BListAutoIdObject Members
		public override void Serialize<TDoc, TCursor>(IO.TagElementStream<TDoc, TCursor, string> s)
		{
			var xs = s.GetSerializerInterface();
			var td = KSoft.Debug.TypeCheck.CastReference<BTacticData>(s.UserData);

			s.StreamElementEnumOpt(kXmlElementActionType, ref mActionType, kNotInvalidActionType);
			s.StreamElementOpt(kXmlElementProjectileSpread, ref mProjectileSpread, PhxPredicates.IsNotInvalid);

			xs.StreamDBID(s, kXmlElementSquadType, ref mSquadTypeID, DatabaseObjectKind.Squad);
			td.StreamID(s, kXmlElementWeapon, ref mWeaponID, TacticDataObjectKind.Weapon);
			td.StreamID(s, kXmlElementLinkedAction, ref mLinkedActionID, TacticDataObjectKind.Action);

			s.StreamElementEnumOpt(kXmlElementSquadMode, ref mSquadMode, kNotInvalidSquadMode);
			s.StreamElementEnumOpt(kXmlElementNewSquadMode, ref mNewSquadMode, kNotInvalidSquadMode);
#if false
			td.StreamID(s, kXmlElementNewTacticState, ref mNewTacticStateID, BTacticData.ObjectKind.TacticState);
#endif

			#region Work
			s.StreamElementOpt(kXmlElementWorkRate, ref mWorkRate, PhxPredicates.IsNotInvalid);
			s.StreamElementOpt(kXmlElementWorkRateVariance, ref mWorkRateVariance, PhxPredicates.IsNotInvalid);
			s.StreamElementOpt(kXmlElementWorkRange, ref mWorkRange, PhxPredicates.IsNotInvalid);
			#endregion

			#region DamageModifiers
			using (var bm = s.EnterCursorBookmarkOpt(kXmlElementDamageModifiers, this, o => mDamageModifiersDmg != PhxUtil.kInvalidSingle)) if (bm.IsNotNull)
			{
				s.StreamAttribute(kXmlElementDamageModifiersAttrDamage, ref mDamageModifiersDmg);
				s.StreamAttributeOpt(kXmlElementDamageModifiersAttrDamageTaken, ref mDamageModifiersDmgTaken, PhxPredicates.IsNotInvalid);
				s.StreamAttributeOpt(kXmlElementDamageModifiersAttrByCombatValue, ref mDamageModifiersByCombatValue, Predicates.IsTrue);
			}
			#endregion

			xs.StreamTypeName(s, kXmlElementResource, ref mResourceID, GameDataObjectKind.Cost);
			s.StreamElementOpt(kXmlElementDefault, ref mDefault, Predicates.IsTrue);

			td.StreamID(s, kXmlElementSlaveAttackAction, ref mSlaveAttackActionID, TacticDataObjectKind.Action);
			td.StreamID(s, kXmlElementBaseDPSWeapon, ref mBaseDPSWeaponID, TacticDataObjectKind.Weapon);

			s.StreamElementEnumOpt(kXmlElementPersistentActionType, ref mPersistentActionType, kNotInvalidActionType);

			#region Duration
			using (var bm = s.EnterCursorBookmarkOpt(kXmlElementDuration, this, o => mDuration != PhxUtil.kInvalidSingle)) if (bm.IsNotNull)
			{
				s.StreamCursor(ref mDuration);
				s.StreamAttributeOpt(kXmlElementDurationAttrSpread, ref mDurationSpread, PhxPredicates.IsNotInvalid);
			}
			#endregion

			#region AutoRepair
			using (var bm = s.EnterCursorBookmarkOpt(kXmlElementAutoRepair, this, o => mAutoRepairIdleTime != PhxUtil.kInvalidSingle)) if (bm.IsNotNull)
			{
				s.StreamAttribute(kXmlElementAutoRepairAttrIdleTime, ref mAutoRepairIdleTime);
				s.StreamAttribute(kXmlElementAutoRepairAttrThreshold, ref mAutoRepairThreshold);
				s.StreamAttribute(kXmlElementAutoRepairAttrSearchDistance, ref mAutoRepairSearchDistance);
			}
			#endregion
			xs.StreamDBID(s, kXmlElementInvalidTarget, ref mInvalidTargetObjectID, DatabaseObjectKind.Object);

			#region ProtoObject
			using (var bm = s.EnterCursorBookmarkOpt(kXmlElementProtoObject, this, o => mProtoObjectID.IsNotNone())) if (bm.IsNotNull)
			{
				// TODO: This IS optional, right? Only on 'true'?
				s.StreamAttributeOpt(kXmlElementProtoObjectAttrSquad, ref mProtoObjectIsSquad, Predicates.IsTrue);
				xs.StreamDBID(s, null, ref mSquadTypeID, 
					mProtoObjectIsSquad ? DatabaseObjectKind.Squad : DatabaseObjectKind.Object, false, XML.XmlUtil.kSourceCursor);
			}
			#endregion
#if false
			xs.StreamXmlForStringID(s, kXmlElementCount, ref mCountStringID);
#endif
			s.StreamElementOpt(kXmlElementMaxNumUnitsPerformAction, ref mMaxNumUnitsPerformAction, Predicates.IsNotNone);
			s.StreamElementOpt(kXmlElementDamageCharge, ref mDamageCharge, PhxPredicates.IsNotInvalid);
		}
		#endregion
	};
}