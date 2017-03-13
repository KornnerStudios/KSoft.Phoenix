using System;
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
		#endregion

		public Engine.PhxEngine Engine { get; private set; }

		public abstract Collections.IProtoEnum GameObjectTypes { get; }
		public abstract Collections.IProtoEnum GameProtoObjectTypes { get; }
		public abstract Collections.IProtoEnum GameScenarioWorlds { get; }

		Dictionary<int, string> mStringTable;
		/// <summary>StringID values indexed by their official locID</summary>
		/// <remarks>Only populated with strings from a StringTable which Phx-based objects reference, to cut down on memory usage</remarks>
		public IReadOnlyDictionary<int, string> StringTable { get { return mStringTable; } }

		public BGameData GameData { get; private set; }
		public HPBarData HPBars { get; private set; }
		// #NOTE place new DatabaseObjectKind code here
		public Collections.BListAutoId<BDamageType> DamageTypes { get; private set; }
		public Collections.BListAutoId<BProtoImpactEffect> ImpactEffects { get; private set; }
		public Collections.BListAutoId<BWeaponType> WeaponTypes { get; private set; }
		public Collections.BListAutoId<BUserClass> UserClasses { get; private set; }
		public Collections.BTypeNamesWithCode ObjectTypes { get; private set; }
		public Collections.BListAutoId<BAbility> Abilities { get; private set; }
		public Collections.BListAutoId<BProtoObject> Objects { get; private set; }
		public Collections.BListAutoId<BProtoSquad> Squads { get; private set; }
		public Collections.BListAutoId<BProtoPower> Powers { get; private set; }
		public Collections.BListAutoId<BProtoTech> Techs { get; private set; }
		public Collections.BListAutoId<TerrainTileType> TerrainTileTypes { get; private set; }
		public Collections.BListAutoId<BCiv> Civs { get; private set; }
		public Collections.BListAutoId<BLeader> Leaders { get; private set; }

		public Collections.BListArray<BProtoMergedSquads> MergedSquads { get; private set; }
		public BProtoShieldBubbleTypes ShieldBubbleTypes { get; private set; }

		public Dictionary<int, BTacticData> ObjectTacticsMap { get; private set; }

		protected BDatabaseBase(Engine.PhxEngine engine, Collections.IProtoEnum gameObjectTypes)
		{
			Engine = engine;

			mStringTable = new Dictionary<int, string>();

			GameData = new BGameData();
			HPBars = new HPBarData();
			DamageTypes = new Collections.BListAutoId<BDamageType>();
			ImpactEffects = new Collections.BListAutoId<BProtoImpactEffect>();
			WeaponTypes = new Collections.BListAutoId<BWeaponType>();
			UserClasses = new Collections.BListAutoId<BUserClass>();
			ObjectTypes = new Collections.BTypeNamesWithCode(gameObjectTypes);
			Abilities = new Collections.BListAutoId<BAbility>();
			Objects = new Collections.BListAutoId<BProtoObject>();
			Squads = new Collections.BListAutoId<BProtoSquad>();
			Powers = new Collections.BListAutoId<BProtoPower>();
			Techs = new Collections.BListAutoId<BProtoTech>();
			TerrainTileTypes = new Collections.BListAutoId<TerrainTileType>();
			Civs = new Collections.BListAutoId<BCiv>();
			Leaders = new Collections.BListAutoId<BLeader>();

			MergedSquads = new Collections.BListArray<BProtoMergedSquads>();
			ShieldBubbleTypes = new BProtoShieldBubbleTypes();

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

		internal void BuildObjectTacticsMap(Dictionary<int, string> idToTacticMap, Dictionary<string, BTacticData> tacticNameToTactic)
		{
			ObjectTacticsMap = new Dictionary<int, BTacticData>(idToTacticMap.Count);

			foreach (var kv in idToTacticMap)
				ObjectTacticsMap.Add(kv.Key, tacticNameToTactic[kv.Value]);
		}

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

		public void Load()
		{
			using (var xs = NewXmlSerializer())
			{
				var flags = XML.BDatabaseXmlSerializerLoadFlags.LoadUpdates | XML.BDatabaseXmlSerializerLoadFlags.UseSynchronousLoading;
				xs.Load(flags);
			}
		}
		public void LoadAsync()
		{
			using (var xs = NewXmlSerializer())
			{
				var flags = XML.BDatabaseXmlSerializerLoadFlags.LoadUpdates;
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