using System;
using Contracts = System.Diagnostics.Contracts;
using Contract = System.Diagnostics.Contracts.Contract;

namespace KSoft.Phoenix.XML
{
	internal static class ProtoEnumUndefinedMembers
	{
		public static void Write<TDoc, TCursor>(IO.TagElementStream<TDoc, TCursor, string> s, BListXmlParams p, 
			Collections.IProtoEnumWithUndefined undefined)
			where TDoc : class
			where TCursor : class
		{
			if (undefined.MemberUndefinedCount == 0)
				return;

			string element_name = "Undefined" + p.ElementName;

			foreach (string str in undefined.UndefinedMembers)
				using (s.EnterCursorBookmark(element_name))
					s.WriteAttribute(p.DataName, str);
		}
	};
}