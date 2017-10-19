using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace KSoft.DDS.Test
{
	[TestClass]
	public sealed class DirectXTexTest
		: Phoenix.BaseTestClass
	{
		[TestMethod]
		public void DirectXTex_SetupTest()
		{
			Assert.IsFalse(DirectXTexDLL.Initialized);
			Assert.IsFalse(DirectXTexDLL.gEntryPointsNotFound);

			DirectXTexDLL.Initialize();
			Assert.IsTrue(DirectXTexDLL.Initialized);
			Assert.IsFalse(DirectXTexDLL.EntryPointsNotFound);
			DirectXTexDLL.Dispose();

			Assert.IsFalse(DirectXTexDLL.Initialized);
			Assert.IsFalse(DirectXTexDLL.gEntryPointsNotFound);
		}

		[TestMethod]
		public void DirectXTex_GetMetadataFromDDSTest()
		{
			const string k_filename =
				@"U:\Ensemble\HaloWars\Content\1.11088.1.2\" +
					@"art\unsc\infantry\spartan_01\" +
						@"spartan_01_nm.ddx";

			var metadata = DirectXTex.GetMetadataFromFile(k_filename);

			Assert.IsTrue(DirectXTexDLL.Initialized);
			Assert.IsFalse(DirectXTexDLL.EntryPointsNotFound);

			Console.WriteLine(metadata.Width);
			Console.WriteLine(metadata.Height);
			Console.WriteLine(metadata.Format);

			DirectXTexDLL.Dispose();
		}
	};
}