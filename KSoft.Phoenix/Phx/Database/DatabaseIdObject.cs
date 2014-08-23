
namespace KSoft.Phoenix.Phx
{
	public abstract class DatabaseIdObject
		: DatabasePurchasableObject
		, IDatabaseIdObject
	{
		#region Xml constants
		const string kXmlAttrDbId = "dbid";
		#endregion

		protected int mDbId;
		public int DbId { get { return mDbId; } }

		protected DatabaseIdObject(Collections.BTypeValuesParams<float> rsrcCostParams, XML.BTypeValuesXmlParams<float> rsrcCostXmlParams)
			: base(rsrcCostParams, rsrcCostXmlParams)
		{
			mDbId = TypeExtensions.kNone;
		}

		#region IXmlElementStreamable Members
		protected virtual void StreamDbId<TDoc, TCursor>(IO.TagElementStream<TDoc, TCursor, string> s)
			where TDoc : class
			where TCursor : class
		{
			s.StreamAttribute(kXmlAttrDbId, ref mDbId);
		}

		public override void Serialize<TDoc, TCursor>(IO.TagElementStream<TDoc, TCursor, string> s)
		{
			base.Serialize(s);

			StreamDbId(s);
		}
		#endregion
	};
}