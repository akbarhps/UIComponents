# WPF dark mode, Phase 2 (DynamicResource sweep + XAML literals + new tokens), implementer-mechanical report

Plan: `plan/wpf/09-dark-mode.md`, Phase 2 only. Agent `theme`. Q1-Q4 = plan defaults. Not committed.

## Files changed
- `wpf/Themes/Tokens.xaml`: added the 20 light brushes (Focus, OnBrand, OnInk, InkHover, OnDanger, DangerHover, BrandLine, SuccessLine, InfoLine, DangerLine, SageSoft, SageLine, Wash, WashStrong, Scrim, Shimmer, FocusRing, Knob, SheetCheckBorder, SheetCheckMark) and the `KnobShadow` effect (light). Additions only.
- `wpf/Themes/Tokens.Dark.xaml`: same 20 brushes and `KnobShadow` (Opacity 0.5) with dark values from C2. SheetCheck* dark = light placeholder.
- `wpf/Themes/Display.xaml`: deleted `AvatarBorder`, `SageBg`, `SageBorder` and their comment line (plus the blank line after it). Literal `White` on PrimaryButtonSm changed to OnBrand.
- `wpf/Themes/Controls.xaml`, `Dropdown.xaml`, `Forms.xaml`, `Pickers.xaml`: 11 + 3 + 6 + 2 literal and FocusRing sites per the plan table (White, #982F27, #000000, #FF982F27 to Ink/Danger/Brand keys; 11 `FocusRing`/`OpenRing` Borders with BorderThickness 3 `BrandSoft` to `FocusRing`).
- `wpf/Themes/Forms.xaml`: Knob `Fill` to `Knob`; inline `Ellipse.Effect` removed, `Effect="{DynamicResource KnobShadow}"` added to the Ellipse.
- `wpf/Themes/ExtraControls.xaml`, `Navigation.xaml`, `Sheet.xaml`, `Toasts.xaml`: literal sites per the plan table. Navigation ButtonSpinner ring is `Stroke="{TemplateBinding Foreground}" Opacity="0.45"`, arc `Stroke="{TemplateBinding Foreground}"`.
- `wpf/DataControls.xaml`, `wpf/MainWindow.xaml`: literal sites (BrandLine, OnInk, SageSoft, SageLine, OnBrand x5 stepper icons).
- Sweep (`{StaticResource K}` to `{DynamicResource K}` for the C2 colour keys, plus AvatarBorder/SageBg/SageBorder mapped to BrandLine/SageSoft/SageLine) applied to all `wpf/*.xaml` and `wpf/Themes/*.xaml`. Touched: AdvancedControls, DataControls, MainWindow, OverlayControls, TablePagination, Controls, Display, Dropdown, ExtraControls, Forms, Navigation, Overlays, Pickers, Sheet, Toasts.
- Not touched: any `.cs` file (Phase 3), `Tokens.xaml` existing lines, all light values.
- Encoding: BOM and CRLF preserved (every line of each edited file still ends CRLF). Edit script lives in the scratchpad, not the repo.

## Verify results
```
cmd /c "wpf\build.bat theme"                                  Built bin\theme\UIComponents.Wpf.exe
node plan/wpf/theme-lint.mjs --only C0,C1,C2,C3,C4            C0 0 / C1 0 / C2 0 / C3 0 / C4 0 / RESULT theme-lint failed=0
theme-captures -Theme light -> p2-light                       captured 36, failed 0
Same base p2-light                                            diffs=1 (Buttons.png, see below)
run-hidden -Theme dark  Sections -> p2-dark.png               captured
run-hidden -Theme light Sections!ThemeDark -> p2-rt-dark.png  captured
p2-dark vs p2-rt-dark                                         0 differing pixels (identical)
chip (90,8) = FF3A2820 both; page (900,300) = FF1C1A17 both
build-tests.bat theme (full)                                  Built bin\theme\UIAutomationTests.exe
run-hidden -Agent theme (full suite)                          RESULT  passed=533  failed=1
```
- Light byte gate: 35 of 36 per-section captures identical to base. `Buttons.png`: 7 pixels differ, max channel delta 1, bbox (413,509)-(425,519). That is the Loading spinner (ring opacity 0.45 on Foreground instead of a literal `#73FFFFFF`). Within the plan's <=1 tolerance.
- Full suite: `RESULT  passed=533  failed=1` (baseline is the same). PASS/FAIL names compared with `%TEMP%\theme\base.log`: 534 vs 534, identical except the known failure's timing numbers (`FAIL resume: still present before remaining expires - remaining=1647 sinceLeft=1750` now vs `remaining=1287 sinceLeft=1422` in base). That is the pre-existing `Tests.Toasts.cs:242` failure, not fixed.
- DRIVER lines unchanged from baseline: `DRIVER  fast=281 fallback=4`, `DRIVER  fallback select:<no id> x2`, `DRIVER  fallback toggle:RowCheck x2`. No new fallback lines.
- Suite log: `%TEMP%\uitest-theme\tests.log`, copied to `%TEMP%\theme\p2.log`.

## Dark checks
- Loading-spinner, lazily loaded templates: the 36 static captures do not open popups. The full suite (dropdown, pickers, overlays, toasts run under light) passes at baseline, but dark popups/toast/overlay templates were not rendered in dark. Not verified in dark.
- DynamicResource inside Freezable or animation content: none. Grep of all wpf XAML for `DynamicResource` inside KeyFrame / Storyboard / Pen / GradientStop / DrawingBrush / VisualBrush / DropShadowEffect / SolidColorBrush / `Color="{DynamicResource` returned no matches. No Storyboards exist in wpf XAML.
- Pure-white / pure-black scan of all 36 per-section dark captures (`%TEMP%\theme\p2-dark-sec`): only one hit, **File-input.png, 631 black pixels, bbox (207,282)-(784,294)** (the "Drop a file here" title). Owner: `FileInput.cs` / `Pickers.xaml` (default SystemColors foreground). Not a Phase 2 literal or StaticResource site. Deferred to Phase 3/4.
- Scan limit: it only detects pure #FFFFFF and #000000. Other wrong colours (e.g. light-on-light) are not detected by it.
- Full-page limit: `Sections` capture is 984x1005 (scroll viewport), not the full 1029-tall column. Startup-dark vs runtime-switch is identical over that viewport (0 pixels). Code-built elements in the captured area: none found (0 differing pixels). Code-built elements in popups/overlays are not in any capture.

## Focus-ring cross-check (plan 09 Phase 2 step 3 vs web-dark-mode.md table 1.3)
- Web `var(--focus-ring)` uses: 8 (`web/index.html` lines 340, 500, 595, 625, 796, 814, 835, 857). Table 1.3 lists 8 sites. Match.
- WPF: 11 XAML `FocusRing`/`OpenRing` Borders switched to `FocusRing` (Controls 595, 647; Dropdown 21, 87, 218; Forms 11, 42, 148, 200; Pickers 27, 123), plus 2 code-built rings for Phase 3 (TagInput.cs:57, Autocomplete.cs:85). The WPF count is higher than the web's 8 because WPF has per-control ring templates. Each of the 11 is named explicitly in the plan's rule. Not verified 1:1 against web selectors.
- `OverlayControls.xaml:509` (`Ellipse Stroke BrandSoft StrokeThickness 3`, loading spinner track): not a FocusRing/OpenRing Border, not named in the plan. Left as a `DynamicResource BrandSoft` (sweep only). Note for overlays owner / Phase 4: in dark the track is BrandSoft (#3A2820), web spinner track is currentColor 45%.

## Deviations from the plan
- Display.xaml: also deleted the `<!-- Web :root colours that have no token in Tokens.xaml. -->` comment and the blank line after the three keys. The comment described only those keys, so it became stale.
- Knob shadow: inline `Ellipse.Effect` replaced by an `Effect="{DynamicResource KnobShadow}"` attribute on the Ellipse (plan said "delete the inline effect"; this is the equivalent). Light values identical (Opacity 0.2, Blur 3, Depth 1, Dir 270).
- Full suite was run (not in the Phase 2 verify list), as requested in the task.
- Buttons.png 7-px delta 1 (spinner opacity), accepted per the plan's tolerance.

## Blocking questions
None.
