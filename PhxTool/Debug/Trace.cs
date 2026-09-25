using Diag = System.Diagnostics;

namespace KSoft.Tool.Debug;

internal static class Trace
{
	public static Diag.TraceSource PhxTool { get; } = new(Program.TraceCategoryName, Diag.SourceLevels.All);
}
