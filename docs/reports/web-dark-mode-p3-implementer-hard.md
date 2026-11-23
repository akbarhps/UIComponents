# web-dark-mode Phase 3 report (implementer-hard)

File changed: `web/build-dist.mjs` only (about 50 lines added inside the `--check` branch, before `if (failed) process.exit(1)`). `web/dist/*` rebuilt output is unchanged (Phase 3 touches no CSS/JS output). Nothing committed.

## What was added
1. Literal lint: comments stripped from `css`, first `:root{...}` block removed, declaration values collected with `/:\s*([^;{}]+)(?=[;}])/g`, flagged by the plan's regex. Prints `HARD-CODED COLOURS outside :root tokens: ...`.
2. Contrast: parses `--name: light-dark(A, B)` hex (3 or 6 digit) tokens from the `:root` block. WCAG 2.x `luminance` and `ratio`. Checks the 17 pairs of plan 1.6 in both themes (4.5, and 3.0 for focus/surface). Missing token = failure. Prints `LOW CONTRAST <theme> <fg>/<bg> <ratio>`.
3. OK line extended: `, colours tokenized, N contrast pairs pass`.

## Verify results
- `node web/build-dist.mjs && node web/build-dist.mjs --check` -> `OK: dist is up to date; ... (285 CSS selectors), colours tokenized, 34 contrast pairs pass`, exit 0.
- Negative test 1 (dark `--ink-3` set to `#5a5449`): exit 1, `LOW CONTRAST dark ink-3/surface 2.12 (min 4.5)` (plus ink-3/surface-2 1.98 and ink-3/bg-deep 2.45).
- Negative test 2 (`.x{color:#123456}` added to CSS): exit 1, `HARD-CODED COLOURS outside :root tokens: #123456`.
- Both reverted from a backup copy, rebuilt, `cmp` confirms `web/index.html` identical to pre-test; final `--check` exit 0.

## Notes / deviations
- "N" counts pair x theme checks: 17 pairs x 2 themes = 34. The plan did not define whether N is pairs or pair-theme checks.
- No false positives from the lint on the current CSS, so no allow-list was needed.
- `git status` additionally shows pre-existing changes from Phases 1-2 (index.html, dist, docs/); not touched here.
- No deviations from the plan otherwise.

RESULT passed=34 failed=0
