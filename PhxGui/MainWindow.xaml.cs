using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using KSoft;
using KSoft.Collections;

namespace PhxGui
{
	/// <summary>
	/// Interaction logic for MainWindow.xaml
	/// </summary>
	public partial class MainWindow : Window
	{
		private MainWindowViewModel mViewModel = new MainWindowViewModel();

		public MainWindow()
		{
			InitializeComponent();

			base.DataContext = mViewModel;
		}

		protected override void OnClosing(System.ComponentModel.CancelEventArgs e)
		{
			Properties.Settings.Default.Save();
			base.OnClosing(e);
		}

		private void OnDrop(object sender, DragEventArgs e)
		{
			if (mViewModel.IsProcessing)
				return;

			if (e.Data.GetDataPresent(DataFormats.FileDrop))
			{
				string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);

				mViewModel.ProcessFiles(files);
			}
		}

		private void OnPreviewDragOver(object sender, DragEventArgs e)
		{
			if (mViewModel.IsProcessing)
				return;

			e.Handled = true;
		}

		private void OnPreviewDragEnter(object sender, DragEventArgs e)
		{
			e.Effects = DragDropEffects.None;
			if (mViewModel.IsProcessing)
				return;

			if (e.Data.GetDataPresent(DataFormats.FileDrop))
			{
				string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);
				if (mViewModel.AcceptsFiles(files))
				{
					e.Effects = DragDropEffects.Move;
				}
			}
		}

		private void OnPreviewDragLeave(object sender, DragEventArgs e)
		{
			if (mViewModel.IsProcessing)
				return;

			mViewModel.ClearProcessFilesHelpText();
		}

		private void OnMessagesBlockMouseRightButtonDown(object sender, MouseButtonEventArgs e)
		{
			if (mViewModel.IsProcessing)
				return;

			if (!string.IsNullOrWhiteSpace(mViewModel.MessagesText))
				Clipboard.SetText(mViewModel.MessagesText);
		}
	};

	public enum MiscFlags
	{
		[Display(	Name="Don't overwrite existing files",
					Description="Files that already exist will not be overwritten (only supported for EXPAND right now!)")]
		DontOverwriteExistingFiles,
		[Display(	Name="Decompress Scaleform files",
					Description="During ERA expansion, Scaleform files will be decompressed to a matching .BIN file")]
		DecompressUIFiles,
		[Display(	Name="Transform GFX files",
					Description="During ERA expansion, .GFX files will be transformed to matching .SWF file")]
		TransformGfxFiles,

		[Browsable(false)] // no longer letting the user toggle this, they can just use the tool to convert the desired XMBs
		[Display(	Name="Don't automatically translate XMB to XML",
					Description="When expanding, XMB files encountered will not be automatically translated into XML")]
		DontTranslateXmbFiles,
		[Browsable(false)]
		[Display(	Name="Don't automatically remove XMB or XML files",
					Description="When expanding, don't ignore files when both their XMB or XML exists in the ERA")]
		DontRemoveXmlOrXmbFiles,

		kNumberOf,
	};

	internal class MainWindowViewModel
		: INotifyPropertyChanged
	{
		#region INotifyPropertyChanged
		public event PropertyChangedEventHandler PropertyChanged;

		protected void NotifyPropertyChanged(PropertyChangedEventArgs args)
		{
			PropertyChanged.SafeNotify(this, args);
		}
		protected void NotifyPropertiesChanged(PropertyChangedEventArgs[] argsList, int startIndex = 0)
		{
			PropertyChanged.SafeNotify(this, argsList, startIndex);
		}
		#endregion

		#region Flags
		static readonly PropertyChangedEventArgs kFlagsChanged =
			KSoft.ObjectModel.Util.CreatePropertyChangedEventArgs((MainWindowViewModel x) => x.Flags);

		private static KSoft.WPF.BitVectorUserInterfaceData gFlagsUserInterfaceSource;
		public static KSoft.WPF.BitVectorUserInterfaceData FlagsUserInterfaceSource { get {
			if (gFlagsUserInterfaceSource == null)
				gFlagsUserInterfaceSource = KSoft.WPF.BitVectorUserInterfaceData.ForEnum(typeof(MiscFlags));
			return gFlagsUserInterfaceSource;
		} }

		KSoft.Collections.BitVector32 mFlags;
		public KSoft.Collections.BitVector32 Flags {
			get { return mFlags; }
			set { mFlags = value;
				NotifyPropertyChanged(kFlagsChanged);
		} }
		#endregion

		#region StatusText
		static readonly PropertyChangedEventArgs kStatusTextChanged =
			KSoft.ObjectModel.Util.CreatePropertyChangedEventArgs((MainWindowViewModel x) => x.StatusText);

		string mStatusText;
		public string StatusText {
			get { return mStatusText; }
			set { mStatusText = value;
				NotifyPropertyChanged(kStatusTextChanged);
		} }
		#endregion

		#region ProcessFilesHelpText
		static readonly PropertyChangedEventArgs kProcessFilesHelpTextChanged =
			KSoft.ObjectModel.Util.CreatePropertyChangedEventArgs((MainWindowViewModel x) => x.ProcessFilesHelpText);

		string mProcessFilesHelpText;
		public string ProcessFilesHelpText {
			get { return mProcessFilesHelpText; }
			set { mProcessFilesHelpText = value;
				NotifyPropertyChanged(kProcessFilesHelpTextChanged);
		} }
		#endregion

		#region MessagesText
		static readonly PropertyChangedEventArgs kMessagesTextChanged =
			KSoft.ObjectModel.Util.CreatePropertyChangedEventArgs((MainWindowViewModel x) => x.MessagesText);

		string mMessagesText;
		public string MessagesText {
			get { return mMessagesText; }
			set { mMessagesText = value;
				NotifyPropertyChanged(kMessagesTextChanged);
		} }
		#endregion

		#region IsProcessing
		static readonly PropertyChangedEventArgs kIsProcessingChanged =
			KSoft.ObjectModel.Util.CreatePropertyChangedEventArgs((MainWindowViewModel x) => x.IsProcessing);

		bool mIsProcessing;
		public bool IsProcessing {
			get { return mIsProcessing; }
			set { mIsProcessing = value;
				NotifyPropertyChanged(kIsProcessingChanged);
		} }
		#endregion

		public MainWindowViewModel()
		{
			mFlags.Set(MiscFlags.DontTranslateXmbFiles);
			mFlags.Set(MiscFlags.DontRemoveXmlOrXmbFiles);

			ClearStatus();
			ClearProcessFilesHelpText();
			ClearMessages();
		}

		private void ClearStatus()
		{
			StatusText = "Ready...";
		}

		public void ClearProcessFilesHelpText()
		{
			ProcessFilesHelpText = "Drag-n-drop files";
		}

		private void ClearMessages()
		{
			MessagesText = "";
		}

		public enum AcceptedFileType
		{
			Unaccepted,
			Directory,
			Era,
			EraDef,
			Exe,
			Xml,
			Xmb,

			kNumberOf
		};
		public struct AcceptedFilesResults
		{
			public BitVector32 AcceptedFileTypes;
			public int FilesCount;
		};
		public static AcceptedFilesResults DetermineAcceptedFiles(string[] files)
		{
			var results = new AcceptedFilesResults();

			if (files == null || files.Length == 0)
				return results;

			results.FilesCount = files.Length;

			foreach (string path in files)
			{
				if (System.IO.Directory.Exists(path))
				{
					results.AcceptedFileTypes.Set(AcceptedFileType.Directory);
					continue;
				}

				string ext = System.IO.Path.GetExtension(path);
				if (string.IsNullOrEmpty(ext)) // extension-less file
				{
					results.AcceptedFileTypes.Set(AcceptedFileType.Unaccepted);
					continue;
				}

				switch (ext)
				{
					case KSoft.Phoenix.Resource.EraFileUtil.kExtensionEncrypted:
						results.AcceptedFileTypes.Set(AcceptedFileType.Era);
						break;
					case KSoft.Phoenix.Resource.EraFileBuilder.kNameExtension:
						results.AcceptedFileTypes.Set(AcceptedFileType.EraDef);
						break;
					case ".exe":
						results.AcceptedFileTypes.Set(AcceptedFileType.Exe);
						break;
					case ".xmb":
						results.AcceptedFileTypes.Set(AcceptedFileType.Xmb);
						break;

					default:
						if (KSoft.Phoenix.Resource.ResourceUtils.IsXmlBasedFile(path))
						{
							results.AcceptedFileTypes.Set(AcceptedFileType.Xml);
						}
						break;
				}
			}

			return results;
		}

		public bool AcceptsFiles(string[] files)
		{
			var results = DetermineAcceptedFiles(files);
			if (results.FilesCount == 0)
				return false;

			if (results.AcceptedFileTypes.Cardinality != 0 && !results.AcceptedFileTypes.Test(AcceptedFileType.Unaccepted))
			{
				if (AcceptsFilesInternal(results, files))
					return true;
			}

			ProcessFilesHelpText = "Unacceptable file or group of files";
			return false;
		}
		public void ProcessFiles(string[] files)
		{
			var results = DetermineAcceptedFiles(files);
			if (results.FilesCount == 0)
				return;

			if (results.AcceptedFileTypes.Cardinality != 0 && !results.AcceptedFileTypes.Test(AcceptedFileType.Unaccepted))
			{
				if (ProcessFilesInternal(results, files))
					ProcessFilesHelpText = "";
			}
		}

		private bool AcceptsFilesInternal(AcceptedFilesResults results, string[] files)
		{
			foreach (int bitIndex in results.AcceptedFileTypes.SetBitIndices)
			{
				var type = (AcceptedFileType)bitIndex;
				switch (type)
				{
					case AcceptedFileType.EraDef:
					{
						if (results.FilesCount == 1)
						{
							ProcessFilesHelpText = "Build ERA";
							return true;
						}
						break;
					}

					case AcceptedFileType.Exe:
					{
						if (results.FilesCount == 1)
						{
							ProcessFilesHelpText = "Try to patch game EXE for modding";
							return true;
						}
						break;
					}

					case AcceptedFileType.Era:
					{
						if (results.AcceptedFileTypes.Cardinality == 1)
						{
							ProcessFilesHelpText = "Expand ERA(s)";
							return true;
						}
						break;
					}

					case AcceptedFileType.Xmb:
					{
						if (results.AcceptedFileTypes.Cardinality == 1)
						{
							ProcessFilesHelpText = "XMB->XML";
							return true;
						}
						break;
					}

					case AcceptedFileType.Directory:
					{
						if (results.AcceptedFileTypes.Cardinality == 1)
						{
							ProcessFilesHelpText = "XMB->XML (in directories)";
							return true;
						}
						break;
					}
				}
			}

			return false;
		}
		private bool ProcessFilesInternal(AcceptedFilesResults results, string[] files)
		{
			foreach (int bitIndex in results.AcceptedFileTypes.SetBitIndices)
			{
				var type = (AcceptedFileType)bitIndex;
				switch (type)
				{
					case AcceptedFileType.EraDef:
					{
						if (results.FilesCount == 1)
						{
							ProcessEraListing(files[0]);
							return true;
						}
						break;
					}

					case AcceptedFileType.Exe:
					{
						if (results.FilesCount == 1)
						{
							PatchGameExe(files[0]);
							return true;
						}
						break;
					}

					case AcceptedFileType.Era:
					{
						if (results.AcceptedFileTypes.Cardinality == 1)
						{
							ProcessEraFiles(files);
							return true;
						}
						break;
					}

					case AcceptedFileType.Xmb:
					{
						if (results.AcceptedFileTypes.Cardinality == 1)
						{
							XmbToXml(files);
							return true;
						}
						break;
					}

					case AcceptedFileType.Directory:
					{
						if (results.AcceptedFileTypes.Cardinality == 1)
						{
							XmbToXmlInDirectories(files);
							return true;
						}
						break;
					}
				}
			}

			return false;
		}

		private void FinishProcessing()
		{
			ClearStatus();
			ClearProcessFilesHelpText();
			IsProcessing = false;
		}

		private void ProcessEraFiles(string[] eraFiles)
		{
			if (!System.IO.Directory.Exists(Properties.Settings.Default.EraExpandOutputPath))
			{
				MessagesText = "Cannot expand ERA file(s)" +
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

			public System.Windows.Threading.Dispatcher Dispatcher;
			public string[] EraFiles;
			private int mEraFilesIndex;

			public void Expand()
			{
				if (mEraFilesIndex >= EraFiles.Length)
					return;

				var args = new ExpandEraFileParameters();
				args.EraOptions = EraOptions;
				args.EraExpanderOptions = EraExpanderOptions;
				args.OutputPath = OutputPath;

				string eraFile = EraFiles[mEraFilesIndex++];

				Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Background,
					new Action(() =>
					{
						ViewModel.StatusText = string.Format("Expanding {0}",
							eraFile);
					}));

				args.EraPath = eraFile;
				args.ListingName = System.IO.Path.GetFileNameWithoutExtension(eraFile);

				var task = Task.Run(() =>
				{
					return ExpandEraFile(args);
				});

				task.ContinueWith(t =>
				{
					if (t.IsFaulted || t.Result != ExpandEraFileResult.Success)
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
						string messag_text = string.Format("Expand {0} {1}{2}{3}",
							error_type,
							eraFile, Environment.NewLine, error_hint);

						Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Background,
							new Action(() =>
							{
								ViewModel.MessagesText += messag_text;
							}));
					}

					if (mEraFilesIndex < EraFiles.Length)
						Expand();
					else
					{
						Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Background,
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

			public string EraPath;
			public string OutputPath;
			public string ListingName;
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

		private void ProcessEraListing(string eraListing)
		{
			if (!System.IO.Directory.Exists(Properties.Settings.Default.EraBuildOutputPath))
			{
				MessagesText = "Cannot expand ERA file(s)" +
					"Specify a valid expand output path";
				return;
			}

			ClearMessages();
			IsProcessing = true;

			var args = new BuildEraFileParameters();
			if (Properties.Settings.Default.GameVersion == GameVersionType.DefinitiveEdition)
				args.EraOptions.Set(KSoft.Phoenix.Resource.EraFileUtilOptions.x64);
			args.EraBuilderOptions.Set(KSoft.Phoenix.Resource.EraFileBuilderOptions.Encrypt);

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
				switch(mMode)
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

		private void PatchGameExe(string exeFile)
		{
			ClearMessages();
			IsProcessing = true;

			var task = Task.Run(() =>
			{
				var exe_file_attrs = System.IO.File.GetAttributes(exeFile);
				if (exe_file_attrs.HasFlag(System.IO.FileAttributes.ReadOnly))
				{
					return string.Format("ERROR Cannot patch read-only file (this tool creates a backup): {0}",
						exeFile);
				}

				{
					string backup_file = System.IO.Path.GetFileNameWithoutExtension(exeFile);
					backup_file += "_UNTOUCHED.exe";
					backup_file = System.IO.Path.Combine(System.IO.Path.GetDirectoryName(exeFile), backup_file);
					System.IO.File.Copy(exeFile, backup_file);
				}

				byte[] exe_file_sha1_bytes = null;
				using (var fs = System.IO.File.OpenRead(exeFile))
				using (var sha1_provider = new System.Security.Cryptography.SHA1CryptoServiceProvider())
				{
					exe_file_sha1_bytes = sha1_provider.ComputeHash(fs);
				}

				var exe_file_sha1 = KSoft.Text.Util.ByteArrayToString(exe_file_sha1_bytes);
				ExePatching.PatchInfo exe_paches;
				if (!ExePatching.TryGetPatchInfo(exe_file_sha1, out exe_paches))
				{
					return string.Format("ERROR Unrecongized file: {0}" +
						"SHA1={1}{2}" +
						"File={3}{4}",
						Environment.NewLine,
						exe_file_sha1, Environment.NewLine,
						exeFile, Environment.NewLine);
				}

				using (var fs = System.IO.File.OpenWrite(exeFile))
				{
					foreach (var kvp in exe_paches.Patches)
					{
						fs.Seek(kvp.Key, System.IO.SeekOrigin.Begin);
						fs.Write(kvp.Value, 0, kvp.Value.Length);
					}
				}

				return exeFile;
			});

			var scheduler = TaskScheduler.FromCurrentSynchronizationContext();
			task.ContinueWith(t =>
			{
				if (t.IsFaulted || t.Result.StartsWith("ERROR", StringComparison.Ordinal))
				{
					MessagesText += string.Format("Patch EXE finished with errors: {0}{1}",
						Environment.NewLine,
						t.IsFaulted ? t.Exception.ToString() : t.Result);
				}
				else
				{
					MessagesText = string.Format("EXE is now ready for modding use: {0}",
						t.Result);
				}

				FinishProcessing();
			}, scheduler);
		}
	};

	static class ExePatching
	{
		public sealed class PatchInfo
		{
			public string Sha1;
			public Dictionary<uint, byte[]> Patches = new Dictionary<uint,byte[]>();

			public PatchInfo(string sha1)
			{
				Sha1 = sha1;
			}

			public PatchInfo Add(uint offset, params byte[] newBytes)
			{
				Patches[offset] = newBytes;
				return this;
			}
		};
		private static List<PatchInfo> kPatches = new List<PatchInfo>();

		static ExePatching()
		{
			var v1_11088_1_2 = new PatchInfo("2C1E144727CFF2AADDAE6BB71EE66B7820D3E163")
				.Add(0x6D971F, 0xE9, 0x0A, 0x01, 0x00, 0x00);
			kPatches.Add(v1_11088_1_2);
		}

		public static bool TryGetPatchInfo(string actualSha1, out PatchInfo info)
		{
			info = null;

			info = (from i in kPatches
					where i.Sha1 == actualSha1
					select i).FirstOrDefault();

			return info != null;
		}
	};
}
