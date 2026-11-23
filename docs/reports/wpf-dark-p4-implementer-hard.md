# WPF dark mode, Phase 4 (dark parity vs web), implementer-hard report

Plan: `plan/wpf/09-dark-mode.md`, Phase 4 only. Agent `theme`. Q1-Q4 = plan defaults. Not committed.

## Result
```
cmd /c "wpf\build.bat theme"                         Built bin\theme\UIComponents.Wpf.exe
node plan/wpf/theme-lint.mjs                         C0..C5 violations=0 / RESULT theme-lint failed=0
theme-captures -Theme light -> p4-light              captured 36, failed 0
Same base p4-light                                   diffs=2: Buttons.png (7-px delta-1 spinner, accepted P2) + File-input.png (intended, see below)
Same p3-light p4-light                               diffs=1: File-input.png only
full suite, run 2 (final)                            RESULT  passed=534  failed=0   DRIVER fast=281 fallback=4 (select:<no id> x2, toggle:RowCheck x2) = baseline
full suite, run 1                                    RESULT  passed=456  failed=1   (FLAKE, see below)
git diff wpf/Themes/Tokens.xaml                      22 insertions, 0 deletions (no light value changed)
```
- Full suite run 1: `FAIL area dropdown threw InvalidOperationException: Unsupported Pattern.` right after `T4 no comma split on blur` (Tests.Dropdown.cs T6 `Invoke(remove0)`; one `DRIVER fallback invoke:<no id> x1`, fast=227). Not reproducible: `run-hidden -Only dropdown` alone gave `RESULT passed=90 failed=0 fast=62 fallback=0`, and the full suite re-run gave 534/0 with identical DRIVER lines to baseline. Treated as a transient UIA flake (no source edit between run 1 and run 2). Log kept at `%TEMP%\theme\p4-run1-flaky.log`; final log `%TEMP%\theme\p4.log`.
- Run 2 vs `base.log`: 534 vs 534 PASS/FAIL names; the only difference is the known baseline failure `resume: still present before remaining expires` (Tests.Toasts.cs:242, timing-flaky) which PASSED this time. Not touched.
- The suite ran with all Phase 4 edits in place. A temporary revert of two CaretBrush setters (for a before/after probe) was restored and rebuilt afterwards; lint is green after the restore. The restored files are byte-equal to what the run-2 suite used.

## Files changed (Phase 4 only; each fix and its owner)
| file | change | owner area |
|---|---|---|
| `wpf/FileInput.cs` | `title.SetResourceReference(TextBlock.ForegroundProperty, "Ink")` ("Drop a file here" was inheriting SystemColors black in both themes) | pickers |
| `wpf/MultiSelect.cs` | `label.SetResourceReference(TextBlock.ForegroundProperty, "Ink")` in BuildOption (option labels black on the dark panel) | dropdown |
| `wpf/Themes/Tokens.xaml` | new key `SheetCheckFill` light `#00FFFFFF` (= the old `Transparent` box fill, so light is identical) | reviewer |
| `wpf/Themes/Tokens.Dark.xaml` | `SheetCheckBorder` `#FF858585`, `SheetCheckMark` `#FF3B3B3B`, new `SheetCheckFill` `#FF3B3B3B` (measured, below); Shadow1/Shadow2 kept | reviewer |
| `wpf/Themes/Sheet.xaml` | Box `Background="{DynamicResource SheetCheckFill}"` (was `Transparent`); comment updated; `SheetTextCell` gets `CaretBrush=Ink` | grid |
| `wpf/Themes/Dropdown.xaml` | `BareInput` style gets `CaretBrush=Ink` | dropdown |
| `wpf/Themes/Overlays.xaml` | `CmdkInput` style gets `CaretBrush=Ink` | overlays |
No `.cs` colour snapshots, no csproj, no tests, no `App.xaml.cs` touched. Generated refs (not source): `plan/wpf/ref/web-dark-<id>.png` (36) + `web-dark-state-*.png`, `plan/wpf/ref/wpf-dark/*.png` (36), `plan/wpf/ref/wpf-dark-states/*.png`.

## Light change (File-input) justified against the web
`File-input.png` light differs from the Phase 3 capture in 1198 px, bbox (205,282)-(785,294) (the title row only), max channel delta 38. Darkest title glyph pixel: p3-light `000000`, p4-light `26241F`, web `plan/wpf/ref/web-file-input.png` `26241F`. So the light title was black and is now ink like the web (web `.file-drop strong` inherits `--ink`). This is the only light pixel change besides the accepted Buttons spinner delta.
The MultiSelect label fix is in a popup, so no static light capture shows it.

## Carry-over items
1. File-input title black in both themes: FIXED (above). Dark: title pixel `ECE8DF`.
2. Multi-select option labels black on the dark panel: FIXED. Verified in `wpf-dark-states/multiopen.png` and `multion.png` (labels light, checks on = Brand fill, 2 chips).
3. `OverlayControls.xaml:509` loading-spinner track: FALSE POSITIVE, no change. The loading modal spinner in web is `.loading-modal__spinner{ border:3px solid var(--brand-soft); border-top-color:var(--brand) }` (web/index.html:554). The `currentColor 45%` rule (line 210) belongs to the button `.spinner`, already ported in Phase 2 (Navigation.xaml). Measured: track `3A2820` present in both `web-dark-state-loading.png` and `wpf-dark-states/loading.png`; arc `E08A66` present in both (dy 28 = modal offset).
4. Autocomplete option list: verified open (focus shows all options). Panel `24221E`, option text ink, scrollbar thumb themed (DropdownScrollBar). Typing a filter is not drivable by `--capture` (`^text` needs a focused TextBox reached through a `!` click, and the autocomplete input is not a button); the filtered list uses the same `AcOption`/`AcEmpty` styles (Ink via `DropdownOption`).
5. Time-row inputs: verified in `wpf-dark-states/dtopen.png` (Hour/Minute boxes `TableFilterInput`: Surface fill, ink text, label Ink2).
6. FileInput with files added: CODE-ONLY. `AddFiles` is private and only reachable through `OpenFileDialog` or drag-drop, neither drivable hidden. Code read (FileInput.cs `Render`): icon Ink3, name Ink, size Ink3, remove icon Ink3, row style `PickerFileItem` (token-driven); web `.file-item` uses `--surface`/`--line`/ink/ink-3. No black-text risk found, but not rendered.
7. Tooltip: verified (`wpf-dark-states/tip.png`): bubble `ECE8DF` (Ink) on dark, label `OnInk`. Web `.tip::after` = `background var(--ink); color var(--on-ink)`, bubble `ECE8DF` measured in `web-dark-state-tip.png`.
8. Shimmer: CODE-ONLY, still not visually verified (`--capture` collapses the overlay). `Shimmer` token `#12FFFFFF` = web `rgba(255,255,255,.07)` (0.07*255=18=0x12).

## Per-section probe table (web dark vs WPF dark; 36 of 36 sections)
Method: `capture-web.ps1` (dark, 1400x1600) vs `theme-captures.ps1 -Theme dark`; WPF capture x0 = web x 327, same y. Per section: page bg (13,20), card surface (card top + 15), card edge (x=0 vs x=327, card top + 40), brightest title glyph R (y 40-85), brightest body glyph R (card region), Brand-fill pixel count, and a token-presence diff over 29 dark token colours (a colour with >=40 px on one side and 0 on the other). Text colour is the brightest glyph pixel (WPF is greyscale AA; the web screenshots use LCD AA, so only the exact peak is compared).
Every section: bg `1C1A17/1C1A17`, card `24221E/24221E`, edge `34302A/34302A`, title peak `EC/EC` = ok. Body ink peak equal in all 36 (`EC` or the section's own peak: forms `F3` knob, form-modal/modal/loading/tabs `E0` brand, skeleton `B8`). Token-presence diff `none` in 33 sections; the three flagged ones:
| section | probe | web | wpf | verdict |
|---|---|---|---|---|
| progress | `5A5449` (Reset button border) | 160 px | 0 | not a token bug: WPF draws the border at a half pixel row, blended (light has the same split: `FAF9F7`/`DFDACF` instead of `D5CFC0`). Pre-existing light geometry gap, owner display/forms |
| data-table | `5A5449` (pager select border) | 146 px | 0 | same half-pixel blend (light: pager Previous top row `F3F1ED`/`FCFCFB`). Pre-existing, owner tables |
| stepper | `2A2723` | 0 | 1864 px | WPF 1px `Line` divider straddles two rows (`2A2723`+`2F2B25` instead of one `34302A`); light same (`F5F4F0`/`F0EDE6`, expected `E6E2D7`). Pre-existing, owner nav |
Other non-token geometry seen in dark and light alike: Tabs underline (`825642` split over 2 rows around the line, light `D2A694`), card bottom edges blended over 2 rows (`25221E`/`2A2620`; light `F4F1EC`/`EFEDE6`). All pre-existing half-pixel layout, not dark mapping. Light stays byte-identical, so left alone.
Flat-colour histogram (runs >=20 px, `Min 40`) per section: WEB-only list empty in 29/36 sections; where web has extra colours they are Shadow ramps (`201E1A`, `191815`, `1C1B17`, `131210`) which are the web box-shadow gradient, compared separately below. WPF-only entries are the WPF shadow ramp and the half-pixel blends above.
Raw tables: scratchpad `table-final.txt`, `hist-all2.txt` (not in repo).

## State captures (web `-Js` vs WPF `!` chains)
| state | probe | web | wpf | ok |
|---|---|---|---|---|
| Modal open | scrim over page bg | `0B0A09` | `0B0A09` | yes |
| Modal open | card fill / border | `24221E` / `34302A` | `24221E` / `34302A` | yes |
| Toasts success/info/error | fill | `1E2E25`/`1D2733`/`3A201D` | same | yes |
| Toasts | border | `2D4636`/`2E3F52`/`5A2F2A` | same | yes |
| Toasts | icon disc | `2F3E36`/`2E3743`/`49312E` | `303E36`/`2F3843`/`49312F` | yes (1/255, AA) |
| Searchable, multi, date, datetime, autocomplete open | panel fill/border | `24221E`/`34302A` | same | yes |
| Focus ring (Forms Full name, Autocomplete, trigger) | ring over Surface | `654637` | `654638` | yes (1/255 blend rounding of the 0x59 alpha) |
| Multi checkbox on | fill / tick | Brand `E08A66` / OnBrand | Brand / `1C1A17` | yes |
| Switch on | knob | `F3EFE7` | `F3EFE7` | yes (forms histogram, both present) |
| Picker selected cell | fill + label | Brand + OnBrand | Brand + OnBrand (`dateon.png`) | yes |
| Loading modal spinner | track / arc | `3A2820` / `E08A66` | same | yes |
| Tooltip | bubble | `ECE8DF` | `ECE8DF` | yes |
| Drawer / form-modal / command palette | panel fills | match | match | yes, except the cmdk scrollbar gap below |
Not capturable by `--capture`: ComboBox popups (Forms Role `CmbRole`, pager "25", column-filter selects, Sheet Category, modal-form selects). They are separate Popup HWNDs (not in RenderTargetBitmap) and a ComboBox cannot be opened by a `!` step. Code-reviewed instead: `FieldSelect`/`WebSelect`/`SheetSelect` popup Border is `Surface` + `LineStrong`/`Line`, `Shadow2`; items are all `FieldSelectItem` (Ink text, BrandSoft hover/selected) or the Sheet ComboBoxItem (Ink, BrandSoft), all DynamicResource. Action menu (`MenuActions`) lives in the OverlayLayer and is capturable, but its open chain `OverlayLayer!MenuActions` was not re-run in dark here; the menu uses the Phase 3-verified token styles. Not visually verified in dark: ComboBox popups, action menu.

## Shadows (Shadow1 / Shadow2), measured, no tuning
| probe | web | wpf | delta |
|---|---|---|---|
| primary button, +1 px below (Shadow1) | `131210` | `1C1A17` | 9 |
| primary button, +4 px below | `1E1D19` | `1F1D1A` | 1 |
| toast, +4 px left of the card (Shadow2) | `141310` | `171512` | 3 |
| toast, edge+1 / +4 below (between toasts) | `0C0B0A` / `0F0E0D` | `151814` / `12110F` | 9 / 3 |
| searchable-select panel, +1 / +4 / +11 below | `0D0C0A` / `0F0E0D` / `141310` | `161512` / `141311` / `1A1815` | 9 / 5 / 6 |
| modal, +1 / +4 below (on scrim) | `050504` / `060605` | `1A1917` (border AA) / `080706` | <=6 beyond +1 |
All within the plan's +-10 per channel at 4 px outside. Starting values kept: `Shadow1` Opacity 0.4 Blur 14 Depth 2, `Shadow2` Opacity 0.55 Blur 30 Depth 4, `KnobShadow` 0.5. (Web uses two stacked shadows; WPF one DropShadowEffect, so the +1 px edge is up to 9 lighter in WPF.)

## Chromium native dark checkbox (grid section, `.sheet__checkbox`, accent-color brand)
Measured in `web-dark-typed-sheet.png` (IsActive column) and WPF `Typed-grid-input.png` after the fix:
| part | web | wpf |
|---|---|---|
| unchecked border | `858585` | `858585` (SheetCheckBorder dark) |
| unchecked fill | `3B3B3B` | `3B3B3B` (new SheetCheckFill dark) |
| checked fill | `E08A66` (no border) | `E08A66` |
| tick | `3B3B3B` (core pixels) | `3B3B3B` (SheetCheckMark dark) |
Light unchanged: `SheetCheckBorder #FF767676`, `SheetCheckMark #FFFFFFFF`, `SheetCheckFill #00FFFFFF` (same as the old Transparent). Light byte gate confirms.

## Caret (plan step 5)
- `FieldInput` (Background Surface): caret `DBDDE1` in dark (`formsfocus.png` x44,y238) = visible, left as is per the plan.
- `BareInput` (Background Transparent, used by Autocomplete/TagInput/MultiSelect): caret was `000000` in dark (Autocomplete `acopen.png` pre-fix x44,y238) = invisible. Fixed with `CaretBrush=Ink` (post-fix `ECE8DF`).
- Same defect in two more transparent-background templates, fixed the same way (measured before/after by temporarily removing the setter): `SheetTextCell` grid cell caret `000000` -> `ECE8DF` (gridfocus-pre/post at x274,y101); `CmdkInput` caret column x157 `000000` -> `ECE8DF` (cmdk-caret-pre/post).
- Side effect: in light the caret of these three styles changes from black to `#26241F` (web caret = ink). Blinking caret, so no static capture or byte gate sees it.
Tab-through FocusVisualStyle: see known gaps.

## Known gaps (not fixed, recorded per Q2/Q3)
1. **Aero2 scrollbars on dark.** A default `ScrollViewer` scrollbar stays light (`CDCDCD` thumb, `F0F0F0` track) in: command-palette list (`OverlayControls.xaml:143`, `CmdkList`; the most visible, in `wpf-dark-states/cmdk.png`), drawer and modal-form bodies (`OverlayControls.xaml:193, 279`, only when content overflows), FileInput multi list (FileInput.cs:104, max 220), the page `ContentScroll` (Q3, accepted), the ScrollViewers inside the default ComboBox popups (`Forms.xaml:173`, `Controls.xaml:614`, `Sheet.xaml:301`, only if the list overflows 200-240) and the `ClaudeInput`/textarea content hosts (`Controls.xaml:135`, `Forms.xaml:206`). The web themes all scrollbars (`web/index.html:76-79`, thumb `line-strong`), so a counterpart exists. Fix = apply the existing `DropdownScrollBar` style (`Resources[typeof(ScrollBar)]`) to those ScrollViewers; that is a non-colour restyle that also changes light, so it is left for an `overlays`/`pickers` follow-up task rather than done here. Owner: overlays (cmdk, drawer, modal), pickers (FileInput), reviewer (page).
2. **Default dashed FocusVisualStyle** (black, invisible on dark) is still active on styles that do not set `FocusVisualStyle={x:Null}`. Grep-derived list (parents that set it, and BasedOn children of them, excluded): `SegmentButton` (includes the new theme toggle), `SortHeaderButton`, `TableAction`, `TablePageButtonActive`, `TableCell`/`TableRow` family (DataGridCell/Row), `TableThinThumb`, `FieldSelectItem`, `TextAreaGrip`, the implicit `ContextMenu`/`MenuItem` (ExtraControls), `ClaudeInput` (legacy). `--capture` does not draw keyboard-focus visuals and there is no Tab step in the capture driver, so this was NOT observed, only derived from the XAML; confirm manually on a visible desktop. Several of these (table cells, thumbs) may never take keyboard focus.
3. **Fractional-pixel lines** (stepper dividers, tabs underline, card bottom edges, secondary button borders in Progress/Data-table) straddle two pixel rows in WPF in light and dark alike; pre-existing layout, not theming. Owners: nav (stepper, tabs), display/forms (card/button edges), tables (pager).
4. ComboBox popups, FileInput with files, Shimmer: not rendered (see above).
5. `ThemeToggle` and the masthead are not parity-tracked; Windows title bar (Q4) is a manual check; Q3 page scrollbar accepted.
6. `-Theme system` run only exercised the dark branch on this machine (`AppsUseLightTheme=0`), carried over from Phase 1.

## Deviations from the plan
- Carry-over item 3 (spinner track) not changed: the plan/P3 premise ("web track is currentColor 45%") is wrong for the loading modal; the web uses `--brand-soft`, which WPF already matches (web wins).
- New key `SheetCheckFill` (plan allows "an off-state fill key if the web box is not transparent, in both dictionaries, light = current"); light value `#00FFFFFF` rather than the `Transparent` literal because a token must be a `SolidColorBrush` key; identical pixels.
- `CaretBrush=Ink` applied at style level (BareInput, plus SheetTextCell and CmdkInput) instead of in `FieldInput`/`BareInput` templates: FieldInput's caret is already visible (`DBDDE1`), the other two had the same invisible black caret and were measured before/after.
- No Shadow1/Shadow2 tuning (within +-10 everywhere measured).
- Web dark references and WPF dark captures were generated for all 36 sections plus 14 state shots; the per-section "table" is a measured summary (bg/card/edge/title/body/brand + token-presence diff), not 36x3 hand-read probes, because section layouts differ by a few px; each flagged difference was then probed by hand.
- Full suite run once more after one transient dropdown UIA flake (run 1: 456/1); not a repeat of the same failure, so the "no third retry" rule was not triggered.

## Blocking questions
None.
