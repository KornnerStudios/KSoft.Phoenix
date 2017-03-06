﻿
namespace KSoft.Phoenix.Phx
{
	// TODO: Nothing in HW uses this
	public sealed class BProtoTechPrereqTypeCount
		: IO.ITagElementStringNameStreamable
	{
		#region Xml constants
		public static readonly XML.BListXmlParams kBListXmlParams = new XML.BListXmlParams
		{
			ElementName = "TypeCount",
		};
		#endregion

		#region UnitID
		int mUnitID = TypeExtensions.kNone;
		[Meta.BProtoObjectReference]
		public int UnitID
		{
			get { return mUnitID; }
			set { mUnitID = value; }
		}
		#endregion

		#region Operator
		BProtoTechTypeCountOperator mOperator;
		public BProtoTechTypeCountOperator Operator
		{
			get { return mOperator; }
			set { mOperator = value; }
		}
		#endregion

		#region Count
		int mCount;
		public int Count
		{
			get { return mCount; }
			set { mCount = value; }
		}

		public const int cMaxCount = 2048;
		#endregion

		#region ITagElementStreamable<string> Members
		public void Serialize<TDoc, TCursor>(IO.TagElementStream<TDoc, TCursor, string> s)
			where TDoc : class
			where TCursor : class
		{
			var xs = s.GetSerializerInterface();

			xs.StreamDBID(s, "unit", ref mUnitID, DatabaseObjectKind.Object, false, XML.XmlUtil.kSourceAttr);
			s.StreamAttributeEnumOpt("operator", ref mOperator, e => e != BProtoTechTypeCountOperator.e);
			s.StreamAttributeOpt("count", ref mCount, Predicates.IsNotZero);
		}
		#endregion
	};
}