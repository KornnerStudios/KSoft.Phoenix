using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using KSoft;

namespace PhxGui
{
	partial class MainWindowViewModel
	{
		private enum XmlConverterMode
		{
			XmbToXml,
			XmlToXmb,
		};
		private class XmlConverter
		{
			private XmlConverterMode mMode = XmlConverterMode.XmbToXml;
			public bool DontOverwriteExistingFiles;
			public System.Windows.Threading.Dispatcher Dispatcher;
			public MainWindowViewModel ViewModel;

			private KSoft.Shell.ProcessorSize mVaSize;
			private KSoft.Shell.EndianFormat mEndianFormat;
			private List<string> mInputFiles;

			public XmlConverter(XmlConverterMode mode, MainWindowViewModel viewModel)
			{
				mMode = mode;
				ViewModel = viewModel;
				Dispatcher = Application.Current.Dispatcher;
				DontOverwriteExistingFiles = ViewModel.Flags.Test(MiscFlags.DontOverwriteExistingFiles);
				mVaSize = Properties.Settings.Default.GameVersion == GameVersionType.Xbox360
					? KSoft.Shell.ProcessorSize.x32
					: KSoft.Shell.ProcessorSize.x64;
				mEndianFormat = KSoft.Shell.EndianFormat.Big;
			}

			private bool IsAcceptableInputFile(string inputFile)
			{
				switch (mMode)
				{
					case XmlConverterMode.XmbToXml:
						return KSoft.Phoenix.Resource.ResourceUtils.IsXmbFile(inputFile);
					case XmlConverterMode.XmlToXmb:
						return KSoft.Phoenix.Resource.ResourceUtils.IsXmlBasedFile(inputFile);
					default:
						return false;
				}
			}

			private void GetConversionFiles(string inputFile, out string xmlFile, out string xmbFile, out string outputFile)
			{
				switch (mMode)
				{
					case XmlConverterMode.XmbToXml:
						xmbFile = inputFile;
						xmlFile = KSoft.Phoenix.Resource.ResourceUtils.RemoveXmbExtension(xmbFile);
						outputFile = xmlFile;
						break;

					case XmlConverterMode.XmlToXmb:
						xmlFile = inputFile;
						xmbFile = null; // #TODO
						outputFile = xmbFile;
						break;

					default:
						xmlFile = xmbFile = outputFile = null;
						break;
				}
			}

			public void SetInputFiles(string[] files)
			{
				mInputFiles = new List<string>(files);
			}

			public void SearchDirectoriesForInputFiles(string[] inputDirs)
			{
				mInputFiles = new List<string>();

				foreach (var input_dir in inputDirs)
				{
					var input_files =
						from file in System.IO.Directory.GetFiles(input_dir, "*.*", System.IO.SearchOption.AllDirectories)
						where IsAcceptableInputFile(file)
						select file;

					mInputFiles.AddRange(input_files);
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
						ViewModel.MessagesText += string.Format("EXCEPTION {0}{1}",
							inputFile, Environment.NewLine);
					}));
			}

			public void Convert()
			{
				var p = Parallel.ForEach(mInputFiles, f =>
				{
					try
					{
						string xml_file, xmb_file, output_file;
						GetConversionFiles(f, out xml_file, out xmb_file, out output_file);

						var output_info = new System.IO.FileInfo(output_file);
						if (output_info.Exists)
						{
							if (DontOverwriteExistingFiles)
							{
								NotifyInputFileSkipped(f);
								return;
							}
							else
							{
								if ((output_info.Attributes & System.IO.FileAttributes.ReadOnly) != 0)
								{
									NotifyOutputFileReadOnly(f, output_file);
									return;
								}
							}
						}

						switch (mMode)
						{
							case XmlConverterMode.XmbToXml:
								ConvertXmbToXml(xml_file, xmb_file);
								break;

							case XmlConverterMode.XmlToXmb:
								ConvertXmlToXmb(xml_file, xmb_file);
								break;
						}
					}
					catch (Exception e)
					{
						NotifyInputFileException(f, e);
					}
				});
			}

			private void ConvertXmbToXml(string xmlFile, string xmbFile)
			{
				byte[] file_bytes = System.IO.File.ReadAllBytes(xmbFile);

				using (var xmb_ms = new System.IO.MemoryStream(file_bytes, false))
				using (var xmb = new KSoft.IO.EndianStream(xmb_ms, mEndianFormat, System.IO.FileAccess.Read))
				using (var xml_ms = new System.IO.MemoryStream(IntegerMath.kMega * 1))
				{
					xmb.StreamMode = System.IO.FileAccess.Read;

					KSoft.Phoenix.Resource.ResourceUtils.XmbToXml(xmb, xml_ms, mVaSize);

					using (var xml_fs = System.IO.File.Create(xmlFile))
						xml_ms.WriteTo(xml_fs);
				}
			}

			private void ConvertXmlToXmb(string xmlFile, string xmbFile)
			{
				throw new NotImplementedException();
			}
		};
		private void XmbToXml(string[] files)
		{
			ClearMessages();
			IsProcessing = true;

			var task = Task.Run(() =>
			{
				var converter = new XmlConverter(XmlConverterMode.XmbToXml, this);
				converter.SetInputFiles(files);
				converter.Convert();
			});

			var scheduler = TaskScheduler.FromCurrentSynchronizationContext();
			task.ContinueWith(t =>
			{
				if (t.IsFaulted)
				{
					MessagesText += string.Format("XMB->XML failure {0}{1}",
						Environment.NewLine, t.Exception);
				}

				FinishProcessing();
			}, scheduler);
		}
		private void XmbToXmlInDirectories(string[] dirs)
		{
			ClearMessages();
			IsProcessing = true;

			var task = Task.Run(() =>
			{
				var converter = new XmlConverter(XmlConverterMode.XmbToXml, this);
				converter.SearchDirectoriesForInputFiles(dirs);
				converter.Convert();
			});

			var scheduler = TaskScheduler.FromCurrentSynchronizationContext();
			task.ContinueWith(t =>
			{
				if (t.IsFaulted)
				{
					MessagesText += string.Format("XMB->XML failure {0}{1}",
						Environment.NewLine, t.Exception);
				}

				FinishProcessing();
			}, scheduler);
		}
	};
}