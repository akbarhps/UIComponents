Toasts round 2: fixes from the final review. Read plan/wpf/handoff/toasts-final-review.md and do ALL items:
1. Tests.Toasts.cs ~228-233 pause/resume tests can never fail: while focused wait past original expiry (remaining+600) and assert toast still present; then move focus away, assert still present at remaining-400 and gone by remaining+800. Verify they would fail with Pause() removed.
2. SheetGrid.cs ~602-620: Ctrl+C must match web (index.html ~3027, 3057, 3081): one cell = no custom copy, no toast; 2+ cells toast "Cells copied. Paste into Excel." (no count). Only the Copy button says "N cells copied.". Delete the one-cell block, pass toast text into CopySelected.
3. Add checks for the F1-F3 toasts (Copied, Upload complete, Rows deleted, Export started) or drop the header claim; prefer checks.
4. DataControls.xaml.cs ~140-148: after Delete uncheck select-all (web does).
5. Focus-restore test: also assert the first toast is gone.
Build + `-Only toasts` only (wpf\build.bat toasts; cd wpf\tests && .\build-tests.bat toasts toasts; run-hidden.ps1 -Agent toasts -Only toasts). If item 2/4 changes affect grid/tables tests, run -Only grid,tables too. Never commit. Report in README format.
