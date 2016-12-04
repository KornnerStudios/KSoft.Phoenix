using System;
using System.Collections.Generic;
using System.IO;
using Mono.Options;

namespace KSoft.Tool.Phoenix
{
	using BPhoenix = KSoft.Phoenix;

	sealed class WwiseTool : ProgramBase
	{
		protected override Environment ProgramEnvironment { get { return Environment.Phx; } }
		public static void _Main(string helpName, List<string> args)
		{
			var prog = new WwiseTool();
			prog.MainImpl(helpName, args);
		}

		enum Mode
		{
			None,

			Extract,
		};
		static string GetValidModes()
		{
			var sb = new System.Text.StringBuilder(64);
			sb.Append("Valid modes: ");

			sb.AppendFormat("{0},", Mode.Extract.ToString().ToLowerInvariant());

			return sb.ToString();
		}

		const string kSoundTablePath = @"Games\HaloWars\SoundTable.xml";
		const string kGeneratedSoundBanksFolder = @"wwise_material\GeneratedSoundBanks\";
		const string kSoundsPackListingName = @"HaloWars_sounds_pck.xml";
		Mode mMode;
		string mPath, mOutputPath, mSwitches;
		bool mTimeOperation;

		protected override void InitializeOptions()
		{
			mOptions = new OptionSet() {
				{"mode=", GetValidModes(),
					v => Program.ParseEnum(v, out mMode) },
				{"path=", "Game's 'sound' directory",
					v => mPath = v },
				{"out:", "Output directory. Defaults to the working directory if blank",
					v => mOutputPath = v },
				{"switches:", "Mode specific switches",
					v => mSwitches = v },
				{"stopwatch", "Time performance",
					v => mTimeOperation = v != null },
			};
			InitializeOptionArgShowHelp();
		}

		#region ValidateArgs
		bool ValidateArgsExtract()
		{
			if (string.IsNullOrWhiteSpace(mPath))
			{
				Console.WriteLine("Error: Invalid sounds path: {0}", mPath);
				return false;
			}
			if (!Directory.Exists(mPath))
			{
				Console.WriteLine("Error: Sounds path does not exist or is inaccessible: {0}", mPath);
				return false;
			}

			mPath = Path.Combine(mPath, kGeneratedSoundBanksFolder, @"xbox360\");
			if (!Directory.Exists(mPath))
			{
				Console.WriteLine("Error: Xbox360 path does not exist or is inaccessible: {0}", mPath);
				return false;
			}

			if (!File.Exists(kSoundTablePath))
			{
				Console.WriteLine("Error: SoundTable file does not exist: {0}", kSoundTablePath);
				return false;
			}

			bool output_path_exists = true;

			if (string.IsNullOrWhiteSpace(mOutputPath)) mOutputPath = Path.Combine(System.Environment.CurrentDirectory, @"\");
			if (!Directory.Exists(mOutputPath))
			{
				output_path_exists = false;

				Console.WriteLine("Error: The output path doesn't exist or is inaccessible: {0}", mOutputPath);

				Console.WriteLine("Do you wish to create it? Enter *any* character to create it, or *just* press Enter to exit");
				if (Console.ReadKey().Key != ConsoleKey.Enter)
				{
					try {
						Directory.CreateDirectory(mOutputPath);
						output_path_exists = true;
					} catch (Exception ex) {
						Console.WriteLine("Error: failed to create output path. Reason: {0}", ex.Message);
					}

					Console.WriteLine();
				}
			}

			return output_path_exists;
		}
		protected override bool ValidateArgs()
		{
			switch (mMode)
			{
				case Mode.Extract: return ValidateArgsExtract();

				default: return true;
			}
		}
		#endregion

		void MainBody()
		{
			switch (mMode)
			{
				case Mode.Extract: Extract(mPath, mOutputPath); break;

				default: Program.UnavailableOption(mMode); break;
			}
		}
		void MainImpl(string helpName, List<string> args)
		{
			List<string> extra;
			MainImpl_Prologue(args, out extra, () => mMode == Mode.None);
			MainImpl_Tool(helpName, "Wwise", MainBody);
		}

		[Flags]
		enum ExtractSwitches
		{
			DumpSoundPackToXml=1<<0,
			OverwriteExisting=1<<1,
		};
		void ExtractParseSwitches(string switches,
			out ExtractSwitches flags)
		{
			flags = 0;
			if (switches == null) switches = "";
			const string k_switches_ctxt = "Wwise:Extract";

			if (SwitchIsOn(switches, 0, k_switches_ctxt, "Dump sound.pck info to xml"))
			{
				flags |= ExtractSwitches.DumpSoundPackToXml;
			}
			if (SwitchIsOn(switches, 1, k_switches_ctxt, "Overwrite existing xma files"))
			{
				flags |= ExtractSwitches.OverwriteExisting;
			}
		}
		void Extract(string banksPath, string outputPath)
		{
			if (!System.IO.File.Exists(kSoundTablePath))
			{
				Console.WriteLine("Error: Couldn't load '{0}'. I need this file to perform extraction",
					kSoundTablePath);
				return;
			}

			ExtractSwitches switches;
			ExtractParseSwitches(mSwitches, out switches);

			var stopwatch = mTimeOperation ? System.Diagnostics.Stopwatch.StartNew() : null;

			#region Read game's soundtable.xml
			Console.WriteLine("\t" + "Initializing...");
			var sound_table = new BPhoenix.Phx.BSoundTable();
			using (var s = new IO.XmlElementStream(kSoundTablePath, FileAccess.Read))
			{
				s.StreamMode = FileAccess.Read;
				s.InitializeAtRootElement();
				sound_table.Serialize(s);
			}

			if (mTimeOperation)
			{
				stopwatch.Stop();
				Console.WriteLine("\t\tPerf: {0}", stopwatch.Elapsed);
				stopwatch.Restart();
			}
			#endregion

			#region setup pck_settings
			var pck_settings = new KSoft.Wwise.FilePackage.AkFilePackageSettings()
			{
				Platform = Shell.Platform.Xbox360,
				SdkVersion = KSoft.Wwise.AkVersion.k2009.Id,
				UseAsciiStrings = false,
			};
			var pck = new KSoft.Wwise.FilePackage.AkFilePackage(pck_settings);
			#endregion

			#region Process sounds.pck
			Console.WriteLine("\t" + "Processing pck...");
			string sounds_pck_filename = Path.Combine(banksPath, "sounds.pck");

			using (var fs = File.OpenRead(sounds_pck_filename))
			using (var s = new IO.EndianStream(fs, Shell.EndianFormat.Big))
			{
				s.StreamMode = FileAccess.Read;
				pck.Serialize(s);
				pck.SerializeSoundBanks(s);
			}

			if (mTimeOperation)
			{
				stopwatch.Stop();
				Console.WriteLine("\t\tPerf: {0}", stopwatch.Elapsed);
				stopwatch.Restart();
			}
			#endregion

			var pck_extractor = new Wwise.FilePackage.AkFilePackageExtractor(sounds_pck_filename, pck, sound_table.EventsMap);

			Console.WriteLine("\t" + "Postprocessing bank data...");
			pck_extractor.PrepareForExtraction();
			if (mTimeOperation)
			{
				stopwatch.Stop();
				Console.WriteLine("\t\tPerf: {0}", stopwatch.Elapsed);
				stopwatch.Restart();
			}

			#region DumpSoundPackToXml
			if ( (switches & ExtractSwitches.DumpSoundPackToXml)!=0 )
				using (var s = IO.XmlElementStream.CreateForWrite("soundsPack"))
			{
				Console.WriteLine("\t" + "Taking a dump...");
				Serialize(s, pck_extractor);
				Console.WriteLine("\t\t" + "flushing...");
				s.Document.Save(Path.Combine(mOutputPath, kSoundsPackListingName));

				if (mTimeOperation)
				{
					stopwatch.Stop();
					Console.WriteLine("\t\tPerf: {0}", stopwatch.Elapsed);
					stopwatch.Restart();
				}
			}
			#endregion

			#region Extract
			Console.WriteLine("\t" + "Extracting...(this normally takes a while)");

			using (var fs = File.OpenRead(sounds_pck_filename))
			using (var s = new IO.EndianStream(fs, Shell.EndianFormat.Big))
			using (var towav = new StreamWriter(Path.Combine(mOutputPath, "HaloWars_towav.bat")))
				pck_extractor.ExtractSounds(mOutputPath, towav, s.Reader, (switches & ExtractSwitches.OverwriteExisting) != 0);
			#endregion

			if (mTimeOperation)
			{
				stopwatch.Stop();
				Console.WriteLine("Perf: {0}", stopwatch.Elapsed);
			}

			Console.WriteLine("Done");
		}

		static void Serialize<TDoc, TCursor>(IO.TagElementTextStream<TDoc, TCursor> s, Wwise.FilePackage.AkFilePackageExtractor extractor)
			where TDoc : class
			where TCursor : class
		{
			using (s.EnterOwnerBookmark(extractor))
			{
#if false // #TODO
				using (s.EnterCursorBookmark("languageMap"))
					mLangMap.Serialize(s);
				using (s.EnterCursorBookmark("banks"))
					mSoundBanksTable.Serialize(s);
				using (s.EnterCursorBookmark("streams"))
					mStreamedFilesTable.Serialize(s);
				if (mExternalFilesTable != null)
				{
					using (s.EnterCursorBookmark("externalFiles"))
						mExternalFilesTable.Serialize(s);
				}

				if (s.IsWriting)
				{
					extractor.PrepareForExtraction();

					using (s.EnterCursorBookmark("bankInfo"))
						foreach (var bank in extractor.Package.SoundBanks)
							using (s.EnterCursorBookmark("bank"))
							{
								s.WriteAttribute("id", bank.Id, NumeralBase.Hex);

								string name;
								if (mIdToName.TryGetValue(bank.Id, out name))
									s.WriteAttribute("name", name);

								//bank.Serialize(s);
							}
				}
#endif
			}
		}
	};
}