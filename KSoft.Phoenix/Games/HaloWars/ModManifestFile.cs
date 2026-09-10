using System;
using System.Collections.ObjectModel;
using System.IO;

namespace KSoft.Phoenix.HaloWars
{
	public sealed partial class ModManifestFile
		: ObjectModel.BasicViewModel
	{
		#region Sku
		DefinitiveEditionSku mSku = DefinitiveEditionSku.Undefined;
		[KSoft.PropertyChanged.SourceGeneration.GeneratedPropertyChangedEventArgs]
		public DefinitiveEditionSku Sku
		{
			get { return mSku; }
			set
			{
				if (this.SetFieldEnum(ref mSku, value, kSkuChangedEventArgs))
				{
					FilePath = Sku.GetModManifestPath();
				}
			}
		}
		#endregion

		#region FilePath
		string? mFilePath;
		public string? FilePath
		{
			get { return mFilePath; }
			[System.Diagnostics.CodeAnalysis.SuppressMessage("Usage", "CA2245:Do not assign a property to itself",
				Justification = "This is how OnPropertyChanged fires")]
			set
			{
				if (!string.Equals(mFilePath, value, System.StringComparison.Ordinal))
				{
					mFilePath = value;
					OnPropertyChanged();
					ContainingFolder = ContainingFolder;
					DisplayTitle = DisplayTitle;
				}
			}
		}
		#endregion

		public string? ContainingFolder
		{
			get
			{
				if (FilePath.IsNullOrEmpty())
				{
					return null;
				}

				string? path = FilePath;
				path = Path.GetDirectoryName(path);
				return path;
			}
			private set { OnPropertyChanged(); }
		}

		public string GetDisplayTitle(IFormatProvider provider)
		{
			ArgumentNullException.ThrowIfNull(provider);

			return string.Create(provider, $"{Sku} ModManifest - {FilePath}");
		}

		public string DisplayTitle
		{
			get { return GetDisplayTitle(System.Globalization.CultureInfo.CurrentCulture); }
			set { this.OnPropertyChanged(); }
		}

		public ObservableCollection<ModManifestDirectory> Directories { get; private set; }
			= new();

		public void ReadFromFile()
		{
			if (!File.Exists(FilePath))
			{
				return;
			}

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
			var filePath = FilePath;
			if (filePath is null || !Directory.Exists(ContainingFolder))
			{
				return;
			}

			using (var sw = new StreamWriter(filePath))
			{
				var line = new System.Text.StringBuilder(512);

				foreach (var dir in Directories)
				{
					line.Clear();
					if (!dir.WriteToLine(line))
					{
						continue;
					}

					sw.WriteLine(line);
				}
			}
		}
	};

	public sealed partial class ModManifestDirectory
		: ObjectModel.BasicViewModel
	{
		const char kDisabledPrefix = ';';

		#region IsDisabled
		bool mIsDisabled;
		[KSoft.PropertyChanged.SourceGeneration.GeneratedPropertyChanged(BackingField = nameof(mIsDisabled))]
		public partial bool IsDisabled { get; set; }
		#endregion

		#region Directory
		string mDirectory = null!;
		public string Directory
		{
			get { return mDirectory; }
			[System.Diagnostics.CodeAnalysis.SuppressMessage("Usage", "CA2245:Do not assign a property to itself",
				Justification = "This is how OnPropertyChanged fires")]
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
			{
				return false;
			}

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
			{
				return false;
			}

			if (IsDisabled)
			{
				line.Append(kDisabledPrefix);
			}

			line.Append(Directory);

			return true;
		}
	};
}