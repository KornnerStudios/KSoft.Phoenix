
namespace KSoft.Phoenix.Phx
{
	public abstract class DatabasePurchasableObject
		: DatabaseNamedObject
	{
		XML.BTypeValuesXmlParams<float> mResourceCostXmlParams;

		public Collections.BTypeValuesSingle ResourceCost { get; private set; }

		#region BuildTime
		float mBuildTime = PhxUtil.kInvalidSingle;
		public float BuildTime
		{
			get { return mBuildTime; }
			set { mBuildTime = value; }
		}
		#endregion

		#region ResearchTime
		float mResearchTime = PhxUtil.kInvalidSingle;
		public float ResearchTime
		{
			get { return mResearchTime; }
			set { mResearchTime = value; }
		}
		#endregion

		/// <summary>Time, in seconds, it takes to build or research this object</summary>
		public float PurchaseTime { get {
			return mBuildTime != PhxUtil.kInvalidSingle
				? mBuildTime
				: mResearchTime;
		} }

		protected DatabasePurchasableObject(Collections.BTypeValuesParams<float> rsrcCostParams, XML.BTypeValuesXmlParams<float> rsrcCostXmlParams)
		{
			mResourceCostXmlParams = rsrcCostXmlParams;

			ResourceCost = new Collections.BTypeValuesSingle(rsrcCostParams);
		}

		#region IXmlElementStreamable Members
		public override void Serialize<TDoc, TCursor>(IO.TagElementStream<TDoc, TCursor, string> s)
		{
			base.Serialize(s);

			XML.XmlUtil.Serialize(s, ResourceCost, mResourceCostXmlParams);
			s.StreamElementOpt("BuildPoints", ref mBuildTime, PhxPredicates.IsNotInvalid);
			s.StreamElementOpt("ResearchPoints", ref mResearchTime, PhxPredicates.IsNotInvalid);
		}
		#endregion
	};
}