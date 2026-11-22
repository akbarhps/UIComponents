# ui-design

A UI component library: a web version and a WPF port.

| Dir | What | Role |
|---|---|---|
| `web/` | `index.html` (single file) + `COMPONENTS.md` | **Source of truth.** Every other port must match it. |
| `wpf/` | .NET Framework 4.8 WPF, C# 5 | Port. Must match `web/` pixel for pixel (±1px, exact colours; glyph rasterisation may differ). |
| `plan/wpf/` | Task files, playbooks, handoff notes, reference captures | How the WPF port is planned and reviewed. |

## If you are an agent

Read `CLAUDE.md` first (project rules), then the file for your role:

- **Planner / reviewer:** `plan/wpf/REVIEWER.md` (playbook, status board). Latest state: newest `plan/wpf/handoff/SESSION-*.md`.
- **Implementer:** `plan/wpf/README.md` (mandatory rules), then the task file you were given (`plan/wpf/NN-*.md`), one phase at a time.
- **Reader / lookup:** `web/COMPONENTS.md` for the component list; `web/index.html` for CSS, markup and JS.

Ground rules (details in `plan/wpf/README.md`):
- The web wins. If a plan disagrees with `web/index.html`, follow the web and report it.
- Build: `wpf\build.bat <agent-name>`. Tests: `wpf/tests/run-hidden.ps1 -Agent <agent-name> [-Only area,...]` (hidden desktop, never your real one).
- Never commit unless the user asks. Never kill processes by name.
- Write full reports to `plan/wpf/handoff/<name>-report.md`; reply in chat with a header of at most 12 lines including the verbatim `RESULT passed=N failed=M`.
- Do not read large report/review files in the main session; send a `reader`.

## Using this library from another repo

1. Clone this repo to `ui-design/` in the hub root.
2. Symlink or copy `skills/ui-components` to the hub's `.claude/skills/ui-components`.
3. Fill in the scope table in that `SKILL.md` (replace the `<...>` placeholders).
4. Projects marked NO need a line in their `CLAUDE.md` saying not to use ui-design.
5. Agents then use the skill's small indexes (`COMPONENTS.md` Quick picker, `wpf-index.md`) and never read `index.html` or `MainWindow.xaml` whole.

## Status

All planned WPF groups (01-08: grid, forms, tables, dropdown family, display, navigation, overlays/pickers, split tests) are ported and pass the full suite (534 checks). Unplanned: anything not covered by `plan/wpf/01-08`.
