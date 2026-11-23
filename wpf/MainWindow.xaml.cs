using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media.Animation;
using System.Windows.Threading;

namespace UIComponents.Wpf
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            SyncThemeToggle();
            ThemeManager.ThemeChanged += OnThemeChanged;
            Closed += delegate { ThemeManager.ThemeChanged -= OnThemeChanged; };
            Toasts.Install(this);
            OverlayHost.Install(this, Overlays.DialogStore);
            DemoGrid.ItemsSource = new List<ProjectRow>
            {
                new ProjectRow { Project = "Website refresh", Owner = "Ada Lovelace", Initials = "AL", Team = "Design team", Status = "Active", Updated = "Today", Budget = "$12,400" },
                new ProjectRow { Project = "Mobile app", Owner = "Jonas Silva", Initials = "JS", Team = "Engineering", Status = "In review", Updated = "Yesterday", Budget = "$28,900" },
                new ProjectRow { Project = "Brand guidelines", Owner = "Mia Chen", Initials = "MC", Team = "Design team", Status = "Pending", Updated = "3 days ago", Budget = "$4,200" },
                new ProjectRow { Project = "Help center", Owner = "Ada Lovelace", Initials = "AL", Team = "Content", Status = "Active", Updated = "Last week", Budget = "$8,750" },
                new ProjectRow { Project = "Legacy archive", Owner = "Sam Patel", Initials = "SP", Team = "Operations", Status = "Draft", Updated = "2 weeks ago", Budget = "$1,100" }
            };
            ApplyStandaloneMode();
            ApplyReducedMotion();
        }

        private bool _syncingTheme;

        private void OnThemeChanged(object sender, EventArgs e) { SyncThemeToggle(); }

        // Checks the radio that matches ThemeManager.Mode (the user choice, not the resolved theme).
        private void SyncThemeToggle()
        {
            _syncingTheme = true;
            try
            {
                ThemeSystem.IsChecked = ThemeManager.Mode == ThemeMode.System;
                ThemeLight.IsChecked = ThemeManager.Mode == ThemeMode.Light;
                ThemeDark.IsChecked = ThemeManager.Mode == ThemeMode.Dark;
            }
            finally { _syncingTheme = false; }
        }

        private void ThemeToggle_Checked(object sender, RoutedEventArgs e)
        {
            if (_syncingTheme) return;
            string tag = (string)((FrameworkElement)sender).Tag;
            ThemeManager.SetTheme(tag == "dark" ? ThemeMode.Dark : tag == "light" ? ThemeMode.Light : ThemeMode.System);
        }

        // Launch with a section title (e.g. "Data table") to show only that section.
        private void ApplyStandaloneMode()
        {
            var args = Environment.GetCommandLineArgs();
            if (args.Length < 2) return;

            var key = args[1];
            // Hosts are the UserControls directly under Sections; each one names its section list "SectionList".
            var panels = new List<Panel> { Sections };
            var hosts = new List<UserControl>();
            foreach (UIElement child in Sections.Children)
            {
                var host = child as UserControl;
                var list = host == null ? null : host.FindName("SectionList") as Panel;
                if (list != null) { hosts.Add(host); panels.Add(list); }
            }
            bool found = false;
            foreach (var panel in panels)
                if (StandaloneHelper.Contains(panel, key)) { found = true; break; }
            if (!found) return;

            foreach (var panel in panels) StandaloneHelper.Apply(panel, key);
            foreach (var host in hosts)
                host.Visibility = StandaloneHelper.Contains((Panel)host.FindName("SectionList"), key) ? Visibility.Visible : Visibility.Collapsed;

            Title = key + " — Claude UI";
            ContentScroll.Padding = new Thickness(48, 8, 48, 24);
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
