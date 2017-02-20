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
		const string kEraCryptInputDir = @"C:\KStudio\HaloWars\PC\";
		const string kEraCryptOutputDir = kTestResultsPath;
		const string kEraCryptTestFileName = "root";

		static readonly Collections.BitVector32 kEraUtilTestOptions = new Collections.BitVector32()
			.Set(EraFileUtilOptions.x64)
			;
		static readonly Collections.BitVector32 kEraExpanderTestOptions = new Collections.BitVector32()
			//EraFileExpanderOptions
			.Set(EraFileExpanderOptions.OnlyDumpListing)
			.Set(EraFileExpanderOptions.DontTranslateXmbFiles)
			;
		static readonly Collections.BitVector32 kEraBuilderTestOptions = new Collections.BitVector32()
			//EraFileBuilderOptions
			;
		const string kEraExpanderFileName = kEraCryptTestFileName;
		const string kEraExpanderInputFile =
			kEraCryptOutputDir
				+ kEraExpanderFileName
					+ EraFileUtil.kExtensionEncrypted
					+ EraFileUtil.kExtensionDecrypted
			;
		const string kEraExpanderOutputDir = kTestResultsPath + @"assets\";
		const string kEraBuilderOutputDir = kEraCryptInputDir;

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

			using (var expander = new EraFileExpander(kEraExpanderInputFile))
			{
				expander.Options = kEraUtilTestOptions;
				expander.ExpanderOptions = kEraExpanderTestOptions;
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
				builder.Options = kEraUtilTestOptions;
				builder.BuilderOptions = kEraBuilderTestOptions;
				builder.VerboseOutput = Console.Out;

				result = builder.Read();
				Assert.IsTrue(result, "Read listing failed");

				result = builder.Build(kEraExpanderOutputDir, kEraCryptTestFileName + "_rebuilt", kEraBuilderOutputDir);
				Assert.IsTrue(result, "Build failed");
			}
		}

		[TestMethod]
		public void EraFile_ExpandAndBuildTest()
		{
			const string k_input_era =
				kEraCryptInputDir
					+ kEraExpanderFileName
						+ EraFileUtil.kExtensionEncrypted
				;
			const string k_listing_path = kEraExpanderOutputDir + kEraExpanderFileName;
			const string k_rebuilt_name = kEraCryptTestFileName + "_rebuilt";
			const string k_rebuilt_era =
				kEraCryptInputDir
				+ k_rebuilt_name
					+ EraFileUtil.kExtensionEncrypted
				;

			bool result = false;

			#region Expand
			if(false)using (var expander = new EraFileExpander(k_input_era))
			{
				expander.Options = kEraUtilTestOptions;
				expander.ExpanderOptions = kEraExpanderTestOptions
					//.Set(EraFileExpanderOptions.DontTranslateXmbFiles)
					.Set(EraFileExpanderOptions.Decrypt);
				expander.VerboseOutput = Console.Out;

				result = expander.Read();
				Assert.IsTrue(result, "Read failed");

				result = expander.ExpandTo(kEraExpanderOutputDir, kEraExpanderFileName);
				Assert.IsTrue(result, "Expansion failed");
			}
			#endregion
			#region Build
			using (var builder = new EraFileBuilder(k_listing_path))
			{
				builder.Options = kEraUtilTestOptions;
				builder.BuilderOptions = kEraBuilderTestOptions
					.Set(EraFileBuilderOptions.Encrypt);
				builder.VerboseOutput = Console.Out;

				result = builder.Read();
				Assert.IsTrue(result, "Read listing failed");

				result = builder.Build(kEraExpanderOutputDir, k_rebuilt_name, kEraBuilderOutputDir);
				Assert.IsTrue(result, "Build failed");
			}
			#endregion
			#region Verify
			using (var expander = new EraFileExpander(k_rebuilt_era))
			{
				expander.Options = kEraUtilTestOptions;
				expander.ExpanderOptions = kEraExpanderTestOptions
					.Set(EraFileExpanderOptions.DontTranslateXmbFiles)
					.Set(EraFileExpanderOptions.Decrypt);
				expander.VerboseOutput = Console.Out;

				result = expander.Read();
				Assert.IsTrue(result, "Re-Read failed");

				//result = expander.ExpandTo(kTestResultsPath + @"assets2\", k_rebuilt_name);
				//Assert.IsTrue(result, "Re-Expansion failed");
			}
			#endregion
		}
	};
}