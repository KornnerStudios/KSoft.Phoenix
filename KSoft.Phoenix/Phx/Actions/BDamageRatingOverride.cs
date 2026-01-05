using System;
using System.Collections.Generic;

namespace KSoft.Phoenix.Phx
{
	// TODO: change to a struct?
	public class BDamageRatingOverride
		: IO.ITagElementStringNameStreamable
		, IEquatable<BDamageRatingOverride>
		, IEqualityComparer<BDamageRatingOverride> // #REPLACE with IEquatable<>
	{
		#region Xml constants
		public static readonly Collections.BTypeValuesParams<BDamageRatingOverride> kBListParams =
			new(db => db.DamageTypes);
		public static readonly XML.BTypeValuesXmlParams<BDamageRatingOverride> kBListXmlParams =
			new("DamageRatingOverride", "type");
		#endregion

		float mRating = PhxUtil.kInvalidSingle;
		public float Rating { get { return mRating; } }

		float mHalfKillCutoffFactor = PhxUtil.kInvalidSingle;
		public float HalfKillCutoffFactor { get { return HalfKillCutoffFactor; } }

		#region ITagElementStreamable<string> Members
		public void Serialize<TDoc, TCursor>(IO.TagElementStream<TDoc, TCursor, string> s)
			where TDoc : class
			where TCursor : class
		{
			// Technically optional, engine defaults to 1.0
			s.StreamCursor(ref mRating);

			s.StreamAttributeOpt("halfKillCutoffFactor", ref mHalfKillCutoffFactor, PhxPredicates.IsNotInvalid);
		}
		#endregion

		#region IEquatable<BDamageRatingOverride> Members
		public bool Equals(BDamageRatingOverride other)
			=> other != null
				&& this.Rating == other.Rating
				&& this.HalfKillCutoffFactor == other.HalfKillCutoffFactor;

		public override bool Equals(object obj)
			=> Equals(obj as BDamageRatingOverride);

		public override int GetHashCode()
			=> HashCode.Combine(Rating, HalfKillCutoffFactor);
		#endregion

		#region IEqualityComparer<BDamageRatingOverride> Members
		public bool Equals(BDamageRatingOverride x, BDamageRatingOverride y)
		{
			return x.Equals(y);
		}

		public int GetHashCode(BDamageRatingOverride obj)
		{
			return obj.GetHashCode();
		}
		#endregion
	};
}