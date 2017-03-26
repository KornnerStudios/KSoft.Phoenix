using System;
using System.Collections.Generic;

namespace KSoft.Phoenix.Phx
{
	public sealed class BWeaponModifier
		: IO.ITagElementStringNameStreamable
		, IComparable<BWeaponModifier>
		, IEqualityComparer<BWeaponModifier>
	{
		#region Xml constants
		public static readonly Collections.BTypeValuesParams<BWeaponModifier> kBListParams = new
			Collections.BTypeValuesParams<BWeaponModifier>(db => db.DamageTypes);
		public static readonly XML.BTypeValuesXmlParams<BWeaponModifier> kBListXmlParams = new
			XML.BTypeValuesXmlParams<BWeaponModifier>("DamageModifier", "type");
		#endregion

		#region Rating
		float mRating = 1.0f;
		public float Rating
		{
			get { return mRating; }
			set { mRating = value; }
		}
		#endregion

		#region DamagePercentage
		float mDamagePercentage = 1.0f;
		public float DamagePercentage
		{
			get { return mDamagePercentage; }
		}
		#endregion

		#region ReflectDamageFactor
		float mReflectDamageFactor;
		public float ReflectDamageFactor
		{
			get { return mReflectDamageFactor; }
			set { mReflectDamageFactor = value; }
		}
		#endregion

		#region Bowlable
		bool mBowlable;
		public bool Bowlable
		{
			get { return mBowlable; }
			set { mBowlable = value; }
		}
		#endregion

		#region Rammable
		bool mRammable;
		public bool Rammable
		{
			get { return mRammable; }
			set { mRammable = value; }
		}
		#endregion

		#region ITagElementStreamable<string> Members
		public void Serialize<TDoc, TCursor>(IO.TagElementStream<TDoc, TCursor, string> s)
			where TDoc : class
			where TCursor : class
		{
			s.StreamAttributeOpt("rating", ref mRating, PhxPredicates.IsNotOne);
			s.StreamCursor(ref mDamagePercentage);
			s.StreamAttributeOpt("reflectDamageFactor", ref mReflectDamageFactor, Predicates.IsNotZero);
			s.StreamAttributeOpt("bowlable", ref mBowlable, Predicates.IsTrue);
			s.StreamAttributeOpt("rammable", ref mRammable, Predicates.IsTrue);
		}
		#endregion

		#region IComparable<BDamageModifier> Members
		public int CompareTo(BWeaponModifier other)
		{
			if (Rating != other.Rating)
				return Rating.CompareTo(other.Rating);

			return DamagePercentage.CompareTo(other.DamagePercentage);
		}
		#endregion

		#region IEqualityComparer<BDamageModifier> Members
		public bool Equals(BWeaponModifier x, BWeaponModifier y)
		{
			return x.Rating == y.Rating
				&& x.DamagePercentage == y.DamagePercentage;
		}

		public int GetHashCode(BWeaponModifier obj)
		{
			return obj.Rating.GetHashCode() ^ obj.DamagePercentage.GetHashCode();
		}
		#endregion
	};
}