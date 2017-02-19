
namespace KSoft.Phoenix.Phx
{
	public abstract class DatabaseNamedObject
		: Collections.BListAutoIdObject
	{
		#region Xml constants
		internal const string kXmlAttrName = "name";
		internal const string kXmlAttrNameN = "Name";
		const string kXmlElementRolloverTextID = "RolloverTextID";
		const string kXmlElementPrereqTextID = "PrereqTextID";
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

		#region IXmlElementStreamable Members
		public override void Serialize<TDoc, TCursor>(IO.TagElementStream<TDoc, TCursor, string> s)
		{
			var xs = s.GetSerializerInterface();

			xs.StreamStringID(s, "DisplayNameID", ref mDisplayNameID);
		}
		#endregion
	};
}