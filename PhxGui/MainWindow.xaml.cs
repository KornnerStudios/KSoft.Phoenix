using System;
using System.Collections.Generic;
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
using ComponentModel = System.ComponentModel;

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

	internal class MainWindowViewModel
		: ComponentModel.INotifyPropertyChanged
	{
		#region INotifyPropertyChanged
		public event ComponentModel.PropertyChangedEventHandler PropertyChanged;

		protected void NotifyPropertyChanged(ComponentModel.PropertyChangedEventArgs args)
		{
			PropertyChanged.SafeNotify(this, args);
		}
		protected void NotifyPropertiesChanged(ComponentModel.PropertyChangedEventArgs[] argsList, int startIndex = 0)
		{
			PropertyChanged.SafeNotify(this, argsList, startIndex);
		}
		#endregion

		#region StatusText
		static readonly ComponentModel.PropertyChangedEventArgs kStatusTextChanged =
			KSoft.ObjectModel.Util.CreatePropertyChangedEventArgs((MainWindowViewModel x) => x.StatusText);

		string mStatusText;
		public string StatusText {
			get { return mStatusText; }
			set { mStatusText = value;
				NotifyPropertyChanged(kStatusTextChanged);
		} }
		#endregion

		#region ProcessFilesHelpText
		static readonly ComponentModel.PropertyChangedEventArgs kProcessFilesHelpTextChanged =
			KSoft.ObjectModel.Util.CreatePropertyChangedEventArgs((MainWindowViewModel x) => x.ProcessFilesHelpText);

		string mProcessFilesHelpText;
		public string ProcessFilesHelpText {
			get { return mProcessFilesHelpText; }
			set { mProcessFilesHelpText = value;
				NotifyPropertyChanged(kProcessFilesHelpTextChanged);
		} }
		#endregion

		#region MessagesText
		static readonly ComponentModel.PropertyChangedEventArgs kMessagesTextChanged =
			KSoft.ObjectModel.Util.CreatePropertyChangedEventArgs((MainWindowViewModel x) => x.MessagesText);

		string mMessagesText;
		public string MessagesText {
			get { return mMessagesText; }
			set { mMessagesText = value;
				NotifyPropertyChanged(kMessagesTextChanged);
		} }
		#endregion

		#region IsProcessing
		static readonly ComponentModel.PropertyChangedEventArgs kIsProcessingChanged =
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
					if (!t.Result)
					{
						Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Background,
							new Action(() =>
							{
								ViewModel.MessagesText += string.Format("Expand FAILED {0}{1}",
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
				if (!t.Result)
				{
					MessagesText += string.Format("Build FAILED {0}{1}",
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
	};
}
