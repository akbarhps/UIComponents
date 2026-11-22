using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Windows;
using System.Windows.Automation;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using Microsoft.Win32;

namespace UIComponents.Wpf
{
    // Code-only control (like CalendarPicker): drop zone, file rows and count.
    public class FileInput : Grid
    {
        private const string ImageFilter = "Images|*.png;*.jpg;*.jpeg;*.gif;*.webp;*.svg;*.bmp";
        private static readonly string[] ImageExtensions = { ".png", ".jpg", ".jpeg", ".gif", ".webp", ".svg", ".bmp" };

        private sealed class Entry
        {
            public string Name;
            public long Size;
        }

        private readonly List<Entry> _files = new List<Entry>();
        private string _id;
        private Button _drop;
        private StackPanel _list;
        private TextBlock _count;

        public bool Multiple { get; set; }
        public bool ImageOnly { get; set; }
        public string Title { get; set; }
        public string Subtitle { get; set; }

        protected override void OnInitialized(EventArgs e)
        {
            base.OnInitialized(e);
            _id = AutomationProperties.GetAutomationId(this);
            if (string.IsNullOrEmpty(_id)) _id = Name;
            Build();
        }

        private static object Res(string key)
        {
            return Application.Current.FindResource(key);
        }

        private void Build()
        {
            _drop = new Button();
            _drop.Style = (Style)Res("PickerFileDrop");
            _drop.AllowDrop = true;
            _drop.Click += Drop_Click;
            _drop.PreviewDragEnter += Drop_DragOver;
            _drop.PreviewDragOver += Drop_DragOver;
            _drop.PreviewDragLeave += Drop_DragLeave;
            _drop.Drop += Drop_Drop;
            AutomationProperties.SetAutomationId(_drop, _id + "_Drop");
            AutomationProperties.SetName(_drop, Title);
            Grid content = new Grid();
            StackPanel stack = new StackPanel();
            stack.HorizontalAlignment = HorizontalAlignment.Center;
            stack.VerticalAlignment = VerticalAlignment.Center;
            ContentControl icon = new ContentControl();
            icon.Style = (Style)Res("IconUpload");
            icon.Width = 24;
            icon.Height = 24;
            icon.Foreground = (Brush)Res("Brand");
            icon.HorizontalAlignment = HorizontalAlignment.Center;
            stack.Children.Add(icon);
            TextBlock title = new TextBlock();
            title.Text = Title;
            title.FontFamily = (FontFamily)Res("Sans");
            title.FontSize = 14;
            title.FontWeight = FontWeights.Bold;
            title.LineHeight = 22.4;
            title.LineStackingStrategy = LineStackingStrategy.BlockLineHeight;
            title.HorizontalAlignment = HorizontalAlignment.Center;
            title.Margin = new Thickness(0, 8, 0, 0);
            stack.Children.Add(title);
            TextBlock subtitle = new TextBlock();
            subtitle.Text = Subtitle;
            subtitle.FontFamily = (FontFamily)Res("Sans");
            subtitle.FontSize = 12;
            subtitle.Foreground = (Brush)Res("Ink3");
            subtitle.LineHeight = 19.2;
            subtitle.LineStackingStrategy = LineStackingStrategy.BlockLineHeight;
            subtitle.HorizontalAlignment = HorizontalAlignment.Center;
            subtitle.Margin = new Thickness(0, 8, 0, 0);
            stack.Children.Add(subtitle);
            content.Children.Add(stack);
            _drop.Content = content;

            _list = new StackPanel();
            AutomationProperties.SetAutomationId(_list, _id + "_List");
            UIElement listHost = _list;
            if (Multiple)
            {
                // Web: max-height 220 with a 4px right gutter; items are 446 wide.
                _list.Margin = new Thickness(0, 0, 4, 0);
                ScrollViewer scroller = new ScrollViewer();
                scroller.MaxHeight = 220; // web max-height; the 12px column gap is the Margin below (outside MaxHeight)
                scroller.VerticalScrollBarVisibility = ScrollBarVisibility.Auto;
                scroller.HorizontalScrollBarVisibility = ScrollBarVisibility.Disabled;
                scroller.Margin = new Thickness(0, 12, 0, 0);
                scroller.Content = _list;
                listHost = scroller;
            }
            else _list.Margin = new Thickness(0, 12, 0, 0);

            _count = new TextBlock();
            _count.FontFamily = (FontFamily)Res("Sans");
            _count.FontSize = 12;
            _count.Foreground = (Brush)Res("Ink2");
            _count.LineHeight = 19.2;
            _count.LineStackingStrategy = LineStackingStrategy.BlockLineHeight;
            _count.Margin = new Thickness(0, 12, 0, 0);
            _count.Visibility = Visibility.Collapsed;
            AutomationProperties.SetAutomationId(_count, _id + "_Count");

            StackPanel root = new StackPanel();
            root.Children.Add(_drop);
            root.Children.Add(listHost);
            root.Children.Add(_count);
            Children.Add(root);
            Render();
        }

        // The same predicate drives both the dialog filter and the drop filter (web W11).
        private bool Accepted(string path)
        {
            if (!ImageOnly) return true;
            string ext = Path.GetExtension(path).ToLowerInvariant();
            foreach (string candidate in ImageExtensions)
                if (ext == candidate) return true;
            return false;
        }

        private static string SizeLabel(long bytes)
        {
            if (bytes < 1024) return bytes.ToString(CultureInfo.InvariantCulture) + " B";
            if (bytes < 1024 * 1024)
                return Math.Round(bytes / 1024.0, MidpointRounding.AwayFromZero).ToString(CultureInfo.InvariantCulture) + " KB";
            return (bytes / (1024.0 * 1024.0)).ToString("0.0", CultureInfo.InvariantCulture) + " MB";
        }

        private void AddFiles(IList<string> paths)
        {
            var fresh = new List<Entry>();
            foreach (string path in paths)
            {
                if (!Accepted(path)) continue;
                string name = Path.GetFileName(path);
                long size = 0;
                try { size = new FileInfo(path).Length; }
                catch (Exception) { size = 0; }
                if (Multiple)
                {
                    bool duplicate = false;
                    foreach (Entry existing in _files)
                        if (existing.Name == name && existing.Size == size) { duplicate = true; break; }
                    foreach (Entry existing in fresh)
                        if (existing.Name == name && existing.Size == size) { duplicate = true; break; }
                    if (duplicate) continue;
                }
                fresh.Add(new Entry { Name = name, Size = size });
            }
            if (fresh.Count == 0) return; // W11: empty/invalid selection is a no-op
            if (Multiple) _files.AddRange(fresh);
            else { _files.Clear(); _files.Add(fresh[0]); }
            Render();
        }

        private void Drop_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog dialog = new OpenFileDialog();
            dialog.Multiselect = Multiple;
            dialog.Filter = ImageOnly ? ImageFilter : "All files|*.*";
            if (dialog.ShowDialog() == true) AddFiles(dialog.FileNames);
        }

        private void Drop_DragOver(object sender, DragEventArgs e)
        {
            e.Effects = DragDropEffects.Copy;
            e.Handled = true;
            _drop.Tag = "dragover";
        }

        private void Drop_DragLeave(object sender, DragEventArgs e)
        {
            _drop.Tag = null;
        }

        private void Drop_Drop(object sender, DragEventArgs e)
        {
            _drop.Tag = null;
            if (!e.Data.GetDataPresent(DataFormats.FileDrop)) return;
            string[] paths = e.Data.GetData(DataFormats.FileDrop) as string[];
            if (paths != null) AddFiles(paths);
        }

        private void Remove_Click(object sender, RoutedEventArgs e)
        {
            Button button = sender as Button;
            if (button == null || !(button.Tag is int)) return;
            int index = (int)button.Tag;
            if (index < 0 || index >= _files.Count) return;
            _files.RemoveAt(index);
            Render();
        }

        private void Render()
        {
            _list.Children.Clear();
            for (int i = 0; i < _files.Count; i++)
            {
                Entry entry = _files[i];
                Border row = new Border();
                row.Style = (Style)Res("PickerFileItem");
                row.Margin = new Thickness(0, i == 0 ? 0 : 8, 0, 0); // web .file-list gap 8
                Grid grid = new Grid();
                grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(16) });
                grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(10) });
                grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
                grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(10) });
                grid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
                grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(10) });
                grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(26) });

                ContentControl fileIcon = new ContentControl();
                fileIcon.Style = (Style)Res("IconFile");
                fileIcon.Width = 16;
                fileIcon.Height = 16;
                fileIcon.Foreground = (Brush)Res("Ink3");
                grid.Children.Add(fileIcon);

                TextBlock name = new TextBlock();
                name.Text = entry.Name;
                name.FontFamily = (FontFamily)Res("Sans");
                name.FontSize = 14;
                name.Foreground = (Brush)Res("Ink");
                name.TextTrimming = TextTrimming.CharacterEllipsis;
                name.TextWrapping = TextWrapping.NoWrap;
                name.VerticalAlignment = VerticalAlignment.Center;
                Grid.SetColumn(name, 2);
                grid.Children.Add(name);

                TextBlock size = new TextBlock();
                size.Text = SizeLabel(entry.Size);
                size.FontFamily = (FontFamily)Res("Sans");
                size.FontSize = 12;
                size.Foreground = (Brush)Res("Ink3");
                size.VerticalAlignment = VerticalAlignment.Center;
                Grid.SetColumn(size, 4);
                grid.Children.Add(size);

                Button remove = new Button();
                remove.Style = (Style)Res("IconButtonSm");
                remove.Tag = i;
                remove.Click += Remove_Click;
                Grid.SetColumn(remove, 6);
                AutomationProperties.SetAutomationId(remove, _id + "_Remove" + i);
                AutomationProperties.SetName(remove, "Remove " + entry.Name);
                ContentControl close = new ContentControl();
                close.Style = (Style)Res("IconClose");
                close.Width = 15;
                close.Height = 15;
                close.Foreground = (Brush)Res("Ink3");
                remove.Content = close;
                grid.Children.Add(remove);

                row.Child = grid;
                AutomationProperties.SetAutomationId(row, _id + "_Item" + i);
                AutomationProperties.SetName(row, entry.Name);
                _list.Children.Add(row);
            }
            _count.Text = _files.Count.ToString(CultureInfo.InvariantCulture)
                + (_files.Count == 1 ? " file selected" : " files selected");
            _count.Visibility = _files.Count == 0 ? Visibility.Collapsed : Visibility.Visible;
        }
    }
}
