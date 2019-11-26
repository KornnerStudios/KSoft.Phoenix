﻿using System;
#if CONTRACTS_FULL_SHIM
using Contract = System.Diagnostics.ContractsShim.Contract;
#else
using Contract = System.Diagnostics.Contracts.Contract; // SHIM'D
#endif

namespace KSoft.Phoenix.XML
{
	internal abstract class BListExplicitIndexXmlSerializerBase<T>
		: BListXmlSerializerBase<T>
	{
		BListExplicitIndexXmlParams<T> mParams;

		public abstract Collections.BListExplicitIndexBase<T> ListExplicitIndex { get; }

		public override BListXmlParams Params { get { return mParams; } }
		public override Collections.BListBase<T> List { get { return ListExplicitIndex; } }

		protected BListExplicitIndexXmlSerializerBase(BListExplicitIndexXmlParams<T> @params)
		{
			Contract.Requires<ArgumentNullException>(@params != null);

			mParams = @params;
		}

		#region IXmlElementStreamable Members
		protected virtual int ReadExplicitIndex<TDoc, TCursor>(IO.TagElementStream<TDoc, TCursor, string> s, BXmlSerializerInterface xs)
			where TDoc : class
			where TCursor : class
		{
			int index = TypeExtensions.kNone;
			mParams.StreamExplicitIndex(s, ref index);

			return index;
		}
		protected virtual void WriteExplicitIndex<TDoc, TCursor>(IO.TagElementStream<TDoc, TCursor, string> s, BXmlSerializerInterface xs, int index)
			where TDoc : class
			where TCursor : class
		{
			mParams.StreamExplicitIndex(s, ref index);
		}

		protected override void WriteNodes<TDoc, TCursor>(IO.TagElementStream<TDoc, TCursor, string> s, BXmlSerializerInterface xs)
		{
			var eip = ListExplicitIndex.ExplicitIndexParams;
			T k_invalid = eip.kTypeGetInvalid();

			int index = 0;
			foreach (T data in ListExplicitIndex)
			{
				if (eip.kComparer.Compare(data, k_invalid) != 0)
				{
					using (s.EnterCursorBookmark(WriteGetElementName(data)))
					{
						WriteExplicitIndex(s, xs, index);
						Write(s, xs, data);
					}
				}

				index++;
			}
		}
		#endregion
	};
}