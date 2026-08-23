using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace KSoft.Phoenix.Resource.Test
{
	[TestClass]
	public sealed class EraFileTest
		: BaseTestClass
	{
		const string kEraCryptInputDir = @"C:\KStudio\HaloWars\PC\";
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE1006:Naming Styles")]
		string kEraCryptOutputDir
		{
			get
			{
				var outputDirectory = TestContext.TestRunResultsDirectory;
				Assert.IsNotNull(outputDirectory);
				return outputDirectory;
			}
		}
		const string kEraCryptTestFileName = "root";

		static readonly Collections.BitVector32 kEraUtilTestOptions = new Collections.BitVector32()
			.Set(EraFileUtilOptions.x64)
			;
		static readonly Collections.BitVector32 kEraExpanderTestOptions = new Collections.BitVector32()
			//EraFileExpanderOptions
			.Set(EraFileExpanderOptions.OnlyDumpListing)
			.Set(EraFileExpanderOptions.DontTranslateXmbFiles)
			;
		static readonly Collections.BitVector32 kEraBuilderTestOptions = new()
			//EraFileBuilderOptions
			;
		const string kEraExpanderFileName = kEraCryptTestFileName;
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE1006:Naming Styles")]
		string kEraExpanderInputFile => System.IO.Path.Combine(
			kEraCryptOutputDir
				, kEraExpanderFileName
					+ EraFileUtil.kExtensionEncrypted
					+ EraFileUtil.kExtensionDecrypted
			);
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE1006:Naming Styles")]
		string kEraExpanderOutputDir => System.IO.Path.Combine(kEraCryptOutputDir, @"assets\");
		const string kEraBuilderOutputDir = kEraCryptInputDir;

		[TestMethod]
		[TestCategory("ExcludedFromAppveyor")]
		public void EraFile_DecryptTest()
		{
			Assert.IsTrue(System.IO.Directory.Exists(kEraCryptInputDir),
				$"Input directory does not exist: {kEraCryptInputDir}");
			Assert.IsTrue(System.IO.File.Exists(System.IO.Path.Combine(kEraCryptInputDir, kEraCryptTestFileName)),
				$"Input file does not exist: {System.IO.Path.Combine(kEraCryptInputDir, kEraCryptTestFileName)}");

			string output_path = System.IO.Path.Combine(kEraCryptOutputDir);
			Console.WriteLine("Outputing to: {0}", output_path);

			// miniloader.era -> miniloader.bin
			EraFileUtil.Crypt(
				kEraCryptInputDir,
				kEraCryptTestFileName,
				output_path,
				Security.Cryptography.CryptographyTransformType.Decrypt,
				Console.Out);
		}

		[TestMethod]
		[TestCategory("ExcludedFromAppveyor")]
		public void EraFile_EncryptTest()
		{
			string input_path = System.IO.Path.Combine(kEraCryptOutputDir);
			Console.WriteLine("Reading from: {0}", input_path);
			string output_path = System.IO.Path.Combine(kEraCryptOutputDir);
			Console.WriteLine("Outputing to: {0}", output_path);

			Assert.IsTrue(System.IO.Directory.Exists(input_path),
				$"Input directory does not exist: {input_path}");

			Assert.IsTrue(System.IO.File.Exists(System.IO.Path.Combine(input_path, kEraCryptTestFileName)),
				$"Input file does not exist: {System.IO.Path.Combine(input_path, kEraCryptTestFileName)}");

			// miniloader.bin -> miniloader.era
			EraFileUtil.Crypt(
				input_path, // DecryptTest outputs here, and that output is what we'll use to test encryption
				kEraCryptTestFileName,
				output_path,
				Security.Cryptography.CryptographyTransformType.Encrypt,
				Console.Out);
		}

		[TestMethod]
		[TestCategory("ExcludedFromAppveyor")]
		public void EraFile_ExpandTest()
		{
			bool result;

			string input_path = kEraExpanderInputFile;
			Console.WriteLine("Reading from: {0}", input_path);

			Assert.IsTrue(System.IO.File.Exists(input_path),
				$"Input file does not exist: {input_path}");

			using (var expander = new EraFileExpander(input_path))
			{
				expander.Options = kEraUtilTestOptions;
				expander.ExpanderOptions = kEraExpanderTestOptions;
				expander.ProgressOutput = Console.Out;
				expander.VerboseOutput = Console.Out;

				result = expander.Read();
				Assert.IsTrue(result, "Read failed");

				result = expander.ExpandTo(kEraExpanderOutputDir, kEraExpanderFileName);
				Assert.IsTrue(result, "Expansion failed: " + kEraExpanderOutputDir);
			}
		}

		[TestMethod]
		[TestCategory("ExcludedFromAppveyor")]
		public void EraFile_BuildTest()
		{
			string k_listing_path = System.IO.Path.Combine(kEraExpanderOutputDir, kEraExpanderFileName);
			Assert.IsTrue(System.IO.File.Exists(k_listing_path),
				$"Listing file does not exist: {k_listing_path}");

			bool result;

			using (var builder = new EraFileBuilder(k_listing_path))
			{
				builder.Options = kEraUtilTestOptions;
				builder.BuilderOptions = kEraBuilderTestOptions;
				builder.ProgressOutput = Console.Out;
				builder.VerboseOutput = Console.Out;

				result = builder.Read();
				Assert.IsTrue(result, "Read listing failed");

				string output_path = kEraBuilderOutputDir;
				result = builder.Build(kEraExpanderOutputDir, kEraCryptTestFileName + "_rebuilt", output_path);
				Assert.IsTrue(result, "Build failed: " + output_path);
			}
		}

		[TestMethod]
		[TestCategory("ExcludedFromAppveyor")]
		public void EraFile_ExpandAndBuildTest()
		{
			const string k_input_era =
				kEraCryptInputDir
					+ kEraExpanderFileName
						+ EraFileUtil.kExtensionEncrypted
				;
			string k_listing_path = System.IO.Path.Combine(kEraExpanderOutputDir, kEraExpanderFileName);
			const string k_rebuilt_name = kEraCryptTestFileName + "_rebuilt";
			const string k_rebuilt_era =
				kEraCryptInputDir
				+ k_rebuilt_name
					+ EraFileUtil.kExtensionEncrypted
				;

			bool result;

			#region Expand
			var expand = false;
			if (expand)
			{
				Assert.IsTrue(System.IO.File.Exists(k_input_era),
					$"Input ERA file does not exist: {k_input_era}");

				using (var expander = new EraFileExpander(k_input_era))
				{
					expander.Options = kEraUtilTestOptions;
					expander.ExpanderOptions = kEraExpanderTestOptions
						//.Set(EraFileExpanderOptions.DontTranslateXmbFiles)
						.Set(EraFileExpanderOptions.Decrypt);
					expander.ProgressOutput = Console.Out;
					expander.VerboseOutput = Console.Out;

					result = expander.Read();
					Assert.IsTrue(result, "Read failed");

					result = expander.ExpandTo(kEraExpanderOutputDir, kEraExpanderFileName);
					Assert.IsTrue(result, "Expansion failed");
				}
			}
			#endregion
			#region Build
			Assert.IsTrue(System.IO.File.Exists(k_listing_path),
				$"Listing file does not exist: {k_listing_path}");

			using (var builder = new EraFileBuilder(k_listing_path))
			{
				builder.Options = kEraUtilTestOptions;
				builder.BuilderOptions = kEraBuilderTestOptions
					.Set(EraFileBuilderOptions.Encrypt);
				builder.ProgressOutput = Console.Out;
				builder.VerboseOutput = Console.Out;

				result = builder.Read();
				Assert.IsTrue(result, "Read listing failed");

				result = builder.Build(kEraExpanderOutputDir, k_rebuilt_name, kEraBuilderOutputDir);
				Assert.IsTrue(result, "Build failed");
			}
			#endregion
			#region Verify
			Assert.IsTrue(System.IO.File.Exists(k_rebuilt_era),
				$"Rebuilt file does not exist: {k_rebuilt_era}");

			using (var expander = new EraFileExpander(k_rebuilt_era))
			{
				expander.Options = kEraUtilTestOptions;
				expander.ExpanderOptions = kEraExpanderTestOptions
					.Set(EraFileExpanderOptions.DontTranslateXmbFiles)
					.Set(EraFileExpanderOptions.Decrypt);
				expander.ProgressOutput = Console.Out;
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
