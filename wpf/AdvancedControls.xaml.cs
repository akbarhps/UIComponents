using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Automation;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Threading;

namespace UIComponents.Wpf
{
    public partial class AdvancedControls : UserControl
    {
        private readonly ObservableCollection<Row> _rows = new ObservableCollection<Row>();
        // Current row order: the web sorts the DOM, so ties keep the previous sort's order.
        private List<Row> _baseRows;

        private sealed class RowSortComparer : IComparer<Row>
        {
            private readonly string _property;
            public RowSortComparer(string property) { _property = property; }
            public int Compare(Row a, Row b) { return CompareSort(a, b, _property); }
        }

        public AdvancedControls()
        {
            InitializeComponent();
            _rows.Add(new Row { Project = "Website refresh", Team = "Design team", Dept = "Design", Owner = "Ada Lovelace", Status = "Active", Priority = "High", Updated = "Today", Due = "Oct 12", Budget = "$12,400", Progress = "72%" });
            _rows.Add(new Row { Project = "Mobile app", Team = "Engineering", Dept = "Engineering", Owner = "Jonas Silva", Status = "Active", Priority = "Medium", Updated = "Yesterday", Due = "Oct 20", Budget = "$28,900", Progress = "45%" });
            _rows.Add(new Row { Project = "Brand guidelines", Team = "Design team", Dept = "Design", Owner = "Mia Chen", Status = "Pending", Priority = "Low", Updated = "3 days ago", Due = "Nov 02", Budget = "$4,200", Progress = "20%" });
            _rows.Add(new Row { Project = "Help center", Team = "Content", Dept = "Content", Owner = "Ada Lovelace", Status = "Active", Priority = "Medium", Updated = "Last week", Due = "Nov 10", Budget = "$8,750", Progress = "88%" });
            _rows.Add(new Row { Project = "Q3 campaign", Team = "Marketing", Dept = "Marketing", Owner = "Sam Patel", Status = "Pending", Priority = "High", Updated = "Last week", Due = "Nov 18", Budget = "$15,600", Progress = "60%" });
            _rows.Add(new Row { Project = "Legacy archive", Team = "Operations", Dept = "Operations", Owner = "Mia Chen", Status = "Archived", Priority = "Low", Updated = "2 weeks ago", Due = "Sep 30", Budget = "$1,100", Progress = "100%" });
            _baseRows = _rows.ToList();
            ColumnGrid.ItemsSource = _rows;
            SortGrid.ItemsSource = _rows;
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
                (p.Length == 0 || (r.Project + r.Team).ToLower().Contains(p)) &&
                (o.Length == 0 || r.Owner.ToLower().Contains(o)) &&
                (status.Length == 0 || string.Equals(r.Status, status, StringComparison.OrdinalIgnoreCase)) &&
                (u.Length == 0 || r.Updated.ToLower().Contains(u)) &&
                (!hasMin || ParseBudget(r.Budget) >= min)).ToList();

            ColumnGrid.ItemsSource = rows;
            ColCount.Text = rows.Count + " of " + _rows.Count + " projects";
            ColEmpty.Visibility = rows.Count == 0 ? Visibility.Visible : Visibility.Collapsed;
            ColumnPagination.InfoText = rows.Count == 0 ? "No projects to show"
                : "Showing 1–" + rows.Count + " of " + rows.Count;
        }

        private static decimal ParseBudget(string text)
        {
            decimal value;
            decimal.TryParse((text ?? "").Replace("$", "").Replace(",", ""), out value);
            return value;
        }

        private void PinColumn_Checked(object sender, RoutedEventArgs e)
        {
            ApplyPinnedState();
        }

        private void PinColumn_Unchecked(object sender, RoutedEventArgs e)
        {
            ApplyPinnedState();
        }

        private void SortScroll_ScrollChanged(object sender, ScrollChangedEventArgs e)
        {
            if (PinToggle.IsChecked == true) ApplyPinnedState();
        }

        private void ApplyPinnedState()
        {
            bool pinned = PinToggle.IsChecked == true;
            double dx = pinned ? SortScroll.HorizontalOffset : 0;
            // Both pinned Borders are opaque and above the column-span separators: they must
            // paint the header/filter bottom line themselves, plus the right line when pinned.
            Thickness borders = pinned ? new Thickness(0, 0, 1, 1) : new Thickness(0, 0, 0, 1);
            PinHeader.BorderThickness = borders;
            PinFilter.BorderThickness = borders;
            PinFilter.Background = (Brush)FindResource(pinned ? "Surface2" : "Surface");
            PinHeader.CornerRadius = pinned ? new CornerRadius(12, 0, 0, 0) : new CornerRadius(0);
            PinHeader.RenderTransform = new TranslateTransform(dx, 0);
            PinFilter.RenderTransform = new TranslateTransform(dx, 0);
            ApplyCellTransform(SortGrid, dx, pinned);
        }

        private static void ApplyCellTransform(DependencyObject node, double dx, bool pinned)
        {
            DataGridCell cell = node as DataGridCell;
            if (cell != null && cell.Column != null && cell.Column.DisplayIndex == 0)
            {
                cell.RenderTransform = new TranslateTransform(dx, 0);
                cell.SetValue(Panel.ZIndexProperty, 1);
                string key = pinned ? "TableCellStickyPinned" : "TableRowHeaderCell";
                cell.SetValue(FrameworkElement.StyleProperty, Application.Current.Resources[key] as Style);
            }
            int count = VisualTreeHelper.GetChildrenCount(node);
            for (int i = 0; i < count; i++) ApplyCellTransform(VisualTreeHelper.GetChild(node, i), dx, pinned);
        }

        private void ReapplyPinned()
        {
            Dispatcher.BeginInvoke(DispatcherPriority.Loaded, new Action(ApplyPinnedState));
        }

        private string _sortProperty = "";
        private bool _sortAscending = true;

        private void SortHeader_Click(object sender, RoutedEventArgs e)
        {
            Button button = sender as Button;
            if (button == null) return;
            string prop = button.Tag as string;
            if (string.IsNullOrEmpty(prop)) return;

            if (prop == _sortProperty) _sortAscending = !_sortAscending;
            else { _sortProperty = prop; _sortAscending = true; }
            RefreshSortGrid();
        }

        // Filter first, then sort the matches: both persist, as in the web's DOM reorder.
        private void RefreshSortGrid()
        {
            if (SortProject == null || SortGrid == null) return;
            var p = SortProject.Text.Trim().ToLower();
            var o = SortOwner.Text.Trim().ToLower();
            var t = SortTeam.Text.Trim().ToLower();
            if (!string.IsNullOrEmpty(_sortProperty))
            {
                var comparer = new RowSortComparer(_sortProperty);
                _baseRows = (_sortAscending ? _baseRows.OrderBy(r => r, comparer)
                                            : _baseRows.OrderByDescending(r => r, comparer)).ToList();
            }
            var rows = _baseRows.Where(r =>
                (p.Length == 0 || (r.Project + r.Team).ToLower().Contains(p)) &&
                (o.Length == 0 || r.Owner.ToLower().Contains(o)) &&
                (t.Length == 0 || r.Dept.ToLower().Contains(t))).ToList();
            SortGrid.ItemsSource = rows;
            SortEmpty.Visibility = rows.Count == 0 ? Visibility.Visible : Visibility.Collapsed;
            SortPagination.InfoText = rows.Count == 0 ? "No projects to show"
                : "Showing 1–" + rows.Count + " of " + rows.Count;
            UpdateSortVisuals();
            ReapplyPinned();
        }

        private static int CompareSort(Row a, Row b, string prop)
        {
            string sa = SortValue(a, prop);
            string sb = SortValue(b, prop);
            decimal na, nb;
            if (decimal.TryParse(sa, out na) && decimal.TryParse(sb, out nb)) return na.CompareTo(nb);
            return string.Compare(sa, sb, StringComparison.OrdinalIgnoreCase);
        }

        private static string SortValue(Row row, string prop)
        {
            switch (prop)
            {
                case "Project": return row.Project;
                case "Owner": return row.Owner;
                case "Team": return row.Dept;
                case "Status": return row.Status;
                case "Priority": return row.Priority;
                case "Updated":
                    if (row.Updated == "Today") return "0";
                    if (row.Updated == "Yesterday") return "1";
                    if (row.Updated == "3 days ago") return "3";
                    if (row.Updated == "Last week") return "7";
                    return "14";
                case "Due":
                    return DateTime.ParseExact(row.Due, "MMM dd", CultureInfo.InvariantCulture).ToString("MMdd");
                case "Budget": return ParseBudget(row.Budget).ToString();
                case "Progress": return row.Progress.Replace("%", "");
                default: return "";
            }
        }

        private void UpdateSortVisuals()
        {
            Brush brand = (Brush)FindResource("Brand");
            foreach (Button button in FindButtons(SortHeaderGrid))
            {
                bool active = (button.Tag as string) == _sortProperty;
                if (active) button.Foreground = brand;
                else button.ClearValue(Button.ForegroundProperty);
                Viewbox chevron = button.Template.FindName("Chevron", button) as Viewbox;
                if (chevron == null) continue;
                chevron.Opacity = active ? 1 : 0.4;
                chevron.LayoutTransform = (active && _sortAscending) ? new RotateTransform(180) : Transform.Identity;
            }
        }

        private static IEnumerable<Button> FindButtons(DependencyObject node)
        {
            Button button = node as Button;
            if (button != null) yield return button;
            int count = VisualTreeHelper.GetChildrenCount(node);
            for (int i = 0; i < count; i++)
                foreach (Button child in FindButtons(VisualTreeHelper.GetChild(node, i))) yield return child;
        }

        private void SortFilter(object sender, TextChangedEventArgs e)
        {
            RefreshSortGrid();
        }

    }

    public class Row
    {
        public string Project { get; set; }
        public string Team { get; set; }
        public string Dept { get; set; }
        public string Owner { get; set; }
        public string Status { get; set; }
        public string Updated { get; set; }
        public string Budget { get; set; }
        public string Priority { get; set; }
        public string Due { get; set; }
        public string Progress { get; set; }
    }
}
