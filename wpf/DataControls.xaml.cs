using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace UIComponents.Wpf
{
    public partial class DataControls : UserControl
    {
        private readonly ObservableCollection<FilterRow> _filterRows = new ObservableCollection<FilterRow>();

        public DataControls()
        {
            InitializeComponent();

            InputGrid.Hint = "Shift-click selects a range, Ctrl/Cmd-click adds cells, and clicking a row or column header selects the whole row or column.";
            InputGrid.Setup(
                new[] { Text("Width"), Text("Height"), Text("Depth"), Text("Weight") },
                new[] { new[] { "Small", "10", "20", "5" },
                        new[] { "Medium", "14", "24", "8" },
                        new[] { "Large", "18", "30", "12" } }, true);

            TypedGrid.Hint = "Select a Category cell to change it with the dropdown, or tick IsActive. Selecting several cells and changing one updates the rest.";
            TypedGrid.Setup(
                new[] { Text("Variant"), Text("Width"), Text("Height"),
                        Select("Category", "Standard", "Pro", "Custom"), Check("IsActive") },
                new[] { new[] { "Small", "10", "20", "Standard", "true" },
                        new[] { "Medium", "14", "24", "Pro", "true" },
                        new[] { "Large", "18", "30", "Custom", "false" } }, false);

            _filterRows.Add(new FilterRow { Project = "Website refresh", Team = "Design team", Owner = "Ada Lovelace", Status = "Active", Updated = "Today" });
            _filterRows.Add(new FilterRow { Project = "Mobile app", Team = "Engineering", Owner = "Jonas Silva", Status = "Active", Updated = "Yesterday" });
            _filterRows.Add(new FilterRow { Project = "Brand guidelines", Team = "Design team", Owner = "Mia Chen", Status = "Pending", Updated = "3 days ago" });
            _filterRows.Add(new FilterRow { Project = "Help center", Team = "Content", Owner = "Ada Lovelace", Status = "Active", Updated = "Last week" });
            _filterRows.Add(new FilterRow { Project = "Q3 campaign", Team = "Marketing", Owner = "Sam Patel", Status = "Pending", Updated = "Last week" });
            _filterRows.Add(new FilterRow { Project = "Legacy archive", Team = "Operations", Owner = "Mia Chen", Status = "Archived", Updated = "2 weeks ago" });
            FilterGrid.ItemsSource = _filterRows;
            SegAll.IsChecked = true;

            SelectionGrid.ItemsSource = new List<SelectionRow>
            {
                new SelectionRow { Project = "Website refresh", Owner = "Ada Lovelace", Status = "Active", Updated = "Today", Budget = "$12,400" },
                new SelectionRow { Project = "Mobile app", Owner = "Jonas Silva", Status = "Active", Updated = "Yesterday", Budget = "$28,900" },
                new SelectionRow { Project = "Brand guidelines", Owner = "Mia Chen", Status = "Pending", Updated = "3 days ago", Budget = "$4,200" },
                new SelectionRow { Project = "Help center", Owner = "Ada Lovelace", Status = "Active", Updated = "Last week", Budget = "$8,750" },
                new SelectionRow { Project = "Q3 campaign", Owner = "Sam Patel", Status = "Pending", Updated = "Last week", Budget = "$15,600" }
            };
        }

        private static SheetColumn Text(string name)
        {
            SheetColumn col = new SheetColumn();
            col.Name = name;
            col.Type = "text";
            return col;
        }

        private static SheetColumn Select(string name, params string[] options)
        {
            SheetColumn col = new SheetColumn();
            col.Name = name;
            col.Type = "select";
            col.Options = options;
            return col;
        }

        private static SheetColumn Check(string name)
        {
            SheetColumn col = new SheetColumn();
            col.Name = name;
            col.Type = "checkbox";
            return col;
        }

        private string _statusFilter = "all";

        private void FilterSegment_Checked(object sender, RoutedEventArgs e)
        {
            var radio = sender as RadioButton;
            if (radio == null) return;
            _statusFilter = radio.Tag as string ?? "all";
            ApplyFilter();
        }

        private void FilterSearch_TextChanged(object sender, TextChangedEventArgs e)
        {
            ApplyFilter();
        }

        private void ApplyFilter()
        {
            if (FilterSearch == null || FilterGrid == null) return;
            var q = FilterSearch.Text.Trim().ToLower();
            var rows = _filterRows.Where(r =>
                (_statusFilter == "all" || string.Equals(r.Status, _statusFilter, StringComparison.OrdinalIgnoreCase)) &&
                (q.Length == 0 || r.Project.ToLower().Contains(q) || r.Owner.ToLower().Contains(q))).ToList();
            FilterGrid.ItemsSource = rows;
            FilterCount.Text = rows.Count + " of " + _filterRows.Count + " projects";
            FilterEmpty.Visibility = rows.Count == 0 ? Visibility.Visible : Visibility.Collapsed;
            FilterPagination.InfoText = rows.Count == 0 ? "No projects to show"
                : "Showing 1–" + rows.Count + " of " + rows.Count;
        }

        private void SelectAll_Checked(object sender, RoutedEventArgs e) { SetAllRows(true); }
        private void SelectAll_Unchecked(object sender, RoutedEventArgs e) { SetAllRows(false); }

        private void SetAllRows(bool selected)
        {
            foreach (var row in SelectionGrid.Items.OfType<SelectionRow>()) row.Selected = selected;
            UpdateBulkBar();
        }

        private void RowCheck_Changed(object sender, RoutedEventArgs e)
        {
            // The Checked event fires before the IsChecked binding pushes into the row.
            CheckBox box = sender as CheckBox;
            if (box != null)
            {
                BindingExpression binding = box.GetBindingExpression(CheckBox.IsCheckedProperty);
                if (binding != null) binding.UpdateSource();
            }
            Dispatcher.BeginInvoke(new Action(UpdateBulkBar));
        }

        private void SelectionGrid_SelectionChanged(object sender, SelectionChangedEventArgs e) { UpdateBulkBar(); }

        private void UpdateBulkBar()
        {
            int count = SelectionGrid.Items.OfType<SelectionRow>().Count(r => r.Selected);
            BulkText.Text = count == 1 ? "1 row selected" : count + " rows selected";
            BulkBar.Visibility = count > 0 ? Visibility.Visible : Visibility.Collapsed;
        }
    }

    public class FilterRow
    {
        public string Project { get; set; }
        public string Team { get; set; }
        public string Owner { get; set; }
        public string Status { get; set; }
        public string Updated { get; set; }
    }

    public class SelectionRow : INotifyPropertyChanged
    {
        private bool _selected;
        public bool Selected
        {
            get { return _selected; }
            set { _selected = value; if (PropertyChanged != null) PropertyChanged(this, new PropertyChangedEventArgs("Selected")); }
        }
        public string Project { get; set; }
        public string Owner { get; set; }
        public string Status { get; set; }
        public string Updated { get; set; }
        public string Budget { get; set; }
        public event PropertyChangedEventHandler PropertyChanged;
    }
}
