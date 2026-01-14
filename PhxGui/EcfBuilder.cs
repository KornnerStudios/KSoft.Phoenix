using System;
using System.IO;
using System.Threading.Tasks;
using KSoft;
using KSoft.Collections;

namespace PhxGui
{
	partial class MainWindowViewModel
	{
		private void ProcessEcfListing(string ecfListing)
		{
			#if false // #TODO make a EcfBuildOutputPath
			if (!Directory.Exists(Properties.Settings.Default.EraBuildOutputPath))
			{
				MessagesText = "Cannot expand ECF file(s)\n" +
					"Specify a valid expand output path";
				return;
			}
			#endif

			ClearMessages();
			IsProcessing = true;

			var args = new BuildEcfFileParameters(Flags.Test(MiscFlags.UseVerboseOutput));
			if (Properties.Settings.Default.GameVersion == GameVersionType.DefinitiveEdition)
			{
				args.EcfOptions.Set(KSoft.Phoenix.Resource.ECF.EcfFileUtilOptions.x64);
			}
			if (Flags.Test(MiscFlags.SkipVerification))
			{
				args.EcfOptions.Set(KSoft.Phoenix.Resource.ECF.EcfFileUtilOptions.SkipVerification);
			}

			args.AssetsPath = Path.GetDirectoryName(ecfListing);
			#if false // #TODO make a EcfBuildOutputPath
			args.OutputPath = Properties.Settings.Default.EraBuildOutputPath;
			#else
			args.OutputPath = Path.GetDirectoryName(ecfListing);
			#endif
			args.ListingPath = ecfListing;

			StatusText = string.Format("Building {0} ECF",
				Path.GetFileNameWithoutExtension(ecfListing));

			var scheduler = TaskScheduler.FromCurrentSynchronizationContext();
			var task = Task.Run(() =>
			{
				return BuildEcfFile(args);
			});

			task.ContinueWith(t =>
			{
				string message_text = "";
				string verbose_output = args.GetVerboseOutput();

				if (t.IsFaulted || t.Result != BuildEcfFileResult.Success)
				{
					bool verbose = Flags.Test(MiscFlags.UseVerboseOutput);

					string error_type;
					string error_hint;
					if (t.IsFaulted)
					{
						error_type = "EXCEPTION";

						var e = t.Exception.GetOnlyExceptionOrAll();
						error_hint = verbose
							? e.ToVerboseString()
							: e.ToBasicString();
					}
					else
					{
						error_type = "FAILED";
						error_hint = t.Result switch
						{
							BuildEcfFileResult.Error
							=> "NO HINT",
							BuildEcfFileResult.ReadFailed
							=> "Failed reading or initializing .ECFDEF data",
							BuildEcfFileResult.BuildFailed
							=> "Failed building ECF (invalid files?). See PhxGui.log for possible details",
							_ => "UNKNOWN",
						};
					}

					var sb = new System.Text.StringBuilder();
					sb.Append($"Build {error_type} ");
					sb.AppendLine(args.ListingPath);
					sb.AppendLine(error_hint);

					message_text = sb.ToString();
				}

				if (!string.IsNullOrEmpty(verbose_output))
				{
					var sb = new System.Text.StringBuilder();
					sb.AppendLine("VerboseOutput:");
					sb.AppendLine(args.VerboseOutput.GetStringBuilder().ToString());
					sb.AppendLine(message_text);

					message_text = sb.ToString();
				}
				if (!string.IsNullOrEmpty(message_text))
				{
					MessagesText += message_text;
				}

				FinishProcessing();
			}, scheduler);
		}

		private class BuildEcfFileParameters
		{
			public BitVector32 EcfOptions;
			public BitVector32 EcfBuilderOptions = default;
			public StringWriter VerboseOutput;

			public string AssetsPath;
			public string OutputPath;
			public string ListingPath;

			public BuildEcfFileParameters(bool useVerboseOutput)
			{
				if (useVerboseOutput)
				{
					VerboseOutput = new StringWriter(new System.Text.StringBuilder(2048));
				}
			}

			public string GetVerboseOutput()
			{
				string output = "";
				if (VerboseOutput != null)
				{
					output = VerboseOutput.GetStringBuilder().ToString();
				}

				return output;
			}
		};
		private enum BuildEcfFileResult
		{
			Success,
			Error,
			ReadFailed,
			BuildFailed,
		};
		private static BuildEcfFileResult BuildEcfFile(BuildEcfFileParameters args)
		{
			var result = BuildEcfFileResult.Error;
			using (var builder = new KSoft.Phoenix.Resource.ECF.EcfFileBuilder(args.ListingPath))
			{
				builder.Options = args.EcfOptions;
				builder.BuilderOptions = args.EcfBuilderOptions;
				builder.VerboseOutput = args.VerboseOutput;

				do
				{
					if (!builder.Read())
					{
						result = BuildEcfFileResult.ReadFailed;
						break;
					}

					if (!builder.Build(args.AssetsPath, args.OutputPath))
					{
						result = BuildEcfFileResult.BuildFailed;
						break;
					}

					result = BuildEcfFileResult.Success;
				} while (false);
			}

			return result;
		}
	};
}
