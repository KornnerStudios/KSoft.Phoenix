using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using KSoft;

namespace PhxGui
{
	partial class MainWindowViewModel
	{
		private void BinaryDataTreeBinToXml(string[] files)
		{
			ClearMessages();
			IsProcessing = true;

			var task = Task.Run(() =>
			{
				var converter = new BinaryDataTreeConverter(BinaryDataTreeConverterMode.BinToXml, this);
				converter.SetInputFiles(files);
				converter.Convert();
			});

			var scheduler = TaskScheduler.FromCurrentSynchronizationContext();
			task.ContinueWith(t =>
			{
				if (t.IsFaulted)
				{
					MessagesText += string.Format("BinaryDataTree BIN->XML failure {0}{1}",
						Environment.NewLine, t.Exception.GetOnlyExceptionOrAll());
				}

				FinishProcessing();
			}, scheduler);
		}

		private enum BinaryDataTreeConverterMode
		{
			BinToXml,
			XmlToBin,
		};
		private class BinaryDataTreeConverter
		{
			private BinaryDataTreeConverterMode mMode = BinaryDataTreeConverterMode.BinToXml;
			public bool DontOverwriteExistingFiles;
			public bool DontDecompileAttributesWithTypeData;
			public System.Windows.Threading.Dispatcher Dispatcher;
			public MainWindowViewModel ViewModel;

			private KSoft.Shell.ProcessorSize mVaSize;
			private KSoft.Shell.EndianFormat mEndianFormat;
			private List<string> mInputFiles;

			public BinaryDataTreeConverter(BinaryDataTreeConverterMode mode, MainWindowViewModel viewModel)
			{
				mMode = mode;
				ViewModel = viewModel;
				Dispatcher = Application.Current.Dispatcher;
				DontOverwriteExistingFiles = ViewModel.Flags.Test(MiscFlags.DontOverwriteExistingFiles);
				DontDecompileAttributesWithTypeData = false;
				mVaSize = Properties.Settings.Default.GameVersion == GameVersionType.Xbox360
					? KSoft.Shell.ProcessorSize.x32
					: KSoft.Shell.ProcessorSize.x64;
				mEndianFormat = KSoft.Shell.EndianFormat.Big;
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
						string xml_file, xmb_file, output_file;
						GetConversionFiles(f, out xml_file, out xmb_file, out output_file);

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
							case BinaryDataTreeConverterMode.BinToXml:
								ConvertBinToXml(xml_file, xmb_file);
								break;

							case BinaryDataTreeConverterMode.XmlToBin:
								ConvertXmlToBin(xml_file, xmb_file);
								break;
						}
					}
					catch (Exception e)
					{
						NotifyInputFileException(f, e);
					}
				});
			}

			private void ConvertBinToXml(string xmlFile, string xmbFile)
			{
				var bdt = new KSoft.Phoenix.Xmb.BinaryDataTree();
				bdt.DecompileAttributesWithTypeData = DontDecompileAttributesWithTypeData == false;

				byte[] bdt_bytes;
				using (var fs = File.OpenRead(xmbFile))
				{
					bdt_bytes = new byte[fs.Length];
					int bytes_read = fs.Read(bdt_bytes, 0, bdt_bytes.Length);
					if (bytes_read != bdt_bytes.Length)
						throw new IOException("Failed to read all BDT bytes");
				}

				using (var bdt_ms = new MemoryStream(bdt_bytes))
				using (var es = new KSoft.IO.EndianStream(bdt_ms, KSoft.Shell.EndianFormat.Big, permissions: FileAccess.Read))
				{
					es.StreamMode = FileAccess.Read;

					bdt.Serialize(es);
				}

				bdt.ToXml(xmlFile);
			}

			private void ConvertXmlToBin(string xmlFile, string xmbFile)
			{
				throw new NotImplementedException();
			}

			private void GetConversionFiles(string inputFile, out string xmlFile, out string xmbFile, out string outputFile)
			{
				switch (mMode)
				{
					case BinaryDataTreeConverterMode.BinToXml:
						xmbFile = inputFile;
						xmlFile = Path.ChangeExtension(xmbFile, KSoft.Phoenix.Xmb.BinaryDataTree.kTextFileExtension);
						outputFile = xmlFile;
						break;

					case BinaryDataTreeConverterMode.XmlToBin:
						xmlFile = inputFile;
						xmbFile = Path.ChangeExtension(xmlFile, KSoft.Phoenix.Xmb.BinaryDataTree.kBinaryFileExtension);
						outputFile = xmbFile;
						break;

					default:
						xmlFile = xmbFile = outputFile = null;
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

			private void NotifyOutputFileReadOnly(string inputFile, string outputFile)
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
		};
	};
}