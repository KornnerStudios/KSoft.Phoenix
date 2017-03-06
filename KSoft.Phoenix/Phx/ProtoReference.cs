using System;

namespace KSoft.Phoenix.Phx.Meta
{
	[AttributeUsage(ProtoReferenceAttribute.kValidOn, AllowMultiple=false)]
	public abstract class ProtoReferenceAttribute : Attribute
	{
		public const AttributeTargets kValidOn = 0
			| AttributeTargets.Field
			| AttributeTargets.Property
			| AttributeTargets.Parameter
			| AttributeTargets.ReturnValue
			| AttributeTargets.GenericParameter
			;

		public Type ProtoType { get; private set; }
		public DatabaseObjectKind ProtoKind { get; private set; }

		protected ProtoReferenceAttribute(Type protoType, DatabaseObjectKind kind = DatabaseObjectKind.None)
		{
			ProtoType = protoType;
		}
	};

	/// <summary>For fields in ProtoData that are not actually used in any meaningful way</summary>
	[AttributeUsage(ProtoReferenceAttribute.kValidOn, AllowMultiple = false)]
	public sealed class UnusedDataAttribute : Attribute
	{
		public UnusedDataAttribute() { }
		public UnusedDataAttribute(string note) { }
	};

	/// <summary>Localized string reference</summary>
	[AttributeUsage(ProtoReferenceAttribute.kValidOn, AllowMultiple=false)]
	public sealed class LocStringReferenceAttribute : Attribute
	{
	};

	/// <summary>DDX reference</summary>
	[AttributeUsage(ProtoReferenceAttribute.kValidOn, AllowMultiple=false)]
	public sealed class TextureReferenceAttribute : Attribute
	{
	};

	[AttributeUsage(ProtoReferenceAttribute.kValidOn, AllowMultiple = false)]
	public sealed class SoundCueReferenceAttribute : Attribute
	{
	};

	[AttributeUsage(ProtoReferenceAttribute.kValidOn, AllowMultiple = false)]
	public sealed class BAnimTypeReferenceAttribute : Attribute
	{
	};

	/// <summary>Cost/Resource type reference</summary>
	[AttributeUsage(ProtoReferenceAttribute.kValidOn, AllowMultiple = false)]
	public sealed class ResourceReferenceAttribute : Attribute
	{
	};
	[AttributeUsage(ProtoReferenceAttribute.kValidOn, AllowMultiple = false)]
	public sealed class PopulationReferenceAttribute : Attribute
	{
	};
	[AttributeUsage(ProtoReferenceAttribute.kValidOn, AllowMultiple = false)]
	public sealed class RateReferenceAttribute : Attribute
	{
	};

	public sealed class BAbilityReferenceAttribute : ProtoReferenceAttribute
	{
		public BAbilityReferenceAttribute() : base(typeof(BAbility), DatabaseObjectKind.Ability) { }
	};

	public sealed class BCivReferenceAttribute : ProtoReferenceAttribute
	{
		public BCivReferenceAttribute() : base(typeof(BCiv), DatabaseObjectKind.Civ) { }
	};

	public sealed class BDamageTypeReferenceAttribute : ProtoReferenceAttribute
	{
		public BDamageTypeReferenceAttribute() : base(typeof(BDamageType), DatabaseObjectKind.DamageType) { }
	};

	public sealed class BProtoImpactEffectReferenceAttribute : ProtoReferenceAttribute
	{
		public BProtoImpactEffectReferenceAttribute() : base(typeof(BProtoImpactEffect), DatabaseObjectKind.ImpactEffect) { }
	};

	public sealed class BLeaderReferenceAttribute : ProtoReferenceAttribute
	{
		public BLeaderReferenceAttribute() : base(typeof(BLeader), DatabaseObjectKind.Leader) { }
	};

	public sealed class BProtoObjectReferenceAttribute : ProtoReferenceAttribute
	{
		public BProtoObjectReferenceAttribute() : base(typeof(BProtoObject), DatabaseObjectKind.Object) { }
	};

	public sealed class ObjectTypeReferenceAttribute : ProtoReferenceAttribute
	{
		public ObjectTypeReferenceAttribute() : base(null, DatabaseObjectKind.ObjectType) { }
	};

	public sealed class BProtoPowerReferenceAttribute : ProtoReferenceAttribute
	{
		public BProtoPowerReferenceAttribute() : base(typeof(BProtoPower), DatabaseObjectKind.Power) { }
	};

	public sealed class BProtoSquadReferenceAttribute : ProtoReferenceAttribute
	{
		public BProtoSquadReferenceAttribute() : base(typeof(BProtoSquad), DatabaseObjectKind.Squad) { }
	};

	public sealed class BProtoTechReferenceAttribute : ProtoReferenceAttribute
	{
		public BProtoTechReferenceAttribute() : base(typeof(BProtoTech), DatabaseObjectKind.Tech) { }
	};

	/// <summary>Object or ObjectType</summary>
	public sealed class UnitReferenceAttribute : ProtoReferenceAttribute
	{
		public UnitReferenceAttribute() : base(null, DatabaseObjectKind.Unit) { }
	};

	public sealed class BUserClassReferenceAttribute : ProtoReferenceAttribute
	{
		public BUserClassReferenceAttribute() : base(typeof(BUserClass), DatabaseObjectKind.UserClass) { }
	};

	public sealed class BWeaponTypeReferenceAttribute : ProtoReferenceAttribute
	{
		public BWeaponTypeReferenceAttribute() : base(typeof(BWeaponType), DatabaseObjectKind.WeaponType) { }
	};
}