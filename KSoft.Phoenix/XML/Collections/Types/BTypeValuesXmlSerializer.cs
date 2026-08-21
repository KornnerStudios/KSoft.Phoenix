using System;
using System.Collections.Generic;

namespace KSoft.Phoenix.XML
{
	partial class XmlUtil
	{
		public static void Serialize<T, TDoc, TCursor>(IO.TagElementStream<TDoc, TCursor, string> s,
			Collections.BTypeValues<T> list, BTypeValuesXmlParams<T> @params)
			where T : IEqualityComparer<T>, IO.ITagElementStringNameStreamable, new()
			where TDoc : class
			where TCursor : class
		{
			ArgumentNullException.ThrowIfNull(s);
			ArgumentNullException.ThrowIfNull(list);
			ArgumentNullException.ThrowIfNull(@params);

			using (var xs = new BTypeValuesXmlSerializer<T>(@params, list))
			{
				xs.Serialize(s);
			}
		}
	};

	internal class BTypeValuesXmlSerializer<T>
		: BTypeValuesXmlSerializerBase<T>
		where T : IEqualityComparer<T>, IO.ITagElementStringNameStreamable, new()
	{
		public BTypeValuesXmlSerializer(BTypeValuesXmlParams<T> @params, Collections.BTypeValues<T> list) : base(@params, list)
		{
			ArgumentNullException.ThrowIfNull(@params);
			ArgumentNullException.ThrowIfNull(list);
		}

		#region IXmlElementStreamable Members
		protected override void Read<TDoc, TCursor>(IO.TagElementStream<TDoc, TCursor, string> s, BXmlSerializerInterface xs, int iteration)
		{
			int index = ReadExplicitIndex(s, xs);

			ListExplicitIndex.InitializeItem(index);
			T data = new();
			data.Serialize(s);
			ListExplicitIndex[index] = data;
		}
		protected override void Write<TDoc, TCursor>(IO.TagElementStream<TDoc, TCursor, string> s, BXmlSerializerInterface xs, T data)
		{
			data.Serialize(s);
		}
		#endregion
	};
}