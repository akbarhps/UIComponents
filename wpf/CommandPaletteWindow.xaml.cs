using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace UIComponents.Wpf
{
    public partial class CommandPaletteWindow : Window
    {
        public string SelectedCommand { get; private set; }

        public CommandPaletteWindow()
        {
            InitializeComponent();
            Search.Focus();
        }

        private void Search_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape) Close();
            else if (e.Key == Key.Enter && Commands.SelectedItem != null)
            {
                SelectedCommand = Commands.SelectedItem.ToString();
                Close();
            }
        }

        private void Commands_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
        }
    }
}
