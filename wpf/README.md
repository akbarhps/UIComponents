# WPF component library (.NET Framework 4.8)

A WPF counterpart to `web/index.html`, using the same design tokens. WPF renders vector graphics with device-independent pixels, so the UI can match the web version far more closely than WinForms.

`Themes/Tokens.xaml` mirrors the web `:root` colours, fonts and radii. `Themes/Controls.xaml` holds reusable `Style`/`ControlTemplate` definitions — the equivalent of the web library's CSS classes.

## Build and run

```bat
build.bat
bin\Debug\UIComponents.Wpf.exe
```

## UI Automation tests — no cursor movement

```bat
cd tests
build-tests.bat
bin\UIAutomationTests.exe
```

Start the app first, then run the tests. They find controls through `AutomationElement` and drive them with `InvokePattern`, `ValuePattern`, `TogglePattern`, `SelectionItemPattern`, `ExpandCollapsePattern` and `RangeValuePattern` — no screen coordinates and no cursor movement.

Covered: window, buttons, text fields, data grid, checkbox, combobox, date picker, progress bar, slider, tabs, expander, multi-select list, modal dialog, avatars, breadcrumbs, stepper, tooltips, action menu, empty state, skeleton, tag input, autocomplete, file input, date-time picker, grid input, typed grid, filter table, selection table, column filters, sortable table, toasts and the dialog launchers — 62 assertions in total.

## Structure

| File | Purpose |
|---|---|
| `Themes/Tokens.xaml` | Colours, fonts, radii, shadows — mirrors web `:root` |
| `Themes/Controls.xaml` | Reusable styles and control templates |
| `MainWindow.xaml` | Showcase with a section per component |
| `DialogWindow.xaml` | Modal dialog demo |
| `tests/UIAutomationTests.cs` | Cursor-free UI automation harness |

## Notes for agents

- Add a style in `Themes/Controls.xaml`, use it from `MainWindow.xaml`, and add an `AutomationProperties.AutomationId` so the test harness can find it.
- Add a matching test in `tests/UIAutomationTests.cs` for every new component.
- Rebuild with `build.bat` and rerun the tests; both run from the command line, no Visual Studio required.
