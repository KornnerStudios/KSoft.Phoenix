using System;

namespace KSoft.Phoenix.XML
{
	internal abstract class BTypeValuesXmlSerializerBase<T>
		: BListExplicitIndexXmlSerializerBase<T>
	{
		readonly Collections.BTypeValuesBase<T> mList;

		public override Collections.BListExplicitIndexBase<T> ListExplicitIndex => mList;

		protected BTypeValuesXmlSerializerBase(BTypeValuesXmlParams<T> @params, Collections.BTypeValuesBase<T> list) : base(@params)
		{
			ArgumentNullException.ThrowIfNull(@params);
			ArgumentNullException.ThrowIfNull(list);

			mList = list;
		}

		#region IXmlElementStreamable Members
		protected override int ReadExplicitIndex<TDoc, TCursor>(IO.TagElementStream<TDoc, TCursor, string> s, BXmlSerializerInterface xs)
		{
			string name = string.Empty;
			Params.StreamDataName(s, ref name);

			Collections.IProtoEnum protoEnumFromDb = mList.TypeValuesParams.kGetProtoEnumFromDB(xs.Database);
			int index = protoEnumFromDb.GetMemberId(name);

			return index;
		}
		protected override void WriteExplicitIndex<TDoc, TCursor>(IO.TagElementStream<TDoc, TCursor, string> s, BXmlSerializerInterface xs, int index)
		{
			Collections.IProtoEnum protoEnumFromDb = mList.TypeValuesParams.kGetProtoEnumFromDB(xs.Database);
			string name = protoEnumFromDb.GetMemberName(index);

			Params.StreamDataName(s, ref name);
		}

		/// <summary>Not Implemented</summary>
		/// <exception cref="NotImplementedException" />
		protected override void Read<TDoc, TCursor>(IO.TagElementStream<TDoc, TCursor, string> s, BXmlSerializerInterface xs, int iteration) { throw new NotImplementedException(); }
		/// <summary>Not Implemented</summary>
		/// <exception cref="NotImplementedException" />
		protected override void Write<TDoc, TCursor>(IO.TagElementStream<TDoc, TCursor, string> s, BXmlSerializerInterface xs, T data) { throw new NotImplementedException(); }
		#endregion
	};
}