# tables: round 5 (final review of round 4 = fix-first, tests only blocking)
Full review: plan/wpf/handoff/tables-final-review.md (read it). Round 3 items all verified in code. Rules unchanged: agent "tables", build.bat tables, run-hidden.ps1 only, never commit, never kill by name. Minimise UIA actions (README test speed rule).

Blocking
1. wpf/tests/UIAutomationTests.cs:1471 "sortable info text from matched count" can never fail: FindByName(root,"Showing 1-5 of 5") also matches the MainWindow data-table pager default text. Use filter "a" + Budget ascending (4 rows, first row "Legacy archive", pager "Showing 1-4 of 4"), or search inside SortPagination if it has a peer.

Should-fix (pre-existing gaps)
2. DataControls.xaml.cs:100 filter-table search must match the whole row text like the web (Project, Owner, Team, Status, Updated).
3. AdvancedControls.xaml.cs:46 and :139 Project column filter must match Project + Team label like the web's first cell.

Nits (do if cheap)
4. ColEmpty / SortEmpty heading Bold not SemiBold (FilterEmpty already Bold).
5. Sort tie order: use a stable sort (OrderBy / index tiebreak) so ties keep the previous order like the web.
6. Unique AutomationIds for the four pagers' "PageSize" (or note why not).
7. TableClip.cs:33 explain or drop the +1 clip height; re-check corners if changed.
8. Fix the test note claiming re-pin after filter cannot be tested (test it or reword); README Covered line mention sort+filter and pin-scroll checks.
Skipped (out of scope): the unwired sortable text filters/combos (web parity unknown) - report back if the web wires them.
Stop after this round; report in README format.

## Applies after task 08 (tests split) is done
Read handoff/tests-split.md first (tooling change).
Area `tables`, file `wpf/tests/Tests.Tables.cs` (the DataGrid block is there too). Blocking 1 (sortable info text) and nit 8 are there (nit 8's README Covered line is now the `// Covered:` header of `Tests.Tables.cs`). Send this round before overlays starts O4-O6.
