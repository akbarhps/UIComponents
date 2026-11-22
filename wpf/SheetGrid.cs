using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows;
using System.Windows.Automation;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace UIComponents.Wpf
{
    public class SheetColumn
    {
        public string Name;
        public string Type;      // "text" | "select" | "checkbox"
        public string[] Options; // select only
    }

    /// <summary>
    /// Port of the web createGrid() control. Phase 4 adds data operations;
    /// Phase 5 adds the typed select/checkbox columns.
    /// </summary>
    public class SheetGrid : UserControl
    {
        private readonly List<SheetColumn> _columns = new List<SheetColumn>();
        private readonly List<List<string>> _data = new List<List<string>>();
        private readonly HashSet<string> _selection = new HashSet<string>();
        private readonly Dictionary<string, Control> _cells = new Dictionary<string, Control>();
        private readonly HashSet<Control> _cellSet = new HashSet<Control>();
        private readonly Dictionary<int, Button> _rowHeads = new Dictionary<int, Button>();
        private readonly Dictionary<int, Button> _colHeads = new Dictionary<int, Button>();
        private readonly StackPanel _root = new StackPanel();
        private bool _allowAddColumn = true;
        private string _hint = "";

        private int _anchorR;
        private int _anchorC;
        private bool _hasAnchor;
        private bool _dragging;
        private bool _dragAdditive;
        private bool _syncing;
        private string _lastDrag = "";

        private Border _frame;
        private Grid _grid;
        private TextBlock _info;
        private TextBlock _hintText;
        private Button _copyButton;
        private Button _clearButton;

        public SheetGrid()
        {
            Focusable = true;
            UseLayoutRounding = true;
            SnapsToDevicePixels = true;
            FocusVisualStyle = null;
            Content = _root;
            PreviewMouseLeftButtonDown += SheetGrid_PreviewMouseLeftButtonDown;
            PreviewMouseUp += SheetGrid_PreviewMouseUp;
            PreviewMouseMove += SheetGrid_PreviewMouseMove;
            PreviewKeyDown += SheetGrid_PreviewKeyDown;
            LostMouseCapture += SheetGrid_LostMouseCapture;
            BuildShell();
        }

        public string Hint
        {
            get { return _hint; }
            set
            {
                _hint = value ?? "";
                if (_hintText != null) _hintText.Text = _hint;
            }
        }

        public int RowCount { get { return _data.Count; } }

        public int ColumnCount { get { return _columns.Count; } }

        public string GetCell(int r, int c)
        {
            return _data[r][c];
        }

        public void Setup(IList<SheetColumn> columns, IList<string[]> rows, bool allowAddColumn)
        {
            _columns.Clear();
            foreach (SheetColumn col in columns) _columns.Add(col);

            _data.Clear();
            foreach (string[] row in rows)
            {
                List<string> values = new List<string>();
                foreach (string value in row) values.Add(value ?? "");
                _data.Add(values);
            }

            _allowAddColumn = allowAddColumn;
            Render();
        }

        public void Render()
        {
            UpdateAutomationIds();
            _cells.Clear();
            _cellSet.Clear();
            _rowHeads.Clear();
            _colHeads.Clear();

            Grid g = new Grid();
            g.UseLayoutRounding = true;
            g.SnapsToDevicePixels = true;
            g.SizeChanged += Grid_SizeChanged;

            // Row heights: header 35, data 38 each, add row 37 (all include their 1px line).
            g.RowDefinitions.Add(new RowDefinition { Height = new GridLength(35) });
            for (int r = 0; r < _data.Count; r++)
                g.RowDefinitions.Add(new RowDefinition { Height = new GridLength(38) });
            g.RowDefinitions.Add(new RowDefinition { Height = new GridLength(37) });
            int lastRow = g.RowDefinitions.Count - 1;

            // Column widths: row numbers 61, text columns star (min 110), add-column 61.
            g.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(61) });
            for (int c = 0; c < _columns.Count; c++)
            {
                ColumnDefinition cd = new ColumnDefinition();
                string type = TypeOf(c);
                if (type == "select") cd.Width = new GridLength(123);
                else if (type == "checkbox") cd.Width = new GridLength(122);
                else
                {
                    cd.Width = new GridLength(1, GridUnitType.Star);
                    cd.MinWidth = 110;
                }
                g.ColumnDefinitions.Add(cd);
            }
            if (_allowAddColumn)
                g.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(61) });
            int lastCol = g.ColumnDefinitions.Count - 1;
            int addRowSpan = _columns.Count + (_allowAddColumn ? 1 : 0);
            string p = Prefix;

            // Header row.
            Border corner = new Border();
            corner.Style = StyleRes("SheetFillCell");
            corner.BorderThickness = CellBorder(0, lastCol, 0, lastRow);
            Place(g, corner, 0, 0, 0, 0);

            for (int c = 0; c < _columns.Count; c++)
            {
                Button head = new Button();
                head.Style = StyleRes("SheetHeadButton");
                head.Content = _columns[c].Name;
                head.BorderThickness = CellBorder(c + 1, lastCol, 0, lastRow);
                head.SetValue(AutomationProperties.AutomationIdProperty, p + "_col" + c);
                head.SetValue(AutomationProperties.NameProperty, _columns[c].Name);
                head.Click += ColHead_Click;
                _colHeads.Add(c, head);
                Place(g, head, 0, c + 1, 0, 0);
            }

            if (_allowAddColumn)
            {
                Button addCol = new Button();
                addCol.Style = StyleRes("SheetAddButton");
                addCol.Content = "+";
                addCol.BorderThickness = CellBorder(lastCol, lastCol, 0, lastRow);
                addCol.SetValue(AutomationProperties.AutomationIdProperty, p + "_AddCol");
                addCol.SetValue(AutomationProperties.NameProperty, "Add column");
                addCol.Click += AddCol_Click;
                Place(g, addCol, 0, lastCol, 0, 0);
            }

            // Data rows.
            for (int r = 0; r < _data.Count; r++)
            {
                Button rowHead = new Button();
                rowHead.Style = StyleRes("SheetRowHeadButton");
                rowHead.Content = (r + 1).ToString();
                rowHead.BorderThickness = CellBorder(0, lastCol, r + 1, lastRow);
                rowHead.SetValue(AutomationProperties.AutomationIdProperty, p + "_row" + r);
                rowHead.Click += RowHead_Click;
                _rowHeads.Add(r, rowHead);
                Place(g, rowHead, r + 1, 0, 0, 0);

                for (int c = 0; c < _columns.Count; c++)
                {
                    string type = TypeOf(c);
                    Control cell;
                    if (type == "select")
                    {
                        ComboBox combo = new ComboBox();
                        combo.Style = StyleRes("SheetSelectCell");
                        combo.Items.Add("");
                        string[] options = _columns[c].Options;
                        if (options != null)
                            foreach (string option in options) combo.Items.Add(option);
                        combo.SelectedItem = _data[r][c];
                        combo.SelectionChanged += Cell_SelectionChanged;
                        cell = combo;
                    }
                    else if (type == "checkbox")
                    {
                        CheckBox check = new CheckBox();
                        check.Style = StyleRes("SheetCheckboxCell");
                        check.IsChecked = _data[r][c] == "true";
                        check.Checked += Cell_CheckChanged;
                        check.Unchecked += Cell_CheckChanged;
                        cell = check;
                    }
                    else
                    {
                        TextBox box = new TextBox();
                        box.Style = StyleRes("SheetTextCell");
                        box.Text = _data[r][c];
                        box.TextChanged += Cell_TextChanged;
                        cell = box;
                    }
                    cell.BorderThickness = CellBorder(c + 1, lastCol, r + 1, lastRow);
                    cell.SetValue(AutomationProperties.AutomationIdProperty, p + "_r" + r + "_c" + c);
                    cell.SetValue(AutomationProperties.NameProperty, _columns[c].Name + ", row " + (r + 1));
                    cell.GotKeyboardFocus += Cell_GotKeyboardFocus;
                    cell.PreviewMouseLeftButtonDown += Cell_PreviewMouseLeftButtonDown;
                    _cells.Add(CellKey(r, c), cell);
                    _cellSet.Add(cell);
                    Place(g, cell, r + 1, c + 1, 0, 0);
                }

                if (_allowAddColumn)
                {
                    Border empty = new Border();
                    empty.Style = StyleRes("SheetFillCell");
                    empty.BorderThickness = CellBorder(lastCol, lastCol, r + 1, lastRow);
                    Place(g, empty, r + 1, lastCol, 0, 0);
                }
            }

            // Add row: the "+" header and the wide "+ Add row" button.
            Button addRowHead = new Button();
            addRowHead.Style = StyleRes("SheetAddButton");
            addRowHead.Content = "+";
            addRowHead.BorderThickness = CellBorder(0, lastCol, lastRow, lastRow);
            addRowHead.SetValue(AutomationProperties.AutomationIdProperty, p + "_AddRowHead");
            addRowHead.SetValue(AutomationProperties.NameProperty, "Add row");
            addRowHead.Click += AddRow_Click;
            Place(g, addRowHead, lastRow, 0, 0, 0);

            Button addRow = new Button();
            addRow.Style = StyleRes("SheetAddRowButton");
            addRow.Content = "+ Add row";
            addRow.SetValue(AutomationProperties.AutomationIdProperty, p + "_AddRow");
            addRow.Click += AddRow_Click;
            Place(g, addRow, lastRow, 1, 0, addRowSpan);

            _grid = g;
            _frame.Child = g;
            UpdateClip();
            Paint();
        }

        // JS paint(): refresh only the selected/info visuals, never the visual tree.
        private void Paint()
        {
            _info.Text = _selection.Count + (_selection.Count == 1 ? " cell selected" : " cells selected");
            foreach (KeyValuePair<string, Control> entry in _cells)
                entry.Value.Tag = _selection.Contains(entry.Key) ? "selected" : null;
            foreach (KeyValuePair<int, Button> entry in _rowHeads)
                entry.Value.Tag = RowSelected(entry.Key) ? "selected" : null;
            foreach (KeyValuePair<int, Button> entry in _colHeads)
                entry.Value.Tag = ColSelected(entry.Key) ? "selected" : null;
        }

        private bool RowSelected(int r)
        {
            for (int c = 0; c < _columns.Count; c++)
                if (!_selection.Contains(CellKey(r, c))) return false;
            return true;
        }

        private bool ColSelected(int c)
        {
            for (int r = 0; r < _data.Count; r++)
                if (!_selection.Contains(CellKey(r, c))) return false;
            return true;
        }

        private void Cell_GotKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
        {
            Control cell = (Control)sender;
            int r, c;
            CoordsOf(cell, out r, out c);
            string key = CellKey(r, c);
            if (_selection.Contains(key)) return;
            _selection.Clear();
            _selection.Add(key);
            SetAnchor(r, c);
            Paint();
        }

        // JS mousedown: set drag state first, then selectCell (not handled, so the cell still focuses).
        private void Cell_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            Control cell = (Control)sender;
            ComboBox combo = cell as ComboBox;
            if (combo != null && combo.IsDropDownOpen) return;
            if (!IsVisualDescendant(e.OriginalSource as DependencyObject, cell)) return;
            SelectCell(cell);
        }

        // Checkbox padding is not hit-testable (only the 16x16 box toggles), so clicks there
        // reach the grid root; select the cell by its bounds in that case.
        private void SheetGrid_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            // Popup content lives in its own visual tree; those clicks must not select cells.
            if (!IsVisualDescendant(e.OriginalSource as DependencyObject, this)) return;
            Point point = e.GetPosition(this);
            if (CellAt(InputHitTest(point)) != null) return;
            Control cell = CellAtPoint(point);
            if (cell != null)
            {
                SelectCell(cell);
                Focus();
            }
        }

        private void SelectCell(Control cell)
        {
            int r, c;
            CoordsOf(cell, out r, out c);

            _dragging = true;
            _dragAdditive = IsCtrl();
            _lastDrag = CellKey(r, c);

            if (IsShift() && _hasAnchor)
            {
                AddRect(_anchorR, _anchorC, r, c);
            }
            else if (_dragAdditive)
            {
                string key = CellKey(r, c);
                if (_selection.Contains(key)) _selection.Remove(key);
                else _selection.Add(key);
                SetAnchor(r, c);
            }
            else
            {
                _selection.Clear();
                _selection.Add(CellKey(r, c));
                SetAnchor(r, c);
            }
            Paint();
        }

        // TextBox captures the mouse on left-button down, so sibling MouseEnter events
        // never fire during a drag. Tunneling PreviewMouseMove + hit-testing covers that case.
        private void SheetGrid_PreviewMouseMove(object sender, MouseEventArgs e)
        {
            if (!_dragging || Mouse.LeftButton != MouseButtonState.Pressed) return;
            if (!IsVisualDescendant(e.OriginalSource as DependencyObject, this)) return;
            Control cell = CellAtPoint(e.GetPosition(this));
            if (cell == null) return;
            int r, c;
            CoordsOf(cell, out r, out c);
            string key = CellKey(r, c);
            if (key == _lastDrag) return;
            _lastDrag = key;
            if (!_hasAnchor) return;
            if (!_dragAdditive) _selection.Clear();
            AddRect(_anchorR, _anchorC, r, c);
            Paint();
        }

        private static bool IsVisualDescendant(DependencyObject child, DependencyObject parent)
        {
            while (child != null)
            {
                if (child == parent) return true;
                child = VisualTreeHelper.GetParent(child);
            }
            return false;
        }

        private Control CellAt(IInputElement hit)
        {
            DependencyObject d = hit as DependencyObject;
            while (d != null)
            {
                Control c = d as Control;
                if (c != null && _cellSet.Contains(c)) return c;
                d = VisualTreeHelper.GetParent(d);
            }
            return null;
        }

        private Control CellAtPoint(Point point)
        {
            Control cell = CellAt(InputHitTest(point));
            if (cell != null) return cell;
            foreach (KeyValuePair<string, Control> entry in _cells)
            {
                FrameworkElement element = (FrameworkElement)entry.Value;
                Rect bounds = element.TransformToAncestor(this).TransformBounds(
                    new Rect(0, 0, element.ActualWidth, element.ActualHeight));
                if (bounds.Contains(point)) return element as Control;
            }
            return null;
        }

        private void SheetGrid_PreviewMouseUp(object sender, MouseButtonEventArgs e)
        {
            StopDrag();
        }

        private void SheetGrid_LostMouseCapture(object sender, MouseEventArgs e)
        {
            StopDrag();
        }

        private void StopDrag()
        {
            _dragging = false;
            _lastDrag = "";
        }

        // JS row-header click: Shift selects rows anchor..r, Ctrl toggles, plain selects the row.
        private void RowHead_Click(object sender, RoutedEventArgs e)
        {
            Button head = (Button)sender;
            int r, ignore;
            CoordsOf(head, out r, out ignore);
            bool shift = IsShift();
            bool ctrl = IsCtrl();

            if (shift && _hasAnchor)
            {
                int start = Math.Min(_anchorR, r);
                int end = Math.Max(_anchorR, r);
                if (!ctrl) _selection.Clear();
                for (int i = start; i <= end; i++)
                    for (int c = 0; c < _columns.Count; c++) _selection.Add(CellKey(i, c));
            }
            else if (ctrl)
            {
                for (int c = 0; c < _columns.Count; c++)
                {
                    string key = CellKey(r, c);
                    if (_selection.Contains(key)) _selection.Remove(key);
                    else _selection.Add(key);
                }
                SetAnchor(r, 0);
            }
            else
            {
                _selection.Clear();
                for (int c = 0; c < _columns.Count; c++) _selection.Add(CellKey(r, c));
                SetAnchor(r, 0);
            }
            Paint();
            Focus();
        }

        private void ColHead_Click(object sender, RoutedEventArgs e)
        {
            Button head = (Button)sender;
            int ignore, c;
            CoordsOf(head, out ignore, out c);
            _selection.Clear();
            for (int r = 0; r < _data.Count; r++) _selection.Add(CellKey(r, c));
            SetAnchor(0, c);
            Paint();
            Focus();
        }

        // JS input: write to the model, then propagate to other selected cells of the same type.
        private void Cell_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (_syncing) return;
            Control cell = (Control)sender;
            int r, c;
            CoordsOf(cell, out r, out c);
            WriteAndPropagate(r, c, ((TextBox)cell).Text);
        }

        private void Cell_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (_syncing) return;
            Control cell = (Control)sender;
            int r, c;
            CoordsOf(cell, out r, out c);
            WriteAndPropagate(r, c, GetCellText(CellKey(r, c)));
        }

        private void Cell_CheckChanged(object sender, RoutedEventArgs e)
        {
            if (_syncing) return;
            Control cell = (Control)sender;
            int r, c;
            CoordsOf(cell, out r, out c);
            WriteAndPropagate(r, c, ((CheckBox)cell).IsChecked == true ? "true" : "false");
        }

        // One propagation rule for every cell type: same type everywhere, non-text same column only.
        private void WriteAndPropagate(int r, int c, string value)
        {
            _data[r][c] = value;
            if (_selection.Count <= 1) return;
            _syncing = true;
            try
            {
                foreach (string key in _selection)
                {
                    string[] parts = key.Split(':');
                    int sr = int.Parse(parts[0]);
                    int sc = int.Parse(parts[1]);
                    if ((sr == r && sc == c) || sr >= _data.Count || sc >= _columns.Count) continue;
                    if (TypeOf(sc) != TypeOf(c)) continue;
                    if (TypeOf(c) != "text" && sc != c) continue;
                    _data[sr][sc] = value;
                    SetCellText(key, value);
                }
            }
            finally { _syncing = false; }
        }

        private void SetCellText(string key, string value)
        {
            Control target;
            if (!_cells.TryGetValue(key, out target)) return;
            TextBox box = target as TextBox;
            if (box != null) { box.Text = value; return; }
            ComboBox combo = target as ComboBox;
            if (combo != null) { combo.SelectedItem = value; return; }
            CheckBox check = target as CheckBox;
            if (check != null) check.IsChecked = value == "true";
        }

        private string GetCellText(string key)
        {
            Control target;
            if (!_cells.TryGetValue(key, out target)) return "";
            TextBox box = target as TextBox;
            if (box != null) return box.Text;
            ComboBox combo = target as ComboBox;
            if (combo != null) return combo.SelectedItem == null ? "" : combo.SelectedItem.ToString();
            CheckBox check = target as CheckBox;
            if (check != null) return check.IsChecked == true ? "true" : "false";
            return "";
        }

        private string TypeOf(int c)
        {
            if (c < 0 || c >= _columns.Count) return "text";
            string type = _columns[c].Type;
            return string.IsNullOrEmpty(type) ? "text" : type;
        }

        private void CopyButton_Click(object sender, RoutedEventArgs e)
        {
            CopySelected();
        }

        private void ClearButton_Click(object sender, RoutedEventArgs e)
        {
            ClearSelected();
        }

        // JS clearSelected(): blank every selected cell, in the model and in place.
        private void ClearSelected()
        {
            _syncing = true;
            try
            {
                foreach (string key in _selection)
                {
                    string[] parts = key.Split(':');
                    int r = int.Parse(parts[0]);
                    int c = int.Parse(parts[1]);
                    if (r >= _data.Count || c >= _columns.Count) continue;
                    _data[r][c] = "";
                    SetCellText(key, "");
                }
            }
            finally { _syncing = false; }
        }

        private void SheetGrid_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (IsCtrl() && !HasAlt() && e.Key == Key.V)
            {
                string text;
                try { text = Clipboard.GetText(); }
                catch (ExternalException) { return; }
                if (Paste(text)) e.Handled = true;
                return;
            }
            if (IsCtrl() && !HasAlt() && e.Key == Key.C)
            {
                TextBox box = Keyboard.FocusedElement as TextBox;
                if (box != null && box.SelectionLength > 0) return;
                if (CopySelected()) e.Handled = true;
                return;
            }

            Control cell = CellAt(Keyboard.FocusedElement);
            if (cell == null) return;
            ComboBox combo = cell as ComboBox;
            if (combo != null && combo.IsDropDownOpen) return;
            int r, c;
            CoordsOf(cell, out r, out c);
            if ((e.Key == Key.Delete || e.Key == Key.Back) && _selection.Count > 1)
            {
                e.Handled = true;
                ClearSelected();
                return;
            }
            if (e.Key == Key.Enter)
            {
                e.Handled = true;
                if (Ensure(r + 1, c)) Render();
                FocusCell(r + 1, c);
            }
        }

        // JS ensure(): grow rows and columns. New columns are text even with allowAddColumn=false.
        private bool Ensure(int r, int c)
        {
            bool changed = false;
            while (_data.Count <= r)
            {
                List<string> row = new List<string>();
                for (int i = 0; i < _columns.Count; i++) row.Add("");
                _data.Add(row);
                changed = true;
            }
            while (_columns.Count <= c)
            {
                SheetColumn col = new SheetColumn();
                col.Name = "Column " + (_columns.Count + 1);
                col.Type = "text";
                _columns.Add(col);
                foreach (List<string> row in _data) row.Add("");
                changed = true;
            }
            return changed;
        }

        private void FocusCell(int r, int c)
        {
            Control cell;
            if (_cells.TryGetValue(CellKey(r, c), out cell)) cell.Focus();
        }

        // JS selectionText(): TSV of the selection's bounding box; only for multiple cells.
        private bool CopySelected()
        {
            if (_selection.Count < 2) return false;
            int minR = int.MaxValue, maxR = int.MinValue, minC = int.MaxValue, maxC = int.MinValue;
            foreach (string key in _selection)
            {
                string[] parts = key.Split(':');
                int r = int.Parse(parts[0]);
                int c = int.Parse(parts[1]);
                minR = Math.Min(minR, r);
                maxR = Math.Max(maxR, r);
                minC = Math.Min(minC, c);
                maxC = Math.Max(maxC, c);
            }
            StringBuilder sb = new StringBuilder();
            for (int r = minR; r <= maxR; r++)
            {
                if (r > minR) sb.Append('\n');
                for (int c = minC; c <= maxC; c++)
                {
                    if (c > minC) sb.Append('\t');
                    // Web fix: only selected cells contribute; the bounding-box shape is kept.
                    string key = CellKey(r, c);
                    sb.Append(_selection.Contains(key) ? GetCellText(key) : "");
                }
            }
            try { Clipboard.SetText(sb.ToString()); }
            catch (ExternalException) { return false; }
            // ponytail: no shared toast yet
            return true;
        }

        // JS pasteData(): only tab/newline text, then grow and parse typed values.
        private bool Paste(string text)
        {
            if (string.IsNullOrEmpty(text) || (!text.Contains("\t") && !text.Contains("\n"))) return false;
            int r0 = 0, c0 = 0;
            Control focused = CellAt(Keyboard.FocusedElement);
            if (focused != null)
                CoordsOf(focused, out r0, out c0);
            else if (_hasAnchor)
            {
                r0 = _anchorR;
                c0 = _anchorC;
            }

            string[] rows = text.Replace("\r", "").Split('\n');
            if (rows.Length > 0 && rows[rows.Length - 1] == "")
            {
                string[] trimmed = new string[rows.Length - 1];
                Array.Copy(rows, 0, trimmed, 0, trimmed.Length);
                rows = trimmed;
            }
            for (int ri = 0; ri < rows.Length; ri++)
            {
                string[] values = rows[ri].Split('\t');
                for (int ci = 0; ci < values.Length; ci++)
                {
                    int c = c0 + ci;
                    // Web fix: with allowAddColumn=false, skip cells past the last column
                    // (continue with the rest of the paste); rows still grow as before.
                    if (!_allowAddColumn && c >= _columns.Count) continue;
                    Ensure(r0 + ri, c);
                    string stored = values[ci];
                    string type = TypeOf(c);
                    if (type == "checkbox")
                    {
                        string v = stored.Trim();
                        bool on = v.Equals("true", StringComparison.OrdinalIgnoreCase)
                               || v.Equals("1", StringComparison.OrdinalIgnoreCase)
                               || v.Equals("yes", StringComparison.OrdinalIgnoreCase);
                        stored = on ? "true" : "false";
                    }
                    else if (type == "select")
                    {
                        bool found = false;
                        string[] options = _columns[c].Options;
                        if (options != null)
                            foreach (string option in options)
                                if (option == stored) { found = true; break; }
                        if (!found) stored = "";
                    }
                    _data[r0 + ri][c] = stored;
                }
            }
            Render();
            FocusCell(r0, c0);
            return true;
        }

        private void AddRow_Click(object sender, RoutedEventArgs e)
        {
            List<string> row = new List<string>();
            for (int c = 0; c < _columns.Count; c++) row.Add("");
            _data.Add(row);
            Render();
        }

        private void AddCol_Click(object sender, RoutedEventArgs e)
        {
            SheetColumn col = new SheetColumn();
            col.Name = "Column " + (_columns.Count + 1);
            col.Type = "text";
            _columns.Add(col);
            foreach (List<string> row in _data) row.Add("");
            Render();
        }

        private void AddRect(int r0, int c0, int r1, int c1)
        {
            for (int r = Math.Min(r0, r1); r <= Math.Max(r0, r1); r++)
                for (int c = Math.Min(c0, c1); c <= Math.Max(c0, c1); c++)
                    _selection.Add(CellKey(r, c));
        }

        private void SetAnchor(int r, int c)
        {
            _anchorR = r;
            _anchorC = c;
            _hasAnchor = true;
        }

        // One coordinate derivation for cells and headers: controls are placed one row/column
        // past the data index to leave room for the header row and row-number column.
        private static void CoordsOf(Control element, out int r, out int c)
        {
            r = (int)element.GetValue(Grid.RowProperty) - 1;
            c = (int)element.GetValue(Grid.ColumnProperty) - 1;
        }

        private static bool IsShift()
        {
            return (Keyboard.Modifiers & ModifierKeys.Shift) != ModifierKeys.None;
        }

        private static bool IsCtrl()
        {
            return (Keyboard.Modifiers & (ModifierKeys.Control | ModifierKeys.Windows)) != ModifierKeys.None;
        }

        // AltGr is reported as Ctrl+Alt; it must not be eaten as a grid shortcut.
        private static bool HasAlt()
        {
            return (Keyboard.Modifiers & ModifierKeys.Alt) != ModifierKeys.None;
        }

        private static string CellKey(int r, int c)
        {
            return r + ":" + c;
        }

        private void BuildShell()
        {
            Grid toolbar = new Grid();
            toolbar.Height = 32;
            toolbar.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            toolbar.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });

            StackPanel actions = new StackPanel();
            actions.Orientation = Orientation.Horizontal;
            actions.VerticalAlignment = VerticalAlignment.Center;

            _copyButton = new Button();
            _copyButton.Style = StyleRes("GhostButtonSm");
            _copyButton.Content = "Copy selected";
            _copyButton.Click += CopyButton_Click;
            actions.Children.Add(_copyButton);

            _clearButton = new Button();
            _clearButton.Style = StyleRes("GhostButtonSm");
            _clearButton.Content = "Clear selected";
            _clearButton.Margin = new Thickness(8, 0, 0, 0);
            _clearButton.Click += ClearButton_Click;
            actions.Children.Add(_clearButton);

            Grid.SetColumn(actions, 0);
            toolbar.Children.Add(actions);

            _info = new TextBlock();
            _info.Style = StyleRes("SheetInfo");
            _info.Text = "0 cells selected";
            Grid.SetColumn(_info, 1);
            toolbar.Children.Add(_info);

            _frame = new Border();
            _frame.Style = StyleRes("SheetFrame");
            _frame.Margin = new Thickness(0, 14, 0, 0);

            _hintText = new TextBlock();
            _hintText.Style = StyleRes("SheetHint");
            _hintText.Margin = new Thickness(0, 14, 0, 0);
            _hintText.Text = _hint;

            _root.Children.Add(toolbar);
            _root.Children.Add(_frame);
            _root.Children.Add(_hintText);
        }

        private void UpdateAutomationIds()
        {
            string p = Prefix;
            _copyButton.SetValue(AutomationProperties.AutomationIdProperty, p + "_Copy");
            _clearButton.SetValue(AutomationProperties.AutomationIdProperty, p + "_Clear");
            _info.SetValue(AutomationProperties.AutomationIdProperty, p + "_Info");
        }

        // Border.CornerRadius does not clip children: clip the grid so the Surface2
        // header and BrandSoft add row follow the frame's 12px radius (12 - 1px frame).
        private void Grid_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            UpdateClip();
        }

        private void UpdateClip()
        {
            if (_grid == null || _grid.ActualWidth <= 0 || _grid.ActualHeight <= 0) return;
            _grid.Clip = new RectangleGeometry(new Rect(0, 0, _grid.ActualWidth, _grid.ActualHeight), 11, 11);
        }

        private static void Place(Grid g, UIElement child, int row, int col, int rowSpan, int colSpan)
        {
            Grid.SetRow(child, row);
            Grid.SetColumn(child, col);
            if (rowSpan > 0) Grid.SetRowSpan(child, rowSpan);
            if (colSpan > 0) Grid.SetColumnSpan(child, colSpan);
            g.Children.Add(child);
        }

        private static Thickness CellBorder(int col, int lastCol, int row, int lastRow)
        {
            double right = col < lastCol ? 1 : 0;
            double bottom = row < lastRow ? 1 : 0;
            return new Thickness(0, 0, right, bottom);
        }

        private string Prefix
        {
            get
            {
                string id = AutomationProperties.GetAutomationId(this);
                return string.IsNullOrEmpty(id) ? "Sheet" : id;
            }
        }

        private static Style StyleRes(string key)
        {
            return (Style)Application.Current.TryFindResource(key);
        }
    }
}
