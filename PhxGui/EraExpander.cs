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
			if (!System.IO.Directory.Exists(Properties.Settings.Default.EraExpandOutputPath))
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
				OutputPath = Properties.Settings.Default.EraExpandOutputPath,
			};
			if (Properties.Settings.Default.GameVersion == GameVersionType.DefinitiveEdition)
				stack.EraOptions.Set(KSoft.Phoenix.Resource.EraFileUtilOptions.x64);
			stack.EraExpanderOptions.Set(KSoft.Phoenix.Resource.EraFileExpanderOptions.Decrypt);

			if (Flags.Test(MiscFlags.DontOverwriteExistingFiles))
				stack.EraExpanderOptions.Set(KSoft.Phoenix.Resource.EraFileExpanderOptions.DontOverwriteExistingFiles);
			if (Flags.Test(MiscFlags.DecompressUIFiles))
				stack.EraExpanderOptions.Set(KSoft.Phoenix.Resource.EraFileExpanderOptions.DecompressUIFiles);
			if (Flags.Test(MiscFlags.TransformGfxFiles))
				stack.EraExpanderOptions.Set(KSoft.Phoenix.Resource.EraFileExpanderOptions.TranslateGfxFiles);
			if (Flags.Test(MiscFlags.IgnoreNonDataFiles))
				stack.EraExpanderOptions.Set(KSoft.Phoenix.Resource.EraFileExpanderOptions.IgnoreNonDataFiles);
			if (Flags.Test(MiscFlags.DontTranslateXmbFiles))
				stack.EraExpanderOptions.Set(KSoft.Phoenix.Resource.EraFileExpanderOptions.DontTranslateXmbFiles);
			if (Flags.Test(MiscFlags.DontRemoveXmlOrXmbFiles))
				stack.EraExpanderOptions.Set(KSoft.Phoenix.Resource.EraFileExpanderOptions.DontRemoveXmlOrXmbFiles);

			Task.Run((Action)stack.Expand);
		}

		private class ExpandEraFilesStack
		{
			public MainWindowViewModel ViewModel;

			public BitVector32 EraOptions;
			public BitVector32 EraExpanderOptions;
			public string OutputPath;

			public Dispatcher Dispatcher;
			public string[] EraFiles;
			private int mEraFilesIndex;

			public void Expand()
			{
				if (mEraFilesIndex >= EraFiles.Length)
					return;

				var args = new ExpandEraFileParameters(ViewModel.Flags.Test(MiscFlags.UseVerboseOutput));
				args.EraOptions = EraOptions;
				args.EraExpanderOptions = EraExpanderOptions;
				args.OutputPath = OutputPath;

				string eraFile = EraFiles[mEraFilesIndex++];

				Dispatcher.BeginInvoke(DispatcherPriority.Background,
					new Action(() =>
					{
						ViewModel.StatusText = string.Format("Expanding {0}",
							eraFile);
					}));

				args.EraPath = eraFile;
				args.ListingName = Path.GetFileNameWithoutExtension(eraFile);

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
							switch (t.Result)
							{
								case ExpandEraFileResult.Error:
									error_hint = "NO HINT";
									break;
								case ExpandEraFileResult.ReadFailed:
									error_hint = "Failed reading ERA file";
									break;
								case ExpandEraFileResult.ExpandFailed:
									error_hint = "Failed expanding archive (do you have the correct game version selected?)";
									break;
								default:
									error_hint = "UNKNOWN";
									break;
							}
						}
						message_text = string.Format("Expand {0} {1}{2}{3}{4}",
							error_type,
							eraFile, Environment.NewLine,
							error_hint, Environment.NewLine);
					}

					if (!string.IsNullOrEmpty(verbose_output))
					{
						message_text = string.Format("VerboseOutput:{0}{1}{2}" + "{3}{4}",
							Environment.NewLine,
							args.VerboseOutput.GetStringBuilder(), Environment.NewLine,
							message_text, Environment.NewLine);
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
						Expand();
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
					VerboseOutput = new StringWriter(new System.Text.StringBuilder(2048));
			}

			public string GetVerboseOutput()
			{
				string output = "";
				if (VerboseOutput != null)
					output = VerboseOutput.GetStringBuilder().ToString();

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