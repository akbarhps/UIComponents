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
            var titleStyle = (Style)FindResource("SectionTitle");
            string current = null;
            bool matched = false;
            foreach (UIElement child in Sections.Children)
            {
                var text = child as TextBlock;
                if (text != null && text.Style == titleStyle) current = text.Text;
                bool show = current != null && string.Equals(current, key, StringComparison.OrdinalIgnoreCase);
                child.Visibility = show ? Visibility.Visible : Visibility.Collapsed;
                if (show) matched = true;
            }

            if (matched)
            {
                Title = key + " — Claude UI";
                ContentScroll.Padding = new Thickness(48, 40, 48, 48);
            }
            else
            {
                foreach (UIElement child in Sections.Children) child.Visibility = Visibility.Visible;
            }
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
}
