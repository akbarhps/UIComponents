using System;
using System.Collections.Generic;
using System.IO;
using System.Windows;
using System.Windows.Automation;
using System.Windows.Controls;
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
        //   UIComponents.Wpf.exe ["Section title"] --capture <Id>[!<ClickId>|~<Key>|^<text>]...[@<FocusId>]=<out.png> ...
        //   Every step is introduced by '!' and runs left to right. !<ClickId> clicks a button (or toggles a
        //   checkbox/toggle), e.g. InputGrid!InputGrid_row1=sel.png; a text/key step is !^eng or !~Down.
        //   ~Down / ~Up / ~Enter / ~Escape raise PreviewKeyDown + KeyDown on Keyboard.FocusedElement.
        //   ^text sets TextBox.Text on the focused element.
        // Renders each element with RenderTargetBitmap at 96 DPI in a 1400x1100 window, then exits.
        // Exit code 0 = all captured, 2 = an AutomationId was not found. Works on a hidden desktop
        // (wpf/tests/run-hidden.ps1), so nothing shows on the user's screen.
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);
            // Test driver (wpf/tests/run-hidden.ps1 sets UITEST_DRIVE=1): UIA actions cost a flat 2 s on this
            // machine, so the tests send the same pattern calls in-process through WM_COPYDATA instead.
            if (Environment.GetEnvironmentVariable("UITEST_DRIVE") == "1")
                Dispatcher.BeginInvoke(DispatcherPriority.Loaded, new Action(delegate { InstallDriver(MainWindow); }));
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
            win.Activate(); // hidden desktop only: lets ~Key reach Keyboard.FocusedElement
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
                    if (clicks[i].Length > 1 && clicks[i][0] == '~')
                    {
                        if (!SendCaptureKey(win, clicks[i].Substring(1))) { missing = clicks[i]; break; }
                    }
                    else if (clicks[i].Length > 1 && clicks[i][0] == '^')
                    {
                        if (!SetFocusedText(clicks[i].Substring(1))) { missing = clicks[i]; break; }
                    }
                    else
                    {
                        ButtonBase button = FindById(win, clicks[i]) as ButtonBase;
                        if (button == null) { missing = clicks[i]; break; }
                        ToggleButton toggle = button as ToggleButton;
                        if (toggle != null) toggle.IsChecked = toggle.IsChecked != true;
                        else button.RaiseEvent(new RoutedEventArgs(ButtonBase.ClickEvent));
                    }
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

        // WM_COPYDATA payload "verb<TAB>automationId<TAB>arg". Result 1 = done, 2 = not found / duplicate id /
        // unsupported (the test then falls back to the real UIA call). Executes the same provider calls UIA would.
        private static void InstallDriver(Window win)
        {
            System.Windows.Interop.HwndSource source = win == null ? null : PresentationSource.FromVisual(win) as System.Windows.Interop.HwndSource;
            if (source != null) source.AddHook(DriverHook);
        }

        private static IntPtr DriverHook(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
        {
            if (msg != 0x4A) return IntPtr.Zero; // WM_COPYDATA
            handled = true;
            try
            {
                int cb = System.Runtime.InteropServices.Marshal.ReadInt32(lParam, IntPtr.Size);
                IntPtr data = System.Runtime.InteropServices.Marshal.ReadIntPtr(lParam, IntPtr.Size + 4 + (IntPtr.Size == 8 ? 4 : 0));
                string[] parts = System.Runtime.InteropServices.Marshal.PtrToStringUni(data, cb / 2).Split('	');
                if (parts[0] == "ping") return new IntPtr(1);
                Window win = Application.Current.MainWindow;
                List<FrameworkElement> found = new List<FrameworkElement>();
                foreach (PresentationSource ps in PresentationSource.CurrentSources)   // main window + popups
                {
                    DependencyObject rootVisual = ps.RootVisual as DependencyObject;
                    if (rootVisual != null) FindAllById(rootVisual, parts[1], found);
                }
                if (found.Count != 1) return new IntPtr(2);
                System.Windows.Automation.Peers.AutomationPeer peer = System.Windows.Automation.Peers.UIElementAutomationPeer.CreatePeerForElement(found[0]);
                if (peer == null) return new IntPtr(2);
                System.Windows.Automation.Provider.IToggleProvider toggle;
                switch (parts[0])
                {
                    case "invoke":
                        System.Windows.Automation.Provider.IInvokeProvider invoke = peer.GetPattern(System.Windows.Automation.Peers.PatternInterface.Invoke) as System.Windows.Automation.Provider.IInvokeProvider;
                        if (invoke == null) return new IntPtr(2);
                        invoke.Invoke(); return new IntPtr(1);
                    case "toggle":
                        toggle = peer.GetPattern(System.Windows.Automation.Peers.PatternInterface.Toggle) as System.Windows.Automation.Provider.IToggleProvider;
                        if (toggle == null) return new IntPtr(2);
                        toggle.Toggle(); return new IntPtr(1);
                    case "setvalue":
                        System.Windows.Automation.Provider.IValueProvider value = peer.GetPattern(System.Windows.Automation.Peers.PatternInterface.Value) as System.Windows.Automation.Provider.IValueProvider;
                        if (value == null) return new IntPtr(2);
                        value.SetValue(parts.Length > 2 ? parts[2] : ""); return new IntPtr(1);
                    case "select":
                        System.Windows.Automation.Provider.ISelectionItemProvider select = peer.GetPattern(System.Windows.Automation.Peers.PatternInterface.SelectionItem) as System.Windows.Automation.Provider.ISelectionItemProvider;
                        if (select == null) return new IntPtr(2);
                        select.Select(); return new IntPtr(1);
                    case "expand":
                    case "collapse":
                        System.Windows.Automation.Provider.IExpandCollapseProvider expand = peer.GetPattern(System.Windows.Automation.Peers.PatternInterface.ExpandCollapse) as System.Windows.Automation.Provider.IExpandCollapseProvider;
                        if (expand == null) return new IntPtr(2);
                        if (parts[0] == "expand") expand.Expand(); else expand.Collapse();
                        return new IntPtr(1);
                    case "focus":
                        if (!found[0].Focusable) return new IntPtr(2);
                        win.Activate();
                        return new IntPtr(found[0].Focus() ? 1 : 2);
                }
            }
            catch (System.Windows.Automation.ElementNotEnabledException) { return new IntPtr(2); }
            catch (Exception ex) { Console.Error.WriteLine("DRIVER error: " + ex); return new IntPtr(3); }
            return new IntPtr(2);
        }

        private static void FindAllById(DependencyObject node, string id, List<FrameworkElement> into)
        {
            FrameworkElement fe = node as FrameworkElement;
            // Same id UIA reports: AutomationId, else x:Name.
            if (fe != null)
            {
                string own = AutomationProperties.GetAutomationId(fe);
                if (own == id || (string.IsNullOrEmpty(own) && fe.Name == id)) into.Add(fe);
            }
            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(node); i++) FindAllById(VisualTreeHelper.GetChild(node, i), id, into);
        }

        private static bool SendCaptureKey(Window win, string name)
        {
            Key key;
            switch (name)
            {
                case "Down": key = Key.Down; break;
                case "Up": key = Key.Up; break;
                case "Enter": key = Key.Return; break;
                case "Escape": key = Key.Escape; break;
                case "Left": key = Key.Left; break;
                case "Right": key = Key.Right; break;
                case "Home": key = Key.Home; break;
                case "End": key = Key.End; break;
                default: return false;
            }
            IInputElement focused = Keyboard.FocusedElement;
            PresentationSource source = focused == null ? null : PresentationSource.FromVisual(win);
            if (source == null) return false;
            try
            {
                KeyEventArgs preview = new KeyEventArgs(Keyboard.PrimaryDevice, source, 0, key);
                preview.RoutedEvent = Keyboard.PreviewKeyDownEvent;
                focused.RaiseEvent(preview);
                KeyEventArgs bubble = new KeyEventArgs(Keyboard.PrimaryDevice, source, 0, key);
                bubble.RoutedEvent = Keyboard.KeyDownEvent;
                focused.RaiseEvent(bubble);
                return true;
            }
            catch (Exception) { return false; }
        }

        private static bool SetFocusedText(string text)
        {
            TextBox box = Keyboard.FocusedElement as TextBox;
            if (box == null) return false;
            box.Text = text;
            return true;
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
