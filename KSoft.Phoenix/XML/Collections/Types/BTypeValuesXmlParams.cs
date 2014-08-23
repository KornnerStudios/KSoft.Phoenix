using System;
using Contracts = System.Diagnostics.Contracts;
using Contract = System.Diagnostics.Contracts.Contract;

namespace KSoft.Phoenix.XML
{
	public class BTypeValuesXmlParams<T>
		: BListExplicitIndexXmlParams<T>
	{
		/// <summary>Sets ElementName and DataName (which defaults to XML attribute usage)</summary>
		/// <param name="elementName"></param>
		/// <param name="typeName">Name of the xml node which represents the type (enum) value</param>
		public BTypeValuesXmlParams(string elementName, string typeName, BCollectionXmlParamsFlags flags = 0)
		{
			ElementName = elementName;
			DataName = typeName;
			Flags = flags;
		}
	};
}