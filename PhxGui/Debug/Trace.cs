using System;
using Diag = System.Diagnostics;

namespace PhxGui.Debug
{
	/// <summary>Utility class for tracing assembly logic (or lack thereof)</summary>
	internal static class Trace
	{
		static Diag.TraceSource kPhxGuixSource
			;

		static Trace()
		{
			kPhxGuixSource =	new Diag.TraceSource("PhxGui", Diag.SourceLevels.All);
		}

		/// <summary>Tracer for the <see cref="PhxGui"/> namespace</summary>
		public static Diag.TraceSource PhxGui { get { return kPhxGuixSource; } }
	};
}