using System;
using System.Collections.Generic;
using System.IO;
using System.Windows;
using System.Windows.Automation;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;

namespace UIComponents.Wpf
{
    public partial class App : Application
    {
        // Off-screen capture for visual review:
        //   UIComponents.Wpf.exe ["Section title"] --capture <AutomationId>[!<ClickId>...][@<FocusAutomationId>]=<out.png> ...
        //   !<ClickId> clicks a button (or toggles a checkbox/toggle) first, in order; e.g. InputGrid!InputGrid_row1=sel.png
        // Renders each element with RenderTargetBitmap at 96 DPI in a 1400x1100 window, then exits.
        // Exit code 0 = all captured, 2 = an AutomationId was not found. Works on a hidden desktop
        // (wpf/tests/run-hidden.ps1), so nothing shows on the user's screen.
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);
            int at = Array.IndexOf(e.Args, "--capture");
            if (at < 0) return;
            List<string> specs = new List<string>();
            for (int i = at + 1; i < e.Args.Length; i++) specs.Add(e.Args[i]);
            Dispatcher.BeginInvoke(DispatcherPriority.ApplicationIdle, new Action(delegate { RunCaptures(specs); }));
        }

        private void RunCaptures(List<string> specs)
        {
            Window win = MainWindow;
            win.Width = 1400;
            win.Height = 1100;
            win.UpdateLayout();
            int exit = 0;
            foreach (string spec in specs)
            {
                int eq = spec.IndexOf('=');
                string target = spec.Substring(0, eq);
                string outPath = Path.GetFullPath(spec.Substring(eq + 1));
                string[] parts = target.Split('@');
                string[] clicks = parts[0].Split('!');
                FrameworkElement element = FindById(win, clicks[0]);
                if (element == null) { exit = 2; File.WriteAllText(outPath + ".error.txt", "AutomationId not found: " + clicks[0]); continue; }
                string missing = null;
                for (int i = 1; i < clicks.Length; i++)
                {
                    ButtonBase button = FindById(win, clicks[i]) as ButtonBase;
                    if (button == null) { missing = clicks[i]; break; }
                    ToggleButton toggle = button as ToggleButton;
                    if (toggle != null) toggle.IsChecked = toggle.IsChecked != true;
                    else button.RaiseEvent(new RoutedEventArgs(ButtonBase.ClickEvent));
                    Dispatcher.Invoke(DispatcherPriority.ApplicationIdle, new Action(delegate { }));
                }
                if (missing != null) { exit = 2; File.WriteAllText(outPath + ".error.txt", "Click target not found or not a button: " + missing); continue; }
                if (parts.Length > 1)
                {
                    FrameworkElement focus = FindById(win, parts[1]);
                    if (focus == null) { exit = 2; File.WriteAllText(outPath + ".error.txt", "Focus AutomationId not found: " + parts[1]); continue; }
                    win.Activate();
                    Keyboard.Focus(focus);
                }
                // Let triggers and Paint() settle before rendering.
                Dispatcher.Invoke(DispatcherPriority.ApplicationIdle, new Action(delegate { }));
                Save(element, outPath);
            }
            Shutdown(exit);
        }

        private static FrameworkElement FindById(DependencyObject node, string id)
        {
            FrameworkElement fe = node as FrameworkElement;
            // AutomationId first; x:Name as fallback (e.g. "Sections" = the whole content column in standalone mode).
            if (fe != null && (AutomationProperties.GetAutomationId(fe) == id || fe.Name == id)) return fe;
            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(node); i++)
            {
                FrameworkElement found = FindById(VisualTreeHelper.GetChild(node, i), id);
                if (found != null) return found;
            }
            return null;
        }

        // Render the whole window content, then crop to the element: the same pixels a screen capture shows.
        private void Save(FrameworkElement element, string outPath)
        {
            element.BringIntoView();
            Dispatcher.Invoke(DispatcherPriority.ApplicationIdle, new Action(delegate { }));
            FrameworkElement root = (FrameworkElement)MainWindow.Content;
            int rw = (int)Math.Ceiling(root.ActualWidth);
            int rh = (int)Math.Ceiling(root.ActualHeight);
            DrawingVisual bg = new DrawingVisual();
            using (DrawingContext dc = bg.RenderOpen())
                dc.DrawRectangle((Brush)FindResource("Bg"), null, new Rect(0, 0, rw, rh));   // page background
            RenderTargetBitmap full = new RenderTargetBitmap(rw, rh, 96, 96, PixelFormats.Pbgra32);
            full.Render(bg);
            full.Render(root);

            Rect r = element.TransformToAncestor(root).TransformBounds(new Rect(0, 0, element.ActualWidth, element.ActualHeight));
            int x = Math.Max(0, (int)Math.Round(r.X));
            int y = Math.Max(0, (int)Math.Round(r.Y));
            Int32Rect crop = new Int32Rect(x, y, Math.Min((int)Math.Round(r.Width), rw - x), Math.Min((int)Math.Round(r.Height), rh - y));
            PngBitmapEncoder png = new PngBitmapEncoder();
            png.Frames.Add(BitmapFrame.Create(new CroppedBitmap(full, crop)));
            using (FileStream fs = File.Create(outPath)) png.Save(fs);
        }
    }
}
