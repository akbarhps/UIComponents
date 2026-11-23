# WPF component index

Paths are relative to the `ui-design/` root; `Themes/X.xaml` means `wpf/Themes/X.xaml`. Open only the named file; find a key with `grep -n 'x:Key="KEY"' ui-design/wpf/Themes/*.xaml`. Never read `wpf/MainWindow.xaml` whole (demo page). Find a demo with `grep -n '<!-- Table -->' ui-design/wpf/MainWindow.xaml`. Demo hosts: `wpf/InputControls.xaml`, `wpf/PickerControls.xaml`, `wpf/DataControls.xaml`, `wpf/AdvancedControls.xaml`, `wpf/ToastControls.xaml`, `wpf/OverlayControls.xaml`.

## Components

| Component | Class or style key | File | Use when |
|---|---|---|---|
| Tokens | Colour keys, same set in both files: `Bg`, `BgDeep`, `Surface`, `Surface2`, `Ink`, `Ink2`, `Ink3`, `Line`, `LineStrong`, `Brand`, `BrandHover`, `BrandBright`, `BrandSoft`, `BrandSoftHover`, `Success`, `SuccessSoft`, `Warning`, `WarningSoft`, `Danger`, `DangerSoft`, `Info`, `InfoSoft`, `Focus`, `OnBrand`, `OnInk`, `InkHover`, `OnDanger`, `DangerHover`, `BrandLine`, `SuccessLine`, `InfoLine`, `DangerLine`, `SageSoft`, `SageLine`, `Wash`, `WashStrong`, `Scrim`, `Shimmer`, `FocusRing`, `Knob`, `SheetCheckBorder`, `SheetCheckMark`, `SheetCheckFill`. Effects per theme: `Shadow1`, `Shadow2`, `KnobShadow`. Light only: `Serif`, `Sans`, `Radius`, `RadiusSm`, `RadiusLg` | `wpf/Themes/Tokens.xaml` (light), `wpf/Themes/Tokens.Dark.xaml` (dark overlay) | Any colour, font, radius, shadow. Colours: `DynamicResource` only |
| Theme | class `ThemeManager` (`Initialize`, `SetTheme`, `Mode`, `IsDark`, `StorageKey`, `ThemeChanged`); enum `ThemeMode` (`System`, `Light`, `Dark`) | `wpf/ThemeManager.cs` | Light, dark or System theme; call `Initialize()` once at startup |
| Theme toggle | `ThemeSystem`, `ThemeLight`, `ThemeDark` (3 `SegmentButton` radios, `GroupName="ThemeMode"`); `MainWindow.SyncThemeToggle` | `wpf/MainWindow.xaml`, `wpf/MainWindow.xaml.cs` | Showcase switch under the masthead chip (masthead is not parity-tracked) |
| Button | `PrimaryButton`, `SecondaryButton`, `GhostButton`, `DangerButton`, `DarkButton` (base `ClaudeButtonBase`) | `wpf/Themes/Controls.xaml` | One action |
| Small button | `PrimaryButtonSm`, `SecondaryButtonSm`, `GhostButtonSm`, `DangerButtonSm` | `wpf/Themes/Display.xaml`, `Themes/Sheet.xaml`, `Themes/Controls.xaml` | Compact action in a card or table |
| Icon button | `IconButton`, `IconButtonSm` | `wpf/Themes/Overlays.xaml` | Icon-only action |
| Segmented control | `SegmentButton` | `wpf/Themes/Controls.xaml` | Exclusive options in one row |
| Action menu | `MenuPanel`, `MenuItem`, `MenuItemDanger`, `MenuShortcut`, `MenuDivider` | `wpf/Themes/Navigation.xaml` | 3+ actions on one object |
| Card | `ClaudeCard`, `DisplayCard`, `DisplayCardTitle`, `DisplayCardText` | `wpf/Themes/Controls.xaml`, `Themes/Display.xaml` | Grouping content |
| Section heading | `SectionTitle`, `SectionDesc` | `wpf/Themes/Controls.xaml` | Section heading and description |
| Text input | `ClaudeInput`, `FieldInput`, `FieldInputError`, `FieldInputSearch` | `wpf/Themes/Controls.xaml`, `Themes/Forms.xaml` | Single-line text |
| Text area | `FieldTextArea`, `TextAreaGrip` | `wpf/Themes/Forms.xaml` | Multi-line text |
| Label, hint | `FieldLabel`, `FieldHint` | `wpf/Themes/Controls.xaml` | Text beside an input |
| Native select | `FieldSelect`, `FieldSelectItem` | `wpf/Themes/Forms.xaml` | Short, known list |
| Checkbox | `ClaudeCheckBox` | `wpf/Themes/Controls.xaml` | Boolean choice |
| Radio, switch | `ClaudeRadio`, `ClaudeSwitch` | `wpf/Themes/Forms.xaml` | One of a few; on/off |
| Searchable select | class `SearchableSelect`; `DropdownPanel`, `DropdownSearch`, `DropdownOption`, `DropdownTrigger` | `wpf/SearchableSelect.cs`, `Themes/Dropdown.xaml` | One option from a long list, with typing |
| Multi-select | class `MultiSelect`; `MultiTriggerChrome`, `MultiOption`, `MultiChip`, `ChipRemove` | `wpf/MultiSelect.cs`, `Themes/Dropdown.xaml` | Several values from a known list |
| Tag input | class `TagInput`; `TagBox`, `BareInput` | `wpf/TagInput.cs`, `Themes/Dropdown.xaml` | Freeform values |
| Autocomplete | class `Autocomplete`; `AcOption`, `AcEmpty` | `wpf/Autocomplete.cs`, `Themes/Dropdown.xaml` | Typed input, filtered suggestions |
| Date, time picker | class `CalendarPicker`; `PickerTrigger`, `PickerPanelRoot`, `PickerCell`, `PickerDow`, `PickerTitle` | `wpf/CalendarPicker.cs`, `Themes/Pickers.xaml` | Date, month, year or time |
| File input | class `FileInput`; `PickerFileDrop`, `PickerFileItem` | `wpf/FileInput.cs`, `Themes/Pickers.xaml` | Upload files |
| Grid input | class `SheetGrid` (with `SheetColumn`); `SheetTextCell`, `SheetSelectCell`, `SheetCheckboxCell`, `SheetHeadButton`, `SheetAddButton` | `wpf/SheetGrid.cs`, `Themes/Sheet.xaml` | Edit many cells as a grid |
| Command palette | `CmdkInput`, `CmdkGroup`, `CmdkItem`, `CmdkChip` | `wpf/Themes/Overlays.xaml` | Run a command by search |
| Badge | `BadgeBase`, `BadgeSuccess`, `BadgeText`, `DisplayBadge`, `BadgeBrandDot`, `BadgeSuccessDot`, `BadgeWarningDot`, `BadgeDangerDot`, `BadgeNeutralDot` | `wpf/Themes/Controls.xaml`, `Themes/Display.xaml` | Short status |
| Alert | `Alert`, `AlertTitle`, `AlertText` | `wpf/Themes/Display.xaml` | Inline feedback in a form or page |
| Toast | class `Toasts` (static `Install`, `Show`); `ToastCard`, `ToastTitle`, `ToastText`, `ToastClose`, `ToastAction` | `wpf/Toasts.cs`, `Themes/Toasts.xaml` | Transient feedback after an action |
| Progress bar | `DisplayProgress` | `wpf/Themes/Display.xaml` | Known-duration work |
| Skeleton | class `Shimmer` (attached `SetEnabled`); `SkLine`, `SkCircle` | `wpf/Shimmer.cs`, `Themes/Display.xaml` | Hold layout while data loads |
| Empty state | `EmptyTitle`, `EmptyText` | `wpf/Themes/Display.xaml` | Nothing here yet |
| Avatar | `AvatarCircle`, `AvatarInitials` | `wpf/Themes/Display.xaml` | User or entity placeholder |
| Icons | `IconInfo`, `IconCheck`, `IconAlert`, `IconClose`, `IconSearch`, `IconCard` | `wpf/Themes/Display.xaml` | Glyph in an alert or toast |
| Modal | class `OverlayHost` (static `Open`, `Close`, `IsOpen`); `ModalCard`, `ModalTitle`, `ModalBody`, `ModalCardShadow` | `wpf/OverlayHost.cs`, `Themes/Overlays.xaml` | Input or confirmation without leaving the page |
| Form error | `FormErrorRow`, `FormErrorText` | `wpf/Themes/Overlays.xaml` | Error summary in a modal form |
| Drawer, loading modal | no style key; `OverlayHost.Open` (demo `wpf/OverlayControls.xaml.cs`) | `wpf/OverlayControls.xaml` | Side-panel form; block UI while work runs |
| Tooltip, popover | class `Popover` (static `Show`, `Hide`, enum `PopoverSide`); `TipBubble`, `TipBubbleText` | `wpf/Popover.cs`, `Themes/Navigation.xaml` | Extra detail on hover or focus |
| Data table | `TableHeader`, `TableCell`, `TableCellText`, `TableCellWrap`, `TableRow`, `TableRowTall` | `wpf/Themes/Controls.xaml`; demo `wpf/DataControls.xaml` | Tabular data |
| Pagination | control `TablePagination`; `TablePageButtonActive` | `wpf/TablePagination.xaml`, `Themes/Controls.xaml` | Paging a table |
| Row selection | class `SelectionRow` with `ClaudeCheckBox` | `wpf/DataControls.xaml.cs` | Select rows, then act |
| Table filters | `TableFilterInput`; class `FilterRow`; demo `wpf/AdvancedControls.xaml` | `wpf/Themes/Controls.xaml`, `wpf/DataControls.xaml.cs` | Narrow rows, or per-column filters |
| Sort and pin | `SortHeaderButton`, `PinButton`, `TableRowSort`, `TableCellStickyPinned` | `wpf/Themes/Controls.xaml` | Sortable or pinned columns |
| Status cell | `StatusBadgeTemplate` (DataTemplate) | `wpf/Themes/Controls.xaml` | Status inside a table |
| Breadcrumbs | `CrumbLink`, `CrumbCurrent`, `CrumbEllipsis`, `CrumbSep` | `wpf/Themes/Navigation.xaml` | Site path |
| Tabs | `TabButton`, `TabPanelText` | `wpf/Themes/Navigation.xaml` | Switch views on one page |
| Accordion | `Accordion`, `AccordionTrigger`, `AccordionPanelText` | `wpf/Themes/Navigation.xaml` | Reveal detail in place |
| Stepper | `StepMarkerText`, `StepTitle`, `StepDesc` | `wpf/Themes/Navigation.xaml` | Wizard position |

Not in WPF (grep found nothing): typed grid. Report it as missing; do not invent it.

## Consumer App.xaml

Mirror `wpf/App.xaml`: merge the same 11 dictionaries in this order. Omit `StartupUri`.

```xml
<ResourceDictionary.MergedDictionaries>
  <ResourceDictionary Source="Themes/Tokens.xaml"/>
  <ResourceDictionary Source="Themes/Controls.xaml"/>
  <ResourceDictionary Source="Themes/ExtraControls.xaml"/>
  <ResourceDictionary Source="Themes/Sheet.xaml"/>
  <ResourceDictionary Source="Themes/Forms.xaml"/>
  <ResourceDictionary Source="Themes/Dropdown.xaml"/>
  <ResourceDictionary Source="Themes/Display.xaml"/>
  <ResourceDictionary Source="Themes/Toasts.xaml"/>
  <ResourceDictionary Source="Themes/Overlays.xaml"/>
  <ResourceDictionary Source="Themes/Pickers.xaml"/>
  <ResourceDictionary Source="Themes/Navigation.xaml"/>
</ResourceDictionary.MergedDictionaries>
```

- Code-behind classes use `namespace UIComponents.Wpf`. No DLL is built, so compile the `.cs` files into the consumer (referencing an assembly is not supported).
- Verified by build (net48, `LangVersion` 5, old-style csproj). Copy `Themes/` in, or link it (below). Compile per family:
  - Always `Shimmer.cs`: `Themes/Display.xaml` uses `xmlns:local="clr-namespace:UIComponents.Wpf"` and `local:Shimmer.Enabled`. Without it the build fails (MC4003).
  - `DropdownPrimitives.cs` for `SearchableSelect`, `MultiSelect`, `CalendarPicker`, `Autocomplete`, `TagInput` (without it: CS0246 `ChipWrapPanel`).
  - `SheetGrid.cs` also needs `Toasts.cs` (CS0103).
  - Standalone: `OverlayHost.cs`, `Toasts.cs`, `Popover.cs`, `FileInput.cs`. `TablePagination.xaml` (Page) plus `TablePagination.xaml.cs` (Compile).
  - Not needed by any listed control: `App.xaml.cs`, `MainWindow*.cs`, `*Controls.xaml(.cs)`, `TableClip.cs`.
- Linked (no copy): `<Page Include="<repo>\wpf\Themes\*.xaml"><Link>Themes\%(Filename)%(Extension)</Link></Page>`, and `<Compile Include="...\X.cs"><Link>Lib\X.cs</Link></Compile>`. `Link` must be a child element (as an attribute: MSB4066). Relative `Source="Themes/Tokens.xaml"` then resolves (same `themes/*.baml` names as the copy). `Source="/Consumer2;component/Themes/Tokens.xaml"` also builds. File-relative `Source="..\..\wpf\Themes\..."` builds but matches no resource (inferred, not run).
- Required at runtime: `Toasts.Install(window)` casts `window.Content` to `Panel`; `OverlayHost.Install(window, panel)` needs a Panel such as `x:Name="DialogStore"` in that window.
- Verified at runtime (hidden desktop, copy and link variants, same render): button, field, badge, SearchableSelect, MultiSelect, Autocomplete, CalendarPicker, a dialog via OverlayHost and a toast all render styled, no exceptions. Not run: SheetGrid, FileInput, TablePagination, Popover, TagInput.

## Theme (light / dark / system)

- Compile `ThemeManager.cs` (required: `Shimmer.cs` uses `ThemeManager.ThemeChanged`).
- Copy or link `Themes/Tokens.Dark.xaml` next to `Themes/Tokens.xaml`. Do not merge it yourself: `ThemeManager` loads it on first dark use and adds it as the last merged dictionary (highest precedence).
- Merge `Tokens.xaml` into `Application.Resources`, not `Window.Resources`, so popups resolve. `ThemeManager` finds the dark sibling through the merged `Tokens.xaml` source; without it, it throws `InvalidOperationException`.
- Call `ThemeManager.Initialize()` first in `OnStartup`, before `base.OnStartup` (so before the first window).
- Colours only via `{DynamicResource K}` or `SetResourceReference`. Never `StaticResource`, and never read a brush in code (`(Brush)FindResource`, `Brushes.White`): those do not switch. `node plan/wpf/theme-lint.mjs` checks this in the repo.
- Persistence: `HKCU\Software\UIComponents.Wpf`, value `ui-theme` (`light` or `dark`; absent = System). `ThemeManager.StorageKey` takes your own subkey, or `null` for no persistence.
- Env `UI_THEME=light|dark|system` forces a mode for the process and never reads or writes storage. Tests and captures use it (`run-hidden.ps1 -Theme`).
- `ThemeManager.SetTheme(ThemeMode.Dark)` applies and persists; `ThemeChanged` fires after every apply, including System-driven ones.
- Title bar: `ThemeManager` asks DWM for a dark title bar on each window (attribute 20, fallback 19). Older Windows ignore it silently. Manual check only; captures do not show it.

Known dark-mode gaps (accepted for task 09, not fixed):
1. Scrollbars: the default Aero2 scrollbars stay light in dark (command palette list, drawer and modal bodies when they overflow, FileInput list, ComboBox popups, textarea hosts, page scroller). The web themes all of them. Fix = apply `DropdownScrollBar`; that also changes light, so it is a separate task.
2. Keyboard focus: the default dashed `FocusVisualStyle` is not overridden on several styles (`SegmentButton`, `SortHeaderButton`, table cells and rows, `TableThinThumb`, `FieldSelectItem`, `TextAreaGrip`, `ContextMenu`/`MenuItem`, legacy `ClaudeInput`). It is black, so invisible on dark. Not observed: captures draw no keyboard focus. Check on a visible desktop.
3. Not visually verified in dark: ComboBox popups, action menu, FileInput with files, Shimmer overlay (code-reviewed only).
4. Legacy `ClaudeInput` stays light by rule (no restyle).
5. `-Theme system` was exercised on the dark branch only.
6. Fractional-pixel lines (stepper dividers, tabs underline, card bottom edges) straddle two pixel rows in both themes. Pre-existing layout, not theming.

Intentional light-mode changes made by task 09 (the only ones; everything else is byte-identical, aside from the accepted File-input.png and Buttons.png 7px deltas):
1. Caret: every `TextBox` style except legacy `ClaudeInput` (`FieldInput` and its Search/Error variants, `FieldTextArea`, `TableFilterInput`, `DropdownSearch`, `BareInput`, `CmdkInput`, `SheetTextCell`) sets `CaretBrush="{DynamicResource Brand}"`. The web has `*{ caret-color: var(--brand) }` (`web/index.html:74`). The WPF default is an inverted-background caret (black in light, near-white in dark). The caret blinks, so static captures do not show it.
2. File-input title ("Drop a file here"): was black, now `Ink` (`#26241F`), like the web `.file-drop strong`.
