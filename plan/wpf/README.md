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
7. Don't commit (rule 5). The reviewer replies with approved / fix-first and the next-phase prompt.

## Who owns what

Edit only your own area. If you need a change in someone else's area, ask in your report.

| Area | Owner |
|---|---|
| `SheetGrid.cs`, `Themes/Sheet.xaml`, the Grid input / Typed grid sections of `DataControls` | `grid` |
| `Themes/Forms.xaml`; the `FieldLabel`, `FieldHint` and `ClaudeCheckBox` styles; the Forms section of `MainWindow.xaml` | `forms` |
| The `Table*`, `StatusBadgeTemplate`, `SegmentButton`, `SortHeaderButton`, `PinButton` and `WebSelect` styles; `TablePagination`; the table sections of `MainWindow`, `DataControls` and `AdvancedControls` | `tables` |
| `wpf/build.bat`, `wpf/tests/run-hidden.ps1`, `App.xaml.cs` (`--capture`), `plan/wpf/*` | reviewer |
| `wpf/tests/UIAutomationTests.cs` | shared: edit only your own assertions. `Main()` belongs to the reviewer |

Reuse shared styles; don't copy them. `SheetSectionDesc` and `DemoCard` (`Themes/Sheet.xaml`) are the section shell for every component, and `FieldInput` (`Themes/Forms.xaml`) is the text input. `ClaudeInput` is legacy: don't change it.

## WPF gotchas (all learned the hard way)

- A TextBox content host adds about 2–3px of inset. Compensate in the template margin, then measure where the **typed text** lands, not where the box is. Custom templates also apply `TextBox.Padding` twice.
- `ClipToBounds` on a rounded `Border` clips to a rectangle, and `CornerRadius` never clips children. For real rounded clipping, give the content a `RectangleGeometry` clip (radius = outer radius − border) and update it on `SizeChanged`. `SheetGrid.UpdateClip` is an example.
- A plain `Border` or `Panel` has no UIA peer, so an AutomationId on it can't be found. Wrap it in a chrome-free `GroupBox` (see `FormsCard`). `--capture` also finds elements by `x:Name`.
- Set fixed row and control heights from the measurements. Don't rely on padding plus font metrics: WPF and Chromium line heights differ by about 0.5px.
- Use `UseLayoutRounding` and `SnapsToDevicePixels` for crisp 1px lines, and `FocusVisualStyle={x:Null}` wherever the web draws its own focus.
- `pwsh -File script.ps1 -Param "a","b"` arrives as **one** comma-joined string. Pass several captures as one comma-separated string.
- The clipboard is shared with the user's session, even from the hidden desktop. Any copy/paste check must save the clipboard text first and restore it afterwards.
- Mouse input can't reach the hidden desktop. For mouse-only behaviour (drag and so on), port it faithfully, say "code-only" in your report, and the user will check it by hand.
- `build-tests.bat` uses relative paths, so run it from `wpf\tests`.

## Task files

| # | File | Scope | Status |
|---|------|-------|--------|
| 01 | [01-grid-input.md](01-grid-input.md) | Grid input + Typed grid input (`createGrid`) | in progress |
| 02 | [02-forms.md](02-forms.md) | Forms section: input, search, select, textarea, error, checkbox, radio, switch | ready (after 01) |
| 03 | — | Dropdown family: searchable select, multi-select, tag input, autocomplete | later |
| 04 | [04-tables.md](04-tables.md) | Fix-up of the existing table port: data, row selection, filters, column filters, sortable | ready (agent `tables`) |
| … | — | everything else | later |

## Hard constraints (apply to every task)

1. **C# 5 only.** The csproj sets `LangVersion 5` and builds with `C:\Windows\Microsoft.NET\Framework\v4.0.30319\MSBuild.exe`, so these are not available:
   - `$"..."` interpolation, `?.`, `??=`, `nameof`
   - `=>` expression-bodied members, auto-property initialisers (`{ get; set; } = x`)
   - `out var`, tuples, pattern matching, local functions

   Use `string.Format` / `+` concatenation and full property bodies instead.
2. **Old-style csproj.** Every new `.xaml` file needs a `<Page Include>` entry in `wpf/UIComponents.Wpf.csproj`, and every new `.cs` file needs a `<Compile Include>` entry. A new resource dictionary must also be merged in `wpf/App.xaml`.
3. **Other agents edit this repo too.** Only touch the files the task lists. Don't restyle shared styles (`ClaudeCard`, `ClaudeButtonBase` and so on) unless the task explicitly says to.
4. **Never show the app on the user's screen. Run everything on a hidden desktop.** The user keeps working while you test. Every agent has a name: `grid`, `forms`, `tables`, and so on (use the one in your prompt).
   - **Build your own copy:**
     ```
     wpf\build.bat <name>
     cd wpf\tests && .\build-tests.bat && cd ..\..     (it uses relative paths, so it must run from wpf\tests)
     ```
     `build.bat <name>` writes to `wpf\bin\<name>\`. Never build into `bin\Debug`.
   - **Run the tests hidden:** `pwsh wpf/tests/run-hidden.ps1 -Agent <name>`. It starts your exe on a private hidden desktop (`uitest-<name>`), runs the UI Automation tests against that PID, prints the PASS/FAIL lines, closes only that PID, and returns the test exit code. Windows, dialogs, popups and focus changes stay on that desktop.
   - **Capture hidden:** `pwsh wpf/tests/run-hidden.ps1 -Agent <name> -NoTests -Section "<Section title>" -Capture "<Id>=<out.png>,<Id>@<FocusId>=<out2.png>"`.
     - The app renders each element off-screen at 1400x1100 and 96 DPI (`--capture` in `App.xaml.cs`). Text is greyscale, like the headless-Edge web references.
     - `@<FocusId>` focuses that element first, for focus-state shots.
     - `!<ClickId>` clicks a button (or toggles a checkbox or toggle button) first. You can chain several, and they run in order. For example, `InputGrid!InputGrid_row1=...` captures with row 2 selected.
     - `-Capture` also accepts an element's `x:Name`. `Sections` is the whole content column in standalone mode.
     - Pass several captures as **one** comma-separated string.
   - **Don't** launch the exe directly on the user's desktop. Don't use `capture-wpf.ps1` or UIA `SetFocus` against a visible window. Both steal the user's screen and focus.
   - **Never** run `taskkill /im UIComponents.Wpf.exe`, `Stop-Process -Name UIComponents.Wpf`, or anything else that kills by name.
   - All tests must pass before a phase is done. Report a failure in an area another agent is editing at that moment; don't fix it.
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
