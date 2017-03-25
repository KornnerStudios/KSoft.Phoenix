using System;
using System.Threading.Tasks;
using KSoft.Collections;

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

			var args = new BuildEraFileParameters();
			if (Properties.Settings.Default.GameVersion == GameVersionType.DefinitiveEdition)
				args.EraOptions.Set(KSoft.Phoenix.Resource.EraFileUtilOptions.x64);
			args.EraBuilderOptions.Set(KSoft.Phoenix.Resource.EraFileBuilderOptions.Encrypt);

			if (Flags.Test(MiscFlags.AlwaysUseXmlOverXmb))
				args.EraBuilderOptions.Set(KSoft.Phoenix.Resource.EraFileBuilderOptions.AlwaysUseXmlOverXmb);

			args.AssetsPath = System.IO.Path.GetDirectoryName(eraListing);
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
				if (t.IsFaulted || t.Result != BuildEraFileResult.Success)
				{
					string error_type;
					string error_hint;
					if (t.IsFaulted)
					{
						error_type = "EXCEPTION";
						error_hint = t.Exception.ToString();
					}
					else
					{
						error_type = "FAILED";
						switch (t.Result)
						{
							case BuildEraFileResult.Error:
								error_hint = "NO HINT";
								break;
							case BuildEraFileResult.ReadFailed:
								error_hint = "Failed reading or initializing .ERADEF data";
								break;
							case BuildEraFileResult.BuildFailed:
								error_hint = "Failed building archive (invalid files?)";
								break;
							default:
								error_hint = "UNKNOWN";
								break;
						}
					}
					MessagesText += string.Format("Build {0} {1}{2}{3}",
						error_type,
						args.ListingPath, Environment.NewLine, error_hint);
				}

				FinishProcessing();
			}, scheduler);
		}

		private class BuildEraFileParameters
		{
			public BitVector32 EraOptions;
			public BitVector32 EraBuilderOptions;

			public string AssetsPath;
			public string OutputPath;
			public string ListingPath;
			public string EraName;
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