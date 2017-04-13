using System;

namespace KSoft.Phoenix
{
	public static class Program
	{
		public static void Initialize()
		{
		}

		public static void Dispose()
		{
		}

		public static Type DebugTraceClass { get { return typeof(Debug.Trace); } }
	};
}