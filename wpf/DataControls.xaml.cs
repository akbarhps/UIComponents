using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace UIComponents.Wpf
{
    public partial class DataControls : UserControl
    {
        private readonly ObservableCollection<FilterRow> _filterRows = new ObservableCollection<FilterRow>();

        public DataControls()
        {
            InitializeComponent();

            InputGrid.Items.Add(new { Width = "Small", Height = "10", Depth = "20" });
            InputGrid.Items.Add(new { Width = "Medium", Height = "14", Depth = "24" });
            InputGrid.Items.Add(new { Width = "Large", Height = "18", Depth = "30" });

            TypedGrid.Items.Add(new { Variant = "Small", Category = "Standard", IsActive = true });
            TypedGrid.Items.Add(new { Variant = "Large", Category = "Custom", IsActive = false });

            _filterRows.Add(new FilterRow { Project = "Website refresh", Owner = "Ada Lovelace", Status = "Active" });
            _filterRows.Add(new FilterRow { Project = "Mobile app", Owner = "Jonas Silva", Status = "Active" });
            _filterRows.Add(new FilterRow { Project = "Brand guidelines", Owner = "Mia Chen", Status = "Pending" });
            FilterGrid.ItemsSource = _filterRows;

            SelectionGrid.ItemsSource = new List<SelectionRow>
            {
                new SelectionRow { Project = "Website refresh", Owner = "Ada Lovelace" },
                new SelectionRow { Project = "Mobile app", Owner = "Jonas Silva" },
                new SelectionRow { Project = "Brand guidelines", Owner = "Mia Chen" }
            };
        }

        private void GridAddRow_Click(object sender, RoutedEventArgs e)
        {
            InputGrid.Items.Add(new { Width = "", Height = "", Depth = "" });
        }

        private void GridAddCol_Click(object sender, RoutedEventArgs e)
        {
            InputGrid.Columns.Add(new DataGridTextColumn { Header = "Column " + (InputGrid.Columns.Count + 1), Width = 120 });
        }

        private void FilterSearch_TextChanged(object sender, TextChangedEventArgs e)
        {
            var q = FilterSearch.Text.Trim().ToLower();
            FilterGrid.ItemsSource = string.IsNullOrEmpty(q)
                ? (IEnumerable<FilterRow>)_filterRows
                : _filterRows.Where(r => r.Project.ToLower().Contains(q) || r.Owner.ToLower().Contains(q)).ToList();
        }

        private void SelectionGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            int count = SelectionGrid.SelectedItems.Count;
            BulkText.Text = count == 0 ? "No rows selected" : count + (count == 1 ? " row selected" : " rows selected");
        }
    }

    public class FilterRow
    {
        public string Project { get; set; }
        public string Owner { get; set; }
        public string Status { get; set; }
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
        public event PropertyChangedEventHandler PropertyChanged;
    }
}
