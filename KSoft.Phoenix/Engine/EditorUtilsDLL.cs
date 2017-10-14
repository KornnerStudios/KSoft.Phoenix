using System;
using System.Runtime.InteropServices;

namespace KSoft.Phoenix.Engine
{
	public static class EditorUtilsDLL
	{
		const string kDllName = @"Engine\EditorUtils.dll";
		const CallingConvention kCallConv = CallingConvention.Cdecl;

		public static bool Initialized { get; private set; }

		internal static bool gEntryPointsNotFound;
		public static bool EntryPointsNotFound
		{
			get
			{
				if (!Initialized && !gEntryPointsNotFound)
					Initialize();

				return gEntryPointsNotFound;
			}
			private set { gEntryPointsNotFound = value; }
		}

		public static void HandleEntryPointNotFound(EntryPointNotFoundException ex)
		{
			if (EntryPointsNotFound)
				return;

			EntryPointsNotFound = true;
			Phoenix.Debug.Trace.Phoenix.TraceData(System.Diagnostics.TraceEventType.Critical, TypeExtensions.kNone,
				"Failed to find a EditorUtils method",
				ex);
		}

		public static void Initialize()
		{
			if (Initialized)
				return;

			try
			{
#if false
				var libPointerSize = DirectXTex_GetPointerSize();
				if (libPointerSize != IntPtr.Size)
				{
					Phoenix.Debug.Trace.Phoenix.TraceData(System.Diagnostics.TraceEventType.Critical, TypeExtensions.kNone,
						"EditorUtils was built for a platform that doesn't match what we're currently running in",
						libPointerSize,
						IntPtr.Size);
					return;
				}

				var libMode = DirectXTex_GetLibraryMode();
				Phoenix.Debug.Trace.Phoenix.TraceData(System.Diagnostics.TraceEventType.Critical, TypeExtensions.kNone,
					"Finished loading EditorUtils",
					libMode);
#endif

				Initialized = true;
			}
			catch (EntryPointNotFoundException ex)
			{
				HandleEntryPointNotFound(ex);
			}
		}

		public static void Dispose()
		{
			if (!Initialized)
				return;

			Initialized = false;
			EntryPointsNotFound = false;
		}
	};
}