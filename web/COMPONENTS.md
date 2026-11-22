# Web component library — usage guide

This library lives in **one file**: `web/index.html`. HTML, CSS and JavaScript are all inline, there are no dependencies, and the file can be opened directly or sent around. This document tells an agent **which component to pick, when not to pick it, and exactly what to wire**.

> For consumers: link the generated `dist/ui.css` and `dist/ui.js` (never copy them) and take each component's markup from `SNIPPETS.md` (`### <name>` per row below).

## How the library is put together

- All colours, radii, shadows and fonts are CSS custom properties in `:root`. Change a token there, not individual rules.
- Components are scoped classes (`.btn`, `.input`, `.card`, `.modal`, `.picker`, …). Link `web/dist/ui.css` and `web/dist/ui.js` (generated; never copy them) and take each component's markup from `SNIPPETS.md`.
- The `<script>` sits at the end of `<body>` and auto-initialises components by class (`.combo`, `.multi`, `.picker`, `.accordion`, `.file-input`, `.tag-input-wrap`, `.ac`, `.menu`, `.overlay`, `.table-wrap` pagination). Anything added to the DOM **after** that script runs must be initialised with one of the exported functions (`openOverlay`, `closeOverlay`, `setFieldError`, `showToast`, `createGrid`, `bindSimpleFilters`, `bindColumnFilters`, `bindRowSelection`, `bindSortable`, `createCommandPalette`) or re-initialised manually. Everything else auto-inits by class at load.
- IDs in the demo (`#sheet`, `#filterTable`, `#sortTable`, `#modal`, …) exist for the showcase. In your app, keep the class hooks and change the IDs — but update any JS that references them.
- **`index.html` is the reference implementation.** For exact markup, class names and CSS, copy from the matching section there. This doc explains the decisions and wiring; the HTML is the working example.

---

## Quick picker

| The user needs to… | Use | Do **not** use |
|---|---|---|
| Trigger a single action | Button | Action menu (only for 3+ actions) |
| Choose from 3+ actions on one object | Action menu | A row of buttons |
| Pick one option from a short, known list | Native select | Searchable select (it's for long lists) |
| Pick one option from a long list, with typing | Searchable select | Multi-select |
| Pick several values from a known list | Multi-select | Tag input |
| Enter freeform values not in a fixed list | Tag input | Multi-select |
| Type and get filtered suggestions | Autocomplete | Searchable select (no freeform value) |
| Pick a date / month / year / time | Date picker | A plain text input |
| Upload files | File input | Tag input |
| Edit many cells as a grid | Grid input / Typed grid | A plain table |
| Show a short status | Badge | Alert |
| Show inline feedback in a form | Alert | Toast |
| Show transient feedback after an action | Toast | Alert |
| Confirm a destructive action | Modal / drawer confirmation | Toast |
| Block the UI while work finishes | Loading modal | Progress bar |
| Show known-duration work | Progress bar | Loading modal |
| Hold layout while data loads | Skeleton | Empty state |
| Show "nothing here yet" | Empty state | Skeleton |
| Ask for input without leaving the page | Modal form | Drawer form (it's for longer forms) |
| Longer form, side panel | Drawer form | Modal form |
| Navigate a site path | Breadcrumbs | Tabs |
| Switch views on the same page | Tabs | Breadcrumbs |
| Reveal extra detail in place | Accordion | Modal |
| Show a wizard's position | Stepper | Progress bar (it's for time-based work) |
| Show tabular data | Data table | Grid input (it's for editing) |
| Narrow rows by search/status | Table with filters | Column-filters table (use when each field has its own rule) |
| Filter each column independently | Column-filters table | Table with filters |
| Select several rows and act on them | Row selection table | Data table |
| Sort and pin columns | Sortable table | Data table |
| Run a command by search | Command palette | Action menu |

---

## Foundations

### Buttons — `#buttons`

- **Use for:** a single action. Variants: `btn--primary`, `btn--dark`, `btn--secondary`, `btn--ghost`, `btn--danger`; sizes `btn--sm`, `btn--lg`.
- **Don't use for:** a list of actions on one object — use an action menu.
- **Wire:** `<button class="btn btn--primary" id="myBtn">Save</button>`, then `document.getElementById('myBtn').addEventListener('click', handler)`. For a loading state, replace inner HTML with `<span class="spinner"></span>Working…` and set `disabled`.

### Action menu — `#action-menu`

- **Use for:** three or more actions on one object, anchored to a button or `⋯` icon.
- **Don't use for:** a single action — use a button. Don't use it as navigation — use the sidebar or breadcrumbs.
- **Wire:** `.menu` wrapper with a trigger carrying `aria-haspopup="menu"` + `aria-expanded` + `aria-controls`, and a `.menu__panel[role=menu][hidden]` with `.menu__item[role=menuitem]` buttons. Use `.menu__divider[role=separator]` and `.menu__item--danger` where needed. Keyboard (arrows, Home/End, Enter, Escape) and edge-aware positioning are automatic. `data-action` on an item is demo-only — replace with your handler.

### Badges — `#badges`

- **Use for:** short status labels in tables and cards. `badge--neutral|brand|success|warning|danger`; add `badge--plain` to drop the dot.
- **Don't use for:** sentences or feedback — use an alert.
- **Wire:** pure CSS, no JS. `<span class="badge badge--success">Active</span>`.

### Alerts — `#alerts`

- **Use for:** inline context or validation feedback inside content. `alert--info|success|warning|danger`.
- **Don't use for:** feedback after an action — use a toast. Don't use a coloured `border-left` to fake one.
- **Wire:** pure markup. For form errors, reuse `.field.has-error` + `.field__error` instead.

### Tooltips — `#tooltip-demo`

- **Use for:** a short label on an icon-only control.
- **Don't use for:** instructions, links or anything needing interaction.
- **Wire:** wrap the control in `<span class="tip" data-tip="Label" tabindex="0">…</span>`. Placement classes: `tip--bottom`, `tip--left`, `tip--right`. Keyboard focus shows it too. Sidebar tooltips use `data-tip` on the nav links and are generated by `.app--collapsed .nav a::after`.

### Sidebar — collapsible, grouped nav

- **Use for:** primary app navigation.
- **Wire:** `.app` is the grid; toggling `.app--collapsed` switches to the 72px icon rail. `#sidebarToggle` (page icon beside the title) and `#brandToggle` (the brand spark icon, used to expand) both call `setCollapsed()`. Links live inside `.nav-group` blocks; each `.nav-label` is a group heading. `navLinks.forEach` copies link text into `data-tip` and sets `aria-label`.

---

## Inputs

### Forms — `#forms`

- **Use for:** text, email, search, select, textarea, checkbox, radio and switch.
- **Wire:** wrap each control in `.field` with `.field__label`, optional `.field__hint` and `.field__error`. Add `.has-error` to the `.field` to reveal the error and turn the input border red. The switch is `<label class="switch"><input type="checkbox"><span class="switch__track"></span>Label</label>`.

### Tag input — `#tag-input`

- **Use for:** freeform values the user types (skills, labels).
- **Don't use for:** a fixed list — use multi-select.
- **Wire:** `.tag-input-wrap` with `data-tags="a,b"` (initial values) and `data-max="6"` (optional cap). Inside: `.tag-input > input.tag-input__field`, plus `.tag-input__hint` for the count. JS adds on Enter/comma, removes via Backspace or chip button. It rejects duplicates case-insensitively.

### Autocomplete — `#autocomplete`

- **Use for:** suggestions from a known list while allowing freeform text.
- **Don't use for:** empty-state or search results pages.
- **Wire:** `.ac` with `data-options="A,B,C"` and optional `data-multiple="true"`. Inside: `.ac__input` and `.ac__list`; optional sibling `.ac__hint`. With `data-multiple`, picks become chips; without it, the input keeps your typed text. Suggestions are capped at 8 in `show()`.

### Searchable select — `#combobox`

- **Use for:** choosing exactly one value from a long list.
- **Don't use for:** freeform values (autocomplete) or multiple values (multi-select).
- **Wire:** `.combo` containing `.combo__trigger[aria-expanded][aria-controls]`, `.combo__panel`, `.combo__input`, `.combo__list[id][role=listbox]` and `.combo__option[role=option]`. Give the list a unique id and point `aria-controls` at it. Keyboard: arrows, Enter, Escape; outside-click closes.

### Multi-select — `#multi-select`

- **Use for:** several values from a fixed list, shown as chips.
- **Don't use for:** freeform tags.
- **Wire:** `.multi[data-placeholder]`. **All of these are required** — the JS queries them directly and will throw if missing: `.multi__trigger`, `.multi__panel`, `.multi__input`, `.multi__list`, `.multi__empty`, `.multi__chips`, `.multi__count`, `.multi__select-all`, `.multi__clear`, `.multi__done`. Options are `label.check.multi__option` with a checkbox and `.multi__option-label`. "Select all" targets only visible (filtered) options.

### Date and time pickers — `#date-picker`, `#datetime-picker`

- **Use for:** dates, months, years, optionally with time.
- **Wire:** `.picker` with `data-mode="date|month|year"`, `data-time="true|false"`, and `data-seconds="true"` to add a seconds field. Inside: `.picker__trigger` containing `.picker__value` whose text is the placeholder. The panel, navigation, time fields and Clear/Today/Apply buttons are generated by `initPicker()` — don't author them. Give each trigger its own `aria-label`.
- **Note:** formatting is fixed English shorthand (`Nov 21, 2026, 09:05`).

### File input — `#file-input`

- **Use for:** file selection with a preview list.
- **Don't use for:** real upload progress — pair it with the progress bar.
- **Wire:** `.file-input[data-multiple="true|false"]` containing `label.file-drop > input[type=file]`, `.file-list` and `.file-count`. The multi variant scrolls at 220px via CSS. No upload happens; the JS only manages the selected list.

### Grid input — `#sheet-input`

- **Use for:** entering many rows/columns of values, Excel-style pasting, selecting ranges.
- **Don't use for:** read-only data — use a data table.
- **Wire:** call `createGrid('productGrid', columns, rows)`. Columns are objects: `{ name:'Width', type:'text' }`, `{ name:'Category', type:'select', options:['Standard','Pro'] }`, `{ name:'IsActive', type:'checkbox' }`. Markup: `.sheet#productGrid` (not `#sheet`, which the demo auto-wires) with `.sheet__toolbar` containing `[data-sheet-copy]`, `[data-sheet-clear]` and `[data-sheet-info]`, then `.sheet__scroll > table > thead/tbody`. Add rows/columns from in-table `+` controls. To disable adding columns, pass `{ allowAddColumn: false }`.
- **Keyboard:** click / Shift-click / Ctrl-click / drag select, Enter moves down, Delete or Clear wipes the selection. Copy (Ctrl+C) and paste (Ctrl+V) are handled globally for the last-used grid, including selections made from row/column headers. Paste expects tab/newline text.

### Typed grid — `#typed-sheet`

- **Use for:** the same grid behaviour where some columns are dropdowns or checkboxes.
- **Don't use for:** a read-only table — use a data table.
- **Wire:** same `createGrid()` call, with `type:'select'` + `options:[…]` and `type:'checkbox'` columns. This instance passes `{ allowAddColumn: false }`, so only the in-table add-row control appears. Multi-cell editing, clearing, copy and paste work across typed cells; paste parses `true/false/yes/1` for checkboxes and ignores select values not in `options`.
- **Accessibility:** generated controls carry `aria-label="{column name}, row {n}"` and header cells use `scope="col"` / `scope="row"` — keep these if you change `cellHTML()`.

---

## Feedback

### Toasts — `#toasts`, `#toast-variants`

- **Use for:** brief confirmation after an action.
- **Don't use for:** validation errors that need to stay visible — use an alert or field error.
- **Wire:** `showToast(type, title, text, options)` where type is `success|info|error` and options are `{ dismissible:true, persistent:true, action:'Undo' }`. Appends to `#toastStack`. Auto-dismisses after 3.6s unless `persistent`. The action button calls `showToast('success','Done',…)` in the demo — replace that with your undo handler.

### Progress bar — `#progress`

- **Use for:** work with a known percentage (uploads).
- **Don't use for:** unknown-duration work — use the loading modal.
- **Wire:** `.progress > .progress__fill[id]`, plus `.progress__meta` with a label and `[id]` for the percentage. Call `setProgress(0–100)`. The demo uses a timer; replace it with your real progress source.

### Skeleton — `#skeleton`

- **Use for:** holding layout while data loads.
- **Don't use for:** a genuinely empty result — use the empty state.
- **Wire:** pure CSS. `.skeleton` for blocks, `.sk-circle` for avatars, `.sk-line` for text with inline widths. Under `prefers-reduced-motion` the shimmer stops and a `.motion-cue` badge appears.

### Empty state — `#empty-state`

- **Use for:** no data, no results, or a fresh account. Always include one clear next action.
- **Don't use for:** loading — use a skeleton.
- **Wire:** pure markup. `.empty-state` with `.empty-state__icon`, `.empty-state__title`, `.empty-state__text`, `.empty-state__actions`.

### Loading modal — `#loading`

- **Use for:** blocking every action until work completes.
- **Don't use for:** anything the user should be able to cancel or ignore — use a progress bar or toast.
- **Wire:** `.overlay.overlay--blocking#loadingOverlay`; call `openOverlay(loadingOverlay, focusEl)` at start and `closeOverlay(loadingOverlay)` when the work finishes. Backdrop clicks and Escape are ignored because of `overlay--blocking`. The demo uses a 3s timer.

---

## Overlays

### Modal / modal form — `#modal`, `#form-modal`

- **Use for:** short, focused confirmation or a short form.
- **Don't use for:** long forms — use a drawer.
- **Wire:** `.overlay[id][role=dialog]` with `.modal`; buttons call `openOverlay(el, focusEl)` / `closeOverlay(el)`. For a form, add `<form>` inside and validate in its `submit` handler, using `.field.has-error`. All overlays share a focus trap and per-dialog focus restore via `lastFocusByOverlay`.

### Drawer form — `#drawer`

- **Use for:** longer forms, side-panel editing, create/update flows.
- **Wire:** `.overlay.drawer-overlay > .drawer` with `.drawer__head`, `.drawer__form` (containing `.drawer__body` and `.drawer__foot`) and optional `.drawer__confirm`. Toggle `.drawer__confirm[hidden]` for an inline delete confirmation; Escape dismisses the confirmation first. The update demo shows Delete / Cancel / Save.

### Command palette — `#command-palette`

- **Use for:** power-user navigation and actions by search.
- **Don't use for:** a primary, discoverable action — use a button.
- **Wire:** `createCommandPalette(overlay, commands)`, called once (one palette per page); `overlay` holds `.cmdk__input` and `.cmdk__list` (see SNIPPETS.md). Commands are `{ group, label, icon, shortcut, run() }`. Global Ctrl/Cmd+K opens the palette; `palette.open` / `palette.close` are returned for a button. Each `run()` should perform the action or navigate.

---

## Disclosure and progress

### Accordion — `#accordion`

- **Use for:** optional or secondary detail in place.
- **Don't use for:** primary content the user must see — keep it visible.
- **Wire:** `.accordion > .accordion__item > button.accordion__trigger[aria-expanded][aria-controls]` plus `.accordion__panel[id][hidden]`. Add `data-single` to the container for one-open-at-a-time. IDs must match `aria-controls`.

### Tabs — `#tabs`

- **Use for:** switching between peer views on the same page.
- **Don't use for:** navigation between pages — use breadcrumbs or the sidebar.
- **Wire:** `.tabs` with `.tab[role=tab][aria-selected][aria-controls]`, and `.tabpanel[id][role=tabpanel][hidden]`. The JS toggles `aria-selected` and `hidden`; no data attributes required.

### Stepper — `#stepper`

- **Use for:** showing position in a wizard.
- **Don't use for:** time-based progress — use the progress bar.
- **Wire:** `#stepperDemo` with `.step` children, plus `#stepPrev`, `#stepNext`, `#stepStatus`. `currentStep` drives `renderStepper()`. The demo only shows the indicator — you must add and swap per-step content panels yourself.

### Breadcrumbs — `#breadcrumbs`

- **Use for:** path/wayfinding above page content.
- **Wire:** pure markup. `nav.crumbs` with `a`, `.crumbs__sep` icons, an ellipsis for collapsed paths, and a final `.crumbs__current[aria-current=page]`.

---

## Data display

### Cards — `#cards`, avatars — `#avatar`

- **Use for:** grouping related content; avatars for user identity.
- **Don't use for:** tables of many rows — use a data table.
- **Wire:** pure markup. `.card` with `.card__title`, `.card__text`, `.card__foot`. Avatars: `.avatar` with `--sm`/`--lg` and tone variants.

### Data table — `#data-table`

- **Use for:** read-only tabular data.
- **Wire:** `.table-wrap > table.table`. For pagination add a `.pagination` block in the same parent element as the `.table-wrap` (no `.demo` needed), containing `[data-page-limit]` (select, options 10/25/50/100, default 25), `[data-page-info]`, `[data-page-list]`, `[data-page-prev]`, `[data-page-next]`. A table-wrap whose parent has no `[data-page-limit]` is skipped by the pagination loop.

### Table with row selection — `#selection-table`

- **Use for:** bulk actions on rows (export, delete).
- **Wire:** `bindRowSelection(table, bulkbar, onBulk)`, called once. The header checkbox is `.select-all-check input`, each row has `.row-check`, the bulk bar's `<strong>` shows the count, and `[data-bulk="export|delete"]` buttons call `onBulk(action, checkedRows)`. The function never removes rows: the callback does (the demo deletes rows there; replace with a confirmed server call).

### Table with filters — `#filter-table`

- **Use for:** one global search plus status segments.
- **Wire:** `bindSimpleFilters('filterTable', 'tableSearch', 'tableFilters', 'tableCount')`. Rows need `data-status`; filter buttons need `data-status`. Remove filters entirely if not needed.

### Column-filters table — `#column-filters`

- **Use for:** each column has its own rule (text contains, select equals, number ≥).
- **Wire:** `bindColumnFilters('columnTable', 'colCount')` — the count element id is optional. Controls live in `thead input/select`; number inputs compare against `data-sort-value` or the cell's numeric text. Keep one control per column in the same order as the cells.

### Sortable table — `#sort-table`

- **Use for:** click-to-sort headers and an optional pinned column.
- **Wire:** `bindSortable(table)`, called once. `.sort-btn[data-sort-key]` on header buttons; `data-sort-value` on cells whose display text is not sortable (money, dates, percentages). The column is derived from the header's `cellIndex`. A `.pin-btn` in the table toggles `.sticky-column`. The showcase also calls `bindColumnFilters('sortTable')`.

---

## Rules for agents extending this file

1. Add a section with `<section class="section" id="…">` and a matching `<a href="#…">` inside the correct `.nav-group`.
2. Add CSS next to the component's existing block; use tokens, don't hard-code colours.
3. Reuse an existing factory/component before writing a new one. If a second instance is needed, parameterise rather than duplicate (see `createGrid` and `bindSimpleFilters`).
4. Wire behaviour in the single inline `<script>` near the bottom, or call the component's factory after injecting dynamic markup.
5. Keep `aria-*` attributes in sync (`aria-expanded`, `aria-selected`, `aria-pressed`, `aria-current`, `role=listbox/menuitem`, `hidden`). Focus traps and focus restore are automatic for `.overlay`.
6. Run the design detector after UI edits (Windows):
   `& "C:\Users\akbar\.agents\skills\impeccable\scripts\impeccable.cmd" detect --json web\index.html`
   Expect three intentional warnings: the cream palette, the skeleton shimmer and the loading bar's infinite animation.
7. Commit only when the user asks; they have been reviewing each iteration before committing.

### Verification checklist

The detector checks code patterns, not behaviour. After any UI edit, also:

1. Open `web/index.html` in a browser and drive **every state you touched** — open/close, hover, focus, error, empty, loading, selected. Don't just look at the resting screen.
2. Check the browser console for errors; a thrown error early in the script silently disables everything after it.
3. Check the 390px-wide layout (single column, no horizontal page scroll) if you changed layout or tables.
4. Check that the overlay you touched doesn't shift the page when it opens (scrollbar gutter).
5. Run the design detector (rule 6) and confirm only the three expected warnings remain.

Bugs found in this library that this checklist would have caught: overlay scroll shift, grid copy/paste from row/column headers, and row-header CSS selectors losing their border radius.

---

## Appendix: four canonical snippets

These are the highest-friction wirings. Everything else is either pure markup or a single class/attribute change — copy the exact markup from `index.html`.

### 1. Grid input — column and row shape

```js
// Markup: <div class="sheet" id="productGrid"> with a toolbar containing
// [data-sheet-copy], [data-sheet-clear], [data-sheet-info], and
// .sheet__scroll > table > thead + tbody. The + row/column controls are
// generated for you.

createGrid('productGrid', [
  { name: 'Variant', type: 'text' },
  { name: 'Category', type: 'select', options: ['Standard', 'Pro', 'Custom'] },
  { name: 'IsActive', type: 'checkbox' }
], [
  ['Small', 'Standard', 'true'],
  ['Large', 'Custom', 'false']
], { allowAddColumn: false }); // omit the options object to keep the + column
```

`type` must be `text`, `select` or `checkbox`; select columns need `options`. Rows are arrays in the same order as columns. Copy/paste, selection and bulk clear are automatic.

### 2. Date / time picker — configuration

```html
<!-- Full date, no time -->
<div class="picker" data-mode="date" data-time="false">
  <button type="button" class="picker__trigger" aria-expanded="false"
          aria-haspopup="dialog" aria-label="Pick a date">
    <svg><use href="#i-calendar"/></svg>
    <span class="picker__value is-placeholder">Pick a date…</span>
  </button>
</div>

<!-- Full date + hour/minute/second -->
<div class="picker" data-mode="date" data-time="true" data-seconds="true">…</div>

<!-- Month + year, or year only (time is ignored) -->
<div class="picker" data-mode="month" data-time="false">…</div>
<div class="picker" data-mode="year" data-time="false">…</div>
```

`data-mode` = `date|month|year`. `data-time="true"` adds hour/minute; `data-seconds="true"` adds seconds. `initPicker()` generates the panel, navigation and Clear/Today/Apply buttons — author only the trigger.

### 3. Toasts — options

```js
showToast('success', 'Saved', 'Your changes have been saved.');
showToast('info', 'Update available', 'Restart to apply.', { dismissible: true });
showToast('error', 'Upload failed', 'Try again.', {
  persistent: true,          // no auto-dismiss; must be closed manually
  dismissible: true,         // show the × button
  action: 'Undo'             // shows an inline action button
});
```

Types: `success|info|error`. Target is `#toastStack`; auto-dismiss is 3.6s unless `persistent`. Replace the demo's action handler with your own undo logic.

### 4. Overlays — open, close, blocking

```js
// Normal modal / drawer:
openOverlay(myOverlay, focusElement);   // focusElement is optional
closeOverlay(myOverlay);                // restores focus to the trigger

// Blocking loading modal: add .overlay--blocking to the overlay element.
// Backdrop clicks and Escape are ignored; only your code closes it.
openOverlay(loadingOverlay, loadingOverlay.querySelector('.loading-modal'));
// …when the work finishes:
closeOverlay(loadingOverlay);
```

`openOverlay`/`closeOverlay` are shared by every `.overlay`: they lock scroll (until the last one closes), trap Tab inside, restore focus per dialog, and honour `.overlay--blocking`. Don't implement your own focus trap.
