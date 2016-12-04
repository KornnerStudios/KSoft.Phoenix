using System;
using System.Collections.Generic;
using System.IO;
using Mono.Options;

namespace KSoft.Tool.Phoenix
{
	sealed class EraTool : ProgramBase
	{
		protected override Environment ProgramEnvironment { get { return Environment.Phx; } }
		public static void _Main(string helpName, List<string> args)
		{
			var prog = new EraTool();
			prog.MainImpl(helpName, args);
		}

		enum Mode
		{
			None,

			Expand,
			Build,
			Decrypt,
			Encrypt,
		};
		static string GetValidModes()
		{
			var sb = new System.Text.StringBuilder(64);
			sb.Append("Valid modes: ");

			sb.AppendFormat("{0},", Mode.Expand.ToString().ToLowerInvariant());
			sb.AppendFormat("{0},", Mode.Build.ToString().ToLowerInvariant());
			sb.AppendFormat("{0},", Mode.Decrypt.ToString().ToLowerInvariant());
			//sb.AppendFormat("{0},", Mode.Encrypt.ToString().ToLowerInvariant());

			return sb.ToString();
		}

		Mode mMode;
		string mPath, mName,
			mOutputPath, mSwitches;

		protected override void InitializeOptions()
		{
			mOptions = new OptionSet() {
				{"mode=", GetValidModes(),
					v => Program.ParseEnum(v, out mMode) },
				{"path=", "Source directory or file",
					v => mPath = v },
				{"name=", "Depending on mode, the input (.era.bin) or output (.xml) file without an extension",
					v => mName = v },
				{"out:", "Output directory. Defaults to the source directory if blank",
					v => mOutputPath = v },
				{"switches:", "Mode specific switches",
					v => mSwitches = v },
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

			string input_file = mPath + KSoft.Phoenix.Resource.EraFileExpander.kNameExtension;
			if (!File.Exists(input_file))
			{
				Console.WriteLine("Error: Input ERA does not exist");
				return false;
			}

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
				Console.WriteLine("Error: Input ERA does not exist");
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

				case Mode.Decrypt:
					return ValidateCryptArgs(Security.Cryptography.CryptographyTransformType.Decrypt);
				case Mode.Encrypt:
					return ValidateCryptArgs(Security.Cryptography.CryptographyTransformType.Encrypt);

				default: return true;
			}
		}
		#endregion

		void MainImpl(string helpName, List<string> args)
		{
			List<string> extra;
			if (!Program.TryParse(Environment.Phx, mOptions, args, out extra) || mMode == Mode.None)
				mArgShowHelp = true;

			if (mArgShowHelp || !ValidateArgs())
				Program.ShowHelp(Environment.Phx, mOptions, helpName);
			else
			{
				try
				{
					switch (mMode)
					{
						case Mode.Expand:
							Expand(mPath, mName, mOutputPath, mSwitches);
							break;
						case Mode.Build:
							Build(mPath, mName, mOutputPath, mSwitches);
							break;
						case Mode.Decrypt:
							Crypt(mPath, mName, mOutputPath, mSwitches, Security.Cryptography.CryptographyTransformType.Decrypt);
							break;
						case Mode.Encrypt:
							Crypt(mPath, mName, mOutputPath, mSwitches, Security.Cryptography.CryptographyTransformType.Encrypt);
							break;

						default: Program.UnavailableOption(mMode); break;
					}
				}
				catch (Exception e)
				{
					Console.Write("Exception while ERA processing: ");
					Console.WriteLine(e);
					if (System.Diagnostics.Debugger.IsAttached)
						throw;
				}
			}
		}

		static void ExpandParseSwitches(string switches,
			out KSoft.Phoenix.Resource.EraFileExpanderOptions options)
		{
			options = 0;
			bool only_dump_listing = false, dont_overwrite = false, dont_trans_xmb = false, decompress_ui_files = false,
				trans_gfx = false, is_32bit = false;

			if (switches == null)
				switches = "";
			if (switches.Length >= 1)
				only_dump_listing = switches[0] == '1';
			if (switches.Length >= 2)
				dont_overwrite = switches[1] == '1';
			if (switches.Length >= 3)
				dont_trans_xmb = switches[2] == '1';
			if (switches.Length >= 4)
				decompress_ui_files = switches[3] == '1';
			if (switches.Length >= 5)
				trans_gfx = switches[4] == '1';
			if (switches.Length >= 6)
				is_32bit = switches[5] == '1';

			if (only_dump_listing)
			{
				Console.WriteLine("Era:Expander: Switch enabled - {0}", "Only dumping listing file");
				options |= KSoft.Phoenix.Resource.EraFileExpanderOptions.OnlyDumpListing;
			}
			if (dont_overwrite)
			{
				Console.WriteLine("Era:Expander: Switch enabled - {0}", "Not overwriting existing files");
				options |= KSoft.Phoenix.Resource.EraFileExpanderOptions.DontOverwriteExistingFiles;
			}
			if (dont_trans_xmb)
			{
				Console.WriteLine("Era:Expander: Switch enabled - {0}", "Not translating XMB files");
				options |= KSoft.Phoenix.Resource.EraFileExpanderOptions.DontTranslateXmbFiles;
			}
			if (decompress_ui_files)
			{
				Console.WriteLine("Era:Expander: Switch enabled - {0}", "Decompressing Scaleform data");
				options |= KSoft.Phoenix.Resource.EraFileExpanderOptions.DecompressUIFiles;
			}
			if (trans_gfx)
			{
				Console.WriteLine("Era:Expander: Switch enabled - {0}", "Translate GFX files to SWF");
				options |= KSoft.Phoenix.Resource.EraFileExpanderOptions.TranslateGfxFiles;
			}
			if (!is_32bit)
			{
				Console.WriteLine("Era:Expander: Treating ERA as 64-bit (Definitive Edition)");
				options |= KSoft.Phoenix.Resource.EraFileExpanderOptions.x64;
			}
			else
			{
				Console.WriteLine("Era:Expander: Switch enabled - {0}", "Treat ERA as 32-bit (Xbox360)");
			}
		}
		static void Expand(string eraPath, string listingName, string outputPath, string switches)
		{
			eraPath += KSoft.Phoenix.Resource.EraFileExpander.kNameExtension;
			if (string.IsNullOrWhiteSpace(outputPath))
				outputPath = Path.GetDirectoryName(eraPath);

			KSoft.Phoenix.Resource.EraFileExpanderOptions options;
			ExpandParseSwitches(switches, out options);

			using (var expander = new KSoft.Phoenix.Resource.EraFileExpander(eraPath, options))
			{
				expander.VerboseOutput = Console.Out;

				if (expander.Read())
					expander.ExpandTo(outputPath, listingName);
			}
		}

		static void Build(string path, string listingName, string outputPath, string switches)
		{
			if (string.IsNullOrWhiteSpace(outputPath))
				outputPath = path;

			string listing_path = Path.Combine(path, listingName) + KSoft.Phoenix.Resource.EraFileBuilder.kNameExtension;
			using (var builder = new KSoft.Phoenix.Resource.EraFileBuilder(listing_path))
			{
				builder.VerboseOutput = Console.Out;

				if (builder.Read())
					if (builder.Build(path, listingName, outputPath))
						builder.VerboseOutput.WriteLine("Success!");
					else
						builder.VerboseOutput.WriteLine("Failed!");
			}
		}

		static void Crypt(string path, string eraName, string outputPath, string switches, Security.Cryptography.CryptographyTransformType transformType)
		{
			KSoft.Phoenix.Resource.EraFileUtil.Crypt(path, eraName, outputPath, transformType,
				Console.Out);
		}
	};
}