
using BVector = System.Numerics.Vector4;

namespace KSoft.Phoenix.Phx
{
	public sealed class BGroundIKNode
		: IO.ITagElementStringNameStreamable
	{
		#region Xml constants
		public static readonly XML.BListXmlParams kBListXmlParams = new XML.BListXmlParams
		{
			ElementName = "GroundIK",
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

		#region IKRange
		float mIKRange;
		public float IKRange
		{
			get { return mIKRange; }
			set { mIKRange = value; }
		}

		public bool IKRangeIsValid { get { return IKRange >= 0.0; } }
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

		#region AxisPositioning
		BVector mAxisPositioning;
		public BVector AxisPositioning
		{
			get { return mAxisPositioning; }
			set { mAxisPositioning = value; }
		}

		public bool OnLeft { get { return mAxisPositioning.X <= -1.0f; } }
		public bool OnRight { get { return mAxisPositioning.X >= +1.0f; } }

		public bool InFront { get { return mAxisPositioning.Z >= +1.0f; } }
		public bool InBack { get { return mAxisPositioning.Z <= -1.0f; } }
		#endregion

		#region ITagElementStreamable<string> Members
		public void Serialize<TDoc, TCursor>(IO.TagElementStream<TDoc, TCursor, string> s)
			where TDoc : class
			where TCursor : class
		{
			s.StreamCursor(ref mName);
			s.StreamAttribute("ikRange", ref mIKRange);
			s.StreamAttribute("linkCount", ref mLinkCount);
			s.StreamAttributeOpt("x", ref mAxisPositioning.X, Predicates.IsNotZero);
			s.StreamAttributeOpt("z", ref mAxisPositioning.Z, Predicates.IsNotZero);
		}
		#endregion
	};
}