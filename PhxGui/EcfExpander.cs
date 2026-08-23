using KSoft;
using KSoft.Collections;
using KSoft.Phoenix.Resource;
using System;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;

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
			{
				stack.EcfOptions.Set(KSoft.Phoenix.Resource.ECF.EcfFileUtilOptions.x64);
			}
			if (Flags.Test(MiscFlags.SkipVerification))
			{
				stack.EcfOptions.Set(KSoft.Phoenix.Resource.ECF.EcfFileUtilOptions.SkipVerification);
			}

			if (Flags.Test(MiscFlags.DontOverwriteExistingFiles))
			{
				stack.EcfExpanderOptions.Set(KSoft.Phoenix.Resource.ECF.EcfFileExpanderOptions.DontOverwriteExistingFiles);
			}

			Task.Run((Action)stack.Expand);
		}

		private sealed class ExpandEcfFilesStack
		{
			public MainWindowViewModel ViewModel = null!;

			public BitVector32 EcfOptions;
			public BitVector32 EcfExpanderOptions;

			public Dispatcher Dispatcher = null!;
			public string[] EcfFiles = null!;
			private int mEcfFilesIndex;

			public void Expand()
			{
				if (mEcfFilesIndex >= EcfFiles.Length)
				{
					return;
				}

				var args = new ExpandEcfFileParameters(ViewModel.Flags.Test(MiscFlags.UseVerboseOutput))
				{
					EcfOptions = EcfOptions,
					EcfExpanderOptions = EcfExpanderOptions
				};

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
								ExpandEcfFileResult.Error
								=> "NO HINT",
								ExpandEcfFileResult.ReadFailed
								=> "Failed reading ECF file",
								ExpandEcfFileResult.ExpandFailed
								=> "Failed expanding ECF (do you have the correct game version selected?)",
								_ => "UNKNOWN",
							};
						}

						var sb = new System.Text.StringBuilder();
						sb.Append($"Expand {error_type} ");
						sb.AppendLine(ecfFile);
						sb.AppendLine(error_hint);

						message_text = sb.ToString();
					}

					if (!string.IsNullOrEmpty(verbose_output))
					{
						var sb = new System.Text.StringBuilder();
						sb.AppendLine("VerboseOutput:");
						// include the ECF path for context, when dealing with multiple files
						sb.AppendLine(args.EcfPath);
						sb.AppendLine(args.VerboseOutput!.GetStringBuilder().ToString());
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

					if (mEcfFilesIndex < EcfFiles.Length)
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

		private sealed class ExpandEcfFileParameters
		{
			public BitVector32 EcfOptions;
			public BitVector32 EcfExpanderOptions;
			public StringWriter? VerboseOutput;

			public string EcfPath = null!;
			public string ListingName = null!;

			public ExpandEcfFileParameters(bool useVerboseOutput)
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
				string output_path = Path.GetDirectoryName(args.EcfPath)!;

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
