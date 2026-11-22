# Review: 08-partial-tests Phases 3 and 4

Verdict: approved. No blocking and no should-fix findings.

## Blocking
none

## Should-fix
none

## Nit
- wpf/tests/Tests.Core.cs:312: `(ms / 1000.0).ToString("0.0")` uses the current culture, so logs print `0,0 s` (see split-p4-only-negative.txt:8, split-p3-full.txt `41,3 s`). Cosmetic, from Phase 1. Optional: pass `CultureInfo.InvariantCulture`.
- wpf/tests/Tests.Core.cs:342-354: with the driver unset the run now waits the full 2 s before `DRIVER off` (ping returns 0 immediately, so it is retried until the deadline). This is expected by the plan. Only the 15 s timeout path throws, and that is swallowed and then exits the loop because the deadline has passed. No fix needed.
- wpf/PickerControls.xaml:3: `xmlns:sys` is unused. The plan said to copy the InputControls header, so leave it.

## Phase 3 verification (by reading, no builds)
- Verbatim move: git has no copy of the pre-phase InputControls.xaml (HEAD predates the pickers worker's Date picker section), so a text diff against the old lines 119-168 is impossible. Substitute evidence:
  - Strings extracted from `obj/split/InputControls.baml` (the Phase 2 build, 10:32) are a subset of those in `obj/split3/InputControls.baml` plus `PickerControls.baml`. Nothing was lost.
  - Generated fields and handlers match: `SectionList`, `FileList`, `Browse_Click` wiring and `RemoveTag_Click` on the InputControls side.
  - AutomationIds (BtnBrowse, FileList, DatePickerCard, PickDate, PickMonth, PickYear) are intact in PickerControls.xaml.
  - 10 of 10 captures are byte-identical, and the full suite has the same 307 names and 306 passed, 1 known FAIL.
- Resources: `SectionTitle`, `ClaudeCard`, `SecondaryButton`, `DemoCard`, `SheetSectionDesc` and `FieldLabel` resolve from the app-level dictionaries, the same as in InputControls. The baml strings show them, and the build and run succeed.
- Namespace and class: `x:Class="UIComponents.Wpf.PickerControls"` matches the code-behind. `Browse_Click` is in PickerControls.xaml.cs, and InputControls.xaml.cs no longer has it or `using Microsoft.Win32`.
- Name collisions: `x:Name="Pickers"` is unique in the repo. `Overlays`, `ToastDemos`, `FormInputs`, `DataTables` and `AdvancedTables` are the other hosts. The csproj uses wildcards and its mtime (10:51) is from Phase 2, so it is untouched.
- Discovery (MainWindow.xaml.cs:41-56): all six direct children of `Sections` (FormInputs, Pickers, DataTables, AdvancedTables, ToastDemos, Overlays) have `<StackPanel x:Name="SectionList">`. `FindName` on a UserControl uses its own namescope, which is filled by the host's `InitializeComponent` inside `MainWindow.InitializeComponent`, so it works from the constructor. Non-UserControl children are skipped.
  - Panel order (`Sections` first, then the lists), the found-check and the `Apply` loop are unchanged.
  - The Visibility rule per host is the same expression as the old lines (`Contains(list, key)` gives Visible, otherwise Collapsed). It now also runs for ToastDemos and Overlays, whose old explicit lines are replaced by the loop. Pickers is new.
  - `Apply(Sections, key)` collapses the hosts first, then the loop re-shows the matching one, as before.
- `ContentScroll.Padding = (48,8,48,24)` is not a Phase 3 change: the same IL constants exist in `bin/split` and `bin/split2` and are absent of `48,40,48,48`.
- Extra standalone DatePickerCard capture (~15k px, max channel diff 166, 984x232): the explanation is credible but unproven, because the comparison is standalone vs full-mode with no standalone baseline. Supporting points:
  - It is a different layout (standalone padding, the card's fractional row heights 73.8 and 95.8, a different Y offset), so text anti-aliasing shifts by about 1 px.
  - `datesec-after` (the standalone column containing the same card) is byte-identical to its Phase 0 baseline, and the full-mode `date-before` is identical too.
  - Repeat runs are stable.
  - A real change would have to alter both the full-mode capture and the standalone column capture, and neither moved.
  - Optional decisive check (one pair, if wanted): run the pre-phase exe with `-Agent split2` (`bin\split2`, built 10:51 from the Phase 2 tree) and the same `-NoTests -Section "Date picker" -Capture "DatePickerCard=<png>"`, then hash it against `standalone-datecard-after.png`. Equal means the shift is layout only.

## Phase 4 verification (wpf/tests/Tests.Core.cs)
- Line 373: separator is `"\t"` and the file contains 0 literal TAB characters.
- Init (339-356): retry loop with 100 ms sleep and a 2 s deadline, catching only `InvalidOperationException` (what `Send` throws on timeout). The driver-off message and the `UITEST_NODRIVE` path are unchanged, and `Send`/`Do` are untouched.
- Line 244: trailing `--only` fails with a message before the window lookup (evidence in split-p4-only-negative.txt). Line 250 keeps `args[0] != "--only"` for the PID. `unknown area` and `not compiled` also exit 1.
- C# 5: no `$""`, `?.`, `nameof`, `=>` members or `out var` in the file.
- Bytes: Tests.Core.cs is UTF-8 without BOM with CRLF (433 lines, 0 bare LF), like Tests.Pickers.cs and Tests.Dropdown.cs. PickerControls.xaml(.cs) are LF without BOM, like InputControls.xaml(.cs) and MainWindow.xaml.cs (git `i/lf w/lf`).
- Evidence: the P4 full run has 306 passed and 1 failed (the known FAIL), the same 307 names, and DRIVER fast=126 fallback=6, the same as Phase 3.
