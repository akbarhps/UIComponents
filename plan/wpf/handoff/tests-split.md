# Common note for every worker (task 08, tests split)

Prepend this to your next prompt, together with the per-worker note at the end of your fix-list file. Read it before your next phase.

```
Tooling change (task 08, tests split), read before your next phase:
- wpf/tests/UIAutomationTests.cs no longer exists. Your assertions are in wpf/tests/Tests.<Area>.cs (yours: see your prompt). Edit only that file. Never edit Tests.Core.cs: ask for a helper in your report. Never reload or save an older copy of UIAutomationTests.cs.
- Build, partial (your area only): cd wpf\tests && .\build-tests.bat <name> <area> && cd ..\..
- Run, your area only: pwsh -NoProfile -File wpf/tests/run-hidden.ps1 -Agent <name> -Only <area>
  Always pass -Only. Your area must pass alone. An area run takes 3-40 s.
- Full build and full run are the reviewer's. Do not run them.
- A partial run prints "FAIL  area <x> not compiled into this exe" for the other areas. That is expected.
- The run-hidden stale-build warning can appear when another worker saves a test file. Ignore it only right after you rebuilt your partial exe this phase. No `AREAS compiled:` line in the output means the exe is pre-split: rebuild with `build-tests.bat <name> <area>`.
- Known failures, not yours: toasts "auto-dismiss resumes when focus leaves" (fails in every full run; toasts owner). pickers "P10 outside click closes" fails with -Only pickers alone (pickers owner; fix is in pickers-next.md).
- Pickers now live in wpf\PickerControls.xaml(.cs), not InputControls. InputControls keeps the dropdown sections.
- MainWindow.xaml.cs is split: progress members are in MainWindow.Display.cs, the textarea grip handler is in MainWindow.Forms.cs. Re-read before editing, never save an older copy of `MainWindow.xaml.cs`, `InputControls.xaml(.cs)` or `wpf\UIComponents.Wpf.csproj`. A stale `.xaml`/`.cs` pair compiles and silently duplicates the File input and Date picker sections and their AutomationIds (driver fallbacks, wrong Find hits); a stale csproj drops the new files from the build. ApplyStandaloneMode finds host panels by SectionList, so a new host needs no edit there.
- csproj: new .xaml and .cs files in wpf\ and wpf\Themes\ compile automatically. Do not add csproj entries. Never leave scratch files there.
- If build.bat, or build-tests.bat <name> <area>, fails in a file you don't own (including another worker's Tests.<Area>.cs): wait a minute, rebuild, then report it. Never edit that file.
- Keep the // Covered: header at the top of your Tests.<Area>.cs current.
- A "UIAutomationTests.cs:NNN" reference in a task file or review means: find the assertion by name in your Tests.<Area>.cs.
- Test-writing rules: plan/wpf/README.md "Writing tests".
```
