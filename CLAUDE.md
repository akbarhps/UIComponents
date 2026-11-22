# ui-design

The web component library in `web/index.html` is the source of truth. `wpf/` (and `winform/`) are ports that must match it pixel for pixel.

WPF parity work is coordinated through `plan/wpf/`:
- **Planner/reviewer session:** read `plan/wpf/REVIEWER.md` first. It has the playbook, the status board, decisions and gotchas.
- **Driving the worker sessions (OpenCode Desktop):** `docs/OPENCODE.md` and `docs/oc.ps1`. General, non-task docs live in `docs/`.
- **Worker session:** read `plan/wpf/README.md` (mandatory rules: hidden-desktop testing via `wpf/tests/run-hidden.ps1`, `build.bat <agent-name>`, never commit, never kill processes by name). Then read the task file you were given (`plan/wpf/NN-*.md`).

## Planner/reviewer session: keep main context small (enforced rules)
- `oc.ps1 last <name>` prints only the last 40 lines; use `-Full` only when the header says a blocking question or failure needs detail. Pipe `wait`, `status`, `ctx` through `tail -3` / `head`.
- Never `Read` a file over ~150 lines in the main session, and never `cat` handoff/report/review files. Send a `reader` (lookup/summary) or a `reviewer`/`phase-reviewer` (point it at the file) and take its 5-line answer. Exception: `plan/wpf/handoff/SESSION-*.md` at session start.
- Send prompts to workers as 2-3 line files that point at `plan/wpf/handoff/worker-common.md` plus the worker's `*-next*.md`; do not re-explain boilerplate or heredoc long text.
