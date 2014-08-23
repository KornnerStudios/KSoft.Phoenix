using System.Collections.Generic;

namespace KSoft.Phoenix.Runtime
{
	sealed partial class BDatabase
		: IO.IEndianStreamSerializable
	{
		public List<string> Civs = new List<string>(); // max=0x64
		public List<string> Leaders = new List<string>(); // max=0x12C
		public List<string> Abilities = new List<string>(); // max=0x3E8
		public List<string> ProtoVisuals = new List<string>(); // max=0x2710
		public List<string> Models = new List<string>(); // max=0x2710
		public List<string> Animations = new List<string>(); // max=0x2710
		public List<string> TerrainEffects = new List<string>(); // max=0x1F4
		public List<string> ProtoImpactEffects = new List<string>(); // max=0x1F4
		public List<string> LightEffects = new List<string>(); // max=0x3E8
		public List<string> ParticleGateways = new List<string>(); // max=0x3E8
		public List<GenericProtoObjectEntry> GenericProtoObjects { get; private set; } // max=0x4E20
		public List<ProtoSquadEntry> ProtoSquads { get; private set; } // max=0x4E20
		public List<string> ProtoTechs { get; private set; } // max=0x2710
		public List<string> ProtoPowers { get; private set; } // max=0x3E8
		public List<string> ProtoObjects { get; private set; } // max=0x4E20 includes objecttypes
		public List<string> Resources { get; private set; } // max=0xC8
		public List<string> Rates { get; private set; } // max=0xC8
		public List<string> Populations { get; private set; } // max=0xC8
		public List<string> WeaponTypes { get; private set; } // max=0x2710
		public List<string> DamageTypes { get; private set; } // max=0xC8
		public List<TemplateEntry> Templates { get; private set; } // max=0x3E8
		public List<string> AnimTypes { get; private set; } // max=0x3E8
		public List<string> EffectTypes { get; private set; } // max=0x7D0
		public List<string> Actions { get; private set; } // max=0xFA
		public List<CondensedListItem16<Tactic>> Tactics { get; private set; }
		int NumUniqueProtoObjects; // max=0x64
		public List<CondensedListItemValue32<DataTagValue>> Shapes { get; private set; }
		public List<CondensedListItemValue32<DataTagValue>> PhysicsInfo { get; private set; }
		public List<ProtoIcon> ProtoIcons { get; private set; } // max=0x3E8

		public BDatabase()
		{
			Civs = new List<string>();			Leaders = new List<string>();
			Abilities = new List<string>();		ProtoVisuals = new List<string>();
			Models = new List<string>();		Animations = new List<string>();
			TerrainEffects = new List<string>();ProtoImpactEffects = new List<string>();
			LightEffects = new List<string>();	ParticleGateways = new List<string>();
			GenericProtoObjects = new List<GenericProtoObjectEntry>();
			ProtoSquads = new List<ProtoSquadEntry>();
			ProtoTechs = new List<string>();	ProtoPowers = new List<string>();
			ProtoObjects = new List<string>();	Resources = new List<string>();
			Rates = new List<string>();			Populations = new List<string>();
			WeaponTypes = new List<string>();	DamageTypes = new List<string>();
			Templates = new List<TemplateEntry>();
			AnimTypes = new List<string>();		EffectTypes = new List<string>();
			Actions = new List<string>();
			Tactics = new List<CondensedListItem16<Tactic>>();
			Shapes = new List<CondensedListItemValue32<DataTagValue>>();
			PhysicsInfo = new List<CondensedListItemValue32<DataTagValue>>();
			ProtoIcons = new List<ProtoIcon>();
		}

		#region IEndianStreamSerializable Members
		public void Serialize(IO.EndianStream s)
		{
			BSaveGame.StreamCollection(s, Civs);
			BSaveGame.StreamCollection(s, Leaders);
			BSaveGame.StreamCollection(s, Abilities);
			BSaveGame.StreamCollection(s, ProtoVisuals);
			BSaveGame.StreamCollection(s, Models);
			BSaveGame.StreamCollection(s, Animations);
			BSaveGame.StreamCollection(s, TerrainEffects);
			BSaveGame.StreamCollection(s, ProtoImpactEffects);
			BSaveGame.StreamCollection(s, LightEffects);
			BSaveGame.StreamCollection(s, ParticleGateways);
			BSaveGame.StreamCollection(s, GenericProtoObjects);
			BSaveGame.StreamCollection(s, ProtoSquads);
			BSaveGame.StreamCollection(s, ProtoTechs);
			BSaveGame.StreamCollection(s, ProtoPowers);
			BSaveGame.StreamCollection(s, ProtoObjects);
			BSaveGame.StreamCollection(s, Resources);
			BSaveGame.StreamCollection(s, Rates);
			BSaveGame.StreamCollection(s, Populations);
			BSaveGame.StreamCollection(s, WeaponTypes);
			BSaveGame.StreamCollection(s, DamageTypes);
			BSaveGame.StreamCollection(s, Templates);
			BSaveGame.StreamCollection(s, AnimTypes);
			BSaveGame.StreamCollection(s, EffectTypes);
			BSaveGame.StreamCollection(s, Actions);
			BSaveGame.StreamList(s, Tactics, kTacticsListInfo);

			s.Stream(ref NumUniqueProtoObjects);
			s.StreamSignature((uint)NumUniqueProtoObjects);

			BSaveGame.StreamList(s, Shapes, kDataTagsListInfo);
			BSaveGame.StreamList(s, PhysicsInfo, kDataTagsListInfo);

			BSaveGame.StreamCollection(s, ProtoIcons);

			s.StreamSignature(cSaveMarker.DB);
		}
		#endregion
	};
}