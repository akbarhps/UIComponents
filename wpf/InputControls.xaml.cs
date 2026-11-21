using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Microsoft.Win32;

namespace UIComponents.Wpf
{
    public partial class InputControls : UserControl
    {
        private readonly ObservableCollection<string> _tags = new ObservableCollection<string>();
        private readonly string[] _options = { "React", "TypeScript", "Vue", "Svelte", "Node.js", "Python" };

        public InputControls()
        {
            InitializeComponent();
            _tags.Add("design");
            _tags.Add("research");
            TagList.ItemsSource = _tags;
            UpdateTagCount();
        }

        private void UpdateTagCount()
        {
            TagCount.Text = _tags.Count + " tags";
        }

        private void TagInput_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter || e.Key == Key.OemComma)
            {
                e.Handled = true;
                foreach (var part in TagInput.Text.Split(','))
                {
                    var value = part.Trim();
                    if (value.Length > 0 && !_tags.Any(t => t.ToLower() == value.ToLower())) _tags.Add(value);
                }
                TagInput.Text = "";
                UpdateTagCount();
            }
            else if (e.Key == Key.Back && TagInput.Text.Length == 0 && _tags.Count > 0)
            {
                _tags.RemoveAt(_tags.Count - 1);
                UpdateTagCount();
            }
        }

        private void RemoveTag_Click(object sender, RoutedEventArgs e)
        {
            var button = (Button)sender;
            var tag = button.DataContext as string;
            if (tag != null) _tags.Remove(tag);
            UpdateTagCount();
        }

        private void AutoInput_TextChanged(object sender, TextChangedEventArgs e)
        {
            var q = AutoInput.Text.Trim().ToLower();
            AutoList.ItemsSource = _options.Where(o => o.ToLower().Contains(q)).ToList();
            AutoPopup.IsOpen = AutoList.Items.Count > 0;
        }

        private void AutoList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (AutoList.SelectedItem == null) return;
            AutoInput.Text = AutoList.SelectedItem.ToString();
            AutoPopup.IsOpen = false;
        }

        private void Browse_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new OpenFileDialog { Multiselect = true, Filter = "All files|*.*" };
            if (dialog.ShowDialog() == true)
                foreach (var file in dialog.FileNames) FileList.Items.Add(file);
        }
    }
}
