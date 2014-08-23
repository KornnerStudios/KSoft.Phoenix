using Contracts = System.Diagnostics.Contracts;
using Contract = System.Diagnostics.Contracts.Contract;

namespace KSoft.Phoenix.XML
{
	public sealed class BBitSetXmlParams
		: BListXmlParams
	{
		/// <summary>Sets ElementName, defaults to InnerText data usage and data interning</summary>
		/// <param name="elementName">Name of the xml element which represents the type (enum) value</param>
		public BBitSetXmlParams(string elementName)
		{
			ElementName = elementName;

			Flags = 0;
			Flags |= BCollectionXmlParamsFlags.UseInnerTextForData;
			Flags |= BCollectionXmlParamsFlags.InternDataNames;
		}

		public static readonly BBitSetXmlParams kFlagsSansRoot = new BBitSetXmlParams("Flag");
	};
}