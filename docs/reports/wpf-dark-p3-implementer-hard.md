# WPF dark mode, Phase 3 (code-behind colours), implementer-hard report

Plan: `plan/wpf/09-dark-mode.md`, Phase 3 only. Agent `theme`. Q1-Q4 = plan defaults. Not committed.

## Files changed
All code-behind colour snapshots became `SetResourceReference(<Owner>.<Prop>Property, "<Key>")`. `Brushes.Transparent` stays a plain assignment (it replaces the reference).
- `wpf/Autocomplete.cs`: multi-mode focus ring `BrandSoft` -> `FocusRing`; chip text `Brand`; highlighted option `BgDeep` (reference when active, `Brushes.Transparent` when not).
- `wpf/CalendarPicker.cs`: trigger icon/value, footer and time-row lines, time labels, selected cell fill/text (`Brushes.White` -> `OnBrand`, ternary picks the key), today ring, commit/clear value colours.
- `wpf/FileInput.cs`: upload icon, subtitle, count, item icon/name/size, remove icon.
- `wpf/MultiSelect.cs`: chevron stroke (Path pulled out of the object-initialiser into `chevronPath`), search border, count, option box border/fill, tick (`Brushes.White` -> `OnBrand`), meta, state setters (`on ? "Brand" : "Surface"` / `"LineStrong"`), chip text.
- `wpf/SearchableSelect.cs`: value, search border, meta, active row (reference / Transparent), row label `selected ? "Brand" : "Ink"`.
- `wpf/TagInput.cs`: focus ring `BrandSoft` -> `FocusRing`, chip text.
- `wpf/Toasts.cs`: `borderHex` -> `borderKey` (`DangerLine`/`InfoLine`/`SuccessLine`); `accent`/`background`/`border` locals removed, shadow background (`bgKey`), card border (`borderKey`), icon and title (`accentKey`) and close icon (`Ink3`) set by reference. Object initialisers split into statements.
- `wpf/OverlayHost.cs`: scrim -> `SetResourceReference(Border.BackgroundProperty, "Scrim")`.
- `wpf/Shimmer.cs`: new static constructor subscribes `ThemeManager.ThemeChanged` and re-reads the `Shimmer` token into `OverlayBrush.GradientStops[1].Color`; `BandColor()` (FindResource, `// theme-lint: allow`) is also used when the brush is created. Reduced-motion `Line` background by reference.
- `wpf/MainWindow.Navigation.cs`: stepper marker/num/title/line by reference (ternaries pick key strings).
- `wpf/AdvancedControls.xaml.cs`: `PinFilter` background by reference; `UpdateSortVisuals`: removed the `Brush brand` snapshot, active header sets `Brand` by reference, inactive still `ClearValue`.
- `wpf/OverlayControls.xaml.cs`: cmdk row icon and shortcut text (object initialisers split).
- `wpf/App.xaml.cs`: line 290 (`(Brush)FindResource("Bg")` capture background) marked `theme-lint: allow` (the plan's step; the lint had it unmarked). Light/dark correct because it is read at capture time.
Line endings unchanged from pre-edit (no whole-file rewrites; `git diff --stat` is line-level). No XAML or Tokens touched in this phase.

## Verify results
```
cmd /c "wpf\build.bat theme"                      Built bin\theme\UIComponents.Wpf.exe
build-tests.bat theme (full)                      Built bin\theme\UIAutomationTests.exe
node plan/wpf/theme-lint.mjs                      C0..C5 violations=0 / RESULT theme-lint failed=0
theme-captures -Theme light -> p3-light           captured 36, failed 0
Same base p3-light                                diffs=1 (Buttons.png)  -- same 7-px spinner delta-1 accepted in P2
Same p2-light p3-light                            diffs=0 (Phase 3 changed nothing in light)
dark startup vs Sections!ThemeDark (full page)    p3-dark vs p3-rt-dark: 984x1005, diffpx=0 (masthead not in this capture)
full suite run-hidden -Agent theme                RESULT  passed=533  failed=1
```
- Full suite: the 534 PASS/FAIL names equal `base.log` (diff empty, after stripping timings). The one failure is the pre-existing `FAIL resume: still present before remaining expires - remaining=1741 sinceLeft=1797` (Tests.Toasts.cs:242), untouched. `DRIVER fast=281 fallback=4` = baseline (`select:<no id> x2`, `toggle:RowCheck x2`): no new fallback lines. Log copied to `%TEMP%\theme\p3.log`.
- Persistence (`-Theme none`): `Sections!ThemeDark` wrote `HKCU\Software\UIComponents.Wpf  ui-theme  REG_SZ  dark`; the next launch with no toggle gave page pixel (900,300) = `FF1C1A17`. Registry key was absent before and was deleted afterwards (verified absent).

## Extra dark check (beyond the plan): code-built popups, overlays, toasts
The plan's full-page diff cannot see popups/overlays/toasts. For each state below I captured dark-at-startup (`-Theme dark`) and light-then-`!ThemeDark` runtime switch in standalone mode (the hidden toggle is still clickable) and compared pixels. All `diffpx=0`, so no snapshot colour remains in these paths:
Tag input (focus ring), Autocomplete multi (focus ring), Searchable select open, Multi-select open + option toggled, Date picker pick day, Date & time picker open, File input, Modal dialog open, Command palette open, Loading modal open, Modal form open, Stepper (Next x2), Sortable table pin toggle, Toasts (success/info/error), Toast variants (dismissible/action/persistent). Viewed combo, multi, modal, loading, toasts captures: dark and legible apart from the gaps below. Not covered: Autocomplete option list (needs typing in a focused field), time-row inputs of the date-time picker, FileInput with files added, tooltip.
Captures are in `%TEMP%\theme\x\*-dark.png` / `*-rt.png`.

## Remaining dark gaps (pre-existing, deliberately NOT fixed here): Phase 4 step 5 inputs
Not in the Phase 3 site list. Both text sites render black (`#000000`) in both themes; web is ink (`#26241F`).
1. File-input title ("Drop a file here" / "Drop images here"), `FileInput.cs` ~line 73: `TextBlock title` has no Foreground and inherits SystemColors black in both themes. Web: `.file-drop` is a `<label>`, text inherits body ink. Fix = `title.SetResourceReference(TextBlock.ForegroundProperty, "Ink")`. Expect the light per-section `File-input.png` to differ afterwards (web-wins parity fix; bbox about (207,282)-(784,294)).
2. Multi-select option labels ("React", "TypeScript", ...), `MultiSelect.cs` BuildOption `label` (and the `MultiOption` style in `Themes/Dropdown.xaml:288` has no Foreground): black on the dark panel (seen in `multi-dark.png`). Same root cause; fix = Ink on the label. This one sits in a popup, so the per-section light captures never render it and fixing it does not trip the light byte gate.
3. `OverlayControls.xaml:509` loading-spinner track: `Ellipse Stroke` is `BrandSoft`; in dark it is `#3A2820` (visible in loading-dark.png as a dim ring). Web track is currentColor at 45%. Phase 4/overlays.
No other black/white text was seen in the viewed captures; the pure white/black scan from P2 only finds #000000/#FFFFFF.

## Deviations from the plan
- `SearchableSelect.cs`/`Autocomplete.cs` active-row ternary `active ? (Brush)Res("BgDeep") : Brushes.Transparent` written as `if (active) SetResourceReference(...); else Background = Brushes.Transparent;` (a ternary cannot mix a key and a brush).
- `MultiSelect.cs` chevron: the nested `new Path {...}` was hoisted into a local to allow `SetResourceReference` (Properties in initialisers must move out, per the rule).
- `Shimmer.cs`: `BandColor()` helper + static constructor for the `ThemeChanged` hook; the allow-marked FindResource is the single one in `BandColor()`.
- Extra dark startup-vs-runtime comparison for popups/overlays/toasts (not required by the plan).
- Light `Buttons.png` 7-px delta-1 difference carried over from P2 (spinner opacity), unchanged by P3.

## Shimmer is not visually verified
`--capture` collapses the shimmer overlay, so no capture exercised the `ThemeChanged` gradient-stop update or the reduced-motion `Line` reference, and the popup/overlay sweep did not include Skeleton. It builds and the suite passes; code-only.

## Blocking questions
No.
