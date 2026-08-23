using System;

namespace KSoft.Phoenix.Phx
{
	public sealed class BProtoObjectSquadModeAnim
		: IO.ITagElementStringNameStreamable
		, IComparable<BProtoObjectSquadModeAnim>
		, IEquatable<BProtoObjectSquadModeAnim>
	{
		#region Xml constants
		public static readonly XML.BListXmlParams kBListXmlParams = new()
		{
			ElementName = "SquadModeAnim",
		};
		#endregion

		#region Mode
		BSquadMode mMode;
		public BSquadMode Mode
		{
			get { return mMode; }
			set { mMode = value; }
		}
		#endregion

		#region AnimType
		string? mAnimType;
		[Meta.BAnimTypeReference]
		public string? AnimType
		{
			get { return mAnimType; }
			set { mAnimType = value; }
		}
		#endregion

		#region ITagElementStreamable<string> Members
		public void Serialize<TDoc, TCursor>(IO.TagElementStream<TDoc, TCursor, string> s)
			where TDoc : class
			where TCursor : class
		{
			s.StreamAttributeEnum("Mode", ref mMode);
			string animType = s.IsReading
				? string.Empty
				: mAnimType ?? throw new InvalidOperationException("Animation type must be initialized before writing.");
			s.StreamCursor(ref animType);
			mAnimType = animType;
		}
		#endregion

		#region IComparable Members
		public int CompareTo(BProtoObjectSquadModeAnim? other)
		{
			if (other is null)
			{
				return 1;
			}

			if (Mode != other.Mode)
			{
				Mode.CompareTo(other.Mode);
			}

			return string.CompareOrdinal(AnimType, other.AnimType);
		}
		#endregion

		#region IEquatable Members
		public bool Equals(BProtoObjectSquadModeAnim? other)
			=> other is not null
				&& Mode == other.Mode
				&& AnimType == other.AnimType;

		public override bool Equals(object? obj)
			=> obj is BProtoObjectSquadModeAnim other && Equals(other);

		public override int GetHashCode()
			=> HashCode.Combine(Mode, AnimType);
		#endregion
	};
}