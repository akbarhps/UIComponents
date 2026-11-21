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
                new ProjectRow { Project = "Website refresh", Owner = "Ada Lovelace", Initials = "AL", Team = "Design team", Status = "Active", Updated = "Today", Budget = "$12,400" },
                new ProjectRow { Project = "Mobile app", Owner = "Jonas Silva", Initials = "JS", Team = "Engineering", Status = "In review", Updated = "Yesterday", Budget = "$28,900" },
                new ProjectRow { Project = "Brand guidelines", Owner = "Mia Chen", Initials = "MC", Team = "Design team", Status = "Pending", Updated = "3 days ago", Budget = "$4,200" },
                new ProjectRow { Project = "Help center", Owner = "Ada Lovelace", Initials = "AL", Team = "Content", Status = "Active", Updated = "Last week", Budget = "$8,750" },
                new ProjectRow { Project = "Legacy archive", Owner = "Sam Patel", Initials = "SP", Team = "Operations", Status = "Draft", Updated = "2 weeks ago", Budget = "$1,100" }
            };
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
