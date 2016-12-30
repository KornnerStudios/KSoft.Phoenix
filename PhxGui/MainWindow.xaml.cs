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
	};

	public enum MiscFlags
	{
		[Display(	Name="Don't overwrite existing files",
					Description="Files that already exist will not be overwritten (only supported for EXPAND right now!)")]
		DontOverwriteExistingFiles,
		[Display(	Name="Don't automatically translate XMB to XML",
					Description="When expanding, XMB files encountered will not be automatically translated into XML")]
		DontTranslateXmbFiles,
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

		public bool AcceptsFiles(string[] files)
		{
			if (files == null || files.Length == 0)
				return false;

			if (files.All(file => System.IO.Path.GetExtension(file) == ".era"))
			{
				ProcessFilesHelpText = "Expand ERA(s)";
				return true;
			}

			if (files.Length == 1 && System.IO.Path.GetExtension(files[0]) == KSoft.Phoenix.Resource.EraFileBuilder.kNameExtension)
			{
				ProcessFilesHelpText = "Build ERA";
				return true;
			}

			if (files.All(file => System.IO.Path.GetExtension(file) == ".xmb"))
			{
				ProcessFilesHelpText = "XMB->XML";
				return true;
			}

			ProcessFilesHelpText = "Unacceptable file or group of files";
			return false;
		}

		public void ProcessFiles(string[] files)
		{
			if (files == null || files.Length == 0)
				return;

			do
			{
				if (files.All(file => System.IO.Path.GetExtension(file) == ".era"))
				{
					ProcessFilesHelpText = "";
					ProcessEraFiles(files);
					break;
				}

				if (files.Length == 1 && System.IO.Path.GetExtension(files[0]) == KSoft.Phoenix.Resource.EraFileBuilder.kNameExtension)
				{
					ProcessFilesHelpText = "";
					ProcessEraListing(files[0]);
					break;
				}

				if (files.All(file => System.IO.Path.GetExtension(file) == ".xmb"))
				{
					ProcessFilesHelpText = "";
					XmbToXml(files);
					break;
				}
			} while (false);
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
#if false
				MessageBox.Show(this,
					"Specify a valid expand output path",
					"Cannot expand ERA file(s)",
					MessageBoxButton.OK);
#else
				MessagesText = "Cannot expand ERA file(s)" +
					"Specify a valid expand output path";
#endif
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
					if (t.IsFaulted || !t.Result)
					{
						bool faulted = t.IsFaulted;
						Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Background,
							new Action(() =>
							{
								ViewModel.MessagesText += string.Format("Expand {0} {1}{2}",
									faulted ? "EXCEPTION" : "FAILED",
									eraFile, Environment.NewLine);
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
		private static bool ExpandEraFile(ExpandEraFileParameters args)
		{
			bool result = false;
			using (var expander = new KSoft.Phoenix.Resource.EraFileExpander(args.EraPath))
			{
				expander.Options = args.EraOptions;
				expander.ExpanderOptions = args.EraExpanderOptions;

				do
				{
					result = expander.Read();
					if (!result)
						break;

					result = expander.ExpandTo(args.OutputPath, args.ListingName);
					if (!result)
						break;

				} while (false);
			}

			return result;
		}

		private void ProcessEraListing(string eraListing)
		{
			if (!System.IO.Directory.Exists(Properties.Settings.Default.EraBuildOutputPath))
			{
#if false
				MessageBox.Show(this,
					"Specify a valid build output path",
					"Cannot build ERA file",
					MessageBoxButton.OK);
#else
				MessagesText = "Cannot expand ERA file(s)" +
					"Specify a valid expand output path";
#endif
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
				if (t.IsFaulted || !t.Result)
				{
					MessagesText += string.Format("Build {0} {1}{2}",
						t.IsFaulted ? "EXCEPTION" : "FAILED",
						args.ListingPath, Environment.NewLine);
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
		private static bool BuildEraFile(BuildEraFileParameters args)
		{
			bool result = false;
			using (var builder = new KSoft.Phoenix.Resource.EraFileBuilder(args.ListingPath))
			{
				builder.Options = args.EraOptions;
				builder.BuilderOptions = args.EraBuilderOptions;

				do
				{
					result = builder.Read();
					if (!result)
						break;

					result = builder.Build(args.AssetsPath, args.EraName, args.OutputPath);
					if (!result)
						break;

				} while (false);
			}

			return result;
		}

		private void XmbToXml(string[] files)
		{
			ClearMessages();
			IsProcessing = true;

			var task = Task.Run(() =>
			{
				int error_count = 0;
				var va_size = Properties.Settings.Default.GameVersion == GameVersionType.Xbox360
					? KSoft.Shell.ProcessorSize.x32
					: KSoft.Shell.ProcessorSize.x64;
				var endian = KSoft.Shell.EndianFormat.Big;

				var p = Parallel.ForEach(files, f =>
				{
					try
					{
						string xmb_path = f;
						string xml_path = KSoft.Phoenix.Resource.EraFileUtil.RemoveXmbExtension(xmb_path);

						using (var xmb_fs = System.IO.File.OpenRead(xmb_path))
						using (var xmb = new KSoft.IO.EndianStream(xmb_fs, endian, System.IO.FileAccess.Read))
						using (var xml_fs = System.IO.File.Create(xml_path))
						{
							xmb.StreamMode = System.IO.FileAccess.Read;

							KSoft.Phoenix.Resource.EraFileUtil.XmbToXml(xmb, xml_fs, va_size);
						}
					} catch (Exception)
					{
						System.Threading.Interlocked.Increment(ref error_count);
					}

				});
				return error_count;
			});

			var scheduler = TaskScheduler.FromCurrentSynchronizationContext();
			task.ContinueWith(t =>
			{
				if (t.IsFaulted || t.Result > 0)
				{
					MessagesText += string.Format("XMB->XML finished with {0} errors",
						t.IsFaulted ? "FATAL" : t.Result.ToString());
				}

				FinishProcessing();
			}, scheduler);
		}
	};
}
