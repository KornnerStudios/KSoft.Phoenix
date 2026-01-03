using Diag = System.Diagnostics;

namespace KSoft.Phoenix.Debug
{
	/// <summary>Utility class for tracing assembly logic (or lack thereof)</summary>
	internal static class Trace
	{
		/// <summary>Tracer for the <see cref="KSoft.Phoenix"/> namespace</summary>
		public static Diag.TraceSource Phoenix { get; }			= new("KSoft.Phoenix",			Diag.SourceLevels.All);
		/// <summary>Tracer for the <see cref="KSoft.Phoenix.Engine"/> namespace</summary>
		public static Diag.TraceSource Engine { get;  }			= new("KSoft.Phoenix.Engine",	Diag.SourceLevels.All);
		/// <summary>Tracer for the <see cref="KSoft.Phoenix.Resource"/> namespace</summary>
		public static Diag.TraceSource Resource { get; }		= new("KSoft.Phoenix.Resource", Diag.SourceLevels.All);
		/// <summary>Tracer for the <see cref="KSoft.Security"/> namespace</summary>
		public static Diag.TraceSource Security { get; }		= new("KSoft.Security",			Diag.SourceLevels.All);
		/// <summary>Tracer for the Trigger System related code</summary>
		public static Diag.TraceSource TriggerSystem { get; }	= new("KSoft.Phoenix.Triggers", Diag.SourceLevels.All);
		/// <summary>Tracer for the <see cref="KSoft.Phoenix.XML"/> namespace</summary>
		public static Diag.TraceSource XML { get; }				= new("KSoft.Phoenix.XML",		Diag.SourceLevels.All);
	};
}