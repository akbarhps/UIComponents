using System.Collections.Generic;
using System.Windows;

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
                new ProjectRow { Project = "Website refresh", Owner = "Ada Lovelace", Status = "Active", Updated = "Today", Budget = "$12,400" },
                new ProjectRow { Project = "Mobile app", Owner = "Jonas Silva", Status = "Active", Updated = "Yesterday", Budget = "$28,900" },
                new ProjectRow { Project = "Brand guidelines", Owner = "Mia Chen", Status = "Pending", Updated = "3 days ago", Budget = "$4,200" },
                new ProjectRow { Project = "Help center", Owner = "Ada Lovelace", Status = "Active", Updated = "Last week", Budget = "$8,750" },
                new ProjectRow { Project = "Legacy archive", Owner = "Sam Patel", Status = "Draft", Updated = "2 weeks ago", Budget = "$1,100" }
            };
        }
    }

    public class ProjectRow
    {
        public string Project { get; set; }
        public string Owner { get; set; }
        public string Status { get; set; }
        public string Updated { get; set; }
        public string Budget { get; set; }
    }
}
