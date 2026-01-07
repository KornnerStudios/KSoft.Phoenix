using System;
using System.Threading;
using System.Threading.Tasks;
#if CONTRACTS_FULL_SHIM
using Contract = System.Diagnostics.ContractsShim.Contract;
#else
using Contract = System.Diagnostics.Contracts.Contract; // SHIM'D
#endif

using FA = System.IO.FileAccess;

namespace KSoft.Phoenix.Phx
{
	public enum DatabaseLoadState
	{
		NotLoaded,
		Failed,
		Preloading,
		Preloaded,
		Loading,
		Loaded,

		kNumberOf
	};

	[ProtoDataTypeObjectSourceKind(ProtoDataObjectSourceKind.Database)]
	public abstract partial class BDatabaseBase
		: ObjectModel.BasicViewModel
		, IDisposable
		, IO.ITagElementStringNameStreamable
		, IProtoDataObjectDatabaseProvider
	{
		public const string kInvalidString = "BORK BORK BORK";

		public ProtoDataObjectDatabase ObjectDatabase { get; private set; }

		#region Xml constants
		internal static readonly XML.BListXmlParams kObjectTypesXmlParams = new("ObjectType");
		internal static readonly Engine.XmlFileInfo kObjectTypesXmlFileInfo = new()
		{
			Directory = Phoenix.Engine.GameDirectory.Data,
			FileName = "ObjectTypes.xml",
			RootName = kObjectTypesXmlParams.RootName
		};
		internal static readonly Engine.ProtoDataXmlFileInfo kObjectTypesProtoFileInfo = new(
			Phoenix.Engine.XmlFilePriority.Lists,
			kObjectTypesXmlFileInfo);
		#endregion

		#region LoadState
		DatabaseLoadState mLoadState = DatabaseLoadState.NotLoaded;
		public DatabaseLoadState LoadState
		{
			get
			{
				lock (mLoadStateLockee)
				{
					return mLoadState;
				}
			}
			set
			{
				lock (mLoadStateLockee)
				{
					this.SetFieldEnum(ref mLoadState, value);
				}
			}
		}

		readonly Lock mLoadStateLockee = new();
		#endregion

		public Engine.PhxEngine Engine { get; private set; }

		public abstract Collections.IProtoEnum GameObjectTypes { get; }
		public abstract Collections.IProtoEnum GameProtoObjectTypes { get; }
		public abstract Collections.IProtoEnum GameScenarioWorlds { get; }

		#region StringTable stuff
		public LocStringTable EnglishStringTable { get; private set; } = new();

		/// <summary>Maps a ID to a bit representing if any data references it somewhere. Only updated on data load</summary>
		public Collections.BitSet ReferencedStringIds { get; private set; }
			= new(ushort.MaxValue, fixedLength: false);

		internal void AddStringIDReference(int id)
		{
			if (ReferencedStringIds.Length < id)
			{
				int bump = id - ReferencedStringIds.Length;
				ReferencedStringIds.Length += bump + 16;
			}

			ReferencedStringIds[id] = true;
		}
		#endregion

		public BGameData GameData { get; private set; } = new();
		public HPBarData HPBars { get; private set; } = new();

		#region DatabaseObjectKind lists
		// #NOTE place new DatabaseObjectKind code here
		public Collections.BListAutoId<BDamageType> DamageTypes { get; private set; } = new();
		public Collections.BListAutoId<BProtoImpactEffect> ImpactEffects { get; private set; } = new();
		public Collections.BListAutoId<BWeaponType> WeaponTypes { get; private set; } = new();
		public Collections.BListAutoId<BUserClass> UserClasses { get; private set; } = new();
		public Collections.BTypeNamesWithCode ObjectTypes { get; private set; }
		public Collections.BListAutoId<BAbility> Abilities { get; private set; } = new();
		public Collections.BListAutoId<BProtoObject> Objects { get; private set; }
			= new(BProtoObject.kBListParams);
		public Collections.BListAutoId<BProtoSquad> Squads { get; private set; }
			= new(BProtoSquad.kBListParams);
		public Collections.BListAutoId<BProtoPower> Powers { get; private set; } = new();
		public Collections.BListAutoId<BTacticData> Tactics { get; private set; } = new();
		public Collections.BListAutoId<BProtoTech> Techs { get; private set; }
			= new(BProtoTech.kBListParams);
		public Collections.BListAutoId<TerrainTileType> TerrainTileTypes { get; private set; } = new();
		public Collections.BListAutoId<BCiv> Civs { get; private set; } = new();
		public Collections.BListAutoId<BLeader> Leaders { get; private set; } = new();
		#endregion

		public Collections.BListArray<BProtoMergedSquads> MergedSquads { get; private set; } = new();
		public BProtoShieldBubbleTypes ShieldBubbleTypes { get; private set; } = new();

		protected BDatabaseBase(Engine.PhxEngine engine, Collections.IProtoEnum gameObjectTypes)
		{
			Engine = engine;

			ObjectDatabase = new ProtoDataObjectDatabase(this, typeof(DatabaseObjectKind));

			ObjectTypes = new Collections.BTypeNamesWithCode(gameObjectTypes);

			InitializeDatabaseInterfaces();
		}

		#region IDisposable Members
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Usage", "CA1816:Dispose methods should call SuppressFinalize",
			Justification = "Not expecting any derived classes to have Finalizers")]
		public virtual void Dispose()
		{
			Util.DisposeAndNull(ref mTriggerSerializer);
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
				{
					ObjectIdIsObjectTypeBitSet(ref id);
				}
				else
				{
					id = Objects.TryGetIdWithUndefined(name);
				}
			}

			return id;
		}
		string TryGetNameUnit(int id)
		{
			if (ObjectIdIsObjectTypeBitGet(ref id))
			{
				return ObjectTypes.TryGetNameWithUndefined(id);
			}

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

			return kind switch
			{
				DatabaseObjectKind.Ability => Abilities,
				DatabaseObjectKind.Civ => Civs,
				DatabaseObjectKind.DamageType => DamageTypes,
				DatabaseObjectKind.ImpactEffect => ImpactEffects,
				DatabaseObjectKind.Leader => Leaders,
				DatabaseObjectKind.Object => Objects,
				DatabaseObjectKind.ObjectType => ObjectTypes,
				DatabaseObjectKind.Power => Powers,
				DatabaseObjectKind.Squad => Squads,
				DatabaseObjectKind.Tactic => Tactics,
				DatabaseObjectKind.Tech => Techs,
				DatabaseObjectKind.TerrainTileType => TerrainTileTypes,
				DatabaseObjectKind.Unit => null,// #TODO?
				DatabaseObjectKind.UserClass => UserClasses,
				DatabaseObjectKind.WeaponType => WeaponTypes,
				_ => throw new KSoft.Debug.UnreachableException(kind.ToString()),
			};
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

			return kind switch
			{
				DatabaseObjectKind.Ability => Abilities.TryGetIdWithUndefined(name),
				DatabaseObjectKind.Civ => Civs.TryGetIdWithUndefined(name),
				DatabaseObjectKind.DamageType => DamageTypes.TryGetIdWithUndefined(name),
				DatabaseObjectKind.ImpactEffect => ImpactEffects.TryGetIdWithUndefined(name),
				DatabaseObjectKind.Leader => Leaders.TryGetIdWithUndefined(name),
				DatabaseObjectKind.Object => Objects.TryGetIdWithUndefined(name),
				DatabaseObjectKind.ObjectType => ObjectTypes.TryGetIdWithUndefined(name),
				DatabaseObjectKind.Power => Powers.TryGetIdWithUndefined(name),
				DatabaseObjectKind.Squad => Squads.TryGetIdWithUndefined(name),
				DatabaseObjectKind.Tactic => Tactics.TryGetIdWithUndefined(name),
				DatabaseObjectKind.Tech => Techs.TryGetIdWithUndefined(name),
				DatabaseObjectKind.TerrainTileType => TerrainTileTypes.TryGetIdWithUndefined(name),
				// TODO: Should just use the Objects DBI AFAICT
				DatabaseObjectKind.Unit => TryGetIdUnit(name),
				DatabaseObjectKind.UserClass => UserClasses.TryGetIdWithUndefined(name),
				DatabaseObjectKind.WeaponType => WeaponTypes.TryGetIdWithUndefined(name),
				_ => throw new KSoft.Debug.UnreachableException(kind.ToString()),
			};
		}
		public string GetName(GameDataObjectKind kind, int id)
		{
			Contract.Requires<ArgumentOutOfRangeException>(kind != GameDataObjectKind.None);

			IProtoDataObjectDatabaseProvider provider = GameData;
			return provider.GetName((int)kind, id);
		}
		public string GetName(HPBarDataObjectKind kind, int id)
		{
			Contract.Requires<ArgumentOutOfRangeException>(kind != HPBarDataObjectKind.None);

			IProtoDataObjectDatabaseProvider provider = HPBars;
			return provider.GetName((int)kind, id);
		}
		public string GetName(DatabaseObjectKind kind, int id)
		{
			Contract.Requires<ArgumentOutOfRangeException>(kind != DatabaseObjectKind.None);

			// #NOTE place new DatabaseObjectKind code here

			return kind switch
			{
				DatabaseObjectKind.Ability => Abilities.TryGetNameWithUndefined(id),
				DatabaseObjectKind.Civ => Civs.TryGetNameWithUndefined(id),
				DatabaseObjectKind.DamageType => DamageTypes.TryGetNameWithUndefined(id),
				DatabaseObjectKind.ImpactEffect => ImpactEffects.TryGetNameWithUndefined(id),
				DatabaseObjectKind.Leader => Leaders.TryGetNameWithUndefined(id),
				DatabaseObjectKind.Object => Objects.TryGetNameWithUndefined(id),
				DatabaseObjectKind.ObjectType => ObjectTypes.TryGetNameWithUndefined(id),
				DatabaseObjectKind.Power => Powers.TryGetNameWithUndefined(id),
				DatabaseObjectKind.Squad => Squads.TryGetNameWithUndefined(id),
				DatabaseObjectKind.Tactic => Tactics.TryGetNameWithUndefined(id),
				DatabaseObjectKind.Tech => Techs.TryGetNameWithUndefined(id),
				DatabaseObjectKind.TerrainTileType => TerrainTileTypes.TryGetNameWithUndefined(id),
				// TODO: Should just use the Objects DBI AFAICT
				DatabaseObjectKind.Unit => TryGetNameUnit(id),
				DatabaseObjectKind.UserClass => UserClasses.TryGetNameWithUndefined(id),
				DatabaseObjectKind.WeaponType => WeaponTypes.TryGetNameWithUndefined(id),
				_ => throw new KSoft.Debug.UnreachableException(kind.ToString()),
			};
		}
		#endregion

		#region IProtoDataObjectDatabaseProvider members
		Engine.XmlFileInfo IProtoDataObjectDatabaseProvider.SourceFileReference { get { return null; } }

		Collections.IBTypeNames IProtoDataObjectDatabaseProvider.GetNamesInterface(int objectKind)
		{
			var kind = (DatabaseObjectKind)objectKind;
			return GetNamesInterface(kind);
		}

		Collections.IHasUndefinedProtoMemberInterface IProtoDataObjectDatabaseProvider.GetMembersInterface(int objectKind)
		{
			var kind = (DatabaseObjectKind)objectKind;
			return GetNamesInterface/*GetMembersInterface*/(kind);
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
		private XML.BDatabaseXmlSerializerBase mXmlSerializer;

		public bool Preload()
		{
			var xs = mXmlSerializer = NewXmlSerializer();//using (var xs = NewXmlSerializer())
			{
				return xs.Preload();
			}
		}

		public bool Load()
		{
			Contract.Assert(mXmlSerializer != null);

			var xs = mXmlSerializer;//using (var xs = NewXmlSerializer())
			{
				return xs.Load();
			}
		}

		public bool LoadAllTactics()
		{
			Contract.Assert(mXmlSerializer != null);

			var xs = mXmlSerializer;//using (var xs = NewXmlSerializer())
			{
				return xs.LoadAllTactics();
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