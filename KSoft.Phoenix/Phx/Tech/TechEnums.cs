
using XmlIgnore = System.Xml.Serialization.XmlIgnoreAttribute;

namespace KSoft.Phoenix.Phx
{
	public enum BProtoTechEffectDisplayNameIconType
	{
		Unit,
		Building,
		Misc,
		Tech,
	};

	public enum BProtoTechEffectSetAgeLevel
	{
		Invalid = TypeExtensions.kNone,
		None = 0,

		Age1, // not explicitly parsed by the engine
		Age2,
		Age3,
		Age4,
	};

	public enum BProtoTechEffectType
	{
		Data,
		TransformUnit,
		TransformProtoUnit,
		TransformProtoSquad,
		Build,
		SetAge,
		GodPower,
		TechStatus,
		Ability,
		SharedLOS,
		AttachSquad,
	};

	public enum BProtoTechEffectTargetType
	{
		None = TypeExtensions.kNone,

		ProtoUnit,
		ProtoSquad,
		Unit,
		Tech,
		TechAll,
		Player,
	};

	public enum BProtoTechFlags
	{
		// 0x1C
		NoSound,// = 1<<0,
		[XmlIgnore] Forbid,// = 1<<1,
		Perpetual,// = 1<<2,
		OrPrereqs,// = 1<<3,
		Shadow,// = 1<<4,
		/// <summary>Tech applies to a unique, ie specific, unit</summary>
		UniqueProtoUnitInstance,// = 1<<5,
		Unobtainable,// = 1<<6,
		[XmlIgnore] OwnStaticData,// = 1<<7,

		// 0x1D
		Instant,// = 1<<7,

		// 0x78
		HiddenFromStats,// = 1<<0, // actually just appears to be a bool field
	};

	public enum BProtoTechStatus
	{
		Invalid = TypeExtensions.kNone,

		UnObtainable = 0,
		Obtainable,
		Available,
		Researching,
		Active,
		Disabled,
		CoopResearching,
	};

	public enum BProtoTechTypeCountOperator : short
	{
		e, // '0' isn't explicitly parsed
		gt,
		lt,
	};

	public enum BProtoTechAlphaMode
	{
		None = -1,
		Excluded = 0,
		AlphaOnly = 1,
	};
}