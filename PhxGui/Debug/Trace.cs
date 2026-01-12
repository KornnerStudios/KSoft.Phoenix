using System;
using Diag = System.Diagnostics;

namespace PhxGui.Debug
{
	/// <summary>Utility class for tracing assembly logic (or lack thereof)</summary>
	internal static class Trace
	{
		/// <summary>Tracer for the <see cref="PhxGui"/> namespace</summary>
		public static Diag.TraceSource PhxGui { get; } = new(nameof(PhxGui), Diag.SourceLevels.All);
	};
}