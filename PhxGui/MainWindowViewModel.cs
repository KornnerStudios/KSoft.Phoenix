using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using KSoft;
using KSoft.Collections;

namespace PhxGui
{
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
		[Display(	Name="Ignore non-data files",
					Description="During ERA expansion, only text and .XMB files will be extracted")]
		IgnoreNonDataFiles,

		[Browsable(false)] // no longer letting the user toggle this, they can just use the tool to convert the desired XMBs
		[Display(	Name="Don't automatically translate XMB to XML",
					Description="When expanding, XMB files encountered will not be automatically translated into XML")]
		DontTranslateXmbFiles,
		[Browsable(false)]
		[Display(	Name="Don't automatically remove XMB or XML files",
					Description="When expanding, don't ignore files when both their XMB or XML exists in the ERA")]
		DontRemoveXmlOrXmbFiles,

		[Display(	Name="Always build with XML instead of XMB",
					Description="During ERA generation, if an XMB is referenced but an XML version exists, the XML file will be picked instead")]
		AlwaysUseXmlOverXmb,

		[Display(	Name="Verbose Output",
					Description="When performing operations, include any verbose details")]
		UseVerboseOutput,

		kNumberOf,
	};

	internal partial class MainWindowViewModel
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
		private static KSoft.WPF.BitVectorUserInterfaceData gFlagsUserInterfaceSource;
		public static KSoft.WPF.BitVectorUserInterfaceData FlagsUserInterfaceSource { get {
			if (gFlagsUserInterfaceSource == null)
				gFlagsUserInterfaceSource = KSoft.WPF.BitVectorUserInterfaceData.ForEnum(typeof(MiscFlags));
			return gFlagsUserInterfaceSource;
		} }

		KSoft.Collections.BitVector32 mFlags;
		public KSoft.Collections.BitVector32 Flags
		{
			get { return mFlags; }
			set { this.SetFieldVal(PropertyChanged, ref mFlags, value); }
		}
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
			mFlags.Set(MiscFlags.UseVerboseOutput);

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
			Xex,
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
					case ".xex":
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
					case AcceptedFileType.Xex:
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
					case AcceptedFileType.Xex:
					{
						if (results.FilesCount == 1)
						{
							PatchGameExe(files[0], type);
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
	};
}