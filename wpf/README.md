# WPF component library (.NET Framework 4.8)

A WPF counterpart to `web/index.html`, using the same design tokens. WPF renders vector graphics with device-independent pixels, so the UI can match the web version closely.

`Themes/Tokens.xaml` mirrors the web `:root` colours, fonts and radii. `Themes/Controls.xaml` holds reusable `Style`/`ControlTemplate` definitions — the equivalent of the web library's CSS classes.

## Build and run

```bat
build.bat
bin\Debug\UIComponents.Wpf.exe
```

To view one component at a time (handy for visual comparison with the web UI), pass its section title:

```bat
bin\Debug\UIComponents.Wpf.exe "Data table"
```

Any `SectionTitle` text works, e.g. `"Buttons"`, `"Forms"`, `"Empty state"`. The app opens normally when no title is given.

## UI Automation tests — no cursor movement

```bat
cd tests
build-tests.bat [agent] [area ...]
bin\UIAutomationTests.exe <pid> [--only a,b]
```

The suite is split into `tests/Tests.Core.cs` (helpers, driver, runner) and one `tests/Tests.<Area>.cs` per area. `--only` runs the listed areas in the canonical order; `build-tests.bat` with area names compiles only those files.

Start the app first, then run the tests. They find controls through `AutomationElement` and drive them with `InvokePattern`, `ValuePattern`, `TogglePattern`, `SelectionItemPattern`, `ExpandCollapsePattern` and `RangeValuePattern` — no screen coordinates and no cursor movement.

A test driver also runs inside the app (`App.xaml.cs` `DriverHook`, enabled by `run-hidden.ps1` through `UITEST_DRIVE=1`) so test actions are faster; see `plan/wpf/README.md` "Writing tests".

Coverage: the `// Covered:` header of each `tests/Tests.<Area>.cs`; the count is the `RESULT` line of a full run.

## Structure

| File | Purpose |
|---|---|
| `Themes/Tokens.xaml` | Colours, fonts, radii, shadows — mirrors web `:root` |
| `Themes/Controls.xaml` | Reusable styles and control templates |
| `MainWindow.xaml` | Showcase with a section per component |
| `tests/Tests.Core.cs` + `tests/Tests.<Area>.cs` | Cursor-free UI automation harness |

## Notes for agents

- Add a style in `Themes/Controls.xaml`, use it from `MainWindow.xaml`, and add an `AutomationProperties.AutomationId` so the test harness can find it.
- Add a matching test in your `tests/Tests.<Area>.cs` for every new component.
- Rebuild with `build.bat` and rerun the tests; both run from the command line, no Visual Studio required.
