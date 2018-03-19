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
		private void ProcessEcfFiles(string[] ecfFiles)
		{
			ClearMessages();
			IsProcessing = true;

			var stack = new ExpandEcfFilesStack()
			{
				ViewModel = this,
				Dispatcher = Application.Current.Dispatcher,
				EcfFiles = ecfFiles,
			};
			if (Properties.Settings.Default.GameVersion == GameVersionType.DefinitiveEdition)
				stack.EcfOptions.Set(KSoft.Phoenix.Resource.ECF.EcfFileUtilOptions.x64);
			if (Flags.Test(MiscFlags.SkipVerification))
				stack.EcfOptions.Set(KSoft.Phoenix.Resource.ECF.EcfFileUtilOptions.SkipVerification);

			if (Flags.Test(MiscFlags.DontOverwriteExistingFiles))
				stack.EcfExpanderOptions.Set(KSoft.Phoenix.Resource.ECF.EcfFileExpanderOptions.DontOverwriteExistingFiles);

			Task.Run((Action)stack.Expand);
		}

		private class ExpandEcfFilesStack
		{
			public MainWindowViewModel ViewModel;

			public BitVector32 EcfOptions;
			public BitVector32 EcfExpanderOptions;

			public Dispatcher Dispatcher;
			public string[] EcfFiles;
			private int mEcfFilesIndex;

			public void Expand()
			{
				if (mEcfFilesIndex >= EcfFiles.Length)
					return;

				var args = new ExpandEcfFileParameters(ViewModel.Flags.Test(MiscFlags.UseVerboseOutput));
				args.EcfOptions = EcfOptions;
				args.EcfExpanderOptions = EcfExpanderOptions;

				string ecfFile = EcfFiles[mEcfFilesIndex++];

				Dispatcher.BeginInvoke(DispatcherPriority.Background,
					new Action(() =>
					{
						ViewModel.StatusText = string.Format("Expanding {0}",
							ecfFile);
					}));

				args.EcfPath = ecfFile;
				// We don't use GetFileNameWithoutExtension here because there are cases where
				// files only differ by their extensions (like Terrain data XTD, XSD, etc)
				args.ListingName = ecfFile;

				var task = Task.Run(() =>
				{
					return ExpandEcfFile(args);
				});

				task.ContinueWith(t =>
				{
					string message_text = "";
					string verbose_output = args.GetVerboseOutput();

					if (t.IsFaulted || t.Result != ExpandEcfFileResult.Success)
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
								case ExpandEcfFileResult.Error:
									error_hint = "NO HINT";
									break;
								case ExpandEcfFileResult.ReadFailed:
									error_hint = "Failed reading ECF file";
									break;
								case ExpandEcfFileResult.ExpandFailed:
									error_hint = "Failed expanding ECF (do you have the correct game version selected?)";
									break;
								default:
									error_hint = "UNKNOWN";
									break;
							}
						}
						message_text = string.Format("Expand {0} {1}{2}{3}{4}",
							error_type,
							ecfFile, Environment.NewLine,
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

					if (mEcfFilesIndex < EcfFiles.Length)
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

		private class ExpandEcfFileParameters
		{
			public BitVector32 EcfOptions;
			public BitVector32 EcfExpanderOptions;
			public StringWriter VerboseOutput;

			public string EcfPath;
			public string ListingName;

			public ExpandEcfFileParameters(bool useVerboseOutput)
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
		private enum ExpandEcfFileResult
		{
			Success,
			Error,
			ReadFailed,
			ExpandFailed,
		};
		private static ExpandEcfFileResult ExpandEcfFile(ExpandEcfFileParameters args)
		{
			var result = ExpandEcfFileResult.Error;
			using (var expander = new KSoft.Phoenix.Resource.ECF.EcfFileExpander(args.EcfPath))
			{
				expander.Options = args.EcfOptions;
				expander.ExpanderOptions = args.EcfExpanderOptions;
				expander.VerboseOutput = args.VerboseOutput;
				string output_path = Path.GetDirectoryName(args.EcfPath);

				do
				{
					if (!expander.Read())
					{
						result = ExpandEcfFileResult.ReadFailed;
						break;
					}

					if (!expander.ExpandTo(output_path, args.ListingName))
					{
						result = ExpandEcfFileResult.ExpandFailed;
						break;
					}

					result = ExpandEcfFileResult.Success;
				} while (false);
			}

			return result;
		}
	};
}