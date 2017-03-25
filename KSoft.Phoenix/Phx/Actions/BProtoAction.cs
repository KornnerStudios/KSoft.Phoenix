using System;

namespace KSoft.Phoenix.Phx
{
	public sealed partial class BProtoAction
		: Collections.BListAutoIdObject
	{
		public static readonly Predicate<BActionType> kNotInvalidActionType = e => e != BActionType.Invalid;
		static readonly Predicate<BSquadMode> kNotInvalidSquadMode = e => e != BSquadMode.Invalid;

		#region Xml constants
		public static readonly XML.BListXmlParams kBListXmlParams = new XML.BListXmlParams
		{
			ElementName = "Action",
			DataName = "Name",
			Flags = XML.BCollectionXmlParamsFlags.UseElementForData |
				XML.BCollectionXmlParamsFlags.ForceNoRootElementStreaming,
		};
		#endregion

		#region Properties
		BActionType mActionType = BActionType.Invalid;
		float mProjectileSpread = PhxUtil.kInvalidSingle;

		[Meta.BProtoSquadReference]
		int mSquadTypeID = TypeExtensions.kNone;
		[Meta.BWeaponTypeReference]
		int mWeaponID = TypeExtensions.kNone;
		[Meta.BProtoActionReference]
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

			s.StreamElementEnumOpt("ActionType", ref mActionType, kNotInvalidActionType);
			s.StreamElementOpt("ProjectileSpread", ref mProjectileSpread, PhxPredicates.IsNotInvalid);

			xs.StreamDBID(s, "SquadType", ref mSquadTypeID, DatabaseObjectKind.Squad);
			td.StreamID(s, "Weapon", ref mWeaponID, TacticDataObjectKind.Weapon);
			td.StreamID(s, "LinkedAction", ref mLinkedActionID, TacticDataObjectKind.Action);

			s.StreamElementEnumOpt("SquadMode", ref mSquadMode, kNotInvalidSquadMode);
			s.StreamElementEnumOpt("NewSquadMode", ref mNewSquadMode, kNotInvalidSquadMode);
#if false
			td.StreamID(s, "NewTacticState", ref mNewTacticStateID, BTacticData.ObjectKind.TacticState);
#endif

			#region Work
			s.StreamElementOpt("WorkRate", ref mWorkRate, PhxPredicates.IsNotInvalid);
			s.StreamElementOpt("WorkRateVariance", ref mWorkRateVariance, PhxPredicates.IsNotInvalid);
			s.StreamElementOpt("WorkRange", ref mWorkRange, PhxPredicates.IsNotInvalid);
			#endregion

			#region DamageModifiers
			using (var bm = s.EnterCursorBookmarkOpt("DamageModifiers", this, o => mDamageModifiersDmg != PhxUtil.kInvalidSingle)) if (bm.IsNotNull)
			{
				s.StreamAttribute("damage", ref mDamageModifiersDmg);
				s.StreamAttributeOpt("damageTaken", ref mDamageModifiersDmgTaken, PhxPredicates.IsNotInvalid);
				s.StreamAttributeOpt("byCombatValue", ref mDamageModifiersByCombatValue, Predicates.IsTrue);
			}
			#endregion

			xs.StreamTypeName(s, "Resource", ref mResourceID, GameDataObjectKind.Cost);
			// if element equals 'true' this is the default action
			s.StreamElementOpt("Default", ref mDefault, Predicates.IsTrue);

			td.StreamID(s, "SlaveAttackAction", ref mSlaveAttackActionID, TacticDataObjectKind.Action);
			td.StreamID(s, "BaseDPSWeapon", ref mBaseDPSWeaponID, TacticDataObjectKind.Weapon);

			s.StreamElementEnumOpt("PersistentActionType", ref mPersistentActionType, kNotInvalidActionType);

			#region Duration
			using (var bm = s.EnterCursorBookmarkOpt("Duration", this, o => mDuration != PhxUtil.kInvalidSingle)) if (bm.IsNotNull)
			{
				s.StreamCursor(ref mDuration);
				s.StreamAttributeOpt("DurationSpread", ref mDurationSpread, PhxPredicates.IsNotInvalid);
			}
			#endregion

			#region AutoRepair
			using (var bm = s.EnterCursorBookmarkOpt("AutoRepair", this, o => mAutoRepairIdleTime != PhxUtil.kInvalidSingle)) if (bm.IsNotNull)
			{
				s.StreamAttribute("AutoRepairIdleTime", ref mAutoRepairIdleTime);
				s.StreamAttribute("AutoRepairThreshold", ref mAutoRepairThreshold);
				s.StreamAttribute("AutoRepairSearchDistance", ref mAutoRepairSearchDistance);
			}
			#endregion
			xs.StreamDBID(s, "InvalidTarget", ref mInvalidTargetObjectID, DatabaseObjectKind.Object);

			#region ProtoObject
			using (var bm = s.EnterCursorBookmarkOpt("ProtoObject", this, o => mProtoObjectID.IsNotNone())) if (bm.IsNotNull)
			{
				// TODO: This IS optional, right? Only on 'true'?
				// inner text: if 0, proto object, if not, proto squad
				s.StreamAttributeOpt("Squad", ref mProtoObjectIsSquad, Predicates.IsTrue);
				xs.StreamDBID(s, null, ref mSquadTypeID,
					mProtoObjectIsSquad ? DatabaseObjectKind.Squad : DatabaseObjectKind.Object, false, XML.XmlUtil.kSourceCursor);
			}
			#endregion
#if false
			xs.StreamXmlForStringID(s, "Count", ref mCountStringID);
#endif
			s.StreamElementOpt("MaxNumUnitsPerformAction", ref mMaxNumUnitsPerformAction, Predicates.IsNotNone);
			s.StreamElementOpt("DamageCharge", ref mDamageCharge, PhxPredicates.IsNotInvalid);
		}
		#endregion
	};
}