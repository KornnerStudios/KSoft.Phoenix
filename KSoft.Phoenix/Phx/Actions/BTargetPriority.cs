using System;
using System.Collections.Generic;

namespace KSoft.Phoenix.Phx
{
	// TODO: change to struct?
	public sealed class BTargetPriority
		: IO.ITagElementStringNameStreamable
		, IEquatable<BTargetPriority>
		, IEqualityComparer<BTargetPriority> // #REPLACE with IEquatable<>
	{
		#region Xml constants
		public static readonly XML.BListXmlParams kBListXmlParams = new()
		{
			ElementName = "TargetPriority",
		};
		#endregion

		BProtoUnitID mUnitTypeID = TypeExtensions.kNone;
		[Meta.UnitReference]
		public BProtoUnitID UnitTypeID => mUnitTypeID;

		float mPriorityAdjustment = PhxUtil.kInvalidSingle;
		public float PriorityAdjustment => mPriorityAdjustment;

		#region ITagElementStreamable<string> Members
		public void Serialize<TDoc, TCursor>(IO.TagElementStream<TDoc, TCursor, string> s)
			where TDoc : class
			where TCursor : class
		{
			var xs = s.GetSerializerInterface();

			xs.StreamDBID(s, "type", ref mUnitTypeID, DatabaseObjectKind.Unit, false, XML.XmlUtil.kSourceAttr);
			// Optional, engine defaults to 0.0:
			s.StreamCursor(ref mPriorityAdjustment);
		}
		#endregion

		#region IEquatable<BTargetPriority> Members
		public bool Equals(BTargetPriority? other)
			=> other != null
				&& this.UnitTypeID == other.UnitTypeID
				&& this.PriorityAdjustment == other.PriorityAdjustment;

		public override bool Equals(object? obj)
			=> Equals(obj as BTargetPriority);

		public override int GetHashCode()
			=> HashCode.Combine(UnitTypeID, PriorityAdjustment);
		#endregion

		#region IEqualityComparer<BTargetPriority> Members
		public bool Equals(BTargetPriority? x, BTargetPriority? y)
		{
			return ReferenceEquals(x, y) || (x is not null && x.Equals(y));
		}

		public int GetHashCode(BTargetPriority obj)
		{
			return obj.GetHashCode();
		}
		#endregion
	};
}