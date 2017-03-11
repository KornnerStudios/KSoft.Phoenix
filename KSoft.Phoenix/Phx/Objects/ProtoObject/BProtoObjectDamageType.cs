using System;

namespace KSoft.Phoenix.Phx
{
	public sealed class BProtoObjectDamageType
		: IO.ITagElementStringNameStreamable
		, IComparable<BProtoObjectDamageType>
		, IEquatable<BProtoObjectDamageType>
	{
		#region Xml constants
		public static readonly XML.BListXmlParams kBListXmlParams = new XML.BListXmlParams
		{
			ElementName = "DamageType",
		};
		#endregion

		#region DamageType
		int mDamageType = TypeExtensions.kNone;
		[Meta.BDamageTypeReference]
		public int DamageType
		{
			get { return mDamageType; }
			set { mDamageType = value; }
		}
		#endregion

		#region Direction
		DamageDirection mDirection = DamageDirection.Invalid;
		public DamageDirection Direction
		{
			get { return mDirection; }
			set { mDirection = value; }
		}
		#endregion

		#region Mode
		BSquadMode mMode = BSquadMode.Normal;
		public BSquadMode Mode
		{
			get { return mMode; }
			set { mMode = value; }
		}
		#endregion

		#region ITagElementStreamable<string> Members
		public void Serialize<TDoc, TCursor>(IO.TagElementStream<TDoc, TCursor, string> s)
			where TDoc : class
			where TCursor : class
		{
			var xs = s.GetSerializerInterface();

			xs.StreamDBID(s, XML.XmlUtil.kNoXmlName, ref mDamageType, DatabaseObjectKind.DamageType, isOptional: false, xmlSource: XML.XmlUtil.kSourceCursor);
			s.StreamAttributeEnum("direction", ref mDirection);
			s.StreamAttributeEnumOpt("mode", ref mMode, e => e != BSquadMode.Normal);
		}
		#endregion

		#region IComparable Members
		public int CompareTo(BProtoObjectDamageType other)
		{
			if (DamageType != other.DamageType)
				DamageType.CompareTo(other.DamageType);

			if (Direction != other.Direction)
				Direction.CompareTo(other.Direction);

			return Mode.CompareTo(other.Mode);
		}
		#endregion

		#region IEquatable Members
		public bool Equals(BProtoObjectDamageType other)
		{
			return DamageType == other.DamageType
				&& Direction == other.Direction
				&& Mode == other.Mode;
		}
		#endregion
	};
}