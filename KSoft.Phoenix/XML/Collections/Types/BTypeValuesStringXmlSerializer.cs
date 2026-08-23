using System;

namespace KSoft.Phoenix.XML
{
	partial class XmlUtil
	{
		public static void Serialize<TDoc, TCursor>(IO.TagElementStream<TDoc, TCursor, string> s,
			Collections.BTypeValuesString list, BTypeValuesXmlParams<string> @params)
			where TDoc : class
			where TCursor : class
		{
			ArgumentNullException.ThrowIfNull(s);
			ArgumentNullException.ThrowIfNull(list);
			ArgumentNullException.ThrowIfNull(@params);

			using (var xs = new BTypeValuesStringXmlSerializer(@params, list))
			{
				xs.Serialize(s);
			}
		}
	};

	internal sealed class BTypeValuesStringXmlSerializer
		: BTypeValuesXmlSerializerBase<string>
	{
		public BTypeValuesStringXmlSerializer(BTypeValuesXmlParams<string> @params, Collections.BTypeValuesString list) : base(@params, list)
		{
			ArgumentNullException.ThrowIfNull(@params);
			ArgumentNullException.ThrowIfNull(list);
		}

		#region IXmlElementStreamable Members
		protected override void Read<TDoc, TCursor>(IO.TagElementStream<TDoc, TCursor, string> s, BXmlSerializerInterface xs, int iteration)
		{
			int index = ReadExplicitIndex(s, xs);

			ListExplicitIndex.InitializeItem(index);
			string value = string.Empty;
			s.ReadCursor(ref value);
			ListExplicitIndex[index] = value;
		}
		protected override void Write<TDoc, TCursor>(IO.TagElementStream<TDoc, TCursor, string> s, BXmlSerializerInterface xs, string data)
		{
			s.WriteCursor(data);
		}
		#endregion
	};
}