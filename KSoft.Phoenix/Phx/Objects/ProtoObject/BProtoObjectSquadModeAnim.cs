using System;

namespace KSoft.Phoenix.Phx
{
	public sealed class BProtoObjectSquadModeAnim
		: IO.ITagElementStringNameStreamable
		, IComparable<BProtoObjectSquadModeAnim>
		, IEquatable<BProtoObjectSquadModeAnim>
	{
		#region Xml constants
		public static readonly XML.BListXmlParams kBListXmlParams = new XML.BListXmlParams
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
		string mAnimType;
		[Meta.BAnimTypeReference]
		public string AnimType
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
			s.StreamCursor(ref mAnimType);
		}
		#endregion

		#region IComparable Members
		public int CompareTo(BProtoObjectSquadModeAnim other)
		{
			if (Mode != other.Mode)
				Mode.CompareTo(other.Mode);

			return AnimType.CompareTo(other.AnimType);
		}
		#endregion

		#region IEquatable Members
		public bool Equals(BProtoObjectSquadModeAnim other)
		{
			return Mode == other.Mode
				&& AnimType == other.AnimType;
		}
		#endregion
	};
}