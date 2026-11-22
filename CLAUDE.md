# ui-design

The web component library in `web/index.html` is the source of truth. `wpf/` is a port that must match it pixel for pixel.

WPF parity work is coordinated through `plan/wpf/`:
- **Planner/reviewer session:** read `plan/wpf/REVIEWER.md` first. It has the playbook, the status board, decisions and gotchas.
- **Implementation:** the `implementer` agent runs a task file. It reads `plan/wpf/README.md` (mandatory rules: hidden-desktop testing via `wpf/tests/run-hidden.ps1`, `build.bat implementer`, never commit, never kill processes by name), then the task file (`plan/wpf/NN-*.md`).

## Planner/reviewer session: keep main context small
- Never `Read` a file over ~150 lines in the main session, and never `cat` handoff/report/review files. Send a `reader` (lookup/summary) or a `reviewer`/`phase-reviewer` (point it at the file) and take its 5-line answer. Exception: `plan/wpf/handoff/SESSION-*.md` at session start.

When you add or change a WPF component, also update `wpf/COMPONENTS.md` (index of components, style keys, files). Same for `web/COMPONENTS.md` on web changes.
