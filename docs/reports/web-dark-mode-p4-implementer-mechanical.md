# web-dark-mode Phase 4 report (implementer-mechanical)

Scope: consumer docs and snippet, per `docs/plans/web-dark-mode.md` Phase 4. Docs only. No CSS, JS or `web/index.html` change. Nothing committed.

## Files changed
1. `web/SNIPPETS.md`
   - Line 3 (usage line): inserted the plan's `dist/theme.js` sentence after the `dist/ui.js` clause.
   - Appended `### Theme toggle` section (html segment markup, `init: auto (ui.js)`, `setTheme`, head-script note), verbatim from the plan.
2. `web/COMPONENTS.md`
   - Line 9: added `light-dark(light, dark)` sentence and the never-hard-code / `--check` sentence.
   - Line 11: added `setTheme` to the exported function list.
   - Quick picker: appended row `| Let the user pick light/dark/system | Theme toggle | A custom switch |`.
   - New `### Theme — light / dark` subsection after Sidebar, before `## Inputs`: use, wire (data-theme, `ui-theme` key, theme.js in head, setTheme), colour (color-scheme, `--on-brand`/`--on-ink`, subtree `style="color-scheme:dark"`), and the 19 new tokens from table 1.3 in one line.
3. `skills/ui-components/SKILL.md`
   - Lookup step 3: appended the theme.js `<script src>` sentence and "Both themes come from the same tokens; never write a colour literal."

Generated: `web/dist/*` rebuilt by `node web/build-dist.mjs`. Output is unchanged from Phase 3 (docs are not inputs to dist, only SNIPPETS.md is checked).

## Checks run
- `node web/build-dist.mjs` : exit 0. Wrote ui.css 59244 bytes, ui.js 80697 bytes, theme.js 280 bytes.
- `node web/build-dist.mjs --check` : exit 0.
  `OK: dist is up to date; all SNIPPETS.md classes exist in ui.css or are JS hooks in ui.js (285 CSS selectors), colours tokenized, 34 contrast pairs pass`
- `grep -n "theme.js" web/SNIPPETS.md web/COMPONENTS.md skills/ui-components/SKILL.md` : hits in all three files (SNIPPETS.md 3 and 493, COMPONENTS.md 95, SKILL.md 36).
- Token name check: every token listed in the new COMPONENTS.md line exists as `--name:` in `web/index.html` (script loop, no missing output).

Verify result (gate command `node web/build-dist.mjs --check`, exit 0):
RESULT passed=34 failed=0
(N = the 34 contrast pair x theme checks the gate reports, same convention as the Phase 3 report. The gate is one command.)

## Deviations / judgement calls
- SKILL.md: the plan says "Lookup step 3" for both sentences. Both are in step 3, not in the Rules section.
- Quick picker row: the plan does not say where in the table. Appended at the end.
- SNIPPETS.md usage line: the plan does not say where in the line. Inserted after the `dist/ui.js` clause.
- SNIPPETS.md `### Theme toggle`: the plan's text has no `when:` line, while other sections have one. The build does not require it, so the section follows the plan verbatim. Add a `when:` line if the docs convention matters to the maintainer.
- COMPONENTS.md theme subsection: the plan lists the required topics but not exact wording. Wording is mine, with the plan's topics covered.

## Skipped
- Visual check (section 3): not run. Phase 4 changes no CSS or JS and renders no new UI.
- Commit: not done (per instruction).
