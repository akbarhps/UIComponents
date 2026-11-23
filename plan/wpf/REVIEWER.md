# Planner / reviewer playbook (WPF pixel parity)

> **Resuming?** Read the latest `plan/wpf/handoff/SESSION-*.md` (currently `SESSION-2026-11-22d.md`) first: session state, commit plan and decisions since the last board update. `HANDOFF.md` is stale.

You are the **planner and reviewer**, not a worker. Work is done by `implementer` agents, one task phase at a time. Your job:
1. Write task plans in `plan/wpf/NN-*.md`.
2. Write the prompt for each `implementer` run.
3. Review each phase report against the web and give a verdict plus the next prompt.
4. Fix shared **tooling** yourself when it blocks the workflow (build, test, capture scripts). Leave component code to the workers.

Read `plan/wpf/README.md` (rules every worker follows) and the task files before doing anything.

**The `implementer` agent runs the tasks.** Give it a task file (`plan/wpf/NN-*.md`) and one phase at a time, check its report in `plan/wpf/handoff/`, then review it with `phase-reviewer` (multi-phase plans) or `reviewer` (final). The OpenCode workers are retired.

## Goal

`wpf/` (.NET Framework 4.8 WPF, C# 5) must look and behave like `web/index.html` (the source of truth) within ±1px and with exact colours. Glyph rasterisation differences are accepted.

## Status board (update this table at the end of every session)

| Task | Agent | Done | Next |
|---|---|---|---|
| 01 grid input (`SheetGrid`) | `grid` | DONE, committed (bebf113 or earlier) | — |
| 02 forms | `forms` | DONE, committed (bebf113 or earlier) | — |
| 03 dropdown family (searchable select, multi-select, tag input, autocomplete) | `dropdown` | DONE, committed (bebf113 or earlier) | — |
| 04 tables | `tables` | DONE, committed (bebf113 or earlier) | — |
| 05 display (cards, badges, alerts, avatar, skeleton, progress, empty state) | `display` | DONE, committed (bebf113 or earlier) | — |
| 06 navigation and buttons (buttons, action menu, accordion, stepper, breadcrumbs, tooltips, tabs) | `nav` | DONE, committed (bebf113 or earlier) | — |
| 07 overlays, pickers, toasts (incl. file input) | `overlays`, `pickers`, `toasts` | DONE, committed (bebf113 or earlier) | — |
| web bug fixes (`web/index.html`, A1-A12, B1, C1-C9) | `web` | DONE, committed f4c5faa | — |
| 08 partial tests (split `UIAutomationTests.cs`, in-process driver, csproj wildcards) | `reviewer` | DONE, committed (bebf113 or earlier) | — |
| 09 dark mode (light/dark/system theming, `ThemeManager`, DynamicResource sweep, parity with web ba418c1) | `theme` | DONE, not committed (Phases 0-5; Q1-Q4 = plan defaults) | Final `reviewer` pass (once). Manual on a visible desktop: title bar dark; System follows a live Windows theme switch; reload keeps the stored choice. Gaps (accepted, see `wpf/COMPONENTS.md` "Theme"): Aero2 scrollbars light in dark, default dashed focus visuals invisible on dark, ComboBox popups / action menu / FileInput-with-files / Shimmer not visually verified in dark |
| rest (anything not in 01-09) | — | not planned | group into similar sets, one task file each |

Dependencies:
- All satisfied.

## The loop

1. **Plan.** Orient first:
   - read the web CSS, markup and JS for the component
   - read the current WPF code
   - capture web references, and dump geometry (recipe in README)

   Then write `NN-name.md`, following the shape of 01/02/04:
   - why the work is needed
   - the files
   - the visual spec with numbers
   - the behaviour spec, listing every JS branch
   - the AutomationIds
   - small phases, each with captures and acceptance criteria
   - a reviewer checklist

   One component group per file. Workers handle one thing at a time poorly, so keep phases small.
2. **Prompt.** Use the templates below. Always give the worker its agent name and say "Stop after Phase N".
3. **Review** the worker's report:
   - Read the diff (`git diff <files>`) and the new code for C# 5, scope (only the files listed), dead code and duplicated mechanisms.
   - Capture it yourself, hidden. Don't trust the worker's screenshots alone.
   - Compare against the `web-*` reference by eye **and** with pixel measurements (snippets below).
   - Test-failure attribution: failures in another agent's area at that moment are not this worker's fault.
4. **Verdict.** Approved, or "fix X first". Then the next-phase prompt, with the fixes and answers to the worker's questions folded in.

Ask the user only for decisions that are really theirs to make: touching another agent's file, a web quirk to replicate or fix, scope changes. Use `AskUserQuestion` and recommend an option.

## Which agent reviews what (don't mix these up)

- **After each worker phase:** `phase-reviewer` (Sonnet 5.5). Never `reviewer` here.
- **Once, after all phases of a task are done:** `reviewer` (Opus 5.5).
- Planning: `planner`. Lookups: `reader`. Never `general-purpose` as a stand-in.
- "reviewer build" below is the build folder name `wpf\bin\reviewer\`, unrelated to the agent.

## Commands you use (reviewer build is `reviewer`)

```powershell
# build and run the tests hidden (nothing on the user's screen)
cmd /c "wpf\build.bat reviewer"
cd wpf\tests; cmd /c ".\build-tests.bat reviewer"; cd ..\..
pwsh -NoProfile -File wpf/tests/run-hidden.ps1 -Agent reviewer

# partial areas (workers' loop; Core + the listed areas only). Build and run the same list. Unlisted areas print "not compiled" when run without -Only
cd wpf\tests; cmd /c ".\build-tests.bat reviewer nav grid"; cd ..\..
pwsh -NoProfile -File wpf/tests/run-hidden.ps1 -Agent reviewer -Only nav,grid

# test acceptance after a refactor: names of two logs (base = before, new = after), status prefix kept, detail stripped
function Names($f) { Get-Content $f | Where-Object { $_ -match '^(PASS|FAIL)  ' } | ForEach-Object { $_ -replace ' — .*$','' } | Sort-Object }
Compare-Object (Names base1.log) (Names new.log)   # empty = same checks; flaky toast timing excepted

# WPF capture, hidden: Id or x:Name; !Click chains; @Focus. Several captures = ONE comma-joined string
pwsh -NoProfile -File wpf/tests/run-hidden.ps1 -Agent reviewer -NoTests -Section "Grid input" -Capture "InputGrid!InputGrid_row1=$env:TEMP\rv\sel.png"
pwsh -NoProfile -File wpf/tests/run-hidden.ps1 -Agent reviewer -NoTests -Section "Data table" -Capture "Sections=$env:TEMP\rv\dt.png"
# dark capture: -Theme is explicit (default light; "none" = real stored choice, persistence check only)
pwsh -NoProfile -File wpf/tests/run-hidden.ps1 -Agent reviewer -NoTests -Theme dark -Section "Data table" -Capture "Sections=$env:TEMP\rv\dt-dark.png"
# theme lint (all checks; must print RESULT theme-lint failed=0)
node plan/wpf/theme-lint.mjs

# web reference: sections by id, optional -Js run after load (click, focus…)
pwsh -NoProfile -File plan/wpf/capture-web.ps1 -Sections data-table -Out plan/wpf/ref/web-table-data-table.png
```

The section titles used by `-Section` are the `SectionTitle` TextBlock texts. List them with `Select-String -Path wpf\*.xaml -Pattern SectionTitle`.

Pixel checks, in PowerShell with System.Drawing:
- the first dark pixel in a row range gives the text x-offset
- a column scan at a fixed x finds border rows
- `GetPixel` gives exact colours

To crop a web shot to match a WPF capture: the web content column starts at x≈314 (sidebar 248 plus padding), and `.demo` cards are 984 wide with a 31px inner offset.

## Prompt templates

**Kickoff (new task or worker).** The workflow, report format, ownership and gotchas are all in `README.md`, so the prompt stays short:
```
You are the worker agent "<name>" on the WPF pixel-parity effort (repo C:\Workspace\Projects\others\ui-design).
Read plan/wpf/README.md fully (rules, worker workflow, ownership, gotchas), then plan/wpf/NN-x.md and the web lines it cites.
Do Phase N only, then stop and report in the README format.
Done criteria: build.bat <name> OK; run-hidden.ps1 -Agent <name> passes; captures: <exact run-hidden -Capture command>; self-check: <acceptance numbers>.
```

**Review then next phase:**
```
Review of Phase N: approved | fix first.
Fixes: 1. <file:line> <what> <why>  2. ...
Answers: <the worker's questions>
Then Phase N+1 of plan/wpf/NN-x.md: <key branches or specs to stress>.
Rules unchanged: agent "<name>", build.bat <name>, run-hidden.ps1 only, never commit, never kill by name.
Done criteria: <tests>, <captures with exact command>, <acceptance numbers>. Stop after Phase N+1.
Report: files changed, build/test summary, capture paths + diffs, deviations, blocking questions.
```

**Onboarding an agent that worked without a plan:** ask for a factual report (scope by web section id, files and shared styles, commits, parity evidence, behaviour, tests, work in progress, problems) with no code changes. Then review it and write a fix-up task file, like `04-tables.md`.

## Decisions already made (don't re-litigate)

- **Page width:** WPF content `MaxWidth` changed 1000→984 (user-approved). The capture renders at 1400×1100, the same as the web refs.
- **Grid:**
  - `createGrid` was ported as one code-only `SheetGrid` control, not a restyled `DataGrid`.
  - A selected select cell hides its chevron. That's a web CSS quirk, and we replicate it.
  - Copy toast deferred: the WPF toast is private to `AdvancedControls`.
  - **Ctrl-clicking an already-selected cell leaves just that cell selected.** That's correct web behaviour: the toggle removes it, then focusin clears the rest and reselects it.
  - Accepted: up to 2px column-boundary rounding in `SheetGrid`, and a 3px toolbar text offset (font metrics).
- **Forms:**
  - `ClaudeInput` stays untouched (others restyle it). `FieldInput` is the web-accurate replacement, and later tasks migrate to it.
  - `FieldLabel`, `FieldHint` and `ClaudeCheckBox` are fixed in place.
  - The textarea grip handler lives in `MainWindow.xaml.cs` with a `ponytail:` note.
  - The selection checkbox in tables must reuse `ClaudeCheckBox`.
  - A plain `Border` has no UIA peer, so cards that need an AutomationId get a chrome-free `GroupBox` wrapper (`FormsCard`). `--capture` also matches `x:Name`.
- **Tables:**
  - The web's `.table__num` is **left-aligned**, because of a CSS specificity quirk. We replicate it.
  - The first column of the data, column-filter and sortable tables is a `<th scope="row">`, styled like a header.
- **Shared styles:** `SheetSectionDesc` (15/24 section description) and `DemoCard` (radius 16, no shadow) in `Themes/Sheet.xaml` are the shared section shell for all tasks.
- **Serif token:** `Palatino Linotype, Book Antiqua, Georgia, Times New Roman, serif`.
- **Workers never commit.** The user commits at stable points. (`eba14b6` swept other agents' files in with `git add -A`. Don't let that happen again.)
- **Dark mode: overlay dictionary.** `Themes/Tokens.Dark.xaml` has the same colour key set as `Tokens.xaml`. `ThemeManager` merges it last to switch and removes it for light. Light values never change, so light mode keeps the same resource graph.
- **Dark mode: rejected approaches (do not retry).** Mutating `SolidColorBrush.Color` in place (style and template sealing freezes the setter brushes, so it throws). Reloading dictionaries or recreating windows (loses state; the web keeps state). `DynamicResource` inside a Freezable property (effects swap as whole keyed objects instead).
- **Dark mode: persistence (Q1).** `HKCU\Software\UIComponents.Wpf`, value `ui-theme`; absent = System. `StorageKey` can take a consumer subkey or `null`. The `UI_THEME` env override never reads or writes storage, so tests cannot persist a choice.
- **Dark mode: accepted gaps (Q2/Q3).** Scrollbars, default focus visuals, ComboBox popups, action menu, FileInput-with-files and Shimmer. Full list in `wpf/COMPONENTS.md` "Theme". Restyling them is a follow-up task: the scrollbar fix also changes light.

## Tests: split layout, driver, open items (task 08)

- **Layout.** `wpf/tests/Tests.Core.cs` has the helpers, `Drv`/`Fast*` and the `Main` runner. `Tests.<Area>.cs` (nav, forms, tables, display, overlays, dropdown, pickers, grid, toasts) hold the checks; each is a partial method that `Core` calls. Canonical run order is that list. Areas compile with any subset: a missing one prints `FAIL  area <x> not compiled into this exe`.
- **Running.** `run-hidden.ps1 -Agent <name> -Only a,b` (passed to the exe as `--only`). `build-tests.bat <name> <area ...>` compiles Core plus those areas. Full build and full run are the reviewer's: run both after every worker phase report that touches tests or shared files, and always before any commit. Each area must pass alone; `-Only` on an area is fast (`-Only grid` ~15 s; see the README speed rule).
- **Commit and worktrees.** Worktrees were considered for test isolation and deferred: the user chose partial tests. Limit: app builds still share one tree and one csproj, so a half-edit in any app file breaks every worker's `build.bat`. Per-worker branches or worktrees: revisit later. Workers never commit.
- **Driver.** `run-hidden.ps1` sets `UITEST_DRIVE=1`; `App.xaml.cs` `DriverHook` installs an in-process WM_COPYDATA driver (`verb\tid\targ`, results 1 done / 2 not resolved, fall back to real UIA / 3 threw in app). Test actions go through `Drv`; reads stay on UIA. `UITEST_NODRIVE=1` forces real UIA. Output: `DRIVER  fast=N fallback=M` and one `fallback <verb>:<id> xN` line per fallback.
- **Known semantic gaps.** Driver actions run inside an inter-process SendMessage (`RPC_E_CANTCALLOUT_ININPUTSYNCCALL` risk for TSF/clipboard/OLE, `driver-review.md` #5). Focus succeeds on the hidden desktop where real UIA SetFocus could not, so two toast focus tests now run instead of printing CODE-ONLY (#6).
- **Open driver items** (`handoff/driver-review.md`; client half of #1 and #2 and #14 done; app side not done, separate reviewer task):
  - fallbacks: `select:<no id>` x4 (item-peer mapping, #4) and `toggle:RowCheck` x2 (identity check and rect, #3). Still fall back to the slow path.
  - `Find` calls cost ~80 s of a ~174 s suite (PROFILE `finds`/`findMs`). Unprofiled walks (#16) are not counted.
  - optional verbs: `setrange` (slider), `scroll` (grid scroll); item-peer mapping for #4.
- **Known failing checks.** `toasts`: "auto-dismiss resumes when focus leaves" fails in every full run (`handoff/toasts-next.md`). `pickers`: "P10 outside click closes" fails with `-Only pickers` alone (`handoff/pickers-next.md`); it passes in the full run only because of earlier areas' state.
- **Acceptance after a refactor.** Compare check names before and after with the Names snippet in the command block (Phase 0 of `08-partial-tests.md`); `-Only` runs union to the full run's list, except the known failures above.

## Ownership map

The map lives in `README.md` under "Who owns what", so there's only one copy. Update it there when a new agent or area appears. Your own area is `wpf/build.bat`, `wpf/tests/run-hidden.ps1`, `App.xaml.cs` (driver and `--capture`), `plan/wpf/*`, and `wpf/tests/Tests.Core.cs` (`Main` runner, `Drv`).

## Tooling facts and gotchas (learned the hard way)

- **Hidden desktop:** `run-hidden.ps1` uses `CreateDesktop`, so UIA tests work there and nothing steals the user's focus. WPF doesn't *paint* on a hidden desktop, though, so screen captures there come out blank. That's why captures go through `--capture` (RenderTargetBitmap of the window content, cropped to the element).
  - Don't use a `VisualBrush` for this: it offset the crop.
- **Isolation:** each agent builds into `wpf\bin\<name>\` (`build.bat <name>`), and the tests target a PID (`UIAutomationTests.exe <pid>`). Agents used to kill each other's windows by process name, and the tests used to drive another agent's window.
- **The clipboard is shared with the user's session,** even from the hidden desktop. Copy/paste checks must save and restore it.
- **Mouse input can't reach the hidden desktop.** Drag and other mouse behaviour is verified by code review plus a quick manual check by the user.
- **`pwsh -File` with an array parameter** passes `"a","b"` as one comma-joined string. Scripts split on commas, and you pass several captures as one string.
- `build-tests.bat` uses relative paths, so run it from `wpf\tests`.
- **WPF quirks:**
  - The TextBox content host adds about 2–3px of inset, so compensate in the template margins and measure the typed text, not the box.
  - `TextBox.Padding` is applied twice in custom templates.
  - `ClipToBounds` on a rounded `Border` clips to a *rectangle*. Use a `RectangleGeometry` clip with radius 11, updated on `SizeChanged`.
  - `Border.CornerRadius` doesn't clip children.
- **Edge headless:** `--screenshot` ignores `#anchor`. `capture-web.ps1` hides the other sections instead. For geometry, use `--dump-dom` with an injected `getBoundingClientRect` script (README).
- **Displays:** 1920×1080 at 100% (96 DPI), so 1 CSS px = 1 DIP. The user has an always-on-top game window, which is another reason captures must not be screen grabs.
- **Wildcard csproj:** new `.xaml` and `.cs` files in `wpf\` and `wpf\Themes\` are picked up automatically. Do not add entries: a duplicate Page can break the markup compile. C# 5 means no `$""`, `?.`, `nameof`, `=>` members or auto-property initialisers.

## Planning the next groups

- Group by shared primitives. Plan the primitives first, then the components built on them.
- Each plan starts with a review of the current WPF state, because most components already have a rough port.
- Capture `wpf-before-*` and `web-*` references, plus a measurements file, into `plan/wpf/ref/` before writing the specs.
