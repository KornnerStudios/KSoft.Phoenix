
namespace KSoft.Phoenix.Phx
{
	public sealed class BProtoSkull
		: DatabaseNamedObject
	{
		#region Xml constants
		public static readonly XML.BListXmlParams kBListXmlParams = new XML.BListXmlParams
		{
			ElementName = "Skull",
			DataName = DatabaseIdObject.kXmlAttrName,
		};
		#endregion

		int mObjectDBID = TypeExtensions.kNone;
		public Collections.BListArray<BCollectibleSkullEffect> Effects { get; private set; }
		int mDescriptionID = TypeExtensions.kNone;
		bool mHidden;

		public BProtoSkull()
		{
			var textData = base.CreateDatabaseObjectUserInterfaceTextData();
			textData.HasDescriptionID = true;
			textData.HasDisplayNameID = true;

			Effects = new Collections.BListArray<BCollectibleSkullEffect>();
		}

		#region ITagElementStreamable<string> Members
		public override void Serialize<TDoc, TCursor>(IO.TagElementStream<TDoc, TCursor, string> s)
		{
			var xs = s.GetSerializerInterface();

			s.StreamAttribute("objectdbid", ref mObjectDBID);
			XML.XmlUtil.Serialize(s, Effects, BCollectibleSkullEffect.kBListXmlParams);
			//DisplayImageOn
			//DisplayImageOff
			//DisplayImageLocked
			s.StreamElementNamedFlag("Hidden", ref mHidden);
		}
		#endregion
	};
}