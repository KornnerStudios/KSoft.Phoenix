
namespace KSoft.Phoenix.Phx
{
	public sealed class BResource
		: Collections.BListAutoIdObject
	{
		// fucking squads.xml and techs.xml uses a lower-case type name :|
		const bool kUseLowercaseCostTypeHack = true;

		#region Xml constants
		public static readonly XML.BListXmlParams kBListXmlParams = new("Resource",
			additionalFlags: XML.BCollectionXmlParamsFlags.DoNotWriteUndefinedData);

		public static readonly Collections.BTypeValuesParams<float> kBListTypeValuesParams =
			new(db => db.GameData.Resources) { kTypeGetInvalid = PhxUtil.kGetInvalidSingle };
		public static readonly XML.BTypeValuesXmlParams<float> kBListTypeValuesXmlParams =
			new("Resource", "Type");
		public static readonly XML.BTypeValuesXmlParams<float> kBListTypeValuesXmlParams_Cost =
			new("Cost", "ResourceType");
#pragma warning disable 0429
		public static readonly XML.BTypeValuesXmlParams<float> kBListTypeValuesXmlParams_CostLowercaseType = !kUseLowercaseCostTypeHack
			? kBListTypeValuesXmlParams_Cost
			: new("Cost", "ResourceType".ToLowerInvariant());
#pragma warning restore 0429
		public static readonly XML.BTypeValuesXmlParams<float> kBListTypeValuesXmlParams_AddResource =
			new("AddResource", null, XML.BCollectionXmlParamsFlags.UseInnerTextForData);
		#endregion

		bool mDeductable;
		public bool Deductable
		{
			get { return mDeductable; }
			set { mDeductable = value; }
		}

		public BResource() { }
		internal BResource(bool deductable) { mDeductable = deductable; }

		#region BListAutoIdObject Members
		public override void Serialize<TDoc, TCursor>(IO.TagElementStream<TDoc, TCursor, string> s)
		{
			s.StreamAttribute("Deductable", ref mDeductable);
		}
		#endregion
	};
}