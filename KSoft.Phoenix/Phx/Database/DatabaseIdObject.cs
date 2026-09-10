
namespace KSoft.Phoenix.Phx
{
	public abstract partial class DatabaseIdObject
		: DatabasePurchasableObject
		, IDatabaseIdObject
	{
		#region DBID
		private int mDbId = TypeExtensions.kNone;
		[KSoft.PropertyChanged.SourceGeneration.GeneratedPropertyChangedEventArgs]
		public int DbId
		{
			get { return mDbId; }
			set { this.SetFieldVal(ref mDbId, value, kDbIdChangedEventArgs); }
		}
		#endregion

		protected DatabaseIdObject(Collections.BTypeValuesParams<float> rsrcCostParams, XML.BTypeValuesXmlParams<float> rsrcCostXmlParams)
			: base(rsrcCostParams, rsrcCostXmlParams)
		{
		}

		#region IXmlElementStreamable Members
		protected virtual void StreamDbId<TDoc, TCursor>(IO.TagElementStream<TDoc, TCursor, string> s)
			where TDoc : class
			where TCursor : class
		{
			s.StreamAttribute("dbid", this, obj => obj.DbId);
		}

		public override void Serialize<TDoc, TCursor>(IO.TagElementStream<TDoc, TCursor, string> s)
		{
			base.Serialize(s);

			StreamDbId(s);
		}
		#endregion
	};
}