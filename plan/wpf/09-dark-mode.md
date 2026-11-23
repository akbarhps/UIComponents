# 09: dark mode (light/dark/system theming, parity with web ba418c1)

Owner: reviewer (cross-area). Worker name: `theme` (build `wpf\bin\theme\`). One worker, alone, while every component worker is idle, as in 08. Phase reviews: `phase-reviewer`. Final review: `reviewer`, once.
Rules from `plan/wpf/README.md` apply: never commit, never kill by name, C# 5 only, every app/test run goes through `run-hidden.ps1`, no csproj edits (wildcards pick up new `.cs`/`.xaml`).
Web source of truth: `web/index.html` `:root` (lines 18-68), `web/COMPONENTS.md` "Theme — light / dark", `docs/plans/web-dark-mode.md` tables 1.2/1.3 (the WPF mapping in section 5 there is implemented here).

## Why
The web now themes via `light-dark()` tokens plus a System/Light/Dark toggle. WPF has one light `Tokens.xaml`, ~1000 `{StaticResource <colour>}` references (resolved once, so they can never switch), ~30 colour literals in XAML and ~60 snapshot colour reads in code-behind (`(Brush)Res("X")`, `Brushes.White`, `new SolidColorBrush(...)`).

## Assumptions
1. All component workers are idle. The `theme` worker temporarily owns every file listed per phase (it crosses every area).
2. Light mode must stay **byte-identical** (per-section captures vs the Phase 0 baseline). Light values never change. Only intended light change: the masthead toggle (the masthead is not parity-tracked, 06 Q5).
3. `node` and `pwsh` are on PATH. The rtk hook rewrites `grep`; use `Select-String` / node in verify steps.
4. `run-hidden.ps1`'s `CreateProcess` passes `IntPtr.Zero` as environment (inherits). Phase 1 confirms; if not, pass the env explicitly.

## Open questions (default = what gets built)
- **Q1 Persistence.** Default: built into the library. `HKCU\Software\UIComponents.Wpf`, REG_SZ value `ui-theme` = `light|dark`; absent = System (mirrors web `localStorage['ui-theme']`). `ThemeManager.StorageKey` lets a consumer set its own subkey or `null` (no persistence). Alt: no built-in persistence.
- **Q2 Controls with no web counterpart** (anything Phase 1's dark inventory shows still light: Aero2 defaults, legacy `ClaudeInput`, SystemColors text). Default: tokenise only if a web counterpart exists; otherwise record as a known gap in REVIEWER.md, no restyle.
- **Q3 Page scrollbar** (`ContentScroll`, default Aero2). Default: accepted gap (web refs use `--hide-scrollbars`; captures exclude it). Follow-up task if wanted.
- **Q4 Dark title bar.** Default: yes, `DwmSetWindowAttribute(hwnd, 20, 1)` (fallback attr 19), silent no-op on older Windows. Manual check only (not in RenderTargetBitmap captures).

## Contracts

### C1. Resource dictionaries
- `wpf/Themes/Tokens.xaml` stays the light dictionary with every key it has today (consumer contract unchanged), plus the new keys below with their light values.
- New `wpf/Themes/Tokens.Dark.xaml`: an **overlay** with only `SolidColorBrush` and `DropShadowEffect` entries, exactly the same key set as Tokens.xaml's brushes and effects (no fonts/radii).
- Switching = `ThemeManager` adds the dark overlay as the **last** entry of `Application.Current.Resources.MergedDictionaries` (highest precedence) or removes it. Light mode = today's exact resource graph.
- Every colour/effect reference in XAML is `{DynamicResource K}`. Code sets colours with `el.SetResourceReference(Prop, "K")`, never by reading a brush.
- Rejected, do not try: mutating `SolidColorBrush.Color` in place (Style/Template sealing freezes the shared setter brushes, so it throws); reloading dictionaries + recreating windows (loses state; web keeps state).
- Never put `DynamicResource` inside a Freezable's property (`<SolidColorBrush Color=...>`, `<DropShadowEffect Color=...>`). Swap whole objects: effects are keyed `DropShadowEffect` resources per theme.

### C2. Token map (web `--kebab` → WPF PascalCase, digits kept). Alpha = round(a×255).
Existing 22 keys keep their names and light values: `Bg BgDeep Surface Surface2 Ink Ink2 Ink3 Line LineStrong Brand BrandHover BrandBright BrandSoft BrandSoftHover Success SuccessSoft Warning WarningSoft Danger DangerSoft Info InfoSoft`. Dark values = web table (`docs/plans/web-dark-mode.md` 1.2; e.g. `Bg #FF1C1A17`, `Brand #FFE08A66`, `BrandSoftHover #FF47302A`).

New keys (both dictionaries):
| key | light | dark | replaces |
|---|---|---|---|
| Focus | #FFA64E2A | #FFE08A66 | nothing yet (web `--focus`; existing `Brand` focus borders stay, same values) |
| OnBrand | #FFFFFFFF | #FF1C1A17 | `White` text/tick on `Brand` fills |
| OnInk | #FFFFFFFF | #FF1C1A17 | `White` on `Ink` fills (dark button, tooltip, dark avatar) |
| InkHover | #FF000000 | #FFFFFFFF | DarkButton hover |
| OnDanger | #FFFFFFFF | #FF1C1A17 | `White` on `Danger` fills |
| DangerHover | #FF982F27 | #FFF29D94 | danger hover |
| BrandLine | #FFECD4C8 | #FF5A3A2C | `AvatarBorder` key and `#FFECD4C8` literals |
| SuccessLine | #FFD7E7DD | #FF2D4636 | toast border (Toasts.cs) |
| InfoLine | #FFD9E4EE | #FF2E3F52 | toast border |
| DangerLine | #FFF0DCD8 | #FF5A2F2A | toast border |
| SageSoft | #FFE6EEE7 | #FF24302A | `SageBg` key, `#FFE6EEE7` |
| SageLine | #FFD3E3D6 | #FF33463A | `SageBorder` key, `#FFD3E3D6` |
| Wash | #99FFFFFF | #0FFFFFFF | `#99FFFFFF` (Toasts close hover) |
| WashStrong | #BFFFFFFF | #14FFFFFF | `#BFFFFFFF` (toast icon disc) |
| Scrim | #6B1E1A14 | #99000000 | OverlayHost scrim |
| Shimmer | #A6FFFFFF | #12FFFFFF | Shimmer.cs middle gradient stop |
| FocusRing | #FFF8ECE4 | #59E08A66 | every 3px `BrandSoft` focus/open ring (not chip or tinted backgrounds) |
| Knob | #FFFFFFFF | #FFF3EFE7 | switch knob fill |
| KnobShadow | DropShadowEffect Color #FF000000 Opacity 0.2 Blur 3 Depth 1 Dir 270 | same, Opacity 0.5 | switch knob inline effect |

Effects: `Shadow1` dark = `Color="#FF000000" Opacity="0.4" BlurRadius="14" ShadowDepth="2" Direction="270"`; `Shadow2` dark = `Color="#FF000000" Opacity="0.55" BlurRadius="30" ShadowDepth="4" Direction="270"` (starting values; Phase 4 tunes by measurement). Write every effect attribute in the order `x:Key Color Opacity BlurRadius ShadowDepth Direction` (the lint parses it).
WPF-only keys for the Sheet checkbox cell (imitates Chromium's native `accent-color` box; web has no token): `SheetCheckBorder` light `#FF767676`, `SheetCheckMark` light `#FFFFFFFF`; dark values measured in Phase 4 (Phase 2 sets dark = light as placeholder).
Removed keys (Display.xaml): `AvatarBorder`, `SageBg`, `SageBorder` (all usages move to the new names in Phase 2).
Collision check (done at planning): no existing `x:Key` in `wpf/**/*.xaml` matches any new key name above.
Contrast: C1 makes the WPF palette identical to the web palette, so the web `--check` contrast pairs (web plan 1.6) hold for WPF; no separate WPF contrast check. `SheetCheck*` copy Chromium's native control, no contrast target.

### C3. `wpf/ThemeManager.cs` (new, namespace `UIComponents.Wpf`, C# 5)
```csharp
public enum ThemeMode { System, Light, Dark }
public static class ThemeManager
{
    private static string _storageKey = @"Software\UIComponents.Wpf";   // null = never read/write
    private static ThemeMode _mode = ThemeMode.System;
    private static bool _isDark;
    public static string StorageKey { get { return _storageKey; } set { _storageKey = value; } }
    public static ThemeMode Mode { get { return _mode; } }      // user choice
    public static bool IsDark { get { return _isDark; } }       // resolved
    public static event EventHandler ThemeChanged;   // after every apply (also System-driven)
    public static void Initialize();                 // call once in App.OnStartup BEFORE any window; idempotent
    public static void SetTheme(ThemeMode mode);     // apply + persist (unless override/null key) + ThemeChanged
}
```
- `Initialize`: if env `UI_THEME` is `light|dark|system` → that mode, **override active: never read or write storage** for this process. Else read `StorageKey\ui-theme` (`light`/`dark` → mode, anything else → System). Apply. Hook `SystemEvents.UserPreferenceChanged` (on `UserPreferenceCategory.General` while `Mode == System` → `Application.Current.Dispatcher.BeginInvoke` re-apply); unhook on `Application.Current.Exit`. Register `EventManager.RegisterClassHandler(typeof(Window), FrameworkElement.LoadedEvent, ...)` to set the title bar on new windows.
- Dark dictionary URI: find the merged dictionary whose `Source.OriginalString` ends with `Tokens.xaml` (OrdinalIgnoreCase); build the sibling by replacing that suffix with `Tokens.Dark.xaml`, `UriKind.RelativeOrAbsolute`. Not found → `InvalidOperationException("ThemeManager: merge Themes/Tokens.xaml into Application.Resources first")`. Load once, cache.
- System dark = `HKCU\Software\Microsoft\Windows\CurrentVersion\Themes\Personalize` DWORD `AppsUseLightTheme == 0` (missing key → light).
- Apply: add/remove overlay, set `IsDark`, title bar for each `Application.Current.Windows` (`WindowInteropHelper.Handle`, skip Zero), raise `ThemeChanged` via local copy + null check.
- Title bar: `[DllImport("dwmapi.dll")] static extern int DwmSetWindowAttribute(IntPtr hwnd, int attr, ref int value, int size);` attr 20, on non-zero HRESULT retry 19. Ignore failure.

### C4. Showcase toggle (MainWindow, mirrors web `[data-theme-toggle]`)
Right after the masthead chip Border (MainWindow.xaml ~line 41): a `Border Background="{DynamicResource BgDeep}" CornerRadius="9" Padding="3" HorizontalAlignment="Left"` with a horizontal StackPanel of three `RadioButton Style="{StaticResource SegmentButton}" GroupName="ThemeMode"`: `x:Name`/AutomationId `ThemeSystem`, `ThemeLight`, `ThemeDark`; Content `System`/`Light`/`Dark`; `Tag` `system`/`light`/`dark`; `Margin="2,0,0,0"` on the 2nd/3rd; `Checked="ThemeToggle_Checked"`. Widths/heights/margins from the web masthead measurement (Phase 1 step 1).
`MainWindow.xaml.cs`: `SyncThemeToggle()` checks the radio matching `ThemeManager.Mode` under a `_syncingTheme` guard; called after `InitializeComponent` and from `ThemeManager.ThemeChanged`. `ThemeToggle_Checked`: if guard set return; map Tag → `ThemeMode`; `ThemeManager.SetTheme(mode)`.

### C5. Harness
- `wpf/tests/run-hidden.ps1`: new param `[ValidateSet('light','dark','system','none')][string]$Theme = 'light'`. Before launching anything: `if ($Theme -ne 'none') { $env:UI_THEME = $Theme } else { Remove-Item Env:UI_THEME -ErrorAction SilentlyContinue }`. Tests and captures therefore never depend on the OS theme or a stored choice. `none` only for the persistence check.
- New `plan/wpf/theme-captures.ps1 -Agent <n> -Out <dir> [-Theme light]`: section titles from `Select-String -Path wpf\*.xaml -Pattern 'SectionTitle'` (extract `Text="..."`; record the count), then per title `run-hidden.ps1 -Agent $Agent -NoTests -Theme $Theme -Section "<t>" -Capture "Sections=<dir>\<slug>.png"` (slug = title, non-alphanumerics → `-`). Prints `captured N, failed M`.
- New `plan/wpf/theme-lint.mjs` (node, no deps). `node plan/wpf/theme-lint.mjs [--only C0,C1,...]`, prints `Cn <name> violations=N` with up to 10 `file:line` each, then `RESULT theme-lint failed=K`; exit 1 if any selected check fails:
  - C0 missing: web `:root` `--x: light-dark(a, b);` tokens (split on the top-level comma) with no PascalCase key in Tokens.xaml.
  - C1 palette: for keys in both, light ARGB == Tokens.xaml, dark ARGB == Tokens.Dark.xaml. `#rgb`/`#rrggbb` → alpha FF; `rgba(r,g,b,a)` → alpha round(a×255). For a `DropShadowEffect` key compare Color RGB and round(Opacity×255) to the alpha (±1).
  - C2 keyset: SolidColorBrush + DropShadowEffect keys of Tokens.xaml == keys of Tokens.Dark.xaml.
  - C3 static: no `{StaticResource K}` for K in the C2 set, in `wpf/*.xaml`, `wpf/Themes/*.xaml`.
  - C4 literals: no `="#[0-9A-Fa-f]{3,8}"` or `="(White|Black)"` in XAML except the two Tokens files (`Transparent` allowed).
  - C5 snapshots: in `wpf/*.cs` no `Brushes\.(?!Transparent\b)\w+`, `Color\.FromA?Rgb`, `ColorConverter`, `\(Brush\)\s*(Res|FindResource|TryFindResource|res\[)`, `\b(Res|FindResource|TryFindResource)\([^)]*\)\s*as\s+Brush`, `\(Brush\)\s*\w*[Rr]esources?\[`, unless the line contains `theme-lint: allow`.
- Runtime switch in captures: `Sections!ThemeDark` works as is. `RunCaptures` (`App.xaml.cs:71-75`) sets `IsChecked = !IsChecked` on any `ToggleButton`, so an unchecked RadioButton becomes checked and fires `Checked`.
- Byte gate snippet (used by every phase):
  ```powershell
  function Same($a,$b){ $d=0; Get-ChildItem $a -Filter *.png | % { $o=Join-Path $b $_.Name; if(-not (Test-Path $o) -or (Get-FileHash $_.FullName).Hash -ne (Get-FileHash $o).Hash){ "DIFF $($_.Name)"; $d++ } }; "diffs=$d" }
  ```
- Pixel probe: `Add-Type -AssemblyName System.Drawing; $b=[Drawing.Bitmap]::new($p); '{0:X8}' -f $b.GetPixel($x,$y).ToArgb()`. In dark, text is the **bright** pixel (REVIEWER "first dark pixel" checks invert).

## Phase 0: baseline (no source edits). Tag: `mechanical`
1. Create `plan/wpf/theme-captures.ps1` (C5). Build: `cmd /c "wpf\build.bat theme"`; `cd wpf\tests; cmd /c ".\build-tests.bat theme"; cd ..\..`.
2. Twice: `pwsh -NoProfile -File plan/wpf/theme-captures.ps1 -Agent theme -Out $env:TEMP\theme\base` then `...\base2`. `Same base base2` → record the **unstable set** (expected empty; any file in it is excluded from later byte gates). (run-hidden has no `-Theme` yet in Phase 0: the script must omit `-Theme` when it is the default and run-hidden lacks the param.)
3. Open one capture: the masthead must not be in it (standalone mode). If it is, stop and report.
4. Full suite once: `pwsh -NoProfile -File wpf/tests/run-hidden.ps1 -Agent theme`; copy `%TEMP%\uitest-theme\tests.log` to `%TEMP%\theme\base.log`. Record the RESULT line.
5. Web dark refs for later: `pwsh plan/wpf/capture-web.ps1 -Sections buttons -Out plan/wpf/ref/web-dark-buttons.png -Js "document.documentElement.dataset.theme='dark'"`; open it: dark background confirms the recipe.
Verify: `captured N, failed 0` twice; `diffs=0` (or the unstable set listed); base.log RESULT recorded.

## Phase 1: walking skeleton (theme switches end to end). Tag: `hard`
Files: `wpf/ThemeManager.cs` (new), `wpf/Themes/Tokens.Dark.xaml` (new), `wpf/App.xaml.cs`, `wpf/MainWindow.xaml`, `wpf/MainWindow.xaml.cs`, `wpf/tests/run-hidden.ps1`, `plan/wpf/theme-captures.ps1`, `plan/wpf/theme-lint.mjs` (new).
1. Measure the web toggle: `capture-web.ps1 -Sections buttons -Out $env:TEMP\theme\web-mast.png -Js "document.querySelector('.masthead').style.display=''; ..."` with a rect dump of `.masthead .segment` and its three buttons (README `--dump-dom` recipe). Use those numbers in C4.
2. `Tokens.Dark.xaml`: the 22 existing keys (dark values from C2) + `Shadow1`, `Shadow2`. Same `xmlns` header as Tokens.xaml.
3. `ThemeManager.cs` exactly per C3 (all of it: env override, storage, System + SystemEvents, title bar, ThemeChanged).
4. `App.xaml.cs` `OnStartup`: first line `ThemeManager.Initialize();` (before `base.OnStartup(e)`, so before `StartupUri` creates the window).
5. MainWindow toggle per C4. In MainWindow.xaml change only the masthead lines (chip Border, its TextBlock, the new toggle) to `DynamicResource`; no other sweep yet.
6. `run-hidden.ps1` `-Theme` (C5); `theme-captures.ps1` passes `-Theme $Theme`. Confirm the `CreateProcess` env argument is `IntPtr.Zero` (inherit).
7. `theme-lint.mjs` (C5, all checks).
Verify:
```
cmd /c "wpf\build.bat theme"
node plan/wpf/theme-lint.mjs --only C1,C2                  # RESULT theme-lint failed=0 (C0 lists 19 missing keys = Focus + 18 of the 19 new web tokens, BrandSoftHover already exists: expected now)
pwsh -NoProfile -File plan/wpf/theme-captures.ps1 -Agent theme -Out $env:TEMP\theme\p1-light ; Same $env:TEMP\theme\base $env:TEMP\theme\p1-light   # diffs=0
pwsh -NoProfile -File wpf/tests/run-hidden.ps1 -Agent theme -NoTests -Theme dark  -Capture "Sections=$env:TEMP\theme\p1-dark-all.png"
pwsh -NoProfile -File wpf/tests/run-hidden.ps1 -Agent theme -NoTests -Theme light -Capture "Sections!ThemeDark=$env:TEMP\theme\p1-rt-dark-all.png"
cd wpf\tests; cmd /c ".\build-tests.bat theme forms"; cd ..\..; pwsh -NoProfile -File wpf/tests/run-hidden.ps1 -Agent theme -Only forms   # same RESULT as base for forms
```
Acceptance: p1-dark-all chip pixel = `FF3A2820` and an empty page pixel = `FF1C1A17`; p1-rt-dark-all chip pixel = `FF3A2820` (runtime switch through DynamicResource) with `ThemeDark` checked. `-Theme system`: page pixel dark iff `(Get-ItemProperty 'HKCU:\Software\Microsoft\Windows\CurrentVersion\Themes\Personalize').AppsUseLightTheme -eq 0`. **Inventory** (report): every region of p1-dark-all still light or with invisible (black) text, with the control and file. This list feeds Q2 and Phase 4.

## Phase 2: DynamicResource sweep + XAML literals + new tokens. Tag: `mechanical`
Files: every `wpf/*.xaml` and `wpf/Themes/*.xaml` (via the command), plus the literal sites below.
1. Add all C2 new keys to Tokens.xaml (light) and Tokens.Dark.xaml (dark), `SheetCheck*` with dark = light placeholder. Delete `AvatarBorder`, `SageBg`, `SageBorder` from Display.xaml.
2. Sweep (preserves BOM and line endings; one replacement per reference, lines do not shift):
   ```powershell
   $keys='Bg|BgDeep|Surface|Surface2|Ink|Ink2|Ink3|Line|LineStrong|Brand|BrandHover|BrandBright|BrandSoft|BrandSoftHover|Success|SuccessSoft|Warning|WarningSoft|Danger|DangerSoft|Info|InfoSoft|Shadow1|Shadow2|AvatarBorder|SageBg|SageBorder'
   Get-ChildItem wpf\*.xaml, wpf\Themes\*.xaml | ForEach-Object {
     $b=[IO.File]::ReadAllBytes($_.FullName); $bom=$b.Length -ge 3 -and $b[0] -eq 0xEF -and $b[1] -eq 0xBB -and $b[2] -eq 0xBF
     $t=[IO.File]::ReadAllText($_.FullName); $n=[regex]::Replace($t,'\{StaticResource ('+$keys+')\}','{DynamicResource $1}')
     $n=$n -replace '\{DynamicResource AvatarBorder\}','{DynamicResource BrandLine}' -replace '\{DynamicResource SageBg\}','{DynamicResource SageSoft}' -replace '\{DynamicResource SageBorder\}','{DynamicResource SageLine}'
     if($n -ne $t){ [IO.File]::WriteAllText($_.FullName,$n,(New-Object Text.UTF8Encoding($bom))); $_.Name } }
   ```
3. Literal sites (pre-Phase-1 line numbers; MainWindow.xaml shifted by Phase 1, re-find by the literal). Each becomes `{DynamicResource <key>}`:
   | site | literal | key |
   |---|---|---|
   | Themes/Controls.xaml:62 PrimaryButton Foreground | White | OnBrand |
   | Controls.xaml:98 DangerButton Foreground; :103 hover | White; #982F27 | OnDanger; DangerHover |
   | Controls.xaml:109 DarkButton Foreground; :114 hover | White; #000000 | OnInk; InkHover |
   | Controls.xaml:239 checkbox `Check` Stroke | White | OnBrand |
   | Controls.xaml:400 (small dark button, Ink fill) | White | OnInk |
   | Controls.xaml:419 DangerButtonSm Foreground; :438 hover | White; #FF982F27 | OnDanger; DangerHover |
   | Controls.xaml, Dropdown.xaml, Forms.xaml, Pickers.xaml: every `x:Name="FocusRing"`/`"OpenRing"` Border with `BorderThickness="3"` and `BrandSoft` | BrandSoft | FocusRing |
   | Themes/Display.xaml:101 PrimaryButtonSm Foreground | White | OnBrand |
   | Themes/ExtraControls.xaml:8 ToolTip Foreground | White | OnInk |
   | Themes/Navigation.xaml:302 TipBubbleText | White | OnInk |
   | Navigation.xaml:33-34 ButtonSpinner | `#73FFFFFF` ring, `White` arc | ring `Stroke="{TemplateBinding Foreground}" Opacity="0.45"`, arc `Stroke="{TemplateBinding Foreground}"` (web: currentColor) |
   | Themes/Forms.xaml:346 Knob Fill; :348-351 inline Ellipse.Effect | White; DropShadowEffect | Knob; `Effect="{DynamicResource KnobShadow}"` (delete the inline effect) |
   | Themes/Sheet.xaml:337 BorderBrush; :339 Stroke | #FF767676; White | SheetCheckBorder; SheetCheckMark |
   | Themes/Toasts.xaml:33; :69 | #BFFFFFFF; #99FFFFFF | WashStrong; Wash |
   | DataControls.xaml:112, MainWindow.xaml:469 | #FFECD4C8 | BrandLine |
   | MainWindow.xaml:488, :804 (avatar on Ink) | White | OnInk |
   | MainWindow.xaml:491; :492 | #FFE6EEE7; #FFD3E3D6 | SageSoft; SageLine |
   | MainWindow.xaml:902, 925, 949, 971, 1000 (stepper IconCheck on Brand) | White | OnBrand |
   Confirm each against the web selector in `docs/plans/web-dark-mode.md` 1.3 (map by the fill behind it, not by value). Chip/tinted `BrandSoft` backgrounds stay `BrandSoft`.
Verify:
```
cmd /c "wpf\build.bat theme"
node plan/wpf/theme-lint.mjs --only C0,C1,C2,C3,C4        # failed=0
pwsh -NoProfile -File plan/wpf/theme-captures.ps1 -Agent theme -Out $env:TEMP\theme\p2-light ; Same $env:TEMP\theme\base $env:TEMP\theme\p2-light   # diffs=0 (spinner opacity: if a Loading capture differs, report the max channel delta; <=1 accepted)
pwsh -NoProfile -File wpf/tests/run-hidden.ps1 -Agent theme -NoTests -Theme dark  -Capture "Sections=$env:TEMP\theme\p2-dark.png"
pwsh -NoProfile -File wpf/tests/run-hidden.ps1 -Agent theme -NoTests -Theme light -Capture "Sections!ThemeDark=$env:TEMP\theme\p2-rt-dark.png"
```
Acceptance: p2-rt-dark vs p2-dark differ only at code-built elements (list their bounding boxes and owners in the report; Phase 3 fixes them). A DynamicResource on a non-DP throws at load, so the app must start (captures succeed).

## Phase 3: code-behind colours. Tag: `hard`
Files: `Autocomplete.cs`, `CalendarPicker.cs`, `FileInput.cs`, `MultiSelect.cs`, `SearchableSelect.cs`, `TagInput.cs`, `Toasts.cs`, `OverlayHost.cs`, `Shimmer.cs`, `MainWindow.Navigation.cs`, `AdvancedControls.xaml.cs`, `OverlayControls.xaml.cs`.
Rule: `x.P = (Brush)Res("K")` → `x.SetResourceReference(<Owner>.PProperty, "K")`; ternaries pick the key string (`label.SetResourceReference(TextBlock.ForegroundProperty, selected ? "OnBrand" : "Ink")`); `Brushes.Transparent` stays a plain assignment (it replaces the reference). Properties set in object initialisers move to a statement after construction. Find every site with `node plan/wpf/theme-lint.mjs --only C5`.
- `Brushes.White` on Brand fills (CalendarPicker 514/585/603, MultiSelect 349 tick) → `OnBrand`.
- Code-built 3px rings: `TagInput.cs:57`, `Autocomplete.cs:85` `BrandSoft` → `FocusRing`.
- State setters (stepper `MainWindow.Navigation.cs:181-197`, `SearchableSelect.cs:372-373`, `Autocomplete.cs:319`, `MultiSelect.cs:418-419`, `AdvancedControls.xaml.cs:100`) re-run on state change only, so they must use `SetResourceReference` too. `AdvancedControls.xaml.cs:200` (`Brush brand`): set by reference at each use.
- `Toasts.cs:55-98`: keep `accentKey`/`bgKey`; `borderHex` becomes `borderKey` = `DangerLine`/`InfoLine`/`SuccessLine`; every element that received `accent`/`background`/`border` gets `SetResourceReference` with the key.
- `OverlayHost.cs:41` scrim → `_scrim.SetResourceReference(Border.BackgroundProperty, "Scrim")`.
- `Shimmer.cs`: middle stop colour = `((SolidColorBrush)Application.Current.FindResource("Shimmer")).Color` when the brush is created and on `ThemeManager.ThemeChanged` (update `OverlayBrush.GradientStops[1].Color`; the brush is animated so not frozen). Reduced-motion `Line` background → `SetResourceReference`. Mark the FindResource line `// theme-lint: allow`.
- `App.xaml.cs:289` (capture background) reads `Bg` at capture time: correct, mark `// theme-lint: allow`.
Verify:
```
cmd /c "wpf\build.bat theme"; cd wpf\tests; cmd /c ".\build-tests.bat theme"; cd ..\..
node plan/wpf/theme-lint.mjs                               # all checks, failed=0
pwsh -NoProfile -File plan/wpf/theme-captures.ps1 -Agent theme -Out $env:TEMP\theme\p3-light ; Same $env:TEMP\theme\base $env:TEMP\theme\p3-light   # diffs=0
(p3-dark vs p3-rt-dark full-page captures as in Phase 2)    # identical except inside the ThemeDark/ThemeLight radio bounds
pwsh -NoProfile -File wpf/tests/run-hidden.ps1 -Agent theme   # full suite; Names(base.log) vs Names(new) identical except known flaky
```
Persistence check (restore afterwards): save `reg query HKCU\Software\UIComponents.Wpf /v ui-theme`; `run-hidden.ps1 -Agent theme -NoTests -Theme none -Capture "Sections!ThemeDark=$env:TEMP\theme\persist1.png"` → value is `dark`; `run-hidden.ps1 -Agent theme -NoTests -Theme none -Capture "Sections=$env:TEMP\theme\persist2.png"` → page pixel `FF1C1A17`; restore/delete the value.

## Phase 4: dark parity vs web. Tag: `hard`
Files: Tokens.Dark.xaml (shadow tuning, `SheetCheck*` dark), plus the owning theme file for each fix the comparison finds (state it per fix).
1. Per web section id (every `<section class="section" id=...>`): `capture-web.ps1 -Sections <id> -Out plan/wpf/ref/web-dark-<id>.png -Js "document.documentElement.dataset.theme='dark'"`; WPF `theme-captures.ps1 -Theme dark -Out plan/wpf/ref/wpf-dark` (crop recipe in REVIEWER: content x≈314, `.demo` 984 wide, 31px inner offset).
2. Probe per section ≥3 points (card surface, a border, a filled control and its text). Exact hex required for fills/borders; text colour from the brightest glyph pixel; shadows within ±10 per channel at 4px outside the card edge.
3. States: focus ring (`@<FocusId>`), open dropdown/picker (`!` chains from the existing task files), selected picker cell, multi checkbox on, switch on, toast variants, modal scrim, loading spinner, tooltip. Compare with web `-Js` that opens the same state.
4. Measure Chromium's dark native checkbox (grid section, checked and unchecked) → `SheetCheckBorder`/`SheetCheckMark` dark (+ an off-state fill key if the web box is not transparent, in both dictionaries, light = current). Tune `Shadow1`/`Shadow2` dark opacity.
5. Phase 1 inventory items: fix those with a web counterpart (they are token bugs); record the rest as known gaps (Q2). Check caret visibility in `FieldInput`/`BareInput` (web caret = ink); if invisible set `CaretBrush="{DynamicResource Ink}"` in those templates. Tab through: any default dashed `FocusVisualStyle` invisible on dark → report.
Verify: per-section table in the report (`section | probe | web | wpf | ok`), all ok or listed gap; light byte gate `diffs=0`; `node plan/wpf/theme-lint.mjs` failed=0.

## Phase 5: docs. Tag: `mechanical`
Files: `wpf/COMPONENTS.md`, `plan/wpf/README.md`, `plan/wpf/REVIEWER.md`.
- COMPONENTS.md: Tokens row adds `Tokens.Dark.xaml` and the C2 keys; new rows `Theme` (`ThemeManager.SetTheme`, `Mode`, `IsDark`, `ThemeChanged`, `wpf/ThemeManager.cs`) and `Theme toggle` (3 `SegmentButton` radios, see MainWindow masthead). Consumer section: compile `ThemeManager.cs`; copy/link `Tokens.Dark.xaml` next to `Tokens.xaml`; merge into `Application.Resources` (not `Window.Resources`, so popups resolve); call `ThemeManager.Initialize()` first in `OnStartup`; colours only via `DynamicResource` / `SetResourceReference`; `StorageKey` (own subkey or null); env `UI_THEME` forces a mode without persisting.
- README.md: ownership row `ThemeManager.cs`, `Themes/Tokens.xaml`, `Themes/Tokens.Dark.xaml`, `plan/wpf/theme-*.{ps1,mjs}` → reviewer. Gotchas: "colour = DynamicResource or SetResourceReference, never StaticResource/Res() (theme-lint C3/C5)"; "captures/tests run light unless `-Theme dark`"; "in dark, text is the bright pixel". Task table: add row 09.
- REVIEWER.md: status row 09 → DONE; decisions: overlay dictionary, rejected in-place mutation, persistence (Q1), gaps (Q2/Q3); command block: `-Theme dark` capture and `node plan/wpf/theme-lint.mjs`.
Verify: `node plan/wpf/theme-lint.mjs` failed=0; `Select-String -Path wpf\COMPONENTS.md -Pattern 'ThemeManager','Tokens.Dark'` both found.

## Reviewer checklist
- Light byte gate `diffs=0` in every phase; no light value changed (`git diff wpf/Themes/Tokens.xaml` adds lines only).
- `theme-lint` all checks green; no `theme-lint: allow` beyond App.xaml.cs:289 and Shimmer.
- Runtime switch == startup dark (full page), so no snapshot colours remain.
- No test or capture can persist a choice (UI_THEME override path); registry restored after the persistence check.
- C# 5 (no `?.`, `$""`, `=>` members); `ThemeChanged` raised via local copy; SystemEvents unhooked on Exit.
- Manual (user, visible desktop): title bar dark; System mode follows a Windows app-theme switch live; reload keeps the stored choice.

## Risks / do not touch
- Do not change any light value or any non-colour property. Do not convert style/font/radius keys to DynamicResource (`BasedOn` must stay static).
- Never DynamicResource inside a Freezable property; never mutate token brushes.
- Never run a test or capture without `-Theme` other than the Phase 3 persistence check (shared HKCU across agents).
- `ClaudeInput` stays as is (legacy rule); if it shows as light in dark, it is a gap, not a fix.
- Web is the truth: if a C2 value disagrees with `web/index.html`, follow the web and report.
