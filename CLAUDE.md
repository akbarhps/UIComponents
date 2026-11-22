# ui-design

The web component library in `web/index.html` is the source of truth. `wpf/` (and `winform/`) are ports that must match it pixel for pixel.

WPF parity work is coordinated through `plan/wpf/`:
- **Planner/reviewer session:** read `plan/wpf/REVIEWER.md` first. It has the playbook, the status board, decisions and gotchas.
- **Driving the worker sessions (OpenCode Desktop):** `docs/OPENCODE.md` and `docs/oc.ps1`. General, non-task docs live in `docs/`.
- **Worker session:** read `plan/wpf/README.md` (mandatory rules: hidden-desktop testing via `wpf/tests/run-hidden.ps1`, `build.bat <agent-name>`, never commit, never kill processes by name). Then read the task file you were given (`plan/wpf/NN-*.md`).
