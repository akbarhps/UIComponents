# Common notes for every worker round (read first)
- Tree changed since you were last active: tests are split per area (`wpf/tests/Tests.<Area>.cs`; `UIAutomationTests.cs` no longer exists), csproj uses wildcards (do NOT add csproj entries), MainWindow is partial, the test run is fast (in-process driver). Old line numbers in your next-file refer to the old test file: find code by name.
- Baseline is committed (3744521, b34f19e, 0121a79). Never commit.
- Read `tests-split.md` fully and `plan/wpf/README.md` "Writing tests" (also step 6: full report goes to `plan/wpf/handoff/<name>-report.md`, chat gets a header of at most 12 lines).
- Build/test only your area: `wpf\build.bat <name>`, `cd wpf\tests && .\build-tests.bat <name> <area>`, `pwsh -NoProfile -File wpf/tests/run-hidden.ps1 -Agent <name> -Only <area>`. Area must pass alone. Full suite is the reviewer's.
- Mutation-check new focus/state assertions (they must be able to fail). Re-capture references you change.
- Never kill processes by name. Edit only your owned files + your Tests file. If the build breaks in someone else's file, wait ~60 s, rebuild, report.
- Other workers run at the same time. nav owns Controls.xaml lines 5-69, Navigation.xaml, MainWindow.Navigation.cs (its button restyle shifts full-size button widths +2px toward the web in your captures).
