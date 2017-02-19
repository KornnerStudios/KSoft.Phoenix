using System;
using System.Collections.Generic;

namespace KSoft.Phoenix.Phx
{
	public struct BDamageModifier
		: IO.ITagElementStringNameStreamable
		, IComparable<BDamageModifier>
		, IEqualityComparer<BDamageModifier>
	{
		#region Xml constants
		public static readonly Collections.BTypeValuesParams<BDamageModifier> kBListParams = new
			Collections.BTypeValuesParams<BDamageModifier>(db => db.DamageTypes);
		public static readonly XML.BTypeValuesXmlParams<BDamageModifier> kBListXmlParams = new
			XML.BTypeValuesXmlParams<BDamageModifier>("DamageModifier", "type");
		#endregion

		float mRating;
		public float Rating { get { return mRating; } }
		float mValue;
		public float Value { get { return mValue; } }

		#region ITagElementStreamable<string> Members
		public void Serialize<TDoc, TCursor>(IO.TagElementStream<TDoc, TCursor, string> s)
			where TDoc : class
			where TCursor : class
		{
			s.StreamAttribute("rating", ref mRating);
			//float reflectDamageFactor
			//bool bowlable, rammable
			s.StreamCursor(ref mValue);
		}
		#endregion

		#region IComparable<BDamageModifier> Members
		public int CompareTo(BDamageModifier other)
		{
			if (this.Rating == other.Rating)
				return this.Rating.CompareTo(other.Rating);
			else
				return this.Value.CompareTo(other.Value);
		}
		#endregion

		#region IEqualityComparer<BDamageModifier> Members
		public bool Equals(BDamageModifier x, BDamageModifier y)
		{
			return x.Rating == y.Rating && x.Value == y.Value;
		}

		public int GetHashCode(BDamageModifier obj)
		{
			return obj.Rating.GetHashCode() ^ obj.Value.GetHashCode();
		}
		#endregion
	};
}