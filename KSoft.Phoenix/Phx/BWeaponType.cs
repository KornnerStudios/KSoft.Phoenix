
namespace KSoft.Phoenix.Phx
{
	public sealed class BWeaponType
		: Collections.BListAutoIdObject
	{
		#region Xml constants
		public static readonly XML.BListXmlParams kBListXmlParams = new XML.BListXmlParams("WeaponType")
		{
			DataName = "Name",
			Flags = XML.BCollectionXmlParamsFlags.UseElementForData
		};
		public static readonly Engine.XmlFileInfo kXmlFileInfo = new Engine.XmlFileInfo
		{
			Directory = Engine.GameDirectory.Data,
			FileName = "WeaponTypes.xml",
			RootName = kBListXmlParams.RootName
		};

		//string DeathAnimation
		#endregion

		public Collections.BTypeValues<BDamageModifier> DamageModifiers { get; private set; }

		public BWeaponType()
		{
			DamageModifiers = new Collections.BTypeValues<BDamageModifier>(BDamageModifier.kBListParams);
		}

		#region BListObjectBase Members
		public override void Serialize<TDoc, TCursor>(IO.TagElementStream<TDoc, TCursor, string> s)
		{
			XML.XmlUtil.Serialize(s, DamageModifiers, BDamageModifier.kBListXmlParams);
		}
		#endregion
	};
}