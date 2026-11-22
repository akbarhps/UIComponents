# 01 — Grid input & Typed grid input

Source of truth: `web/index.html`
- CSS: lines ~605–638 (`.sheet*`)
- markup: ~1181–1216
- JS: `createGrid()` ~2769–3058

Docs: `web/COMPONENTS.md` sections "Grid input" and "Typed grid", plus Appendix snippet 1.

References in `plan/wpf/ref/`:
- `web-grid-input.png` is the idle state.
- `web-grid-sel.png` shows row 2 selected in the grid input and the Category column selected in the typed grid.
- `web-grid-focus.png` shows the Height cell of row 1 focused.
- `web-measurements.txt` holds the measured geometry.

Read the README in this folder first. Its constraints (C# 5, old csproj, CLI build) apply to every phase.

---

## Why rewrite instead of restyle

The current `DataControls.xaml` grids are broken, not just off-style:
- The `DataGridTextColumn`s have no `Binding`, so every cell renders empty.
- The rows are anonymous types, which are read-only, so nothing is editable.
- The `DataGridComboBoxColumn` has no `ItemsSource`.
- The grid input is missing the Weight column, and the typed grid is missing Width/Height.
- "+ Add row" and "+ Add column" are toolbar buttons. On the web they are in-table controls.

The web component is **not** a data grid:
- every cell is an always-editable input
- there is a row-number header column
- there is a "+" add-column header and a full-width "+ Add row" row
- cells can be selected non-contiguously with Ctrl
- typing propagates across the selection

Bending `DataGrid` into that shape (edit mode vs. view mode, cell templates, corner clipping) is more code than porting `createGrid` directly.

**Decision:** port `createGrid` into **one** reusable control, `SheetGrid`, used twice. COMPONENTS.md's rule is "parameterise rather than duplicate", and this follows it.

## Files

| File | Change |
|---|---|
| `wpf/SheetGrid.cs` | **new.** A code-only `UserControl` that ports `createGrid` |
| `wpf/Themes/Sheet.xaml` | **new.** All `Sheet*` styles and the `DemoCard` and `GhostButtonSm` styles |
| `wpf/App.xaml` | merge `Themes/Sheet.xaml` |
| `wpf/UIComponents.Wpf.csproj` | none: wildcards pick up `SheetGrid.cs` and `Themes\Sheet.xaml` (no entries) |
| `wpf/DataControls.xaml` / `.cs` | replace the two `DataGrid` sections with two `SheetGrid` sections; leave the filter and selection tables alone |
| `wpf/Themes/Tokens.xaml` | Phase 0 only: serif font order and the new `BrandSoftHover` brush |
| `wpf/tests/UIAutomationTests.cs` | Phase 6: replace the grid tests |
| `wpf/tests/build-tests.bat` | Phase 6: add `/r:System.Windows.Forms.dll` for clipboard reads |
| `wpf/README.md` | Phase 6: update the covered list and the assertion count |

Don't touch `MainWindow.xaml`. Another agent has uncommitted work there; see the "Open item" under Phase 0.

## Public API (C# 5)

```csharp
public class SheetColumn
{
    public string Name;
    public string Type;      // "text" | "select" | "checkbox"
    public string[] Options; // select only
}

public class SheetGrid : UserControl
{
    public string Hint { get; set; }                 // text under the table (.sheet__hint)
    public void Setup(IList<SheetColumn> columns, IList<string[]> rows, bool allowAddColumn);
    public string GetCell(int r, int c);             // for tests and callers
    public int RowCount { get { return _data.Count; } }        // C# 5: no getter-only auto-props
    public int ColumnCount { get { return _columns.Count; } }
}
```

Store the data exactly as the web does: `List<SheetColumn>` and `List<List<string>>`. Checkbox values are the strings `"true"`/`"false"`. Store the selection as a `HashSet<string>` of `"r:c"` keys plus an anchor `(r,c)`.

`Render()` rebuilds the whole visual tree from the model, like the JS `render()`. `Paint()` only updates selected/info visuals, like the JS `paint()`. Don't add bindings or view models. The JS shape is the spec, and one-to-one is easiest to review.

Usage in `DataControls.xaml.cs`, matching the web demo data exactly:

```csharp
InputGrid.Hint = "Shift-click selects a range, Ctrl/Cmd-click adds cells, and clicking a row or column header selects the whole row or column.";
InputGrid.Setup(new[] { Text("Width"), Text("Height"), Text("Depth"), Text("Weight") },
    new[] { new[] {"Small","10","20","5"}, new[] {"Medium","14","24","8"}, new[] {"Large","18","30","12"} }, true);

TypedGrid.Hint = "Select a Category cell to change it with the dropdown, or tick IsActive. Selecting several cells and changing one updates the rest.";
TypedGrid.Setup(new[] { Text("Variant"), Text("Width"), Text("Height"),
        Select("Category", "Standard","Pro","Custom"), Check("IsActive") },
    new[] { new[] {"Small","10","20","Standard","true"}, new[] {"Medium","14","24","Pro","true"},
            new[] {"Large","18","30","Custom","false"} }, false);
```

---

## Visual spec

All numbers are in DIPs, which equal CSS px at 100% scale. Colours are token names from `Tokens.xaml`.

### Section shell (one per instance, in `DataControls.xaml`)

| Part | Web | WPF |
|---|---|---|
| Title | "Grid input" / "**Typed grid input**", serif 27px, weight 500, line-height 1.2 (32.4px), 46px above | `SectionTitle`, `Margin="0,46,0,0"`. Fix the second title text (the current one says "Typed grid") |
| Description | 15px, line-height 24, `Ink2`, max width 501px, 8px below the title | a TextBlock: `FontSize=15`, `LineHeight=24`, `LineStackingStrategy=BlockLineHeight`, `MaxWidth=501`, `HorizontalAlignment=Left`, `Margin=0,8,0,0`. Don't reuse `SectionDesc`, which is 14px |
| Card | `.demo`: `Surface`, 1px `Line`, radius **16**, padding 30, **no shadow**, 22px below the description | new style `DemoCard` in `Sheet.xaml`. `ClaudeCard` (radius 12 + shadow) is wrong for this, so don't edit it |

Description texts, verbatim:
- Grid input: "A table you can type into. Paste a block of Excel cells at once, select a row or column to edit every cell together, and clear a whole selection in one action."
- Typed: "The same grid with typed columns: plain text, a dropdown select, and a checkbox. Selection, bulk editing, paste and clearing all work across them."

### Sheet layout (inside the card, vertical, 14px gaps)

1. **Toolbar**, 32px tall:
   - On the left, the buttons "Copy selected" and "Clear selected" with an 8px gap.
   - On the right, the info text "0 cells selected": 12px, `Ink3`, vertically centred.
   - Buttons use the new `GhostButtonSm`: 600 weight, 13px, padding `12,7`, MinHeight 32, radius **7**, transparent background, `Ink2` text. On hover the background is `BgDeep` and the text `Ink`; there is no opacity change. That's the web `.btn--ghost.btn--sm` look. Measured widths are 108 × 32.
2. **Table frame**: 1px `Line` border, radius 12, `Surface` background, full width.
3. **Hint**: 12px, `Ink3`, line-height 19.2, wrapping.

### Table grid

Use a single WPF `Grid`. Each cell is a `Border` with `BorderThickness="0,0,1,1"` and `BorderBrush=Line`:
- The **last column** has no right border.
- The **last row** (the add row) has no bottom border.
- The frame supplies the outer 1px.

| Row | Height (includes its 1px bottom border) |
|---|---|
| header | 35 |
| each data row | 38 |
| add row | 37 (no border) |

Fixed heights are deliberate. Don't rely on TextBox padding, because WPF and Chromium line heights differ by about 0.5px.

| Column | Width |
|---|---|
| row-number column | 61 |
| text columns | `*` (star), `MinWidth=110` |
| select column | 123 |
| checkbox column | 122 |
| add-column column (only when `allowAddColumn`) | 61 |

Check against `web-measurements.txt`. Text columns there are 199.6 (grid) and 204.2 (typed) at a 922px frame.

**Corner clipping.** `Border.CornerRadius` does not clip children, so the `Surface2` header and the `BrandSoft` add row would poke out square. Clip the inner `Grid` with a `RectangleGeometry` of `RadiusX=RadiusY=11` (12 minus the 1px frame), recomputed in `SizeChanged`.

**Crispness.** Set `UseLayoutRounding="True"` and `SnapsToDevicePixels="True"` on the control. Set `FocusVisualStyle="{x:Null}"` on every cell control and header button.

### Header cells (`.sheet__head`)

- Use a flat-template `Button` so UIA `Invoke` works. Add `Focusable=False` so clicking doesn't steal focus from the grid.
- Background `Surface2`. Text 12px, SemiBold, `Ink2`, padding `12,0`, vertically centred.
  - Column headers are left-aligned.
  - Row numbers ("1", "2", …) are centred.
- The top-left corner cell is a plain `Surface2` border with no button.
- On hover the text turns `Ink`. Use the `Hand` cursor.
- When selected, the background is `BrandSoft` and the text `Brand`. Set this from `Paint()`, for example through `Tag="selected"` and a style trigger.

### Text cell (`.sheet__input`)

- The TextBox template is: background `Border` → focus-ring `Border` → `PART_ContentHost`.
- Font is Sans 14px, `Ink`. The background is transparent with no border.
- **The text must start 12px from the cell's left edge.** WPF's TextBox host adds about 2px of internal inset, so expect a content margin of about `10,0`. Verify against `web-grid-input.png`: "Small" starts 12px right of the column line.
- When **focused**, an inset 2px `Brand` ring fills the cell (radius ≈2; compare with `web-grid-focus.png`).
  - A focused cell is always selected too, because focusing selects it. So its background is `BrandSoft`, not `Surface2`.
- When **selected**, the cell background is `BrandSoft`.

### Select cell (`.sheet__select`)

- Use a `ComboBox` with a custom template. It looks exactly like the text cell: 14px, text 12px from the left, padding-right 34.
- Draw the **chevron** as a `Path` with `Data="M6,9 L12,15 L18,9"`. Use a 24-unit box scaled to 16×16, `Stroke=Ink3`, a stroke of about 1.33 (2 scaled), and round caps and joins. Place it 12px from the right and vertically centred.
- **Web quirk:** a *selected* select cell loses its chevron, because the `background:` shorthand wipes `background-image` (see the Category column in `web-grid-sel.png`). Replicate it by hiding the chevron when selected. The user can ask to fix the web side later instead.
- Items: an empty `""` item first (the web has `<option value="">`), then the options.
- The dropdown popup can't match Chromium's native list. Style it simply:
  - `Surface` background, 1px `LineStrong`, radius 8, `Shadow2`
  - items 14px, padding `12,7`
  - hover and current item `BrandSoft`

### Checkbox cell (`.sheet__checkbox`)

- The cell is centred. Pixel-sampled from the reference, it is **not** `ClaudeCheckBox`.
- 16×16, corner radius 2.5.
- **Unchecked:** `Surface` fill with a 1px `#767676` border.
- **Checked:** solid `Brand` fill with no visible border, and a white check stroke 2px wide with round joins. The approximate path in the 16-unit box is `M3.5,8 L6.5,11 L12.5,4`.
- Hover can stay unchanged; the idle states are what's compared.

### Add controls

- **"+" header** (add-column, top-right, only when `allowAddColumn`) and the **"+" add-row header** (bottom-left): `BrandSoft` background, `Brand` text, 600 weight, 20px, centred, `Hand` cursor.
- **Add-row wide cell** ("+ Add row", spanning every column after the row-number column, including the add-column column): `BrandSoft`, `Brand`, 600 weight, 13px, centred, `Hand`.
- On hover, all three turn `#F0D9CD`. Add `BrandSoftHover` to `Tokens.xaml`.
- **Body cells under the add-column header** are empty `Surface2` cells.
- All three must be `Button`s for UIA, with these AutomationIds:
  - `{Id}_AddCol`
  - `{Id}_AddRowHead`
  - `{Id}_AddRow`

---

## Behaviour spec (port of `createGrid`; keep every branch)

**Selection.** Cell selection goes through `PreviewMouseLeftButtonDown` on the cell. Don't mark it handled, so the control still gets focus.
- Plain click: clear the selection, select the cell, set the anchor.
- Shift-click with an anchor: add the anchor→cell rectangle **without clearing**. This is the web behaviour.
- Ctrl-click: toggle the cell and set the anchor.

**Drag.**
- On mouse down, set `dragging=true` and `dragAdditive=Ctrl`.
- On `MouseEnter` of another cell while the left button is pressed, call `SelectRange(anchor→cell, additive)`.
- On mouse up anywhere in the control, or on `LostMouseCapture`, set `dragging=false`.

**Focusing a cell** (`GotKeyboardFocus`): if the cell isn't selected, clear the selection, select it and set the anchor.

**Row-header click.**
- Plain click: select the whole row and set the anchor to `(r,0)`.
- Ctrl-click: toggle every cell in the row.
- Shift-click: select rows anchor..r; clear the selection first unless Ctrl is also held.
- Afterwards, move keyboard focus to the `SheetGrid` itself (`Focusable=True`). This is the web's `blur()`, and it keeps Ctrl+C/V working.

**Column-header click:** select the whole column, set the anchor to `(0,c)`, and focus the `SheetGrid`.

**Info text:** "1 cell selected" or "N cells selected". Use `0 cells` for zero.

**Typing** (TextChanged, ComboBox SelectionChanged, CheckBox Checked/Unchecked):
- Write the value into `data[r][c]`.
- If the selection has more than one cell, copy the value to every other selected cell **of the same type**. Select and checkbox values propagate **only within the same column**.
- Guard re-entry with a `_syncing` bool. **Don't** guard with `IsKeyboardFocusWithin`, because UIA `ValuePattern.SetValue` doesn't focus the cell and the tests rely on propagation.

**Delete/Backspace** in a cell with more than one cell selected: run `ClearSelected()` and mark the key handled. With a single cell selected, normal editing applies.

**Enter** in a cell: mark it handled, call `Ensure(r+1, c)` (which grows rows), re-render if the shape changed, and focus cell `(r+1, c)`.

**Clear selected:** set every selected cell to `""`; a checkbox becomes unchecked. Update the controls in place without re-rendering.

**Copy** (the "Copy selected" button, or Ctrl+C when focus is in the grid and the TextBox has **no** text selection):
- Only when more than one cell is selected.
- Copy TSV of the selection's **bounding box**. Checkboxes copy as `true`/`false`.
- Put it on the clipboard with `Clipboard.SetText`.
- *Deferred:* the web also shows a "Copied" toast. The WPF toast is private to `AdvancedControls`, so skip it and leave a `// ponytail: no shared toast yet` comment.

**Paste** (Ctrl+V anywhere in the grid, including when the `SheetGrid` itself is focused after a header click):
- Only handle the paste if the clipboard text contains `\t` or `\n`. Otherwise let the TextBox paste normally.
- Start at the focused cell, falling back to the anchor and then to `(0,0)`.
- Strip `\r` and drop the trailing empty line.
- `Ensure()` grows rows **and columns** as needed. New columns are named "Column N" of type text, **even when `allowAddColumn=false`**. That is the web behaviour.
- Checkbox cells: `/^(true|1|yes)$/i` (trimmed) becomes `"true"`, anything else `"false"`.
- Select cells: a value not in the options becomes `""`.
- Then `Render()` and focus the start cell.

**Add row:** append an empty row and re-render. **Add column:** append "Column N" (text), extend every row with `""`, and re-render.

**Accessibility and AutomationIds.** Root AutomationIds are `InputGrid` and `TypedGrid`; set them in `DataControls.xaml`.

| Element | AutomationId | Name |
|---|---|---|
| cell control | `{Id}_r{r}_c{c}` | `"{col name}, row {r+1}"` (the web aria-label) |
| row header | `{Id}_row{r}` | |
| column header | `{Id}_col{c}` | |
| Copy selected | `{Id}_Copy` | |
| Clear selected | `{Id}_Clear` | |
| info text | `{Id}_Info` | |

---

## Phases — do one, capture, stop for review

Each phase must build, the existing tests must still pass (except the grid tests that Phase 6 replaces), and the screenshots named below must be captured into `plan/wpf/ref/`.

### Phase 0 — shared prerequisites

- `Tokens.xaml`: change `Serif` to `Palatino Linotype, Book Antiqua, Georgia, serif`. The web lists Palatino first and Windows has it.
- `Tokens.xaml`: add `<SolidColorBrush x:Key="BrandSoftHover" Color="#FFF0D9CD"/>`.
- Create `Themes/Sheet.xaml` with `DemoCard` and `GhostButtonSm`, merge it in `App.xaml` (no csproj entry: wildcards).
- **Open item for the user/reviewer.** The web content column is 984px (max 1080 minus 2×48 padding). WPF's `MainWindow.xaml` content `StackPanel` has `MaxWidth="1000"`. Until that one line becomes `984`, text columns will be about 4px wider than the web. That doesn't fail review. **Don't edit `MainWindow.xaml`** while it has someone else's uncommitted changes.

Acceptance: it builds, and section titles across the app render in Palatino.

### Phase 1 — static render (grid input only)

- Build `SheetGrid`: `Setup`/`Render` with text cells, headers, the add controls, the toolbar, info and hint, clipping and borders. No interaction yet beyond the default TextBox.
- Replace the Grid input section in `DataControls.xaml`.

Capture: `pwsh plan/wpf/capture-wpf.ps1 -Id InputGrid -Out plan/wpf/ref/wpf-grid-input.png`.

Acceptance: compared with the Grid input card in `web-grid-input.png`:
- every row and column line is within ±1px
- the colours are exact
- the "+" corners are rounded with no square poke-out
- the text x-offset is 12px

### Phase 2 — editing visuals

- Focus ring, selected background, and selected headers.
- The info text updates for single-cell focus.

Capture: focus Height row 1, then `-Out plan/wpf/ref/wpf-grid-focus.png`.

Acceptance: matches `web-grid-focus.png`.

### Phase 3 — selection

- Click, Shift, Ctrl, drag, row-header and column-header selection, all with Paint.

Capture: invoke the row 2 header, then `-Out plan/wpf/ref/wpf-grid-sel.png`.

Acceptance: matches the top card of `web-grid-sel.png`, and the info reads "4 cells selected".

### Phase 4 — data operations

- Multi-cell typing propagation, Clear selected, Delete/Backspace, Enter-down with auto-grow, Copy (button and Ctrl+C), Paste, add row, add column.

Acceptance: a manual check of each item in the behaviour spec. List what was checked in the phase notes.

### Phase 5 — typed grid

- Select cells and checkbox cells, `allowAddColumn=false`, and the typed instance with its demo data and hint.
- Replace the Typed grid section in `DataControls.xaml` and delete the old `DataGrid` code it replaces.

Capture: `-Id TypedGrid -Out plan/wpf/ref/wpf-typed-grid.png`, plus the Category-column-selected state as `wpf-typed-grid-sel.png`.

Acceptance:
- matches the bottom cards of `web-grid-input.png` and `web-grid-sel.png`, including the hidden chevron on selected select cells
- no add-column column
- the checkbox matches the pixel spec

### Phase 6 — UI Automation tests

Replace the old `InputGrid`/`GridAddRow`/`GridAddCol`/`TypedGrid` checks in `tests/UIAutomationTests.cs` with these:

1. `InputGrid_r0_c0` has the value "Small"; `InputGrid_r2_c3` has the value "12".
2. Invoke `InputGrid_row1`: `InputGrid_Info` reads "4 cells selected".
3. With row 1 selected, SetValue "99" on `InputGrid_r1_c0`: `InputGrid_r1_c2` is also "99". This tests propagation.
4. Invoke `InputGrid_Clear`: `InputGrid_r1_c0` is "".
5. Invoke `InputGrid_col0`, then `InputGrid_Copy`. The clipboard text must equal the column's **current** cell values (read them through `GetValue`; test 4 cleared one) joined by `\n`.
   - Read the clipboard with `System.Windows.Forms.Clipboard.GetText()` on a dedicated `Thread` that has `SetApartmentState(ApartmentState.STA)`.
   - This needs the `build-tests.bat` reference.
6. Invoke `InputGrid_AddRow`: `InputGrid_r3_c0` exists. Invoke `InputGrid_AddCol`: `InputGrid_col4` exists and its Name is "Column 5".
7. `TypedGrid_AddCol` **does not** exist.
8. `TypedGrid_r0_c4` is a checkbox with toggle state On; Toggle sets it Off.
9. `TypedGrid_r0_c3` is a ComboBox: select "Custom" through `ExpandCollapse` + `SelectionItem`, and its value becomes "Custom".
10. *(Optional)* Paste through `SendKeys` Ctrl+V. Skip it if keyboard injection isn't wanted.

Then update `wpf/README.md`: the covered list and the assertion total.

Acceptance: `UIAutomationTests.exe` passes everything.

---

## Reviewer checklist (what I'll check on each phase)

- [ ] Only the files listed above changed. `MainWindow.xaml` is untouched.
- [ ] C# 5 compliant, and `build.bat` is clean.
- [ ] Screenshots are captured and diffed against the `web-*` reference: lines ±1px, exact hex colours, text offsets.
- [ ] Every behaviour-spec branch is ported, including the odd ones: Shift-click doesn't clear, paste grows columns even when `allowAddColumn=false`, and propagation is type- and column-restricted.
- [ ] No `DataGrid` is left in the two grid sections, and no dead code is left from the old version.
- [ ] Tests pass.
