# WPF dark mode, Phase 5 (docs), implementer-mechanical report

Plan: `plan/wpf/09-dark-mode.md`, Phase 5 only. Agent `theme`. Docs only: no source, XAML, test or capture was run. Not committed.

## Files changed (this phase)
- `wpf/COMPONENTS.md`
  - Tokens row: lists the colour keys (same set in both files, incl. `SheetCheckFill`), the per-theme effects, and the light-only keys; points at `Tokens.xaml` and `Tokens.Dark.xaml`.
  - New rows: `Theme` (`ThemeManager` API: `Initialize`, `SetTheme`, `Mode`, `IsDark`, `StorageKey`, `ThemeChanged`, enum `ThemeMode`) and `Theme toggle` (`ThemeSystem`/`ThemeLight`/`ThemeDark`, `MainWindow.SyncThemeToggle`).
  - New section `## Theme (light / dark / system)`: consumer steps (compile `ThemeManager.cs`, copy/link `Tokens.Dark.xaml`, merge `Tokens.xaml` into `Application.Resources`, `Initialize()` first in `OnStartup`, DynamicResource-only colours, `StorageKey`, `UI_THEME`, title bar) and the six accepted dark-mode gaps from Phase 4.
- `plan/wpf/README.md`
  - Ownership row: `wpf/ThemeManager.cs`, `Themes/Tokens.xaml`, `Themes/Tokens.Dark.xaml`, `plan/wpf/theme-*.{ps1,mjs}` to reviewer.
  - Three gotchas: colour = DynamicResource / SetResourceReference (lint C3/C5); captures and tests run light unless `-Theme dark`; in dark, text is the bright pixel.
  - Task table row 09.
- `plan/wpf/REVIEWER.md`
  - Status row 09: DONE, not committed; next = final `reviewer` pass, manual checks, accepted gaps.
  - Decisions: overlay dictionary; rejected in-place mutation and window recreation; persistence (Q1); accepted gaps (Q2/Q3).
  - Command block: `-Theme dark` capture and `node plan/wpf/theme-lint.mjs`.
- `docs/reports/wpf-dark-p5-implementer-mechanical.md` (this file).

Line endings: the three edited docs were CRLF before; they were normalised to CRLF after editing (107/178/198 lines, 0 bare LF).

## Checks
```
node plan/wpf/theme-lint.mjs                     C0..C5 violations=0 / RESULT theme-lint failed=0
grep -c 'ThemeManager|Tokens.Dark' wpf/COMPONENTS.md   9 (both terms present; checked with grep, not Select-String)
git status (phase files)                          M wpf/COMPONENTS.md, M plan/wpf/README.md, M plan/wpf/REVIEWER.md, ?? this report
```
`git status` for other paths still shows the source changes from Phases 0-4 (uncommitted, expected); this phase touched none of them.

## Deviations from the plan
- Status row 09 reads "DONE, not committed" and keeps the final reviewer pass as a pending item in the Next column. The plan says only "DONE"; the pending review is added so the board is not misleading.
- COMPONENTS.md `Theme` section: the plan asked for consumer bullets; the Phase 4 known gaps are also listed there (plan allows COMPONENTS and/or REVIEWER). REVIEWER.md points to that list rather than repeating it in full.
- Consumer note: `ThemeManager.cs` is required (not optional), because `Shimmer.cs` references `ThemeManager.ThemeChanged` and Shimmer is always compiled.
- The `Theme toggle` row and the API list include `StorageKey` (in the plan's C3 API, missing from the plan's row text).

## Not run (docs only)
- Full test suite, builds and captures: not run, per the Phase 5 instructions. The `-Theme dark` command added to REVIEWER.md is documented but not executed here.
- No commit (rule 5).

## Blocking questions
None.
