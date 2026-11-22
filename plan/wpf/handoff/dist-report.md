# Web dist + snippets report

Goal: consumers link generated `web/dist/ui.css` and `web/dist/ui.js` and take per-component markup from `web/SNIPPETS.md`, instead of copying CSS/JS by hand. `web/index.html` stays the source of truth. Nothing committed.

## Files
- `web/build-dist.mjs` (new): node, no deps, ESM. Writes `dist/ui.css` (all `<style>` blocks) and `dist/ui.js` (all inline `<script>` blocks, document order, header `GENERATED from web/index.html - run node web/build-dist.mjs`). `--check` regenerates in memory, reports STALE, and reports snippet classes that are neither CSS selectors nor JS hooks.
- `web/dist/ui.css` (57,262 bytes), `web/dist/ui.js` (69,283 bytes), generated, not minified. LF endings.
- `web/SNIPPETS.md` (new, 27,442 bytes): 30 `### <name>` sections, one per Quick picker row, names identical to the picker. Each has `when:` (picker text), one html block (max 19 lines, section max 25 lines), `init:` line. No CSS or JS.
- `web/index.html`: guards only (below). No restructuring.
- `web/COMPONENTS.md`: +2 lines (pointer to dist/ and SNIPPETS.md).
- `skills/ui-components/SKILL.md`: Web lookup replaced by the 4-step protocol (45 lines). The file already had uncommitted changes before this task; only the Web block was touched.
- `README.md`: "Using this library from another repo" steps 5-6 updated (dist committed, never copy).
- `CLAUDE.md`: one rule (after editing index.html run `node web/build-dist.mjs`, commit `web/dist`, keep SNIPPETS.md in sync via `--check`).

## index.html guards (exact)
Each guard only skips a block whose root element is absent. In index.html every element exists, so behaviour is unchanged.
1. `#overlay`: `if(overlay){` around openModal / closeModal / cancelModal / confirmModal listeners.
2. `#loadingOverlay`: `if(loadingOverlay){` around openLoading listener.
3. `#formOverlay`: `if(formOverlay){` around openFormModal / closeFormModal / cancelFormModal / projectForm submit / fm-name+fm-email input loop. Also `document.getElementById(id)?.addEventListener` in that loop (the snippet has no fm-email field).
4. `#drawerOverlay`: `if(drawerOverlay){` around the invite drawer listeners.
5. `#editDrawerOverlay`: `if(editOverlay){` around the edit drawer listeners.
6. `#filterTable`: `if(document.getElementById('filterTable'))` on the bindSimpleFilters call.
7. `#sortTable`: `if(sortTable){` around sort, stickyToggle; `if(sortTable)` on bindColumnFilters('sortTable').
8. `#columnTable`: `if(document.getElementById('columnTable'))` on the bindColumnFilters call.
9. `#cmdkOverlay`: `if(cmdkOverlay){` around the command palette block through its Ctrl+K listener.
10. `#progressFill`: `if(progressFill){` around the progress block; `?.` on progressStart and progressReset listeners.
11. `#stepperDemo`: `if(stepperDemo){` around the stepper block.
12. `#selectionTable`: `if(selectionTable){` around the row selection block.
13. `#toastDismissible`: `if(document.getElementById('toastDismissible')){` around the three toast-variant listeners.
14. `#loadingBtn`: `if(loadingBtn){` around the loading button demo.
15. `#sheet` / `#typedSheet`: `if(document.getElementById(...)) createGrid(...)`.
16. `#sidebarToggle` / `#brandToggle`: `if(sidebarToggle)` / `if(brandToggle)` on their click listeners.
Not guarded (no load-time throw): showToast (needs `#toastStack`), `#menuResult` (only read on menu action), `_resetPage` (only on filter input).
Style: several wrapped blocks were not re-indented, to keep the diff small.

## Check results
- web-checks before any change: `RESULT: 43/43 checks passed, 0 failed`, console errors on load 0.
- web-checks after the guards (final run): `RESULT: 43/43 checks passed, 0 failed`.
- `node web/build-dist.mjs --check`: `OK: dist is up to date; all SNIPPETS.md classes exist in ui.css or are JS hooks in ui.js (285 CSS selectors)`, exit 0.
- Consumer page (throwaway, scratchpad, minimal markup for 5 components, `dist/ui.css` in head, `dist/ui.js` at end of body): before guards `Uncaught TypeError: Cannot read properties of null (reading 'addEventListener')` at the modal block, which also aborted the rest of ui.js. After guards: `ERRORS: none`.
- Each of the 30 snippets loaded alone: `ERRORS: none` for 30/30. All 30 together on one page: `ERRORS: none`. Snippet gaps found this way and fixed in the snippets: multi-select lacked `multi__select-all`, row selection lacked `#bulkbar`, stepper lacked its controls. Index.html guard added for the progress start/reset listeners (item 10).
- Verbatim check (throwaway): 293 snippet code lines, 0 not found in index.html after whitespace normalisation. Trimmed content is only whole rows, options and optional buttons.
- Browser: headless Edge (msedge.exe), the same launcher `plan/wpf/web-checks.mjs` uses (the brief said Chrome; Edge is what that script launches). No leftover browser processes.

## Deviation from the spec (please confirm)
`--check` accepts a class if it is a CSS selector OR a JS hook referenced in a quoted string in ui.js. Strict CSS-only checking flagged six classes that the snippets need for behaviour and that have no CSS rule: `multi__clear`, `multi__done`, `tag-input-wrap`, `ac__input`, `overlay--blocking`, `row-check`. Removing them would break those components. To revert to strict mode, delete the `hooks` lookup in `build-dist.mjs`; the check will then fail on those six.

## Gaps and placeholders
1. Icon sprite: every `<use href="#i-…">` needs the `<symbol>` sprite, which is markup and not in dist. Consumers must copy index.html lines 889-917 once (`#i-plus` is at 3582). Adding it to dist would need a third output file, which the spec did not ask for.
2. No reusable factory (ID-wired demo code). Sortable table (`#sortTable`, `#stickyToggle`), command palette (`#cmdkOverlay`, `#cmdkInput`, `#cmdkList`), row selection (`#selectionTable`, `#selectAll`, `#bulkbar`; bulk Export/Delete run demo toasts from ui.js). Stepper works through its ids (`#stepperDemo`, `#stepPrev`, `#stepNext`, `#stepStatus`), which the snippet keeps. Progress bar: set width and aria manually.
3. Auto-init runs once at load for class hooks: `.combo`, `.multi`, `.tag-input-wrap`, `.ac`, `.file-input`, `.menu`, `.accordion`, `.tab`, `.picker`, `.demo > .table-wrap`. Elements added later are not initialised. Factories that exist: `createGrid`, `initPicker`, `bindSimpleFilters`, `bindColumnFilters`, `showToast`, `openOverlay` / `closeOverlay`, `setFieldError`.
4. Table filters: `bindSimpleFilters` and `bindColumnFilters` need the table inside `.demo` with the Data table pagination block (they call `_resetPage`). Without it, typing throws. Snippets say so; the pagination block is not repeated in each snippet.
5. Toast: `showToast` needs `<div id="toastStack">` (in the Toast snippet).
6. Modal / drawer / loading / modal form: open and close go through the global `openOverlay` / `closeOverlay`. Open and submit wiring is ID-based (ids from the snippets). The modal-form snippet has no email field, so its submit handler reads a missing field if you submit; add the email field from index.html 2044-2049 if you use submit.
7. Some snippets are condensed (multi-select footer, column-filter status select) by whitespace only, with trimmed options and rows.
8. Not committed. `web/dist/` must be committed (README and CLAUDE.md say so).
