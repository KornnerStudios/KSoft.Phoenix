using Contracts = System.Diagnostics.Contracts;
using Contract = System.Diagnostics.Contracts.Contract;

namespace KSoft.Phoenix.Phx
{
	public sealed class BTacticData
		: Collections.BListAutoIdObject
	{
		public const string kFileExt = ".tactics";

		#region Xml constants
		public const string kXmlRoot = "TacticData";

		public static Engine.XmlFileInfo CreateFileInfo(System.IO.FileAccess mode, string filename)
		{
			return new Engine.XmlFileInfo()
			{
				Location = Engine.ContentStorage.UpdateOrGame,
				Directory = Engine.GameDirectory.Tactics,

				RootName = kXmlRoot,
				FileName = filename,

				Writable = mode == System.IO.FileAccess.Write,
			};
		}
		#endregion

		public string SourceFileName { get; set; }
		public Engine.XmlFileInfo SourceXmlFile { get; set; }

		public Collections.BListAutoId<		BWeapon> Weapons { get; private set; }
			= new Collections.BListAutoId<	BWeapon>();
		public Collections.BListAutoId<		BProtoAction> Actions { get; private set; }
			= new Collections.BListAutoId<	BProtoAction>();

		public BTactic Tactic { get; private set; }
			= new BTactic();

		public BTacticData()
		{
			InitializeDatabaseInterfaces();
		}

		#region Database interfaces
		void InitializeDatabaseInterfaces()
		{
			Weapons.SetupDatabaseInterface();
			//TacticStates.SetupDatabaseInterface();
			Actions.SetupDatabaseInterface();
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

		#region BListAutoIdObject Members
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

		public override void Serialize<TDoc, TCursor>(IO.TagElementStream<TDoc, TCursor, string> s)
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