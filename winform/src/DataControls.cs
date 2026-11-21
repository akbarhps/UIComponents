using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace UIComponents
{
    public enum GridColumnType { Text, Select, Checkbox }

    public class GridColumnDef
    {
        public string Name;
        public GridColumnType Type;
        public string[] Options;
        public GridColumnDef(string name, GridColumnType type, string[] options = null)
        {
            Name = name; Type = type; Options = options;
        }
    }

    /// <summary>Spreadsheet-style input: multi-cell selection, bulk edit, Excel copy/paste.</summary>
    public class ClaudeGrid : UserControl
    {
        private readonly DataGridView _grid = new DataGridView();
        private readonly List<GridColumnDef> _columns = new List<GridColumnDef>();
        private bool _allowAddColumn;
        private DataGridViewCell _dragAnchor;
        private bool _dragging;

        public ClaudeGrid(GridColumnDef[] columns, string[,] rows = null, bool allowAddColumn = true)
        {
            _allowAddColumn = allowAddColumn;
            BackColor = Theme.Surface;
            _columns.AddRange(columns);

            var toolbar = new Panel { Dock = DockStyle.Top, Height = 40, BackColor = Theme.Surface };
            var copy = new ClaudeButton { Text = "Copy selected", Variant = ButtonVariant.Ghost, Location = new Point(0, 4), Size = new Size(110, 30) };
            copy.Click += (s, e) => CopySelection();
            var clear = new ClaudeButton { Text = "Clear selected", Variant = ButtonVariant.Ghost, Location = new Point(116, 4), Size = new Size(110, 30) };
            clear.Click += (s, e) => ClearSelection();
            var addRow = new ClaudeButton { Text = "+ Add row", Variant = ButtonVariant.Ghost, Location = new Point(232, 4), Size = new Size(90, 30) };
            addRow.Click += (s, e) => _grid.Rows.Add();
            toolbar.Controls.Add(addRow);
            toolbar.Controls.Add(clear);
            toolbar.Controls.Add(copy);
            if (allowAddColumn)
            {
                var addCol = new ClaudeButton { Text = "+ Add column", Variant = ButtonVariant.Ghost, Location = new Point(330, 4), Size = new Size(110, 30) };
                addCol.Click += (s, e) =>
                {
                    int index = _grid.Columns.Count;
                    _grid.Columns.Add("col" + index, "Column " + (index + 1));
                    _columns.Add(new GridColumnDef("Column " + (index + 1), GridColumnType.Text));
                };
                toolbar.Controls.Add(addCol);
            }

            Draw.StyleGrid(_grid);
            _grid.Dock = DockStyle.Fill;
            _grid.MultiSelect = true;
            _grid.SelectionMode = DataGridViewSelectionMode.CellSelect;
            _grid.ClipboardCopyMode = DataGridViewClipboardCopyMode.EnableWithoutHeaderText;
            _grid.EditMode = DataGridViewEditMode.EditOnKeystrokeOrF2;
            _grid.AllowUserToAddRows = false;

            foreach (var col in _columns)
            {
                if (col.Type == GridColumnType.Select)
                {
                    var combo = new DataGridViewComboBoxColumn { HeaderText = col.Name, DataPropertyName = col.Name, FlatStyle = FlatStyle.Flat };
                    if (col.Options != null) foreach (var o in col.Options) combo.Items.Add(o);
                    _grid.Columns.Add(combo);
                }
                else if (col.Type == GridColumnType.Checkbox)
                {
                    _grid.Columns.Add(new DataGridViewCheckBoxColumn { HeaderText = col.Name, DataPropertyName = col.Name, Width = 80 });
                }
                else
                {
                    _grid.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = col.Name, DataPropertyName = col.Name, Width = 140 });
                }
            }

            if (rows != null)
                for (int r = 0; r < rows.GetLength(0); r++)
                {
                    int index = _grid.Rows.Add();
                    for (int c = 0; c < _grid.Columns.Count && c < rows.GetLength(1); c++) _grid.Rows[index].Cells[c].Value = rows[r, c];
                }

            // Drag selection, matching the web grid's click / shift-click / drag behaviour.
            _grid.MouseDown += (s, e) =>
            {
                if (e.Button != MouseButtons.Left) return;
                if (_grid.IsCurrentCellInEditMode) _grid.CancelEdit();
                var hit = _grid.HitTest(e.X, e.Y);
                if (hit.Type == DataGridViewHitTestType.Cell)
                {
                    _dragAnchor = _grid.Rows[hit.RowIndex].Cells[hit.ColumnIndex];
                    _dragging = true;
                    _grid.ClearSelection();
                    _dragAnchor.Selected = true;
                    _grid.CurrentCell = _dragAnchor;
                }
            };
            _grid.MouseMove += (s, e) =>
            {
                if (!_dragging || _dragAnchor == null || e.Button != MouseButtons.Left) return;
                var hit = _grid.HitTest(e.X, e.Y);
                if (hit.Type != DataGridViewHitTestType.Cell) return;
                _grid.ClearSelection();
                int r1 = Math.Min(_dragAnchor.RowIndex, hit.RowIndex), r2 = Math.Max(_dragAnchor.RowIndex, hit.RowIndex);
                int c1 = Math.Min(_dragAnchor.ColumnIndex, hit.ColumnIndex), c2 = Math.Max(_dragAnchor.ColumnIndex, hit.ColumnIndex);
                for (int r = r1; r <= r2; r++)
                    for (int c = c1; c <= c2; c++)
                        if (r < _grid.Rows.Count && c < _grid.Columns.Count) _grid.Rows[r].Cells[c].Selected = true;
            };
            _grid.MouseUp += (s, e) => { _dragging = false; _dragAnchor = null; };

            // Owner-draw cells to match the web grid's surface, borders and selection tint.
            _grid.CellPainting += (s, e) =>
            {
                if (e.RowIndex < 0 || e.ColumnIndex < 0) return;
                e.PaintBackground(e.CellBounds, false);
                bool selected = e.State.HasFlag(DataGridViewElementStates.Selected);
                Color background = selected ? Theme.BrandSoft : Theme.Surface;
                using (var brush = new SolidBrush(background)) e.Graphics.FillRectangle(brush, e.CellBounds);
                e.Graphics.DrawLine(new Pen(Theme.Line), e.CellBounds.Left, e.CellBounds.Bottom - 1, e.CellBounds.Right - 1, e.CellBounds.Bottom - 1);
                e.Graphics.DrawLine(new Pen(Theme.Line), e.CellBounds.Right - 1, e.CellBounds.Top, e.CellBounds.Right - 1, e.CellBounds.Bottom);
                var value = e.Value == null ? "" : e.Value.ToString();
                Draw.CenteredText(e.Graphics, value, new Rectangle(e.CellBounds.X + 10, e.CellBounds.Y, e.CellBounds.Width - 14, e.CellBounds.Height), Theme.Sans(9.5f), Theme.Ink, HorizontalAlignment.Left);
                e.Handled = true;
            };

            // Multi-cell edit: applying a value to one selected cell updates the rest.
            _grid.CellEndEdit += (s, e) =>
            {
                if (_dragging || _grid.SelectedCells.Count < 2) return;
                var value = _grid.Rows[e.RowIndex].Cells[e.ColumnIndex].Value;
                foreach (DataGridViewCell cell in _grid.SelectedCells)
                    if (cell.RowIndex != e.RowIndex || cell.ColumnIndex != e.ColumnIndex)
                        cell.Value = value;
            };

            Controls.Add(_grid);
            Controls.Add(toolbar);
            Size = new Size(760, 260);
        }

        private void CopySelection()
        {
            var data = _grid.GetClipboardContent();
            if (data != null) Clipboard.SetDataObject(data, true);
        }

        private void ClearSelection()
        {
            foreach (DataGridViewCell cell in _grid.SelectedCells) cell.Value = "";
        }
    }

    public class TableOptions
    {
        public bool ShowFilter;
        public bool ShowColumnFilters;
        public bool ShowPagination;
        public bool ShowSelection;
        public bool Sortable;
        public bool StickyFirst;
        public int PageSize = 25;
    }

    /// <summary>Configurable data table covering filter, column filters, sort, sticky, selection and pagination.</summary>
    public class ClaudeTable : UserControl
    {
        private readonly DataTable _source;
        private readonly DataTable _page = new DataTable();
        private readonly DataGridView _grid = new DataGridView();
        private readonly BindingSource _binding = new BindingSource();
        private readonly TableOptions _options;
        private readonly TextBox _search = new TextBox();
        private readonly ComboBox _status = new ComboBox();
        private readonly ComboBox _pageSize = new ComboBox();
        private readonly Label _pageInfo = new Label();
        private readonly Label _bulk = new Label();
        private readonly Panel _bulkBar = new Panel();
        private readonly Dictionary<string, Control> _columnFilters = new Dictionary<string, Control>();
        private int _pageIndex = 1;
        private int _selectedStatus = 0;

        public ClaudeTable(DataTable source, TableOptions options)
        {
            _source = source;
            if (!_source.Columns.Contains("_match")) _source.Columns.Add("_match", typeof(bool));
            _options = options ?? new TableOptions();
            BackColor = Theme.Surface;

            if (_options.ShowFilter || _options.ShowColumnFilters)
            {
                var filterBar = new Panel { Dock = DockStyle.Top, Height = 42, BackColor = Theme.Surface };
                if (_options.ShowFilter)
                {
                    _search.Font = Theme.Sans(9.5f);
                    _search.BorderStyle = BorderStyle.FixedSingle;
                    _search.Location = new Point(0, 8);
                    _search.Width = 220;
                    _search.TextChanged += (s, e) => { _pageIndex = 1; Apply(); };
                    filterBar.Controls.Add(_search);

                    _status.Font = Theme.Sans(9.5f);
                    _status.FlatStyle = FlatStyle.Flat;
                    _status.DropDownStyle = ComboBoxStyle.DropDownList;
                    _status.Items.AddRange(new object[] { "All", "Active", "Pending", "Archived" });
                    _status.SelectedIndex = 0;
                    _status.Location = new Point(232, 8);
                    _status.Width = 100;
                    _status.SelectedIndexChanged += (s, e) => { _selectedStatus = _status.SelectedIndex; _pageIndex = 1; Apply(); };
                    filterBar.Controls.Add(_status);
                }
                if (_options.ShowColumnFilters)
                {
                    int x = 350;
                    foreach (DataColumn col in _source.Columns)
                    {
                        if (col.ColumnName == "_match") continue;
                        var box = new TextBox { Font = Theme.Sans(8.5f), BorderStyle = BorderStyle.FixedSingle, Location = new Point(x, 8), Width = 100 };
                        box.TextChanged += (s, e) => { _pageIndex = 1; Apply(); };
                        _columnFilters[col.ColumnName] = box;
                        filterBar.Controls.Add(box);
                        x += 108;
                    }
                }
                Controls.Add(filterBar);
            }

            if (_options.ShowSelection)
            {
                _bulkBar.Visible = false;
                _bulkBar.Dock = DockStyle.Top;
                _bulkBar.Height = 38;
                _bulkBar.BackColor = Theme.BrandSoft;
                _bulk.ForeColor = Theme.Brand;
                _bulk.Font = Theme.Sans(9.5f, FontStyle.Bold);
                _bulk.Location = new Point(10, 9);
                _bulk.AutoSize = true;
                var delete = new ClaudeButton { Text = "Delete", Variant = ButtonVariant.Danger, Dock = DockStyle.Right, Width = 80, Height = 28, Margin = new Padding(4) };
                delete.Click += (s, e) => DeleteSelected();
                _bulkBar.Controls.Add(delete);
                _bulkBar.Controls.Add(_bulk);
                Controls.Add(_bulkBar);
            }

            Draw.StyleGrid(_grid);
            _grid.Dock = DockStyle.Fill;
            _grid.AllowUserToAddRows = false;
            _grid.AutoGenerateColumns = true;
            _grid.DataSource = _binding;
            _grid.SelectionMode = _options.ShowSelection ? DataGridViewSelectionMode.FullRowSelect : DataGridViewSelectionMode.CellSelect;
            _grid.MultiSelect = _options.ShowSelection;
            if (_options.StickyFirst && _grid.Columns.Count > 0) _grid.Columns[0].Frozen = true;
            if (_options.Sortable)
                foreach (DataGridViewColumn col in _grid.Columns) col.SortMode = DataGridViewColumnSortMode.Automatic;
            if (_options.ShowSelection)
                _grid.SelectionChanged += (s, e) => UpdateBulk();

            Controls.Add(_grid);

            if (_options.ShowPagination)
            {
                var pager = new Panel { Dock = DockStyle.Bottom, Height = 40, BackColor = Theme.Surface };
                var limitLabel = new Label { Text = "Show", Location = new Point(0, 10), AutoSize = true, Font = Theme.Sans(9f), ForeColor = Theme.Ink2 };
                _pageSize.Font = Theme.Sans(9f);
                _pageSize.FlatStyle = FlatStyle.Flat;
                _pageSize.DropDownStyle = ComboBoxStyle.DropDownList;
                foreach (var n in new[] { "10", "25", "50", "100" }) _pageSize.Items.Add(n);
                _pageSize.SelectedItem = _options.PageSize.ToString();
                _pageSize.Location = new Point(36, 6);
                _pageSize.Width = 60;
                _pageSize.SelectedIndexChanged += (s, e) => { _options.PageSize = int.Parse(_pageSize.SelectedItem.ToString()); _pageIndex = 1; Apply(); };
                var prev = new ClaudeButton { Text = "Previous", Variant = ButtonVariant.Secondary, Location = new Point(110, 5), Size = new Size(80, 30) };
                prev.Click += (s, e) => { if (_pageIndex > 1) { _pageIndex--; Apply(); } };
                var next = new ClaudeButton { Text = "Next", Location = new Point(198, 5), Size = new Size(70, 30) };
                next.Click += (s, e) => { _pageIndex++; Apply(); };
                _pageInfo.Font = Theme.Sans(9f);
                _pageInfo.ForeColor = Theme.Ink3;
                _pageInfo.Location = new Point(280, 10);
                _pageInfo.AutoSize = true;
                pager.Controls.Add(_pageInfo);
                pager.Controls.Add(next);
                pager.Controls.Add(prev);
                pager.Controls.Add(_pageSize);
                pager.Controls.Add(limitLabel);
                Controls.Add(pager);
            }

            Size = new Size(820, 320);
            Apply();
            if (_options.StickyFirst && _grid.Columns.Count > 0) _grid.Columns[0].Frozen = true;
            if (_options.Sortable) foreach (DataGridViewColumn col in _grid.Columns) col.SortMode = DataGridViewColumnSortMode.Automatic;
            if (_options.ShowSelection) { _grid.ClearSelection(); _bulkBar.Visible = false; }
        }

        private void Apply()
        {
            var matched = _source.Select();
            foreach (DataRow row in matched)
            {
                bool include = true;
                if (_options.ShowFilter)
                {
                    string query = _search.Text.Trim().ToLowerInvariant();
                    if (query.Length > 0 && !string.Join("|", row.ItemArray).ToLowerInvariant().Contains(query)) include = false;
                    if (_selectedStatus > 0)
                    {
                        string status = _source.Columns.Contains("Status") && row["Status"] != DBNull.Value ? row["Status"].ToString() : "";
                        string wanted = _status.SelectedItem.ToString();
                        if (status != wanted) include = false;
                    }
                }
                if (_options.ShowColumnFilters)
                    foreach (DataColumn col in _source.Columns)
                    {
                        Control control;
                        if (!_columnFilters.TryGetValue(col.ColumnName, out control)) continue;
                        string value = control.Text.Trim().ToLowerInvariant();
                        if (value.Length > 0 && row[col] != DBNull.Value && !row[col].ToString().ToLowerInvariant().Contains(value)) include = false;
                    }
                row["_match"] = include;
            }

            var visible = _source.Select("_match = true");
            int total = visible.Length;
            int pages = Math.Max(1, (int)Math.Ceiling(total / (double)_options.PageSize));
            _pageIndex = Math.Min(_pageIndex, pages);
            int start = (_pageIndex - 1) * _options.PageSize;

            _page.Reset();
            foreach (DataColumn col in _source.Columns) if (col.ColumnName != "_match") _page.Columns.Add(col.ColumnName, col.DataType);
            for (int i = start; i < Math.Min(start + _options.PageSize, total); i++)
            {
                var values = new object[_page.Columns.Count];
                for (int c = 0; c < values.Length; c++) values[c] = visible[i][_page.Columns[c].ColumnName];
                _page.Rows.Add(values);
            }
            _binding.DataSource = _page;
            _pageInfo.Text = total == 0 ? "No projects to show"
                : "Showing " + (start + 1) + "–" + Math.Min(start + _options.PageSize, total) + " of " + total;
        }

        private void DeleteSelected()
        {
            var toRemove = new List<DataRow>();
            foreach (DataGridViewRow row in _grid.SelectedRows)
                if (!row.IsNewRow && row.DataBoundItem is DataRowView)
                {
                    string name = ((DataRowView)row.DataBoundItem)["Project"].ToString();
                    foreach (DataRow sourceRow in _source.Rows)
                        if (sourceRow["Project"].ToString() == name) { toRemove.Add(sourceRow); break; }
                }
            foreach (var row in toRemove) _source.Rows.Remove(row);
            Apply();
            _bulkBar.Visible = false;
        }

        private void UpdateBulk()
        {
            if (!_options.ShowSelection) return;
            int count = _grid.SelectedRows.Count;
            _bulkBar.Visible = count > 0;
            _bulk.Text = count + (count == 1 ? " row selected" : " rows selected");
        }
    }
}
