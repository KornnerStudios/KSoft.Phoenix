using Contracts = System.Diagnostics.Contracts;

namespace KSoft.Phoenix.XML
{
	public class BListXmlParams : BCollectionXmlParams
	{
		public /*readonly*/ string DataName;

		#region Flags
		[Contracts.Pure]
		public bool InternDataNames { get { return HasFlag(BCollectionXmlParamsFlags.InternDataNames); } }
		[Contracts.Pure]
		public bool UseInnerTextForData { get { return HasFlag(BCollectionXmlParamsFlags.UseInnerTextForData); } }
		[Contracts.Pure]
		public bool UseElementForData { get { return HasFlag(BCollectionXmlParamsFlags.UseElementForData); } }
		[Contracts.Pure]
		public bool ToLowerDataNames { get { return HasFlag(BCollectionXmlParamsFlags.ToLowerDataNames); } }
		[Contracts.Pure]
		public bool RequiresDataNamePreloading { get { return HasFlag(BCollectionXmlParamsFlags.RequiresDataNamePreloading); } }
		[Contracts.Pure]
		public bool SupportsUpdating { get { return HasFlag(BCollectionXmlParamsFlags.SupportsUpdating); } }
		[Contracts.Pure]
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