
using XmlIgnore = System.Xml.Serialization.XmlIgnoreAttribute;

namespace KSoft.Phoenix.Phx
{
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
}