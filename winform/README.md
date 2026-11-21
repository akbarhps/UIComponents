# WinForms component library (.NET Framework 4.8)

A native WinForms counterpart to `web/index.html`, using the same warm, editorial visual identity. All colours, fonts and shape rules live in `src/Theme.cs` and `src/Draw.cs`; every control is a reusable class in the `UIComponents` namespace.

`DemoForm` is a runnable showcase with a collapsible, grouped sidebar that scrolls to each section.

## Build and run

```bat
build.bat
bin\UIComponents.exe
```

The machine used to create this had the .NET Framework 4.8 runtime and `csc.exe`, but no Visual Studio or `dotnet` CLI, so verification was compile-only. `UIComponents.csproj` is a classic (non-SDK) project file for opening in Visual Studio with a .NET Framework 4.8 targeting pack.

## Components in this first pass

| Web component | WinForms class |
|---|---|
| Tokens / shape language | `Theme`, `Draw` |
| Button (variants, loading, hover) | `ClaudeButton` |
| Badge | `ClaudeBadge` |
| Card | `ClaudeCard` |
| Divider | `ClaudeDivider` |
| Alert | `ClaudeAlert` |
| Switch | `ClaudeSwitch` |
| Progress bar | `ClaudeProgress` |
| Skeleton | `ClaudeSkeleton` |
| Avatar | `ClaudeAvatar` |
| Forms / fields | `ClaudeField` |
| Tag input | `ClaudeTagInput` |
| Autocomplete | `ClaudeAutocomplete` |
| File input | `ClaudeFileInput` |
| Date / month / year picker | `ClaudeDatePicker`, `DatePickerMode` |
| Modal form | `ClaudeModalForm` |
| Tabs | `ClaudeTabs` |
| Accordion | `ClaudeAccordion` |
| Stepper | `ClaudeStepper` |
| Breadcrumbs | `ClaudeBreadcrumbs` |
| Sidebar | `ClaudeSidebar` |
| Empty state | `ClaudeEmptyState` |
| Toast | `ClaudeToastHost` |
| Searchable select | `ClaudeSearchSelect` |
| Multi-select | `ClaudeMultiSelect` |
| Date-time picker | `ClaudeDateTimePicker` |
| Grid input / typed grid | `ClaudeGrid`, `GridColumnDef` |
| Table variants (filter, column filters, sort, sticky, selection, pagination) | `ClaudeTable`, `TableOptions` |
| Loading modal | `ClaudeLoadingDialog` |
| Drawer form with delete confirmation | `ClaudeDrawerForm` |
| Command palette (Ctrl+K) | `ClaudeCommandPalette` |
| Tooltips | themed `ToolTip` on the overlay buttons |

All web components now have a WinForms counterpart. Note that native WinForms controls (`DateTimePicker`, `NumericUpDown`, `DataGridView`) keep some platform styling — they are themed with the palette but cannot be made pixel-identical to the web versions.

## Notes for agents extending this

- Keep new controls in a dedicated class under `src/`, add them to `DemoForm.BuildContent()`, and add a nav entry in `BuildSidebar()`.
- Use `Theme` colours and fonts, and `Draw.RoundedBox` for shapes — don't hard-code colours.
- The compiler here is legacy C# 5: no auto-property initializers, no expression-bodied members, no string interpolation. Set defaults in constructors.
- Rebuild with `build.bat` after any change and check the console output; the app is GUI-only, so compile verification is the automated gate on this machine.
