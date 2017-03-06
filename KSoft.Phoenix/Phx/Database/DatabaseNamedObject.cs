
namespace KSoft.Phoenix.Phx
{
	public abstract class DatabaseNamedObject
		: Collections.BListAutoIdObject
	{
		#region Xml constants
		internal const string kXmlAttrName = "name";
		internal const string kXmlAttrNameN = "Name";
		#endregion

		#region DisplayNameID
		int mDisplayNameID = TypeExtensions.kNone;
		[Meta.LocStringReference]
		public int DisplayNameID
		{
			get { return mDisplayNameID; }
			set { mDisplayNameID = value; }
		}
		#endregion

		#region RolloverTextID
		int mRolloverTextID = TypeExtensions.kNone;
		[Meta.LocStringReference]
		public int RolloverTextID
		{
			get { return mRolloverTextID; }
			set { mRolloverTextID = value; }
		}
		#endregion

		#region PrereqTextID
		int mPrereqTextID = TypeExtensions.kNone;
		[Meta.LocStringReference]
		public int PrereqTextID
		{
			get { return mPrereqTextID; }
			set { mPrereqTextID = value; }
		}
		#endregion

		#region IXmlElementStreamable Members
		public override void Serialize<TDoc, TCursor>(IO.TagElementStream<TDoc, TCursor, string> s)
		{
			var xs = s.GetSerializerInterface();

			xs.StreamStringID(s, "DisplayNameID", ref mDisplayNameID);
			xs.StreamStringID(s, "RolloverTextID", ref mRolloverTextID);
			xs.StreamStringID(s, "PrereqTextID", ref mPrereqTextID);
		}
		#endregion
	};
}