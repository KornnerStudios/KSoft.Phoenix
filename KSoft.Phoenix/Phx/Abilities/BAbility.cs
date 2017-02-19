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

		#region ITagElementStreamable Members
		public override void Serialize<TDoc, TCursor>(IO.TagElementStream<TDoc, TCursor, string> s)
		{
			base.Serialize(s);

			s.StreamElementEnumOpt("Type", ref mType, e => e != BAbilityType.Invalid);
			//AmmoCost
			//Object
			s.StreamElementEnumOpt("SquadMode", ref mSquadMode, e => e != BSquadMode.Invalid);
			//RecoverStart BRecoverType
			//RecoverType BRecoverType
			//RecoverTime float
			//MovementSpeedModifier float
			//MovementModifierType BMovementModifierType
			s.StreamElementOpt("DamageTakenModifier", ref mDamageTakenModifier, PhxPredicates.IsNotInvalid);
			//DodgeModifier float
			//Icon
			//DisplayName2ID (unused in code)
			s.StreamElementEnumOpt("TargetType", ref mTargetType, e => e != BAbilityTargetType.Invalid);
			//RecoverAnimAttachment string, AttachmentType
			//RecoverStartAnim string, AnimType
			//RecoverEndAnim string, AnimType
			//Sprinting bool
			//DontInterruptAttack bool
			//KeepSquadMode bool
			//AttackSquadMode bool
			s.StreamElementOpt("Duration", ref mDuration, PhxPredicates.IsNotInvalid);
			//SmartTargetRange float
			//CanHeteroCommand bool
			//NoAbilityReticle bool
		}
		#endregion
	};
}