using System.Collections.Generic;

namespace KSoft.Phoenix.Phx
{
	// TODO: change to struct?
	public sealed class BTargetPriority
		: IO.ITagElementStringNameStreamable
		, IEqualityComparer<BTargetPriority>
	{
		#region Xml constants
		public static readonly XML.BListXmlParams kBListXmlParams = new XML.BListXmlParams
		{
			ElementName = "TargetPriority",
		};

		const string kXmlAttrType = "type"; // proto unit or object type
		#endregion

		int mUnitTypeID = TypeExtensions.kNone;
		public int UnitTypeID { get { return mUnitTypeID; } }
		float mPriority = PhxUtil.kInvalidSingle;
		public float Priority { get { return mPriority; } }

		#region ITagElementStreamable<string> Members
		public void Serialize<TDoc, TCursor>(IO.TagElementStream<TDoc, TCursor, string> s)
			where TDoc : class
			where TCursor : class
		{
			var xs = s.GetSerializerInterface();

			xs.StreamDBID(s, kXmlAttrType, ref mUnitTypeID, DatabaseObjectKind.Unit, false, XML.XmlUtil.kSourceAttr);
			s.StreamCursor(ref mPriority);
		}
		#endregion

		#region IEqualityComparer<BTargetPriority> Members
		public bool Equals(BTargetPriority x, BTargetPriority y)
		{
			return x.UnitTypeID == y.UnitTypeID && x.Priority == y.Priority;
		}

		public int GetHashCode(BTargetPriority obj)
		{
			return obj.UnitTypeID.GetHashCode() ^ obj.Priority.GetHashCode();
		}
		#endregion
	};
}