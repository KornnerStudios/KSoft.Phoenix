using System;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;
using KSoft;
using KSoft.Collections;

namespace PhxGui
{
	partial class MainWindowViewModel
	{
		private void ProcessEraFiles(string[] eraFiles)
		{
			var expandPath = Properties.Settings.Default.EraExpandOutputPath;
			if (!System.IO.Directory.Exists(expandPath))
			{
				MessagesText = "Cannot expand ERA file(s)\n" +
					"Specify a valid expand output path";
				return;
			}

			ClearMessages();
			IsProcessing = true;

			var stack = new ExpandEraFilesStack()
			{
				ViewModel = this,
				Dispatcher = Application.Current.Dispatcher,
				EraFiles = eraFiles,
				BaseOutputPath = expandPath,
			};
			if (Properties.Settings.Default.GameVersion == GameVersionType.DefinitiveEdition)
			{
				stack.EraOptions.Set(KSoft.Phoenix.Resource.EraFileUtilOptions.x64);
			}
			if (Flags.Test(MiscFlags.SkipVerification))
			{
				stack.EraOptions.Set(KSoft.Phoenix.Resource.EraFileUtilOptions.SkipVerification);
			}
			stack.EraExpanderOptions.Set(KSoft.Phoenix.Resource.EraFileExpanderOptions.Decrypt);

			if (Flags.Test(MiscFlags.DontOverwriteExistingFiles))
			{
				stack.EraExpanderOptions.Set(KSoft.Phoenix.Resource.EraFileExpanderOptions.DontOverwriteExistingFiles);
			}
			if (Flags.Test(MiscFlags.DecompressUIFiles))
			{
				stack.EraExpanderOptions.Set(KSoft.Phoenix.Resource.EraFileExpanderOptions.DecompressUIFiles);
			}
			if (Flags.Test(MiscFlags.TransformGfxFiles))
			{
				stack.EraExpanderOptions.Set(KSoft.Phoenix.Resource.EraFileExpanderOptions.TranslateGfxFiles);
			}
			if (Flags.Test(MiscFlags.IgnoreNonDataFiles))
			{
				stack.EraExpanderOptions.Set(KSoft.Phoenix.Resource.EraFileExpanderOptions.IgnoreNonDataFiles);
			}
			if (Flags.Test(MiscFlags.DontTranslateXmbFiles))
			{
				stack.EraExpanderOptions.Set(KSoft.Phoenix.Resource.EraFileExpanderOptions.DontTranslateXmbFiles);
			}
			if (Flags.Test(MiscFlags.DontRemoveXmlOrXmbFiles))
			{
				stack.EraExpanderOptions.Set(KSoft.Phoenix.Resource.EraFileExpanderOptions.DontRemoveXmlOrXmbFiles);
			}

			Task.Run((Action)stack.Expand);
		}

		private class ExpandEraFilesStack
		{
			public MainWindowViewModel ViewModel;

			public BitVector32 EraOptions;
			public BitVector32 EraExpanderOptions;
			public string BaseOutputPath;

			public Dispatcher Dispatcher;
			public string[] EraFiles;
			private int mEraFilesIndex;

			public void Expand()
			{
				if (mEraFilesIndex >= EraFiles.Length)
				{
					return;
				}

				var args = new ExpandEraFileParameters(ViewModel.Flags.Test(MiscFlags.UseVerboseOutput))
				{
					EraOptions = EraOptions,
					EraExpanderOptions = EraExpanderOptions,
				};
				string eraFile = EraFiles[mEraFilesIndex++];

				Dispatcher.BeginInvoke(DispatcherPriority.Background,
					new Action(() =>
					{
						ViewModel.StatusText = string.Format("Expanding {0}",
							eraFile);
					}));

				args.EraPath = eraFile;
				args.ListingName = Path.GetFileNameWithoutExtension(eraFile);

				var targetOutputPath = BaseOutputPath;
				if (ViewModel.Flags.Test(MiscFlags.SeparateEraFolders))
				{
					targetOutputPath = Path.Combine(BaseOutputPath, args.ListingName);
					System.IO.Directory.CreateDirectory(targetOutputPath);
				}
				args.OutputPath = targetOutputPath;

				var task = Task.Run(() =>
				{
					return ExpandEraFile(args);
				});

				task.ContinueWith(t =>
				{
					string message_text = "";
					string verbose_output = args.GetVerboseOutput();

					if (t.IsFaulted || t.Result != ExpandEraFileResult.Success)
					{
						bool verbose = ViewModel.Flags.Test(MiscFlags.UseVerboseOutput);

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
								ExpandEraFileResult.Error
								=> "NO HINT",
								ExpandEraFileResult.ReadFailed
								=> "Failed reading ERA file",
								ExpandEraFileResult.ExpandFailed
								=> "Failed expanding archive (do you have the correct game version selected?)",
								_ => "UNKNOWN",
							};
						}

						var sb = new System.Text.StringBuilder();
						sb.Append($"Expand {error_type} ");
						sb.AppendLine(eraFile);
						sb.AppendLine(error_hint);

						message_text = sb.ToString();
					}

					if (!string.IsNullOrEmpty(verbose_output))
					{
						var sb = new System.Text.StringBuilder();
						sb.AppendLine("VerboseOutput:");
						// include the ERA path for context, when dealing with multiple files
						sb.AppendLine(args.EraPath);
						sb.AppendLine(args.VerboseOutput.GetStringBuilder().ToString());
						sb.AppendLine(message_text);

						message_text = sb.ToString();
					}
					if (!string.IsNullOrEmpty(message_text))
					{
						Dispatcher.BeginInvoke(DispatcherPriority.Background,
							new Action(() =>
							{
								ViewModel.MessagesText += message_text;
							}));
					}

					if (mEraFilesIndex < EraFiles.Length)
					{
						Expand();
					}
					else
					{
						Dispatcher.BeginInvoke(DispatcherPriority.Background,
							new Action(() =>
							{
								ViewModel.FinishProcessing();
							}));
					}
				});
			}
		};

		private class ExpandEraFileParameters
		{
			public BitVector32 EraOptions;
			public BitVector32 EraExpanderOptions;
			public StringWriter VerboseOutput;

			public string EraPath;
			public string OutputPath;
			public string ListingName;

			public ExpandEraFileParameters(bool useVerboseOutput)
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
		private enum ExpandEraFileResult
		{
			Success,
			Error,
			ReadFailed,
			ExpandFailed,
		};
		private static ExpandEraFileResult ExpandEraFile(ExpandEraFileParameters args)
		{
			var result = ExpandEraFileResult.Error;
			using (var expander = new KSoft.Phoenix.Resource.EraFileExpander(args.EraPath))
			{
				expander.Options = args.EraOptions;
				expander.ExpanderOptions = args.EraExpanderOptions;
				expander.VerboseOutput = args.VerboseOutput;

				do
				{
					if (!expander.Read())
					{
						result = ExpandEraFileResult.ReadFailed;
						break;
					}

					if (!expander.ExpandTo(args.OutputPath, args.ListingName))
					{
						result = ExpandEraFileResult.ExpandFailed;
						break;
					}

					result = ExpandEraFileResult.Success;
				} while (false);
			}

			return result;
		}
	};
}
