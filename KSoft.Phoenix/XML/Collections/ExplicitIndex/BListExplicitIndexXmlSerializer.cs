﻿using System;
using System.Collections.Generic;
using Contracts = System.Diagnostics.Contracts;
using Contract = System.Diagnostics.Contracts.Contract;

namespace KSoft.Phoenix.XML
{
	partial class XmlUtil
	{
		public static void Serialize<T, TDoc, TCursor>(IO.TagElementStream<TDoc, TCursor, string> s,
			Collections.BListExplicitIndex<T> list, BListExplicitIndexXmlParams<T> @params)
			where T : IEqualityComparer<T>, IO.ITagElementStringNameStreamable, new()
			where TDoc : class
			where TCursor : class
		{
			Contract.Requires(s != null);
			Contract.Requires(list != null);
			Contract.Requires(@params != null);

			using(var xs = new BListExplicitIndexXmlSerializer<T>(@params, list))
			{
				xs.Serialize(s);
			}
		}
	};

	internal class BListExplicitIndexXmlSerializer<T>
		: BListExplicitIndexXmlSerializerBase<T>
		where T : IEqualityComparer<T>, IO.ITagElementStringNameStreamable, new()
	{
		Collections.BListExplicitIndex<T> mList;

		public override Collections.BListExplicitIndexBase<T> ListExplicitIndex { get { return mList; } }

		public BListExplicitIndexXmlSerializer(BListExplicitIndexXmlParams<T> @params, Collections.BListExplicitIndex<T> list) : base(@params)
		{
			Contract.Requires<ArgumentNullException>(@params != null);
			Contract.Requires<ArgumentNullException>(list != null);

			mList = list;
		}

		#region IXmlElementStreamable Members
		protected override void Read<TDoc, TCursor>(IO.TagElementStream<TDoc, TCursor, string> s, BXmlSerializerInterface xs, int iteration)
		{
			int index = ReadExplicitIndex(s, xs);
			Contract.Assert(index.IsNotNone());

			mList.InitializeItem(index);
			T data = new T();
			data.Serialize(s);
			mList[index] = data;
		}
		protected override void Write<TDoc, TCursor>(IO.TagElementStream<TDoc, TCursor, string> s, BXmlSerializerInterface xs, T data)
		{
			data.Serialize(s);
		}
		#endregion
	};
}