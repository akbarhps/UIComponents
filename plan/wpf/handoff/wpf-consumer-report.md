# WPF copy/link consumer smoke test

Scratch work: `C:\Users\akbar\AppData\Local\Temp\claude\7ef8e2b7f5e35375\2f20943b-0b42-42e9-827a-7126ed563e39\scratchpad\wpf-consumer` (copy of library: `ui-design-copy\`, Variant A: `ConsumerA\`, Variant B: `ConsumerB\`, probes: `min-build.ps1`, `parse-themes.ps1`, `run-smoke.ps1`).

## Verdict

- Variant A (copy) builds: `ConsumerA\Consumer.csproj`, net48, LangVersion 5, MSBuild 4.8, `/v:minimal`, exit 0. The consumer XAML uses PrimaryButton, FieldInput, BadgeSuccess/BadgeText, ModalCard/ModalTitle/SecondaryButton, and SearchableSelect, MultiSelect, Autocomplete, CalendarPicker as `local:` elements; all compile.
- Variant B (link, no copy) builds: `ConsumerB\Consumer2.csproj` links `Themes\*.xaml` and the .cs files from the real repo `wpf\` via `<Page>`/`<Compile>` with `<Link>` child elements. Relative `Source="Themes/X.xaml"` yields the same resource names as A (`themes/*.baml`, 11 of them), so it resolves. Component form `Source="/Consumer2;component/Themes/X.xaml"` also builds. File-relative `Source="..\..\wpf\Themes\..."` also builds but matches no resource, so it cannot resolve (inferred from the resource list, not run).
- Runtime rendering: NOT verified. Every WPF launch on a hidden desktop died at start in this sandbox, including the library's own `wpf\bin\Debug\UIComponents.Wpf.exe` (copied to scratchpad and run). Exit codes: `0x8007045A` for WPF exes, `0xC0000142` for a Windows PowerShell WPF probe. `cmd.exe` on the same hidden desktop exits normally (exit 7 test), so the desktop itself works. This looks environmental (sandbox), not a consumer problem. I did not use the real desktop and did not disable the sandbox. A screenshot, the exception log check and `run-hidden.ps1` remain to be run where GUI launch works.

## Minimal .cs set (verified by build, `min-build.ps1`)

| Case | Compile set (plus Themes/* and App/MainWindow) | Result |
|---|---|---|
| Themes only, no .cs | none | FAIL CS0246 `UIComponents` (Display.xaml) |
| Shimmer | `Shimmer.cs` | PASS |
| Themes, no Shimmer, Toasts | `Toasts.cs` | FAIL MC4003 `Enabled` (Display.xaml) |
| SearchableSelect | `Shimmer.cs`, `DropdownPrimitives.cs`, `SearchableSelect.cs` (+ XAML use) | PASS |
| SearchableSelect without DropdownPrimitives | `Shimmer.cs`, `SearchableSelect.cs` | FAIL CS0246 `DropdownOutsideClick` |
| MultiSelect | Shimmer + DropdownPrimitives + `MultiSelect.cs` (+ XAML use) | PASS |
| MultiSelect without DropdownPrimitives | | FAIL CS0246 `ChipWrapPanel` |
| CalendarPicker | Shimmer + DropdownPrimitives + `CalendarPicker.cs` (+ XAML use) | PASS |
| CalendarPicker without DropdownPrimitives | | FAIL CS0246 `DropdownOutsideClick` |
| Autocomplete | Shimmer + DropdownPrimitives + `Autocomplete.cs` (+ XAML use) | PASS |
| Autocomplete without DropdownPrimitives | | FAIL CS0246 `ChipWrapPanel` |
| TagInput | Shimmer + DropdownPrimitives + `TagInput.cs` | PASS |
| TagInput without DropdownPrimitives | | FAIL CS0246 `ChipWrapPanel` |
| OverlayHost | Shimmer + `OverlayHost.cs` | PASS (standalone) |
| Toasts | Shimmer + `Toasts.cs` | PASS (standalone) |
| FileInput | Shimmer + `FileInput.cs` | PASS (standalone) |
| SheetGrid | Shimmer + `Toasts.cs` + `SheetGrid.cs` | PASS |
| SheetGrid without Toasts | Shimmer + `SheetGrid.cs` | FAIL CS0103 `Toasts` |
| Popover | Shimmer + `Popover.cs` | PASS (standalone) |
| TablePagination | Shimmer + `TablePagination.xaml` (Page) + `TablePagination.xaml.cs` (Compile) (+ XAML use) | PASS |

Shimmer is needed by every Themes build because `Themes/Display.xaml` declares `xmlns:local="clr-namespace:UIComponents.Wpf"` and uses `local:Shimmer.Enabled` (Style setters in lines ~368 and ~375). The `clr-namespace` has no assembly, so the types must be compiled into the consumer assembly.

Not needed by any listed control: `App.xaml.cs`, `MainWindow*.cs`, `*Controls.xaml(.cs)`, `DataControls.xaml.cs`, `AdvancedControls.xaml.cs`, `TableClip.cs` (no library file references it). `TableClip.cs` was not built into any case, so its status is by grep only.

## Hidden dependencies and pitfalls

- Shimmer.cs: see above, the one dependency every Themes build hits.
- DropdownPrimitives.cs: shared by SearchableSelect, MultiSelect, CalendarPicker, Autocomplete, TagInput.
- Toasts.cs: SheetGrid calls `Toasts.Show` (SheetGrid.cs ~line 685).
- Runtime-only (source reading, not run): `Toasts.Install(window)` casts `window.Content` to `Panel` (`Toasts.cs` ~line 45). `OverlayHost.Install(window, dialogStore)` moves the children of a Panel (demo: `x:Name="DialogStore"` in `OverlayControls.xaml` ~line 105) into its layer and casts `window.Content` to `Panel`. Use a Grid as the window root.
- Static name grep of `Source` forms is not a runtime test. `parse-themes.ps1` tried an in-process XamlReader parse of the 11 dictionaries via file URIs. It failed with "Cannot find resource named 'Sans'", because the file-URI Source merge does not reproduce App-level resource chaining. That is a limit of the probe, not evidence about the real app, so I did not count it.
- csproj: `<Link>` must be a child element. As an attribute, MSBuild gives MSB4066 for Page and Compile.
- The library csproj imports `Microsoft.WindowsDesktop.targets` conditionally; that file does not exist under `v4.0.30319`, so the import is skipped. The consumer used the same import and built.
- The library csproj references `UIAutomationProvider` and `UIAutomationTypes`. The consumer build passed without them (first build), so they are not a compile requirement. I added them to match the library before the launch attempts, but the launch attempts did not reach the point of testing them.

## Changes

- `wpf\COMPONENTS.md`: replaced the two "not verified" bullets (lines ~76-77) with the verified facts above. File is now 85 lines (limit 95). Only this file under `wpf\` was edited.
- `skills\ui-components\SKILL.md`: unchanged. Its WPF step (lines 39-41) names only COMPONENTS.md and the Themes grep, neither of which changed.
- `plan\wpf\handoff\wpf-consumer-report.md`: this file.

## Checks run

- MSBuild 4.8 build of `ConsumerA\Consumer.csproj`: PASS (exit 0).
- MSBuild build of `ConsumerB\Consumer2.csproj` (linked): PASS (exit 0). Also builds with component-form and file-relative `Source` (exit 0).
- Resource-name listing (`obj\Debug\*.g.resources`): A and B both list 11 `themes/*.baml`.
- `min-build.ps1`: 20 build cases, all as in the table above.
- Hidden-desktop launch of ConsumerA, ConsumerA with `modal`, the library's own exe, and a Windows PowerShell WPF probe: all died at start (see Verdict). Exception log and screenshot: not obtained.
- `git status`: only `wpf\COMPONENTS.md` modified. Nothing under `wpf\` source touched, no commit.

## Skipped

- Runtime launch, exception-free check, and screenshot (environment blocked it).
- `wpf\tests\run-hidden.ps1` run (needs a working hidden desktop; same blocker).
- Checking the rendered look of the controls (non-default style) (same blocker).
- Verifying that B2/B3 `Source` forms resolve at runtime (B3 is inferred).
