namespace KSoft.Phoenix.XML
{
	public class BListXmlParams : BCollectionXmlParams
	{
		public /*readonly*/ string? DataName;

		#region Flags
		public bool InternDataNames { get { return HasFlag(BCollectionXmlParamsFlags.InternDataNames); } }
		public bool UseInnerTextForData { get { return HasFlag(BCollectionXmlParamsFlags.UseInnerTextForData); } }
		public bool UseElementForData { get { return HasFlag(BCollectionXmlParamsFlags.UseElementForData); } }
		public bool ToLowerDataNames { get { return HasFlag(BCollectionXmlParamsFlags.ToLowerDataNames); } }
		public bool RequiresDataNamePreloading { get { return HasFlag(BCollectionXmlParamsFlags.RequiresDataNamePreloading); } }
		public bool SupportsUpdating { get { return HasFlag(BCollectionXmlParamsFlags.SupportsUpdating); } }
		public bool DoNotWriteUndefinedData { get { return HasFlag(BCollectionXmlParamsFlags.DoNotWriteUndefinedData); } }
		#endregion

		public BListXmlParams() { }
		/// <summary>Sets RootName to plural of ElementName and sets UseInnerTextForData</summary>
		/// <param name="elementName"></param>
		/// <param name="additionalFlags"></param>
		public BListXmlParams(string elementName, BCollectionXmlParamsFlags additionalFlags = 0) : base(elementName)
		{
			Flags = additionalFlags;
			Flags |= BCollectionXmlParamsFlags.UseInnerTextForData;
		}

		public void StreamDataName<TDoc, TCursor>(IO.TagElementStream<TDoc, TCursor, string> s, ref string name)
			where TDoc : class
			where TCursor : class
		{
			BCollectionXmlParams.StreamValue(s, DataName, ref name,
				UseInnerTextForData, UseElementForData, InternDataNames,
				false/*ToLowerDataNames*/);
		}
	};
}