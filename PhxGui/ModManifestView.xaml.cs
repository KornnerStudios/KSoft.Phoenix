using System;
using System.Windows;
using System.Windows.Input;
using KSoft;
using WinForms = System.Windows.Forms;

namespace PhxGui
{
	public partial class ModManifestView : Window
	{
		private KSoft.Phoenix.HaloWars.ModManifestFile ModManifest { get {
			return this.DataContext as KSoft.Phoenix.HaloWars.ModManifestFile;
		} }
		private KSoft.Phoenix.HaloWars.ModManifestDirectory SelectedModManifestDirectory { get {
			return DirectoriesDataGrid.SelectedItem as KSoft.Phoenix.HaloWars.ModManifestDirectory;
		} }

		public ModManifestView()
		{
			InitializeComponent();
		}

		private void OnWindowLoaded(object sender, RoutedEventArgs e)
		{
			var modmanifest = ModManifest;
			if (modmanifest == null)
				return;

			modmanifest.ReadFromFile();
		}

		private int SelectedModManifestDirectoryIndex { get {
			var modmanifest = ModManifest;
			if (modmanifest == null)
				return TypeExtensions.kNone;

			int selected_index = DirectoriesDataGrid.SelectedIndex;
			if (selected_index.IsNone())
				return TypeExtensions.kNone;

			// deals with NewItemPlaceholder crap
			if (selected_index >= modmanifest.Directories.Count)
				return TypeExtensions.kNone;

			return selected_index;
		} }

		private bool ReadManifestFromFile()
		{
			var manifest = ModManifest;
			if (manifest == null)
				return false;

			try
			{
				manifest.ReadFromFile();
			} catch (Exception e)
			{
				Debug.Trace.PhxGui.TraceData(System.Diagnostics.TraceEventType.Error, -1,
					"Failed to load ModManifest",
					manifest.DisplayTitle,
					e);

				MessageBox.Show(this,
					"See error log for more details",
					"Failed to load ModManifest",
					MessageBoxButton.OK);

				return false;
			}

			return true;
		}

		private bool WriteManifestFromFile()
		{
			var manifest = ModManifest;
			if (manifest == null)
				return false;

			try
			{
				manifest.WriteToFile();
			}
			catch (Exception e)
			{
				Debug.Trace.PhxGui.TraceData(System.Diagnostics.TraceEventType.Error, -1,
					"Failed to write ModManifest",
					manifest.DisplayTitle,
					e);

				MessageBox.Show(this,
					"See error log for more details",
					"Failed to write ModManifest",
					MessageBoxButton.OK);

				return false;
			}

			return true;
		}

		private void OnDirectoriesDataGridDoubleClick(object sender, MouseButtonEventArgs e)
		{
			var item = SelectedModManifestDirectory;
			if (item == null)
				return;

			using (var dlg = new WinForms.FolderBrowserDialog())
			{
				dlg.Description =
					"Select directory with modded files\n" +
					"(selected folder should contain a 'art', 'data', etc subfolder)";
				dlg.SelectedPath = item.Directory;
				dlg.ShowNewFolderButton = true;
				var result = dlg.ShowDialog();
				if (result == WinForms.DialogResult.OK)
				{
					item.Directory = dlg.SelectedPath;
				}
			}
		}

		private void OnMoveRowUpClicked(object sender, RoutedEventArgs e)
		{
			int selected_index = SelectedModManifestDirectoryIndex;
			if (selected_index.IsNone())
				return;

			if (selected_index == 0)
				return;

			ModManifest.Directories.Move(selected_index, selected_index - 1);
		}

		private void OnMoveRowDownClicked(object sender, RoutedEventArgs e)
		{
			int selected_index = SelectedModManifestDirectoryIndex;
			if (selected_index.IsNone())
				return;

			if (selected_index == ModManifest.Directories.Count-1)
				return;

			ModManifest.Directories.Move(selected_index, selected_index + 1);
		}

		private void OnAddRowClicked(object sender, RoutedEventArgs e)
		{
			ModManifest.Directories.Add(new KSoft.Phoenix.HaloWars.ModManifestDirectory());
		}

		private void OnDeleteRowClicked(object sender, RoutedEventArgs e)
		{
			int selected_index = SelectedModManifestDirectoryIndex;
			if (selected_index.IsNone())
				return;

			ModManifest.Directories.RemoveAt(selected_index);
		}

		private void OnOpenModDirectoryClicked(object sender, RoutedEventArgs e)
		{
			var item = SelectedModManifestDirectory;
			if (item == null)
				return;

			if (!item.IsValid)
			{
				MessageBox.Show(this,
					"No path has been set yet",
					"Can not open mod folder",
					MessageBoxButton.OK);
				return;
			}

			string path = item.Directory;

			if (!System.IO.Directory.Exists(path))
			{
				MessageBox.Show(this,
					"Path does not exist: " + path,
					"Can not open mod folder",
					MessageBoxButton.OK);
				return;
			}

			System.Diagnostics.Process.Start(path);
		}

		private void OnOpenModManifestDirectoryClicked(object sender, RoutedEventArgs e)
		{
			var manifest = ModManifest;
			if (manifest == null)
				return;

			string path = manifest.ContainingFolder;

			if (!System.IO.Directory.Exists(path))
			{
				MessageBox.Show(this,
					"Path does not exist: " + path,
					"Can not open folder",
					MessageBoxButton.OK);
				return;
			}

			System.Diagnostics.Process.Start(path);
		}

		private void OnFileSaveClicked(object sender, RoutedEventArgs e)
		{
			WriteManifestFromFile();
		}
		private void OnFileReloadClicked(object sender, RoutedEventArgs e)
		{
			ReadManifestFromFile();
		}
	};
}
