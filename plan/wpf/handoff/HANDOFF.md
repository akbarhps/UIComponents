# Handoff: planner/reviewer state (written 2026-11-21 ~21:30, before the 5h limit)

Read `plan/wpf/REVIEWER.md` (playbook, status board, decisions) and `plan/wpf/README.md` first; this file is the delta. Resume with the global `opencode` skill (`~/.claude/skills/opencode/SKILL.md`).
> **Newest state: read `plan/wpf/handoff/SESSION-2026-11-22b.md` first (newest, 09:12), then `SESSION-2026-11-22.md` (older background: worker states, tooling changes, open investigation, next actions). This file is older background.**

## How to resume (first 5 minutes)

```powershell
$oc = "$env:USERPROFILE\.claude\skills\opencode\scripts\oc.ps1"
cd C:\Workspace\Projects\others\ui-design
pwsh -NoProfile -File $oc status      # who is RUNNING / idle (+ pending permission prompts)
pwsh -NoProfile -File $oc ctx         # context size per worker (compact >=50%, new session >=70%)
```
- A worker is only touched when idle. For each IDLE worker: `last <name>` to read its report, review it (spawn the `phase-reviewer` agent (NOT `reviewer`: that is the single final review after all phases) on its diff, with the spec, not your conclusion), write the verdict + next prompt to a file, `send <name> -File ...`, then ALWAYS start a background `wait <name>` (one listener per worker).
- Background listeners from the old session are gone: restart `wait` for every worker that is RUNNING (`wait <name>`), so no report is missed.
- Agent map: `.opencode-agents.json` (git-ignored): forms, grid, web, tables, display, dropdown. Session titles: `ui-design_20261121_<task>`.
- Archived sessions (finished): forms, grid, web. `unarchive <name>` brings one back for a follow-up. Archive a worker as soon as its task is approved.
- Never approve permission prompts, interrupt, or kill by name on your own. Workers never commit. The user commits (or asks you to), after you confirm the tree builds and tests pass.

## Worker state at hand-off

| Worker | Session | Task | State when written | What to do next |
|---|---|---|---|---|
| `tables` | ses_e04098a66ffeTvKHynZrtF4SOi | 04-tables | Phase 3 APPROVED (138/0, bulk bar exact). The tables-next.md prompt (15 defects + stacking-by-measurement + Phase 5) was SENT at ~21:40 and is RUNNING | When idle: read report, review with `phase-reviewer` (sortable frame/scrollbar outside, pin, sort asc/desc and numeric keys, chevron/pin icons, wrapped pitch 22, Team column, column-filters 2 last lines, empty states, ink per stacking style, TableSmallSecondary deleted if display's disabled state is whole-button), then approve Phase 5 (tests/README), archive. Compact at >=50%. |
| `display` | ses_e01bdae47ffeo5NzXatzlN1VRG | 05-display | Phase 1 fixes + Phase 3 APPROVED (138/0). Phase 4 (skeleton + shimmer, plus alert-ink variant test and '0%' edge) SENT at ~21:50 and RUNNING; a reviewer agent was reviewing the Phase 1 fixes + Phase 3 (its findings are NOT sent yet) | When idle: read report; get the reviewer's findings (re-run `phase-reviewer` on the display diff if the earlier one never reported), send them with the Phase 5 prompt (tests, README, final recapture); then archive. Compact at >=50%. |
| `dropdown` | ses_e01b39eeaffe9xUDeERoufKr3F | 03-dropdown | RUNNING Phase 1 (primitives + Searchable select shell, `~Key`/`^text` capture steps in App.xaml.cs, adorner hosting proof) | When idle: read report, review with `phase-reviewer`; append `plan/wpf/handoff/dropdown-next.md` notes (web fixed, autocomplete refs regenerated, `~Tab` crash, stacking-by-measurement) to the next prompt; then Phases 2-6 one at a time (see `plan/wpf/03-dropdown.md`). |
| `grid` | ses_e0238a9d0ffe1NkofzNcUvbWyi | 01-grid-input | DONE (119/0), archived | Nothing. Uncommitted. |
| `forms` | ses_e023107ceffes9dzt57DNkx3dR | 02-forms | DONE, archived | Nothing. Uncommitted. |
| `web` | ses_e01aa2b39ffe7CnHGAVBT2H03f | web bug fixes | DONE, archived, committed `f4c5faa` (22/22 CDP checks) | Nothing. |

## Committed so far
- `bc0d578` docs (plan files, web references, .gitignore)
- `f4c5faa` web fixes + `plan/wpf/web-checks.mjs` (`node plan/wpf/web-checks.mjs` must stay 22/22)
Everything else (about 59 paths: wpf/ source, wpf-*.png captures, tests, README edits) is UNCOMMITTED on purpose: workers share Controls.xaml, App.xaml(.cs), the csproj, MainWindow.xaml, UIAutomationTests.cs and wpf/README.md, so commit only when tables, display and dropdown are idle and approved. Plan: build + run `wpf/tests/run-hidden.ps1 -Agent reviewer` (clipboard is the user's: the harness saves/restores it), then one commit per finished task by owned files (grid: SheetGrid.cs, Themes/Sheet.xaml; forms: Themes/Forms.xaml + Controls.xaml ClaudeCheckBox parts; tables; display: Themes/Display.xaml, Shimmer.cs; dropdown: Themes/Dropdown.xaml etc.), then one commit for the shared files. Ask the user before committing. Also still to do then: delete `docs/OPENCODE.md` and `docs/oc.ps1` (untracked) and repoint `CLAUDE.md` + `plan/wpf/REVIEWER.md` at the `opencode` skill.

## Decisions made this session (not all are in REVIEWER.md)
- Web is the source of truth, and it was bug-fixed (A1-A12, B1 picker icon `>`, C1-C9). WPF ports the FIXED behaviour (e.g. grid typed paste clips per cell, copy only selected cells: done in grid). B1: picker triggers now draw the icon (value text +39px): the future picker port must copy it. C3: autocomplete shows an N-selected hint (card 162 tall).
- Display defines `SecondaryButtonSm`/`PrimaryButtonSm` in `Themes/Display.xaml` (merged last in App.xaml); tables adds only `DangerButtonSm`; tables must reuse display's secondary and delete `TableSmallSecondary` once its disabled state is whole-button (opacity .45 on the control).
- Shared `SectionTitle` is LineHeight 32 + BlockLineHeight (tables changed it) so card tops are whole pixels. `SheetSectionDesc` got TextWrapping=Wrap (reviewer edit in Sheet.xaml).
- Dropdown panels are hosted in an adorner on ContentScroll's layer (not a Popup, which the capture cannot see); fallback zero-height Canvas. Dropdown worker may add `~Key`/`^text` capture steps to App.xaml.cs (reviewer-owned file).
- Web quirks replicated earlier (still true): Budget `.table__num` left-aligned; a selected select cell hides its chevron. Filter/column-filter tables KEEP the last data row's bottom line (two line rows); data and selection tables show one.
- Pinned table cell is Surface (white) with a right line, Surface2 on hover (web wins over the plan).
- Line stacking in WPF (BlockLineHeight vs MaxHeight vs margin variant) is disputed by two reviews: decide per style by measuring ink rows against the web capture (within 1px), not by offscreen models.
- Context policy: `ctx` per worker at each idle point; compact at >=50%, new session at >=70%; new session per task group; archive finished sessions. Poll interval default 5s.
- Subagent rules: use the named agents: `planner`; `phase-reviewer` after each phase; `reviewer` (Opus) only once, for the final review after all phases (all have write tools except `reader`); stop a reviewer agent that hands back with "waiting on its own background work" once its report is in.

## Open items / risks
- Row-checkbox capture can't be driven in `--capture` (selected-row capture uses `!SelectAll`); mouse input cannot reach the hidden desktop: mouse-only behaviours are code-only and need a hand check (popup option click in grid select, drag selection, outside click).
- `--capture` `~Tab` step crashes the hidden-desktop app (tables worker report): dropdown to fix.
- README assertion count drifts as workers add tests (119 at last run): update from the real run output at the end.
- `plan/wpf/README.md` ownership table already lists display and dropdown; tables' ownership of the `SectionTitle` style should be added there.
- The unattended `msedge` processes left over from headless CDP runs (about 30 at 17:11/17:34/20:25) are not workers; close them by hand if memory gets tight.
- Remaining task groups not yet planned (see REVIEWER.md): 06 navigation (buttons, action-menu, accordion, stepper, breadcrumbs, tabs, tooltip), 07 overlays and pickers (modal, drawer, loading, command-palette, toasts, date/datetime pickers, form-modal, file-input). Use the `planner` agent (it can write the plan file), capture web refs, then spawn a worker (`new <name> -Title <task> -Model opencode/hy4-flash-free -Effort high -OcAgent build -File kickoff.md`).
