
namespace KSoft.Phoenix.Phx
{
	public sealed class BWeaponType
		: Collections.BListAutoIdObject
	{
		#region Xml constants
		public static readonly XML.BListXmlParams kBListXmlParams = new("WeaponType")
		{
			DataName = "Name",
			Flags = XML.BCollectionXmlParamsFlags.UseElementForData |
					XML.BCollectionXmlParamsFlags.RequiresDataNamePreloading
		};
		public static readonly Engine.XmlFileInfo kXmlFileInfo = new()
		{
			Directory = Engine.GameDirectory.Data,
			FileName = "WeaponTypes.xml",
			RootName = kBListXmlParams.RootName
		};
		public static readonly Engine.ProtoDataXmlFileInfo kProtoFileInfo = new(
			Engine.XmlFilePriority.Lists,
			kXmlFileInfo);
		#endregion

		#region DeathAnimation
		string? mDeathAnimation;
		public string? DeathAnimation
		{
			get { return mDeathAnimation; }
			set { mDeathAnimation = value; }
		}
		#endregion

		// The Modifiers list requires BWeaponType names!
		[Meta.BWeaponTypeReference]
		public Collections.BTypeValues<BWeaponModifier> Modifiers { get; private set; }

		public BWeaponType()
		{
			Modifiers = new Collections.BTypeValues<BWeaponModifier>(BWeaponModifier.kBListParams);
		}

		#region BListObjectBase Members
		public override void Serialize<TDoc, TCursor>(IO.TagElementStream<TDoc, TCursor, string> s)
		{
			s.StreamElementOpt("DeathAnimation", ref mDeathAnimation, Predicates.IsNotNullOrEmpty);

			XML.XmlUtil.Serialize(s, Modifiers, BWeaponModifier.kBListXmlParams);
		}
		#endregion
	};
}