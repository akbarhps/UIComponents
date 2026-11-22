using System.Windows;
using System.Windows.Controls;
using Microsoft.Win32;

namespace UIComponents.Wpf
{
    public partial class PickerControls : UserControl
    {
        public PickerControls()
        {
            InitializeComponent();
        }

        private void Browse_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new OpenFileDialog { Multiselect = true, Filter = "All files|*.*" };
            if (dialog.ShowDialog() == true)
                foreach (var file in dialog.FileNames) FileList.Items.Add(file);
        }
    }
}
