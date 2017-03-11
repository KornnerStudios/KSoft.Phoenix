
namespace KSoft.Phoenix.Phx
{
	public sealed class BProtoObjectTrainLimit
		: IO.ITagElementStringNameStreamable
	{
		public enum LimitType
		{
			Invalid = TypeExtensions.kNone,

			Unit,
			Squad,
		};

		#region Xml constants
		public static readonly XML.BListXmlParams kBListXmlParams = new XML.BListXmlParams
		{
			ElementName = "TrainLimit",
		};
		#endregion

		#region Type
		LimitType mType = LimitType.Invalid;
		public LimitType Type
		{
			get { return mType; }
			set { mType = value; }
		}
		#endregion

		#region ID
		int mID = TypeExtensions.kNone;
		public int ID
		{
			get { return mID; }
			set { mID = value; }
		}
		#endregion

		#region Count
		int mCount;
		public int Count
		{
			get { return mCount; }
			set { mCount = value; }
		}

		public bool IsCountValid { get { return Count >= byte.MinValue && Count < byte.MaxValue; } }
		#endregion

		#region Bucket
		int mBucket;
		public int Bucket
		{
			get { return mBucket; }
			set { mBucket = value; }
		}

		public bool IsBucketValid { get { return Bucket >= byte.MinValue && Bucket < byte.MaxValue; } }
		#endregion

		public bool IsValid { get {
			return Type != LimitType.Invalid
				&& ID.IsNotNone()
				&& IsCountValid
				&& IsBucketValid;
		} }

		#region ITagElementStreamable<string> Members
		public void Serialize<TDoc, TCursor>(IO.TagElementStream<TDoc, TCursor, string> s)
			where TDoc : class
			where TCursor : class
		{
			var xs = s.GetSerializerInterface();

			s.StreamAttributeEnum("Type", ref mType);

			switch (mType)
			{
				case LimitType.Unit:
					xs.StreamDBID(s, XML.XmlUtil.kNoXmlName, ref mID, DatabaseObjectKind.Object, false, XML.XmlUtil.kSourceCursor);
					break;
				case LimitType.Squad:
					xs.StreamDBID(s, XML.XmlUtil.kNoXmlName, ref mID, DatabaseObjectKind.Squad, false, XML.XmlUtil.kSourceCursor);
					break;
			}

			s.StreamAttributeOpt("Count", ref mCount, Predicates.IsNotZero);
			s.StreamAttributeOpt("Bucket", ref mBucket, Predicates.IsNotZero);
		}
		#endregion
	};
}