using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Automation;
using System.Windows.Controls;
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
    /// Port of the web createGrid() control. Phase 1 renders the static idle state;
    /// later phases add selection, editing and data operations on the same model.
    /// </summary>
    public class SheetGrid : UserControl
    {
        private readonly List<SheetColumn> _columns = new List<SheetColumn>();
        private readonly List<List<string>> _data = new List<List<string>>();
        private readonly StackPanel _root = new StackPanel();
        private bool _allowAddColumn = true;
        private string _hint = "";

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
                cd.Width = new GridLength(1, GridUnitType.Star);
                cd.MinWidth = 110;
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
                Place(g, rowHead, r + 1, 0, 0, 0);

                for (int c = 0; c < _columns.Count; c++)
                {
                    TextBox cell = new TextBox();
                    cell.Style = StyleRes("SheetTextCell");
                    cell.Text = _data[r][c];
                    cell.BorderThickness = CellBorder(c + 1, lastCol, r + 1, lastRow);
                    cell.SetValue(AutomationProperties.AutomationIdProperty, p + "_r" + r + "_c" + c);
                    cell.SetValue(AutomationProperties.NameProperty, _columns[c].Name + ", row " + (r + 1));
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
            Place(g, addRowHead, lastRow, 0, 0, 0);

            Button addRow = new Button();
            addRow.Style = StyleRes("SheetAddRowButton");
            addRow.Content = "+ Add row";
            addRow.SetValue(AutomationProperties.AutomationIdProperty, p + "_AddRow");
            Place(g, addRow, lastRow, 1, 0, addRowSpan);

            _grid = g;
            _frame.Child = g;
            UpdateClip();
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
            actions.Children.Add(_copyButton);

            _clearButton = new Button();
            _clearButton.Style = StyleRes("GhostButtonSm");
            _clearButton.Content = "Clear selected";
            _clearButton.Margin = new Thickness(8, 0, 0, 0);
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
