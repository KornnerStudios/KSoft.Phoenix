using System;

namespace KSoft.Phoenix.XML
{
	partial class XmlUtil
	{
		public static void Serialize<T, TDoc, TCursor>(IO.TagElementStream<TDoc, TCursor, string> s,
			Collections.BListArray<T> list, BListXmlParams @params)
			where T : IO.ITagElementStringNameStreamable, new()
			where TDoc : class
			where TCursor : class
		{
			ArgumentNullException.ThrowIfNull(s);
			ArgumentNullException.ThrowIfNull(list);
			ArgumentNullException.ThrowIfNull(@params);

			using (var xs = new BListArrayXmlSerializer<T>(@params, list))
			{
				xs.Serialize(s);
			}
		}
	};

	internal class BListArrayXmlSerializer<T>
		: BListXmlSerializerBase<T>
		where T : IO.ITagElementStringNameStreamable, new()
	{
		readonly BListXmlParams mParams;
		readonly Collections.BListArray<T> mList;

		public override BListXmlParams Params => mParams;
		public override Collections.BListBase<T> List => mList;

		public BListArrayXmlSerializer(BListXmlParams @params, Collections.BListArray<T> list)
		{
			ArgumentNullException.ThrowIfNull(@params);
			ArgumentNullException.ThrowIfNull(list);

			mParams = @params;
			mList = list;
		}

		#region IXmlElementStreamable Members
		protected override void Read<TDoc, TCursor>(IO.TagElementStream<TDoc, TCursor, string> s, BXmlSerializerInterface xs, int iteration)
		{
			T item = new();
			item.Serialize(s);

			List.AddItem(item);
		}

		protected override void Write<TDoc, TCursor>(IO.TagElementStream<TDoc, TCursor, string> s, BXmlSerializerInterface xs, T data)
		{
			data.Serialize(s);
		}
		#endregion
	};
}