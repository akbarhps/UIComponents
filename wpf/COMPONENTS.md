# WPF component index

Paths are relative to the `ui-design/` root; `Themes/X.xaml` means `wpf/Themes/X.xaml`. Open only the named file; find a key with `grep -n 'x:Key="KEY"' ui-design/wpf/Themes/*.xaml`. Never read `wpf/MainWindow.xaml` whole (demo page). Find a demo with `grep -n '<!-- Table -->' ui-design/wpf/MainWindow.xaml`. Demo hosts: `wpf/InputControls.xaml`, `wpf/PickerControls.xaml`, `wpf/DataControls.xaml`, `wpf/AdvancedControls.xaml`, `wpf/ToastControls.xaml`, `wpf/OverlayControls.xaml`.

## Components

| Component | Class or style key | File | Use when |
|---|---|---|---|
| Tokens | `Bg`, `Ink`, `Brand`, `Line`, `Serif`, `Sans`, `Radius`, `RadiusSm`, `RadiusLg`, `Shadow1`, `Shadow2` | `wpf/Themes/Tokens.xaml` | Any colour, font, radius, shadow |
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

- Code-behind classes use `namespace UIComponents.Wpf`. The csproj is `WinExe` with no DLL output, so compile the `.cs` files into the consumer (referencing an assembly is not verified).
- `Source` resolves against the consumer project. If `Themes/` sits outside it, copy the folder in or link each file. Not verified here.
