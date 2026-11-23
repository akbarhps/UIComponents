# WPF pixel-parity plan

Goal: every WPF component in `wpf/` should look and behave the same as its `web/index.html` counterpart. The web version is the source of truth.

Roles:
- **Planner/reviewer** writes one task file per component group, then reviews the worker's diff and screenshots against it.
- **Worker** does one task file at a time, one phase at a time, and stops after each phase so it can be reviewed.

## Worker workflow (every phase)

1. Your prompt gives you an **agent name** and a **task file and phase**. Do that phase only, then stop. Don't start the next phase on your own.
2. Before coding, read the task file, the web lines it cites (CSS, markup, JS) and the current WPF code you'll change.
3. **The web wins.** If the task file disagrees with `web/index.html`, follow the web and report the conflict.
4. **Ask instead of guessing** when something blocks you, such as needing a file outside your scope.
5. Build with `wpf\build.bat <name>`. Test and capture with `wpf/tests/run-hidden.ps1` (rule 4). Self-check your captures against the `web-*` references and the acceptance numbers in the task file.
6. Report back in this shape:
   1. the files you changed, with a one-line reason each
   2. a build and test summary: pass/fail counts, failures quoted exactly, and whose area each failure is in
   3. capture paths, plus every remaining difference from the web with measured pixel numbers
   4. deviations from the plan, and why
   5. blocking questions
   **Where it goes (saves reviewer tokens):** write the full report above to `plan/wpf/handoff/<name>-report.md` (overwrite each round). Your final chat message is only a header of at most 12 lines: (a) the verbatim `RESULT passed=N failed=M` line from your `-Only <area>` run, (b) files changed, comma-separated, (c) new `DRIVER fallback` lines or "none", (d) deviations, one line each or "none", (e) "Blocking questions: yes/no" plus "full report: plan/wpf/handoff/<name>-report.md". Do not paste the full report into chat.
7. Don't commit (rule 5). The reviewer replies with approved / fix-first and the next-phase prompt.

## Who owns what

Edit only your own area. If you need a change in someone else's area, ask in your report.

| Area | Owner |
|---|---|
| `SheetGrid.cs`, `Themes/Sheet.xaml`, the Grid input / Typed grid sections of `DataControls` | `grid` |
| `Themes/Forms.xaml`; the `FieldLabel`, `FieldHint` and `ClaudeCheckBox` styles; the Forms section of `MainWindow.xaml` | `forms` |
| The `Table*`, `StatusBadgeTemplate`, `SegmentButton`, `SortHeaderButton`, `PinButton` and `WebSelect` styles; `TablePagination`; the table sections of `MainWindow`, `DataControls` and `AdvancedControls` | `tables` |
| `Themes/Display.xaml`, `Shimmer.cs`; the Cards, Badges, Alerts, Avatar, Skeleton, Progress and Empty state blocks of `MainWindow.xaml` | `display` |
| `Themes/Dropdown.xaml`, `DropdownPrimitives.cs`, the per-control files of task 03; the Tag input, Autocomplete, Searchable select and Multi-select sections of `InputControls` | `dropdown` |
| `Toasts.cs`, `Themes/Toasts.xaml`, `ToastControls.xaml(.cs)`; the Toast variants and Toasts sections; removing the old toast block from `AdvancedControls` | `toasts` |
| `OverlayHost.cs`, `Themes/Overlays.xaml` (incl. `IconButton`), `OverlayControls.xaml(.cs)`; removing the four `*Window` classes, MainWindow's "Overlays" block and `AdvancedControls`' "Dialogs" block | `overlays` |
| `CalendarPicker.cs`, `FileInput.cs`, `Themes/Pickers.xaml`, `PickerControls.xaml(.cs)` (File input, Date picker and Date & time picker sections) | `pickers` |
| `wpf/build.bat`, `wpf/tests/run-hidden.ps1`, `App.xaml.cs` (`--capture`), `plan/wpf/*` | reviewer |
| `wpf/tests/Tests.Core.cs` (helpers, `Drv`/`Fast*`, `Main` runner) | reviewer |
| `wpf/tests/Tests.<Area>.cs` | that area's owner: Nav `nav`, Forms `forms`, Tables `tables`, Display `display`, Overlays `overlays`, Dropdown `dropdown`, Pickers `pickers`, Grid `grid`, Toasts `toasts`. Edit only your own file |
| `PickerControls.xaml(.cs)` | `pickers` |
| `MainWindow.Display.cs` | `display` |
| `MainWindow.Forms.cs` | `forms` |
| `MainWindow.xaml.cs` (`ApplyStandaloneMode`) | reviewer |
| `App.xaml.cs` (in-process driver and `--capture`) | reviewer |
| `wpf/ThemeManager.cs`, `Themes/Tokens.xaml`, `Themes/Tokens.Dark.xaml`, `plan/wpf/theme-*.{ps1,mjs}` (task 09) | reviewer |

Reuse shared styles; don't copy them. `SheetSectionDesc` and `DemoCard` (`Themes/Sheet.xaml`) are the section shell for every component, and `FieldInput` (`Themes/Forms.xaml`) is the text input. `ClaudeInput` is legacy: don't change it.

## WPF gotchas (all learned the hard way)

- A TextBox content host adds about 2–3px of inset. Compensate in the template margin, then measure where the **typed text** lands, not where the box is. Custom templates also apply `TextBox.Padding` twice.
- `ClipToBounds` on a rounded `Border` clips to a rectangle, and `CornerRadius` never clips children. For real rounded clipping, give the content a `RectangleGeometry` clip (radius = outer radius − border) and update it on `SizeChanged`. `SheetGrid.UpdateClip` is an example.
- A plain `Border` or `Panel` has no UIA peer, so an AutomationId on it can't be found. Wrap it in a chrome-free `GroupBox` (see `FormsCard`). `--capture` also finds elements by `x:Name`.
- Set fixed row and control heights from the measurements. Don't rely on padding plus font metrics: WPF and Chromium line heights differ by about 0.5px.
- Use `UseLayoutRounding` and `SnapsToDevicePixels` for crisp 1px lines, and `FocusVisualStyle={x:Null}` wherever the web draws its own focus.
- `pwsh -File script.ps1 -Param "a","b"` arrives as **one** comma-joined string. Pass several captures as one comma-separated string.
- The clipboard is shared with the user's session, even from the hidden desktop. Any copy/paste check must save the clipboard text first and restore it afterwards.
- OS mouse input does not reach the hidden desktop: posted `WM_LBUTTON*` is ignored, and `SendInput` would go to the user's screen, so never use it. Presses go through `Drv.Click` / `Drv.ClickAt` (see "Writing tests"). Drag and other gesture-only behaviour is still code-only unless a test drives it that way; say so in your report.
- `build-tests.bat` uses relative paths, so run it from `wpf\tests`.
- Colour = `{DynamicResource K}` or `SetResourceReference`, never `StaticResource` or a `Res()`/`FindResource` snapshot (`node plan/wpf/theme-lint.mjs`, checks C3 and C5).
- Captures and tests run light unless you pass `-Theme dark` (`run-hidden.ps1 -Theme` sets `UI_THEME`, so the OS theme and a stored choice never leak in).
- In dark, text is the **bright** pixel. Invert any "first dark pixel" check from the older light-mode notes.

## Task files

| # | File | Scope | Status |
|---|------|-------|--------|
| 01 | [01-grid-input.md](01-grid-input.md) | Grid input + Typed grid input (`createGrid`) | in progress |
| 02 | [02-forms.md](02-forms.md) | Forms section: input, search, select, textarea, error, checkbox, radio, switch | ready (after 01) |
| 03 | [03-dropdown.md](03-dropdown.md) | Dropdown family: searchable select, multi-select, tag input, autocomplete | ready (agent `dropdown`) |
| 04 | [04-tables.md](04-tables.md) | Fix-up of the existing table port: data, row selection, filters, column filters, sortable | ready (agent `tables`) |
| 05 | [05-display.md](05-display.md) | Cards, badges, alerts, avatar, skeleton, progress, empty state (fix-up audit) | in progress (agent `display`) |
| 09 | [09-dark-mode.md](09-dark-mode.md) | Light, dark and System theme: tokens, DynamicResource sweep, `ThemeManager` | done, uncommitted; final review pending (agent `theme`) |
| … | — | everything else | later |

## Hard constraints (apply to every task)

1. **C# 5 only.** The csproj sets `LangVersion 5` and builds with `C:\Windows\Microsoft.NET\Framework\v4.0.30319\MSBuild.exe`, so these are not available:
   - `$"..."` interpolation, `?.`, `??=`, `nameof`
   - `=>` expression-bodied members, auto-property initialisers (`{ get; set; } = x`)
   - `out var`, tuples, pattern matching, local functions

   Use `string.Format` / `+` concatenation and full property bodies instead.
2. **No csproj edits for new files.** The csproj uses wildcards: every `.xaml` in `wpf\` (except `App.xaml`) and `wpf\Themes\` is a Page, and every `.cs` in `wpf\` is a Compile. **Do not add `<Page Include>` or `<Compile Include>` entries**: a duplicate Page can break the markup compile. Never leave a scratch `.cs` or `.xaml` in `wpf\` or `wpf\Themes\`. A new resource dictionary still needs its merge line in `wpf/App.xaml` (merge order matters).
3. **Other agents edit this repo too.** Only touch the files the task lists. Don't restyle shared styles (`ClaudeCard`, `ClaudeButtonBase` and so on) unless the task explicitly says to.
4. **Never show the app on the user's screen. Run everything on a hidden desktop.** The user keeps working while you test. Every agent has a name: `grid`, `forms`, `tables`, and so on (use the one in your prompt).
   - **Build your own copy:**
     ```
     wpf\build.bat <name>
     cd wpf\tests && .\build-tests.bat <name> <area> && cd ..\..     (per phase, your area only; it uses relative paths, so it must run from wpf\tests; see "Writing tests")
     ```
     If `build.bat <name>` or `build-tests.bat <name> <area>` fails in a file you don't own (including another worker's `Tests.<Area>.cs`): wait ~60 s, rebuild, and report it. Never edit that file.
     `build.bat <name>` writes to `wpf\bin\<name>\`. Never build into `bin\Debug`. `build-tests.bat <name>` writes `wpf\tests\bin\<name>\UIAutomationTests.exe`, which `run-hidden.ps1 -Agent <name>` runs: always pass your name, never use the shared `bin\UIAutomationTests.exe` (another agent running it locks it).
   - **Run the tests hidden:** `pwsh wpf/tests/run-hidden.ps1 -Agent <name> -Only <area>`. It starts your exe on a private hidden desktop (`uitest-<name>`), runs the UI Automation tests against that PID, prints the PASS/FAIL lines, closes only that PID, and returns the test exit code. Windows, dialogs, popups and focus changes stay on that desktop.
     - At most 4 suites run at once machine-wide (`wpf/tests/test-slot.ps1`; `UITEST_SLOTS` changes it). If you see `waiting for a free test slot...` the script is queued behind other agents, not stuck: wait in the same turn (the 600s timeout starts only when your run starts). `-NoTests` captures are not limited.
   - **Capture hidden:** `pwsh wpf/tests/run-hidden.ps1 -Agent <name> -NoTests -Section "<Section title>" -Capture "<Id>=<out.png>,<Id>@<FocusId>=<out2.png>"`.
     - The app renders each element off-screen at 1400x1100 and 96 DPI (`--capture` in `App.xaml.cs`). Text is greyscale, like the headless-Edge web references.
     - `@<FocusId>` focuses that element first, for focus-state shots.
     - `!<ClickId>` clicks a button (or toggles a checkbox or toggle button) first. You can chain several, and they run in order. For example, `InputGrid!InputGrid_row1=...` captures with row 2 selected.
      - Steps run left to right: `~Down|~Up|~Enter|~Escape` raises PreviewKeyDown + KeyDown on the focused element, `^text` sets the focused TextBox's text; e.g. `ComboCard!ComboAssignee_Trigger!^eng!~Down=...` (`!` separates every step). `^text` cannot contain `!`, `@`, `=` or `,`.
     - `-Capture` also accepts an element's `x:Name`. `Sections` is the whole content column in standalone mode.
     - Pass several captures as **one** comma-separated string.
   - **Don't** launch the exe directly on the user's desktop. Don't use `capture-wpf.ps1` or UIA `SetFocus` against a visible window. Both steal the user's screen and focus.
   - **Never** run `taskkill /im UIComponents.Wpf.exe`, `Stop-Process -Name UIComponents.Wpf`, or anything else that kills by name.
   - Before a phase is done, your area (`-Only <area>`) passes; known failures listed in `handoff/tests-split.md` excepted. Report failures in other areas, do not fix them.
5. **Never `git commit`.** The user commits at stable points.
6. **Fidelity target:** geometry and colours within ±1px / exact hex. Glyph rasterisation differs between Chromium and WPF (sub-pixel shape and hinting). Do **not** chase glyph shape. Do chase font family, size, weight, colour and position.

## Visual comparison workflow

The display runs at 100% scale (96 DPI), so 1 CSS px = 1 WPF DIP. Keep the WPF window on a 100% monitor while capturing.

```powershell
# Web reference: show only the given sections (window 1400x1100, scale 1)
pwsh plan/wpf/capture-web.ps1 -Sections sheet-input,typed-sheet -Out plan/wpf/ref/web-grid-input.png
#   optional -Js "..." runs after load, e.g. to click a header or focus a cell

# WPF actual: hidden desktop, rendered off-screen, nothing on the user's screen
pwsh wpf/tests/run-hidden.ps1 -Agent grid -NoTests -Capture "InputGrid=plan/wpf/ref/wpf-grid-input.png,InputGrid@InputGrid_r0_c1=plan/wpf/ref/wpf-grid-focus.png"
#   -Section "Forms" starts the app in standalone mode (faster, only that section)
#   a "CAPTURE FAIL" line means the AutomationId wasn't found in that build
```

`plan/wpf/capture-wpf.ps1` (a visible-window screen capture) is kept only for the reviewer. Workers use `run-hidden.ps1`.

Re-measuring web geometry for a new component: add `-Js` to dump rects, or run Edge with `--dump-dom` against the same injected page. This is the script used for `ref/web-measurements.txt`:

```js
const r=e=>{const b=e.getBoundingClientRect();return [b.x,b.y,b.width,b.height].map(v=>Math.round(v*10)/10).join(',')};
const out=[];
['sheet-input','typed-sheet'].forEach(id=>{const s=document.getElementById(id);
  out.push(id+' demo '+r(s.querySelector('.demo'))+' scroll '+r(s.querySelector('.sheet__scroll')));
  out.push(' thead: '+[...s.querySelectorAll('thead th')].map(r).join(' | '));
  s.querySelectorAll('tbody tr').forEach(tr=>out.push(' tr: '+[...tr.children].map(r).join(' | ')));
});
document.body.innerHTML='<pre id=o>'+out.join('\n')+'</pre>';
```

```powershell
& 'C:\Program Files (x86)\Microsoft\Edge\Application\msedge.exe' --headless=new --disable-gpu --force-device-scale-factor=1 --window-size=1400,1100 --dump-dom <file:///…injected.html>
```

Leave the WPF captures (`plan/wpf/ref/wpf-*.png`) in place after each phase so the reviewer can compare them side by side with the `web-*.png` files.

## Test speed rule (profiled 2026-11-22)
The full suite takes ~430 s wall (measured 2026-11-22, `-Agent implementer`, 534 checks, while other agents ran; the earlier ~167 s figure was from a quieter run). Per-area runs: `-Only grid` took 15 s wall (measured; other areas not measured). Nobody needs the full suite except the reviewer: run your `-Only <area>`. Find each section container ONCE and search inside it; cache elements you reuse; never search from the window root for something inside a known section. Use a poll-until helper, not fixed sleeps.
Pattern actions go through the in-process driver (see "Writing tests"): do not add more actions than a check needs, prefer one action per state, and do not Invoke just to re-read.

## Writing tests (task 08)

**The driver.** `run-hidden.ps1` sets `UITEST_DRIVE=1`. The app then installs an in-process driver (`App.xaml.cs` `DriverHook`), and test actions go through it (milliseconds instead of ~2 s per action). Reads (`Current.*`, `Find*`) still go through real UIA. `UITEST_NODRIVE=1` in the test process forces real UIA for everything. Run it once per task before sign-off: the driver is not an accessibility check.

**Layout.** `wpf/tests/Tests.Core.cs` holds the helpers, `Drv`, the `Fast*` helpers and the `Main` runner (reviewer only). `wpf/tests/Tests.<Area>.cs` holds one area each (your file is in the README table above). Edit only your own file. Ask for a helper in your report instead of editing `Tests.Core.cs`.

**Rules:**
- Act only through `Invoke(el)`, `SetValue(el, v)`, `FastToggle.Of(el)`, `FastSelectionItem.Of(el)`, `FastExpandCollapse.Of(el)` and `Drv.Focus(el)`. Never `el.SetFocus()` or `((XPattern)el.GetCurrentPattern(...)).Action()`.
- Mouse presses: `Drv.Click(x, y)` (screen point) and `Drv.ClickAt(el, dx, dy)` (offset from an element's top-left) press through the app's real hit test (`WindowFromPoint`, `VisualTreeHelper.HitTest`), then raise Preview/bubble mouse-button events on the hit element, or on the window root if nothing is hit, so window-level hooks such as outside-click see the same `OriginalSource` as a real press. They do not move the cursor or change focus. Result: 1 = element hit, 4 = fell through to the window, 2 = not delivered, -1 = driver off (`Drv.Enabled` is false: print CODE-ONLY). Use them for mouse-only behaviour; keyboard and focus still use `Drv.Focus` and `PostKey`. Examples: `Pickers` M1-M3. Limit: a press does not raise `Button.Click` (ButtonBase needs real `IsMouseOver`), so check button handlers with `Invoke`.
- Every element a test acts on has an AutomationId (or `x:Name`) that is unique across the window and its popups. Duplicates and missing ids silently fall back to the slow path.
- Add no new `DRIVER fallback` lines. This is a done criterion.
- `Invoke` is asynchronous, for real UIA too. Never read state right after it: `WaitFor(() => ..., 1000-2000)`. No fixed `Thread.Sleep` for state changes.
- Assert focus with `WaitFor(() => el.Current.HasKeyboardFocus)`, never `AutomationElement.FocusedElement`. For a negative check, also assert where focus is.
- Find the section card once and search inside it. Never search from the window root for something inside a known section.
- Private helpers in your file are prefixed with your area (`ToastsFind`, `PickersOpen`). The partial class shares one member namespace, and a clash only shows in the full build.
- Leave no dialog, panel or popup open when your area exits. Never depend on state another area set.
- Never set `UITEST_DRIVE` outside run-hidden (`Drv.Focus` calls `Window.Activate()`).
- Keep the `// Covered:` header at the top of your file current. It replaces the README count.
- A `UIAutomationTests.cs:NNN` reference in a task file or review now means: find the assertion by name in `Tests.<Area>.cs`.

**Commands:**
- Your phase: `cd wpf\tests`, `.\build-tests.bat <name> <area>`, `cd ..\..`, then `pwsh -NoProfile -File wpf/tests/run-hidden.ps1 -Agent <name> -Only <area>`. Per-area timings are in the speed rule above (`-Only grid` ~15 s wall); each area must pass alone.
- Always pass `-Only`. A partial build prints `FAIL  area <x> not compiled into this exe` for every area it does not contain.
- Full build and full run (`build-tests.bat <name>` with no area, then `run-hidden.ps1` with no `-Only`): reviewer only.
- Shared files (`App.xaml`, `MainWindow.xaml`, shared styles): a change there is tested by your own `-Only` runs. The reviewer runs the full suite.
- The stale-build warning from `run-hidden.ps1` can appear for a partial build when another worker saves a test file. Ignore it only right after you rebuilt your partial exe this phase. No `AREAS compiled:` line in the output means the exe is pre-split: rebuild with `build-tests.bat <name> <area>`.
- Output: `DRIVER  in-process actions enabled` (or `off`), `PROFILE finds=N findMs=M`, `DRIVER  fast=N fallback=M`, and one `DRIVER  fallback <verb>:<id> xN` line per fallback. These are in the run-hidden output and in `%TEMP%\uitest-<agent>\tests.log`. Check them before reporting done.
- Known failures and their owners: `handoff/tests-split.md`.
