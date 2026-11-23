using System;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;
using Microsoft.Win32;

namespace UIComponents.Wpf
{
    public enum ThemeMode { System, Light, Dark }

    // Light/dark/system theming. Light = the resource graph as merged in App.xaml; dark = Themes/Tokens.Dark.xaml
    // added as the LAST merged dictionary of Application.Resources (highest precedence), so every
    // {DynamicResource Key} / SetResourceReference colour switches live. Persistence mirrors web localStorage['ui-theme'].
    public static class ThemeManager
    {
        private const string ValueName = "ui-theme";
        private static string _storageKey = @"Software\UIComponents.Wpf";   // null = never read/write
        private static ThemeMode _mode = ThemeMode.System;
        private static bool _isDark;
        private static bool _initialized;
        private static bool _override;                  // env UI_THEME active: never read or write storage
        private static ResourceDictionary _dark;        // loaded once, cached

        public static string StorageKey { get { return _storageKey; } set { _storageKey = value; } }
        public static ThemeMode Mode { get { return _mode; } }      // user choice
        public static bool IsDark { get { return _isDark; } }       // resolved
        public static event EventHandler ThemeChanged;              // after every apply (also System-driven)

        [DllImport("dwmapi.dll")]
        private static extern int DwmSetWindowAttribute(IntPtr hwnd, int attr, ref int value, int size);

        // Call once in App.OnStartup BEFORE any window is created; idempotent.
        public static void Initialize()
        {
            if (_initialized) return;
            _initialized = true;

            string env = Environment.GetEnvironmentVariable("UI_THEME");
            ThemeMode envMode;
            if (TryParseMode(env, out envMode))
            {
                _override = true;
                _mode = envMode;
            }
            else
            {
                string stored = ReadStored();
                _mode = string.Equals(stored, "dark", StringComparison.OrdinalIgnoreCase) ? ThemeMode.Dark
                      : string.Equals(stored, "light", StringComparison.OrdinalIgnoreCase) ? ThemeMode.Light
                      : ThemeMode.System;
            }

            Apply();

            SystemEvents.UserPreferenceChanged += OnUserPreferenceChanged;
            Application.Current.Exit += delegate { SystemEvents.UserPreferenceChanged -= OnUserPreferenceChanged; };
            EventManager.RegisterClassHandler(typeof(Window), FrameworkElement.LoadedEvent,
                new RoutedEventHandler(delegate(object sender, RoutedEventArgs e) { ApplyTitleBar(sender as Window, _isDark); }));
        }

        // Apply + persist (unless the env override is active or StorageKey is null) + ThemeChanged.
        public static void SetTheme(ThemeMode mode)
        {
            _mode = mode;
            Apply();
            if (!_override) WriteStored(mode);
        }

        private static bool TryParseMode(string s, out ThemeMode mode)
        {
            mode = ThemeMode.System;
            if (string.Equals(s, "light", StringComparison.OrdinalIgnoreCase)) mode = ThemeMode.Light;
            else if (string.Equals(s, "dark", StringComparison.OrdinalIgnoreCase)) mode = ThemeMode.Dark;
            else if (string.Equals(s, "system", StringComparison.OrdinalIgnoreCase)) mode = ThemeMode.System;
            else return false;
            return true;
        }

        private static void OnUserPreferenceChanged(object sender, UserPreferenceChangedEventArgs e)
        {
            // Raised on a SystemEvents thread: hop to the UI thread.
            if (e.Category != UserPreferenceCategory.General || _mode != ThemeMode.System) return;
            Application app = Application.Current;
            if (app != null) app.Dispatcher.BeginInvoke(new Action(Apply));
        }

        private static bool SystemIsDark()
        {
            try
            {
                using (RegistryKey key = Registry.CurrentUser.OpenSubKey(@"Software\Microsoft\Windows\CurrentVersion\Themes\Personalize"))
                {
                    object v = key == null ? null : key.GetValue("AppsUseLightTheme");
                    return v is int && (int)v == 0;   // missing key or value -> light
                }
            }
            catch (Exception) { return false; }
        }

        private static string ReadStored()
        {
            if (_storageKey == null) return null;
            try
            {
                using (RegistryKey key = Registry.CurrentUser.OpenSubKey(_storageKey))
                    return key == null ? null : key.GetValue(ValueName) as string;
            }
            catch (Exception) { return null; }
        }

        private static void WriteStored(ThemeMode mode)
        {
            if (_storageKey == null) return;
            try
            {
                using (RegistryKey key = Registry.CurrentUser.CreateSubKey(_storageKey))
                {
                    if (key == null) return;
                    if (mode == ThemeMode.System) key.DeleteValue(ValueName, false);   // like web removeItem
                    else key.SetValue(ValueName, mode == ThemeMode.Dark ? "dark" : "light", RegistryValueKind.String);
                }
            }
            catch (Exception) { }   // persistence is best effort
        }

        private static ResourceDictionary LoadDark(ResourceDictionary merged)
        {
            if (_dark != null) return _dark;
            const string suffix = "Tokens.xaml";
            foreach (ResourceDictionary d in merged.MergedDictionaries)
            {
                string src = d.Source == null ? null : d.Source.OriginalString;
                if (src != null && src.EndsWith(suffix, StringComparison.OrdinalIgnoreCase))
                {
                    string sibling = src.Substring(0, src.Length - suffix.Length) + "Tokens.Dark.xaml";
                    _dark = new ResourceDictionary { Source = new Uri(sibling, UriKind.RelativeOrAbsolute) };
                    return _dark;
                }
            }
            throw new InvalidOperationException("ThemeManager: merge Themes/Tokens.xaml into Application.Resources first");
        }

        private static void Apply()
        {
            Application app = Application.Current;
            if (app == null) return;
            bool dark = _mode == ThemeMode.Dark || (_mode == ThemeMode.System && SystemIsDark());
            ResourceDictionary res = app.Resources;
            if (_dark != null && res.MergedDictionaries.Contains(_dark)) res.MergedDictionaries.Remove(_dark);
            if (dark) res.MergedDictionaries.Add(LoadDark(res));   // last = highest precedence
            _isDark = dark;
            foreach (Window w in app.Windows) ApplyTitleBar(w, dark);
            EventHandler handler = ThemeChanged;
            if (handler != null) handler(null, EventArgs.Empty);
        }

        private static void ApplyTitleBar(Window w, bool dark)
        {
            if (w == null) return;
            IntPtr hwnd = new WindowInteropHelper(w).Handle;
            if (hwnd == IntPtr.Zero) return;
            int on = dark ? 1 : 0;
            try
            {
                if (DwmSetWindowAttribute(hwnd, 20, ref on, 4) != 0) DwmSetWindowAttribute(hwnd, 19, ref on, 4);
            }
            catch (Exception) { }   // older Windows / no dwmapi: silent no-op
        }
    }
}
