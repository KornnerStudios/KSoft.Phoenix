using System;
using System.Collections.Generic;

namespace KSoft.Phoenix.XML
{
	internal abstract class BListXmlSerializerBase<T>
		: IDisposable
		, IO.ITagElementStringNameStreamable
	{
		public abstract BListXmlParams Params { get; }
		public abstract Collections.BListBase<T> List { get; }

		#region IXmlElementStreamable Members
		protected abstract void Read<TDoc, TCursor>(IO.TagElementStream<TDoc, TCursor, string> s, BXmlSerializerInterface xs, int iteration)
			where TDoc : class
			where TCursor : class;
		protected abstract void Write<TDoc, TCursor>(IO.TagElementStream<TDoc, TCursor, string> s, BXmlSerializerInterface xs, T data)
			where TDoc : class
			where TCursor : class;

		protected virtual void ReadDetermineListSize<TDoc, TCursor>(IO.TagElementStream<TDoc, TCursor, string> s, BXmlSerializerInterface xs)
			where TDoc : class
			where TCursor : class
		{
			int child_element_count = s.TryGetCursorElementCount();
			if (List.Capacity < child_element_count)
				List.Capacity = child_element_count;
		}
		protected virtual IEnumerable<TCursor> ReadGetNodes<TDoc, TCursor>(IO.TagElementStream<TDoc, TCursor, string> s)
			where TDoc : class
			where TCursor : class
		{
			return Params.UseElementName
				? s.ElementsByName(Params.ElementName)
				: s.Elements;
		}
		protected virtual void ReadNodes<TDoc, TCursor>(IO.TagElementStream<TDoc, TCursor, string> s, BXmlSerializerInterface xs)
			where TDoc : class
			where TCursor : class
		{
			ReadDetermineListSize(s, xs);

			int x = 0;
			foreach (var n in ReadGetNodes(s))
			{
				using (s.EnterCursorBookmark(n))
					Read(s, xs, x++);
			}

			List.OptimizeStorage();
		}
		protected virtual string WriteGetElementName(T data)
		{
			return Params.ElementName;
		}
		protected virtual void WriteNodes<TDoc, TCursor>(IO.TagElementStream<TDoc, TCursor, string> s, BXmlSerializerInterface xs)
			where TDoc : class
			where TCursor : class
		{
			foreach (T data in List)
				using (s.EnterCursorBookmark(WriteGetElementName(data)))
					Write(s, xs, data);
		}

		public void Serialize<TDoc, TCursor>(IO.TagElementStream<TDoc, TCursor, string> s)
			where TDoc : class
			where TCursor : class
		{
			bool should_stream = true;
			string root_name = Params.GetOptionalRootName();
			var xs = s.GetSerializerInterface();

			if (s.IsReading) // If the stream doesn't have the expected element, don't try to stream
				should_stream = root_name == null || s.ElementsExists(root_name);
			else if (s.IsWriting)
				should_stream = List != null && List.IsEmpty == false;

			if (should_stream) using (s.EnterCursorBookmark(root_name))
			{
					 if (s.IsReading)	ReadNodes(s, xs);
				else if (s.IsWriting)	WriteNodes(s, xs);
			}
		}
		#endregion

		#region IDisposable Members
		public virtual void Dispose()
		{
		}
		#endregion
	};
}