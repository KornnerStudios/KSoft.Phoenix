
namespace KSoft.Phoenix.Phx
{
	public sealed class BUserClass
		: Collections.BListAutoIdObject
		, IDatabaseIdObject
	{
		#region Xml constants
		public static readonly XML.BListXmlParams kBListXmlParams = new XML.BListXmlParams("UserClass")
		{
			DataName = "Name",
			Flags = 0
		};
		public static readonly Engine.XmlFileInfo kXmlFileInfo = new Engine.XmlFileInfo
		{
			Directory = Engine.GameDirectory.Data,
			FileName = "UserClasses.xml",
			RootName = kBListXmlParams.RootName
		};

		const string kXmlAttrDbId = "DBID";
		#endregion

		int mDbId;
		public int DbId { get { return mDbId; } }

		public Collections.BListAutoId<BUserClassField> Fields { get; private set; }

		public BUserClass()
		{
			mDbId = TypeExtensions.kNone;
			Fields = new Collections.BListAutoId<BUserClassField>();
		}

		#region BListAutoIdObject Members
		public override void Serialize<TDoc, TCursor>(IO.TagElementStream<TDoc, TCursor, string> s)
		{
			s.StreamAttribute(kXmlAttrDbId, ref mDbId);
			XML.XmlUtil.Serialize(s, Fields, BUserClassField.kBListXmlParams);
		}
		#endregion
	};
}