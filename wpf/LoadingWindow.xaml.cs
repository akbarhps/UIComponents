using System.Threading;
using System.Windows;

namespace UIComponents.Wpf
{
    public partial class LoadingWindow : Window
    {
        public LoadingWindow()
        {
            InitializeComponent();
            var timer = new Timer(_ => Dispatcher.Invoke(() => Close()), null, 1800, Timeout.Infinite);
            Closed += (s, e) => timer.Dispose();
        }
    }
}
