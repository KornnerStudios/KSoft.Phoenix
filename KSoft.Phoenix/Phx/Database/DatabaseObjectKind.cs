
namespace KSoft.Phoenix.Phx
{
	public enum DatabaseObjectKind
	{
		None = PhxUtil.kObjectKindNone,

		// #NOTE place new DatabaseObjectKind code here

		Ability,
		Civ,
		DamageType,
		ImpactEffect,
		Leader,
		Object,
		/// <summary>Object or ObjectType</summary>
		/// <remarks>
		/// The engine doesn't have a type for things which are just used in object_types.xml.
		/// An ObjectType is anything named in objects.xml and object_types.xml.
		/// </remarks>
		/// <seealso cref="Meta.ObjectTypeReferenceAttribute"/>
		ObjectType,
		Power,
		Squad,
		Tactic,
		Tech,
		TerrainTileType,
		/// <summary>Object or ObjectType</summary>
		// #TODO these should all be a ObjectType. Need to #REMOVE BProtoUnitID and UnitReference
//		[System.Obsolete($"Use {nameof(ObjectType)} and {nameof(BObjectTypeID)}")]
		Unit,
		UserClass,
		WeaponType,
	};
}