
namespace KSoft.Phoenix.Phx
{
	public abstract class DatabaseNamedObject
		: Collections.BListAutoIdObject
	{
		#region Xml constants
		internal const string kXmlAttrName = "name";
		internal const string kXmlAttrNameN = "Name";
		const string kXmlElementDisplayName = "DisplayNameID";
		const string kXmlElementRolloverTextID = "RolloverTextID";
		const string kXmlElementPrereqTextID = "PrereqTextID";
		#endregion

		int mDisplayNameID;
		public int DisplayNameID { get { return mDisplayNameID; } }

		protected DatabaseNamedObject()
		{
			mDisplayNameID = TypeExtensions.kNone;
		}

		#region IXmlElementStreamable Members
		public override void Serialize<TDoc, TCursor>(IO.TagElementStream<TDoc, TCursor, string> s)
		{
			var xs = s.GetSerializerInterface();

			xs.StreamStringID(s, kXmlElementDisplayName, ref mDisplayNameID);
		}
		#endregion
	};
}