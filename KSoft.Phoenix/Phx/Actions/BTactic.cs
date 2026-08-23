using System.Collections.Generic;

namespace KSoft.Phoenix.Phx
{
	public sealed class BTactic
		: IO.ITagElementStringNameStreamable
	{
		#region Xml constants
		const string kXmlRoot = "Tactic";
		#endregion

		public Collections.BListArray<BTacticTargetRule> TargetRules { get; private set; }
		[Meta.BProtoActionReference]
		public List<BProtoActionID> PersistentActions { get; private set; }
		[Meta.BProtoActionReference]
		public List<BProtoActionID> PersistentSquadActions { get; private set; }

		public BTactic()
		{
			TargetRules = new Collections.BListArray<BTacticTargetRule>();
			PersistentActions = new List<BProtoActionID>();
			PersistentSquadActions = new List<BProtoActionID>();
		}

		#region ITagElementStreamable<string> Members
		public void Serialize<TDoc, TCursor>(IO.TagElementStream<TDoc, TCursor, string> s)
			where TDoc : class
			where TCursor : class
		{
			var userData = s.UserData;
			System.ArgumentNullException.ThrowIfNull(userData);
			var td = KSoft.Debug.TypeCheck.CastReference<BTacticData>(userData);

			XML.XmlUtil.Serialize(s, TargetRules, BTacticTargetRule.kBListXmlParams);
			s.StreamElements("PersistentAction", PersistentActions, td, BTacticData.StreamProtoActionID);
			s.StreamElements("PersistentSquadAction", PersistentSquadActions, td, BTacticData.StreamProtoActionID);
		}
		#endregion
	};
}