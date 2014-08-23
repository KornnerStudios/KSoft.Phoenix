using System;

namespace KSoft.Phoenix.XML
{
	[Flags]
	public enum BDatabaseXmlSerializerLoadFlags
	{
		LoadUpdates = 1<<0,
		UseSynchronousLoading = 1<<1,
	};
}