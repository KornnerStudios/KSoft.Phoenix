using System;
using System.Collections.Generic;

namespace KSoft.Phoenix.Phx
{
	public sealed class BProtoObjectVeterancy
		: IO.ITagElementStringNameStreamable
		, IComparable<BProtoObjectVeterancy>
		, IEquatable<BProtoObjectVeterancy>
	{
		#region Constants
		static readonly BProtoObjectVeterancy kInvalid = new(),
			kDefaultLevel1 = new()
			{
				mDamage = 1.15f, mVelocity = 1, mAccuracy = 1.6f, mWorkRate = 1.2f, mWeaponRange = 1f, mDamageTaken = 0.87f
			},
			kDefaultLevel2 = new()
			{
				mDamage = 1.15f, mVelocity = 1, mAccuracy = 1.7f, mWorkRate = 1.2f, mWeaponRange = 1f, mDamageTaken = 0.80f
			},
			kDefaultLevel3 = new()
			{
				mDamage = 1.15f, mVelocity = 1, mAccuracy = 1.8f, mWorkRate = 1.2f, mWeaponRange = 1f, mDamageTaken = 0.74f
			},
			kDefaultLevel4 = new()
			{
				mDamage = 2.00f, mVelocity = 1, mAccuracy = 1.1f, mWorkRate = 2.0f, mWeaponRange = 1f, mDamageTaken = 0.50f
			},
			kDefaultLevel5 = new()
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
		public static readonly Collections.BListExplicitIndexParams<BProtoObjectVeterancy> kBListExplicitIndexParams =
			new(5)
			{
				// We use a zero'd instance as the invalid format
				// Game considers Vets with XP = 0 as 'null' basically
				kTypeGetInvalid = () => kInvalid
			};
		public static readonly XML.BListExplicitIndexXmlParams<BProtoObjectVeterancy> kBListExplicitIndexXmlParams =
			new("Veterancy", "Level");
		#endregion

		#region Properties
		float mXP;
		public float XP { get { return mXP; } }
		float mDamage = 1.0f;
		public float Damage { get { return mDamage; } }
		float mVelocity = 1.0f;
		public float Velocity { get { return mVelocity; } }
		float mAccuracy = 1.0f;
		public float Accuracy { get { return mAccuracy; } }
		float mWorkRate = 1.0f;
		public float WorkRate { get { return mWorkRate; } }
		float mWeaponRange = 1.0f;
		public float WeaponRange { get { return mWeaponRange; } }
		float mDamageTaken = 1.0f;
		public float DamageTaken { get { return mDamageTaken; } }
		#endregion

		public bool IsInvalid => object.ReferenceEquals(this, kInvalid);
		public bool IsIgnored => mXP == 0.0f;

		#region IComparable Members
		int IComparable<BProtoObjectVeterancy>.CompareTo(BProtoObjectVeterancy other)
		{
			if (XP != other.XP)
			{
				return XP.CompareTo(other.XP);
			}

			if (Damage != other.Damage)
			{
				return Damage.CompareTo(other.Damage);
			}

			if (Velocity != other.Velocity)
			{
				return Velocity.CompareTo(other.Velocity);
			}

			if (Accuracy != other.Accuracy)
			{
				return Accuracy.CompareTo(other.Accuracy);
			}

			if (WorkRate != other.WorkRate)
			{
				return WorkRate.CompareTo(other.WorkRate);
			}

			if (WeaponRange != other.WeaponRange)
			{
				return WeaponRange.CompareTo(other.WeaponRange);
			}

			if (DamageTaken != other.DamageTaken)
			{
				return DamageTaken.CompareTo(other.DamageTaken);
			}

			return 0;
		}
		#endregion

		#region IEquatable<BProtoObjectVeterancy> Members
		public bool Equals(BProtoObjectVeterancy other)
			=> other != null
				&& this.XP == other.XP
				&& this.Damage == other.Damage
				&& this.Velocity == other.Velocity
				&& this.Accuracy == other.Accuracy
				&& this.WorkRate == other.WorkRate
				&& this.WeaponRange == other.WeaponRange
				&& this.DamageTaken == other.DamageTaken;

		public override bool Equals(object obj)
			=> Equals(obj as BProtoObjectVeterancy);

		public override int GetHashCode()
			=> HashCode.Combine(XP,
				Damage,
				Velocity,
				Accuracy,
				WorkRate,
				WeaponRange,
				DamageTaken);
		#endregion

		#region ITagElementStreamable<string> Members
		public void Serialize<TDoc, TCursor>(IO.TagElementStream<TDoc, TCursor, string> s)
			where TDoc : class
			where TCursor : class
		{
			s.StreamAttributeOpt("XP", ref mXP, Predicates.IsNotZero);
			s.StreamAttributeOpt("Damage", ref mDamage, PhxPredicates.IsNotOne);
			s.StreamAttributeOpt("Velocity", ref mVelocity, PhxPredicates.IsNotOne);
			s.StreamAttributeOpt("Accuracy", ref mAccuracy, PhxPredicates.IsNotOne);
			s.StreamAttributeOpt("WorkRate", ref mWorkRate, PhxPredicates.IsNotOne);
			s.StreamAttributeOpt("WeaponRange", ref mWeaponRange, PhxPredicates.IsNotOne);
			s.StreamAttributeOpt("DamageTaken", ref mDamageTaken, PhxPredicates.IsNotOne);
		}
		#endregion
	};
}