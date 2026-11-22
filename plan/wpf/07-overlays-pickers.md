# 07 — Overlays and pickers: modal, form modal, drawers, loading, command palette, toasts, date / date-time pickers, file input (agents "toasts", "overlays", "pickers")

A fix-up audit, but in practice a rewrite. Every overlay in WPF today is a separate OS `Window` opened with `ShowDialog()`. That means `--capture` can't see it, it is unstyled, and it blocks the UI thread. The toast is private to `AdvancedControls`. The pickers are a stock WPF `DatePicker` (a `Popup`) plus a text box. The file input is a Browse button plus a `ListBox`. This plan builds three shared primitives first:
- an **overlay layer** in the main window (scrim, focus trap, Escape, focus restore)
- a **global toast host**
- **picker hosting** that reuses the dropdown family's adorner

Then it rebuilds each web section on top of them.

**Web group vs this list.** The web has these ten sections. Each name below is the web `h2`, so it is also the WPF `SectionTitle`:

| Web id | h2 (= WPF section title) | Markup | JS | CSS |
|---|---|---|---|---|
| `form-modal` | Modal form | 1218 (+ dialog 2020–2055) | 2157–2178 | `.modal--form`/`.form-grid` 547–551 |
| `drawer` | Sidebar modal form | 1811 (+ invite 2057–2089, edit 1952–1995) | 2180–2236 | `.drawer*` 729–750 |
| `modal` | Modal dialog | 1822 (+ dialog 1937–1950) | 2140–2145 | `.overlay`/`.modal`/`.icon-btn` 430–455 |
| `loading` | Loading modal | 1830 (+ dialog 2010–2018) | 2147–2155 | 517–524 |
| `date-picker` | Date picker | 1838 | 2526–2667 | `.picker*` 457–493 |
| `datetime-picker` | Date &amp; time picker | 1865 | same | same |
| `command-palette` | Command palette | 1904 (+ dialog 1997–2008) | 3184–3249 | `.cmdk*` 223–245 |
| `toast-variants` | Toast variants | 1912 | 3413–3418 | 706–712 |
| `toasts` | Toasts | 1924 (+ `#toastStack` 2091) | 3376–3411 | 526–545 |
| `file-input` | File input | 1147 | 3136–3182 | 581–603 |

- The shared overlay JS (`openOverlay`/`closeOverlay`, backdrop click, Escape, Tab trap) is at 2094–2138.
- Shared CSS: tokens 14–45, global `:focus-visible` and the scroll bar 49–55, `.btn*` 159–186, `.field`/`.input`/`.select`/`.textarea`/`.switch` 295–353, `.input--sm` 662, responsive and reduced motion 836–873.
- Icons (sprite 878–905, plus `i-plus` at 3467; all 24-unit boxes): `i-close` 890 (stroke 2), `i-check` 889 (2.4), `i-info` 893 (1.9), `i-alert` 887 (1.8), `i-search` 891 (1.9), `i-chevron` 897 (2), `i-calendar` 899, `i-clock` 900, `i-upload` 901, `i-file` 902, `i-more` 888, `i-download` 905, `i-button` 882, `i-table` 894, `i-panel` 896 (all 1.8), `i-plus` 3467 (2).

There is no web section called "pickers" or "overlays". The list in the task maps one-to-one onto the ten sections above. The web splits "date picker" into two sections (date, and date & time), and it has two toast sections.

**References** in `plan/wpf/ref/` (2026-11-22, 1400×1100, scale 1, transitions and animations off):
- **Idle sections:** `web-overlays-<modal|form-modal|drawer|loading|cmdk|toasts|toast-variants>.png`, `web-pickers-<date|datetime>.png`, `web-file-input.png`.
- **Open overlays** (the whole window, scrim over the page and sidebar):
  - `web-overlays-modal-open.png`
  - `-form-modal-open`, `-form-modal-errors` (empty submit)
  - `-drawer-invite`, `-drawer-invite-errors`, `-drawer-edit`, `-drawer-edit-confirm`
  - `-loading-open`
  - `-cmdk-open`, `-cmdk-down2` (2× Down), `-cmdk-filter` ("go"), `-cmdk-empty` ("zzz")
- **Command palette with the real 11px scroll bar** (its list scrolls): `web-overlays-cmdk-open-scrollbar.png`, `-cmdk-down2-scrollbar.png`, `-cmdk-up-scrollbar.png` (Up from item 0 wraps to the last item and scrolls). The other PNGs use `--hide-scrollbars`, so their list is 11px too wide. For the palette list, compare against the `-scrollbar` files.
- **Toasts:** `web-overlays-toasts-shown.png` (success, info and error stacked) and `web-overlays-toast-variants-shown.png` (dismissible, with action, persistent).
- **Pickers:**
  - `web-pickers-date-open.png`
  - `-date-selected` (day 14 picked, reopened)
  - `-month-open`, `-year-open`
  - `-date-values` (the three date-picker values set)
  - `-datetime-open` (day 14 picked, time row)
  - `-datetime-seconds-open`, `-datetime-values`
- **File input:** `web-file-input-files.png` (1 file + 3 images seeded with `DataTransfer`).
- **Current WPF state:** `wpf-before-overlays-<toasts|toasts-shown|dialogs|overlays|file-input|date-time>.png`. The four dialog windows don't render in `--capture` at all, so there is no "before" shot of any open overlay.
- **Measurements:** `web-overlays-measurements.txt` holds every rect, font, colour, radius, padding and shadow. `web-overlays-measure.js` produced it (append it to `index.html`, then run Edge `--dump-dom --hide-scrollbars`).
  - **The dump viewport is 1376×1008, not 1400×1100.** Its `vp` numbers belong to that viewport only. Use the `rel` numbers (relative to the dialog box, toast, picker panel or `.demo` card) plus the anchoring formulas in this plan.
- **Scripts:** `web-overlays-capture.ps1` regenerates all the PNGs above except the palette `-scrollbar` ones, which come from `web-overlays-capture-scrollbar.ps1`.
  - **After any accepted web fix (W1–W12) lands**, re-run both scripts and `web-overlays-measure.js`, and use the fixed values over this plan's numbers. Affected numbers:
    - W1: palette position
    - W2: loading focus ring
    - W3: Delete 64.1×42
    - W4: date-only icons
    - W10/W11: toast and file behaviour
  - **Picker refs depend on today's date** (title "November 2026", today ring on the 22nd, the "Today" button). Re-run the script on the day you compare, and compute picker test expectations from `DateTime.Today`.

Read `plan/wpf/README.md` first. Rules 4 (hidden desktop, your agent name) and 5 (never commit) are mandatory.

---

## Review of the current port (audit, 2026-11-22, build `planner`)

| Web section | WPF now | Problems |
|---|---|---|
| Modal dialog | "Overlays" block at the end of `MainWindow.xaml`: a `ClaudeCard` with "Open dialog" and a `DialogResultText` line. `DialogWindow.xaml` is a separate 420×230 OS window | **Not capturable**: own HWND. There is no scrim, X button, `.modal` card or shadow. The title isn't 600. The body text is shortened. The buttons are "Delete" at fixed widths 90/100. There is no Escape, backdrop or focus-restore handling. `ShowDialog` blocks the thread, so the test runs it on a worker thread and closes it with `WindowPattern` |
| Loading modal | `LoadingWindow` (OS window, 380×220, default `ProgressBar IsIndeterminate`), launched from the "Dialogs" block in `AdvancedControls` | Not capturable. It closes after **1800ms** (web: 3000). There is no spinner, no 4px bar, no "Ready" toast, and the text is shortened. It uses `System.Threading.Timer` plus `Dispatcher.Invoke` |
| Modal form | missing | |
| Sidebar modal form | `DrawerWindow` (OS window, 420×460, centred, not docked right). Only an "Edit member" form with a WPF `ComboBox`; "Delete" does nothing | Not capturable. There is no invite drawer, no validation, no confirm step and no switch. Save and Cancel just close |
| Command palette | `CommandPaletteWindow` (OS window, `ClaudeInput` + default `ListBox` with 3 items) | Not capturable. There are no groups, icons, shortcuts, filter, empty state, foot, arrow keys or Ctrl+K, and the commands do nothing |
| Toasts | "Toasts" block in `AdvancedControls`: a `ClaudeCard` with 3 buttons. Toasts are appended to an in-flow `ItemsControl ToastHost` under the card (radius 8, `Shadow1`, padding 14, full width) | They are in the page flow, not fixed bottom-right at 360 wide. There is no icon circle, no border, the serif title is missing, and they never dismiss. The host is private to `AdvancedControls`, so grid, progress and tables can't raise toasts (REVIEWER "Copy toast deferred") |
| Toast variants | missing | |
| Date picker / Date & time picker | One "Date &amp; time" section in `InputControls`: a stock `DatePicker` (180 wide, Aero chrome, its calendar is a `Popup`) plus a `ClaudeInput` "09:30:00" | Wrong control. The `Popup` isn't capturable. There are no modes (month, year, time, seconds), no 46px trigger and no icon (B1: the value text starts at +39) |
| File input | "File input" section in `InputControls`: "Browse files…" `SecondaryButton` + a 100px `ListBox` of full paths | There is no drop zone, no two variants, no file rows (icon, name, size, remove) and no count. It shows full paths instead of names |

All the old sections use the shadowed `ClaudeCard` with a 16px gap and have no `SheetSectionDesc`.

---

## Decisions (made in this plan; reviewer confirms)

1. **One in-window overlay layer, no OS windows, no `Popup`.** `OverlayHost.Install(MainWindow)` (called once from the `MainWindow` constructor) adds a `Grid` named **`OverlayLayer`** to `MainWindow.Content` (the root `Grid`): `Grid.ColumnSpan=2`, `Panel.ZIndex=100`.
   - It **always exists and is never `Collapsed`**:
     - `--capture` resolves the target **before** any `!` step runs.
     - A collapsed target would crop to 0×0.
   - **Closed:** `Background = null`, so clicks pass through to the page, and every dialog inside it is `Collapsed`.
   - **Open:** a scrim child `Border` (`OverlayScrim`, `#6B1E1A14` = rgba(30,26,20,.42)) is shown, plus the dialog.
   - **Why this works:**
     - It sits inside `MainWindow.Content`, so it is captured, `FindById`-reachable and in the main window's UIA tree.
     - It covers the sidebar too, like the web's `position:fixed; inset:0`.
     - It sits above the dropdown adorners, which live inside `ContentScroll`.
   - **Dialog content is authored in XAML** inside a collapsed `Grid x:Name="DialogStore"` in `OverlayControls.xaml`.
     - **Signature:** `OverlayHost.Install(Window window, Panel dialogStore)`, called in the `MainWindow` constructor right after `InitializeComponent()` as `OverlayHost.Install(this, Overlays.DialogStore);`.
     - It moves the store's children into `OverlayLayer` **once, synchronously** (no dependence on `Loaded`), so `x:Name` fields stay valid in code-behind.
     - **Rule:** once moved, the dialogs leave `OverlayControls`' tree. So they may use **only app-level Themes keys through `StaticResource`**: no `OverlayControls.Resources`, no implicit styles defined in the UserControl, no `DynamicResource` to local keys, and no `ElementName` bindings that cross the store boundary.
2. **One global toast host.**
   - `Toasts.Install(MainWindow)` adds a `Grid` named **`ToastLayer`** (ColumnSpan 2, ZIndex **200**, `Background null`, never collapsed) holding a `StackPanel` named **`ToastStack`**: 360 wide, `HorizontalAlignment Right`, `VerticalAlignment Bottom`, `Margin 0,0,24,24`, 10px gaps.
   - **API:** `public static Border Toasts.Show(string type, string title, string text, bool dismissible, string action, bool persistent)`, plus a 3-argument overload. `type` is "success", "info" or "error".
   - Grid, progress and tables call this later (follow-ups F1–F3).
3. **Pickers reuse `DropdownAdorner` unmodified** (`DropdownPrimitives.cs`, owned by `dropdown`).
   - It arranges the panel at the anchor's width, 6px below it. The web measures 110.8 − 58.8 = 52 = 46 + 6, which matches.
   - The picker panel is narrower than the trigger, so the root handed to the adorner is a transparent `Grid` whose child has `Width=300` and `HorizontalAlignment=Left`.
   - **Outside click:** the hook is currently private to `SearchableSelect`. Ask `dropdown` (through the reviewer) to lift it into `DropdownPrimitives.cs` as a small reusable helper. **The pickers wait for that.** If it hasn't landed by Phase P1, copy the ~30 lines with a `ponytail:` note naming the lift, but only with the reviewer's OK.
4. **Code-only picker and file controls, like `SearchableSelect`.**
   - `CalendarPicker : Grid` with properties `Mode` (date|month|year), `WithTime`, `WithSeconds`, `Placeholder`, `Icon` (Calendar|Clock).
   - `FileInput : Grid` with properties `Multiple`, `Title`, `Subtitle`.
   - Each builds its tree in `OnInitialized` and prefixes its children with its own AutomationId.
   - **Don't name the class `DatePicker`:** it collides with `System.Windows.Controls.DatePicker` in C# files.
5. **Interactive parts are `ButtonBase`**, so UIA Invoke and the `!Id` chain reach them:
   - picker triggers are `ToggleButton`s, with open/close driven by `Checked`/`Unchecked`, as in 03
   - cells, icon buttons, toast close/action, palette items and file remove are `Button`s
   - the drop zone is a `Button` (Space/Enter or a click opens the file dialog)
6. **Focus trap = `KeyboardNavigation.TabNavigation="Cycle"`** on each dialog root. The edit-drawer confirm gets its own `TabNavigation="Cycle"` on the `EditConfirm` panel, and focus moves into it (`KeepMember`) when it shows. Tab then cycles Keep member ↔ Delete member only, and never reaches the head's X or the form underneath. That matches the web, which scopes the trap to `.drawer__confirm:not([hidden])`.
7. **Scroll lock comes free.**
   - The open layer has a hit-testable background, so the wheel never reaches `ContentScroll`.
   - Focus is trapped, so no keys reach it either.
   - **Never** change `ContentScroll.VerticalScrollBarVisibility` (Disabled resets the offset).
   - **Never** set `IsEnabled=false` on the page (the disabled styles would show through the scrim).
8. **Escape routing.** Handle Escape on the **`OverlayLayer` bubbling `KeyDown`**, not a window `PreviewKeyDown`: a tunnelling handler would close the edit drawer before the confirm's own Escape handler runs (the web relies on that handler's `stopPropagation`). The edit drawer root handles `KeyDown` Escape first while the confirm is visible and sets `Handled`.
9. **No entrance or exit transitions** (overlay fade .18s, dialog scale .2s, drawer slide .26s, toast slide in/out .25s, removal after 260ms). Set the final state directly, as in 03 decision 4. A dismissed toast is removed immediately. **Open question Q2.**
   - **Exception:** the loading spinner (0.8s linear spin) and the indeterminate bar (1.2s ease-in-out, −100% → 390%) are functional, so port them.
   - **Guard them like `Shimmer.cs`:** don't start them when the command line contains `--capture` or when `SystemParameters.ClientAreaAnimation` is false. When static, the spinner sits at 0° (brand arc on top) and the bar fill at x 0, 35% wide. That matches the refs.
10. **Backdrop blur.** The web scrim has `backdrop-filter: blur(3px)`. **Recommended:**
    - While a dialog is open, set a `BlurEffect` on the other root children (sidebar `Border` and `ContentScroll`). Start at `Radius 9` (σ≈3) and tune it against the ref.
    - Remove it on close.
    - Pixel check: a scrim pixel over the page is `#9B9993` in the ref; the unblurred maths gives `#9B9893`.
    - **Open question Q1.**
11. **Web scroll-bar gutter.** The web keeps an 11px `scrollbar-gutter` on the right that the scrim, toasts and drawer don't cover: in the ref, x 1389–1399 stays `#F5F4EF`, and toasts end 24px left of the gutter. WPF's layer spans the whole window content, so toasts end 24px from the window edge and the drawer is flush right. **Accepted difference** (Q7). Compare dialog geometry relative to the dialog, and check centring within `OverlayLayer` ±1.
12. **Reuse, don't copy:**
    - **From `forms`:** `FieldLabel`, `FieldHint`, `FieldInput`, `FieldInputError`, `FieldSelect`, `FieldTextArea`, `ClaudeSwitch`, `ErrorIcon`.
    - **From `tables`:** `TableFilterInput` for the picker time inputs (34.2 tall, 13px, text inset 10, which matches `.input--sm`) and `DangerButtonSm`.
    - **From `display`:** `PrimaryButtonSm`, `SecondaryButtonSm`, `IconCheck`, `IconInfo`, `IconClose`, `IconAlert`, `IconSearch`.
    - **From `Sheet.xaml`:** `GhostButtonSm`, `SheetSectionDesc`, `DemoCard`.
    - **From `Controls.xaml`:** `PrimaryButton`, `SecondaryButton`, `DangerButton`.
    - New shared style: `IconButton` (`.icon-btn`), with sizes 32/18 and 26/15, defined by `overlays` in `Themes/Overlays.xaml`. 06 navigation should reuse it.
13. **Section placement.**
    - Keep the WPF order (precedent: 05 Q3).
    - New `OverlayControls.xaml` (owned by `overlays`): "Modal form", "Sidebar modal form", "Modal dialog", "Loading modal", "Command palette", plus the `DialogStore`.
    - New `ToastControls.xaml` (owned by `toasts`): "Toast variants", "Toasts".
    - Both go into `MainWindow.xaml` where the "Overlays" block is now, and both are added to `ApplyStandaloneMode`'s panel list. Otherwise `-Section` can't find their titles.
    - The pickers replace "Date &amp; time" in `InputControls` with "Date picker" and "Date &amp; time picker". "File input" stays where it is.

---

## Files and ownership

| File | Agent | Change |
|---|---|---|
| `wpf/Toasts.cs` (**new**) | toasts | `Toasts.Install`, `Toasts.Show`, timers |
| `wpf/Themes/Toasts.xaml` (**new**) | toasts | toast card, icon circle, close and action styles |
| `wpf/ToastControls.xaml(.cs)` (**new**) | toasts | the two toast sections |
| `wpf/OverlayHost.cs` (**new**) | overlays | `OverlayHost.Install/Open/Close`, scrim, Escape, backdrop, focus save/restore, blur |
| `wpf/Themes/Overlays.xaml` (**new**) | overlays | `ModalCard`, `IconButton` (+`IconButtonSm`), drawer, loading and palette styles, icons not in Display (`IconPlus`, `IconMore`, `IconDownload`, `IconButtonGlyph`, `IconTable`, `IconPanel`, `IconCalendar`, `IconClock`, `IconUpload`, `IconFile`, `IconChevron`), in Display's `Icon*` pattern (stroke bound to `Foreground`) |
| `wpf/OverlayControls.xaml(.cs)` (**new**) | overlays | 5 sections + `DialogStore` with 6 dialogs (modal, loading, form modal, invite drawer, edit drawer, palette) |
| `wpf/CalendarPicker.cs`, `wpf/FileInput.cs`, `wpf/Themes/Pickers.xaml` (**new**) | pickers | the picker control, the file control and their styles |
| `wpf/InputControls.xaml(.cs)` (**shared with `dropdown`**) | pickers | **only** the "File input" and "Date &amp; time" sections plus `Browse_Click`. Don't touch Tag input, Autocomplete, Searchable select, Multi-select or `ClaudeInput` |
| `wpf/AdvancedControls.xaml(.cs)` (**shared with `tables`**) | toasts: the "Toasts" block, `_toasts`, `ShowToast`, `*Toast_Click`, the `ToastHost.ItemsSource` line. overlays: the "Dialogs" block, `CommandPalette_Click`, `LoadingModal_Click`, `Drawer_Click` | delete only these |
| `wpf/MainWindow.xaml` (**shared**) | toasts, overlays | replace the `<!-- Overlay -->` block with `<local:OverlayControls x:Name="Overlays"/>` and `<local:ToastControls x:Name="ToastDemos"/>` (each agent adds its own line) |
| `wpf/MainWindow.xaml.cs` (**shared**) | toasts, overlays | delete `OpenDialog_Click` (overlays). One `Install` line each in the constructor after `InitializeComponent`. Add the control to `ApplyStandaloneMode`'s `panels` array and its `Visibility` line, copying the `AdvancedTables` pattern. Nothing else |
| `wpf/DialogWindow`, `LoadingWindow`, `DrawerWindow`, `CommandPaletteWindow` (`.xaml` + `.cs`) | overlays | **delete**, each in the phase that replaces it (no csproj entries to remove) |
| `wpf/App.xaml` (**shared**) | each agent | **append** its own new theme as the **last** merged dictionary, never in the middle (`Pickers.xaml` uses `IconButton`/`IconChevron` from `Overlays.xaml`, which lands first, in O1; a dictionary placed above the one it references throws at startup) |
| `wpf/UIComponents.Wpf.csproj` | nobody: wildcards (no entries) |
| `wpf/tests/UIAutomationTests.cs` (**shared**) | each agent | its own assertions. Replace the old ones: overlays the modal block (~550–580) and "Dialog launchers"; toasts the "Toasts" block; pickers "File input" and "Date &amp; time" |
| `wpf/README.md` | each agent, last phase | covered list and assertion count |

**Never edit:** `DropdownPrimitives.cs`, `SearchableSelect.cs`, `Themes/Dropdown.xaml` (dropdown), `Forms.xaml`, `Sheet.xaml`, `Controls.xaml`, `Display.xaml`, `Shimmer.cs`, `App.xaml.cs`, `run-hidden.ps1`. If one of them blocks you, ask.

**Shared files:** re-read each one immediately before editing, make small `Edit`s, add only your own lines, and never reorder other people's entries.

**Proposed rows for the README ownership table** (the reviewer adds them):

| Area | Owner |
|---|---|
| `Toasts.cs`, `Themes/Toasts.xaml`, `ToastControls.xaml(.cs)`; the Toast variants and Toasts sections; removing the old toast block from `AdvancedControls` | `toasts` |
| `OverlayHost.cs`, `Themes/Overlays.xaml` (incl. `IconButton`), `OverlayControls.xaml(.cs)`; removing the four `*Window` classes, MainWindow's "Overlays" block and `AdvancedControls`' "Dialogs" block | `overlays` |
| `CalendarPicker.cs`, `FileInput.cs`, `Themes/Pickers.xaml`; the File input and Date picker / Date &amp; time picker sections of `InputControls` | `pickers` |

---

## Dependencies

| On | What | Blocks |
|---|---|---|
| `dropdown` (running, Phase 1 under review) | `DropdownAdorner` hosting proof approved; the outside-click helper lifted into `DropdownPrimitives.cs` (reviewer to ask) | `pickers` P1 |
| `dropdown` | `InputControls.xaml(.cs)` is edited by both. `~Key`/`^text` capture steps exist; **`~Tab` was removed** (it crashed the app) | `pickers` must re-read the file before every edit |
| `tables` (running) | `AdvancedControls.xaml(.cs)` is shared; reuse `TableFilterInput`, `DangerButtonSm` | `toasts` T2 and `overlays` O5 delete blocks there: do it while `tables` is idle, or with small edits after a re-read |
| `display` (running) | `PrimaryButtonSm`, `SecondaryButtonSm`, `Icon*` (exist); the `MainWindow.xaml(.cs)` blocks | none, reuse only |
| `toasts` T1 | `Toasts.Show` | `overlays` O1 (confirm toast). Until it lands, O1 may call a one-line stub `// ponytail: Toasts.Show lands in T1` |
| `overlays` O1 | `IconButton`, `IconChevron`, `IconCalendar`, `IconClock`, `IconUpload`, `IconFile` | `pickers` P1/P4 |
| 06 navigation (**unplanned**) | full-size `PrimaryButton`/`SecondaryButton`/`DangerButton` still use the opacity hover, no `Shadow1` and no `:active` shift; the tooltip is not used here | none. Their look is an accepted difference here (as in 05), so measure geometry only. 06 should reuse `IconButton` |

**Follow-ups unblocked by `Toasts.Show`** (raise each with its owner after T1; not part of 07):
- **F1 grid copy:** "Copied" / "N cells copied. Paste into Excel." and "Cells copied. Paste into Excel." (web 3024, 3048). Grid is archived, so the reviewer or a small task does it.
- **F2 display progress complete:** "Upload complete" / "All files were uploaded." (3272). Remove the `ponytail:` note in `MainWindow.xaml.cs`.
- **F3 tables bulk bar:** "Rows deleted" / "N row(s) removed." and info "Export started" / "Preparing N row(s) for export." (3369, 3371).

---

## Visual spec (DIPs)

### Section shell (all ten)

- **Title:** `SectionTitle`, Margin `0,46,0,0`.
- **Description:** `SheetSectionDesc`, Margin `0,8,0,0`, verbatim text:
  - Modal form: "A focused form in a dialog. Fields keep the same labels, hints and error states as inline forms."
  - Sidebar modal form: "Slide-out panels for longer forms. Create a member, or edit one with a delete step that asks for confirmation first."
  - Modal dialog: "Focused confirmation that interrupts the task. Opens from this button; closes via the button, the backdrop, or Escape."
  - Loading modal: "Blocks every action while work is in progress. It can't be dismissed by clicking the backdrop or pressing Escape — the task closes it when it finishes." (3 lines)
  - Date picker: "One component, three granularities. Pick a full date, a month and year, or just a year."
  - Date &amp; time picker: "The same picker with an hour and minute step for full dates. Month and year modes stay date-only."
  - Command palette: "A search-driven action menu. Press Ctrl+K (or Cmd+K) anywhere, or use the button below, to jump to an action."
  - Toast variants: "Toasts with a manual close button, an inline action, or persistence until dismissed."
  - Toasts: "Brief, non-blocking feedback that appears in the corner and dismisses itself." (1 line)
  - File input: "Drop files onto the area or click to browse. Selected files are listed with their size and can be removed individually."
- **Card:** a chrome-free `GroupBox` with an id, Margin `0,22,0,0`, around a `Border Style=DemoCard`.
- **Button rows:** horizontal, 12px gaps, at (31,31).

| Section | Card id | Demo height | Buttons (style, width×42 unless stated, id) |
|---|---|---|---|
| Modal form | `FormModalCard` | 104 | "Create project" Primary 125.4 `OpenFormModal` |
| Sidebar modal form | `DrawerCard` | 104 | "Invite a member" Secondary 139.7 `OpenDrawer`, "Edit member" Secondary 117.5 `OpenEditDrawer` |
| Modal dialog | `ModalCard` | 104 | "Open dialog" Primary 114.3 `OpenModal` |
| Loading modal | `LoadingCard` | 104 | "Start loading" Primary 117.9 `OpenLoading` |
| Command palette | `CmdkCard` | **109** | "Open command palette" + kbd chip "Ctrl K", Secondary **234.5×47**, `OpenCmdk`. The chip is 40.4×23 at button x 176 (8px gap), 11px `Ink3`, 1px `Line`, radius 5, padding 3/6. The chip makes the button 47 tall |
| Toast variants | `ToastVariantsCard` | 104 | "Dismissible" 107.1 `ToastDismissible`, "With action" 109.4 `ToastWithAction`, "Persistent" 97.8 `ToastPersistent` (all Secondary) |
| Toasts | `ToastsCard` | 104 | "Success toast" 119.9 `ToastSuccess`, "Info toast" 96.9 `ToastInfo`, "Error toast" 102.5 `ToastError` (all Secondary) |
| Date picker | `DatePickerCard` | 231.6 | grid-2 |
| Date &amp; time picker | `DateTimeCard` | 327.4 | grid-2, 3 rows |
| File input | `FileCard` | 237.4 idle (428.6 with the seeded files) | grid-2 |

Button widths are ±2 (text advance). The full-size button look is out of scope (06).

### Overlay primitives (`OverlayHost.cs`, `Themes/Overlays.xaml`)

| Key | Spec |
|---|---|
| `OverlayLayer` | See Decision 1. **Open:** shows `OverlayScrim` (`#6B1E1A14`, fills the layer, hit-testable) and the dialog. Centred dialogs get `Margin 20` (the `.overlay` padding) and are centred in both axes. The drawer is right-aligned at full height |
| `ModalCard` (Border) | `Surface`, 1px `Line`, radius **16**, `Padding 26`. **`Shadow2` on a sibling background `Border`**, never on the content (an Effect rasterises the text; same rule as 03). Width: modal **460**, form **540**, loading **380**. Height comes from the content |
| `ModalTitle` | Serif 21px **600** (Palatino: see 05's SemiBold/Bold check and use the same answer), `LineHeight 26.25`, `Ink` |
| `ModalBody` | 14px, `LineHeight 22.4`, `Ink2`, wrap, top margin 10 |
| modal foot | right-aligned, 10px gaps, top margin 24. Buttons are 42 tall |
| `IconButton` (Button) | 32×32, radius 7, transparent, the icon 18×18 centred, `Foreground Ink3`. **Hover:** `BgDeep` background and `Ink` icon. **Keyboard focus:** a 2px `Brand` ring 2px outside (`Margin -4`, radius 11), on `IsKeyboardFocused` (the forms quirk is accepted: it also shows after programmatic focus, which is what the web ref shows on the modal's X). `FocusVisualStyle {x:Null}`. `IconButtonSm`: 26×26 with a 15px icon (file rows) |

### Modal dialog (`web-overlays-modal-open.png`; rel to the 460×206.8 card)

- **Title** "Delete this project?" at (27,27), 185.4×26.3.
- **Close** `IconButton` at (401,27); `i-close` 18 at (408,34). Id `ModalClose`, Name "Close dialog".
- **Body** at (27,69), 406 wide, 2 lines (44.8): "This will permanently remove the project and all of its contents. This action can't be undone."
- **Foot** at y 137.8:
  - "Cancel" `SecondaryButton` 77.9 at x 219.7, `ModalCancel`
  - "Delete project" `DangerButton` 125.4 at x 307.6, `ModalConfirm`
- **Card:** 460×206.8 ±1, centred in `OverlayLayer` ±1. The dialog root is a chrome-free `GroupBox` `ModalDialog` (Name "Delete this project?").

### Loading modal (`web-overlays-loading-open.png`; rel to the 380×214 card; everything centred)

- **Spinner:** 36×36 at (172,27).
  - An `Ellipse` ring, 3px `BrandSoft`, plus a 3px `Brand` top quarter arc. The arc is a `Path`: the `border-top` of a CSS circle covers the 90° segment from −45° to +45° around 12 o'clock.
  - `RotateTransform` around the centre. Id `LoadingSpinner`.
- **Title** "Preparing your workspace": `ModalTitle`, centred, at y 81.
- **Text** "Please wait a moment while we set things up for you.": 14px `Ink2`, `LineHeight 22.4`, centred, 2 lines, at y 116.3 (top margin 9).
- **Bar** at (27,183), 326×4:
  - track: `BgDeep`, radius 2
  - fill: 35% (114.1) `Brand`, radius 2, clipped to the track (`ClipToBounds` on a plain rectangle is fine here)
  - id `LoadingBar`
  - animated `TranslateTransform.X` from −100% to +390% of the fill width, 1.2s ease-in-out, Forever (Decision 9 guard)
- **The web draws a 2px `Brand` focus outline around the whole card,** because the focused `tabindex=-1` modal matches `:focus-visible`. This is web issue **W2**. Port no ring unless the reviewer keeps W2.

### Modal form (`web-overlays-form-modal-open.png`, `-errors.png`; rel to the 540 card)

- **Card height:** 515.8, and **542** with both errors.
- **Head:** "Create a project" at (27,27). Close at (481,27), `FormModalClose`.
- **Body** "Give the project a name and an owner. You can change both later." at (27,69), 1 line.
- **Form grid** at y 113.4: two columns of 235 with a 16 gap (x 27 and 278) and a 16 row gap. Field = label (20.8) + 7 + control:
  - "Project name": `FieldInput` 45, placeholder "Website refresh", `FmName`. Error "Give the project a name." `FmNameError`.
  - "Owner email": `FieldInput`, "you@example.com", `FmEmail`. Error "Enter a valid email address." `FmEmailError`.
  - "Team" (wide, y 202.2): `FieldSelect` 486×45 with Design / Engineering / Marketing, `FmRole`.
- **"Description"** label at y 291, `FieldTextArea` 486×**104** at y 318.8, "What is this project for?", `FmDesc`.
- **Foot** at y 446.8:
  - "Cancel" Secondary 77.9 at x 299.8, `FormModalCancel`
  - "Create project" Primary 125.4 at x 387.6, `FormModalSubmit`
- **Error row** (forms' error pattern: `ErrorIcon` 13 + 5 gap + 12px `Danger` text, `LineHeight 19.2`): at y 193.2 under each input (input bottom + 7). It is `Collapsed` until the error shows. The input switches to `FieldInputError`. With either error row shown, everything below moves down **26.2**.

### Sidebar modal form (`web-overlays-drawer-*.png`; rel to the drawer's outer box, which includes its 1px left border)

- **`Drawer` (Border):**
  - 440 wide, full layer height, right-aligned
  - `Surface`, a 1px `Line` **left** border, `Shadow2` (sibling Border)
  - layout: a 3-row `Grid` (head Auto, body `*`, foot Auto)
- **Head:** `Padding 26,24,26,20`, 1px `Line` bottom border, **77** tall.
  - Title (`ModalTitle`) at (27,24).
  - Close `IconButton` at (382,24).
- **Body:**
  - `ScrollViewer` (vertical auto, reuse `TableThinScrollViewer` if it fits, else default), `Padding 26,24`.
  - A `StackPanel` with **20** gaps between children.
  - Intro 14px `Ink2` `LineHeight 22.4` at y 101.
- **Foot:**
  - `Padding 26,18`, 1px `Line` top border, **79** tall, at the bottom
  - buttons right-aligned, 10 apart, 42 tall, at foot y + 19
- **Invite** (`DrawerInvite`, title "Invite a member", close `DrawerClose`):

  | Element | y | Spec |
  |---|---|---|
  | intro | 101 | "Add someone to this workspace. They'll get an email with a link to join." (2 lines) |
  | Email address | 165.8 | input 387×45 at 193.6, placeholder "teammate@example.com", `DrEmail`; hint "Use their work email so we can find their account." at 245.6; error "Enter a valid email address." at **271.8**, **below the hint, which stays visible**, `DrEmailError` |
  | Role | 284.8 | `FieldSelect` at 312.6: Member/Admin/Viewer, `DrRole` |
  | Personal note | 377.6 | `FieldTextArea` 104 at 405.4, "Add a short message…", `DrNote` |
  | switch | 529.4 | `ClaudeSwitch` "Send an invitation email", checked, `DrSendEmail` |
  | foot | | "Cancel" 77.9 at x 219.6 `DrawerCancel`; "Send invite" Primary 106.5 at x 307.5 `DrawerSubmit` |

  The error moves everything below it down 26.2.
- **Edit** (`DrawerEdit`, title "Edit member", close `EditDrawerClose`):
  - Intro "Update this member's details and workspace role." (1 line).
  - Full name (label 143.4, input 171.2) "Jamie Rivera" `EdName`.
  - Email address (236.2 / 264) "jamie@example.com" `EdEmail`; error "Enter a valid email address." `EdEmailError`.
  - Role (329 / 356.8) `EdRole` "Member".
  - Switch at 421.8 "Active in workspace", checked, `EdActive`.
  - **Foot:**
    - "Delete" at x 27, **64.1×42** (`DangerButtonSm` look stretched to 42: web quirk **W3**), `EditDelete`
    - "Cancel" 77.9 at x 204.8, `EditCancel`
    - "Save changes" Primary 121.3 at x 292.7, `EditSave`
- **Confirm** (`EditConfirm`, `web-overlays-drawer-edit-confirm.png`):
  - A `Surface` panel over the whole drawer (head included), `Padding 26`.
  - A 320-wide card, centred both ways (y = (H − 193)/2):
    - circle 42 `DangerSoft` with `IconAlert` 20 `Danger` (card y 0)
    - title "Delete this member?" (`ModalTitle`) at +58
    - text "Jamie will lose access to this workspace immediately. This can't be undone." 14px `Ink2`, centred, 2 lines, at +84.2
    - actions at +151, centred, 10 apart: "Keep member" Secondary 124.8 `KeepMember`; "Delete member" `DangerButton` 134.1 `ConfirmDelete`

### Command palette (`web-overlays-cmdk-*.png`; rel to the 560 card)

- **Card:** 560 wide, `Surface`, 1px `Line`, radius 16, `Shadow2`. Clip the content to radius 15 (`RectangleGeometry` on `SizeChanged`, as in `SheetGrid.UpdateClip`): the active row and the list must not paint into the corners.
- **Position: centred in the layer** (the web's `.cmdk-overlay` top alignment is dead CSS, web issue **W1**). If the reviewer fixes W1 first, use `VerticalAlignment Top` with top = 12% of the layer height instead. Either way it is horizontally centred.
- **Heights:** 436.6 with all items, 298.2 for "go", 191 empty. A centred box moves while you filter; replicate that.
- **Search row** (56 tall, `Padding 18,16`, 1px `Line` bottom border):
  - `IconSearch` 18 `Ink3` at (19,19.5)
  - borderless `TextBox` (`CmdkInput`, Name "Search commands") 16px `Ink`; its box starts at x 48 and the text at **x 50**; placeholder "Search for an action…" `Ink3`, visible while focused
  - an "ESC" chip at (508.8,17), 32.2×23 (11px `Ink3`, 1px `Line`, radius 5, padding 3/6)
- **List:** `ScrollViewer` with `MaxHeight 340` (padding included) and `Padding 8`. Use the 03 scroll-bar look (11 wide); reuse `DropdownScrollBar` by key, scoped.
  - **Group header:** 11px 600 `Ink3`, `Padding 10,8,10,4`, 29.6 tall.
  - **Item** (Button): radius 8, `Padding 11,10`.
    - `IconX` 16 `Ink3`, then an 11 gap, then a 14px `Ink` label (text at x 47 in the card), then the shortcut chip right-aligned 11 from the item edge
    - **height 39, or 41 with a shortcut chip** (chip 21 tall: 11px, padding 2/6, 1px `Line`, radius 5). The uneven heights are a web quirk; replicate them
    - active or hover: `BgDeep`
  - **Rows** (item y in the card): Actions header 65; items 94.6 / 135.6 / 174.6; Navigation header 213.6; items 243.2 / 282.2 / 321.2 / 360.2; Appearance header 399.2; item 428.8 (below the fold; the list scrolls, content 421 vs 340).

  | # | Group | Label | Icon | Shortcut | Run |
  |---|---|---|---|---|---|
  | 0 | Actions | Create a project | `IconPlus` | Ctrl N | toast success "Create project" / "A new project draft was started." |
  | 1 | Actions | Invite a member | `IconMore` | | toast info "Invite member" / "Opening the invite form." |
  | 2 | Actions | Export workspace | `IconDownload` | | toast info "Export" / "Preparing your export." |
  | 3 | Navigation | Go to Buttons | `IconButtonGlyph` | | scroll the "Buttons" section title into view |
  | 4 | Navigation | Go to Data table | `IconTable` | | "Data table" |
  | 5 | Navigation | Go to Grid input | `IconTable` | | "Grid input" |
  | 6 | Navigation | Go to Date picker | `IconCalendar` | | "Date picker" |
  | 7 | Appearance | Toggle sidebar | `IconPanel` | Ctrl \ | Q3 (WPF has no collapsible sidebar) |

- **Empty:** "No matching commands", 14px `Ink2` centred, `Padding 16,28`, 78.4 tall, `CmdkEmpty`.
- **Foot:** 1px `Line` top border, `Padding 16,10`, 16px gaps, 11px `Ink3` `LineHeight 17.6`, 38.6 tall. Three items: bold "↑↓" + " to navigate", "↵" + " to select", "esc" + " to close". Bold = 600 `Ink2`.

### Toasts (`web-overlays-toasts-shown.png`, `-toast-variants-shown.png`; rel to the toast)

- **Stack:** Decision 2. **Newest at the bottom** (append order).
- **Toast** (Border): 360 wide, radius 12, 1px border, `Padding 16,14`, `Shadow2` (sibling Border).

  | Type | Background | Border | Accent (icon, title) | Icon |
  |---|---|---|---|---|
  | success | `#E8F2EC` | `#D7E7DD` | `Success` | `IconCheck` |
  | info | `#EAF0F6` | `#D9E4EE` | `Info` | `IconInfo` |
  | error | `#F9EAE7` | `#F0DCD8` | `Danger` | `IconClose` |

- **Icon circle:** 24 at (17,15), `#BFFFFFFF`, icon 14 at (22,20).
- **Body** at x 52 (11 gap). Width 291, or 262 when there is a close button.
  - **Title:** serif 15px 600 `LineHeight 19.5`, accent colour.
  - **Text:** 13px `Ink2`, but **in a 24px line box** (the web span is inline inside a 24px line). So a one-line toast is **73.5** tall and a two-line one **97.5**. Use `LineHeight 24` + `BlockLineHeight` and measure the ink row against the ref.
  - **Action** (Button, optional): 13px 600 `Brand`, `Padding 0,2`, top margin 6, 21 tall, so the toast is **100.5**. Hover underline. Id `Toast<n>_Action`.
- **Close** (dismissible or persistent): 18×18 at (325,15), radius 5, padding 2, `IconClose` 14 `Ink3`. Hover: `Ink` icon and `#99FFFFFF` background. Name "Dismiss", id `Toast<n>_Close`.
- **Ids:** `Toast<n>`, a running index per app run. A chrome-free `GroupBox` with Name = the title.
- **Texts:**
  - Toasts section: success "Saved" / "Your changes have been saved."; info "Heads up" / "A new version is available."; error "Upload failed" / "The file could not be uploaded."
  - Variants: info "New update available" / "Restart to apply the latest changes." (dismissible); success "Message sent" / "Delivered to the team." (action "Undo", dismissible); error "Connection lost" / "We could not reach the server. Check your network." (persistent + dismissible, 2 lines)

### Pickers (`web-pickers-*.png`; trigger rel to itself, panel rel to its outer box)

- **Trigger** (`ToggleButton`, `PickerTrigger` style): 450×**46**, `Surface`, 1px `LineStrong`, radius 8.
  - Icon 16 `Ink3` at **(14,15)**: `IconCalendar`, or `IconClock` in the date-time section, including its three date-only pickers (web issue **W4**: port whichever icon the web has when you start).
  - Value at **x 39**, y 12.5 (B1: +39): 15px, `LineHeight 21`, ellipsis, `Ink3` while it is the placeholder, `Ink` once set.
  - Hover border `Ink3`.
  - **Checked (open):** border `Brand` + a 3px `BrandSoft` ring (`Margin -3`, radius 11).
  - Keyboard focus while closed: the 2px `Brand` ring (`Margin -4`, radius 12, as `DropdownTrigger`).
- **Panel** (`PickerPanel`, through `DropdownAdorner`, Decision 3): at y +52, **300** wide, `Surface`, 1px `Line`, radius 12, `Shadow2` (sibling Border), `Padding 12`.

  | Panel height | Contents |
  |---|---|
  | **316.6** | date, 5 week rows (November 2026). 4 rows = 282.6, 6 rows = 350.6 |
  | **262** | month |
  | **299** | year |
  | **398** | date + time (5 rows) |

- **Head** (32 tall, at (13,13)):
  - Prev and next `IconButton`s with `IconChevron` 18, rotated **90°** (prev) and **−90°** (next). Ids `<Id>_Prev`/`_Next`, Names "Previous" / "Next".
  - Title: serif 14px 600 `Ink`, centred, `<Id>_Title`.
  - 8px below the head, the body starts at y 53.
- **Date body:**
  - 7 columns of **37.4** with 2 gaps.
  - Weekday row "S M T W T F S": 11px `Ink3`, 27.6 tall.
  - Cells start at y **82.6**, rows every **34** (32 + 2).
  - Leading blank cells = weekday of the 1st (Sunday first).
- **Cell** (Button): 32 tall, radius 7, 13px **500** `Ink`, centred. Hover `BgDeep`.
  - **Today:** an inset 1px `LineStrong` ring (an inner Border with radius 7).
  - **Selected:** `Brand` background, white text, 600.
  - Ids: `<Id>_Day<d>`, `<Id>_Month<0-11>`, `<Id>_Year<yyyy>`.
- **Month body:** 3 columns of **88** with 5 gaps; 4 rows of 32 with 5 gaps (143 tall). Labels Jan…Dec.
- **Year body:** 4 columns of **64.8** with 5 gaps; 5 rows (180). `MaxHeight 232` never scrolls with 20 years.
- **Time row** (`WithTime`):
  - 1px `Line` top border, `Padding-top 12`, top margin 10 (block 71.4).
  - Columns 10 apart: 2 × 132, or 3 × 84.7 with seconds.
  - Each column: a label (12px `Ink2`, `LineHeight 19.2`: "Hour", "Minute", "Second"), a 5 gap, then a `TableFilterInput` 34.2 tall with text "09" / "30" / "00".
  - Ids `<Id>_Hour`/`_Minute`/`_Second`. No spin buttons (Chromium shows them only on hover).
- **Foot:** 1px `Line` top border, `Padding-top 10`, top margin 10 (43).
  - "Clear" `GhostButtonSm` 56×32 on the left, `<Id>_Clear`.
  - On the right: "Today" (60.9) or, with time, "Apply" (60.4), both `PrimaryButtonSm`. Ids `<Id>_Today`/`_Apply`.
- **Instances:**

  | Id | Section | Label | Mode | Time | Placeholder |
  |---|---|---|---|---|---|
  | `PickDate` | Date picker | Full date | date | – | Pick a date… |
  | `PickMonth` | Date picker | Month and year | month | – | Pick a month… |
  | `PickYear` | Date picker | Year only | year | – | Pick a year… |
  | `PickDateTime` | Date &amp; time picker | Full date and time | date | yes | Pick a date and time… |
  | `PickDateTimeSec` | Date &amp; time picker | Full date and time with seconds | date | +seconds | Pick a date and time… |
  | `PickDateOnly` | Date &amp; time picker | Full date only | date | – | Pick a date… |
  | `PickMonth2` | Date &amp; time picker | Month and year | month | – | Pick a month… |
  | `PickYear2` | Date &amp; time picker | Year only | year | – | Pick a year… |

  Grid-2 rows at y 31 / 126.8 / 222.6 (label) and 58.8 / 154.6 / 250.4 (trigger). Columns at x 31 and 503.

### File input (`web-file-input*.png`; rel to the demo card)

- **Field:** label (20.8), 7, then `.file-input`: a column with **12** gaps (drop zone, list, count).
  - **The empty list still takes its 12px gap.** So the idle demo is 31 + 20.8 + 7 + 135.6 + 12 + 31 = 237.4.
  - The count is `Collapsed` at 0 files.
- **Drop zone** (`FileDrop`, a Button): 450×**135.6**, `Surface2` background, radius 12, `Padding 20,26`, content centred with 8 gaps.
  - **Border:** 1px **dashed** `LineStrong`, **dash 3 / gap 2** (pixel-probed). A `Border` can't dash, so draw a `Rectangle` (`RadiusX/Y 12`, `StrokeDashArray "3 2"`, `StrokeThickness 1`, `SnapsToDevicePixels`) behind the content. The CSS says 1.5px; Chromium rounds it to 1 at scale 1.
  - **Content:** `IconUpload` 24 `Brand` at y 27; "Drop a file here" / "Drop images here" 14px **Bold** `Ink`, `LineHeight 22.4`, at y 59; "or click to browse" / "or click to browse · images only" 12px `Ink3`, `LineHeight 19.2`, at y 89.4.
  - **Hover or drag-over:** border `Brand`, background `BrandSoft`.
  - **Keyboard focus** (`.file-drop:focus-within`): border `Brand` + a 3px `BrandSoft` ring (`Margin -3`, radius 15).
  - Ids `FileSingle_Drop`, `FileMulti_Drop`.
- **List:** 8 gaps. In `Multiple` mode: `MaxHeight 220`, vertical scroll, and a 4px right padding, so items are **446** wide (single mode: 450).
- **Item** (Border): **48** tall, `Surface`, 1px `Line`, radius 8, `Padding 12,10`, 10 gaps.
  - `IconFile` 16 `Ink3` at x 13.
  - Name: 14px `Ink`, ellipsis, starting at x 39.
  - Size: 12px `Ink3`, tabular, right-aligned.
  - Remove: `IconButtonSm` 26 (`IconClose` 15), Name "Remove file" (or "Remove &lt;name&gt;" if **W11** is fixed).
  - Ids `<Id>_Item<i>` (Name = file name), `<Id>_Remove<i>`.
- **Count:** 12px `Ink2`, tabular: "1 file selected" / "N files selected", at list bottom + 12. Id `<Id>_Count`.
- **Seeded rows** (y in the card): single 206.4; multi 206.4 / 262.4 / 318.4; counts at 266.4 and 378.4.

---

## Behaviour spec (every web branch)

**Verify column:**
- **U:** UIA assertion.
- **K:** UIA plus `PostKey` (`PostMessage` WM_KEYDOWN/UP to the main HWND; the 03 probe passed for Enter, Up, Down and Escape; Tab is probed in O0).
- **P:** capture or pixel check.
- **C:** code-only; the user checks it by hand.
- Never use `SendKeys`, `SendInput` or `keybd_event`. PostMessage can't set modifier state, so **Ctrl+K and Shift+Tab are C**.

### Overlay host (JS 2094–2138)

| # | Branch | Verify |
|---|---|---|
| O1 | **Open:** remember `Keyboard.FocusedElement`, show the scrim and the dialog, apply the blur (Q1), then focus the given element. If none is given, focus the first `input, button` in the dialog. Focus with `Dispatcher.BeginInvoke(Input)` (the 03 gotcha: the element isn't loaded yet) | U (after Invoke `OpenModal`: `ModalClose.HasKeyboardFocus`) |
| O2 | **Close:** hide the dialog. Hide the scrim only if no other dialog is open. Restore the remembered focus if it is still connected (`PresentationSource.FromVisual(el) != null`) | U (focus back on `OpenModal`) |
| O3 | **Backdrop click:** a mouse-up on `OverlayScrim` itself (not on the dialog), unless blocking → close. The web uses `click` with `target === overlay`, so use `MouseLeftButtonUp` on the scrim | C |
| O4 | **Escape:** close the open non-blocking dialog (Decision 8 routing). Blocking (loading) ignores it | K (and P: `!OpenModal!~Escape` shows no dialog) |
| O5 | **Tab trap:** `TabNavigation=Cycle` on the dialog root. Tab from the last element goes to the first; Shift+Tab from the first goes to the last. No focusables (loading) → focus stays on the card (`Focusable=True`, no ring) | K for Tab if the O0 probe passes, else C; Shift+Tab C |
| O6 | Scroll lock (Decision 7) | C |
| O7 | Only one dialog is open at a time. Ctrl+K while one is open is swallowed (`Handled`) | C |

### Modal dialog (2140–2145)

| # | Branch | Verify |
|---|---|---|
| M1 | Open → focus `ModalClose` | U |
| M2 | Close (X), Cancel → close | U |
| M3 | Delete project → close, then toast success "Deleted" / "The project was removed." | U (a toast named "Deleted" exists) |

### Loading modal (2147–2155)

| # | Branch | Verify |
|---|---|---|
| L1 | Open, blocking, focus the card. Backdrop and Escape do nothing | U (Escape via K: still open) |
| L2 | After **3000ms** (`DispatcherTimer`, one-shot): close, then toast success "Ready" / "Your workspace is ready." | U (poll ≤ 4s: dialog gone and "Ready" toast present) |
| L3 | Spinner and bar run only when allowed (Decision 9) | C (code review) |

### Modal form (2157–2178)

| # | Branch | Verify |
|---|---|---|
| F1 | **Open:** reset the form (name and email empty, Team = Design, description empty, both errors hidden, inputs back to `FieldInput`), then focus `FmName` | U |
| F2 | **Submit** (button, or **Enter** in a single-line TextBox of the form; never from the textarea, which keeps `AcceptsReturn`): name valid = `Trim().Length > 0`; email valid = `^[^\s@]+@[^\s@]+\.[^\s@]+$` on the trimmed value. Show or hide each error independently. If either is invalid, stop | U (empty submit → both errors visible) |
| F3 | **Valid:** close, toast success "Project created" / `“<trimmed name>” is ready.` (curly quotes), then reset | U (SetValue name "Atlas" and email "a@b.co", Invoke submit → toast "Project created" with text containing "“Atlas”") |
| F4 | Typing in name or email (`TextChanged`) clears **that** field's error | U |
| F5 | Close (X) and Cancel → close, without validating | U |

### Sidebar drawers (2180–2236)

| # | Branch | Verify |
|---|---|---|
| D1 | **Invite open:** reset (email empty, role Member, note empty, switch **on**, error hidden), then focus `DrEmail` | U |
| D2 | **Invite submit** (button or Enter in `DrEmail`): email rule as F2. Invalid → show the error (the hint stays) and stop. Valid → close, toast success "Invite sent" / "An invitation is on its way to <trimmed email>.", reset | U |
| D3 | Typing in `DrEmail` clears its error | U |
| D4 | **Edit open:** hide the confirm, reset to defaults ("Jamie Rivera", "jamie@example.com", Member, switch on, error hidden), then focus `EdName` | U |
| D5 | **Edit save:** email rule only; the name isn't validated. Valid → close, toast success "Changes saved" / "Jamie's details have been updated." (straight apostrophe) | U |
| D6 | Typing in `EdEmail` clears its error | U |
| D7 | **Delete** → show the confirm (its own Tab cycle, Decision 6), focus `KeepMember` | U |
| D8 | **Keep member** → hide the confirm, focus `EditDelete` | U |
| D9 | **Delete member** → hide the confirm, close the drawer, toast success "Member deleted" / "Jamie no longer has access." | U |
| D10 | **Escape with the confirm visible** → hide the confirm and focus `EditDelete`. **Don't close the drawer** | K |
| D11 | Close (X) and Cancel → close | U |

### Command palette (3184–3249)

| # | Branch | Verify |
|---|---|---|
| K1 | **Open** (button, or **Ctrl+K anywhere** via a window `PreviewKeyDown` with `Keyboard.Modifiers == Control` and `Key.K`): open the overlay, focus `CmdkInput`, clear the text, render all items, active = 0 | U (button); Ctrl+K C |
| K2 | **Filter** on `TextChanged`: `label.ToLower().Contains(query.Trim().ToLower())`; group headers only for groups with matches; active = 0. No matches → only `CmdkEmpty` | U ("go" → 4 items, 1 header; "zzz" → `CmdkEmpty`) |
| K3 | **Down/Up** (in the input): `active = (i + n) % n`, so it **wraps**; Up from 0 goes to the last item. Bring the active item into view (`BringIntoView`, nearest). With no items, nothing happens. Mark it `Handled` | K, P |
| K4 | **Enter** with an active match → close, then run the command | K |
| K5 | **Click** an item → close, then run it (hover doesn't change the active item) | U (Invoke `CmdkItem0` → toast "Create project") |
| K6 | "Go to …" → `BringIntoView()` the matching `SectionTitle` (no smooth scroll; in standalone mode a hidden target does nothing) | U |
| K7 | Escape, backdrop, Tab → O4, O3, O5 | K/C |

Items expose `AutomationProperties.ItemStatus` "active" on the active one, and Name = the label.

### Toasts (3376–3418)

| # | Branch | Verify |
|---|---|---|
| T1 | `Show` appends to the bottom of the stack (stack layer above everything) | U, P |
| T2 | Auto-dismiss after **3600ms** unless persistent (`DispatcherTimer` per toast) | U (poll ≤ 4.5s: a plain toast is gone, the persistent one is still there) |
| T3 | The close button (dismissible or persistent) → dismiss: stop the timer and remove it now (Decision 9) | U (Invoke `_Close`) |
| T4 | Action → show success "Done" / "The action was completed.", then dismiss this toast | U |
| T5 | Live region: `AutomationProperties.LiveSetting = Polite` on each toast, and raise `LiveRegionChanged` on show (.NET 4.8 has both; web `aria-live="polite"`) | C |

### Pickers (2526–2667)

| # | Branch | Verify |
|---|---|---|
| P1 | **Init:** view = today's month and year, selection = none, value = the placeholder. The panel isn't built until opened | U |
| P2 | **Trigger toggle:** open → render, then focus the selected cell if there is one (else focus stays on the trigger). Close → hide (no focus change). The `ToggleButton` state = `aria-expanded` | U |
| P3 | **Format:** year "2030"; month "May 2030" (3-letter month); date "Nov 14, 2026"; time `", HH:mm"` (+`":ss"`), zero-padded | U |
| P4 | **Cell click:** day → (y, m, d); month → (y, m, 1); year → (y, 0, 1). **Without time** → commit (set the value, `Ink`, close). **With time** → copy the hour, minute and second from the inputs (`int.TryParse` else 0, **unclamped**), re-render, stay open, **don't commit** | U |
| P5 | **Prev/next:** date ±1 month (wrapping the year); month ±1 year; year ±20. Titles "November 2026" / "2026" / "2018–2037" (start = viewYear − 8, en dash) | U |
| P6 | **Apply** (time): no selection → nothing. Otherwise clamp h 0–23, min 0–59, s 0–59, then commit | U |
| P7 | **Today** (no time): selection = today, then commit (month mode shows "Nov 2026", year "2026") | U (value from `DateTime.Today`) |
| P8 | **Clear:** no selection, placeholder back (`Ink3`), close. The time inputs keep their values (quirk, **W6**) | U |
| P9 | **Escape** while open → close and focus the trigger. **Gotcha:** the panel lives in the adorner, so it is *not* a visual descendant of the control. Handle `KeyDown` on the panel root **and** the trigger | K |
| P10 | **Outside click** (`PreviewMouseDown` outside the trigger and panel) → close, no focus change. A real click on another picker's trigger closes this one through the same rule | C |
| P11 | The view isn't reset when you reopen (the last navigated month stays), and an uncommitted time-mode selection stays highlighted (quirk **W6**) | U |
| P12 | Tab out of the panel → the panel stays open (web; **W5**) | C |

### File input (3136–3182)

| # | Branch | Verify |
|---|---|---|
| FI1 | **Click / Space / Enter** on the drop zone → `OpenFileDialog` (`Multiselect = Multiple`; images: `Filter "Images|*.png;*.jpg;*.jpeg;*.gif;*.webp;*.svg;*.bmp"`) | C (**never invoke it in a test or capture**: the dialog would block the hidden app) |
| FI2 | **add:** multiple → append; single → keep the first chosen file only, replacing the old one | C |
| FI3 | **Drag enter/over** → drag-over look and `Effects = Copy`. **Leave/drop** → normal look. **Drop:** `DataFormats.FileDrop`; in image mode, keep image extensions only | C |
| FI4 | **Remove** → drop that file and re-render | C |
| FI5 | **Size label:** `< 1024` → "N B"; `< 1048576` → `Math.Round(kb)` + " KB" (JS rounding: use `MidpointRounding.AwayFromZero`); else one decimal + " MB". Count "1 file selected" / "N files selected"; hidden at 0 | U only if Q5 adds a seeding hook, else C |

---

## AutomationIds (summary)

| Area | Ids |
|---|---|
| Layers | `OverlayLayer`, `OverlayScrim`, `ToastLayer`, `ToastStack` |
| Section cards | `FormModalCard`, `DrawerCard`, `ModalCard`, `LoadingCard`, `CmdkCard`, `ToastVariantsCard`, `ToastsCard`, `DatePickerCard`, `DateTimeCard`, `FileCard` |
| Openers | `OpenFormModal`, `OpenDrawer`, `OpenEditDrawer`, `OpenModal`, `OpenLoading`, `OpenCmdk`, `ToastSuccess`, `ToastInfo`, `ToastError`, `ToastDismissible`, `ToastWithAction`, `ToastPersistent` |
| Dialogs (chrome-free `GroupBox` roots) | `ModalDialog`, `LoadingDialog`, `FormModalDialog`, `DrawerInvite`, `DrawerEdit`, `CmdkDialog` |
| Inside dialogs | as listed in the visual spec (`ModalClose`, `FmName`, `FmNameError`, `DrEmail`, `EditConfirm`, `CmdkInput`, `CmdkItem<i>`, `CmdkGroup<i>`, `CmdkEmpty`, `LoadingSpinner`, `LoadingBar`, …) |
| Toasts | `Toast<n>`, `Toast<n>_Close`, `Toast<n>_Action` |
| Pickers | `<Id>_Trigger`, `_Value`, `_Prev`, `_Next`, `_Title`, `_Day<d>`, `_Month<m>`, `_Year<y>`, `_Hour`, `_Minute`, `_Second`, `_Clear`, `_Today`, `_Apply` |
| File | `<Id>_Drop`, `_Item<i>`, `_Remove<i>`, `_Count` |

**aria → UIA:**
- dialog `aria-labelledby` → Name of the `GroupBox` root = the title
- `aria-label` "Close dialog" / "Close panel" / "Dismiss" / "Search commands" / "Previous" / "Next" / "Remove file" → Name
- `aria-expanded` → the trigger `ToggleButton` state
- `aria-busy` (loading) → `ItemStatus` "busy"

Old ids that go away: `BtnOpenDialog`, `DialogResult`, `DlgCancel`, `DlgDelete`, `BtnCommandPalette`, `CmdSearch`, `CmdList`, `CmdCreate`, `BtnLoadingModal`, `LoadingBar` (the old one), `BtnDrawer`, `Drawer*`, `BtnSuccessToast`, `BtnInfoToast`, `BtnErrorToast`, `ToastHost`, `ToastItem`, `BtnBrowse`, `FileList`, `DtDate`, `DtTime`. Update or replace their assertions in the same phase.

---

## Capture recipe (hidden) and what the harness can't capture

```powershell
wpf\build.bat <agent>
cd wpf\tests; .\build-tests.bat; cd ..\..
pwsh wpf/tests/run-hidden.ps1 -Agent <agent>
# Overlays: target the full-window layer, open with a ! chain; key/text steps act on the focused element
pwsh wpf/tests/run-hidden.ps1 -Agent overlays -NoTests -Section "Modal dialog" -Capture "ModalCard=plan/wpf/ref/wpf-overlays-modal.png,OverlayLayer!OpenModal=plan/wpf/ref/wpf-overlays-modal-open.png,OverlayLayer!OpenModal!~Escape=plan/wpf/ref/wpf-overlays-modal-escaped.png"
pwsh wpf/tests/run-hidden.ps1 -Agent overlays -NoTests -Section "Command palette" -Capture "OverlayLayer!OpenCmdk=plan/wpf/ref/wpf-overlays-cmdk-open.png,OverlayLayer!OpenCmdk!~Down!~Down=plan/wpf/ref/wpf-overlays-cmdk-down2.png,OverlayLayer!OpenCmdk!~Up=plan/wpf/ref/wpf-overlays-cmdk-up.png,OverlayLayer!OpenCmdk!^go=plan/wpf/ref/wpf-overlays-cmdk-filter.png,OverlayLayer!OpenCmdk!^zzz=plan/wpf/ref/wpf-overlays-cmdk-empty.png"
# Toasts: the toast layer is also full-window
pwsh wpf/tests/run-hidden.ps1 -Agent toasts -NoTests -Section "Toasts" -Capture "ToastsCard=plan/wpf/ref/wpf-overlays-toasts.png,ToastLayer!ToastSuccess!ToastInfo!ToastError=plan/wpf/ref/wpf-overlays-toasts-shown.png"
# Pickers: the panel overflows the card, so capture ContentScroll
pwsh wpf/tests/run-hidden.ps1 -Agent pickers -NoTests -Section "Date picker" -Capture "DatePickerCard=plan/wpf/ref/wpf-pickers-date.png,ContentScroll!PickDate_Trigger=plan/wpf/ref/wpf-pickers-date-open.png,ContentScroll!PickDate_Trigger!PickDate_Day14!PickDate_Trigger=plan/wpf/ref/wpf-pickers-date-selected.png"
```

| State | Capture | How |
|---|---|---|
| Every idle section | P | the `*Card` id |
| Modal / form modal / drawers / loading / palette open | P | `OverlayLayer!Open…` |
| Form and invite errors (empty submit) | P | `OverlayLayer!OpenFormModal!FormModalSubmit`, `OverlayLayer!OpenDrawer!DrawerSubmit` |
| Edit confirm | P | `OverlayLayer!OpenEditDrawer!EditDelete` |
| Escape closes; palette Down/Up/filter/empty | P | `~Escape`, `~Down`, `~Up`, `^go`, `^zzz` (the opener focuses the input or the X first) |
| Toast stack, toast variants | P | `ToastLayer!Toast…` chains |
| Picker open, selected, month, year, time row, values | P | `ContentScroll!<Id>_Trigger!<Id>_Day14…` (a `ToggleButton` flips `IsChecked`; cells are `Button`s) |
| **Valid-email submit states** | **no** | `^text` can't contain `@`. UIA `SetValue` covers the path (U) |
| **Tab / Shift+Tab focus trap** | **no** | `~Tab` was removed (crash). Tab is K if the O0 probe passes; Shift+Tab is C |
| **Ctrl+K** | **no** | no modifier step; PostMessage can't set the modifier state. C |
| **Backdrop click, outside click, every hover state, drag-over, drag-drop** | **no** | no mouse on the hidden desktop. C |
| **File dialog and the "with files" state** | **no** | `OpenFileDialog` would block. Code-only unless Q5 adds a seeding hook |
| **Spinner rotation, bar motion, auto-dismiss and loading timeouts** | **no** (static frame only) | U assertions with timeouts cover the timers. Motion is code review |
| **Focus rings after real keyboard navigation** | partial | `@Id` focuses programmatically, which shows the `IsKeyboardFocused` ring (accepted quirk) |
| Two pickers: a real click on another trigger closes the first | **no** | `!Trigger` raises no `PreviewMouseDown`. C |

**Comparing:** align on the dialog's top-left. Scan for its `#E6E2D7` border; for the drawer, its left border. Compare against the `rel` numbers. Check centring: (layer width − dialog width)/2 ±1. Toasts: right edge = layer right − 24, bottom = layer bottom − 24, ±1.

---

## Phases: one at a time, capture, stop for review

**Agents and order:**
- **`toasts`:** T0 → T1 → T2 → T3. It can start now.
- **`overlays`:** O0 → O1 (after T1, or with the stub) → O2 → O3 → O4 → O5 → O6.
- **`pickers`:** P0 (any time) → P1 (after dropdown Phase 1 approval, the outside-click lift and overlays O1) → P2 → P3 → P4 → P5.

`toasts` and `pickers` P0 can run in parallel with everything. Each phase is self-contained: stop and report after it.

### toasts

**T0 (audit, no code):** build `toasts`, run the tests (counts and wall time), re-capture `wpf-before-overlays-toasts*.png`, and confirm the numbers in this plan against `web-overlays-measurements.txt`.

**T1: toast host + "Toasts" section.**
- `Toasts.cs` (Install, Show, timers, T1/T2/T3/T5), `Themes/Toasts.xaml`, `ToastControls.xaml` with the "Toasts" section.
- The constructor `Install` line; the MainWindow placement and standalone list; App entry (no csproj entries).
- Delete the old toast block and code from `AdvancedControls`; replace the old "Toasts" assertions.
- **Captures:** `wpf-overlays-toasts.png`, `wpf-overlays-toasts-shown.png`.
- **Acceptance:**
  1. Card 104 ±1.
  2. Toasts 360×73.5 ±1, 10 apart, right/bottom 24 ±1 from the layer.
  3. Colours exact: bg, border, accent, icon circle `#BFFFFFFF`.
  4. Icon circle 24 at (17,15); title at x 52, y 15; text ink row ±1 of the ref.
  5. T1/T2 U tests pass, auto-dismiss within 3.6–4.5s.

**T2: "Toast variants" section** (close, action, persistent, T3/T4).
- **Capture:** `wpf-overlays-toast-variants-shown.png`.
- **Acceptance:**
  1. Heights 73.5 / 100.5 / 97.5 ±1.
  2. Close 18×18 at (325,15) with a 14px icon.
  3. Body width 262; "Undo" at (52,64.5), 13px 600 `#A64E2A`.
  4. The persistent toast survives 4.5s; close and action work (U).

**T3: tests, README, follow-up note.** Full run green; record the wall-time delta. In the report, list the F1–F3 call sites for the reviewer.

### overlays

**O0 (audit and probes; code only in the tests):** build, run the tests (counts and wall time).
- **Tab probe:** `PostKey(VK_TAB 0x09)` against the current Forms section. Focus `FormsCard`'s first input, post Tab, and expect focus on the next field. Report pass/fail: it decides K vs C for O5.
- Confirm the four `*Window` usages (grep) and the old test blocks.

**O1: overlay host + "Modal dialog".**
- `OverlayHost.cs`, `Themes/Overlays.xaml` (`ModalCard`, `IconButton`/`Sm`, icons), and `OverlayControls.xaml` with the "Modal dialog" section and `DialogStore` (modal only).
- The constructor `Install` line; MainWindow placement and standalone list.
- O1–O7, M1–M3, the blur if Q1 = yes.
- Delete `DialogWindow`, the MainWindow "Overlays" block and `OpenDialog_Click`; rewrite the modal UIA test (no worker thread, no `WindowPattern`).
- **Prove:** `OverlayLayer` is found before the clicks, `!OpenModal` renders the dialog, UIA finds `ModalClose` from the window root, and closing restores focus.
- **Captures:** `wpf-overlays-modal.png`, `-modal-open.png`, `-modal-escaped.png`.
- **Acceptance:**
  1. Card 460×206.8 ±1, centred ±1; radius 16; `#E6E2D7` border; shadow visible.
  2. Title at (27,27), Palatino 600; X 32×32 at (401,27) with an 18px icon; body at y 69 in 2 lines; buttons at y 137.8, x 219.7 / 307.6 ±2.
  3. The scrim pixel over the page matches the ref ±2 per channel (Q1 decides blur), and the sidebar is covered.
  4. After Escape, the capture shows no scrim.
  5. Tests pass.

**O2: "Loading modal".** L1–L3, spinner and bar with the guard. Delete `LoadingWindow` and the "Dialogs" button for it.
- **Capture:** `wpf-overlays-loading-open.png`.
- **Acceptance:**
  1. Card 380×214 ±1.
  2. Spinner 36 at (172,27): `#F8ECE4` ring with a `#A64E2A` top arc.
  3. Title y 81, text y 116.3 (2 lines, centred), bar at (27,183) 326×4 with a 114 ±1 fill at x 0.
  4. U: closes within 3–4s and the "Ready" toast appears.
  5. The test wall time grows by ≤ 4s.

**O3: "Modal form".** F1–F5, the textarea per Q6.
- **Captures:** `-form-modal-open.png`, `-form-modal-errors.png`.
- **Acceptance:**
  1. Heights 515.8 / 542 ±1.
  2. Every label, input, select, textarea and button rect ±1 (±2 widths) of the list.
  3. The error rows sit at y 193.2 with `#B0433A` input borders.
  4. U tests for F1–F5 pass.

**O4: "Sidebar modal form".** D1–D11. Delete `DrawerWindow`.
- **Captures:** `-drawer-invite.png`, `-drawer-invite-errors.png`, `-drawer-edit.png`, `-drawer-edit-confirm.png`.
- **Acceptance:**
  1. The drawer is 440 wide, full height, flush right, with a 1px `#E6E2D7` left border.
  2. Head 77, foot 79 at the bottom; body rows ±1 of the list.
  3. Delete 64.1×42 (or per W3); confirm card centred with the circle 42.
  4. D10 (K) passes.

**O5: "Command palette".** K1–K7. Delete `CommandPaletteWindow` and the rest of the "Dialogs" block in `AdvancedControls`.
- **Captures:** `-cmdk-open.png`, `-cmdk-down2.png`, `-cmdk-up.png`, `-cmdk-filter.png`, `-cmdk-empty.png`.
- **Acceptance:**
  1. Card 560 wide; 436.6 / 298.2 / 191 ±1; centred (or per W1).
  2. Search row 56; input text x 50; ESC chip at (508.8,17).
  3. Item rows per the table (39/41), groups 29.6, list 340 with the 11px scroll bar (compare against `-scrollbar` refs).
  4. Up from 0 lands on "Toggle sidebar" and scrolls it into view.
  5. Foot 38.6.
  6. U/K tests pass.

**O6: tests, cleanup, README.** No `*Window` files are left, no dead handlers, csproj clean, full run green.

### pickers

**P0 (audit, no code):** build, run the tests, re-capture `wpf-before-overlays-date-time.png` and `-file-input.png`. Re-run `plan/wpf/ref/web-overlays-capture.ps1` the same day (picker refs depend on the date). Confirm the B1 +39 offset and the W4 icon state in the current web.

**P1: `CalendarPicker` shell + date mode** ("Date picker" section with `PickDate` only).
- `Themes/Pickers.xaml`, `CalendarPicker.cs`; App entry (no csproj entries).
- Trigger, panel via `DropdownAdorner`, date grid, prev/next, Today, Clear, Escape, outside click.
- Implements P1–P5, P7–P10.
- **Captures:** `wpf-pickers-date-open.png`, `-date-selected.png`.
- **Acceptance:**
  1. Trigger 450×46; icon at (14,15); value at x 39 ±1.
  2. Panel at y +52, 300×316.6 ±1 (same month as the web ref), radius 12, `#E6E2D7` border.
  3. Title centred; weekday row 27.6; cells 37.4×32 at y 82.6 + 34n; today ring `#D5CFC0`; selected `#A64E2A`, white 600.
  4. Foot buttons 56×32 / 60.9×32.
  5. U: P3 format, P5 titles, P7 Today.

**P2: month and year modes + the rest of the "Date picker" section.**
- **Captures:** `-month-open.png`, `-year-open.png`, `-date-values.png`.
- **Acceptance:**
  1. Month panel 262 with cells 88×32 (5 gaps).
  2. Year panel 299 with cells 64.8×32 and title "2018–2037" (for 2026).
  3. Card 231.6 ±1.

**P3: time modes + the "Date &amp; time picker" section.** P4 time branch, P6, P11. Delete the old "Date &amp; time" section and replace its assertions.
- **Captures:** `wpf-pickers-datetime.png`, `-datetime-open.png` (`!PickDateTime_Trigger!PickDateTime_Day14`), `-datetime-seconds-open.png`, `-datetime-values.png`.
- **Acceptance:**
  1. Panel 398.
  2. Time row at y 260.6: inputs 132 (84.7) × 34.2 at y 297.8.
  3. Apply 60.4×32.
  4. Values "Nov 14, 2026, 09:30" style (month from today).
  5. Card 327.4 ±1.

**P4: `FileInput`** ("File input" section rewrite).
- FI1–FI5; delete `Browse_Click`, the old ListBox and its assertions.
- **Captures:** `wpf-file-input.png` (+ `-files.png` only if Q5 adds a hook).
- **Acceptance:**
  1. Card 237.4 ±1; drop zone 450×135.6 with a 1px `#D5CFC0` dash 3/gap 2, radius 12, `#FAF9F5`.
  2. Icon at y 27, title y 59 Bold, subtitle y 89.4, centred.
  3. Item, list and count specs by code review (or capture with the hook).

**P5: tests, cleanup, README.**

---

## WPF quirks that apply

- **Never `ShowDialog` / `Window` / `Popup`** for any of these: it isn't captured, it blocks the hidden app, and it is outside the UIA root.
- **Adorner-hosted panel:** its key events don't route through the picker control (P9). Remove the adorner on close, so the cells disappear from UIA.
- **Focus on open:** `Dispatcher.BeginInvoke(DispatcherPriority.Input, …)` after making the element visible.
- **`ToggleButton`:** `!Id` sets `IsChecked` without a `Click`, so drive open/close from `Checked`/`Unchecked`.
- **`DropShadowEffect`** blurs whatever it's applied to: put it on a sibling background `Border`. **`BlurEffect`** on the page forces software rendering of that subtree: apply it only while a dialog is open.
- **Rounded clips:** `ClipToBounds` on a rounded `Border` clips to a rectangle. The palette list needs a `RectangleGeometry` clip (radius 15) updated on `SizeChanged`.
- **Line heights:** set the measured heights explicitly (toast text 24-line box, cells 32, rows 39/41, field rows). Don't trust font metrics (±0.5px between WPF and Chromium).
- **TextBox inset:** the content host adds 2–3px. Compensate and measure the typed text (palette input text at x 50).
- **Tests share one app run with every other agent's tests.** A dialog left open traps focus and covers the page, and later tests in other areas would then fail and be blamed on the wrong agent. So:
  - every overlay test closes its dialog in a `finally` (Escape or the close button)
  - the loading test waits until the dialog is gone (≤ 4s) before it continues
  - toast tests dismiss what they create, or wait for the auto-dismiss
- **Timers:** `DispatcherTimer` only (no `System.Threading.Timer` + `Invoke` like the old `LoadingWindow`). Stop each timer when its toast or dialog goes away.
- **The clipboard** isn't used here.
- **C# 5:** no `$""`, `?.`, `nameof`, `=>` members or auto-property initialisers. Regex via `System.Text.RegularExpressions`.

## Reviewer checklist

- [ ] Only the files in the ownership table changed, with each agent in its own rows. `DropdownPrimitives.cs`, `SearchableSelect.cs`, `Forms.xaml`, `Sheet.xaml`, `Controls.xaml`, `Display.xaml` and `App.xaml.cs` are untouched.
- [ ] No `Window`/`ShowDialog`/`Popup`. `OverlayLayer` and `ToastLayer` exist at startup and are never `Collapsed`. Shadows are on sibling Borders. The spinner and bar have the `--capture` and reduced-motion guards.
- [ ] Every O/M/L/F/D/K/T/P/FI branch is implemented as written. U/K tests exist where the table says so; timers are tested with timeouts.
- [ ] I captured every P state myself, hidden, and compared it to the `web-*` refs (same-day picker refs; `-scrollbar` refs for the palette list): ±1px, exact hex.
- [ ] Old windows, sections, handlers and ids are gone. C# 5. Build clean, tests pass, no commits.

---

## Web issues to decide (fix in `web/index.html` before the port, or replicate)

Found while reading the JS and CSS for these sections, and confirmed against the code or the measurements. Ids are `W*`, because `D*` is taken by the drawer branches. For every fix accepted, add a CDP check to `plan/wpf/web-checks.mjs` (now 22/22).

| # | Where | Problem | Suggested fix |
|---|---|---|---|
| **W1** | CSS 224 vs 431 | `.cmdk-overlay{align-items:flex-start; padding-top:12vh; z-index:120}` comes **before** `.overlay` (same specificity), so `.overlay`'s `align-items:center`, `padding:20px` and `z-index:100` win. The palette is **centred instead of top-aligned** and **jumps vertically while you type** (measured top 285.7 → 354.9 for "go" → 408.5 empty, in a 1008 viewport) | Move the `.cmdk-overlay` rule after `.overlay`, or use `.overlay.cmdk-overlay` |
| **W2** | 2150, 2012, CSS 50 | The loading modal is focused (`tabindex=-1`) and matches `:focus-visible`, so a 2px brand outline is drawn around the whole card (visible in `web-overlays-loading-open.png`) | `.loading-modal:focus{outline:none}` |
| **W3** | CSS 743–744 | The edit drawer's "Delete" is `btn--sm`, but the flex foot stretches it to 42 tall (64.1×42 with 13px text, radius 7) | `align-items:center` on `.drawer__foot`, or drop `btn--sm` |
| **W4** | markup 1884, 1890, 1896 | The three date-only pickers in "Date &amp; time picker" (Full date only, Month and year, Year only) use `i-clock`, not `i-calendar` | Use `i-calendar` for date-only modes |
| **W5** | 2658–2664 | Picker: Tab out of the panel leaves it open (there is only a document click handler), the same class of bug as C6 fixed for the combo | Close on `focusout` when focus leaves `root` |
| **W6** | 2620–2647 | Picker, time mode: a day click selects without committing, so closing by Escape or outside click keeps a highlighted selection while the trigger still shows the placeholder. Clear doesn't reset the hour and minute inputs. The view month isn't reset on reopen | Decide: reset the uncommitted selection on close, and reset the time inputs on Clear |
| **W7** | 1845 ff., 2582–2598 | Picker ARIA: `aria-haspopup="dialog"` but the panel has no `role="dialog"`/label/`aria-controls`. Day buttons are labelled only "14" (no month or year), and the selected and today cells have no `aria-pressed`/`aria-current` | Add `role="dialog" aria-label`, full-date `aria-label`s and `aria-pressed`/`aria-current="date"` |
| **W8** | 3200–3218 | Palette ARIA: the input has no `role="combobox"`/`aria-controls`/`aria-activedescendant`, the list no `role="listbox"`, the items no `role="option"`. The active item is visual only (the combo got this in the earlier web fixes) | Mirror the combo's ARIA |
| **W9** | 2165–2216 | Form modal and drawers: no `aria-invalid`, errors not linked with `aria-describedby`, focus not moved to the first invalid field on a failed submit, and the errors aren't announced | Set `aria-invalid` + `aria-describedby`, and focus the first invalid field |
| **W10** | 2091, 3378–3399 | Toasts: error toasts go into a `polite` live region (should be `role="alert"`). Auto-dismiss (3.6s) doesn't pause on hover or focus, including the "With action" toast whose Undo disappears (WCAG 2.2.1). Dismissing a toast whose close button has focus drops focus to `body` | `role="alert"` for errors; pause the timer on `mouseenter`/`focusin`; keep action toasts until dismissed or move focus |
| **W11** | 3171–3180 | File input: dropping zero accepted files (for example dragged text) onto the **single** input clears the current file (`chosen.slice(0,1)` = `[]`). The remove button says "Remove file", not the name. Browse isn't type-filtered (`accept` only filters the dialog) while drop is. Multiple mode keeps duplicates | Ignore empty drops; `aria-label="Remove " + name`; optional de-dupe |
| **W12** | 3190 | Palette "Invite a member" shows an info toast but doesn't open the invite drawer that exists on the page | Call `document.getElementById('openDrawer').click()` (or leave as demo copy) |

---

## Decisions confirmed (2026-11-22)

User answered Q1, Q2, Q4, Q5 explicitly (all as recommended); Q3, Q6, Q7 take the recommendation by default (the user can still overrule them):
- **Q1** blur: yes (`BlurEffect` on the page while a dialog is open). **Q2** transitions: skip (spinner and bar stay). **Q3** "Toggle sidebar": keep as a no-op with a `ponytail:` note. **Q6** textarea grip: attached behaviour, done in overlays O3 with the reviewer's OK. **Q7** scroll-bar gutter difference: accepted.
- **Q4** web fixes BEFORE the port (a `web` worker, each with a CDP check in `plan/wpf/web-checks.mjs`, which must stay green): W1, W2, W4, W5, W7, W8, W9, W10 (role=alert for errors, pause on hover/focus, action toasts persist), W11 (empty drop, remove label, de-dupe). NOT now: W3, W6, W12 (replicate the web as it is). After the fixes: re-run `ref/web-overlays-capture.ps1`, `web-overlays-capture-scrollbar.ps1` and the measure script, and refresh the measurements before overlays O5 (palette), pickers P1 and toasts T1.
- **Q5** file input "with files": code-only, no seeding hook.

## Open questions (user / reviewer)

1. **Q1 Backdrop blur:** `BlurEffect` (Radius ≈ 9, tuned) on the page while a dialog is open, or a plain scrim? *Recommend blur*: it is visible in every ref and costs ~10 lines.
2. **Q2 Transitions:** skip the entrance and exit animations (fade, scale, slide, toast in/out), as in 03 decision 4, while keeping the spinner and bar? *Recommend skip.*
3. **Q3 "Toggle sidebar" command:** WPF has no collapsible sidebar. Keep the item as a no-op with a `ponytail:` note, or drop it? *Recommend keep as a no-op* (the list then matches the web: 8 items, 3 groups, same scroll extent).
4. **Q4 Web fixes first:** which of W1–W12 should the `web` worker fix before the port? *Recommend W1, W2, W4, W5, W10 (role and pause), W11 (empty drop), plus the ARIA items W7–W9.* W3, W6 and W12 are low priority.
5. **Q5 File seeding:** the "with files" state and FI2–FI5 can't be driven hidden (the file dialog blocks; no drop). Leave them code-only (user checks by hand), or have the reviewer add a seeding hook (for example a `--seed-files` argument read by `FileInput`, passed by `run-hidden.ps1`)? *Recommend code-only for now.*
6. **Q6 Textarea grip in dialogs:** the form modal and invite drawer add two more `FieldTextArea`s. The grip handler's `ponytail:` note in `MainWindow.xaml.cs` (forms, archived) says to move it into an attached behaviour "when a second FieldTextArea needs a grip". Do that now (overlays O3, with the reviewer's OK), or ship the dialog textareas without a grip? *Recommend the attached behaviour.*
7. **Q7 Scroll-bar gutter:** accept that WPF toasts sit 24px from the window edge and the drawer is flush right, while the web leaves the 11px gutter uncovered? *Recommend accept.*

---

## Worker kickoffs

```
You are the worker agent "toasts" on the WPF pixel-parity effort (repo C:\Workspace\Projects\others\ui-design).
Read plan/wpf/README.md fully (rules, workflow, ownership, gotchas), then plan/wpf/07-overlays-pickers.md
(Decisions, Files, Toasts visual spec, T table, toasts phases) and the web lines it cites
(index.html CSS 526-545, 706-712; markup 1912-1933, 2091; JS 3376-3418).
Rules: build with wpf\build.bat toasts; test and capture only with wpf/tests/run-hidden.ps1 -Agent toasts; never commit;
never kill processes by name; touch only the toasts rows of the plan's Files table (re-read shared files before each edit).
Do Phase T0 only (audit, no code changes), then stop and report in the README format.
```

```
You are the worker agent "overlays" on the WPF pixel-parity effort (repo C:\Workspace\Projects\others\ui-design).
Read plan/wpf/README.md fully, then plan/wpf/07-overlays-pickers.md (Decisions, Files, overlay specs, O/M/L/F/D/K tables,
overlays phases) and the web lines it cites (CSS 223-245, 430-455, 517-524, 547-551, 729-750; markup 1218-1223, 1811-1835,
1904-1909, 1937-2089; JS 2094-2236, 3184-3249).
Rules: build with wpf\build.bat overlays; test and capture only with wpf/tests/run-hidden.ps1 -Agent overlays; never commit;
never kill processes by name; never use SendKeys/SendInput; touch only the overlays rows of the Files table.
Do Phase O0 only (audit plus the PostKey(VK_TAB) probe; the only code change allowed is that probe assertion in
UIAutomationTests.cs), then stop and report in the README format, including the Tab probe result.
```

```
You are the worker agent "pickers" on the WPF pixel-parity effort (repo C:\Workspace\Projects\others\ui-design).
Read plan/wpf/README.md fully, then plan/wpf/07-overlays-pickers.md (Decisions 3-5, Files, Pickers and File input specs,
P and FI tables, pickers phases) and the web lines it cites (CSS 457-493, 581-603, 662; markup 1147-1179, 1838-1901;
JS 2526-2667, 3136-3182). Also read wpf/DropdownPrimitives.cs and wpf/SearchableSelect.cs (reuse, never edit).
Rules: build with wpf\build.bat pickers; test and capture only with wpf/tests/run-hidden.ps1 -Agent pickers; never commit;
never kill processes by name; InputControls is shared with "dropdown": re-read it before every edit and touch only
the File input and Date & time sections.
Do Phase P0 only (audit, no code changes; re-run plan/wpf/ref/web-overlays-capture.ps1 the same day), then stop and report.
```
