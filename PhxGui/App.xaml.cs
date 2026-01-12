using System;
using System.Windows;

namespace PhxGui
{
	/// <summary>
	/// Interaction logic for App.xaml
	/// </summary>
	public partial class App : Application
	{
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
