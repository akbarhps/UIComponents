# 03 — Dropdown family: searchable select, multi-select, tag input, autocomplete (agent "dropdown")

A fix-up audit plus the missing parts. **Tag input** and **Autocomplete** have early, unplanned ports in `InputControls.xaml(.cs)`. **Searchable select** and **Multi-select** don't exist in WPF yet. All four share one popup panel, one option row, one chip and one scroll bar, so Phase 1 builds those primitives once and later phases only compose them.

Source of truth: `web/index.html`
- CSS: `.tag-input` / `.tag` 553–566; `.ac*` 568–579; `.combo*` 752–790; `.multi*` 792–834. Shared: `.field` 296, `.input` / `.input-icon` 303–324, `.check*` 325–336, `.btn--ghost` / `.btn--primary` / `.btn--sm` 172–182, global `:focus-visible` and scroll bar ~49–55.
- markup: `#tag-input` 1103, `#autocomplete` 1122, `#combobox` 1226, `#multi-select` 1277
- JS: combo 2373–2438, multi 2440–2515, autocomplete 2659–2722, tag input 2724–2767
- icons: `i-check` 889 (`M5,12 L10,17 L19,7`, stroke 2.4), `i-close` 890 (`M6,6 L18,18 M18,6 L6,18`, stroke 2), `i-search` 891, `i-chevron` 897 (`M6,9 L12,15 L18,9`, stroke 2). All are in 24-unit boxes.

References in `plan/wpf/ref/` (web, 1400×1100, transitions off):
- `web-dropdown-<combobox|multi|tag-input|autocomplete>.png`: idle.
- `web-dropdown-combobox-open.png`: Assignee open, "Jonas Silva" selected, "Mia Chen" highlighted. Also `-filter`, `-empty`.
- `web-dropdown-multi-open.png`: three chips, panel open. Also `-chips` (6 chips wrapping to 2 rows, plus 2 chips) and `-empty`.
- `web-dropdown-tag-input-focus.png`: Tags focused (focus-within ring), Skills at its max, "4 of 4 tags".
- `web-dropdown-autocomplete-open.png`: chips React and Vue, typed "o", 2nd row highlighted. Also `-open2` (Framework, all 7, 1st highlighted) and `-empty`.
- **`*-scrollbar.png`** (combobox-open, multi-open, autocomplete-open2): the same states **with the real 11px scroll bar**. The other PNGs were taken with `--hide-scrollbars`, so their overflowing lists are 11px too wide. For open lists that scroll, compare against the `-scrollbar` files.
- `web-dropdown-measurements.txt`: geometry, fonts, colours, radii, padding and shadows for every state. Coordinates are relative to the `.demo` card. `web-dropdown-measure.js` is the script that produced it; append it to `index.html` and run `--dump-dom`, as in the README recipe.

Read `plan/wpf/README.md` first. Rules 4 (hidden desktop, agent name `dropdown`) and 5 (never commit) are mandatory.

---

## Why

These four are the "pick values" components. The web builds them from one visual language: a 46px field, a panel 6px below it (`Line` border, radius 12, `Shadow2`), 40.4px option rows with a `BgDeep` highlight, and `BrandSoft` chips. Today WPF has:
- **Tag input:**
  - wrong card (`ClaudeCard`), no description, only one field with no label
  - chips with a 42px "×" `GhostButton`
  - a fixed 140px `ClaudeInput` that never wraps with the chips, and no placeholder
  - hint "N tags" (web: "N of M tags"), no max, no blur-add, no click-to-focus
- **Autocomplete:**
  - one unlabelled 320px `ClaudeInput` with a default `ListBox` in a `Popup`
  - opens only on text change; no keys, no empty row, no 8-item cap, no tags mode
- **Searchable select, Multi-select:** missing.

## Decisions (made in this plan; don't re-litigate)

1. **No `Popup`.** A Popup is its own HWND, so:
   - `--capture` (RenderTargetBitmap of `MainWindow.Content`) never renders it
   - `FindById` can't reach it for `!click` or `@focus`
   - UIA from the window root doesn't see it
   - it doesn't scroll with the page, unlike the web's `position:absolute` panel

   Host the panel in an **`Adorner` on the nearest `AdornerLayer` of the trigger**. That layer belongs to `ContentScroll`'s `ScrollContentPresenter`, so it is **inside** `MainWindow.Content` and in the visual tree. That means it is captured, UIA- and `FindById`-reachable, clipped to the viewport, paints above later sections, and follows scrolling, just like the web. Don't use the window-level adorner layer (it's outside `Content`).
   - **Fallback** if Phase 1 proves it is not reachable: a zero-height `Canvas` under the control, panel at `Top = control height + 6`, with `Panel.ZIndex = 1` set on each ancestor up to `SectionList` while open.
2. **Code-only controls, like `SheetGrid`.** These are:
   - `SearchableSelect`, `MultiSelect`, `TagInput` and `Autocomplete`, each a `Grid` subclass that builds its tree in `OnInitialized` from CLR properties set in XAML (`Placeholder`, `SearchPlaceholder`, `Options`, …)
   - shared primitives in one file
   - all styles in the new `Themes/Dropdown.xaml`

   Label and static hint stay in XAML (`FieldLabel` / `FieldHint`), as in the web `.field`.
3. **Interactive parts are `ButtonBase`**, so UIA Invoke/Toggle and the `--capture !Id` chain reach them: triggers are `ToggleButton`, options and chip-remove are `Button`, multi options are `CheckBox` (`ClaudeCheckBox`). Open/close is driven by the trigger's `Checked`/`Unchecked` events, because `--capture !` sets `IsChecked` and raises no `Click`.
4. **No animations.** Skip the web's 0.15s chevron and border transitions: set the final state directly. Captures render immediately, and an animation would hang or blur them on the hidden desktop.
5. **Web quirks are replicated** (rule "the web wins"), each listed in the behaviour spec with a `quirk` tag. The open questions ask the reviewer to confirm the three that look like bugs.

## Files and ownership (agent `dropdown`)

| File | Change |
|---|---|
| `wpf/Themes/Dropdown.xaml` | **new**, the styles listed under "Primitives" |
| `wpf/DropdownPrimitives.cs` | **new**: `DropdownAdorner` (hosts a panel under an anchor, plus an outside-click hook) and `ChipWrapPanel` |
| `wpf/SearchableSelect.cs`, `wpf/MultiSelect.cs`, `wpf/TagInput.cs`, `wpf/Autocomplete.cs` | **new**, one control each |
| `wpf/App.xaml` (**shared**) | merge `Themes/Dropdown.xaml` **after** `Forms.xaml` (it uses `FieldInputSearch`, `SearchIcon`, `ClaudeCheckBox`, `GhostButtonSm`) |
| `wpf/UIComponents.Wpf.csproj` | none: wildcards pick up the new files (no entries, README hard constraint 2) |
| `wpf/InputControls.xaml(.cs)` | **only** the `Tag input` and `Autocomplete` sections, plus two new sections `Searchable select` and `Multi-select with filter` inserted after Autocomplete. Delete the old tag/auto code (`_tags`, `_options`, `TagInput_KeyDown`, `RemoveTag_Click`, `AutoInput_*`). **Don't touch** `File input`, `Date & time`, `Browse_Click` or `ClaudeInput` (`DtTime` uses it) |
| `wpf/tests/UIAutomationTests.cs` | your own assertions, plus one `PostKey` helper (Phase 0). `Main()`'s harness belongs to the reviewer |
| `wpf/README.md` | Phase 6: the covered list and the assertion count |

Reuse without editing:
- `FieldLabel`, `FieldHint`, `ClaudeCheckBox` (`Controls.xaml`)
- `FieldInputSearch`, `SearchIcon` (`Forms.xaml`)
- `SheetSectionDesc`, `DemoCard`, `GhostButtonSm` (`Sheet.xaml`)
- `Shadow1`, `Shadow2` and the colour keys (`Tokens.xaml`)

**Never edit `Forms.xaml`, `Sheet.xaml`, `Controls.xaml` or `MainWindow.xaml`.** If something there blocks you, ask.

**Shared files** (`App.xaml`, `MainWindow.xaml`, shared styles): other workers (display, tables) edit them right now. Re-read each one immediately before editing, add only your own lines, and never reorder or reformat other people's entries.

`PrimaryButtonSm` doesn't exist yet. First grep `wpf/Themes` for it, because the tables plan adds `*ButtonSm` styles to `Controls.xaml`; reuse it if it has appeared. Otherwise define it in `Dropdown.xaml`, modelled on `GhostButtonSm`: `Brand` background, white text, `Shadow1`, hover `BrandHover`.

## Section shell (all four; copy the Forms pattern in `MainWindow.xaml` ~63–75)

- **Title:** `SectionTitle`, `Margin 0,46,0,0`. Texts are the web h2s, verbatim: "Searchable select", "Multi-select with filter", "Tag input", "Autocomplete".
- **Description:** `SheetSectionDesc`, `Margin 0,8,0,0`. It wraps to 2 lines. Use the verbatim texts:
  - combobox: "A dropdown that filters its options as you type. Open it, search, then choose with a click or the arrow keys and Enter."
  - multi: "Keep several values at once. Selections appear as removable chips, and the search narrows the options."
  - tag: "Type a value and press Enter or comma to add it as a chip. Backspace on an empty field removes the last one, and each chip has its own remove button."
  - autocomplete: "Suggestions filter as you type. Use the arrow keys and Enter to pick one, or type a value that isn't in the list."
- **Card:** a chrome-free `GroupBox` with AutomationId `ComboCard` / `MultiCard` / `TagCard` / `AcCard`, `Margin 0,22,0,0`, holding a `Border Style=DemoCard`.
- **Grid-2:** columns `*`, 22, `*`, so each column is 450.
- **Field stack:** label (20.8), 7, control, 7, hint (19.2).
- **Card heights:**

| Card | Height | Why |
|---|---|---|
| combobox | **162** | control 46 + hint |
| multi | **135.8** | no hint |
| tag | **162** | |
| autocomplete | **161** | the right column is 45 + hint; the left column's hint is empty, see A6 |

---

## Visual spec (DIPs; numbers relative to the control's top-left unless stated)

### Primitives (`Dropdown.xaml` + `DropdownPrimitives.cs`)

| Key | Spec |
|---|---|
| `DropdownTrigger` (ToggleButton) | **Height 46**, `Surface`, 1px `LineStrong`, radius 8. Hover border `Ink3`. **Checked (open):** border `Brand` + a 3px `BrandSoft` ring outside (`Margin -3`, radius 11, no layout). **Keyboard focus while closed:** the web's global `:focus-visible`, a 2px `Brand` outline 2px outside (`Margin -4`, radius 12), on `IsKeyboardFocused` only. `FocusVisualStyle {x:Null}`. Chevron 16×16 `Ink3`, stroke 2 (24 box), `RotateTransform 180` when checked |
| `DropdownPanel` | `Surface`, 1px **`Line`** (not `LineStrong`), radius **12**. **`Shadow2` on a sibling background `Border`**, never on the content (an Effect rasterises the text). The adorner arranges it at `(0, anchorHeight + 6)` with the anchor's width. No clip needed: nothing paints into the corners |
| `DropdownSearch` (TextBox) | Top area `Padding 10`, 1px `Line` bottom border, so **57.2** tall in total. The input is 428×**36.2**: 13px `Ink`, 1px `LineStrong`, radius **7**, text and placeholder start **33** from the input's outer left (`padding-left 32` + border; compensate the content-host inset as `FieldInputSearchTemplate` does and measure the typed text). Focus: border `Brand` + 3px `BrandSoft` ring, radius **10**. Icon: the `SearchIcon` style at **15×15**, at x **22** from the panel's outer left, vertically centred on the input. The placeholder shows even when focused |
| list host | `ScrollViewer`, **MaxHeight 220** (including padding), content `Margin 6`, vertical auto, horizontal disabled |
| `DropdownScrollBar` (**keyed**; apply it through the list `ScrollViewer.Resources[typeof(ScrollBar)]`, never as an implicit app-wide style) | Vertical, **11 wide**, transparent track, no arrow buttons. The thumb is a `Border` with `Background LineStrong`, `BorderBrush #F5F4EF` (`Bg`), `BorderThickness 3` and `CornerRadius 5.5`, which gives a 5px core with a 3px rim (pixel-probed). Options shrink by 11 when it shows |
| `DropdownOption` (Button) | **Height 40.4**, radius 7, horizontal padding 10, 14px `Ink`. Background: transparent; **`BgDeep`** on hover or when active (`BrandSoft` is wrong here). Active is set from code (`Background = BgDeep`) |
| `AcOption` | `BasedOn DropdownOption` with **Height 37** (the web `font` shorthand resets line-height to normal). Text only, at x 10 |
| `DropdownEmpty` (TextBlock) | 13px, LineHeight 20.8, `Ink2`, centred. Padding **18/10** in combo/multi (row 56.8, list 68.8); **16/10** in autocomplete (52.8 / 66.8) |
| `Chip` (Border + TextBlock + remove Button) | `BrandSoft`, radius 6, padding 7 horizontally. Text 12px SemiBold `Brand`, 5px gap, then the remove button: 12×12, the `i-close` path in `Brand`, stroke 2 in a 24 box. **Two heights:** **27.2** in tag input and autocomplete (text LineHeight 19.2) and **24.8** in multi (LineHeight 16.8). The remove button sets `AutomationProperties.Name = "Remove <text>"` |
| `ChipWrapPanel` (Panel) | Flex-wrap with a **6px** column and row gap. Items in a line are vertically centred. **Optional "fill last child":** the last child (the text field) takes the rest of the line, min **120**, or wraps to a new line at full width. That's 25 lines of Measure/Arrange; no `WrapPanel` (it can't stretch the field) |
| `TagBox` (Border) | `MinHeight 46`, `Padding 10,8`, `Surface`, 1px `LineStrong`, radius 8. Hover `Ink3`. **`IsKeyboardFocusWithin`:** border `Brand` + 3px `BrandSoft` ring (radius 11). `Cursor IBeam`. One line of chips is centred in the 28px content box; it grows with wrapping (4 chips + field = **78.2**) |
| `BareInput` (TextBox) | Transparent, no border, 14px `Ink`, **Height 27**. Text and placeholder start **2** from the field's left (`padding 4/2`; compensate the host inset). Placeholder `Ink3` via `Tag`, visible while focused. `FocusVisualStyle {x:Null}`, no ring |
| `PrimaryButtonSm` | See Files |

### Searchable select (`#combobox`) — `SearchableSelect`, ids `ComboAssignee`, `ComboProject`

- **Instances:**
  - Assignee: placeholder "Select a person…", search "Search people…". Options: Ada Lovelace|Design, Jonas Silva|Engineering, Mia Chen|Design, Sam Patel|Operations, Alex Rivera|Marketing, Priya Shah|Content. Hint "Type a name to narrow the list."
  - Project: "Select a project…", "Search projects…". Options: Website refresh|Design, Mobile app|Engineering, Brand guidelines|Design, Help center|Content, Q3 campaign|Marketing, Legacy archive|Operations. Hint "Arrow keys and Enter work too."
- **Trigger:** 450×46.
  - Value: 15px, LineHeight 21, at x **14**, y **12.5**, single line with ellipsis. `Ink3` while it's the placeholder, `Ink` once chosen.
  - Chevron at x **420** (14 from the right), y 15.
- **Panel:** y **52**, 450 wide. Open with 6 options it is **279.2** tall (1 + 57.2 + 220 + 1). The list overflows (254 > 220), so the scroll bar shows.
- **Option row** (rel. the option):
  - check 14×14 at (10, 13.2), `Brand`, stroke 2.4, visible only when selected
  - label 14px, LineHeight 22.4, at x **32**, y 9, takes the remaining width
  - meta 12px `Ink3` 400, right-aligned 10 from the right, vertically centred
  - **Selected:** label `Brand` SemiBold, check visible. The meta stays `Ink3` 400.
  - Option width: 436, or **425** with the scroll bar.
- **Empty:** "No matches". The panel is **128** tall.
- Hint y **111.8**.

### Multi-select (`#multi-select`) — `MultiSelect`, ids `MultiSkills`, `MultiTags`

- **Instances:**
  - Skills: placeholder "Select skills…", search "Search skills…". Options: React|Frontend, TypeScript|Frontend, Design systems|Design, Accessibility|Design, Testing|Engineering, Documentation|Content.
  - Tags: "Select tags…", "Search tags…". Options: Urgent|Priority, Follow-up|Workflow, Blocked|Status, Research|Phase.
  - No hints.
- **Trigger:** `DropdownTrigger` look, but `Padding 11,9`, font 14px, LineHeight 19.6.
  - **No chips:** placeholder `Ink3` at x **12**, y **13.2** (centred). Chevron at x **422** (12 from the right), y **16.5** (centred plus the web's 3px top margin).
  - **With chips:** a `ChipWrapPanel` (no fill) of 24.8 chips at x 12, y **10**, width **402** (an 8px gap to the chevron). The chevron is **top-aligned** at y **13**. The trigger grows: 2 rows give **75.6**.
  - Chips are listed in **option order**, not click order.
- **Panel:** same as combobox, plus a foot. With 6 options it is **332.2** tall (1 + 57.2 + 220 + 53 + 1). Empty: **181**.
- **Option row** (`ClaudeCheckBox` inside a row `Border`: radius 7, `Padding 10,9`, hover `BgDeep`; clicking the padding also toggles):
  - box at (10, 10)
  - label at x **39**, y 9
  - meta 12px `Ink3`, right-aligned 10 from the right, **top**-aligned at y 9 (`.check` is `align-items:flex-start`, unlike combo)
  - no selected or active styling
- **Foot:** 1px `Line` top border, `Padding 10`, **53** tall.
  - "Select all" (78.3×32) and "Clear all" (73.1×32) are `GhostButtonSm`, 4 apart.
  - On the right: count "N selected" (12px `Ink3`), 10 gap, "Done" `PrimaryButtonSm` (57.6×32).

### Tag input (`#tag-input`) — `TagInput`, ids `TagTags`, `TagSkills`

- **Instances:**
  - Tags: label "Tags", initial `design,research`, max **6**, placeholder "Add a tag…".
  - Skills: label "Skills (no duplicates)", initial `react`, max **4**, placeholder "Add a skill…".
- **Layout:** `TagBox` > `ChipWrapPanel` (fill) of 27.2 chips plus a `BareInput`. The control also owns the hint (`FieldHint`): "**2 of 6 tags**" / "1 of 4 tags".
- **Measured** (rel. the TagBox): first chip at (11, 9.4). The chips "design" 67.2, "research" 76.5 and "react" 58 are 6 apart. The field starts at x **166.7** (Tags) and is 27 tall at y 9.5. The hint is at y 111.8 in the card.

### Autocomplete (`#autocomplete`) — `Autocomplete`, ids `AcTech`, `AcFramework`

- **AcTech:** label "Technologies as tags", `Multiple=true`, options React, TypeScript, Vue, Svelte, Node.js, Python, GraphQL, PostgreSQL, Docker, AWS. Chrome is `TagBox` + `ChipWrapPanel` + `BareInput` "Search technologies…" (46 tall). **No visible hint** (A6).
- **AcFramework:** label "Framework", options React, Vue, Angular, Svelte, Solid, Next.js, Nuxt. Chrome is exactly the Forms search: `FieldInputSearch` + `SearchIcon` overlay (45 tall), placeholder "Search or type your own…". Hint "Pick a suggestion or keep your own text." at y **110.8**.
- **List:** a single `DropdownPanel` with the `ScrollViewer` **inside the 1px border** (MaxHeight 218 client) and the `Padding 6` inside the scroller.
  - y = control height + 6 (52 or 51), 450 wide
  - `AcOption` rows, 436 wide (425 with the scroll bar)
  - 4 rows = 162 tall; 7 rows = **220** (scrolls); empty = 66.8

---

## Behaviour spec (every web branch). Verify: **U** = UIA assertion, **K** = UIA plus `PostKey` (only if the Phase 0 probe passes, else **C**), **P** = capture/pixel, **C** = code-only, the user checks by hand

`PostKey` = `PostMessage(hwnd, WM_KEYDOWN/WM_KEYUP, vk)` to the app's main window HWND (`root.Current.NativeWindowHandle`) after UIA `SetFocus`. It is a message to a window on the hidden desktop. **Never use `SendKeys`, `SendInput` or `keybd_event`**: they type into the user's foreground window.

Text is entered with `ValuePattern.SetValue`, which raises `TextChanged`, the WPF equivalent of `input`.

### Searchable select (JS 2373–2438)

| # | Branch | Verify |
|---|---|---|
| S1 | Trigger click toggles. **Open:** clear search, show all options, hide empty, clear the highlight and active index (-1), focus search. The selected option keeps its check. The list does **not** scroll to it | U (Toggle → `_Search` exists and `HasKeyboardFocus`; 6 options on screen) |
| S2 | **Close:** hide the panel. Focus returns to the trigger, except on an outside click | U |
| S3 | Trigger key **Down, Enter or Space while closed** → open + highlight the first option. Mark it Handled so the ToggleButton doesn't toggle twice | K, P |
| S4 | Filter on the search `input`: `query.Trim().ToLower()` is a **substring of (label + meta) lowercased, with no separator** (`textContent`). For example "eng" → only Jonas (Engineering); "design" → Ada and Mia. Empty row when nothing matches. **Active index → -1 but the old highlight stays painted** (quirk) | U ("eng" → 1, "design" → 2, "xyz" → `_Empty` + 0) |
| S5 | Search **Down/Up**: `active = (i + n) % n` over the visible options, so it **wraps**. **Up with nothing active lands on the LAST visible option** (web fixed; the old n-2 quirk is gone). Scroll the active option into view (`BringIntoView`, nearest) | K, P |
| S6 | Search **Enter** with an active option → choose it. With no active option, nothing happens | K |
| S7 | Search **Escape** → close, and focus the trigger | K |
| S8 | **Choose:** a single selection. Set the value text, the label colour and the check, then close and focus the trigger | U (Invoke `_Option1` → `_Value` Name "Jonas Silva", `ItemStatus` "selected", `_Search` gone, trigger focused) |
| S9 | **Outside click** (window `PreviewMouseDown` outside the control and panel) → close **without** restoring focus | C |
| S10 | **Tab** out of the search box: the web does nothing, and the panel **stays open** (quirk) | C |
| S11 | Hover → `BgDeep`, independent of the active row | C/P |

Expose the row state as `AutomationProperties.ItemStatus`: `""`, `"active"`, `"selected"` or `"active selected"`.

### Multi-select (JS 2440–2515)

| # | Branch | Verify |
|---|---|---|
| M1 | Trigger click toggles. **Open:** clear search, show all, hide empty, focus search. **Closing from the trigger** doesn't move focus | U |
| M2 | Filter: the same rule as S4 (label + meta). Empty row "No matches" | U ("design" → Design systems and Accessibility = 2; "xyz" → `_Empty`) |
| M3 | Checkbox toggle → re-render the chips (option order) and the count "N selected". With 0 selected, show the placeholder and centre the chevron | U |
| M4 | Chip remove → uncheck that option and re-render. It must **not** toggle the panel (the inner `Button` handles the click) | U (Invoke `_Remove0`) |
| M5 | **Select all** checks only the **visible** (filtered) options. **Clear all** unchecks **all** of them, hidden ones included | U ("design" + Select all, with React and TS already on → "4 selected"; Clear → "0 selected" + placeholder) |
| M6 | Done → close and focus the trigger | U |
| M7 | **Escape anywhere** while open → close and focus the trigger. Handle it at window level, like the web's document listener | K |
| M8 | Outside click → close **and focus the trigger** (quirk: the web steals focus back) | C |
| M9 | No arrow-key navigation. Tab walks search → checkboxes (`ClaudeCheckBox` ring) → buttons. Space toggles | C |

### Tag input (JS 2724–2767)

| # | Branch | Verify |
|---|---|---|
| T1 | Initial tags from the instance data. Hint "N of M tags" | U (`_Hint` "2 of 6 tags"; `_Chip0` "design") |
| T2 | **Enter, or typing ","** → split the value on commas and add each part: trim, skip empty, skip once `count ≥ max`, skip case-insensitive duplicates. Then **always** clear the field. Detect the comma via `PreviewTextInput == ","` (layout-aware, like `e.key`), not `Key.OemComma` | K (SetValue "a, b ,A" + Enter → "4 of 6 tags") |
| T3 | **Backspace** on an empty field with tags → remove the last tag | K |
| T4 | **Blur** with non-blank text → `add(value)` **without** a comma split (quirk). Clear the field | U (SetValue "zeta", SetFocus on another field → chip "zeta") |
| T5 | Click on the container → focus the field. A chip-remove click must not refocus | C |
| T6 | Chip remove → remove that tag | U (Invoke `_Remove0`) |
| T7 | At max, the field stays enabled and further adds are silently ignored. Skills: "vue,node,go,x" → "4 of 4 tags" | K (or U via T4 blur, one value at a time) |

### Autocomplete (JS 2659–2722)

| # | Branch | Verify |
|---|---|---|
| A1 | **Focus** or **text change** → show: matches = options containing the trimmed, lowercased query; in Multiple mode, minus already-chosen tags (case-insensitive); **first 8 only**. Reset the active index to -1. Show the empty row "No suggestions" when there are no matches. The list opens even with an empty query | U (Framework focus → 7; "s" → Svelte, Solid, Next.js; "zzz" → `_Empty`; Tech focus → 8) |
| A2 | **Down/Up:** if the list is hidden or has no matches, show it and stop. Otherwise **clamp, no wrap** (Down `min(a+1, n-1)`, Up `max(a-1, 0)`). **No scroll-into-view** (quirk) | K, P |
| A3 | **Enter:** with the list open and an active row → choose it (Handled). Otherwise, in Multiple mode with non-blank text → choose the trimmed text as a **new tag, without a duplicate check** (quirk). Otherwise nothing | K |
| A4 | **Escape** → hide (focus stays) | K |
| A5 | **Choose:** in Multiple mode, add a chip and clear the text; in single mode, set the text. Then hide and keep focus in the input | U (Invoke `_Option0` → AcFramework value "React", list gone) |
| A6 | `updateHint` never finds `.ac__hint` (it is outside `.ac`), so **no "N selected" text is ever shown**. The span is empty and 0 tall (quirk). Add no hint to AcTech | P |
| A7 | Chip remove (Multiple) → remove the tag. No refocus | U |
| A8 | Outside click → hide. Tab away → the list stays open (quirk). No Backspace, comma or blur handling, and no container click-to-focus in AcTech | C |

---

## AutomationIds (`<Id>` = the instance id; the control reads its own `AutomationProperties.AutomationId` and prefixes its children)

| Area | Ids |
|---|---|
| Combo | `<Id>_Trigger`, `_Value`, `_Search` (exists only while open), `_Option<i>` (Name = label), `_Empty` |
| Multi | `<Id>_Trigger`, `_Placeholder`, `_Chip<i>` (the text), `_Remove<i>`, `_Search`, `_Option<i>`, `_Empty`, `_SelectAll`, `_Clear`, `_Count`, `_Done` |
| Tag | `<Id>_Input`, `_Hint`, `_Chip<i>`, `_Remove<i>` |
| Autocomplete | `<Id>_Input`, `_Chip<i>`, `_Remove<i>`, `_Option<i>`, `_Empty` |
| Cards | `ComboCard`, `MultiCard`, `TagCard`, `AcCard` |

**Counting rule for "N options visible":** count the `<Id>_Option<i>` elements that are found **and** have `IsOffscreen == false`. Filtered-out options are `Collapsed`, and WPF may either drop them from the UIA tree or keep them with `IsOffscreen = true`. Phase 1 reports which one happens.

**aria → UIA:**

| Web | WPF |
|---|---|
| trigger `aria-labelledby` (the label) | `AutomationProperties.Name` = the label text ("Assignee", "Skills", …) on the trigger |
| `aria-expanded` | the trigger `ToggleButton`'s Toggle state |
| search `aria-label` | Name "Search people" / "Search projects" / "Search skills" / "Search tags" |
| field `aria-label` (Tags / Skills / Technologies / Framework) | Name on `_Input` |
| `aria-selected`, `aria-activedescendant` | `ItemStatus` (see S table) |
| remove `aria-label` "Remove X" | Name on `_Remove<i>` |

The old ids `TagInput`, `TagCount`, `AutoInput`, `TagRemove` and `AutoList` go away. Update the three existing assertions ("tag input found", "tag count found", "autocomplete input found") to `TagTags_Input`, `TagTags_Hint` and `AcFramework_Input`. They are in your area.

## Capture recipe (hidden)

```powershell
wpf\build.bat dropdown
cd wpf\tests; .\build-tests.bat; cd ..\..
pwsh wpf/tests/run-hidden.ps1 -Agent dropdown          # tests
# Closed state: the card. Open state: ContentScroll (x:Name). The panel overflows below the card, and the crop is the element's bounds
pwsh wpf/tests/run-hidden.ps1 -Agent dropdown -NoTests -Section "Searchable select" -Capture "ComboCard=plan/wpf/ref/wpf-dropdown-combobox.png,ContentScroll!ComboAssignee_Trigger!ComboAssignee_Option1!ComboAssignee_Trigger=plan/wpf/ref/wpf-dropdown-combobox-open.png"
```

- `!` works on `ToggleButton` (it flips `IsChecked`) and `Button` (it raises `Click`), and the steps run in order. The grammar is `Id!a!b@Focus`: every `!` step runs first, then at most one `@` focus at the very end. So **closed, open, selected and reopened** states are all click-reachable.
- **Highlighted-row and typed-filter states need keys or text.** Until the reviewer adds a key/text step to `--capture` (open question 2), capture them only if it exists, and otherwise say "not capturable" in the report.
- Compare positions relative to the card's top-left. The web card is at page x 314.5; find the WPF card by scanning for its `Line` border.

---

## Phases: one at a time, capture, stop for review

**Order:** 0 → 1 → {2, 3, 4 in any order} → 5 (it needs 4's `TagBox` chrome) → 6. Each phase is self-contained; stop and report after it.

### Phase 0: audit and probes (report only, no app changes)

1. Capture `wpf-before-dropdown-tag-input.png` and `wpf-before-dropdown-autocomplete.png` (`-Section "Tag input"` / `"Autocomplete"`, `Sections=`).
2. Confirm or correct every gap in "Why", with measured numbers (chip height, container padding, input x).
3. **Key probe:**
   - Add a `PostKey(IntPtr hwnd, int vk)` helper (`PostMessage` with `WM_KEYDOWN 0x100` / `WM_KEYUP 0x101`) to the tests.
   - Add one assertion against the **current** tag input: SetFocus `TagInput`, SetValue "probe", `PostKey(VK_RETURN)`, sleep 200 ms, then expect `TagCount` "3 tags".
   - Report whether it passes. This decides **K** vs **C** for every keyboard branch.
4. Report: before-captures, the gap list, the probe result, and any conflict with this plan.

### Phase 1: primitives plus the Searchable select shell

- Create `Dropdown.xaml` (all the primitive styles), `DropdownPrimitives.cs` (`DropdownAdorner`, `ChipWrapPanel`), and the App entry (no csproj entries: wildcards).
- Add the "Searchable select" section with both `SearchableSelect`s. Phase 1 only needs: open/close by trigger (S1/S2), click-to-choose (S8) and the outside-click hook (S9). Filter and keys come in Phase 2.
- **Prove the hosting decision:**
  - `ContentScroll!ComboAssignee_Trigger` renders the panel.
  - `FindById` finds `ComboAssignee_Option1` (the chained `!` works).
  - The UIA test finds `ComboAssignee_Search` from the window root.
  - Also report whether a `Collapsed` option is absent from UIA or present with `IsOffscreen`.

  If any of these fail, switch to the Canvas fallback and report.

Captures: `wpf-dropdown-combobox.png` (card), `wpf-dropdown-combobox-open.png` (the chain in the recipe).

Acceptance (vs `web-dropdown-combobox.png`, `-open-scrollbar.png` and the measurements):
1. Card 162 ±1; triggers 450×46; value text at x 14 / chevron at x 420 ±1.
2. Panel at y +52, 450×279.2 ±1, 1px `#E6E2D7` border, radius 12, shadow visible below.
3. Search input 428×36.2 at (11,11) in the panel; typed text x 33 ±1; icon at x 22.
4. Option rows 40.4 ±0.5 (rows 1–5 start at +6, 46.4, 86.8 … in the list); the scroll bar is 11 wide with a 5px `#D5CFC0` core; options are 425 wide.
5. Selected row: `#A64E2A` SemiBold label and a check at x 10.
6. The open trigger has a `#A64E2A` border and a `#F8ECE4` 3px ring.
7. Tests pass; the panel's elements are absent when closed.

### Phase 2: Searchable select behaviour

- Implement S3–S7, S10 and S11 exactly as in the table, plus the empty row and `ItemStatus`.
- Add the U and K assertions for S1–S8 (K only if the probe passed).
- **Captures:**
  - `wpf-dropdown-combobox-empty.png` (Project, "xyz"): needs the text step, or report that it can't be captured
  - the open state again
- **Acceptance:**
  1. All S assertions pass.
  2. The empty panel is 128 ±1; "No matches" is 13px `#5B554A`, centred.
  3. Filtering "eng" leaves 1 option and "design" 2.
  4. Up from no-active lands on the LAST visible option ("Priya Shah"): the web was fixed (`setActive(activeIndex < 0 ? -1 : activeIndex - 1)` wraps).

### Phase 3: Multi-select

- Add the "Multi-select with filter" section. Implement M1–M9 and the trigger chip layout (empty vs with-chips chevron positions).
- Add the U and K assertions.
- **Captures:**
  - `wpf-dropdown-multi.png`
  - `wpf-dropdown-multi-open.png`: `ContentScroll!MultiSkills_Trigger!MultiSkills_Option0!MultiSkills_Option1!MultiSkills_Option3`, panel open with 3 chips
  - `wpf-dropdown-multi-chips.png`: `MultiCard!MultiSkills_Trigger!MultiSkills_SelectAll!MultiSkills_Done!MultiTags_Trigger!MultiTags_Option0!MultiTags_Option2!MultiTags_Done`
- **Acceptance** (vs `web-dropdown-multi*.png`, `-open-scrollbar.png`):
  1. Card 135.8 ±1.
  2. Placeholder at x 12 / y 13.2; empty chevron at y 16.5, with-chips chevron at y 13, both at x 422.
  3. Chips are 24.8 tall, radius 6, 6 apart. Skills with 6 chips wraps after "Accessibility": the trigger is 75.6 and the second row is at y 40.8.
  4. Panel 332.2 ±1; foot 53; Done 57.6×32 `#A64E2A`; count "3 selected".
  5. Option checkbox at (10,10); label x 39; meta top-aligned.

### Phase 4: Tag input (rewrite of the existing section)

- Replace the section with two `TagInput`s in a grid-2, plus the shell. Implement T1–T7.
- Delete the old code and ids. Update the old assertions and add the T assertions.
- **Captures:**
  - `wpf-dropdown-tag-input.png` (`TagCard`)
  - `wpf-dropdown-tag-input-focus.png` (`TagCard@TagTags_Input`)
- **Acceptance** (vs `web-dropdown-tag-input*.png`):
  1. Card 162 ±1; containers 46.
  2. Chips 27.2 tall at (11, 9.4); widths ±1 of 67.2 / 76.5 / 58; field at x 166.7 ±1 with the placeholder at +2.
  3. The focus-within ring is a 3px `#F8ECE4` and the border `#A64E2A`.
  4. Hints "2 of 6 tags" / "1 of 4 tags" at y 111.8.
  5. Wrapping, checked by a U test: blur-add "user interviews" and then "accessibility audit" to TagTags. `TagTags_Input`'s `BoundingRectangle.Top` must move down by about 33 (27.2 + 6), and the container becomes 78.2.

### Phase 5: Autocomplete (rewrite of the existing section)

- Two `Autocomplete`s: AcTech reuses `TagBox`/chips, AcFramework reuses the Forms search. Implement A1–A8.
- Delete the old `Popup` and `ListBox`.
- **Captures:**
  - `wpf-dropdown-autocomplete.png`
  - `wpf-dropdown-autocomplete-open2.png`: `ContentScroll@AcFramework_Input`, since focus opens it
  - `wpf-dropdown-autocomplete-open.png`: `ContentScroll@AcTech_Input`, the 8-row list that focus opens, with no chip. Under today's grammar the `!` clicks always run **before** the one `@` focus, so the web state (chips + "o" + highlight) needs open question 2; report it as not capturable until then
- **Acceptance** (vs `web-dropdown-autocomplete*.png`, `-open2-scrollbar.png`):
  1. Card 161 ±1.
  2. AcTech 46 tall, AcFramework 45. The Framework hint is at y 110.8, and AcTech shows no hint.
  3. List at +6, rows 37 ±0.5; 7 rows give a 220-tall list with the scroll bar.
  4. Tech focus shows 8 rows, never an already-chosen tag.
  5. Empty list 66.8 with "No suggestions".

### Phase 6: tests, cleanup, README

- Full run: all assertions pass. No dead code is left in `InputControls.xaml.cs` (`System.Linq` / `ObservableCollection` usings only if still used).
- `wpf/README.md`: add "searchable select, multi-select, tag input, autocomplete (keyboard: K or C per probe)" to the covered list and update the assertion count.

---

## WPF quirks that apply

- **Popup / adorners:**
  - Popups are separate HWNDs: not captured, not in `FindById`, not under the window's UIA root (hence Decision 1).
  - The adorner must override `VisualChildrenCount`, `GetVisualChild`, `MeasureOverride` and `ArrangeOverride` for its hosted panel, or the panel neither renders nor shows up in UIA.
  - Remove the adorner on close, so `_Search` and the options really disappear.
- **Focus on open:** focusing the search box synchronously after adding the adorner fails, because it isn't loaded yet. Use `Dispatcher.BeginInvoke(DispatcherPriority.Input, …)`.
- **`ToggleButton`:**
  - Space and Enter toggle it natively. In S3, handle `PreviewKeyDown` and set `Handled`, so the open isn't undone.
  - `--capture !` sets `IsChecked` without a `Click`, so drive open/close from `Checked`/`Unchecked`.
- **WPF `ComboBox`:** its focus and keyboard model (type-ahead, selection on arrow while closed, its own Popup) doesn't match the web. **Don't base these controls on `ComboBox`.** `FieldSelect`/`FieldSelectItem` stay the native-select look, used only in Forms.
- **TextBox:**
  - The content host adds about 2–3px of inset, and `Padding` is applied twice in custom templates. Set `Padding 0` and position with template margins (like `FieldInputTemplate`), then **measure the typed text**.
- **Rounded clips:**
  - `ClipToBounds` on a rounded `Border` clips to a rectangle. It isn't needed here.
  - If you ever clip the panel, use a `RectangleGeometry` (radius 11) updated on `SizeChanged`.
- **`DropShadowEffect`:** it also blurs and greyscales the content it's applied to. Put it on a separate background `Border`.
- **Line heights:** set row heights explicitly (40.4 / 37 / 27.2 / 24.8 / 46). Don't rely on font metrics: WPF and Chromium differ by about 0.5px.
- **Hidden desktop:**
  - No mouse, so S9, S11, M8, T5 and A8 are code-only.
  - Keys only through `PostMessage` (Phase 0).
  - No animations.
- **Shared clipboard:** not used here. Don't add copy/paste.

## Reviewer checklist

- [ ] Only the listed files changed. `File input`, `Date & time`, `ClaudeInput`, `Forms.xaml`, `Sheet.xaml`, `Controls.xaml` and `MainWindow.xaml` are untouched.
- [ ] C# 5 throughout (no `$""`, `?.`, `=>` members or auto-property initialisers); the App entry is present (no csproj entries: wildcards); Dropdown.xaml is merged after Forms.xaml.
- [ ] No `Popup`, no `ComboBox` base, no animations, and no implicit app-wide styles (the scroll bar is keyed and scoped).
- [ ] Every branch in the S, M, T and A tables is implemented as written, quirks included. U/K assertions exist where the table says so.
- [ ] I captured the states myself, hidden, and checked them against the `web-dropdown-*` refs (`-scrollbar` for scrolling lists): ±1px, exact hex.
- [ ] Old tag/auto code and ids are gone; tests pass; no commits.

## Decisions (reviewer, final)

1. **Hosting: accepted.** Adorner on `ContentScroll`'s layer, zero-height Canvas as the fallback if Phase 1 cannot prove capture, `FindById` and UIA reach it.
2. **Keys and text in captures: yes.** The `dropdown` worker may extend `App.xaml.cs` `--capture` with `~Key` (raise `PreviewKeyDown`+`KeyDown` on `Keyboard.FocusedElement`) and `^text` (set `TextBox.Text` on the focused element) in Phase 1, as a small, self-contained addition (reviewer-owned file: touch nothing else in it, re-read it first, keep the existing `!Id` and `@Id` behaviour byte-for-byte). The tables worker needs `^text` for its empty-state capture and will reuse it afterwards. Document the grammar in the plan README line for `--capture`.
3. **Web quirks: SUPERSEDED.** The user decided they are web bugs and a `web` worker is fixing `web/index.html` (see REVIEWER.md). Port the FIXED behaviour, not the quirk: M8 / C1 outside click closes the multi WITHOUT stealing focus (Escape and selection still restore focus to the trigger); A3 / C2 autocomplete Multiple mode ignores a case-insensitive duplicate on Enter/choose; A6 / C3 the "N selected" hint is shown under Technologies (12px/19.2px line; the web captures and measurements of the autocomplete section are being regenerated: use the new `web-dropdown-autocomplete*.png` and `web-dropdown-measurements.txt` once the reviewer tells you they landed); S4 / C4 filtering clears the highlighted row and aria-activedescendant; S5 / C5 ArrowUp with nothing active selects the LAST option (the fixed JS calls setActive(-1), which wraps; copy the fixed JS exactly once it lands); S10 and A8 / C6 Tab out of the combo search or the autocomplete input closes the panel/list; A2 / C7 autocomplete arrow navigation scrolls the active row into view; T4 / A2 (tag input): clicking a chip's remove button works even with pending text (no blur-add first). Until the web worker reports, treat the fixed behaviour as the spec; if the web file differs when you read it, the web file wins.
4. README updated (task table and ownership table) by the reviewer.

---

## Worker kickoff (first message to the worker)

```
You are the worker agent "dropdown" on the WPF pixel-parity effort (repo C:\Workspace\Projects\others\ui-design).
Read plan/wpf/README.md fully (rules, workflow, ownership, gotchas), then plan/wpf/03-dropdown.md and the web lines it cites
(index.html CSS 553-579, 752-834; markup 1103-1144, 1226-1341; JS 2373-2515, 2659-2767).
Rules: build with wpf\build.bat dropdown; test and capture only with wpf/tests/run-hidden.ps1 -Agent dropdown; never commit;
never kill processes by name; never use SendKeys/SendInput; touch only the files in the plan's "Files" table.
Do Phase 0 only (audit and key probe; the only code change allowed is the PostKey helper and one probe assertion in
UIAutomationTests.cs), then stop and report in the README format: files changed, build/test summary, capture paths,
gap list with measured numbers, PostMessage probe result, deviations, blocking questions.
```
