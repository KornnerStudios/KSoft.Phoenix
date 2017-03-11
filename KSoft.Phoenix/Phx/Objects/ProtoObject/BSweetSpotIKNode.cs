
namespace KSoft.Phoenix.Phx
{
	public sealed class BSweetSpotIKNode
		: IO.ITagElementStringNameStreamable
	{
		#region Xml constants
		public static readonly XML.BListXmlParams kBListXmlParams = new XML.BListXmlParams
		{
			ElementName = "SweetSpotIK",
		};
		#endregion

		#region Name
		string mName;
		public string Name
		{
			get { return mName; }
			set { mName = value; }
		}
		#endregion

		#region LinkCount
		int mLinkCount;
		public int LinkCount
		{
			get { return mLinkCount; }
			set { mLinkCount = value; }
		}

		public bool LinkCountIsValid { get { return LinkCount >= byte.MinValue && LinkCount <= byte.MaxValue; } }
		#endregion

		#region ITagElementStreamable<string> Members
		public void Serialize<TDoc, TCursor>(IO.TagElementStream<TDoc, TCursor, string> s)
			where TDoc : class
			where TCursor : class
		{
			s.StreamCursor(ref mName);
			s.StreamAttribute("linkCount", ref mLinkCount);
		}
		#endregion
	};
}