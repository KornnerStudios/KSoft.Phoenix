using System;
using System.IO;

namespace KSoft.Phoenix.Resource.PKG
{
	public abstract class CaPackageFileUtil
		: IDisposable
	{
		public CaPackageFileDefinition PkgDefinition { get; private set; }
		internal CaPackageFile mPkgFile;
		protected string mSourceFile; // filename of the source file which the util stems from
		public TextWriter ProgressOutput { get; set; }
		public TextWriter VerboseOutput { get; set; }
		public TextWriter DebugOutput { get; set; }

		protected CaPackageFileUtil()
		{
			PkgDefinition = new CaPackageFileDefinition();

			if (System.Diagnostics.Debugger.IsAttached)
			{
				ProgressOutput = Console.Out;
				VerboseOutput = Console.Out;
			}
		}

		#region IDisposable Members
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Usage", "CA1816:Dispose methods should call SuppressFinalize",
			Justification = "Not expecting any derived classes to have Finalizers")]
		public virtual void Dispose()
		{
			ProgressOutput = null;
			VerboseOutput = null;
			DebugOutput = null;
			mPkgFile = null;
		}
		#endregion
	};
}
