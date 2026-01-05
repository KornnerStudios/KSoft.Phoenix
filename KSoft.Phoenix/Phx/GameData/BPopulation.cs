using System;
using System.Collections.Generic;

namespace KSoft.Phoenix.Phx
{
	public struct BPopulation
		: IO.ITagElementStringNameStreamable
		, IComparable<BPopulation>
		, IEquatable<BPopulation>
		, IEqualityComparer<BPopulation> // #REPLACE with IEquatable<>
	{
		#region Xml constants
		public static readonly Collections.BTypeValuesParams<BPopulation> kBListParams =
			new(db => db.GameData.Populations)
			{
				kTypeGetInvalid = () => BPopulation.kInvalid
			};
		public static readonly XML.BTypeValuesXmlParams<BPopulation> kBListXmlParams =
			new("Pop", "Type");

		public static readonly Collections.BTypeValuesParams<float> kBListParamsSingle =
			new(db => db.GameData.Populations)
			{
				kTypeGetInvalid = PhxUtil.kGetInvalidSingle
			};
		public static readonly XML.BTypeValuesXmlParams<float> kBListXmlParamsSingle =
			new("Pop", "Type");
		public static readonly XML.BTypeValuesXmlParams<float> kBListXmlParamsSingle_LowerCase =
			new("Pop", "Type".ToLowerInvariant());
		public static readonly XML.BTypeValuesXmlParams<float> kBListXmlParamsSingle_CapAddition =
			new("PopCapAddition", "Type");
		#endregion

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE1006:Naming Styles")]
		private static BPopulation kInvalid => new(PhxUtil.kInvalidSingle, PhxUtil.kInvalidSingle);

		float mMax;
		public readonly float Max => mMax;

		float mCount;
		public readonly float Count => mCount;

		BPopulation(float max, float count) { mMax = max; mCount = count; }

		#region IComparable<T> Members
		readonly int IComparable<BPopulation>.CompareTo(BPopulation other)
		{
			if (this.Max == other.Max)
			{
				return this.Count.CompareTo(other.Count);
			}
			else
			{
				return this.Max.CompareTo(other.Max);
			}
		}
		#endregion

		#region IEquatable<BPopulation> Members
		public readonly bool Equals(BPopulation other)
			=> this.Max == other.Max && this.Count == other.Count;

		public override readonly bool Equals(object obj)
			=> obj is BPopulation population && Equals(population);

		public static bool operator ==(BPopulation left, BPopulation right)
		{
			return left.Equals(right);
		}

		public static bool operator !=(BPopulation left, BPopulation right)
		{
			return !(left == right);
		}

		public override readonly int GetHashCode()
			=> HashCode.Combine(Max, Count);
		#endregion

		#region IEqualityComparer<BPopulation> Members
		public readonly bool Equals(BPopulation x, BPopulation y)
		{
			return x.Equals(y);
		}

		public readonly int GetHashCode(BPopulation obj)
		{
			return obj.GetHashCode();
		}
		#endregion

		#region ITagElementStreamable<string> Members
		public void Serialize<TDoc, TCursor>(IO.TagElementStream<TDoc, TCursor, string> s)
			where TDoc : class
			where TCursor : class
		{
			s.StreamAttribute("Max", ref mMax);
			s.StreamCursor(ref mCount);
		}
		#endregion
	}
}