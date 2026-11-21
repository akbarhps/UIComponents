using System;
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
            _rows.Add(new Row { Project = "Website refresh", Team = "Design team", Owner = "Ada Lovelace", Status = "Active", Priority = "High", Updated = "Today", Due = "Oct 12", Budget = "$12,400", Progress = "72%" });
            _rows.Add(new Row { Project = "Mobile app", Team = "Engineering", Owner = "Jonas Silva", Status = "Active", Priority = "Medium", Updated = "Yesterday", Due = "Oct 20", Budget = "$28,900", Progress = "45%" });
            _rows.Add(new Row { Project = "Brand guidelines", Team = "Design team", Owner = "Mia Chen", Status = "Pending", Priority = "Low", Updated = "3 days ago", Due = "Nov 02", Budget = "$4,200", Progress = "20%" });
            _rows.Add(new Row { Project = "Help center", Team = "Content", Owner = "Ada Lovelace", Status = "Active", Priority = "Medium", Updated = "Last week", Due = "Nov 10", Budget = "$8,750", Progress = "88%" });
            _rows.Add(new Row { Project = "Q3 campaign", Team = "Marketing", Owner = "Sam Patel", Status = "Pending", Priority = "High", Updated = "Last week", Due = "Nov 18", Budget = "$15,600", Progress = "60%" });
            _rows.Add(new Row { Project = "Legacy archive", Team = "Operations", Owner = "Mia Chen", Status = "Archived", Priority = "Low", Updated = "2 weeks ago", Due = "Sep 30", Budget = "$1,100", Progress = "100%" });
            ColumnGrid.ItemsSource = _rows;
            SortGrid.ItemsSource = _rows;
            ToastHost.ItemsSource = _toasts;
        }

        private void Filter(object sender, RoutedEventArgs e)
        {
            if (ColProject == null || ColumnGrid == null) return;

            var p = ColProject.Text.Trim().ToLower();
            var o = ColOwner.Text.Trim().ToLower();
            var u = ColUpdated.Text.Trim().ToLower();
            decimal min;
            bool hasMin = decimal.TryParse(ColBudget.Text.Trim().Replace("$", "").Replace(",", ""), out min);
            var item = ColStatus.SelectedItem as ComboBoxItem;
            var status = item == null ? "" : item.Content.ToString();
            if (status == "All statuses") status = "";

            var rows = _rows.Where(r =>
                (p.Length == 0 || r.Project.ToLower().Contains(p)) &&
                (o.Length == 0 || r.Owner.ToLower().Contains(o)) &&
                (status.Length == 0 || string.Equals(r.Status, status, StringComparison.OrdinalIgnoreCase)) &&
                (u.Length == 0 || r.Updated.ToLower().Contains(u)) &&
                (!hasMin || ParseBudget(r.Budget) >= min)).ToList();

            ColumnGrid.ItemsSource = rows;
            ColCount.Text = rows.Count + " of " + _rows.Count + " projects";
        }

        private static decimal ParseBudget(string text)
        {
            decimal value;
            decimal.TryParse((text ?? "").Replace("$", "").Replace(",", ""), out value);
            return value;
        }

        private void PinColumn_Click(object sender, RoutedEventArgs e)
        {
            SortGrid.FrozenColumnCount = SortGrid.FrozenColumnCount == 0 ? 1 : 0;
        }

        private string _sortProperty = "Project";
        private bool _sortAscending = true;

        private void SortHeader_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            if (button == null) return;
            var prop = button.Tag as string;
            if (string.IsNullOrEmpty(prop)) return;

            if (prop == _sortProperty) _sortAscending = !_sortAscending;
            else { _sortProperty = prop; _sortAscending = true; }

            var list = _rows.ToList();
            list.Sort(delegate(Row a, Row b)
            {
                int cmp = string.Compare(GetValue(a, prop), GetValue(b, prop), StringComparison.OrdinalIgnoreCase);
                return _sortAscending ? cmp : -cmp;
            });
            SortGrid.ItemsSource = list;
        }

        private static string GetValue(Row row, string prop)
        {
            switch (prop)
            {
                case "Project": return row.Project;
                case "Owner": return row.Owner;
                case "Team": return row.Team;
                case "Status": return row.Status;
                case "Priority": return row.Priority;
                case "Updated": return row.Updated;
                case "Due": return row.Due;
                case "Budget": return row.Budget;
                case "Progress": return row.Progress;
                default: return "";
            }
        }

        private void SortFilter(object sender, TextChangedEventArgs e)
        {
            if (SortProject == null || SortGrid == null) return;
            var p = SortProject.Text.Trim().ToLower();
            var o = SortOwner.Text.Trim().ToLower();
            var t = SortTeam.Text.Trim().ToLower();
            SortGrid.ItemsSource = _rows.Where(r =>
                (p.Length == 0 || r.Project.ToLower().Contains(p)) &&
                (o.Length == 0 || r.Owner.ToLower().Contains(o)) &&
                (t.Length == 0 || r.Team.ToLower().Contains(t))).ToList();
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
        public string Team { get; set; }
        public string Owner { get; set; }
        public string Status { get; set; }
        public string Updated { get; set; }
        public string Budget { get; set; }
        public string Priority { get; set; }
        public string Due { get; set; }
        public string Progress { get; set; }
    }
}
