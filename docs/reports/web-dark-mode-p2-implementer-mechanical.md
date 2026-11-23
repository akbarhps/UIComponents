# web-dark-mode Phase 2 report (implementer-mechanical)

RESULT passed=6 failed=0

Files changed: web/index.html (CSS only), web/dist/ui.css, web/dist/ui.js (rebuilt, unchanged content vs Phase 1 apart from ui.css), web/dist/theme.js (unchanged). Nothing committed. web/build-dist.mjs untouched.

Changes (all per plan table 1.3 and the spinner rule):
- 19 new tokens added to `:root` after `--focus`, each `light-dark(<light literal>, <dark>)`: on-brand, on-ink, ink-hover, on-danger, danger-hover, brand-line, brand-soft-hover, success-line, info-line, danger-line, sage-soft, sage-line, wash, wash-strong, scrim, shimmer, focus-ring, knob, knob-shadow.
- Replacement counts applied (asserted by script): rgba(255,255,255,.6) x4 to --wash; #ecd4c8 x3; #f0d9cd x2; `0 0 0 3px var(--brand-soft)` x8 to `0 0 0 3px var(--focus-ring)`; #fff text on fills: on-brand x4, on-ink x5, on-danger x1; switch knob bg and knob-shadow; #000 to --ink-hover; #982f27 to --danger-hover; rgba(30,26,20,.42) to --scrim; rgba(255,255,255,.65) to --shimmer; rgba(255,255,255,.75) to --wash-strong; the remaining single-use hex to their tokens.
- `.spinner`: border `color-mix(in srgb, currentColor 45%, transparent)`, border-top-color `currentColor`. Both spinner uses in index.html sit inside `.btn--primary`, so light pixels are unchanged.
- Line numbers in the plan were stale; every site was located by literal content, not line number.

Verify:
- `node web/build-dist.mjs`: wrote ui.css, ui.js, theme.js.
- `node web/build-dist.mjs --check`: `OK: dist is up to date; ... (285 CSS selectors)`, exit 0.
- Literal scan `grep -nE "#[0-9a-fA-F]{3,8}\b|rgba?\(" web/dist/ui.css | grep -v "light-dark("`: no output.
- `light-dark(` lines in ui.css: 43 (24 Phase 1 + 19 new).
- Light-mode equivalence (scratchpad script p2-verify-light3.cjs): resolve light-dark(a,b) to a and var(--token) to its light value on both the pre-Phase-2 backup and the new `<style>`; result `LIGHT-EQUIVALENT: no differences`. The script was checked to be sensitive: it flagged real diffs before the fix.
- Light screenshot (Edge headless, 1440x9000, plan section 3): the Phase 2 light.png is byte-identical to the Phase 1 light.png (531,568 bytes, `cmp` exit 0). Light pixels did not change in Phase 2.
- Dark screenshot viewed: renders dark throughout; primary buttons use dark text on terracotta; no white-on-dark leftovers visible at the overview scale.

Deviations from plan:
- One token line had no space after its colon (`--brand-soft-hover:light-dark(`, because the name exceeds the 17-column pad). Fixed to `--brand-soft-hover: light-dark(`. Cosmetic; no value change. Build re-run after the fix.
- The Edit tool warned that index.html had changed on disk. That was my own scripted write; I did not detect any other edits.

Skipped:
- Manual interactive checklist (plan section 3: modal/drawer scrim, toasts, Ctrl+K palette, focus rings via Tab, native select, scrollbars, reload-no-flash). Needs an interactive browser; not run.
- Project lint/typecheck/test: the repo has no package.json scripts for these; the plan's build + --check gate is the full verify.
- Phase 3 (guards), Phase 4 (docs), and any WPF work: out of scope.
- Commit: not done (per instructions).
