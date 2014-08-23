
namespace KSoft.Phoenix.Phx
{
	public abstract class DatabasePurchasableObject
		: DatabaseNamedObject
	{
		#region Xml constants
		const string kXmlElementBuildPoints = "BuildPoints";
		const string kXmlElementResearchPoints = "ResearchPoints";
		#endregion

		XML.BTypeValuesXmlParams<float> mResourceCostXmlParams;

		public Collections.BTypeValuesSingle ResourceCost { get; private set; }

		float mBuildTime;
		public float BuildTime { get { return mBuildTime; } }
		float mResearchTime;
		public float ResearchTime { get { return mResearchTime; } }
		/// <summary>Time, in seconds, it takes to build or research this object</summary>
		public float PurchaseTime { get { return mBuildTime != PhxUtil.kGetInvalidSingle() ? mBuildTime : mResearchTime; } }

		protected DatabasePurchasableObject(Collections.BTypeValuesParams<float> rsrcCostParams, XML.BTypeValuesXmlParams<float> rsrcCostXmlParams)
		{
			mResourceCostXmlParams = rsrcCostXmlParams;

			ResourceCost = new Collections.BTypeValuesSingle(rsrcCostParams);

			mBuildTime = mResearchTime = PhxUtil.kGetInvalidSingle();
		}

		#region IXmlElementStreamable Members
		public override void Serialize<TDoc, TCursor>(IO.TagElementStream<TDoc, TCursor, string> s)
		{
			base.Serialize(s);

			XML.XmlUtil.Serialize(s, ResourceCost, mResourceCostXmlParams);
			s.StreamElementOpt(kXmlElementBuildPoints, ref mBuildTime, PhxPredicates.IsNotInvalid);
			s.StreamElementOpt(kXmlElementResearchPoints, ref mResearchTime, PhxPredicates.IsNotInvalid);
		}
		#endregion
	};
}