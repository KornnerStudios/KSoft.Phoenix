using System;
using System.Collections.Generic;
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
		public Collections.BListAutoId<BDamageType> DamageTypes { get; private set; }
		public Collections.BListAutoId<BWeaponType> WeaponTypes { get; private set; }
		public Collections.BListAutoId<BUserClass> UserClasses { get; private set; }
		public Collections.BTypeNamesWithCode ObjectTypes { get; private set; }
		public Collections.BListAutoId<BAbility> Abilities { get; private set; }
		public Collections.BListAutoId<BProtoObject> Objects { get; private set; }
		public Collections.BListAutoId<BProtoSquad> Squads { get; private set; }
		public Collections.BListAutoId<BProtoPower> Powers { get; private set; }
		public Collections.BListAutoId<BProtoTech> Techs { get; private set; }
		public Collections.BListAutoId<BCiv> Civs { get; private set; }
		public Collections.BListAutoId<BLeader> Leaders { get; private set; }

		public Dictionary<int, BTacticData> ObjectTacticsMap { get; private set; }

		protected BDatabaseBase(Engine.PhxEngine engine, Collections.IProtoEnum gameObjectTypes)
		{
			Engine = engine;

			mStringTable = new Dictionary<int, string>();

			GameData = new BGameData();
			DamageTypes = new Collections.BListAutoId<BDamageType>();
			WeaponTypes = new Collections.BListAutoId<BWeaponType>();
			UserClasses = new Collections.BListAutoId<BUserClass>();
			ObjectTypes = new Collections.BTypeNamesWithCode(gameObjectTypes);
			Abilities = new Collections.BListAutoId<BAbility>();
			Objects = new Collections.BListAutoId<BProtoObject>();
			Squads = new Collections.BListAutoId<BProtoSquad>();
			Powers = new Collections.BListAutoId<BProtoPower>();
			Techs = new Collections.BListAutoId<BProtoTech>();
			Civs = new Collections.BListAutoId<BCiv>();
			Leaders = new Collections.BListAutoId<BLeader>();

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
		Dictionary<string, BDamageType> mDbiDamageTypes;
		Dictionary<string, BWeaponType> mDbiWeaponTypes;
		Dictionary<string, BUserClass> mDbiUserClasses;
		Dictionary<string, BAbility> mDbiAbilities;
		Dictionary<string, BProtoObject> mDbiObjects;
		Dictionary<string, BProtoSquad> mDbiSquads;
		Dictionary<string, BProtoPower> mDbiPowers;
		Dictionary<string, BProtoTech> mDbiTechs;
		Dictionary<string, BCiv> mDbiCivs;
		Dictionary<string, BLeader> mDbiLeaders;

		void InitializeDatabaseInterfaces()
		{
			DamageTypes.SetupDatabaseInterface(out mDbiDamageTypes);
			WeaponTypes.SetupDatabaseInterface(out mDbiWeaponTypes);
			UserClasses.SetupDatabaseInterface(out mDbiUserClasses);
			Abilities.SetupDatabaseInterface(out mDbiAbilities);
			Objects.SetupDatabaseInterface(out mDbiObjects);
			Squads.SetupDatabaseInterface(out mDbiSquads);
			Techs.SetupDatabaseInterface(out mDbiTechs);
			Powers.SetupDatabaseInterface(out mDbiPowers);
			Civs.SetupDatabaseInterface(out mDbiCivs);
			Leaders.SetupDatabaseInterface(out mDbiLeaders);
		}

		static int TryGetId(Collections.BTypeNames dbi, string name)
		{
			return dbi.TryGetMemberId(name);
		}
		static string TryGetName(Collections.BTypeNames dbi, int id)
		{
			if (id >= 0 && id < dbi.Count)
				return dbi[id];

			return null;
		}
		static int TryGetId<T>(Collections.BListAutoId<T> dbi, string name)
			where T : class, Collections.IListAutoIdObject, new()
		{
			return dbi.TryGetMemberId(name);
		}
		internal static string TryGetName<T>(Collections.BListAutoId<T> dbi, int id)
			where T : class, Collections.IListAutoIdObject, new()
		{
			if (id >= 0 && id < dbi.Count) return dbi[id].Data;

			return null;
		}
		internal static int TryGetId<T>(Dictionary<string, T> dbi, string name, Collections.BListAutoId<T> _unused)
			where T : class, Collections.IListAutoIdObject, new()
		{
			int id = TypeExtensions.kNone;

			T obj;
			if (dbi.TryGetValue(name, out obj))
				id = obj.AutoId;

			return id;
		}

		static int TryGetIdWithUndefined(Collections.BTypeNames dbi, string name)
		{
			return dbi.UndefinedInterface.GetMemberIdOrUndefined(name);
		}
		static string TryGetNameWithUndefined(Collections.BTypeNames dbi, int id)
		{
			return dbi.UndefinedInterface.GetMemberNameOrUndefined(id);
		}
		static int TryGetIdWithUndefined<T>(Collections.BListAutoId<T> dbi, string name)
			where T : class, Collections.IListAutoIdObject, new()
		{
			return dbi.UndefinedInterface.GetMemberIdOrUndefined(name);
		}
		internal static string TryGetNameWithUndefined<T>(Collections.BListAutoId<T> dbi, int id)
			where T : class, Collections.IListAutoIdObject, new()
		{
			return dbi.UndefinedInterface.GetMemberNameOrUndefined(id);
		}
		internal static int TryGetIdWithUndefined<T>(Dictionary<string, T> dbi, string name, Collections.BListAutoId<T> list)
			where T : class, Collections.IListAutoIdObject, new()
		{
			int id = TryGetId(dbi, name, null);

			if (id.IsNone())
				id = list.UndefinedInterface.GetMemberIdOrUndefined(name);

			return id;
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
			int id = TryGetIdWithUndefined(mDbiObjects, name, Objects);

			if (id.IsNone() && (id = TryGetId(ObjectTypes, name)).IsNotNone())
				ObjectIdIsObjectTypeBitSet(ref id);

			return id;
		}
		string TryGetNameUnit(int id)
		{
			if (ObjectIdIsObjectTypeBitGet(ref id))
				return TryGetNameWithUndefined(ObjectTypes, id);

			return TryGetNameWithUndefined(Objects, id);
		}

		public int GetId(GameDataObjectKind kind, string name)
		{
			Contract.Requires<ArgumentOutOfRangeException>(kind != GameDataObjectKind.None);

			switch (kind)
			{
			case GameDataObjectKind.Cost:	return TryGetIdWithUndefined(GameData.Resources, name);
			case GameDataObjectKind.Pop:	return TryGetIdWithUndefined(GameData.Populations, name);
			case GameDataObjectKind.Rate:	return TryGetIdWithUndefined(GameData.Rates, name);

			default: throw new KSoft.Debug.UnreachableException(kind.ToString());
			}
		}
		public int GetId(DatabaseObjectKind kind, string name)
		{
			Contract.Requires<ArgumentOutOfRangeException>(kind != DatabaseObjectKind.None);

			switch (kind)
			{
			case DatabaseObjectKind.Ability:	return TryGetIdWithUndefined(mDbiAbilities, name, Abilities);
			case DatabaseObjectKind.Civ:		return TryGetIdWithUndefined(mDbiCivs, name, Civs);
			case DatabaseObjectKind.DamageType:	return TryGetIdWithUndefined(mDbiDamageTypes, name, DamageTypes);
			case DatabaseObjectKind.Leader:		return TryGetIdWithUndefined(mDbiLeaders, name, Leaders);
			case DatabaseObjectKind.Object:		return TryGetIdWithUndefined(mDbiObjects, name, Objects);
			case DatabaseObjectKind.ObjectType:	return TryGetIdWithUndefined(ObjectTypes, name);
			case DatabaseObjectKind.Power:		return TryGetIdWithUndefined(mDbiPowers, name, Powers);
			case DatabaseObjectKind.Squad:		return TryGetIdWithUndefined(mDbiSquads, name, Squads);
			case DatabaseObjectKind.Tech:		return TryGetIdWithUndefined(mDbiTechs, name, Techs);
			// TODO: Should just use the Objects DBI AFAICT
			case DatabaseObjectKind.Unit:		return TryGetIdUnit(name);
			case DatabaseObjectKind.UserClass:	return TryGetIdWithUndefined(mDbiUserClasses, name, UserClasses);
			case DatabaseObjectKind.WeaponType:	return TryGetIdWithUndefined(mDbiWeaponTypes, name, WeaponTypes);

			default: throw new KSoft.Debug.UnreachableException(kind.ToString());
			}
		}
		public string GetName(GameDataObjectKind kind, int id)
		{
			Contract.Requires<ArgumentOutOfRangeException>(kind != GameDataObjectKind.None);

			switch (kind)
			{
			case GameDataObjectKind.Cost:	return TryGetNameWithUndefined(GameData.Resources, id);
			case GameDataObjectKind.Pop:	return TryGetNameWithUndefined(GameData.Populations, id);
			case GameDataObjectKind.Rate:	return TryGetNameWithUndefined(GameData.Rates, id);

			default: throw new KSoft.Debug.UnreachableException(kind.ToString());
			}
		}
		public string GetName(DatabaseObjectKind kind, int id)
		{
			Contract.Requires<ArgumentOutOfRangeException>(kind != DatabaseObjectKind.None);

			switch (kind)
			{
			case DatabaseObjectKind.Ability:	return TryGetNameWithUndefined(Abilities, id);
			case DatabaseObjectKind.Civ:		return TryGetNameWithUndefined(Civs, id);
			case DatabaseObjectKind.DamageType:	return TryGetNameWithUndefined(DamageTypes, id);
			case DatabaseObjectKind.Leader:		return TryGetNameWithUndefined(Leaders, id);
			case DatabaseObjectKind.Object:		return TryGetNameWithUndefined(Objects, id);
			case DatabaseObjectKind.ObjectType:	return TryGetNameWithUndefined(ObjectTypes, id);
			case DatabaseObjectKind.Power:		return TryGetNameWithUndefined(Powers, id);
			case DatabaseObjectKind.Squad:		return TryGetNameWithUndefined(Squads, id);
			case DatabaseObjectKind.Tech:		return TryGetNameWithUndefined(Techs, id);
			// TODO: Should just use the Objects DBI AFAICT
			case DatabaseObjectKind.Unit:		return TryGetNameUnit(id);
			case DatabaseObjectKind.UserClass:	return TryGetNameWithUndefined(UserClasses, id);
			case DatabaseObjectKind.WeaponType:	return TryGetNameWithUndefined(WeaponTypes, id);

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
		public BTriggerSystem LoadScript(string script_name, BTriggerScriptType type = BTriggerScriptType.TriggerScript)
		{
			var ctxt = mTriggerSerializer.StreamTriggerScriptGetContext(FA.Read, type, script_name);
			var task = System.Threading.Tasks.Task<bool>.Factory.StartNew((state) => {
				var _ctxt = state as XML.BTriggerScriptSerializer.StreamTriggerScriptContext;
				return mTriggerSerializer.TryStreamData(_ctxt.FileInfo, FA.Read, mTriggerSerializer.StreamTriggerScript, _ctxt);
			}, ctxt);

			return task.Result ? ctxt.Script : null;
		}
		public bool LoadScenarioScripts(string scnrPath)
		{
			var ctxt = mTriggerSerializer.StreamTriggerScriptGetContext(FA.Read, BTriggerScriptType.Scenario, scnrPath);
			var task = System.Threading.Tasks.Task<bool>.Factory.StartNew((state) => {
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