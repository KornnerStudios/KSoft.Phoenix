using KSoft;
using KSoft.Collections;
using KSoft.Phoenix.Resource;
using System;
using System.IO;
using System.Threading.Tasks;

namespace PhxGui
{
	partial class MainWindowViewModel
	{
		private void ProcessEraListing(string eraListing)
		{
			if (!System.IO.Directory.Exists(Properties.Settings.Default.EraBuildOutputPath))
			{
				MessagesText = "Cannot expand ERA file(s)\n" +
					"Specify a valid expand output path";
				return;
			}

			ClearMessages();
			IsProcessing = true;

			var args = new BuildEraFileParameters(Flags.Test(MiscFlags.UseVerboseOutput));
			if (Properties.Settings.Default.GameVersion == GameVersionType.DefinitiveEdition)
			{
				args.EraOptions.Set(KSoft.Phoenix.Resource.EraFileUtilOptions.x64);
			}
			if (Flags.Test(MiscFlags.SkipVerification))
			{
				args.EraOptions.Set(KSoft.Phoenix.Resource.EraFileUtilOptions.SkipVerification);
			}
			args.EraBuilderOptions.Set(KSoft.Phoenix.Resource.EraFileBuilderOptions.Encrypt);

			if (Flags.Test(MiscFlags.AlwaysUseXmlOverXmb))
			{
				args.EraBuilderOptions.Set(KSoft.Phoenix.Resource.EraFileBuilderOptions.AlwaysUseXmlOverXmb);
			}

			args.AssetsPath = System.IO.Path.GetDirectoryName(eraListing)!;
			args.OutputPath = Properties.Settings.Default.EraBuildOutputPath;
			args.ListingPath = eraListing;
			args.EraName = System.IO.Path.GetFileNameWithoutExtension(eraListing);

			StatusText = string.Format("Building {0}.era",
				args.EraName);

			var scheduler = TaskScheduler.FromCurrentSynchronizationContext();
			var task = Task.Run(() =>
			{
				return BuildEraFile(args);
			});

			task.ContinueWith(t =>
			{
				string message_text = "";
				string verbose_output = args.GetVerboseOutput();

				if (t.IsFaulted || t.Result != BuildEraFileResult.Success)
				{
					bool verbose = Flags.Test(MiscFlags.UseVerboseOutput);

					string error_type;
					string error_hint;
					if (t.IsFaulted)
					{
						error_type = "EXCEPTION";

						var e = t.Exception!.GetOnlyExceptionOrAll()!;
						error_hint = verbose
							? e.ToVerboseString()!
							: e.ToBasicString()!;
					}
					else
					{
						error_type = "FAILED";
						error_hint = t.Result switch
						{
							BuildEraFileResult.Error
							=> "NO HINT",
							BuildEraFileResult.ReadFailed
							=> "Failed reading or initializing .ERADEF data",
							BuildEraFileResult.BuildFailed
							=> "Failed building archive (invalid files?). See PhxGui.log for possible details",
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
					sb.AppendLine(args.VerboseOutput!.GetStringBuilder().ToString());
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

		private class BuildEraFileParameters
		{
			public BitVector32 EraOptions;
			public BitVector32 EraBuilderOptions;
			public StringWriter? VerboseOutput;

			public string AssetsPath = null!;
			public string OutputPath = null!;
			public string ListingPath = null!;
			public string EraName = null!;

			public BuildEraFileParameters(bool useVerboseOutput)
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
		private enum BuildEraFileResult
		{
			Success,
			Error,
			ReadFailed,
			BuildFailed,
		};
		private static BuildEraFileResult BuildEraFile(BuildEraFileParameters args)
		{
			var result = BuildEraFileResult.Error;
			using (var builder = new KSoft.Phoenix.Resource.EraFileBuilder(args.ListingPath))
			{
				builder.Options = args.EraOptions;
				builder.BuilderOptions = args.EraBuilderOptions;
				builder.VerboseOutput = args.VerboseOutput;

				do
				{
					if (!builder.Read())
					{
						result = BuildEraFileResult.ReadFailed;
						break;
					}

					if (!builder.Build(args.AssetsPath, args.EraName, args.OutputPath))
					{
						result = BuildEraFileResult.BuildFailed;
						break;
					}

					result = BuildEraFileResult.Success;
				} while (false);
			}

			return result;
		}
	};
}
