using System.Collections.Generic;

namespace KSoft.Phoenix.Phx
{
	public sealed class BSoundTable
		: IO.ITagElementStringNameStreamable
	{
		#region Xml constants
		internal static readonly Engine.XmlFileInfo kXmlFileInfo = new()
		{
			Directory = Phoenix.Engine.GameDirectory.Data,
			FileName = "SoundTable.xml",
			RootName = "Table"
		};
		#endregion

		public Dictionary<uint, string> mEventsMap = new();
		public IReadOnlyDictionary<uint, string> EventsMap => mEventsMap;

		#region ITagElementTextStreamable Members
		void FromStream<TDoc, TCursor>(IO.TagElementStream<TDoc, TCursor, string> s)
			where TDoc : class
			where TCursor : class
		{
			foreach (var element in s.ElementsByName("Sound"))
			{
				using (s.EnterCursorBookmark(element))
				{
					string name = string.Empty; uint event_id = 0;
					s.StreamElement("CueName", ref name);
					s.StreamElement("CueIndex", ref event_id);
					mEventsMap.Add(event_id, name);
				}
			}
		}
		public void Serialize<TDoc, TCursor>(IO.TagElementStream<TDoc, TCursor, string> s)
			where TDoc : class
			where TCursor : class
		{
			if (s.IsReading)
			{
				FromStream(s);
			}
		}
		#endregion
	};
}