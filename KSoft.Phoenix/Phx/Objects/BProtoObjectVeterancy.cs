using System;
using System.Collections.Generic;

namespace KSoft.Phoenix.Phx
{
	public sealed class BProtoObjectVeterancy
		: IO.ITagElementStringNameStreamable
		, IComparable<BProtoObjectVeterancy>
		, IEqualityComparer<BProtoObjectVeterancy>
	{
		sealed class _EqualityComparer : IEqualityComparer<BProtoObjectVeterancy>
		{
			#region IEqualityComparer<BProtoObjectVeterancy> Members
			public bool Equals(BProtoObjectVeterancy x, BProtoObjectVeterancy y)
			{
				return x.XP == y.XP && x.Damage == y.Damage && x.Velocity == y.Velocity && x.Accuracy == y.Accuracy &&
					x.WorkRate == y.WorkRate && x.WeaponRange == y.WeaponRange && x.DamageTaken == y.DamageTaken;
			}

			public int GetHashCode(BProtoObjectVeterancy obj)
			{
				return obj.XP.GetHashCode() ^ obj.Damage.GetHashCode() ^ obj.Velocity.GetHashCode() ^ obj.Accuracy.GetHashCode() ^
					obj.WorkRate.GetHashCode() ^ obj.WeaponRange.GetHashCode() ^ obj.DamageTaken.GetHashCode();
			}
			#endregion
		};
		public static readonly IEqualityComparer<BProtoObjectVeterancy> kEqualityComparer = new _EqualityComparer();

		#region Constants
		static readonly BProtoObjectVeterancy kInvalid = new BProtoObjectVeterancy(),
			kDefaultLevel1 = new BProtoObjectVeterancy()
			{
				mDamage = 1.15f, mVelocity = 1, mAccuracy = 1.6f, mWorkRate = 1.2f, mWeaponRange = 1f, mDamageTaken = 0.87f
			},
			kDefaultLevel2 = new BProtoObjectVeterancy()
			{
				mDamage = 1.15f, mVelocity = 1, mAccuracy = 1.7f, mWorkRate = 1.2f, mWeaponRange = 1f, mDamageTaken = 0.80f
			},
			kDefaultLevel3 = new BProtoObjectVeterancy()
			{
				mDamage = 1.15f, mVelocity = 1, mAccuracy = 1.8f, mWorkRate = 1.2f, mWeaponRange = 1f, mDamageTaken = 0.74f
			},
			kDefaultLevel4 = new BProtoObjectVeterancy()
			{
				mDamage = 2.00f, mVelocity = 1, mAccuracy = 1.1f, mWorkRate = 2.0f, mWeaponRange = 1f, mDamageTaken = 0.50f
			},
			kDefaultLevel5 = new BProtoObjectVeterancy()
			{
				mDamage = 2.00f, mVelocity = 1, mAccuracy = 1.2f, mWorkRate = 2.0f, mWeaponRange = 1f, mDamageTaken = 0.50f
			};

		public static IEnumerable<BProtoObjectVeterancy> GetLevelDefaults()
		{
			yield return kDefaultLevel1;
			yield return kDefaultLevel2;
			yield return kDefaultLevel3;
			yield return kDefaultLevel4;
			yield return kDefaultLevel5;
		}
		#endregion

		#region Xml constants
		public static readonly Collections.BListExplicitIndexParams<BProtoObjectVeterancy> kBListExplicitIndexParams = new
			Collections.BListExplicitIndexParams<BProtoObjectVeterancy>(5)
			{
				// We use a zero'd instance as the invalid format
				// Game considers Vets with XP = 0 as 'null' basically
				kTypeGetInvalid = () => kInvalid
			};
		public static readonly XML.BListExplicitIndexXmlParams<BProtoObjectVeterancy> kBListExplicitIndexXmlParams = new
			XML.BListExplicitIndexXmlParams<BProtoObjectVeterancy>("Veterancy", "Level");

		const string kXmlAttrXP = "XP";
		const string kXmlAttrDamage = "Damage";
		const string kXmlAttrVelocity = "Velocity";
		const string kXmlAttrAccuracy = "Accuracy";
		const string kXmlAttrWorkRate = "WorkRate";
		const string kXmlAttrWeaponRange = "WeaponRange";
		const string kXmlAttrDamageTaken = "DamageTaken";
		#endregion

		#region Properties
		float mXP;
		public float XP { get { return mXP; } }
		float mDamage;
		public float Damage { get { return mDamage; } }
		float mVelocity;
		public float Velocity { get { return mVelocity; } }
		float mAccuracy;
		public float Accuracy { get { return mAccuracy; } }
		float mWorkRate;
		public float WorkRate { get { return mWorkRate; } }
		float mWeaponRange;
		public float WeaponRange { get { return mWeaponRange; } }
		float mDamageTaken;
		public float DamageTaken { get { return mDamageTaken; } }
		#endregion

		public bool IsInvalid { get { return object.ReferenceEquals(this, kInvalid); } }
		public bool IsNull { get { return mXP == 0.0f; } }

		#region IComparable<BProtoObjectVeterancy> Members
		int IComparable<BProtoObjectVeterancy>.CompareTo(BProtoObjectVeterancy other)
		{
			if (this.XP < other.XP) return -1;
			else if (this.XP > other.XP) return 1;

			return 0;
		}
		#endregion

		#region IEqualityComparer<BProtoObjectVeterancy> Members
		public bool Equals(BProtoObjectVeterancy x, BProtoObjectVeterancy y)
		{
			return kEqualityComparer.Equals(x, y);
		}

		public int GetHashCode(BProtoObjectVeterancy obj)
		{
			return kEqualityComparer.GetHashCode(obj);
		}
		#endregion

		#region ITagElementStreamable<string> Members
		public void Serialize<TDoc, TCursor>(IO.TagElementStream<TDoc, TCursor, string> s)
			where TDoc : class
			where TCursor : class
		{
			s.StreamAttributeOpt(kXmlAttrXP, ref mXP, Predicates.IsNotZero);
			s.StreamAttributeOpt(kXmlAttrDamage, ref mDamage, Predicates.IsNotZero);
			s.StreamAttributeOpt(kXmlAttrVelocity, ref mVelocity, Predicates.IsNotZero);
			s.StreamAttributeOpt(kXmlAttrAccuracy, ref mAccuracy, Predicates.IsNotZero);
			s.StreamAttributeOpt(kXmlAttrWorkRate, ref mWorkRate, Predicates.IsNotZero);
			s.StreamAttributeOpt(kXmlAttrWeaponRange, ref mWeaponRange, Predicates.IsNotZero);
			s.StreamAttributeOpt(kXmlAttrDamageTaken, ref mDamageTaken, Predicates.IsNotZero);
		}
		#endregion
	};
}