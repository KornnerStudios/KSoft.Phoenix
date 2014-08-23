using System;
using Contracts = System.Diagnostics.Contracts;
using Contract = System.Diagnostics.Contracts.Contract;

namespace KSoft.Phoenix.XML
{
	partial class XmlUtil
	{
		public static void Serialize<TDoc, TCursor>(IO.TagElementStream<TDoc, TCursor, string> s,
			Collections.BTypeValuesInt32 list, BTypeValuesXmlParams<int> @params)
			where TDoc : class
			where TCursor : class
		{
			Contract.Requires(s != null);
			Contract.Requires(list != null);
			Contract.Requires(@params != null);

			using (var xs = new BTypeValuesInt32XmlSerializer(@params, list))
			{
				xs.Serialize(s);
			}
		}
	};

	internal sealed class BTypeValuesInt32XmlSerializer : BTypeValuesXmlSerializerBase<int>
	{
		public BTypeValuesInt32XmlSerializer(BTypeValuesXmlParams<int> @params, Collections.BTypeValuesInt32 list) : base(@params, list)
		{
			Contract.Requires<ArgumentNullException>(@params != null);
			Contract.Requires<ArgumentNullException>(list != null);
		}

		#region IXmlElementStreamable Members
		protected override void Read<TDoc, TCursor>(IO.TagElementStream<TDoc, TCursor, string> s, BXmlSerializerInterface xs, int iteration)
		{
			int index = ReadExplicitIndex(s, xs);

			ListExplicitIndex.InitializeItem(index);
			int value = 0;
			s.ReadCursor(ref value);
			ListExplicitIndex[index] = value;
		}
		protected override void Write<TDoc, TCursor>(IO.TagElementStream<TDoc, TCursor, string> s, BXmlSerializerInterface xs, int data)
		{
			s.WriteCursor(data);
		}
		#endregion
	};
}