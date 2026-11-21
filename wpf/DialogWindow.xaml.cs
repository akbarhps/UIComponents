using System.Windows;

namespace UIComponents.Wpf
{
    public partial class DialogWindow : Window
    {
        public bool Confirmed { get; private set; }

        public DialogWindow()
        {
            InitializeComponent();
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            Confirmed = false;
            Close();
        }

        private void Delete_Click(object sender, RoutedEventArgs e)
        {
            Confirmed = true;
            Close();
        }
    }
}
