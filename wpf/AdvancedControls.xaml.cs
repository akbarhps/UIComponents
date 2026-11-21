using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Automation;
using System.Windows.Controls;
using System.Windows.Media;

namespace UIComponents.Wpf
{
    public partial class AdvancedControls : UserControl
    {
        private readonly ObservableCollection<Row> _rows = new ObservableCollection<Row>();
        private readonly ObservableCollection<Border> _toasts = new ObservableCollection<Border>();

        public AdvancedControls()
        {
            InitializeComponent();
            _rows.Add(new Row { Project = "Website refresh", Owner = "Ada Lovelace", Status = "Active", Budget = "$12,400" });
            _rows.Add(new Row { Project = "Mobile app", Owner = "Jonas Silva", Status = "Active", Budget = "$28,900" });
            _rows.Add(new Row { Project = "Brand guidelines", Owner = "Mia Chen", Status = "Pending", Budget = "$4,200" });
            ColumnGrid.ItemsSource = _rows;
            SortGrid.ItemsSource = _rows;
            ColProject.TextChanged += Filter;
            ColOwner.TextChanged += Filter;
            ToastHost.ItemsSource = _toasts;
        }

        private void Filter(object sender, TextChangedEventArgs e)
        {
            var p = ColProject.Text.Trim().ToLower();
            var o = ColOwner.Text.Trim().ToLower();
            ColumnGrid.ItemsSource = _rows.Where(r =>
                (p.Length == 0 || r.Project.ToLower().Contains(p)) &&
                (o.Length == 0 || r.Owner.ToLower().Contains(o))).ToList();
        }

        private void PinColumn_Click(object sender, RoutedEventArgs e)
        {
            SortGrid.FrozenColumnCount = SortGrid.FrozenColumnCount == 0 ? 1 : 0;
        }

        private void ShowToast(string title, string message, SolidColorBrush background, SolidColorBrush foreground)
        {
            var toast = new Border
            {
                Background = background, CornerRadius = new CornerRadius(8), Padding = new Thickness(14, 14, 14, 14),
                Margin = new Thickness(0, 8, 0, 0), Effect = (System.Windows.Media.Effects.Effect)FindResource("Shadow1")
            };
            var stack = new StackPanel();
            stack.Children.Add(new TextBlock { Text = title, FontWeight = FontWeights.SemiBold, Foreground = foreground });
            stack.Children.Add(new TextBlock { Text = message, FontSize = 12, Foreground = (SolidColorBrush)FindResource("Ink2"), Margin = new Thickness(0, 3, 0, 0) });
            toast.Child = stack;
            toast.SetValue(AutomationProperties.AutomationIdProperty, "ToastItem");
            _toasts.Add(toast);
        }

        private void SuccessToast_Click(object sender, RoutedEventArgs e)
        {
            ShowToast("Saved", "Your changes have been saved.", (SolidColorBrush)FindResource("SuccessSoft"), (SolidColorBrush)FindResource("Success"));
        }
        private void InfoToast_Click(object sender, RoutedEventArgs e)
        {
            ShowToast("Heads up", "A new version is available.", (SolidColorBrush)FindResource("InfoSoft"), (SolidColorBrush)FindResource("Info"));
        }
        private void ErrorToast_Click(object sender, RoutedEventArgs e)
        {
            ShowToast("Upload failed", "The file could not be uploaded.", (SolidColorBrush)FindResource("DangerSoft"), (SolidColorBrush)FindResource("Danger"));
        }

        private void CommandPalette_Click(object sender, RoutedEventArgs e)
        {
            var palette = new CommandPaletteWindow { Owner = Window.GetWindow(this) };
            palette.ShowDialog();
        }

        private void LoadingModal_Click(object sender, RoutedEventArgs e)
        {
            var loading = new LoadingWindow { Owner = Window.GetWindow(this) };
            loading.ShowDialog();
        }

        private void Drawer_Click(object sender, RoutedEventArgs e)
        {
            var drawer = new DrawerWindow { Owner = Window.GetWindow(this) };
            drawer.ShowDialog();
        }
    }

    public class Row
    {
        public string Project { get; set; }
        public string Owner { get; set; }
        public string Status { get; set; }
        public string Budget { get; set; }
    }
}
