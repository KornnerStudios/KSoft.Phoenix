
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
		ObjectType,
		Power,
		Squad,
		Tactic,
		Tech,
		TerrainTileType,
		/// <summary>Object or ObjectType</summary>
		Unit,
		UserClass,
		WeaponType,
	};
}