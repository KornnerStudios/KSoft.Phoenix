using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Windows;
using KSoft;

namespace PhxGui
{
	/// <summary>
	/// Interaction logic for App.xaml
	/// </summary>
	public partial class App : Application
	{
		public static List<TraceSource> AllTraceSources { get; } = KSoft.Debug.AssemblyTraceSourcesCollector.FromClasses(
			null,
			KSoft.Program.DebugTraceClass,
			KSoft.Phoenix.Program.DebugTraceClass,
			KSoft.Wwise.Program.DebugTraceClass,
			typeof(Debug.Trace))
			.SortAndReturn(KSoft.Debug.AssemblyTraceSourcesCollector.CompareTraceSourcesByName);

		public App()
		{
			KSoft.Program.Initialize();
			KSoft.Program.RegisterTraceSources(AllTraceSources);
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
