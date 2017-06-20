
namespace KSoft.Phoenix.HaloWars
{
	public enum GameVersionType
	{
		DefinitiveEdition,
		Xbox360,
	};

	public enum DefinitiveEditionSku
	{
		Undefined,

		Steam,
		WindowsStore,
	};
}

namespace KSoft.Phoenix
{
	public static partial class TypeExtensionsPhx
	{
		public static string GetModManifestPath(this HaloWars.DefinitiveEditionSku sku)
		{
			var local_app_data = System.Environment.GetFolderPath(System.Environment.SpecialFolder.LocalApplicationData);
			if (local_app_data.IsNullOrEmpty())
				return null;

			string sku_subdir = null;
			switch (sku)
			{
				case HaloWars.DefinitiveEditionSku.Steam:
					sku_subdir = @"Halo Wars\";
					break;

				case HaloWars.DefinitiveEditionSku.WindowsStore:
					sku_subdir = @"Packages\Microsoft.BulldogThreshold_8wekyb3d8bbwe\LocalState\";
					break;

				default:
					return null;
			}

			string modmanifest_file = sku_subdir + "ModManifest.txt";
			string path = System.IO.Path.Combine(local_app_data, modmanifest_file);

			return path;
		}
	};
}