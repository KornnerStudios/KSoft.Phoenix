using System;
namespace KSoft.Phoenix.Phx
{
	[ProtoDataTypeObjectSourceKind(ProtoDataObjectSourceKind.TacticData)]
	public sealed class BTacticData
		: Collections.BListAutoIdObject
		, IProtoDataObjectDatabaseProvider
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

		public string? SourceFileName { get; set; }
		public Engine.XmlFileInfo? SourceXmlFile { get; set; }
		public bool SourceXmlFileIsXmb { get; set; }

		public Collections.BListAutoId<BWeapon> Weapons { get; private set; } = new();
		public Collections.BListAutoId<BProtoAction> Actions { get; private set; } = new();

		public BTactic Tactic { get; private set; } = new();

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

		internal Collections.IBTypeNames GetNamesInterface(TacticDataObjectKind kind)
		{
			if (kind == TacticDataObjectKind.None)
			{
				throw new ArgumentOutOfRangeException(nameof(kind));
			}

			return kind switch
			{
				TacticDataObjectKind.Weapon => Weapons,
				//TacticDataObjectKind.TacticState => TacticStates,
				TacticDataObjectKind.Action => Actions,
				_ => throw new KSoft.Debug.UnreachableException(kind.ToString()),
			};
		}

		internal Collections.IHasUndefinedProtoMemberInterface GetMembersInterface(TacticDataObjectKind kind)
		{
			if (kind == TacticDataObjectKind.None)
			{
				throw new ArgumentOutOfRangeException(nameof(kind));
			}

			return kind switch
			{
				TacticDataObjectKind.Weapon => Weapons,
				//TacticDataObjectKind.TacticState => TacticStates,
				TacticDataObjectKind.Action => Actions,
				_ => throw new KSoft.Debug.UnreachableException(kind.ToString()),
			};
		}
		#endregion

		#region BListAutoIdObject Members
		internal bool StreamID<TDoc, TCursor>(IO.TagElementStream<TDoc, TCursor, string> s, string xmlName, ref int dbid,
			TacticDataObjectKind kind,
			bool isOptional = true, IO.TagElementNodeType xmlSource = XML.XmlUtil.kSourceElement)
			where TDoc : class
			where TCursor : class
		{
			XML.XmlUtil.ValidateXmlSourceName(xmlName, xmlSource);
			if (kind == TacticDataObjectKind.None)
			{
				throw new ArgumentOutOfRangeException(nameof(kind));
			}

			string? id_name = null;
			bool was_streamed = true;
			bool to_lower = false;

			if (s.IsReading)
			{
				if (isOptional)
				{
					was_streamed = s.StreamStringOpt(xmlName, ref id_name, to_lower, xmlSource, intern: true);
				}
				else
				{
					string required_id_name = id_name!;
					s.StreamString(xmlName, ref required_id_name, to_lower, xmlSource, intern: true);
					id_name = required_id_name;
				}

				if (was_streamed)
				{
					IProtoDataObjectDatabaseProvider provider = this;
					dbid = provider.GetId((int)kind, id_name!);
					if (dbid.IsNone())
					{
						s.ThrowReadException(new System.IO.InvalidDataException(string.Create(KSoft.Util.InvariantCultureInfo,
							$"Failed to resolve tactic {kind} reference '{id_name}' from {xmlName ?? "ElementText"}.")));
					}
				}
				else
				{
					dbid = TypeExtensions.kNone;
				}
			}
			else if (s.IsWriting && dbid.IsNotNone())
			{
				IProtoDataObjectDatabaseProvider provider = this;
				id_name = provider.GetName((int)kind, dbid);
				if (string.IsNullOrEmpty(id_name))
				{
					throw new InvalidOperationException(string.Create(KSoft.Util.InvariantCultureInfo,
						$"Failed to resolve tactic {kind} reference name for id {dbid}."));
				}

				if (isOptional)
				{
					s.StreamStringOpt(xmlName, ref id_name, to_lower, xmlSource, intern: true);
				}
				else
				{
					s.StreamString(xmlName, ref id_name, to_lower, xmlSource, intern: true);
				}
			}

			return was_streamed;
		}
		internal static void StreamWeaponID<TDoc, TCursor>(IO.TagElementStream<TDoc, TCursor, string> s, BTacticData td,
			ref int id)
			where TDoc : class
			where TCursor : class
		{
			td.StreamID(s, XML.XmlUtil.kNoXmlName!, ref id, TacticDataObjectKind.Weapon, false, XML.XmlUtil.kSourceCursor);
		}
		internal static void StreamProtoActionID<TDoc, TCursor>(IO.TagElementStream<TDoc, TCursor, string> s, BTacticData td,
			ref int protoActionId)
			where TDoc : class
			where TCursor : class
		{
			td.StreamID(s, XML.XmlUtil.kNoXmlName!, ref protoActionId, TacticDataObjectKind.Action, false, XML.XmlUtil.kSourceCursor);
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

		#region IProtoDataObjectDatabaseProvider members
		Engine.XmlFileInfo IProtoDataObjectDatabaseProvider.SourceFileReference => SourceXmlFile!;

		Collections.IBTypeNames IProtoDataObjectDatabaseProvider.GetNamesInterface(int objectKind)
		{
			var kind = (TacticDataObjectKind)objectKind;
			return GetNamesInterface(kind);
		}

		Collections.IHasUndefinedProtoMemberInterface IProtoDataObjectDatabaseProvider.GetMembersInterface(int objectKind)
		{
			var kind = (TacticDataObjectKind)objectKind;
			return GetMembersInterface(kind);
		}
		#endregion
	};
}