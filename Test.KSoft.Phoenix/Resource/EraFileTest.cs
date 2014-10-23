using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using FA = System.IO.FileAccess;

namespace KSoft.Phoenix.Resource.Test
{
	[TestClass]
	public sealed class EraFileTest
		: BaseTestClass
	{
		const string kEraCryptInputDir = @"C:\Mount\A\Xbox\Xbox360\Games\Halo Wars\";
		const string kEraCryptOutputDir = kTestResultsPath;
		const string kEraCryptTestFileName = "miniloader";

		const EraFileExpanderOptions kEraExpanderTestOptions = 0;
		const string kEraExpanderFileName = kEraCryptTestFileName;
		const string kEraExpanderInputFile = kEraCryptOutputDir + kEraExpanderFileName
			+ EraFileUtil.kExtensionEncrypted
			+ EraFileUtil.kExtensionDecrypted;
		const string kEraExpanderOutputDir = kTestResultsPath + @"assets\";

		[TestMethod]
		public void EraFile_DecryptTest()
		{
			// miniloader.era -> miniloader.bin
			EraFileUtil.Crypt(
				kEraCryptInputDir,
				kEraCryptTestFileName,
				kEraCryptOutputDir,
				Security.Cryptography.CryptographyTransformType.Decrypt,
				Console.Out);
		}

		[TestMethod]
		public void EraFile_EncryptTest()
		{
			// miniloader.bin -> miniloader.era
			EraFileUtil.Crypt(
				kEraCryptOutputDir, // DecryptTest outputs here, and that output is what we'll use to test encryption
				kEraCryptTestFileName,
				kEraCryptOutputDir,
				Security.Cryptography.CryptographyTransformType.Encrypt,
				Console.Out);
		}

		[TestMethod]
		public void EraFile_ExpandTest()
		{
			bool result = false;

			using (var expander = new EraFileExpander(kEraExpanderInputFile, kEraExpanderTestOptions))
			{
				expander.VerboseOutput = Console.Out;

				result = expander.Read();
				Assert.IsTrue(result, "Read failed");

				result = expander.ExpandTo(kEraExpanderOutputDir, kEraExpanderFileName);
				Assert.IsTrue(result, "Expansion failed");
			}
		}

		[TestMethod]
		public void EraFile_BuildTest()
		{
			const string k_listing_path = kEraExpanderOutputDir + kEraExpanderFileName;

			bool result = false;

			using (var builder = new EraFileBuilder(k_listing_path))
			{
				builder.VerboseOutput = Console.Out;

				result = builder.Read();
				Assert.IsTrue(result, "Read listing failed");

				result = builder.Build(kEraExpanderOutputDir, kEraCryptTestFileName + "_rebuilt");
				Assert.IsTrue(result, "Build failed");
			}
		}
	};
}