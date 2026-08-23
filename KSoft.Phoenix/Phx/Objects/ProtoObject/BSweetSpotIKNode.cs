
namespace KSoft.Phoenix.Phx
{
	public sealed class BSweetSpotIKNode
		: IO.ITagElementStringNameStreamable
	{
		#region Xml constants
		public static readonly XML.BListXmlParams kBListXmlParams = new()
		{
			ElementName = "SweetSpotIK",
		};
		#endregion

		#region Name
		string? mName;
		public string? Name
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

		public bool LinkCountIsValid => LinkCount >= byte.MinValue && LinkCount <= byte.MaxValue;
		#endregion

		#region ITagElementStreamable<string> Members
		public void Serialize<TDoc, TCursor>(IO.TagElementStream<TDoc, TCursor, string> s)
			where TDoc : class
			where TCursor : class
		{
			string name = s.IsReading ? string.Empty : mName ?? throw new System.ArgumentNullException(nameof(mName));
			s.StreamCursor(ref name);
			mName = name;
			s.StreamAttribute("linkCount", ref mLinkCount);
		}
		#endregion
	};
}