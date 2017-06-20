using System.Collections.ObjectModel;
using System.IO;

namespace KSoft.Phoenix.HaloWars
{
	public sealed class ModManifestFile
		: ObjectModel.BasicViewModel
	{
		#region Sku
		DefinitiveEditionSku mSku = DefinitiveEditionSku.Undefined;
		public DefinitiveEditionSku Sku
		{
			get { return mSku; }
			set
			{
				if (this.SetFieldEnum(ref mSku, value))
				{
					FilePath = Sku.GetModManifestPath();
				}
			}
		}
		#endregion

		#region FilePath
		string mFilePath;
		public string FilePath
		{
			get { return mFilePath; }
			set
			{
				if (this.SetFieldObj(ref mFilePath, value))
				{
					ContainingFolder = ContainingFolder;
					DisplayTitle = DisplayTitle;
				}
			}
		}
		#endregion

		public string ContainingFolder
		{
			get
			{
				if (FilePath.IsNullOrEmpty())
					return null;

				string path = FilePath;
				path = Path.GetDirectoryName(path);
				return path;
			}
			private set { OnPropertyChanged(); }
		}

		public string DisplayTitle
		{
			get { return string.Format("{0} ModManifest - {1}", Sku, FilePath); }
			set { this.OnPropertyChanged(); }
		}

		public ObservableCollection<ModManifestDirectory> Directories { get; private set; }
			= new ObservableCollection<ModManifestDirectory>();

		public void ReadFromFile()
		{
			if (!File.Exists(FilePath))
				return;

			string[] lines = File.ReadAllLines(FilePath);

			Directories.Clear();

			for (int x = 0; x < lines.Length; x++)
			{
				string line = lines[x];

				var dir = new ModManifestDirectory();
				if (!dir.ReadFromLine(line))
				{
					continue;
				}

				Directories.Add(dir);
			}
		}

		public void WriteToFile()
		{
			if (!Directory.Exists(ContainingFolder))
				return;

			using (var sw = new StreamWriter(FilePath))
			{
				var line = new System.Text.StringBuilder(512);

				foreach (var dir in Directories)
				{
					line.Clear();
					if (!dir.WriteToLine(line))
						continue;

					sw.WriteLine(line);
				}
			}
		}
	};

	public sealed class ModManifestDirectory
		: ObjectModel.BasicViewModel
	{
		const char kDisabledPrefix = ';';

		#region IsDisabled
		bool mIsDisabled;
		public bool IsDisabled
		{
			get { return mIsDisabled; }
			set { this.SetFieldVal(ref mIsDisabled, value); }
		}
		#endregion

		#region Directory
		string mDirectory;
		public string Directory
		{
			get { return mDirectory; }
			set
			{
				if (this.SetFieldObj(ref mDirectory, value))
				{
					// refresh validity
					IsValid = IsValid;
					DoesExist = DoesExist;
				}
			}
		}
		#endregion

		#region IsValid
		public bool IsValid
		{
			get { return Directory.IsNotNullOrEmpty(); }
			private set
			{
				OnPropertyChanged();
			}
		}
		#endregion

		#region DoesExist
		public bool DoesExist
		{
			get { return IsValid && System.IO.Directory.Exists(Directory); }
			private set
			{
				OnPropertyChanged();
			}
		}
		#endregion

		public bool ReadFromLine(string line)
		{
			if (line.IsNullOrEmpty())
				return false;

			IsDisabled = false;

			int directory_start_index = 0;
			if (line.StartsWith(kDisabledPrefix))
			{
				IsDisabled = true;
				directory_start_index = 1;
			}

			string dir = line.Substring(directory_start_index);
			var invalid_chars = Path.GetInvalidPathChars();

			foreach (char c in dir)
			{
				foreach (char invalid_char in invalid_chars)
				{
					if (c == invalid_char)
					{
						return false;
					}
				}
			}

			Directory = dir;

			return true;
		}

		public bool WriteToLine(System.Text.StringBuilder line)
		{
			if (!IsValid)
				return false;

			if (IsDisabled)
			{
				line.Append(kDisabledPrefix);
			}

			line.Append(Directory);

			return true;
		}
	};
}