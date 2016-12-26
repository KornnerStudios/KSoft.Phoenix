using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
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
		public IEnumerable<GameVersionType> GameVersionTypeValues { get {
			return Enum.GetValues(typeof(GameVersionType))
				.Cast<GameVersionType>();
		} }
	};
}
