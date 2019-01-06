using System;
using System.Collections.Generic;
using System.IO;
using Mono.Options;

namespace KSoft.Tool.Hogan
{
	sealed class PkgTool : ProgramBase
	{
		protected override Environment ProgramEnvironment { get { return Environment.Hogan; } }
		public static void _Main(string helpName, List<string> args)
		{
			var prog = new PkgTool();
			prog.MainImpl(helpName, args);
		}

		enum Mode
		{
			None,

			Expand,
			Build,
		};
		static string GetValidModes()
		{
			var sb = new System.Text.StringBuilder(64);
			sb.Append("Valid modes: ");

			sb.AppendFormat("{0},", Mode.Expand.ToString().ToLowerInvariant());
			sb.AppendFormat("{0},", Mode.Build.ToString().ToLowerInvariant());

			return sb.ToString();
		}

		Mode mMode;
		string mPath, mName,
			mOutputPath;

		string mHoganWorkPath;
		string mHoganBuildPath;
		string mPkgPath;

		bool mFlagDontOverwriteExistingFiles;
		bool mBuildFlagAlwaysUseXmlOverXmb;
		bool mExpandFlagOnlyDumpListing;

		protected override void InitializeOptions()
		{
			mOptions = new OptionSet() {
				{"mode=", GetValidModes(),
					v => Program.ParseEnum(v, out mMode) },
				{"path=", "Source directory or file",
					v => mPath = v },
				{"name=", "Depending on mode, the input (expand .pkg) or output (build .pkgdef) file",
					v => mName = v },
				{"out:", "Output directory. Defaults to the source directory if blank",
					v => mOutputPath = v },

				{"nooverwrite", "Don't overwrite existing files (Build and Expand)",
					v => mFlagDontOverwriteExistingFiles = v != null },
				{"usexmloverxmb", "Prefer XML over XMB where both exist (Build)",
					v => mBuildFlagAlwaysUseXmlOverXmb = v != null },
				{"onlydumplisting", "Only Dump PKGDEF (Expand)",
					v => mExpandFlagOnlyDumpListing = v != null },
			};
			InitializeOptionArgShowHelp();
		}

		#region ValidateArgs
		bool ValidateArgsExpand()
		{
			if (string.IsNullOrWhiteSpace(mPath) || string.IsNullOrWhiteSpace(mName))
			{
				Console.WriteLine("Error: Invalid path or name");
				return false;
			}

#if false // checked in expand's logic now since we support in-situ
			string input_file = mPath + KSoft.Phoenix.Resource.EraFileExpander.kNameExtension;
			if (!File.Exists(input_file))
			{
				Console.WriteLine("Error: Input ERA does not exist");
				return false;
			}
#endif

			return true;
		}
		bool ValidateArgsBuild()
		{
			if (string.IsNullOrWhiteSpace(mPath) || string.IsNullOrWhiteSpace(mName))
			{
				Console.WriteLine("Error: Invalid path or name");
				return false;
			}

			string input_file = Path.Combine(mPath, mName) + KSoft.Phoenix.Resource.EraFileBuilder.kNameExtension;
			if (!File.Exists(input_file))
			{
				Console.WriteLine("Error: Listing file does not exist in source directory");
				return false;
			}

			return true;
		}
		bool ValidateCryptArgs(Security.Cryptography.CryptographyTransformType transformType)
		{
			if (string.IsNullOrWhiteSpace(mPath) || string.IsNullOrWhiteSpace(mName))
			{
				Console.WriteLine("Error: Invalid path or name");
				return false;
			}

			string input_file = Path.Combine(mPath, mName) + KSoft.Phoenix.Resource.EraFileUtil.kExtensionEncrypted;
			if (transformType == Security.Cryptography.CryptographyTransformType.Encrypt)
				input_file += KSoft.Phoenix.Resource.EraFileUtil.kExtensionDecrypted;

			if (!File.Exists(input_file))
			{
				Console.WriteLine("Error: Input PKG does not exist");
				return false;
			}

			return true;
		}
		protected override bool ValidateArgs()
		{
			switch (mMode)
			{
				case Mode.Expand:
					return ValidateArgsExpand();
				case Mode.Build:
					return ValidateArgsBuild();

				default: return true;
			}
		}
		#endregion

		void MainImpl(string helpName, List<string> args)
		{
			List<string> extra;
			if (!Program.TryParse(Environment.Hogan, mOptions, args, out extra) || mMode == Mode.None)
				mArgShowHelp = true;

			if (mArgShowHelp || !ValidateArgs())
				Program.ShowHelp(Environment.Hogan, mOptions, helpName);
			else
			{
				try
				{
					switch (mMode)
					{
						case Mode.Expand:
							Expand(mPath, mName, mOutputPath, null);
							break;
						case Mode.Build:
							Build(mPath, mName, mOutputPath);
							break;

						default: Program.UnavailableOption(mMode); break;
					}
				}
				catch (Exception e)
				{
					Console.Write("Exception while PKG processing: ");
					Console.WriteLine(e);
					if (System.Diagnostics.Debugger.IsAttached)
						throw;
				}
			}
		}

		static void ParseSwitch(string switches, int index, ref bool flag)
		{
			if (switches.Length >= index+1)
				flag = switches[index] == '1';
		}
		static void ExpandParseSwitches(string switches,
			out Collections.BitVector32 options,
			out Collections.BitVector32 expanderOptions)
		{
			options = new Collections.BitVector32();
			expanderOptions = new Collections.BitVector32();
			bool only_dump_listing = false, dont_overwrite = false, dont_trans_xmb = false, decompress_ui_files = false,
				trans_gfx = false, is_32bit = false, decrypt = false, dont_load_into_memory = false,
				skip_verification = false, dump_dbg_info = false, dont_remove_xml_xmb = false;

			if (switches == null)
				switches = "";

			int index = 0;
			ParseSwitch(switches, index++, ref only_dump_listing);
			ParseSwitch(switches, index++, ref dont_overwrite);
			ParseSwitch(switches, index++, ref dont_trans_xmb);
			ParseSwitch(switches, index++, ref decompress_ui_files);
			ParseSwitch(switches, index++, ref trans_gfx);
			ParseSwitch(switches, index++, ref is_32bit);
			ParseSwitch(switches, index++, ref skip_verification);
			ParseSwitch(switches, index++, ref decrypt);
			ParseSwitch(switches, index++, ref dont_load_into_memory);
			ParseSwitch(switches, index++, ref dump_dbg_info);
			ParseSwitch(switches, index++, ref dont_remove_xml_xmb);

			if (only_dump_listing)
			{
				Console.WriteLine("Era:Expander: Switch enabled - {0}", "Only dumping listing file");
				expanderOptions.Set(KSoft.Phoenix.Resource.EraFileExpanderOptions.OnlyDumpListing);
			}
			if (dont_overwrite)
			{
				Console.WriteLine("Era:Expander: Switch enabled - {0}", "Not overwriting existing files");
				expanderOptions.Set(KSoft.Phoenix.Resource.EraFileExpanderOptions.DontOverwriteExistingFiles);
			}
			if (dont_trans_xmb)
			{
				Console.WriteLine("Era:Expander: Switch enabled - {0}", "Not translating XMB files");
				expanderOptions.Set(KSoft.Phoenix.Resource.EraFileExpanderOptions.DontTranslateXmbFiles);
			}
			if (decompress_ui_files)
			{
				Console.WriteLine("Era:Expander: Switch enabled - {0}", "Decompressing Scaleform data");
				expanderOptions.Set(KSoft.Phoenix.Resource.EraFileExpanderOptions.DecompressUIFiles);
			}
			if (trans_gfx)
			{
				Console.WriteLine("Era:Expander: Switch enabled - {0}", "Translate GFX files to SWF");
				expanderOptions.Set(KSoft.Phoenix.Resource.EraFileExpanderOptions.TranslateGfxFiles);
			}
			if (decrypt)
			{
				Console.WriteLine("Era:Expander: Switch enabled - {0}", "ERA will be loaded into memory and decrypted in place");
				expanderOptions.Set(KSoft.Phoenix.Resource.EraFileExpanderOptions.Decrypt);
				dont_load_into_memory = false;
			}
			if (dont_load_into_memory)
			{
				Console.WriteLine("Era:Expander: Switch enabled - {0}", "Not loading entire ERA into memory");
				expanderOptions.Set(KSoft.Phoenix.Resource.EraFileExpanderOptions.DontLoadEntireEraIntoMemory);
			}
			if (dont_remove_xml_xmb)
			{
				Console.WriteLine("Era:Expander: Switch enabled - {0}", "Not skipping XML or XMB counterparts");
				expanderOptions.Set(KSoft.Phoenix.Resource.EraFileExpanderOptions.DontRemoveXmlOrXmbFiles);
			}

			if (!is_32bit)
			{
				Console.WriteLine("Era:Expander: Treating ERA as 64-bit (Definitive Edition)");
				options.Set(KSoft.Phoenix.Resource.EraFileUtilOptions.x64);
			}
			else
			{
				Console.WriteLine("Era:Expander: Switch enabled - {0}", "Treat ERA as 32-bit (Xbox360)");
			}

			if (skip_verification)
			{
				Console.WriteLine("Era:Expander: Switch enabled - {0}", "Skip verification");
				options.Set(KSoft.Phoenix.Resource.EraFileUtilOptions.SkipVerification);
			}
			if (dump_dbg_info)
			{
				Console.WriteLine("Era:Expander: Switch enabled - {0}", "Dump debug info");
				options.Set(KSoft.Phoenix.Resource.EraFileUtilOptions.DumpDebugInfo);
			}
		}

		static void Expand(string eraPath, string listingName, string outputPath, string switches)
		{
			if (string.IsNullOrWhiteSpace(outputPath))
				outputPath = Path.GetDirectoryName(eraPath);

			Collections.BitVector32 options, expanderOptions;
			ExpandParseSwitches(switches, out options, out expanderOptions);

			if (expanderOptions.Test(KSoft.Phoenix.Resource.EraFileExpanderOptions.Decrypt))
				eraPath += KSoft.Phoenix.Resource.EraFileUtil.kExtensionEncrypted;
			else
				eraPath += KSoft.Phoenix.Resource.EraFileExpander.kNameExtension;

			if (!File.Exists(eraPath))
			{
				Console.WriteLine("Error: Input ERA does not exist '{0}'",
					eraPath);
				return;
			}

			StreamWriter debug_output = false//options.Test(KSoft.Phoenix.Resource.EraFileUtilOptions.DumpDebugInfo)
				? new StreamWriter("debug_expander.txt")
				: null;

			using (var expander = new KSoft.Phoenix.Resource.EraFileExpander(eraPath))
			{
				expander.Options = options;
				expander.ExpanderOptions = expanderOptions;
				expander.VerboseOutput = Console.Out;
				expander.DebugOutput = debug_output;

				if (expander.Read())
					expander.ExpandTo(outputPath, listingName);
			}

			if (debug_output != null)
				debug_output.Close();
		}

		void Build(string path, string listingName, string outputPath)
		{
			if (string.IsNullOrWhiteSpace(outputPath))
				outputPath = path;

			var builderOptions = new Collections.BitVector32();
			{
				if (mBuildFlagAlwaysUseXmlOverXmb)
				{
					Console.WriteLine("PKG:Builder: Switch enabled - {0}", "Use XML over XMB");
					builderOptions.Set(KSoft.Phoenix.Resource.PKG.CaPackageFileBuilderOptions.AlwaysUseXmlOverXmb);
				}
			}

			StreamWriter debug_output = false//options.Test(KSoft.Phoenix.Resource.EraFileUtilOptions.DumpDebugInfo)
				? new StreamWriter("debug_builder.txt")
				: null;

			string listing_path = Path.Combine(path, listingName) + KSoft.Phoenix.Resource.EraFileBuilder.kNameExtension;
			using (var builder = new KSoft.Phoenix.Resource.PKG.CaPackageFileBuilder(listing_path))
			{
				builder.BuilderOptions = builderOptions;
				builder.ProgressOutput = Console.Out;
				builder.VerboseOutput = Console.Out;
				builder.DebugOutput = debug_output;

				if (builder.Read())
				{
#if false // #TODO
					if (builder.Build(path, listingName, outputPath))
						builder.ProgressOutput.WriteLine("Success!");
					else
#endif
						builder.ProgressOutput.WriteLine("Failed!");
				}
			}

			if (debug_output != null)
				debug_output.Close();
		}
	};
}