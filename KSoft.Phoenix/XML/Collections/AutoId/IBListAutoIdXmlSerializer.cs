using System;

namespace KSoft.Phoenix.XML
{
	public interface IBListAutoIdXmlSerializer
		: IDisposable
		, IO.ITagElementStringNameStreamable
	{
		BListXmlParams Params { get; }

		void StreamPreload<TDoc, TCursor>(IO.TagElementStream<TDoc, TCursor, string> s)
			where TDoc : class
			where TCursor : class;
		void StreamUpdate<TDoc, TCursor>(IO.TagElementStream<TDoc, TCursor, string> s)
			where TDoc : class
			where TCursor : class;
	};
}