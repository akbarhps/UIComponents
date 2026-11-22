# 08: split the shared test file and shrink the other shared files

Owner: reviewer (tooling). Implementer: one worker or the `implementer` agent, working alone while every component worker is idle. Phase reviews: `phase-reviewer`. Final review: `reviewer`, once.
Never commit, never kill by name, C# 5 only, and every app or test run goes through `run-hidden.ps1`. Use the build name `split` everywhere below.

## Why

- Up to six workers edit `wpf/tests/UIAutomationTests.cs` (1839 lines, one `Main`). Stale editor copies overwrite each other's assertions. One worker's half-edit also breaks every worker's test build, because `build-tests.bat` compiles that one file (`build-tests.bat:12`).
- Workers can only run the whole suite (~3-6 min). They cannot run just their own area.
- The same lost-edit problem, at a lower rate, hits `UIComponents.Wpf.csproj` (one entry per new file), `InputControls.xaml(.cs)` (dropdown and pickers are both active), `MainWindow.xaml.cs` and `wpf/README.md` (the "Covered: ... N assertions" line).

**Limit (stated plainly):** with one working tree and one csproj, a half-edit in *any* app file still breaks every worker's `build.bat`. Splitting app files only fixes overwritten and lost edits, not build breakage. Only the **test** build gets real isolation here, because a worker compiles just `Tests.Core.cs` plus its own area file. Worktrees remain the only full fix (deferred by the user).

## Assumptions

1. All workers are idle, nobody edits `wpf/` during phases 1-5, and the tree builds now.
2. `python` is on PATH (it was used during planning). The slicing script lives in the scratchpad, not the repo.
3. Line numbers below refer to `UIAutomationTests.cs` as of 2026-11-22 10:14. The script slices on the **anchor comments**, not on line numbers, and asserts that each anchor is found exactly once.
4. Archived owners (grid, forms, display; tables until its round 5 is sent) keep their area files. The reviewer edits those files when needed.

## Design decisions (with evidence)

- **One partial class, not one class per area.** Every helper is `private static` in `UIAutomationTests` (`Check` :20, `WaitFor` :27, `Find` :38 ... `ReadClipboardText` :214). C# 5 has no `using static`, so separate classes would force a `UIAutomationTests.` prefix on every helper call. With `internal static partial class UIAutomationTests` in each file, the blocks move verbatim.
- **Compiles with any subset of area files.** `Tests.Core.cs` declares one partial method per area, for example `static partial void RunNav(AutomationElement root, ref bool ran);`. C# 5 partial methods are void, implicitly private, allow `ref`, and the compiler drops the call when no file implements the method. Each area file implements its method and sets `ran = true;` as the first statement. A worker's build therefore needs only Core plus its own file, and another worker's broken area file cannot break it.
- **Locals do not collide when blocks are merged.** All blocks are in one method today and that method compiles. Any subset of its top-level statements, in any order, therefore compiles too, as long as nothing is used before it is declared. The only local that crosses blocks is `dateSection`: it is declared at :1067 and used at :1199/1203/1219/1223, and both blocks are pickers. Every other apparent hit from the planning scan was a word inside a string or comment.
- **Canonical run order** is each area's first appearance in today's `Main`: `nav, forms, tables, display, overlays, dropdown, pickers, grid, toasts`. `--only` always runs in this order, whatever order the names are given in.
- **App state that crosses areas** (code does not share it, the running app does):
  - overlays raises the "Deleted" (:718) and "Ready" (:764) toasts, which live 3.6 s. Toasts selects by name, so these do not collide today. `--only overlays,toasts` runs them back to back, and toasts' fix-list makes its dups check independent (driver-review #7).
  - Keyboard focus: every focus check sets focus itself first (:332, :664, :825, :910, :1179, :1681).
  - Open panels: pickers closes its own (:1183, :1194).
  - Leftover demo values stay inside the area that set them: FilterSearch "ada" :1341 (tables), tag "probe" :826 (dropdown), grid row/column added :1296-1299 (grid).
  - Rule from now on: an area leaves no dialog, panel or popup open when it exits, and never reads state another area set.
- **`-Section` is not used for tests.** `ApplyStandaloneMode` (`MainWindow.xaml.cs:94-119`) takes one title from `args[1]`. Only forms maps to a single section, and forms is archived. Every other area spans 2-7 sections. Standalone mode also changes what whole-root `FindByName` finds (the tables "Showing 1–5 of 5" check depends on another section's pager, see tables-final-review) and changes scroll position (`IsOffscreen` counts in S1/M1). Deferred; see "Not doing".

## Block → area map (anchor comment, current lines, owner)

| # | Anchor (start of the comment) | Lines | Area file |
|---|---|---|---|
| 1 | `// Buttons (06 N1)` | 254-308 | Nav |
| 2 | `// Text fields` | 310-325 | Forms |
| 3 | `// O0 probe (overlays)` | 327-340 | Forms (it uses only Forms fields and re-finds them; overlays' Tab-trap checks are in the modal block) |
| 4 | `// Forms — search` | 342-364 | Forms |
| 5 | `// DataGrid — verify rows` | 366-374 | Tables (`DemoGrid`, "Data table" section; README:32 "the table sections of MainWindow") |
| 6 | `// CheckBox — toggle` | 376-386 | Forms (`ChkNotifications`, MainWindow.xaml:239, Forms section) |
| 7 | `// Selection controls` | 388-417 | Forms |
| 8 | `// ComboBox — select` | 419-440 | Forms (`CmbRole`, MainWindow.xaml:179) |
| 9 | `// Cards & badges` | 442-463 | Display |
| 10 | `// Alerts + empty state` | 465-482 | Display |
| 11 | `// Progress bar` | 484-586 | Display |
| 12-15 | `// Slider`, `// TabControl`, `// Expander`, `// ListBox with multiple selection` | 588-640 | Nav (06-navigation.md:151, :492) |
| 16 | `// Modal dialog (07` | 642-725 | Overlays |
| 17 | `// Loading modal (07` | 727-770 | Overlays |
| 18 | `// Avatars (05-display` | 772-782 | Display |
| 19-22 | `// Breadcrumbs`, `// Stepper`, `// Tooltip button`, `// Action menu button` | 784-798 | Nav |
| 23 | `// Empty state` (the legacy one, line 800) | 800-805 | Display |
| 24 | `// Skeleton (05-display` | 807-815 | Display |
| 25-28 | `// Tag input`, `// Autocomplete`, `// Searchable select`, `// Multi-select — M1-M7` | 817-1058 | Dropdown |
| 29-31 | `// File input`, `// Date picker (07 P1)`, `// Date picker (07 P2)` | 1060-1239 | Pickers (keep all three in this order, because of `dateSection`) |
| 32-33 | `// Grid input — Phase 6`, `// Typed grid` | 1241-1336 | Grid |
| 34-37 | `// Filter table`, `// Selection table`, `// Column filters`, `// Sortable table` (incl. the code-only note 1484-1485) | 1338-1485 | Tables |
| 38 | `// Toasts T1/T2` | 1487-1731 | Toasts |
| 39 | `// Dialog launchers` | 1733-1735 | Overlays |
| Core | usings, class, helpers, `Main` up to `Drv.Init` | 1-252 | Tests.Core.cs |
| Core | the report lines (`Console.WriteLine(); Drv.Report(); RESULT ...`) | 1737-1741 | Tests.Core.cs |
| Core | `Drv`, `FastToggle`, `FastSelectionItem`, `FastExpandCollapse` | 1744-end (1842 at 10:15; the driver fixes landed then) | Tests.Core.cs |

Within each area file the blocks keep their original relative order. `"Empty state"` occurs twice as an anchor: match the legacy block by its exact text `// Empty state` on a line of its own (line 800). `// Alerts + empty state` is a different anchor.

## Phase 0: freshness check and baseline (no edits)

0. Nobody is mid-edit: `UIAutomationTests.cs` and `App.xaml.cs` were last written at 10:15 (the driver fixes). Their mtimes must not change during Phase 0, and `build.bat split` must be green.
1. Freshness: grep `UIAutomationTests.cs` for each worker's newest reviewed assertion name: `B1 loading disabled + Working…` (nav), `M7 Escape closes multi` (dropdown), `P2 month trigger found` (pickers), `focus restore to next close` (toasts), `backdrop click closes modal` (overlays), `sortable info text from matched count` (tables), `grid add column` (grid). **Stop and report** if any is missing.
2. Static count of `Check("` call sites: `python -c "import re;print(len(re.findall(r'Check\(\"',open('wpf/tests/UIAutomationTests.cs',encoding='utf-8').read())))"` → record N (311 at planning time). Use the same one-liner over all `Tests.*.cs` later. Don't use `grep` here: the rtk hook rewrites it and printed 0.
3. Build: `cmd /c "wpf\build.bat split"`, then `cd wpf\tests; cmd /c ".\build-tests.bat split"; cd ..\..`.
4. Run the full suite **twice**, with no other suites running: `pwsh -NoProfile -File wpf/tests/run-hidden.ps1 -Agent split`. After each run, copy `%TEMP%\uitest-split\tests.log` to `%TEMP%\split\base1.log` and `base2.log`.
5. Normalise each log to the sorted list of `^(PASS|FAIL)  <name>` with the detail after ` — ` removed:
   ```powershell
   function Names($f) { Get-Content $f | Where-Object { $_ -match '^(PASS|FAIL)  ' } | ForEach-Object { $_ -replace ' — .*$','' } | Sort-Object }
   ```
   First open `base1.log` once: the `cmd /c ... > log` redirect may write ` — ` and `…` as `?` or other bytes. Adjust the regex to the separator as it actually appears, so FAIL details (`gap=NaN`, `value=...`) are really stripped.
   `Compare-Object (Names base1.log) (Names base2.log)` gives the **flaky set**: the check names (status prefix stripped) whose status differed between the two runs. Expect toast timing checks only. Record the flaky set and the RESULT lines.
6. Captures for Phase 3 (byte-compare later):
   ```
   pwsh -NoProfile -File wpf/tests/run-hidden.ps1 -Agent split -NoTests -Capture "DatePickerCard=$env:TEMP\split\date-before.png"
   pwsh -NoProfile -File wpf/tests/run-hidden.ps1 -Agent split -NoTests -Section "File input" -Capture "Sections=$env:TEMP\split\file-before.png"
   pwsh -NoProfile -File wpf/tests/run-hidden.ps1 -Agent split -NoTests -Section "Date picker" -Capture "Sections=$env:TEMP\split\datesec-before.png"
   pwsh -NoProfile -File wpf/tests/run-hidden.ps1 -Agent split -NoTests -Capture "Sections=$env:TEMP\split\all-before.png"
   ```
   Also take one `-Section "<title>" Sections=` capture per existing host: "Tag input", "Grid input", "Table with column filters", "Toasts", "Modal dialog", "Forms".

## Phase 1: split the tests (a pure move: no assertion, guard or wait changes)

1. Scratchpad Python script. Read `UIAutomationTests.cs` as **bytes** and keep them: the file is UTF-8 **without** BOM with CRLF endings, and the assertions compare non-ASCII literals (`Uploading files…`, `Pick a date…`, `Showing 1–1 of 1`, the year-title en dash, `Working…`). Write every new file the same way: UTF-8, no BOM, CRLF. Find every anchor from the map (exactly once each, or abort), then write:
   - `wpf/tests/Tests.Core.cs`: lines 1-252, then the new runner (step 2), then lines 1737-1741, the closing brace of `Main` and of the class, then 1744 to the end of the file. Change `internal static class UIAutomationTests` to `internal static partial class UIAutomationTests`.
   - `wpf/tests/Tests.<Area>.cs` for Nav, Forms, Tables, Display, Overlays, Dropdown, Pickers, Grid, Toasts. Each contains the same `using` lines as Core, `namespace UIComponents.Wpf.Tests { internal static partial class UIAutomationTests { static partial void Run<Area>(AutomationElement root, ref bool ran) { ran = true; <blocks verbatim, original indentation is fine> } } }`.
   - Delete `UIAutomationTests.cs` (`git rm --cached` is not needed; just delete the file. Workers never commit, and the user commits later).
2. Runner in `Main` (Core), replacing the old block sequence. Keep the PID parsing as it is, with `args[0]` the PID when it is not `--only`:
   ```csharp
   static partial void RunNav(AutomationElement root, ref bool ran);   // ... one per area, same signature
   // in Main, after Drv.Init:
   string[] order = { "nav", "forms", "tables", "display", "overlays", "dropdown", "pickers", "grid", "toasts" };
   // --only a,b (case-insensitive). Unknown name -> "FAIL  unknown area x" and return 1.
   // For each area in order (filtered by --only): print "AREA  <name>", Stopwatch, switch(name) calling Run<Area>(root, ref ran),
   //   inside try/catch(Exception ex): on exception _failed++ and print "FAIL  area <name> threw " + ex.GetType().Name + ": " + ex.Message.
   //   ran == false -> _failed++ and print "FAIL  area <name> not compiled into this exe".
   //   print "AREA  <name> done in N.N s".
   // After the loop: if (_passed + _failed == 1) { print "FAIL  no checks ran"; _failed++; }   // only "window found" ran
   ```
   The `try/catch` only adds a FAIL line on a crash, so a clean run keeps the same check names. At startup, list the compiled areas with `typeof(UIAutomationTests).GetMethod("Run" + Cap(name), BindingFlags.NonPublic | BindingFlags.Static) != null` and print `AREAS compiled: ...` (unimplemented partial methods are removed from metadata).
   Arg parsing: PID is `args[0]` if present and not `--only`. `--only` is followed by a comma list.
3. `wpf/tests/build-tests.bat`: usage `build-tests.bat [agent] [area ...]`. No area → compile `Tests.*.cs`. Areas → compile `Tests.Core.cs` plus `Tests.<area>.cs` for each. cmd splits `a,b` into separate arguments, so loop with `shift`:
   ```bat
   set OUT=bin
   if not "%~1"=="" set OUT=bin\%~1
   set SRC=Tests.*.cs
   shift
   if "%~1"=="" goto build
   set SRC=Tests.Core.cs
   :areas
   if "%~1"=="" goto build
   set SRC=%SRC% Tests.%~1.cs
   shift
   goto areas
   :build
   ```
   Then the existing `csc` line with `%SRC%` instead of `UIAutomationTests.cs`. Exe name and `bin\<agent>\` stay the same. Update the usage comment.
4. `wpf/tests/run-hidden.ps1`:
   - Add the `[string]$Only = ''` parameter.
   - Line 100: append `" --only $Only"` to the test command when `$Only` is set.
   - Line 47 staleness: compare against the newest `wpf\tests\Tests.*.cs` (`Get-ChildItem ... | Sort-Object LastWriteTime | Select-Object -Last 1`).
   - Usage comment: add `-Only dropdown`.
   - Do not pass `-Section` to the test app.

**Verify (all must hold):**
- `grep -o 'Check(' wpf/tests/Tests.*.cs | wc -l` equals N from Phase 0.
- `diff` the concatenated area bodies against the original lines 254-1735: the script prints the line count of each area, and their sum plus the blank separators equals 1482.
- `build-tests.bat split` (all files) compiles. `build-tests.bat split dropdown` compiles with only Core + Dropdown.
- Full run: `Names` equals the base logs, except names in the flaky set. RESULT failed is no higher than the worse baseline.
- Each area alone: `run-hidden.ps1 -Agent split -Only <area>` for all 9 (build the full exe first). Remove `window found` from each per-area list (Core runs it once per run), concatenate the 9 lists, add one `window found`, sort. The result equals the full-run list, flaky set excepted.
- `build-tests.bat split dropdown` + `-Only nav` prints `FAIL  area nav not compiled` and exits 1. `-Only nope` prints `FAIL  unknown area nope` and exits 1.

## Phase 2: csproj wildcards + MainWindow.xaml.cs partials (pure moves)

1. Before editing: `Remove-Item -Recurse wpf\obj\split, wpf\bin\split` (your own folders only), `build.bat split`, then record `Get-ChildItem -Recurse wpf\obj\split -Include *.baml,*.g.cs | ForEach-Object Name | Sort-Object` → `%TEMP%\split\obj-before.txt`.
2. `wpf/UIComponents.Wpf.csproj`: replace both item groups (lines 33-160) with:
   ```xml
   <ItemGroup>
     <ApplicationDefinition Include="App.xaml" />
     <Page Include="*.xaml" Exclude="App.xaml" />
     <Page Include="Themes\*.xaml" />
   </ItemGroup>
   <ItemGroup>
     <Compile Include="*.cs" />
     <Compile Include="Properties\AssemblyInfo.cs" />
   </ItemGroup>
   ```
   These wildcards are non-recursive, so `obj\<agent>\*.g.cs`, `bin\` and `tests\*.cs` are not picked up. MSBuild 4.0 static-item wildcards and `Exclude` work, and `Generator`/`SubType`/`DependentUpon` are designer-only metadata. Today every root `.cs`/`.xaml` is already listed (checked with `ls wpf`), so the item set is unchanged.
3. Split `wpf/MainWindow.xaml.cs` (move members verbatim, add the same `using` lines):
   - `MainWindow.Display.cs`: `_progressTimer`, `_progressRandom`, `_progressValue`, `ProgressStart_Click`, `ProgressTimer_Tick`, `ProgressReset_Click`, `SetProgress`, `ApplyReducedMotion` (lines 13-67, 86-91).
   - `MainWindow.Forms.cs`: `MessageGrip_DragDelta` with its ponytail comment (121-127).
   - Keep in `MainWindow.xaml.cs`: constructor, `ApplyStandaloneMode`, `ProjectRow`, `StandaloneHelper`. Both new files are `public partial class MainWindow`, like `MainWindow.Navigation.cs:11`.

**Verify:** clean rebuild (delete `obj\split`, `bin\split` first); the obj list equals `obj-before.txt` (the MainWindow partials add no generated files); the full suite `Names` match Phase 1, flaky set excepted.

## Phase 3: move pickers out of InputControls + self-registering standalone hosts

1. New `wpf/PickerControls.xaml`: a UserControl with the same header as `InputControls.xaml:1-5`. The root is `<StackPanel x:Name="SectionList">` with no MaxWidth, same as InputControls. Move `InputControls.xaml` lines 119-168 (the "File input" and "Date picker" sections) into it verbatim.
2. New `wpf/PickerControls.xaml.cs`: `public partial class PickerControls : UserControl`, a constructor with `InitializeComponent()`, and `Browse_Click` moved from `InputControls.xaml.cs:72-77`, with the `using Microsoft.Win32;`. Remove `Browse_Click` and any using it alone needed from InputControls.xaml.cs.
3. `MainWindow.xaml`: directly after `<local:InputControls x:Name="FormInputs"/>` (line 903), add `<!-- Pickers -->` and `<local:PickerControls x:Name="Pickers"/>`. The page order stays identical.
4. `ApplyStandaloneMode` (`MainWindow.xaml.cs:100-115`): replace the hard-coded panel array and the five Visibility lines with discovery, so a future host needs no edit here:
   ```csharp
   var panels = new List<Panel> { Sections };
   var hosts = new List<UserControl>();
   foreach (UIElement child in Sections.Children)
   {
       var host = child as UserControl;
       var list = host == null ? null : host.FindName("SectionList") as Panel;
       if (list != null) { hosts.Add(host); panels.Add(list); }
   }
   // found-check and StandaloneHelper.Apply loop unchanged (over panels)
   foreach (var host in hosts)
       host.Visibility = StandaloneHelper.Contains((Panel)host.FindName("SectionList"), key) ? Visibility.Visible : Visibility.Collapsed;
   ```
   Hosts are direct children of `Sections` (MainWindow.xaml:903-915), and `StandaloneHelper.Apply(Sections, key)` already collapses them as non-title children. The explicit Visibility loop then re-shows the matching host, as the old lines did.

**Verify:**
- Repeat every Phase 0 step 6 capture into `*-after.png`; each must be byte-identical (`(Get-FileHash a).Hash -eq (Get-FileHash b).Hash`). If one is not, compare pixels; any difference blocks the phase.
- The full suite `Names` match.
- `-Section "Toast variants"` and `-Section "Modal dialog"` captures are still found, which proves discovery covers the old hosts.

## Phase 4: Core test defects (reviewer area; expected delta: 0 checks)

From `handoff/driver-review.md`, client side only. The `App.xaml.cs` items are a separate driver task, not this plan. #1 and the client half of #2 are already done (10:15): `Send` throws on timeout (`UIAutomationTests.cs:1789-1790`) and `Do` throws on result 3 (:1804). Read `Send`/`Do`/`Init` first, and leave the driver-off path and the empty-id fallback (`select:<no id>`) alone.
1. #10 in `Drv.Send` (:1782): write the separator as `"\t"` instead of a literal TAB character (same character, no protocol change).
2. #14 `Drv.Init`: retry `ping` (catch the timeout exception) every 100 ms for up to 2 s before printing `DRIVER off`.

**Verify:** the full suite `Names` match, the DRIVER fast/fallback counts are unchanged, and `build-tests.bat split grid` compiles.

## Phase 5: docs and worker notes (plan/ and READMEs only)

1. `plan/wpf/README.md`:
   - "Who owns what": replace the `UIAutomationTests.cs` row with these rows:
     - `wpf/tests/Tests.Core.cs` (helpers, `Drv`/`Fast*`, `Main` runner): reviewer
     - `wpf/tests/Tests.<Area>.cs`: that area's owner (Nav nav, Forms forms, Tables tables, Display display, Overlays overlays, Dropdown dropdown, Pickers pickers, Grid grid, Toasts toasts)
     - `PickerControls.xaml(.cs)`: pickers
     - `MainWindow.Display.cs`: display
     - `MainWindow.Forms.cs`: forms
     - `MainWindow.xaml.cs` (`ApplyStandaloneMode`): reviewer
     - `App.xaml.cs` (driver and `--capture`): reviewer
   - Pickers' row: replace "the File input and Date picker / Date & time picker sections of InputControls" with `PickerControls.xaml(.cs)`.
   - Hard constraint 2: `.cs`/`.xaml` files in `wpf\` and `wpf\Themes\` are compiled automatically by wildcard, so no csproj edit is needed. Never leave a scratch `.cs` or `.xaml` file there. A new resource dictionary still needs its line in `App.xaml` (merge order matters for `StaticResource`).
   - Rule 4 commands:
     - per phase: `.\build-tests.bat <name> <area>` + `run-hidden.ps1 -Agent <name> -Only <area>`
     - before the final report of a task, **or** after editing anything outside your own files (shared styles, `App.xaml`, `MainWindow.xaml`): `.\build-tests.bat <name>` (all files, which also proves your helper names don't collide) + a full run
   - New section "Writing tests" (replaces the stale "Test speed rule" line 136, using driver-review "Docs"):
     - Edit only your own `Tests.<Area>.cs`. Never edit `Tests.Core.cs`; ask for a helper in your report.
     - Private helpers in your file are prefixed with your area (`ToastsFind`, `PickersOpen`). The partial class shares one member namespace, and a clash only shows up in the full build.
     - Act only through `Invoke`, `SetValue`, `FastToggle.Of`, `FastSelectionItem.Of`, `FastExpandCollapse.Of` and `Drv.Focus`. Never call a pattern's action or `SetFocus()` directly.
     - Every element a test acts on has an AutomationId unique across the window and its panels. Add no new `DRIVER fallback` lines.
     - Assert focus with `WaitFor(() => el.Current.HasKeyboardFocus)`, never `AutomationElement.FocusedElement`. For a negative check, also assert where focus is.
     - `Invoke` is asynchronous: never read state right after it without `WaitFor`. No fixed `Thread.Sleep` for state changes.
     - Find the section card once and search inside it.
     - Leave no dialog, panel or popup open when your area exits. Never depend on state another area set.
     - Never set `UITEST_DRIVE` outside run-hidden (`Drv.Focus` calls `Window.Activate()`).
     - Keep the `// Covered:` line at the top of your file current. It replaces the README count.
   - Add one line: "A `UIAutomationTests.cs:NNN` reference in a task file or review now means: find the assertion by name in `Tests.<Area>.cs`." This covers 03/06/07 and the handoff reviews without editing them.
2. `plan/wpf/REVIEWER.md`: in the command block, `build-tests.bat reviewer` stays (full). Add `-Only <area>` and the Phase 0 `Names` compare snippet as "test acceptance after a refactor". Board row: "08 partial tests: done".
3. `wpf/README.md`:
   - tests section: `build-tests.bat [agent] [area ...]`, `UIAutomationTests.exe <pid> [--only a,b]`
   - replace the "Covered: ... 265 assertions" line with "Coverage: the `// Covered:` header of each `tests/Tests.<Area>.cs`; the count is the `RESULT` line of a full run"
   - Notes for agents: "add a matching test in your `tests/Tests.<Area>.cs`"
   - Structure table: `tests/Tests.Core.cs` + `tests/Tests.<Area>.cs`
4. Add a `// Covered: ...` comment line at the top of each area file, taking each area's part of the old README Covered line. Comments only.
5. Write `plan/wpf/handoff/tests-split.md` (the common note, text below). Append each per-worker note below to the end of its fix-list file, headed "Applies after task 08 (tests split) is done".

**Verify:** no file outside `plan/`, `wpf/README.md` and the comment lines of `wpf/tests/Tests.*.cs` changed in this phase. The full build compiles.

## Common note for every worker (`handoff/tests-split.md`)

```
Tooling change (task 08), read before your next phase:
- UIAutomationTests.cs is gone. Your assertions are in wpf/tests/Tests.<Area>.cs (yours: see below). Edit only that file; never Tests.Core.cs.
- Build: cd wpf\tests && .\build-tests.bat <name> <area> && cd ..\..   (compiles Core + your file only; other workers' half-edits can't break it)
- Run: pwsh -NoProfile -File wpf/tests/run-hidden.ps1 -Agent <name> -Only <area>
- Before your final report of a task, or after editing a file outside your own (shared styles, App.xaml, MainWindow.xaml): .\build-tests.bat <name> (all files) and a full run without -Only.
- Review/plan references "UIAutomationTests.cs:NNN": find the assertion by name in your Tests.<Area>.cs.
- New csproj: .cs/.xaml files in wpf\ and wpf\Themes\ compile automatically; no csproj edits. Never leave scratch files there.
- If build.bat, or the full build-tests.bat <name>, fails in a file you don't own (including another worker's Tests.<Area>.cs): wait a minute, rebuild, then report it. Never edit that file.
- Test-writing rules: plan/wpf/README.md "Writing tests".
```

## Per-worker notes (append to the named file in Phase 5)

- **dropdown** → `handoff/dropdown-next2.md`: area `dropdown`, file `Tests.Dropdown.cs`. Fix 1 (M7 vacuous Escape) and the Shift+Tab nit are in its Multi-select / Searchable select blocks. Phase 4 tag-input tests go in the same file (Tag input block). `InputControls.xaml` now holds only your four sections; the File input and Date picker sections moved to `PickerControls.xaml`.
- **toasts** → `handoff/toasts-next.md`: area `toasts`, file `Tests.Toasts.cs`. Fixes 1-3 and the nits are there. Also apply `handoff/driver-review.md` #7: resume wait = `3600 - (focusAt - shownAt) + 800` plus a "not gone after remaining-400" lower bound; the dups block keeps only toasts with an id number above the highest `ToastN` seen before the two Invokes; `WaitFor(..., 1000)` before the first read after each `Invoke`. Also #15: reword CODE-ONLY to "(driver off)". F2 now edits `MainWindow.Display.cs` (`ProgressTimer_Tick`), not `MainWindow.xaml.cs`.
- **overlays** → `handoff/overlays-next.md`: area `overlays`, file `Tests.Overlays.cs` (Modal dialog, Loading modal, Dialog launchers). Fix 2's backdrop test uses `PostMouseClick`/`RectPart` from Core. The O0 probe moved to `Tests.Forms.cs`: leave it. Coordinate AdvancedControls' "Dialogs" removal (O4-O6) **after** tables round 5, not at the same time (both edit `AdvancedControls.xaml(.cs)`).
- **pickers** → `handoff/pickers-next.md`: area `pickers`, file `Tests.Pickers.cs`. Fixes 3 (Today reopen asserts), 5 (`PostMouseClick` on `PickDate_Day14`), 6 (`IsKeyboardFocusable`) and 10 are there. Fix 1's rows are now in `PickerControls.xaml`, not `InputControls.xaml:140-141`. `Browse_Click` is in `PickerControls.xaml.cs`. P3's Date & time picker section goes into `PickerControls.xaml`.
- **nav** → `handoff/nav-next.md`: area `nav`, file `Tests.Nav.cs`. Fixes 2 (B1 test) and 4 (BtnDisabled, COMException) are in its Buttons block. The legacy Slider/TabControl/Expander/ListBox/Breadcrumbs/Stepper/Tooltip/Action menu blocks are also in `Tests.Nav.cs`; replace them there in N2-N6 (06-navigation.md:151 line refs → this file). You are the only writer of `MainWindow.xaml` until N7; tell the reviewer before any edit outside the nav sections.
- **tables** → `handoff/tables-next.md`: area `tables`, file `Tests.Tables.cs` (the DataGrid block is there too). Blocking 1 (sortable info text) and nit 8 are there. Send this round before overlays starts O4-O6.
- **display** (archived) → `handoff/polish.md`, display items: the EmptyState duplicate and the progress-restart read are in `Tests.Display.cs` (expected delta −1 check when the duplicate goes).
- **grid** (archived) → `handoff/polish.md`, new line: driver-review #7 add-row / add-column reads (`grid add row`, `grid add column`) need `WaitFor` in `Tests.Grid.cs`.
- **forms** (archived): nothing open; the O0 probe lives in `Tests.Forms.cs`.

## Sequencing

1. Phases 0-5 in order by one implementer, with all workers idle. Stop after each phase for a `phase-reviewer` check against this file. A phase whose `Names` compare fails is not done.
2. The `reviewer` runs once after Phase 5: the full build, one full run (`Names` vs baseline), `-Only` for two areas, and a partial build.
3. Only then resume workers: prepend `handoff/tests-split.md` to each next prompt, together with the note already appended to its fix-list file.
4. Order the two AdvancedControls writers: tables round 5 first, then overlays O4-O6.

## Risks / do not touch

- **Build breakage is not solved for the app** (see "Why"). Rule: wait, rebuild, report; never fix a file you don't own.
- **Phase 1 must stay a pure move.** No fixes, guards or waits, even obvious ones (e.g. `Find(dateSection, ...)` at :1199 outside the null guard). Defects go to the owners' fix-lists with their expected deltas.
- Toast checks are timing-flaky under load. Run the comparisons with no other suite running, and judge only against the flaky set from two baselines.
- With the wildcard csproj, any stray `.cs`/`.xaml` in `wpf\` or `wpf\Themes\` gets compiled. Never add `**` (recursive) patterns: they would pick up `obj\<agent>\*.g.cs`.
- Phase 3 is the only phase that can change pixels. Byte-identical captures are the gate.
- Do not touch: `App.xaml` (merge order is load-bearing; dropdown-next.md item 5), `Themes/*.xaml`, `AdvancedControls`, `DataControls`, `MainWindow.xaml` beyond the one host line, `App.xaml.cs`, any product behaviour, `web/`.

## Not doing (and when to revisit)

- **Splitting `MainWindow.xaml` into per-area UserControls:** nav is its only active writer through N7, and moving six sections mid-task would invalidate nav's line refs. Revisit when the sidebar/masthead task gives it a second active writer.
- **Splitting `Themes/Controls.xaml`:** a misplaced `StaticResource` key fails only at runtime, and the main writers (forms, tables) are archived. Revisit if two active workers need it.
- **`App.xaml` glob or `_All.xaml`:** merge order matters, so a glob would break it. It's already one append-only line per new theme.
- **`AdvancedControls` / `DataControls` splits:** solved by ordering (tables round 5, then overlays) and archived owners (grid).
- **`-Section` for test runs:** to add later, `ApplyStandaloneMode` would accept `"A|B"` and run-hidden would pass `-Section` to the test app. Do it only if a measurement (PROFILE findMs and wall time for `-Only grid` with and without it) shows a real gain **and** that area's `Names` are identical both ways.
- **Driver app-side items** (driver-review #2-#6, #8, #11): a separate reviewer task.
