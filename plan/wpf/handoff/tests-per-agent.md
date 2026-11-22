superseded by plan/wpf/README.md Writing tests and handoff/tests-split.md

Tooling change from the reviewer (read after your current turn; it changes how you build and run tests, nothing else):

The shared test binary `wpf\tests\bin\UIAutomationTests.exe` is gone from your workflow: one agent rebuilding it while another runs it caused the "another agent holds the test binary" waits. Each agent now has its own copy:
- Build: `cd wpf\tests && .\build-tests.bat <your-agent-name> && cd ..\..` (writes `wpf\tests\bin\<name>\UIAutomationTests.exe`).
- Run: unchanged, `pwsh -NoProfile -File wpf/tests/run-hidden.ps1 -Agent <name>`; it now runs YOUR copy. If the copy is missing it falls back to the shared one (avoid that: always run build-tests.bat with your name). If your copy is older than `UIAutomationTests.cs`, it prints a WARNING: rebuild it.
- Rebuild your test exe whenever you or another agent changed `UIAutomationTests.cs` (it is a shared file: edit only your own assertions).
- `plan/wpf/README.md` and `REVIEWER.md` are updated. Everything else in the rules is unchanged (never commit, never kill by name, build.bat <name>, C# 5).
Do not reply to this message; continue your current task and use the new commands from now on.
