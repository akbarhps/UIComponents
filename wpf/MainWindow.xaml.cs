using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;

namespace UIComponents.Wpf
{
    public partial class MainWindow : Window
    {
        private void OpenDialog_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new DialogWindow { Owner = this };
            dialog.ShowDialog();
            DialogResultText.Text = dialog.Confirmed ? "Project deleted." : "Dialog cancelled.";
        }

        public MainWindow()
        {
            InitializeComponent();
            DemoGrid.ItemsSource = new List<ProjectRow>
            {
                new ProjectRow { Project = "Website refresh", Owner = "Ada Lovelace", Initials = "AL", Team = "Design team", Status = "Active", Updated = "Today", Budget = "$12,400" },
                new ProjectRow { Project = "Mobile app", Owner = "Jonas Silva", Initials = "JS", Team = "Engineering", Status = "In review", Updated = "Yesterday", Budget = "$28,900" },
                new ProjectRow { Project = "Brand guidelines", Owner = "Mia Chen", Initials = "MC", Team = "Design team", Status = "Pending", Updated = "3 days ago", Budget = "$4,200" },
                new ProjectRow { Project = "Help center", Owner = "Ada Lovelace", Initials = "AL", Team = "Content", Status = "Active", Updated = "Last week", Budget = "$8,750" },
                new ProjectRow { Project = "Legacy archive", Owner = "Sam Patel", Initials = "SP", Team = "Operations", Status = "Draft", Updated = "2 weeks ago", Budget = "$1,100" }
            };
            ApplyStandaloneMode();
        }

        // Launch with a section title (e.g. "Data table") to show only that section.
        private void ApplyStandaloneMode()
        {
            var args = Environment.GetCommandLineArgs();
            if (args.Length < 2) return;

            var key = args[1];
            var panels = new Panel[]
            {
                Sections, FormInputs.SectionList, DataTables.SectionList, AdvancedTables.SectionList
            };
            bool found = false;
            foreach (var panel in panels)
                if (StandaloneHelper.Contains(panel, key)) { found = true; break; }
            if (!found) return;

            foreach (var panel in panels) StandaloneHelper.Apply(panel, key);
            FormInputs.Visibility = StandaloneHelper.Contains(FormInputs.SectionList, key) ? Visibility.Visible : Visibility.Collapsed;
            DataTables.Visibility = StandaloneHelper.Contains(DataTables.SectionList, key) ? Visibility.Visible : Visibility.Collapsed;
            AdvancedTables.Visibility = StandaloneHelper.Contains(AdvancedTables.SectionList, key) ? Visibility.Visible : Visibility.Collapsed;

            Title = key + " — Claude UI";
            ContentScroll.Padding = new Thickness(48, 40, 48, 48);
        }
    }

    public class ProjectRow
    {
        public string Project { get; set; }
        public string Owner { get; set; }
        public string Initials { get; set; }
        public string Team { get; set; }
        public string Status { get; set; }
        public string Updated { get; set; }
        public string Budget { get; set; }
    }

    internal static class StandaloneHelper
    {
        public static bool Contains(Panel panel, string key)
        {
            var style = (Style)panel.FindResource("SectionTitle");
            string current = null;
            foreach (UIElement child in panel.Children)
            {
                var text = child as TextBlock;
                if (text != null && text.Style == style) current = text.Text;
                if (current != null && string.Equals(current, key, StringComparison.OrdinalIgnoreCase)) return true;
            }
            return false;
        }

        public static void Apply(Panel panel, string key)
        {
            var style = (Style)panel.FindResource("SectionTitle");
            string current = null;
            foreach (UIElement child in panel.Children)
            {
                var text = child as TextBlock;
                if (text != null && text.Style == style) current = text.Text;
                child.Visibility = current != null && string.Equals(current, key, StringComparison.OrdinalIgnoreCase)
                    ? Visibility.Visible : Visibility.Collapsed;
            }
        }
    }
}
