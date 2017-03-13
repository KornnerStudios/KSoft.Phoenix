using System.Collections.Generic;
using Contracts = System.Diagnostics.Contracts;
using Contract = System.Diagnostics.Contracts.Contract;

namespace KSoft.Phoenix.Phx
{
	public sealed class BTacticData
		: IO.ITagElementStringNameStreamable
	{
		public const string kFileExt = ".tactics";

		#region Xml constants
		public const string kXmlRoot = "TacticData";
		#endregion

		public string Name { get; private set; }

		public Collections.BListAutoId<BWeapon> Weapons { get; private set; }
		public Collections.BListAutoId<BProtoAction> Actions { get; private set; }

		public BTactic Tactic { get; private set; }

		public BTacticData(string name)
		{
			Contract.Requires(!string.IsNullOrEmpty(name));

			Name = name;

			Weapons = new Collections.BListAutoId<BWeapon>();
			Actions = new Collections.BListAutoId<BProtoAction>();

			Tactic = new BTactic();

			InitializeDatabaseInterfaces();
		}

		#region Database interfaces
		Dictionary<string, BWeapon> mDbiWeapons;
		//Dictionary<string, BTacticState> mDbiTacticStates;
		Dictionary<string, BProtoAction> mDbiActions;

		void InitializeDatabaseInterfaces()
		{
			Weapons.SetupDatabaseInterface(out mDbiWeapons);
			//TacticStates.SetupDatabaseInterface(out mDbiTacticStates);
			Actions.SetupDatabaseInterface(out mDbiActions);
		}

		public int GetId(TacticDataObjectKind kind, string name)
		{
			switch (kind)
			{
			case TacticDataObjectKind.Weapon:		return Weapons.TryGetIdWithUndefined(name);
			//case TacticDataObjectKind.TacticState:	return TacticStates.TryGetIdWithUndefined(name);
			case TacticDataObjectKind.Action:		return Actions.TryGetIdWithUndefined(name);

			default: throw new KSoft.Debug.UnreachableException(kind.ToString());
			}
		}
		public string GetName(TacticDataObjectKind kind, int id)
		{
			switch (kind)
			{
			case TacticDataObjectKind.Weapon:		return Weapons.TryGetNameWithUndefined(id);
			//case TacticDataObjectKind.TacticState:	return TacticStates.TryGetNameWithUndefined(id);
			case TacticDataObjectKind.Action:		return Actions.TryGetNameWithUndefined(id);

			default: throw new KSoft.Debug.UnreachableException(kind.ToString());
			}
		}
		#endregion

		#region ITagElementStreamable<string> Members
		internal bool StreamID<TDoc, TCursor>(IO.TagElementStream<TDoc, TCursor, string> s, string xmlName, ref int dbid,
			TacticDataObjectKind kind,
			bool isOptional = true, IO.TagElementNodeType xmlSource = XML.XmlUtil.kSourceElement)
			where TDoc : class
			where TCursor : class
		{
			Contract.Requires(xmlSource.RequiresName() == (xmlName != XML.XmlUtil.kNoXmlName));

			string id_name = null;
			bool was_streamed = true;
			bool to_lower = false;

			if (s.IsReading)
			{
				if (isOptional)
					was_streamed = s.StreamStringOpt(xmlName, ref id_name, to_lower, xmlSource, intern: true);
				else
					s.StreamString(xmlName, ref id_name, to_lower, xmlSource, intern: true);

				if (was_streamed)
				{
					dbid = GetId(kind, id_name);
					Contract.Assert(dbid.IsNotNone());
				}
				else
					dbid = TypeExtensions.kNone;
			}
			else if (s.IsWriting && dbid.IsNotNone())
			{
				id_name = GetName(kind, dbid);
				Contract.Assert(!string.IsNullOrEmpty(id_name));

				if (isOptional)
					s.StreamStringOpt(xmlName, ref id_name, to_lower, xmlSource, intern: true);
				else
					s.StreamString(xmlName, ref id_name, to_lower, xmlSource, intern: true);
			}

			return was_streamed;
		}
		internal static void StreamWeaponID<TDoc, TCursor>(IO.TagElementStream<TDoc, TCursor, string> s, BTacticData td,
			ref int id)
			where TDoc : class
			where TCursor : class
		{
			td.StreamID(s, XML.XmlUtil.kNoXmlName, ref id, TacticDataObjectKind.Weapon, false, XML.XmlUtil.kSourceCursor);
		}
		internal static void StreamProtoActionID<TDoc, TCursor>(IO.TagElementStream<TDoc, TCursor, string> s, BTacticData td,
			ref int protoActionId)
			where TDoc : class
			where TCursor : class
		{
			td.StreamID(s, XML.XmlUtil.kNoXmlName, ref protoActionId, TacticDataObjectKind.Action, false, XML.XmlUtil.kSourceCursor);
		}

		public void Serialize<TDoc, TCursor>(IO.TagElementStream<TDoc, TCursor, string> s)
			where TDoc : class
			where TCursor : class
		{
			using (s.EnterUserDataBookmark(this))
			{
				XML.XmlUtil.Serialize(s, Weapons, BWeapon.kBListXmlParams);
				//XML.XmlUtil.Serialize(s, TacticStates, BTacticState.kBListXmlParams);
				XML.XmlUtil.Serialize(s, Actions, BProtoAction.kBListXmlParams);
				Tactic.Serialize(s);
			}
		}
		#endregion
	};
}