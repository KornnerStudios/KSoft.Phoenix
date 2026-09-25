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
			KSoft.Security.Program.Initialize();
			KSoft.Wwise.Program.Initialize();
			KSoft.Program.RegisterTraceSources(DebugTraceClass);
		}

		public static void Dispose()
		{
			KSoft.Wwise.Program.Dispose();
			KSoft.Security.Program.Dispose();
		}

		public static Type DebugTraceClass { get { return typeof(Debug.Trace); } }
	};
}