# Final review: 195561f WPF dark mode (plan/wpf/09-dark-mode.md)

Counts: high 0, med 3, low 6.
Checks run: `node plan/wpf/theme-lint.mjs` gives C0-C5 violations=0 and RESULT theme-lint failed=0. Suite not re-run (baseline 534/0 from the phase reports).
Verified clean, with no finding:
- Every Tokens.Dark.xaml value equals the web `:root` dark value, checked by hand including the alpha roundings (Scrim 99, Wash 0F, WashStrong 14, Shimmer 12, FocusRing 59, light Scrim 6B).
- Tokens.xaml and Tokens.Dark.xaml have the same 43 brush keys and 3 effect keys, and the web token names map 1:1 (C0=0).
- The light additions in Tokens.xaml are additions only.
- No XAML colour literals or named colours outside the two Tokens files.
- No `<SolidColorBrush>` or effect resources outside the two Tokens files.
- No StaticResource on colour keys.
- No code-behind brush snapshots apart from the 2 allowed lines.
- The code state setters use SetResourceReference plus ClearValue.
- Toasts, OverlayHost scrim and the stepper are converted.
- Registry read/write: a null StorageKey is handled, errors are swallowed, System deletes the value (same as web removeItem), and the UI_THEME override never reads or writes.
- SystemEvents is unhooked on Exit.
- ThemeChanged uses a local copy of the handler.
- run-hidden and theme-captures do not kill processes by name and do not write the registry. theme-captures cannot pass `none`.

## med

1. **wpf/ThemeManager.cs:143-155 (Apply)**
   - Problem: every `UserPreferenceChanged` with Category General while Mode==System removes the dark overlay and adds it again, even when the resolved theme did not change. Each call forces a full DynamicResource invalidation of every element in every window, twice, and raises ThemeChanged. Windows raises General often: accent colour, ImmersiveColorSet broadcasts, some settings pages, and repeated bursts on Windows 11. The result is UI hitches for consumers in System mode.
   - Fix: at the top of Apply, compute `dark`. If `_applied && dark == _isDark`, update only the title bars (or return). Otherwise do the dictionary mutation. Change the dictionary only when membership has to change, for example `if (dark && !contains) Add; else if (!dark && contains) Remove;`.

2. **wpf/Themes/Dropdown.xaml:358, wpf/Themes/Overlays.xaml:55, wpf/Themes/Sheet.xaml:160 (CaretBrush=Ink)**
   - Problem: the web caret is Brand, not Ink: `web/index.html:74 *{ caret-color: var(--brand); }`. The plan's Phase 4 note "web caret = ink" is wrong. The Phase 4 report justifies the light caret change (black to #26241F) with that wrong claim. This is an unjustified light-pixel change that does not reach parity.
   - Related gap: `FieldInput` (Forms.xaml:67) still uses the WPF default inverted-background caret (black in light, #DBDDE1 in dark). That is also not parity.
   - Fix: set `CaretBrush="{DynamicResource Brand}"` on FieldInput, BareInput, CmdkInput, SheetTextCell and the other TextBox styles. This matches web in both themes, is a light change, and should be recorded in REVIEWER.md. If that is out of scope, at least correct the justification and list "caret = Ink, web = Brand" as a known gap in wpf/COMPONENTS.md.

3. **wpf/ThemeManager.cs:126-141 (LoadDark) with :84 and :53**
   - Problem 1: the sibling URI is a relative string used from code, so it resolves against `pack://application:,,,/`, not against the URI of the dictionary that held Tokens.xaml. A consumer whose App.xaml is in a subfolder, or who merges with a path relative to another dictionary, gets an IOException on the first dark apply.
   - Problem 2: a missing Tokens.Dark.xaml throws only when dark is resolved. With System mode, that happens only on machines where the OS is dark. The throw happens in Initialize (startup crash) or in the Dispatcher callback from the SystemEvents handler (unhandled crash mid-session).
   - Fix: build the sibling as an absolute URI, for example `new Uri(d.Source.IsAbsoluteUri ? d.Source : new Uri(BaseUriHelper.GetBaseUri(d), d.Source), "Tokens.Dark.xaml")`, or document that the path must be root-relative or a pack URI. Resolve and load the dark dictionary once in Initialize, regardless of mode, so a missing file fails deterministically at startup on every machine.

## low

4. **plan/wpf/theme-lint.mjs:179**
   - Problem: `/Color\.FromA?Rgb/` is case-sensitive and never matches `Color.FromArgb` (lower-case "rgb"). C5 is blind to the most common colour constructor. Today it misses only Shimmer.cs:57,59 (transparent stops, harmless). `Colors.X` is not checked either.
   - Fix: use `/Color\.From(A?Rgb|Argb|ScRgb)|\bColors\.(?!Transparent\b)\w+/`. Mark Shimmer.cs:57,59 `// theme-lint: allow` (transparent ends), or build them from the band colour with alpha 0.

5. **wpf/ThemeManager.cs:57-58, 165 (title bar timing)**
   - Problem: the attribute is set on `Window.Loaded`, which is after the window is shown. On Windows 10 (attr 19/20) and some Windows 11 builds, DWM does not repaint the caption until the next activation or resize. The startup title bar and runtime switches can therefore stay light until focus changes. This is a manual-check item only.
   - Fix: also set the attribute from `SourceInitialized`, before the window is shown. Hook it per window in the class handler, or from `Window` construction via `EventManager.RegisterClassHandler` on `FrameworkElement.LoadedEvent` plus `HwndSource`. On a runtime switch, follow it with `SetWindowPos(hwnd, 0,0,0,0,0, SWP_NOMOVE|SWP_NOSIZE|SWP_NOZORDER|SWP_FRAMECHANGED)` to force a non-client repaint.

6. **wpf/OverlayHost.cs:221-227 (pre-existing, now theme-relevant)**
   - Problem: SetPageBlur saves `fe.Effect` and restores it as a local value. If a root child's Effect came from a style or DynamicResource (Shadow1/2), restoring it replaces the resource reference with a snapshot, so the effect stops following theme switches. `null` becomes a local null that overrides style setters.
   - Fix: save `fe.ReadLocalValue(UIElement.EffectProperty)`. On restore, call `ClearValue` when the saved value is `DependencyProperty.UnsetValue`, and otherwise SetValue the saved value (a ResourceReferenceExpression round-trips through SetValue).

7. **wpf/Shimmer.cs:30, 47-50**
   - Problem: the static field initializer now runs `(SolidColorBrush)Application.Current.FindResource("Shimmer")`. Before, it used `as Brush` with a null check. If Application.Current is null (designer, ElementHost/WinForms host) or the consumer's Tokens.xaml has no `Shimmer` key, the type initializer throws and Shimmer is unusable for the process lifetime (TypeInitializationException on every access).
   - Fix: in BandColor, use `TryFindResource` with `as SolidColorBrush` and fall back to the light `#A6FFFFFF` when null. Guard `Application.Current == null`.

8. **plan/wpf/REVIEWER.md (decisions)**
   - Problem: the two intentional light-pixel changes (FileInput title black to Ink, and caret black to Ink in BareInput/CmdkInput/SheetTextCell) break assumption 2 ("light byte-identical"). They are recorded only in docs/reports/wpf-dark-p4-implementer-hard.md, not in REVIEWER.md or COMPONENTS.md. The next baseline comparison against older refs will look like a regression.
   - Fix: add both to the 09 decisions in REVIEWER.md, with the web justification. Fix the caret justification per finding 2.

9. **wpf/tests/run-hidden.ps1:53, 117**
   - Problem: the finally block always removes `Env:UI_THEME` and never restores the caller's value. This is harmless with `pwsh -File`, the documented usage. If the script is invoked with `&` or dot-sourced in an interactive session, a user-set UI_THEME is silently lost. (It is the same pattern as UITEST_DRIVE.)
   - Fix: save `$prevTheme = $env:UI_THEME` before setting it, and restore it in finally (remove the variable only if `$prevTheme` was null).

## Notes (no action)

- `ThemeManager.Initialize` (line 56) dereferences `Application.Current` without the null guard that Apply has. That is fine under the contract ("call in OnStartup"), but `if (Application.Current != null)` would make it consistent.
- If a consumer sets `StorageKey = ""`, `CreateSubKey("")` writes `ui-theme` into the HKCU root. Consider treating an empty string as null.
- Shimmer's static ThemeChanged subscription is process-lifetime on a static target, so it does not leak. Ordering is fine: the brush reads the current token at creation, so an Apply that runs before type initialization is not missed.
