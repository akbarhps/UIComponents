# WPF pixel-parity plan

Goal: every WPF component in `wpf/` should look and behave the same as its `web/index.html` counterpart. The web version is the source of truth.

Roles:
- **Planner/reviewer** writes one task file per component group, then reviews the worker's diff and screenshots against it.
- **Worker** does one task file at a time, one phase at a time, and stops after each phase so it can be reviewed.

## Task files

| # | File | Scope | Status |
|---|------|-------|--------|
| 01 | [01-grid-input.md](01-grid-input.md) | Grid input + Typed grid input (`createGrid`) | in progress |
| 02 | [02-forms.md](02-forms.md) | Forms section: input, search, select, textarea, error, checkbox, radio, switch | ready (after 01) |
| 03 | — | Dropdown family: searchable select, multi-select, tag input, autocomplete | later |
| 04 | — | Table with filters / row selection / column filters / sortable | later |
| … | — | everything else | later |

## Hard constraints (apply to every task)

1. **C# 5 only.** The csproj sets `LangVersion 5` and builds with `C:\Windows\Microsoft.NET\Framework\v4.0.30319\MSBuild.exe`, so these are not available:
   - `$"..."` interpolation, `?.`, `??=`, `nameof`
   - `=>` expression-bodied members, auto-property initialisers (`{ get; set; } = x`)
   - `out var`, tuples, pattern matching, local functions

   Use `string.Format` / `+` concatenation and full property bodies instead.
2. **Old-style csproj.** Every new `.xaml` file needs a `<Page Include>` entry in `wpf/UIComponents.Wpf.csproj`, and every new `.cs` file needs a `<Compile Include>` entry. A new resource dictionary must also be merged in `wpf/App.xaml`.
3. **Other agents edit this repo too.** Only touch the files the task lists. Don't restyle shared styles (`ClaudeCard`, `ClaudeButtonBase` and so on) unless the task explicitly says to.
4. **Build and test from the CLI:** `wpf\build.bat`, then start `wpf\bin\Debug\UIComponents.Wpf.exe`, then run `wpf\tests\build-tests.bat` and `wpf\tests\bin\UIAutomationTests.exe`. All tests must pass before a phase is done.
5. **A running app locks the exe.** While any `UIComponents.Wpf.exe` runs from `wpf\bin\Debug`, `build.bat` can't overwrite it. Close your own instance before rebuilding, and coordinate if another agent's instance is running. To build without touching `bin\Debug`, pass `/p:OutputPath=<dir>\bin\ /p:IntermediateOutputPath=<dir>\obj\` to MSBuild.
6. **Fidelity target:** geometry and colours within ±1px / exact hex. Glyph rasterisation differs between Chromium and WPF (sub-pixel shape and hinting). Do **not** chase glyph shape. Do chase font family, size, weight, colour and position.

## Visual comparison workflow

The display runs at 100% scale (96 DPI), so 1 CSS px = 1 WPF DIP. Keep the WPF window on a 100% monitor while capturing.

```powershell
# Web reference: show only the given sections (window 1400x1100, scale 1)
pwsh plan/wpf/capture-web.ps1 -Sections sheet-input,typed-sheet -Out plan/wpf/ref/web-grid-input.png
#   optional -Js "..." runs after load, e.g. to click a header or focus a cell

# WPF actual: app must be running; scrolls the element on screen and screenshots it, no cursor movement
pwsh plan/wpf/capture-wpf.ps1 -Id InputGrid -Out plan/wpf/ref/wpf-grid-input.png -Pad 0
#   uses the newest UIComponents.Wpf process; pass -ProcessId N to choose one
```

If `capture-wpf.ps1` says "AutomationId not found", you are probably attached to a stale or foreign instance (another agent's old build). Pass `-ProcessId` for your own instance. `InputGrid` and `TypedGrid` inside `DataControls` were verified reachable on 2026-11-21 (see `ref/wpf-before-*.png`).

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

Commit the WPF captures with the phase (`plan/wpf/ref/wpf-*.png`) so the reviewer can compare them side by side with the `web-*.png` files.
