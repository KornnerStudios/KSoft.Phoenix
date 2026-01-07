using System;

namespace KSoft.Phoenix
{
	internal static class PhxConstants
	{
		/// <summary>Applied to enumeration members which are values the engine doesn't actually use or implement functionality for</summary>
		public const string kUnusedByEngineMsg = "Unused or implemented by Phoenix Engine.";
		/// <summary>Applied to enumeration members which are values the engine doesn't actually use or implement functionality for</summary>
		public const string kAlphaOnlyMsg = "Only featured in the Alpha build of HW.";
	};

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