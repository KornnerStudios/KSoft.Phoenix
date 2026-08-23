using KSoft;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using System.Windows;

using PhxWinSaveFile = KSoft.Phoenix.Resource.SAV.WinSaveFile;

namespace PhxGui;

partial class MainWindowViewModel
{
	private void WinSaveFilesDecrypt(string[] files)
	{
		ClearMessages();
		IsProcessing = true;

		var task = Task.Run(() =>
		{
			var converter = new WinSaveFileConverter(WinSaveFileConverterMode.Decrypt, this);
			converter.SetInputFiles(files);
			converter.Convert();
		});

		var scheduler = TaskScheduler.FromCurrentSynchronizationContext();
		task.ContinueWith(t =>
		{
			if (t.IsFaulted)
			{
				MessagesText += string.Format("SAV decrypt failure {0}{1}",
					Environment.NewLine, t.Exception.GetOnlyExceptionOrAll());
			}

			FinishProcessing();
		}, scheduler);
	}

	private enum WinSaveFileConverterMode
	{
		Decrypt,
		Encrypt,
	};

	sealed class WinSaveFileConverter
	{
		private readonly WinSaveFileConverterMode mMode = WinSaveFileConverterMode.Decrypt;
		public bool DontOverwriteExistingFiles;
		public System.Windows.Threading.Dispatcher Dispatcher;
		public MainWindowViewModel ViewModel;

		private List<string> mInputFiles = null!;

		public WinSaveFileConverter(WinSaveFileConverterMode mode, MainWindowViewModel viewModel)
		{
			mMode = mode;
			ViewModel = viewModel;
			Dispatcher = Application.Current.Dispatcher;
			DontOverwriteExistingFiles = ViewModel.Flags.Test(MiscFlags.DontOverwriteExistingFiles);
		}

		public void SetInputFiles(string[] files)
		{
			mInputFiles = new List<string>(files);
		}

		public void Convert()
		{
			var p = Parallel.ForEach(mInputFiles, f =>
			{
				try
				{
					GetConversionFiles(f,
						out string savDecryptedFile, out string savFile, out string output_file);

					var output_info = new FileInfo(output_file);
					if (output_info.Exists)
					{
						if (DontOverwriteExistingFiles)
						{
							NotifyInputFileSkipped(f);
							return;
						}
						else
						{
							if ((output_info.Attributes & FileAttributes.ReadOnly) != 0)
							{
								NotifyOutputFileReadOnly(f, output_file);
								return;
							}
						}
					}

					switch (mMode)
					{
						case WinSaveFileConverterMode.Decrypt:
							DecryptSav(savDecryptedFile, savFile);
							break;

						case WinSaveFileConverterMode.Encrypt:
							EncryptSav(savDecryptedFile, savFile);
							break;
					}
				}
				catch (Exception e)
				{
					NotifyInputFileException(f, e);
				}
			});
		}

		private void DecryptSav(string savDecryptedFile, string savFile)
		{
			PhxWinSaveFile.OpenSavResult openSavResult = PhxWinSaveFile.OpenSav(savFile,
				out MemoryStream? savFileStream);

			switch (openSavResult)
			{
				case PhxWinSaveFile.OpenSavResult.Invalid:
					NotifyInputFileSkippedDueToInvalid(savFile);
					return;

				case PhxWinSaveFile.OpenSavResult.Valid:
					NotifyInputFileSkippedDueToNotEncrypted(savFile);
					return;

				case PhxWinSaveFile.OpenSavResult.ValidAndNeedsDecryption:
					PhxWinSaveFile.DecryptUserProfileData(savFileStream!.GetBuffer());
					System.IO.File.WriteAllBytes(savDecryptedFile, savFileStream.GetBuffer());
					break;

				default:
					throw new KSoft.Debug.UnreachableException(openSavResult.ToString());
			}
		}

		private void EncryptSav(string savDecryptedFile, string savFile)
		{
			// #TODO
			throw new NotImplementedException();
		}

		private void GetConversionFiles(string inputFile, out string savDecryptedFile, out string savFile, out string outputFile)
		{
			switch (mMode)
			{
				case WinSaveFileConverterMode.Decrypt:
					savFile = inputFile;
					savDecryptedFile = Path.ChangeExtension(savFile, KSoft.Phoenix.Resource.SAV.WinSaveFile.kExtensionDecrypted);
					outputFile = savDecryptedFile;
					break;

				case WinSaveFileConverterMode.Encrypt:
					savDecryptedFile = inputFile;
					savFile = Path.ChangeExtension(savDecryptedFile, KSoft.Phoenix.Resource.SAV.WinSaveFile.kExtensionEncrypted);
					outputFile = savFile;
					break;

				default:
					savDecryptedFile = savFile = outputFile = null!;
					break;
			}
		}

		private void NotifyInputFileSkipped(string inputFile)
		{
			Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Background,
				new Action(() =>
				{
					ViewModel.MessagesText += string.Format("Skipped due to existing output {0}{1}",
						inputFile, Environment.NewLine);
				}));
		}

		private void NotifyOutputFileReadOnly(string inputFile, string /*outputFile*/_)
		{
			Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Background,
				new Action(() =>
				{
					ViewModel.MessagesText += string.Format("Skipped due to read-only output {0}{1}",
						inputFile, Environment.NewLine);
				}));
		}

		private void NotifyInputFileException(string inputFile, Exception e)
		{
			Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Background,
				new Action(() =>
				{
					ViewModel.MessagesText += string.Format("EXCEPTION {0}{1}{2}",
						inputFile, Environment.NewLine, e);
				}));
		}

		private void NotifyInputFileSkippedDueToInvalid(string inputFile)
		{
			Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Background,
				new Action(() =>
				{
					ViewModel.MessagesText += string.Format("Skipped as the file is not valid {0}{1}",
						inputFile, Environment.NewLine);
				}));
		}

		private void NotifyInputFileSkippedDueToNotEncrypted(string inputFile)
		{
			Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Background,
				new Action(() =>
				{
					ViewModel.MessagesText += string.Format("Skipped as the file is not encrypted {0}{1}",
						inputFile, Environment.NewLine);
				}));
		}
	};
};
