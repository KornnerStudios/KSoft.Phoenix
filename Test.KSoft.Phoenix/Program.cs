using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace KSoft.Phoenix
{
	[TestClass] // required for AssemblyInitialize & AssemblyCleanup to work
	public static partial class TestLibrary
	{
		[AssemblyInitialize]
		public static void AssemblyInitialize(TestContext context)
		{
			KSoft.Program.Initialize();
			KSoft.Phoenix.Program.Initialize();
		}
		[AssemblyCleanup]
		public static void AssemblyDispose()
		{
			KSoft.Phoenix.Program.Dispose();
			KSoft.Program.Dispose();
		}
	};

	[TestClass]
	public abstract class BaseTestClass
	{
		/// <summary>
		/// Gets or sets the test context which provides information about and functionality for the current test run.
		/// </summary>
		public TestContext TestContext { get; set; }
	};
}
