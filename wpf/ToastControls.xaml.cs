using System.Windows;
using System.Windows.Controls;

namespace UIComponents.Wpf
{
    public partial class ToastControls : UserControl
    {
        public ToastControls()
        {
            InitializeComponent();
        }

        private void ToastDismissible_Click(object sender, RoutedEventArgs e)
        {
            Toasts.Show("info", "New update available", "Restart to apply the latest changes.", true, null, false);
        }

        private void ToastWithAction_Click(object sender, RoutedEventArgs e)
        {
            Toasts.Show("success", "Message sent", "Delivered to the team.", true, "Undo", false);
        }

        private void ToastPersistent_Click(object sender, RoutedEventArgs e)
        {
            Toasts.Show("error", "Connection lost", "We could not reach the server. Check your network.", true, null, true);
        }

        private void ToastSuccess_Click(object sender, RoutedEventArgs e)
        {
            Toasts.Show("success", "Saved", "Your changes have been saved.");
        }

        private void ToastInfo_Click(object sender, RoutedEventArgs e)
        {
            Toasts.Show("info", "Heads up", "A new version is available.");
        }

        private void ToastError_Click(object sender, RoutedEventArgs e)
        {
            Toasts.Show("error", "Upload failed", "The file could not be uploaded.");
        }
    }
}
