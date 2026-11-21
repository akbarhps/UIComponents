# 04 — Tables: fix-up of the existing port (agent "tables")

The tables agent ported five web tables before this plan existed. This file is the **review of that work** plus the phased fix list. The components are already functional; the gaps are visual, and most come from a few shared mistakes. So **Phase 1 fixes the shared table primitives once**, and later phases only tune each table.

Source of truth: `web/index.html`
- CSS: `.table` ~642–663, `.badge` ~362–375, `.filters` / `.segment` ~667–677, sort / pin / sticky ~680–693, `.bulkbar` ~696–703, `.pagination` / `.page-btn` ~715–727
- markup: `#data-table` 1594, `#selection-table` 1637, `#filter-table` 1666, `#column-filters` 1711, `#sort-table` 1756

References in `plan/wpf/ref/`:
- `web-table-<id>.png`: web, idle
- `web-table-selection-table-selected.png`: two rows ticked, with the bulk bar showing
- `wpf-before-table-<id>.png`: the current WPF state, captured off-screen at 1400×1100 on 2026-11-21
- `web-tables-measurements.txt`: every row height, column x/width, badge, pagination, filter and sort control. Positions are relative to the `.demo` card.

Read `plan/wpf/README.md` first. Rules 4 (hidden desktop, agent name `tables`) and 5 (never commit) are mandatory.

---

## Review of the current port

**Good:**
- All five tables exist, have real data and are interactive: search, segments, the five column filters, row and header checkboxes with the bulk bar, sort and pin.
- The pagination is a shared `TablePagination` control.
- The table row and header styles are shared rather than duplicated.
- The header tint `#FAF9F5` is exact. Tests pass (64/0).

**Shared mistakes (these affect every table):**

| # | Web | WPF now |
|---|---|---|
| S1 | Section description: 15px, line-height 24, max width 501, full text, **22px** to the card | `SectionDesc` is 14px, single line, uses a **shortened** text, with 16px to the card |
| S2 | Card is `.demo`: radius **16**, no shadow | `ClaudeCard`: radius 12 plus a shadow |
| S3 | Table columns **fill the full 922px**. Row lines and the header span the whole width | Columns are fixed or narrow, so rows stop at about 777px while the header runs to 952 (data table, column filters) |
| S4 | The table frame is 1px `Line` with radius 12 on **all four corners**. The last row has **no** bottom border | `ClipToBounds` on a rounded `Border` clips to a *rectangle*, so the bottom corners are square and a bottom line is drawn |
| S5 | Cells: padding 13/16, body 14px/22.4 `Ink`, header 12px 600 `Ink2` on `Surface2`. Header row 46.2, body rows **52** (single line) or **67.4** (two-line name) | Header 43, body 55 / 63–64 |
| S6 | **The first column is a `<th scope="row">`** in data table, column filters and sortable, so it gets *header* styling: `Surface2` background, 12px **600 `Ink2`** name, 12px 400 `Ink3` sub-line (2px gap), no wrapping | 14px bold `Ink` on white |
| S7 | `.table__num` (Budget) is **left-aligned** in both header and cells. `.table th,td{text-align:left}` outranks `.table__num`. Quirk, but it's the web | The Budget header is right-aligned or centred |
| S8 | Badge: a pill **24 tall** with radius 99, padding 6/10, 12px 600, a 6px dot and a 6px gap. "Active" is 65.5 wide | Taller oval (~26–28) |
| S9 | Pagination: 16px above, all 50.2 tall. "Show [select 70×34.2] per page" is 13px `Ink2`. Page buttons are 32×32, radius 7, 13px 600, and the active one is `Ink` with white text. The info text is 13px `Ink3`. The select is `.select--sm`: 13px, padding 7/9, chevron 8px from the right | The select is a narrow 50px fake with a large chevron. The rest is close |

**Per-table mistakes:**
- **Data table:** the avatar is 30. The name and sub-line follow S6 (12px). The "Open" action is 13px 600 `Brand`, with no background.
- **Row selection:**
  - The checkbox column is 80 wide, and the header checkbox has to sit at the same x as the row checkboxes (it's offset now).
  - The checkbox is the web `.check` (19px). Use `ClaudeCheckBox` once task 02 has fixed it; don't make your own.
  - The bulk bar is wrong: see Phase 3.
- **Filters:**
  - The search box is `.input` with an icon: 320×**45**. Reuse `FieldInput` plus the search-icon overlay from `Themes/Forms.xaml` (task 02) instead of the hand-built 42px box.
  - The segment is 37 tall (padding 3, buttons 31 tall, 13px 600, radius 7, active is `Surface` with `Shadow1`), with 12px gaps.
  - The table starts 16px below the filter bar.
- **Column filters:**
  - The count line sits **above** the table, right-aligned, 13px `Ink3`.
  - The filter row is 51.2 tall, with a `Surface` background and padding 8/10. Inputs are `.input--sm`: 34.2 tall, 13px, padding 7/9, radius 8.
  - Inputs fill their column; the web's are wider ("Min amount" fits).
- **Sortable:**
  - The table is 973 wide inside the 922 wrapper, so it scrolls horizontally. Column widths are in the measurements file.
  - Body cells **wrap** ("Ada / Lovelace" spans two lines, giving 71.8 rows). WPF doesn't wrap, and truncates "Design team".
  - The pinned first column is `Surface2` (S6) and has a 1px right `Line` (`box-shadow 1px 0 0`).
  - Sort buttons are 12px 600 `Ink2` with a 13px chevron at opacity 0.4; asc/desc turn them `Brand`, opacity 1, with the chevron rotating 180 for asc. The pin button is 18×18 (14px icon), `Ink3`, and `Brand` when pressed.
  - The horizontal scroll bar is WPF's thick grey default. Restyle it as a thin (8px) `LineStrong` thumb on a transparent track, the closest neutral look.

**Process issues:**
1. `eba14b6` swept other agents' files in with `git add -A`. Rule 5 applies now: no more commits.
2. `BadgeWarning` in `Controls.xaml` is unused. Delete it.
3. **Ownership.** The `Table*`, `StatusBadgeTemplate`, `SegmentButton`, `SortHeaderButton`, `PinButton`, `WebSelect` and `TablePagination` styles and controls belong to `tables`. `PlaceholderInput` duplicates task 02's `FieldInput`; replace its two uses with `FieldInput` and delete it in Phase 1. Don't touch `ClaudeCheckBox` (task 02), `Sheet*` (task 01) or `Field*` (task 02).

---

## Files

- `wpf/Themes/Controls.xaml`: only the table styles listed under Ownership
- `wpf/TablePagination.xaml` / `.cs`
- `wpf/MainWindow.xaml`: **only** the `Data table` section block (other agents edit the rest of the file)
- `wpf/DataControls.xaml` / `.cs`: only the filter and selection table sections (`Grid input` and `Typed grid` belong to agent `grid`)
- `wpf/AdvancedControls.xaml` / `.cs`: only the column-filters and sortable sections
- `wpf/tests/UIAutomationTests.cs`: table assertions only

Use `SheetSectionDesc` and `DemoCard` from `Themes/Sheet.xaml` for the shell (S1, S2). Don't copy them.

## Capture recipe (hidden, nothing on the user's screen)

```powershell
wpf\build.bat tables
pwsh wpf/tests/run-hidden.ps1 -Agent tables -NoTests -Section "Data table" -Capture "Sections=plan/wpf/ref/wpf-table-data-table.png"
# Section titles: "Data table", "Table with row selection", "Table with filters",
#                 "Table with column filters", "Sortable table with sticky column"
# -Capture also accepts x:Name; "Sections" is the content column in standalone mode
pwsh wpf/tests/run-hidden.ps1 -Agent tables    # tests
```

Compare with `plan/wpf/ref/web-table-<id>.png`. The web shot includes the sidebar; the WPF content column starts at web x≈314.

---

## Phases: one at a time, capture, stop for review

### Phase 1: shared primitives (S1–S9) and cleanup

- **S1 and S2** for all five sections. Use the full web description texts, verbatim, from `index.html`.
- **S3 and S4:**
  - Columns are star-sized to fill 922. The ratios come from the measured web widths, for example data table 231 : 160 : 160 : 153 : 114 : 102.
  - Make the frame clip truly rounded: a `Border` with `CornerRadius=12`, plus a `Clip` `RectangleGeometry` (radius 11) on the inner content, updated on `SizeChanged`. That's the same technique as `SheetGrid.UpdateClip`.
  - Drop the bottom border of the last row.
- **S5:** row heights and padding, as in the table above.
- **S6:** a row-header style for the first column (`Surface2` background, 12px 600 `Ink2`, sub-line 12px 400 `Ink3`). Apply it to data table, column filters and sortable only. Filters and selection use normal `<td>`s.
- **S7:** left-align Budget everywhere.
- **S8:** the badge template: Height 24, `CornerRadius` 12, padding 10,0, dot 6, gap 6.
- **S9:** `TablePagination`: a real `.select--sm` look (70×34.2), 16px top margin, and the rest of the spec.
- Delete `BadgeWarning`. Replace `PlaceholderInput` with `FieldInput` (and `.input--sm` sizing where the web uses `--sm`), then delete it.

Capture all five sections.

Acceptance, for each table compared with its web reference:
- the card, description and gaps match
- the header row is 46.2 ±1 and body rows 52 / 67.4 ±1
- column x positions are ±2px of `web-tables-measurements.txt`
- the corners are rounded on all four sides, and the last row has no bottom line
- the badge is 24 tall

### Phase 2: data table and filters

- **Data table:** the avatar, the name/sub-line, and the Open action.
- **Filters:**
  - The search box uses `FieldInput` with the search overlay: 320×45, at y 31.
  - The segment geometry matches the measurements (35, 37 tall; buttons 39.5 / 60.3 / 72.3 / 75.7 wide).
  - The count is 13px `Ink3`, right-aligned.
  - The table starts at y 92.
  - The empty state is `.empty`: padding 44/20, 14px `Ink2`, with a 15px `Ink` strong line.

Capture both. Acceptance: matches `web-table-data-table.png` and `web-table-filter-table.png` within ±1–2px.

### Phase 3: row selection

- **Checkboxes:** the column is 80 wide, and the header and row checkboxes are vertically aligned. Use `ClaudeCheckBox` after task 02's Phase 3 lands. Until then, leave the checkbox style alone and report it.
- **Selected rows:** **no** row tint (see the web reference).
- **Bulk bar** (shown only when at least one row is ticked; the table shifts down):
  - `BrandSoft` background, 1px `#ECD4C8` border, radius 8, padding 10/14, 16px below
  - text "N rows selected" / "1 row selected", 14px 600 `Brand`
  - "Export" (secondary, small) and "Delete" (danger, small) on the right, 8px apart
  - the bar is about 54 tall
  - `SecondaryButtonSm` is defined by task 05 (Themes/Display.xaml); reuse it, do not define another. Add only `DangerButtonSm` to `Controls.xaml` next to `GhostButtonSm`'s pattern (same spec as below; if 05 has not landed `SecondaryButtonSm` yet, ask the reviewer): radius 7, padding `12,7`, MinHeight 32, 13px 600; hover changes the background, not the opacity.

Capture idle, plus a selected state. `--capture` can't click, so for this one capture, temporarily tick rows 1 and 3 in code-behind under `#if DEBUG` behind an env var, or report the visual from the test run. Ask the reviewer if unsure.

Acceptance: matches `web-table-selection-table.png` and `web-table-selection-table-selected.png`.

### Phase 4: column filters and sortable

- **Column filters:**
  - The count line is above the table.
  - The filter row is 51.2 tall, with `.input--sm` / `.select--sm` controls at the measured widths.
  - The "Open" column is 72 wide.
- **Sortable:**
  - The table is 973 wide with horizontal scroll and the thin scroll bar.
  - Body cells wrap (rows 71.8).
  - Sort buttons and the pin button match the specs above, and the pinned column gets its `Surface2` background and right line.

Capture both. Acceptance: matches `web-table-column-filters.png` and `web-table-sort-table.png`.

### Phase 5: tests and README

- Keep all the existing table assertions green.
- Add:
  - The bulk bar is hidden at 0 rows selected.
  - It reads "1 row selected" after ticking one row and "N rows selected" after select-all.
  - The sort header toggles asc → desc (check the first cell's value changes).
  - The pin toggles.
  - A filter in the column-filters row reduces the count text.
- Update the covered list and the assertion count in `wpf/README.md`.

---

## Reviewer checklist

- [ ] Only the files and sections listed above changed. Nothing in the grid or forms areas changed.
- [ ] C# 5, and the build is clean (`build.bat tables`).
- [ ] Captures were made with `run-hidden.ps1` and match the web references at the tolerances above.
- [ ] `BadgeWarning` and `PlaceholderInput` are deleted, and no other dead styles remain.
- [ ] Tests pass on the hidden desktop. No commits.
