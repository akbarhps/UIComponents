# Polish ledger (non-blocking items found in final reviews; do after the 07 group, before or with the commit step)

## From the display final review (approved 2026-11-22; display archived)
Display-owned nits (do in one short round with `unarchive display`, then archive again; no phase review needed):
- UIAutomationTests.cs ~617-620: `Find(root,"EmptyState") ?? FindByName(root,"No projects yet")` duplicates the by-id check at ~383 and the fallback is dead: delete 617-618, keep the EmptyAction check.
- Shimmer.cs ~81-89: BeginAnimation on the shared transform runs once per element (~23x); an element loaded later would restart every band: start it once (static bool).
- MainWindow.xaml ~665: `AutomationId="LoadingCue"` is on a Border (no UIA peer), does nothing: drop it, keep x:Name.
- MainWindow.xaml ~621, 638: `<TextBlock Width="9"/>` spacers create empty UIA Text nodes: use `Margin="9,0,0,0"` on the title instead.
- Display.xaml ~19-21: "See each style's note" but only DisplayCardTitle has one: trim the header comment.
- UIAutomationTests.cs ~457-463: restart checks read the value straight after Invoke; if the first 350ms tick lands first on a loaded machine the test fails without a real bug: read the text before the value (or accept).

Owned by others / shared (route; each needs a before/after measurement, they can shift layout):
- DemoCard bottom border smears across 2 rows where the demo height is fractional (237.6, 213.4, 193.8, 232.9): `UseLayoutRounding` on DemoCard (Themes/Sheet.xaml) or the content root. WARNING: tables saw UseLayoutRounding shift row heights by -2px; re-measure ALL section captures after. Owner: reviewer (shared tooling) or a polish worker.
- Shared `DemoLabel` (Forms.xaml, BlockLineHeight 20.8) puts the skeleton's "Table rows" ink 2px low (253-262 vs web 251-260): owner `forms` (unarchive for a follow-up).
- Full-size `PrimaryButton`/`SecondaryButton` (Controls.xaml): "Start upload" right edge 142 vs 144, Reset starts 156 vs 157: belongs to the buttons task (group 06 navigation).
- Forms `SearchIcon`/`ErrorIcon` hard-code their colour, duplicating `IconSearch` and the alert icons: non-blocking.
- `BadgeBrand` and `BadgeNeutral` (Controls.xaml ~140, 143) are unreferenced: delete at commit time, keep BadgeBase/BadgeSuccess/BadgeText (StatusBadgeTemplate uses them).
- README assertion count (183): verify against the commit-time run output.

## Wall time (decided)
The shimmer is not the cause of the longer suite (inert run 263.8s vs normal 233-280s; assertions grew 138 -> 183). No no-motion test flag. Wall time is not a gate while several agents run suites; the 2-slot limiter (wpf/tests/test-slot.ps1) keeps runs near their quiet time.

## From the web fixes review (approved 2026-11-22; web archived; 31/31 CDP checks)
Fixed by the reviewer inline: stray quote in the month-cell markup (`aria-label="May"">`, index.html ~2605).
Open nits (web, low priority; do in one web follow-up if wanted):
- W13 (new): `render()` still drops focus to body when a day cell had focus (e.g. after Enter/Space on a day in a date+time picker); Escape is bound on `root`, so it then stops closing the picker until a click outside. Pre-existing, not a regression. Optional fix: after `render()` in the cell handler, refocus the equivalent cell (`[data-day=N]` / month / year).
- Extra `render()` before `commit()` for all picker modes (index.html ~2638-2639) is harmless but its stated reason was wrong (the trigger click already renders on every open); revert or leave.
- web-checks.mjs ~499/508: the hour check passes with value "097" (typing 7 appends to "09"): select the contents first and assert '7'.
- web-checks.mjs W10: the `dismissed` guard in `schedule` is redundant (dismiss() has its own `if(dismissed) return`) and no check covers it: drop it or add a case.
- Still deliberately NOT fixed: W3 (drawer Delete 42px), W6 (picker uncommitted selection / Clear keeps time / view month not reset), W12 (palette "Invite a member").

Picker behaviour for the WPF `pickers` worker (already true in the web; tell it with the P0 verdict): time inputs take focus by mouse while buttons and cells do not steal focus on mousedown; the panel stays open while focus is anywhere inside the picker root and closes on Tab/Shift+Tab out, outside click and Escape; Next/Previous/day click do not close it; a date-only, month-only or year-only pick closes the panel, a date+time pick only re-renders and Apply commits.

## From tables final rounds (approved 2026-11-22; tables archived)
- Faint last-line row remains (fractional last-cell border; clip +1/+2 and SnapsToDevicePixels don't fix it): column-filters 726 faint vs web 726+727 crisp; selection 496 faint (web crisp 497), selection-selected 566 (web 567); filter table last two lines 1px above web (606/607 vs 607/608). Same family as the DemoCard fractional-bottom smear: fix together with UseLayoutRounding + re-measure all sections.
- Optional Owner placeholder right inset (TableFilterInput) not done.
- Overlays: IconTable and IconPanel in Themes/Overlays.xaml draw their rectangle x 1..23 instead of 3..21 (4 units too wide), from the nav planner.

## Applies after task 08 (tests split) is done
Read handoff/tests-split.md first (tooling change).
- display: the EmptyState duplicate and the progress-restart read are in `wpf/tests/Tests.Display.cs` (find them by name; line numbers above are pre-split). Expected delta -1 check when the duplicate goes.
- grid: driver-review #7 add-row / add-column reads (`grid add row`, `grid add column`) need `WaitFor` in `wpf/tests/Tests.Grid.cs`.
