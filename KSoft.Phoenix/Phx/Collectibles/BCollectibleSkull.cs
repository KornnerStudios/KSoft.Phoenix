
namespace KSoft.Phoenix.Phx
{
	public sealed class BCollectibleSkull
		: DatabaseNamedObject
	{
		#region Xml constants
		public static readonly XML.BListXmlParams kBListXmlParams = new XML.BListXmlParams
		{
			ElementName = "Skull",
			DataName = DatabaseIdObject.kXmlAttrName,
		};

		const string kXmlAttrObjectDBID = "objectdbid";
		const string kXmlElementDescriptionID = "DescriptionID";

		const string kXmlElementHidden = "Hidden";
		#endregion

		int mObjectDBID = TypeExtensions.kNone;
		public Collections.BListArray<BCollectibleSkullEffect> Effects { get; private set; }
		int mDescriptionID = TypeExtensions.kNone;
		bool mHidden;

		public BCollectibleSkull()
		{
			Effects = new Collections.BListArray<BCollectibleSkullEffect>();
		}

		#region ITagElementStreamable<string> Members
		public override void Serialize<TDoc, TCursor>(IO.TagElementStream<TDoc, TCursor, string> s)
		{
			var xs = s.GetSerializerInterface();

			s.StreamAttribute(kXmlAttrObjectDBID, ref mObjectDBID);
			XML.XmlUtil.Serialize(s, Effects, BCollectibleSkullEffect.kBListXmlParams);
			xs.StreamStringID(s, kXmlElementDescriptionID, ref mDescriptionID);
			s.StreamElementNamedFlag(kXmlElementHidden, ref mHidden);
		}
		#endregion
	};
}