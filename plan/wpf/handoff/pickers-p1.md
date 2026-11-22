Standing rules (agent "pickers", build.bat pickers, `cd wpf\tests && .\build-tests.bat pickers` (rebuild whenever UIAutomationTests.cs changed), run-hidden.ps1 -Agent pickers only (up to 4 suites run at once machine-wide; `waiting for a free test slot...` = queued, not stuck; the 600s timeout starts when your run starts), never commit, never kill by name, never SendKeys/SendInput, clipboard untouched, re-read shared files before every edit, C# 5, no BOM; if another agent's half-edit breaks the build wait ~60s and rebuild, after two tries keep working on your own files). NEVER end your turn waiting for a background run: run the suite in the foreground and wait in the same turn.
Speed rule (README "Test speed rule"): a suite is ~355 s and UIA Find is ~45% of it. In your tests find the section container ONCE, search inside it, cache reused elements, use a poll-until helper instead of fixed sleeps.

VERDICT on P0: approved, good audit. Updates to the plan rows (web wins):
- P12: Tab/focus leaving the picker root closes the panel, no focus restore (W5 fixed). Panel stays open while focus is anywhere inside the picker root.
- Time inputs take focus by mouse; buttons and day cells do not steal focus on mousedown. Next/Previous/day click do not close. A date-only, month-only or year-only pick closes the panel; a date+time pick only re-renders, Apply commits.
- W7: panel is role=dialog with a label; day cell UIA Name = full date ("November 14, 2026"); selected = toggle/selected state, today = marker (aria-pressed / aria-current). Set AutomationProperties accordingly.
- W4: IconCalendar for all date-only triggers, IconClock only for pickers with time.
- FI2/FI3/FI4 (for later phases): empty/no-accepted additions are a no-op; multiple mode de-dupes by (name,size); drop uses the same image/* predicate as the browse dialog; remove button Name "Remove <name>".
- W6 is NOT fixed in the web: P8 (Clear keeps time) and P11 (uncommitted selection and view month survive reopen) stay as written in the plan.
- Outside click: use the lifted helper `new DropdownOutsideClick(control, Func<DependencyObject> panel, Action close)` with `.Hook()/.Unhook()` and `DropdownTree.IsWithin` from DropdownPrimitives.cs (sanctioned, do not copy it).
- Icons (IconButton, IconChevron, IconCalendar, IconClock) come from Themes/Overlays.xaml (overlays O1). Append your Pickers.xaml as the LAST merged dictionary in App.xaml.

Now do Phase P1 of plan/wpf/07-overlays-pickers.md: CalendarPicker shell + date mode ("Date picker" section with PickDate only; P1-P5, P7-P10). Stop after P1.
Done criteria: build.bat pickers OK; run-hidden.ps1 -Agent pickers passes for your own assertions (counts + wall time; quote failures with whose area they are in); captures per the plan with measured numbers vs web-pickers-date*.png and the measurements file (trigger 450x46, panel 300 wide, offsets).
Report: files changed, build/test summary, capture paths + measurements, deviations, blocking questions.
