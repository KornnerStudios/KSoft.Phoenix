using System.Collections.Generic;

using BDamageTypeID = System.Int32;
using BProtoUnitID = System.Int32; // object type or proto unit

namespace KSoft.Phoenix.Phx
{
	public sealed class BTacticTargetRule
		: IO.ITagElementStringNameStreamable
	{
		#region Xml constants
		public static readonly XML.BListXmlParams kBListXmlParams = new XML.BListXmlParams
		{
			ElementName = "TargetRule",
			Flags = XML.BCollectionXmlParamsFlags.ForceNoRootElementStreaming
		};

		const string kXmlElementRelation = "Relation"; // BDiplomacy
		const string kXmlElementSquadMode = "SquadMode"; // BSquadMode
		const string kXmlElementAutoTargetSquadMode = "AutoTargetSquadMode"; // BSquadMode

		const string kXmlElementAction = "Action"; // BProtoAction
		const string kXmlElementTargetState = "TargetState"; // BTacticTargetRuleTargetState
		const string kXmlElementAbility = "Ability"; // BAbility
		const string kXmlElementOptionalAbility = "OptionalAbility"; // BAbility
		#endregion

		BDiplomacy mRelation = BDiplomacy.Invalid;

		BSquadMode mSquadMode = BSquadMode.Invalid,
			mAutoTargetSquadMode = BSquadMode.Invalid;

		public List<BDamageTypeID> DamageTypes { get; private set; }
		public List<BProtoUnitID> TargetTypes { get; private set; }

		int mProtoActionID = TypeExtensions.kNone;
		//BTacticTargetRuleTargetState mTargetStates;

		int mAbilityID = TypeExtensions.kNone;
		public bool IsOptionalAbility { get; private set; }

		public BTacticTargetRule()
		{
			DamageTypes = new List<BDamageTypeID>();
			TargetTypes = new List<BProtoUnitID>();
		}

		#region ITagElementStreamable<string> Members
		public void Serialize<TDoc, TCursor>(IO.TagElementStream<TDoc, TCursor, string> s)
			where TDoc : class
			where TCursor : class
		{
			var xs = s.GetSerializerInterface();
			var td = KSoft.Debug.TypeCheck.CastReference<BTacticData>(s.UserData);

			s.StreamElementEnumOpt(kXmlElementRelation, ref mRelation, e => e != BDiplomacy.Invalid);
			if(!s.StreamElementEnumOpt(kXmlElementSquadMode, ref mSquadMode, e => e != BSquadMode.Invalid))
				s.StreamElementEnumOpt(kXmlElementAutoTargetSquadMode, ref mAutoTargetSquadMode, e => e != BSquadMode.Invalid);

			s.StreamElements("DamageType", DamageTypes, xs, XML.BDatabaseXmlSerializerBase.StreamDamageType);
			s.StreamElements("TargetType", TargetTypes, xs, XML.BDatabaseXmlSerializerBase.StreamUnitID);

			td.StreamID(s, kXmlElementAction, ref mProtoActionID, TacticDataObjectKind.Action);

			if (!xs.StreamDBID(s, kXmlElementAbility, ref mAbilityID, DatabaseObjectKind.Ability))
				IsOptionalAbility = xs.StreamDBID(s, kXmlElementOptionalAbility, ref mAbilityID, DatabaseObjectKind.Ability);
		}
		#endregion
	};
}