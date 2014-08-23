using System;
using Contracts = System.Diagnostics.Contracts;
using Contract = System.Diagnostics.Contracts.Contract;

namespace KSoft.Phoenix.XML
{
	internal abstract class BTypeValuesXmlSerializerBase<T>
		: BListExplicitIndexXmlSerializerBase<T>
	{
		Collections.BTypeValuesBase<T> mList;

		public override Collections.BListExplicitIndexBase<T> ListExplicitIndex { get { return mList; } }

		protected BTypeValuesXmlSerializerBase(BTypeValuesXmlParams<T> @params, Collections.BTypeValuesBase<T> list) : base(@params)
		{
			Contract.Requires<ArgumentNullException>(@params != null);
			Contract.Requires<ArgumentNullException>(list != null);

			mList = list;
		}

		#region IXmlElementStreamable Members
		protected override int ReadExplicitIndex<TDoc, TCursor>(IO.TagElementStream<TDoc, TCursor, string> s, BXmlSerializerInterface xs)
		{
			string name = null;
			Params.StreamDataName(s, ref name);

			int index = mList.TypeValuesParams.kGetProtoEnumFromDB(xs.Database).GetMemberId(name);

			return index;
		}
		protected override void WriteExplicitIndex<TDoc, TCursor>(IO.TagElementStream<TDoc, TCursor, string> s, BXmlSerializerInterface xs, int index)
		{
			string name = mList.TypeValuesParams.kGetProtoEnumFromDB(xs.Database).GetMemberName(index);

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