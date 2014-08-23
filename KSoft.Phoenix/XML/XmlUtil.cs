using System;
using Contracts = System.Diagnostics.Contracts;
using Contract = System.Diagnostics.Contracts.Contract;

namespace KSoft.Phoenix.XML
{
	public static partial class XmlUtil
	{
		public const IO.TagElementNodeType kSourceAttr = IO.TagElementNodeType.Attribute;
		public const IO.TagElementNodeType kSourceElement = IO.TagElementNodeType.Element;
		public const IO.TagElementNodeType kSourceCursor = IO.TagElementNodeType.Text;
	};
}