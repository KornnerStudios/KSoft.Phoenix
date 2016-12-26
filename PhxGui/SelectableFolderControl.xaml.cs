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
using WinForms = System.Windows.Forms;

namespace PhxGui
{
	/// <summary>
	/// Interaction logic for SelectableFolderControl.xaml
	/// </summary>
	public partial class SelectableFolderControl : UserControl
	{
		public SelectableFolderControl()
		{
			InitializeComponent();
		}

		public static DependencyProperty TextProperty = DependencyProperty.Register("Text",
			typeof(string), typeof(SelectableFolderControl), new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));
		public static DependencyProperty DescriptionProperty = DependencyProperty.Register("Description",
			typeof(string), typeof(SelectableFolderControl), new PropertyMetadata(null));

		public string Text { get { return GetValue(TextProperty) as string; } set { SetValue(TextProperty, value); } }

		public string Description { get { return GetValue(DescriptionProperty) as string; } set { SetValue(DescriptionProperty, value); } }

		private void OnBrowseFolder(object sender, RoutedEventArgs e)
		{
			using (var dlg = new WinForms.FolderBrowserDialog())
			{
				dlg.Description = Description;
				dlg.SelectedPath = Text;
				dlg.ShowNewFolderButton = true;
				var result = dlg.ShowDialog();
				if (result == WinForms.DialogResult.OK)
				{
					Text = dlg.SelectedPath;
					BindingExpression be = GetBindingExpression(TextProperty);
					if (be != null)
					{
						// Textbox bindings are only updated on the lostfocus event.
						be.UpdateSource();
					}
				}
			}
		}
	};
}
