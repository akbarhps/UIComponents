# WPF dark mode, Phase 0 (baseline), implementer-mechanical report

Plan: `plan/wpf/09-dark-mode.md`, Phase 0 only. Agent name `theme`. No source edits. Not committed.

## Files changed
- `plan/wpf/theme-captures.ps1` (new): per-section capture script (C5). Runs one `run-hidden.ps1 -NoTests -Section <title> -Capture Sections=<png>` child process per title, decodes XML entities in titles, dedupes, throws on slug collisions, and omits `-Theme` until run-hidden declares it. Prints `titles=N` and `captured N, failed M`.
- `plan/wpf/ref/web-dark-buttons.png` (new): web Buttons section in dark, via `capture-web.ps1 -Js "document.documentElement.dataset.theme='dark'"`. Viewed: dark background confirmed.
- Build outputs only (ignored by git): `wpf/bin/theme/`, `wpf/tests/bin/theme/`.

`git status --short` now shows only: ` M plan/wpf/REVIEWER.md` (pre-existing), `?? plan/wpf/09-dark-mode.md` (pre-existing), `?? plan/wpf/theme-captures.ps1`, `?? plan/wpf/ref/web-dark-buttons.png`.

## Checks run
1. Build: `cmd /c "wpf\build.bat theme"` gave `Built bin\theme\UIComponents.Wpf.exe`. `build-tests.bat theme` (full, no area) gave `Built bin\theme\UIAutomationTests.exe`. The suite log printed `AREAS compiled: nav, forms, tables, display, overlays, dropdown, pickers, grid, toasts`, so the full build is confirmed.
2. Base capture, `theme-captures.ps1 -Agent theme -Out %TEMP%\theme\base`: `titles=36`, `captured 36, failed 0`.
3. Base2 capture, `...\base2`: `titles=36`, `captured 36, failed 0`.
4. Byte gate `Same base base2`: `diffs=0`. Both directories have 36 PNGs. Unstable set: empty.
5. Silent-fallback check: 36 distinct SHA-256 hashes across the 36 PNGs, so no title fell back to the full page. Every capture is 984x1029 (min and max both 984 wide and 1029 tall).
   - Clipping check: the bottom pixel row of all 36 PNGs matches the background colour `FFF5F4EF` (36 of 36 clean). Viewed `Forms.png` (the tallest-content section I opened): the whole form and the "Selection controls" card are visible, with the card's bottom edge above y=1029. `Typed-grid-input.png` is also complete. Conclusion: captures are full-height for the sections checked. Not every section was viewed.
6. Masthead check: `%TEMP%\theme\base\Buttons.png` (the section right after the masthead) shows no masthead. Pass.
7. Full suite: `run-hidden.ps1 -Agent theme`. Log copied to `%TEMP%\theme\base.log`.
   - Verbatim log line: `RESULT  passed=533  failed=1` (`PROFILE finds=689`, `DRIVER fast=281 fallback=4`).
   - The one failure: `FAIL  resume: still present before remaining expires - remaining=1287 sinceLeft=1422`. It is `wpf/tests/Tests.Toasts.cs:242`, inside the focus-pause block (lines 211-245), in the toasts area.
   - DRIVER baseline lines, verbatim: `DRIVER  fallback select:<no id> x2` and `DRIVER  fallback toggle:RowCheck x2`.
   - Baseline result recorded as is. Not fixed (not in Phase 0 scope).
8. Web reference: `capture-web.ps1 -Sections buttons ... -Js "document.documentElement.dataset.theme='dark'"` wrote `plan/wpf/ref/web-dark-buttons.png`. `id="buttons"` exists at `web/index.html:1015`. The sidebar is kept, which is the script's design.

## Deviations from the plan
- None to the plan's steps. Implementation choices the plan left open:
  - Run-hidden `-Theme` detection uses `(Get-Command $runHidden).Parameters.ContainsKey('Theme')`. The script throws if `-Theme` is non-light and the param is absent.
  - `-Out` relative paths resolve against the repo root, as run-hidden's `Get-OutPath` does.
  - Each run deletes only its own target PNG before capturing.
  - Failure is counted when the exit code is nonzero, the output has `CAPTURE FAIL`, or the PNG is missing.
- Known-failure match: `plan/wpf/handoff/tests-split.md` lists toasts `auto-dismiss resumes when focus leaves` as failing in every full run. That check is at `Tests.Toasts.cs:244`, one line below the one that failed here (`:242`, same test block). It passed in this run. So the failure is in the same test but is not the listed check. Treat it as a baseline failure and have the toasts owner confirm.

## Blocking questions
None.

Full report: docs/reports/wpf-dark-p0-implementer-mechanical.md
