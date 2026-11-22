using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows.Automation;
using System.Windows.Forms;

namespace UIComponents.Wpf.Tests
{
    /// <summary>
    /// UI Automation tests: drives the running WPF app by finding controls through
    /// AutomationElement and invoking patterns. No cursor movement, no coordinates.
    /// </summary>
    internal static partial class UIAutomationTests
    {
        private static int _passed;
        private static int _failed;

        private static void Check(string name, bool condition, string detail = "")
        {
            if (condition) { _passed++; Console.WriteLine("PASS  " + name); }
            else { _failed++; Console.WriteLine("FAIL  " + name + (detail.Length > 0 ? " — " + detail : "")); }
        }

        // Poll instead of a fixed sleep: returns as soon as the condition holds.
        private static bool WaitFor(Func<bool> condition, int timeoutMs)
        {
            DateTime deadline = DateTime.UtcNow.AddMilliseconds(timeoutMs);
            while (DateTime.UtcNow < deadline)
            {
                if (condition()) return true;
                Thread.Sleep(25);
            }
            return condition();
        }

        private static AutomationElement Find(AutomationElement root, string automationId)
        {
            var condition = new PropertyCondition(AutomationElement.AutomationIdProperty, automationId);
            Drv.FindStart();
            try { return root.FindFirst(TreeScope.Descendants, condition); } finally { Drv.FindStop(); }
        }

        private static AutomationElement FindByName(AutomationElement root, string name)
        {
            var condition = new PropertyCondition(AutomationElement.NameProperty, name);
            Drv.FindStart();
            try { return root.FindFirst(TreeScope.Descendants, condition); } finally { Drv.FindStop(); }
        }

        private static int CountByName(AutomationElement root, string name)
        {
            var condition = new PropertyCondition(AutomationElement.NameProperty, name);
            Drv.FindStart();
            try { return root.FindAll(TreeScope.Descendants, condition).Count; } finally { Drv.FindStop(); }
        }

        private static AutomationElement FindActiveComboOption(AutomationElement root, string prefix)
        {
            var buttons = root.FindAll(TreeScope.Descendants,
                new PropertyCondition(AutomationElement.ControlTypeProperty, ControlType.Button));
            foreach (AutomationElement button in buttons)
            {
                string id = button.Current.AutomationId;
                string status = button.Current.ItemStatus;
                if (id != null && id.StartsWith(prefix) && status != null && status.Contains("active")) return button;
            }
            return null;
        }

        private static void CountComboOptions(AutomationElement root, string prefix,
            out int total, out int onscreen, out int offscreen)
        {
            total = 0; onscreen = 0; offscreen = 0;
            var buttons = root.FindAll(TreeScope.Descendants, Condition.TrueCondition);
            foreach (AutomationElement button in buttons)
            {
                string id = button.Current.AutomationId;
                if (id == null || !id.StartsWith(prefix)) continue;
                total++;
                if (button.Current.IsOffscreen) offscreen++; else onscreen++;
            }
        }

        private static void Invoke(AutomationElement element)
        {
            Drv.Do("invoke", element, null, delegate { ((InvokePattern)element.GetCurrentPattern(InvokePattern.Pattern)).Invoke(); });
        }

        private static string GetValue(AutomationElement element)
        {
            try
            {
                var pattern = (ValuePattern)element.GetCurrentPattern(ValuePattern.Pattern);
                return pattern.Current.Value;
            }
            catch (InvalidOperationException)
            {
                try
                {
                    var text = (TextPattern)element.GetCurrentPattern(TextPattern.Pattern);
                    return text.DocumentRange.GetText(-1);
                }
                catch (InvalidOperationException)
                {
                    return element.Current.Name;
                }
            }
        }

        private static void SetValue(AutomationElement element, string value)
        {
            Drv.Do("setvalue", element, value, delegate { ((ValuePattern)element.GetCurrentPattern(ValuePattern.Pattern)).SetValue(value); });
        }

        private static double CellLeft(AutomationElement element)
        {
            object rect = element.GetCurrentPropertyValue(AutomationElement.BoundingRectangleProperty);
            return (double)rect.GetType().GetProperty("X").GetValue(rect, null);
        }

        private static string FirstGridCell(AutomationElement root, string automationId)
        {
            var grid = Find(root, automationId);
            if (grid == null) return null;
            var rows = grid.FindAll(TreeScope.Descendants,
                new PropertyCondition(AutomationElement.ControlTypeProperty, ControlType.DataItem));
            if (rows.Count == 0) return null;
            var texts = rows[0].FindAll(TreeScope.Descendants,
                new PropertyCondition(AutomationElement.ControlTypeProperty, ControlType.Text));
            return texts.Count > 0 ? GetValue(texts[0]) : GetValue(rows[0]);
        }

        [DllImport("user32.dll")]
        private static extern bool PostMessage(IntPtr hWnd, uint msg, IntPtr wParam, IntPtr lParam);

        [DllImport("user32.dll")]
        private static extern bool SetForegroundWindow(IntPtr hWnd);

        // Phase 0 probe: a key as a window message only. Never SendKeys/SendInput/keybd_event —
        // those type into the user's foreground window.
        private static void PostKey(IntPtr hwnd, int vk)
        {
            PostMessage(hwnd, 0x100, new IntPtr(vk), IntPtr.Zero);                              // WM_KEYDOWN
            PostMessage(hwnd, 0x101, new IntPtr(vk), new IntPtr(unchecked((int)0xC0000001)));  // WM_KEYUP
        }

        private static double RectPart(AutomationElement element, string name)
        {
            object rect = element.GetCurrentPropertyValue(AutomationElement.BoundingRectangleProperty);
            return (double)rect.GetType().GetProperty(name).GetValue(rect, null);
        }

        // A mouse message on the hidden desktop: WPF turns it into PreviewMouseDown without
        // moving the real cursor or focusing the clicked (non-focusable) element.
        private static void PostMouseClick(IntPtr hwnd, double screenX, double screenY, double windowX, double windowY)
        {
            int x = (int)Math.Round(screenX - windowX);
            int y = (int)Math.Round(screenY - windowY);
            IntPtr point = new IntPtr((y << 16) | (x & 0xFFFF));
            PostMessage(hwnd, 0x201, IntPtr.Zero, point); // WM_LBUTTONDOWN
            PostMessage(hwnd, 0x202, IntPtr.Zero, point); // WM_LBUTTONUP
        }

        // Clipboard is shared with the user's session: snapshot every format on STA, and
        // skip the restore entirely if the snapshot itself failed.
        private static bool SnapshotClipboard(out IDataObject saved)
        {
            bool ok = false;
            IDataObject snapshot = null;
            Thread thread = new Thread((ThreadStart)delegate
            {
                try
                {
                    IDataObject data = Clipboard.GetDataObject();
                    ok = true; // read succeeded; null means genuinely empty
                    if (data != null)
                    {
                        DataObject copy = new DataObject();
                        foreach (string format in data.GetFormats(false))
                        {
                            object value = data.GetData(format);
                            if (value != null) copy.SetData(format, value);
                        }
                        snapshot = copy;
                    }
                }
                catch (Exception) { ok = false; snapshot = null; }
            });
            thread.SetApartmentState(ApartmentState.STA);
            thread.Start();
            thread.Join();
            saved = snapshot;
            return ok;
        }

        private static void RestoreClipboard(IDataObject saved)
        {
            Thread thread = new Thread((ThreadStart)delegate
            {
                try
                {
                    if (saved == null) Clipboard.Clear();
                    else Clipboard.SetDataObject(saved, true);
                }
                catch (Exception) { }
            });
            thread.SetApartmentState(ApartmentState.STA);
            thread.Start();
            thread.Join();
        }

        private static string ReadClipboardText()
        {
            string text = null;
            Thread thread = new Thread((ThreadStart)delegate
            {
                try { text = Clipboard.ContainsText() ? Clipboard.GetText() : null; }
                catch (Exception) { }
            });
            thread.SetApartmentState(ApartmentState.STA);
            thread.Start();
            thread.Join();
            return text;
        }

        // Area files (Tests.<Area>.cs) implement these; a missing one is dropped by the compiler and reported by the runner.
        static partial void RunNav(AutomationElement root, ref bool ran);
        static partial void RunForms(AutomationElement root, ref bool ran);
        static partial void RunTables(AutomationElement root, ref bool ran);
        static partial void RunDisplay(AutomationElement root, ref bool ran);
        static partial void RunOverlays(AutomationElement root, ref bool ran);
        static partial void RunDropdown(AutomationElement root, ref bool ran);
        static partial void RunPickers(AutomationElement root, ref bool ran);
        static partial void RunGrid(AutomationElement root, ref bool ran);
        static partial void RunToasts(AutomationElement root, ref bool ran);

        private static int Main(string[] args)
        {
            Console.WriteLine("WPF UI Automation tests");
            Console.WriteLine("Start UIComponents.Wpf.exe first, then run this. Pass its PID to target your own instance.");
            Console.WriteLine();
            if (args.Length > 0 && args[args.Length - 1] == "--only") { Console.WriteLine("FAIL  --only needs an area list, e.g. --only nav,grid"); return 1; }

            AutomationElement root;
            try
            {
                // With a PID argument, only that process's window is tested — parallel agents don't drive each other's app.
                Condition target = args.Length > 0 && args[0] != "--only"
                    ? (Condition)new PropertyCondition(AutomationElement.ProcessIdProperty, int.Parse(args[0]))
                    : new PropertyCondition(AutomationElement.NameProperty, "Claude UI — WPF components");
                var windows = AutomationElement.RootElement.FindAll(TreeScope.Children, target);
                if (windows.Count == 0) { Console.WriteLine("FAIL  window not found — start the app first"); return 1; }
                root = windows[0];
            }
            catch (Exception ex)
            {
                Console.WriteLine("FAIL  UI Automation unavailable — " + ex.Message);
                return 1;
            }

            Check("window found", root != null);
            Drv.Init(new IntPtr(root.Current.NativeWindowHandle));

            // Areas run in this fixed order (each area's first appearance in the old Main). --only a,b runs a subset in this order.
            string[] order = { "nav", "forms", "tables", "display", "overlays", "dropdown", "pickers", "grid", "toasts" };
            var compiled = new List<string>();
            foreach (string name in order)
            {
                if (typeof(UIAutomationTests).GetMethod("Run" + char.ToUpperInvariant(name[0]) + name.Substring(1), System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static) != null) compiled.Add(name);
            }
            Console.WriteLine("AREAS compiled: " + string.Join(", ", compiled.ToArray()));
            string[] only = null;
            int onlyAt = Array.IndexOf(args, "--only");
            if (onlyAt >= 0 && onlyAt + 1 < args.Length)
            {
                only = args[onlyAt + 1].ToLowerInvariant().Split(',');
                for (int i = 0; i < only.Length; i++)
                {
                    only[i] = only[i].Trim();
                    if (Array.IndexOf(order, only[i]) < 0) { Console.WriteLine("FAIL  unknown area " + only[i]); return 1; }
                }
            }
            foreach (string name in order)
            {
                if (only != null && Array.IndexOf(only, name) < 0) continue;
                Console.WriteLine("AREA  " + name);
                var watch = System.Diagnostics.Stopwatch.StartNew();
                bool ran = false;
                try
                {
                    switch (name)
                    {
                        case "nav": RunNav(root, ref ran); break;
                        case "forms": RunForms(root, ref ran); break;
                        case "tables": RunTables(root, ref ran); break;
                        case "display": RunDisplay(root, ref ran); break;
                        case "overlays": RunOverlays(root, ref ran); break;
                        case "dropdown": RunDropdown(root, ref ran); break;
                        case "pickers": RunPickers(root, ref ran); break;
                        case "grid": RunGrid(root, ref ran); break;
                        case "toasts": RunToasts(root, ref ran); break;
                    }
                }
                catch (Exception ex)
                {
                    _failed++;
                    Console.WriteLine("FAIL  area " + name + " threw " + ex.GetType().Name + ": " + ex.Message);
                }
                if (!ran) { _failed++; Console.WriteLine("FAIL  area " + name + " not compiled into this exe"); }
                Console.WriteLine("AREA  " + name + " done in " + (watch.ElapsedMilliseconds / 1000.0).ToString("0.0") + " s");
            }
            if (_passed + _failed == 1) { Console.WriteLine("FAIL  no checks ran"); _failed++; }

            Console.WriteLine();
            Drv.Report();
            Console.WriteLine("RESULT  passed=" + _passed + "  failed=" + _failed);
            return _failed == 0 ? 0 : 1;
        }
    }

    // In-process driver client (see App.xaml.cs DriverHook): every UIA action (Invoke, SetValue, Toggle, Select,
    // Expand/Collapse, SetFocus) costs a flat ~2 s on this machine, so they are sent as WM_COPYDATA and run
    // in-process; reads stay on UIA. Anything the app cannot resolve falls back to the real UIA call.
    internal static class Drv
    {
        [StructLayout(LayoutKind.Sequential)]
        private struct COPYDATA { public IntPtr dwData; public int cbData; public IntPtr lpData; }
        [DllImport("user32.dll", CharSet = CharSet.Unicode)]
        private static extern IntPtr SendMessageTimeout(IntPtr hWnd, uint msg, IntPtr wParam, ref COPYDATA lParam,
            uint flags, uint timeoutMs, out IntPtr result);

        private static IntPtr _hwnd;
        private static bool _enabled;
        private static int _fast, _fallback;
        private static readonly System.Collections.Generic.Dictionary<string, int> _fallbackIds = new System.Collections.Generic.Dictionary<string, int>();

        public static void Init(IntPtr hwnd)
        {
            _hwnd = hwnd;
            // The hook is installed a moment after the window shows (driver-review #14): retry ping for up to 2 s.
            _enabled = false;
            if (Environment.GetEnvironmentVariable("UITEST_NODRIVE") != "1")
            {
                DateTime deadline = DateTime.UtcNow.AddSeconds(2);
                for (;;)
                {
                    try { _enabled = Send("ping", "", null) == 1; }
                    catch (InvalidOperationException) { }
                    if (_enabled || DateTime.UtcNow >= deadline) break;
                    Thread.Sleep(100);
                }
            }
            Console.WriteLine(_enabled ? "DRIVER  in-process actions enabled" : "DRIVER  off (set UITEST_DRIVE=1 for the app); real UIA actions, ~2 s each");
        }

        private static readonly System.Diagnostics.Stopwatch _findWatch = new System.Diagnostics.Stopwatch();
        private static int _finds;
        public static void FindStart() { _finds++; _findWatch.Start(); }
        public static void FindStop() { _findWatch.Stop(); }

        public static void Report()
        {
            Console.WriteLine("PROFILE finds=" + _finds + " findMs=" + _findWatch.ElapsedMilliseconds);
            if (!_enabled) return;
            Console.WriteLine("DRIVER  fast=" + _fast + " fallback=" + _fallback);
            foreach (var kv in _fallbackIds) Console.WriteLine("DRIVER  fallback " + kv.Key + " x" + kv.Value);
        }

        private static int Send(string verb, string id, string arg)
        {
            string text = verb + "\t" + id + (arg == null ? "" : "\t" + arg);
            IntPtr buf = Marshal.StringToHGlobalUni(text);
            try
            {
                COPYDATA cd = new COPYDATA();
                cd.dwData = IntPtr.Zero; cd.cbData = text.Length * 2; cd.lpData = buf;
                IntPtr result;
                if (SendMessageTimeout(_hwnd, 0x4A, IntPtr.Zero, ref cd, 2 /*SMTO_ABORTIFHUNG*/, 15000, out result) == IntPtr.Zero)
                    throw new InvalidOperationException("driver: no reply for '" + verb + " " + id + "' (never re-run through UIA: the action may have happened)");
                return result.ToInt32();
            }
            finally { Marshal.FreeHGlobal(buf); }
        }

        public static void Do(string verb, AutomationElement el, string arg, Action fallback)
        {
            if (_enabled)
            {
                string id = null;
                try { id = el.Current.AutomationId; } catch (Exception) { }
                int r = string.IsNullOrEmpty(id) ? 2 : Send(verb, id, arg);
                if (r == 1) { _fast++; return; }
                if (r == 3) throw new InvalidOperationException("driver: app handler threw for '" + verb + " " + id + "' (see DRIVER error on stderr)");
                string key = verb + ":" + (string.IsNullOrEmpty(id) ? "<no id>" : id);
                int n; _fallbackIds.TryGetValue(key, out n); _fallbackIds[key] = n + 1;
                _fallback++;
            }
            fallback();
        }

        public static void Focus(AutomationElement el) { Do("focus", el, null, delegate { el.SetFocus(); }); }

        public static bool Enabled { get { return _enabled; } }

        // A real press at a screen point, hit-tested in the app (App.xaml.cs ClickScreenPoint). Returns 1 = an element
        // was pressed, 4 = nothing was hit (the press fell through to the window root), 2 = not delivered, -1 = driver off.
        public static int Click(int x, int y)
        {
            if (!_enabled) return -1;
            int r = Send("click", "", x + "," + y);
            if (r == 3) throw new InvalidOperationException("driver: app handler threw for 'click " + x + "," + y + "' (see DRIVER error on stderr)");
            if (r == 1 || r == 4) _fast++;
            return r;
        }

        // Press at (dx, dy) from el's top-left corner (screen coordinates; the test window runs at 100% scale).
        public static int ClickAt(AutomationElement el, double dx, double dy)
        {
            object rect = el.GetCurrentPropertyValue(AutomationElement.BoundingRectangleProperty);
            double x = (double)rect.GetType().GetProperty("X").GetValue(rect, null);
            double y = (double)rect.GetType().GetProperty("Y").GetValue(rect, null);
            return Click((int)Math.Round(x + dx), (int)Math.Round(y + dy));
        }
    }

    internal sealed class FastToggle
    {
        private readonly AutomationElement _el; private readonly TogglePattern _p;
        private FastToggle(AutomationElement el) { _el = el; _p = (TogglePattern)el.GetCurrentPattern(TogglePattern.Pattern); }
        public static FastToggle Of(AutomationElement el) { return new FastToggle(el); }
        public TogglePattern.TogglePatternInformation Current { get { return _p.Current; } }
        public void Toggle() { Drv.Do("toggle", _el, null, delegate { _p.Toggle(); }); }
    }

    internal sealed class FastSelectionItem
    {
        private readonly AutomationElement _el; private readonly SelectionItemPattern _p;
        private FastSelectionItem(AutomationElement el) { _el = el; _p = (SelectionItemPattern)el.GetCurrentPattern(SelectionItemPattern.Pattern); }
        public static FastSelectionItem Of(AutomationElement el) { return new FastSelectionItem(el); }
        public SelectionItemPattern.SelectionItemPatternInformation Current { get { return _p.Current; } }
        public void Select() { Drv.Do("select", _el, null, delegate { _p.Select(); }); }
    }

    internal sealed class FastExpandCollapse
    {
        private readonly AutomationElement _el; private readonly ExpandCollapsePattern _p;
        private FastExpandCollapse(AutomationElement el) { _el = el; _p = (ExpandCollapsePattern)el.GetCurrentPattern(ExpandCollapsePattern.Pattern); }
        public static FastExpandCollapse Of(AutomationElement el) { return new FastExpandCollapse(el); }
        public ExpandCollapsePattern.ExpandCollapsePatternInformation Current { get { return _p.Current; } }
        public void Expand() { Drv.Do("expand", _el, null, delegate { _p.Expand(); }); }
        public void Collapse() { Drv.Do("collapse", _el, null, delegate { _p.Collapse(); }); }
    }
}
