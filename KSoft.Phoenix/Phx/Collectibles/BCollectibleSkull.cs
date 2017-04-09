
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

		#region ObjectDBID
		int mObjectDBID = TypeExtensions.kNone;
		[Meta.BProtoObjectReference]
		public int ObjectDBID
		{
			get { return mObjectDBID; }
			set { mObjectDBID = value; }
		}
		#endregion

		public Collections.BListArray<BCollectibleSkullEffect> Effects { get; private set; }

		#region DisplayImageOn
		string mDisplayImageOn;
		[Meta.TextureReference]
		public string DisplayImageOn
		{
			get { return mDisplayImageOn; }
			set { mDisplayImageOn = value; }
		}
		#endregion

		#region DisplayImageOff
		string mDisplayImageOff;
		[Meta.TextureReference]
		public string DisplayImageOff
		{
			get { return mDisplayImageOff; }
			set { mDisplayImageOff = value; }
		}
		#endregion

		#region DisplayImageLocked
		string mDisplayImageLocked;
		[Meta.TextureReference]
		public string DisplayImageLocked
		{
			get { return mDisplayImageLocked; }
			set { mDisplayImageLocked = value; }
		}
		#endregion

		#region Hidden
		bool mHidden;
		public bool Hidden
		{
			get { return mHidden; }
			set { mHidden = value; }
		}
		#endregion

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
			s.StreamElementOpt("DisplayImageOn", ref mDisplayImageOn, Predicates.IsNotNullOrEmpty);
			s.StreamElementOpt("DisplayImageOff", ref mDisplayImageOff, Predicates.IsNotNullOrEmpty);
			s.StreamElementOpt("DisplayImageLocked", ref mDisplayImageLocked, Predicates.IsNotNullOrEmpty);
			s.StreamElementNamedFlag("Hidden", ref mHidden);
		}
		#endregion
	};
}