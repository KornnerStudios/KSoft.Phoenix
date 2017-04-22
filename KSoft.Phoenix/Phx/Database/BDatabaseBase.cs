﻿using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Contracts = System.Diagnostics.Contracts;
using Contract = System.Diagnostics.Contracts.Contract;

using FA = System.IO.FileAccess;

namespace KSoft.Phoenix.Phx
{
	public abstract partial class BDatabaseBase
		: IDisposable
		, IO.ITagElementStringNameStreamable
	{
		public const string kInvalidString = "BORK BORK BORK";

		#region Xml constants
		internal static readonly XML.BListXmlParams kObjectTypesXmlParams = new XML.BListXmlParams("ObjectType");
		internal static readonly Engine.XmlFileInfo kObjectTypesXmlFileInfo = new Engine.XmlFileInfo
		{
			Directory = Phoenix.Engine.GameDirectory.Data,
			FileName = "ObjectTypes.xml",
			RootName = kObjectTypesXmlParams.RootName
		};
		internal static readonly Engine.ProtoDataXmlFileInfo kObjectTypesProtoFileInfo = new Phoenix.Engine.ProtoDataXmlFileInfo(
			Phoenix.Engine.XmlFilePriority.Lists,
			kObjectTypesXmlFileInfo);
		#endregion

		public Engine.PhxEngine Engine { get; private set; }

		public abstract Collections.IProtoEnum GameObjectTypes { get; }
		public abstract Collections.IProtoEnum GameProtoObjectTypes { get; }
		public abstract Collections.IProtoEnum GameScenarioWorlds { get; }

		Dictionary<int, string> mStringTable = new Dictionary<int, string>();
		/// <summary>StringID values indexed by their official locID</summary>
		/// <remarks>Only populated with strings from a StringTable which Phx-based objects reference, to cut down on memory usage</remarks>
		public IReadOnlyDictionary<int, string> StringTable { get { return mStringTable; } }

		public BGameData GameData { get; private set; }
			 = new BGameData();
		public HPBarData HPBars { get; private set; }
			 = new HPBarData();
		// #NOTE place new DatabaseObjectKind code here
		public Collections.BListAutoId<		BDamageType> DamageTypes { get; private set; }
			= new Collections.BListAutoId<	BDamageType>();
		public Collections.BListAutoId<		BProtoImpactEffect> ImpactEffects { get; private set; }
			= new Collections.BListAutoId<	BProtoImpactEffect>();
		public Collections.BListAutoId<		BWeaponType> WeaponTypes { get; private set; }
			= new Collections.BListAutoId<	BWeaponType>();
		public Collections.BListAutoId<		BUserClass> UserClasses { get; private set; }
			= new Collections.BListAutoId<	BUserClass>();
		public Collections.BTypeNamesWithCode ObjectTypes { get; private set; }
		public Collections.BListAutoId<		BAbility> Abilities { get; private set; }
			 = new Collections.BListAutoId<	BAbility>();
		public Collections.BListAutoId<		BProtoObject> Objects { get; private set; }
			= new Collections.BListAutoId<	BProtoObject>(BProtoObject.kBListParams);
		public Collections.BListAutoId<		BProtoSquad> Squads { get; private set; }
			= new Collections.BListAutoId<	BProtoSquad>(BProtoSquad.kBListParams);
		public Collections.BListAutoId<		BProtoPower> Powers { get; private set; }
			= new Collections.BListAutoId<	BProtoPower>();
		public Collections.BListAutoId<		BTacticData> Tactics { get; private set; }
			= new Collections.BListAutoId<	BTacticData>();
		public Collections.BListAutoId<		BProtoTech> Techs { get; private set; }
			= new Collections.BListAutoId<	BProtoTech>(BProtoTech.kBListParams);
		public Collections.BListAutoId<		TerrainTileType> TerrainTileTypes { get; private set; }
			= new Collections.BListAutoId<	TerrainTileType>();
		public Collections.BListAutoId<		BCiv> Civs { get; private set; }
			= new Collections.BListAutoId<	BCiv>();
		public Collections.BListAutoId<		BLeader> Leaders { get; private set; }
			= new Collections.BListAutoId<	BLeader>();

		public Collections.BListArray<		BProtoMergedSquads> MergedSquads { get; private set; }
			= new Collections.BListArray<	BProtoMergedSquads>();
		public BProtoShieldBubbleTypes ShieldBubbleTypes { get; private set; }
			= new BProtoShieldBubbleTypes();

		protected BDatabaseBase(Engine.PhxEngine engine, Collections.IProtoEnum gameObjectTypes)
		{
			Engine = engine;

			ObjectTypes = new Collections.BTypeNamesWithCode(gameObjectTypes);

			InitializeDatabaseInterfaces();
		}

		#region IDisposable Members
		public virtual void Dispose()
		{
			Util.DisposeAndNull(ref mTriggerSerializer);
		}
		#endregion

		#region StringTable Util
		internal void AddStringIDReference(int index)
		{
			mStringTable[index] = kInvalidString;
		}
		void SetStringIDValue(int index, string value)
		{
			mStringTable[index] = value;
		}
		#endregion

		#region Database interfaces
		// #NOTE place new DatabaseObjectKind code here

		void InitializeDatabaseInterfaces()
		{
			DamageTypes.SetupDatabaseInterface();
			ImpactEffects.SetupDatabaseInterface();
			WeaponTypes.SetupDatabaseInterface();
			UserClasses.SetupDatabaseInterface();
			Abilities.SetupDatabaseInterface();
			Objects.SetupDatabaseInterface();
			Squads.SetupDatabaseInterface();
			Tactics.SetupDatabaseInterface();
			Techs.SetupDatabaseInterface();
			TerrainTileTypes.SetupDatabaseInterface();
			Powers.SetupDatabaseInterface();
			Civs.SetupDatabaseInterface();
			Leaders.SetupDatabaseInterface();
		}

		const int kObjectIdIsObjectTypeBitMask = 1<<30;
		static void ObjectIdIsObjectTypeBitSet(ref int id)
		{
			id |= kObjectIdIsObjectTypeBitMask;
		}
		static bool ObjectIdIsObjectTypeBitGet(ref int id)
		{
			if ((id & kObjectIdIsObjectTypeBitMask) != 0)
			{
				id &= ~kObjectIdIsObjectTypeBitMask;
				return true;
			}
			return false;
		}

		int TryGetIdUnit(string name)
		{
			int id = Objects.TryGetId/*WithUndefined*/(name);

			if (id.IsNone())
			{
				if ((id = ObjectTypes.TryGetId(name)).IsNotNone())
					ObjectIdIsObjectTypeBitSet(ref id);
				else
					id = Objects.TryGetIdWithUndefined(name);
			}

			return id;
		}
		string TryGetNameUnit(int id)
		{
			if (ObjectIdIsObjectTypeBitGet(ref id))
				return ObjectTypes.TryGetNameWithUndefined(id);

			return Objects.TryGetNameWithUndefined(id);
		}

		public Collections.IBTypeNames GetNamesInterface(GameDataObjectKind kind)
		{
			Contract.Requires<ArgumentOutOfRangeException>(kind != GameDataObjectKind.None);

			return GameData.GetNamesInterface(kind);
		}
		public Collections.IBTypeNames GetNamesInterface(HPBarDataObjectKind kind)
		{
			Contract.Requires<ArgumentOutOfRangeException>(kind != HPBarDataObjectKind.None);

			return HPBars.GetNamesInterface(kind);
		}
		public Collections.IBTypeNames GetNamesInterface(DatabaseObjectKind kind)
		{
			Contract.Requires<ArgumentOutOfRangeException>(kind != DatabaseObjectKind.None);

			// #NOTE place new DatabaseObjectKind code here

			switch (kind)
			{
			case DatabaseObjectKind.Ability:	return Abilities;
			case DatabaseObjectKind.Civ:		return Civs;
			case DatabaseObjectKind.DamageType:	return DamageTypes;
			case DatabaseObjectKind.ImpactEffect: return ImpactEffects;
			case DatabaseObjectKind.Leader:		return Leaders;
			case DatabaseObjectKind.Object:		return Objects;
			case DatabaseObjectKind.ObjectType:	return ObjectTypes;
			case DatabaseObjectKind.Power:		return Powers;
			case DatabaseObjectKind.Squad:		return Squads;
			case DatabaseObjectKind.Tactic:		return Tactics;
			case DatabaseObjectKind.Tech:		return Techs;
			case DatabaseObjectKind.TerrainTileType: return TerrainTileTypes;
			case DatabaseObjectKind.Unit:		return null; // #TODO?
			case DatabaseObjectKind.UserClass:	return UserClasses;
			case DatabaseObjectKind.WeaponType:	return WeaponTypes;

			default: throw new KSoft.Debug.UnreachableException(kind.ToString());
			}
		}

		public int GetId(GameDataObjectKind kind, string name)
		{
			Contract.Requires<ArgumentOutOfRangeException>(kind != GameDataObjectKind.None);

			var dbi = GameData.GetMembersInterface(kind);
			return dbi.TryGetIdWithUndefined(name);
		}
		public int GetId(HPBarDataObjectKind kind, string name)
		{
			Contract.Requires<ArgumentOutOfRangeException>(kind != HPBarDataObjectKind.None);

			var dbi = HPBars.GetMembersInterface(kind);
			return dbi.TryGetIdWithUndefined(name);
		}
		public int GetId(DatabaseObjectKind kind, string name)
		{
			Contract.Requires<ArgumentOutOfRangeException>(kind != DatabaseObjectKind.None);

			// #NOTE place new DatabaseObjectKind code here

			switch (kind)
			{
			case DatabaseObjectKind.Ability:	return Abilities.TryGetIdWithUndefined(name);
			case DatabaseObjectKind.Civ:		return Civs.TryGetIdWithUndefined(name);
			case DatabaseObjectKind.DamageType:	return DamageTypes.TryGetIdWithUndefined(name);
			case DatabaseObjectKind.ImpactEffect: return ImpactEffects.TryGetIdWithUndefined(name);
			case DatabaseObjectKind.Leader:		return Leaders.TryGetIdWithUndefined(name);
			case DatabaseObjectKind.Object:		return Objects.TryGetIdWithUndefined(name);
			case DatabaseObjectKind.ObjectType:	return ObjectTypes.TryGetIdWithUndefined(name);
			case DatabaseObjectKind.Power:		return Powers.TryGetIdWithUndefined(name);
			case DatabaseObjectKind.Squad:		return Squads.TryGetIdWithUndefined(name);
			case DatabaseObjectKind.Tactic:		return Tactics.TryGetIdWithUndefined(name);
			case DatabaseObjectKind.Tech:		return Techs.TryGetIdWithUndefined(name);
			case DatabaseObjectKind.TerrainTileType: return TerrainTileTypes.TryGetIdWithUndefined(name);
			// TODO: Should just use the Objects DBI AFAICT
			case DatabaseObjectKind.Unit:		return TryGetIdUnit(name);
			case DatabaseObjectKind.UserClass:	return UserClasses.TryGetIdWithUndefined(name);
			case DatabaseObjectKind.WeaponType:	return WeaponTypes.TryGetIdWithUndefined(name);

			default: throw new KSoft.Debug.UnreachableException(kind.ToString());
			}
		}
		public string GetName(GameDataObjectKind kind, int id)
		{
			Contract.Requires<ArgumentOutOfRangeException>(kind != GameDataObjectKind.None);

			return GameData.GetName(kind, id);
		}
		public string GetName(HPBarDataObjectKind kind, int id)
		{
			Contract.Requires<ArgumentOutOfRangeException>(kind != HPBarDataObjectKind.None);

			return HPBars.GetName(kind, id);
		}
		public string GetName(DatabaseObjectKind kind, int id)
		{
			Contract.Requires<ArgumentOutOfRangeException>(kind != DatabaseObjectKind.None);

			// #NOTE place new DatabaseObjectKind code here

			switch (kind)
			{
			case DatabaseObjectKind.Ability:	return Abilities.TryGetNameWithUndefined(id);
			case DatabaseObjectKind.Civ:		return Civs.TryGetNameWithUndefined(id);
			case DatabaseObjectKind.DamageType:	return DamageTypes.TryGetNameWithUndefined(id);
			case DatabaseObjectKind.ImpactEffect: return ImpactEffects.TryGetNameWithUndefined(id);
			case DatabaseObjectKind.Leader:		return Leaders.TryGetNameWithUndefined(id);
			case DatabaseObjectKind.Object:		return Objects.TryGetNameWithUndefined(id);
			case DatabaseObjectKind.ObjectType:	return ObjectTypes.TryGetNameWithUndefined(id);
			case DatabaseObjectKind.Power:		return Powers.TryGetNameWithUndefined(id);
			case DatabaseObjectKind.Squad:		return Squads.TryGetNameWithUndefined(id);
			case DatabaseObjectKind.Tactic:		return Tactics.TryGetNameWithUndefined(id);
			case DatabaseObjectKind.Tech:		return Techs.TryGetNameWithUndefined(id);
			case DatabaseObjectKind.TerrainTileType: return TerrainTileTypes.TryGetNameWithUndefined(id);
			// TODO: Should just use the Objects DBI AFAICT
			case DatabaseObjectKind.Unit:		return TryGetNameUnit(id);
			case DatabaseObjectKind.UserClass:	return UserClasses.TryGetNameWithUndefined(id);
			case DatabaseObjectKind.WeaponType:	return WeaponTypes.TryGetNameWithUndefined(id);

			default: throw new KSoft.Debug.UnreachableException(kind.ToString());
			}
		}
		#endregion

		XML.BTriggerScriptSerializer mTriggerSerializer;
		internal void InitializeTriggerScriptSerializer()
		{
			mTriggerSerializer = new XML.BTriggerScriptSerializer(Engine);
		}
		public BTriggerSystem LoadScript(string scriptName, BTriggerScriptType type = BTriggerScriptType.TriggerScript)
		{
			var ctxt = mTriggerSerializer.StreamTriggerScriptGetContext(FA.Read, type, scriptName);
			var task = Task<bool>.Factory.StartNew((state) => {
				var _ctxt = state as XML.BTriggerScriptSerializer.StreamTriggerScriptContext;
				return mTriggerSerializer.TryStreamData(_ctxt.FileInfo, FA.Read, mTriggerSerializer.StreamTriggerScript, _ctxt);
			}, ctxt);

			return task.Result ? ctxt.Script : null;
		}
		public bool LoadScenarioScripts(string scnrPath)
		{
			var ctxt = mTriggerSerializer.StreamTriggerScriptGetContext(FA.Read, BTriggerScriptType.Scenario, scnrPath);
			var task = Task<bool>.Factory.StartNew((state) => {
				var _ctxt = state as XML.BTriggerScriptSerializer.StreamTriggerScriptContext;
				return mTriggerSerializer.TryStreamData(_ctxt.FileInfo, FA.Read, mTriggerSerializer.LoadScenarioScripts, _ctxt);
			}, ctxt);

			return task.Result;
		}

		protected abstract XML.BDatabaseXmlSerializerBase NewXmlSerializer();

		public void LoadAsync()
		{
			using (var xs = NewXmlSerializer())
			{
				var flags = 0
					// this is not compatible with the old Serina-style app database
					| XML.BDatabaseXmlSerializerLoadFlags.DoNotAutoLoadTactics;
				flags = 0; // but actually do load them
				xs.Load(flags);
			}
		}

		#region ITagElementStreamable<string> Members
		public void Serialize<TDoc, TCursor>(IO.TagElementStream<TDoc, TCursor, string> s)
			where TDoc : class
			where TCursor : class
		{
			using (var xs = NewXmlSerializer())
			{
				s.SetSerializerInterface(xs);
				xs.Serialize(s);
				s.SetSerializerInterface(null);
			}
		}
		#endregion
	};
}