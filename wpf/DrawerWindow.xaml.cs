using System.Windows;

namespace UIComponents.Wpf
{
    public partial class DrawerWindow : Window
    {
        public DrawerWindow()
        {
            InitializeComponent();
        }

        private void Cancel_Click(object sender, RoutedEventArgs e) { Close(); }
        private void Save_Click(object sender, RoutedEventArgs e) { Close(); }
    }
}
