using System.Collections.Generic;

namespace KSoft.Phoenix.Runtime
{
	sealed partial class BDatabase
		: IO.IEndianStreamSerializable
	{
		public List<string> Civs = new(); // max=0x64
		public List<string> Leaders = new(); // max=0x12C
		public List<string> Abilities = new(); // max=0x3E8
		public List<string> ProtoVisuals = new(); // max=0x2710
		public List<string> Models = new(); // max=0x2710
		public List<string> Animations = new(); // max=0x2710
		public List<string> TerrainEffects = new(); // max=0x1F4
		public List<string> ProtoImpactEffects = new(); // max=0x1F4
		public List<string> LightEffects = new(); // max=0x3E8
		public List<string> ParticleGateways = new(); // max=0x3E8
		public List<GenericProtoObjectEntry> GenericProtoObjects { get; private set; } = new(); // max=0x4E20
		public List<ProtoSquadEntry> ProtoSquads { get; private set; } = new(); // max=0x4E20
		public List<string> ProtoTechs { get; private set; } = new(); // max=0x2710
		public List<string> ProtoPowers { get; private set; } = new(); // max=0x3E8
		public List<string> ProtoObjects { get; private set; } = new(); // max=0x4E20 includes objecttypes
		public List<string> Resources { get; private set; } = new(); // max=0xC8
		public List<string> Rates { get; private set; } = new(); // max=0xC8
		public List<string> Populations { get; private set; } = new(); // max=0xC8
		public List<string> WeaponTypes { get; private set; } = new(); // max=0x2710
		public List<string> DamageTypes { get; private set; } = new(); // max=0xC8
		public List<TemplateEntry> Templates { get; private set; } = new(); // max=0x3E8
		public List<string> AnimTypes { get; private set; } = new(); // max=0x3E8
		public List<string> EffectTypes { get; private set; } = new(); // max=0x7D0
		public List<string> Actions { get; private set; } = new(); // max=0xFA
		public List<CondensedListItem16<Tactic>> Tactics { get; private set; } = new();
		int NumUniqueProtoObjects; // max=0x64
		public List<CondensedListItemValue32<DataTagValue>> Shapes { get; private set; } = new();
		public List<CondensedListItemValue32<DataTagValue>> PhysicsInfo { get; private set; } = new();
		public List<ProtoIcon> ProtoIcons { get; private set; } = new(); // max=0x3E8

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