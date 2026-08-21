using System;

namespace KSoft.Phoenix.XML
{
	/// <summary>
	/// Lame hack for type value maps which store their type name in the InnerText and the value in a fucking attribute
	/// </summary>
	internal sealed class BTypeValuesSingleAttrHackXmlSerializer
		: BTypeValuesXmlSerializerBase<float>
	{
		readonly string kAttrName;

		public BTypeValuesSingleAttrHackXmlSerializer(BTypeValuesXmlParams<float> @params, Collections.BTypeValuesSingle list, string attributeName)
			: base(@params, list)
		{
			ArgumentNullException.ThrowIfNull(@params);
			ArgumentNullException.ThrowIfNull(list);
			ArgumentNullException.ThrowIfNull(attributeName);

			kAttrName = attributeName;
		}

		#region IXmlElementStreamable Members
		protected override void Read<TDoc, TCursor>(IO.TagElementStream<TDoc, TCursor, string> s, BXmlSerializerInterface xs, int iteration)
		{
			int index = ReadExplicitIndex(s, xs);

			ListExplicitIndex.InitializeItem(index);
			float value = 0;
			s.ReadAttribute(kAttrName, ref value);
			ListExplicitIndex[index] = value;
		}
		protected override void Write<TDoc, TCursor>(IO.TagElementStream<TDoc, TCursor, string> s, BXmlSerializerInterface xs, float data)
		{
			s.WriteAttribute(kAttrName, data);
		}
		#endregion
	};
}