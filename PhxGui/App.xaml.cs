using System;
using System.Windows;

namespace PhxGui
{
	/// <summary>
	/// Interaction logic for App.xaml
	/// </summary>
	public partial class App : Application
	{
		public App()
		{
			KSoft.Program.Initialize();
		}

		protected override void OnStartup(StartupEventArgs e)
		{
			base.OnStartup(e);

			KSoft.Phoenix.Program.Initialize();

			var mainWindow = new MainWindow
			{
				DataContext = new MainWindowViewModel(),
			};
			MainWindow = mainWindow;
			mainWindow.Show();
		}

		protected override void OnExit(ExitEventArgs e)
		{
			KSoft.Phoenix.Program.Dispose();
			KSoft.Program.Dispose();

			base.OnExit(e);
		}
	}
}

namespace PhxGui.Properties
{
	partial class Settings
	{
		public GameVersionType[] GameVersionTypeValues
			=> Enum.GetValues<GameVersionType>();
	};
}
