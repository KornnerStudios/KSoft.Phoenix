using System;

namespace KSoft.Phoenix.Phx
{
	/// <remarks>
	/// * Has no PrereqTextID property
	/// </remarks>
	public sealed class BAbility
		: DatabaseNamedObject
	{
		#region Xml constants
		public static readonly XML.BListXmlParams kBListXmlParams = new XML.BListXmlParams
		{
			RootName = "Abilities",
			ElementName = "Ability",
			DataName = DatabaseNamedObject.kXmlAttrNameN,
		};
		public static readonly Engine.XmlFileInfo kXmlFileInfo = new Engine.XmlFileInfo
		{
			Directory = Engine.GameDirectory.Data,
			FileName = "Abilities.xml",
			RootName = kBListXmlParams.RootName
		};

		const string kXmlElementType = "Type";
		//const string kXmlElementAmmoCost = "AmmoCost";
		//const string kXmlElementObject = "Object";
		const string kXmlElementSquadMode = "SquadMode";
		//const string kXmlElementRecoverStart = "RecoverStart"; // BRecoverType
		//const string kXmlElementRecoverType = "RecoverType"; // BRecoverType
		//const string kXmlElementRecoverTime = "RecoverTime"; // float
		//const string kXmlElementMovementSpeedModifier = "MovementSpeedModifier"; // float
		//const string kXmlElementMovementModifierType = "MovementModifierType"; // BMovementModifierType
		const string kXmlElementDamageTakenModifier = "DamageTakenModifier";
		//const string kXmlElementDodgeModifier = "DodgeModifier"; // float
		//const string kXmlElementIcon = "Icon";
		//const string kXmlElementDisplayName2ID = "DisplayName2ID";
		const string kXmlElementTargetType = "TargetType";
		//const string kXmlElementRecoverAnimAttachment = "RecoverAnimAttachment"; // string, AttachmentType
		//const string kXmlElementRecoverStartAnim = "RecoverStartAnim"; // string
		//const string kXmlElementRecoverEndAnim = "RecoverEndAnim"; // string
		//const string kXmlElementSprinting = "Sprinting"; // bool
		//const string kXmlElementDontInterruptAttack = "DontInterruptAttack"; // bool
		//const string kXmlElementKeepSquadMode = "KeepSquadMode"; // bool
		//const string kXmlElementAttackSquadMode = "AttackSquadMode"; // bool
		const string kXmlElementDuration = "Duration";
		//const string kXmlElementSmartTargetRange = "SmartTargetRange"; // float
		//const string kXmlElementCanHeteroCommand = "CanHeteroCommand"; // bool
		//const string kXmlElementNoAbilityReticle = "NoAbilityReticle"; // bool
		#endregion

		BAbilityType mType = BAbilityType.Invalid;
		public BAbilityType Type { get { return mType; } }

		BSquadMode mSquadMode = BSquadMode.Invalid;
		public BSquadMode SquadMode { get { return mSquadMode; } }

		float mDamageTakenModifier = PhxUtil.kInvalidSingle;
		public float DamageTakenModifier { get { return mDamageTakenModifier; } }

		BAbilityTargetType mTargetType = BAbilityTargetType.Invalid;
		public BAbilityTargetType TargetType { get { return mTargetType; } }

		float mDuration = PhxUtil.kInvalidSingle;
		public float Duration { get { return mDuration; } }

		#region ITagElementStreamable<string> Members
		public override void Serialize<TDoc, TCursor>(IO.TagElementStream<TDoc, TCursor, string> s)
		{
			base.Serialize(s);

			s.StreamElementEnumOpt(kXmlElementType, ref mType, e => e != BAbilityType.Invalid);
			s.StreamElementEnumOpt(kXmlElementSquadMode, ref mSquadMode, e => e != BSquadMode.Invalid);
			s.StreamElementOpt    (kXmlElementDamageTakenModifier, ref mDamageTakenModifier, PhxPredicates.IsNotInvalid);
			s.StreamElementEnumOpt(kXmlElementTargetType, ref mTargetType, e => e != BAbilityTargetType.Invalid);
			s.StreamElementOpt    (kXmlElementDuration, ref mDuration, PhxPredicates.IsNotInvalid);
		}
		#endregion
	};
}