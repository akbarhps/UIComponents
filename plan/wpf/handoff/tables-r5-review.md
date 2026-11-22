# tables round 5 review: fix-first (tests only, small)

Items 1-8 of tables-next.md are all implemented and correct against web/index.html. Product code is clean: no C# 5 violations (no $"", ?., nameof, => members), no scope creep (empty-state LineHeight/Margin mirror the existing FilterEmpty). Suite not run, per instruction.

Verified in code:
- 1 info text: "Showing 1-3 of 3" ("o" on Project+Team = Mobile app, Help center, Legacy archive); default pager text is "1-5 of 5", other pagers read 6 of 6 at that time, and the check is also scoped to SortPagination. Not vacuous.
- 2 filter search: Project/Owner/Team/Status/Updated, matches web row.textContent.
- 3 Project filter: Project+Team in both ColumnGrid and SortGrid, matches the web th text.
- 4 Bold + LineHeight + Margin 5 on ColEmpty/SortEmpty, same as FilterEmpty and `.empty strong`.
- 5 stable sort: _baseRows + OrderBy/OrderByDescending keeps ties in previous order, as the web DOM reorder does. Re-sort on filter is idempotent.
- 6 PagerId DP (default "PageSize"), four unique ids, nothing in the tests referenced "PageSize".
- 7 TableClip: code unchanged, comment now explains the +1.
- 8 Covered header updated, old "cannot be tested" note replaced by a real pin+scroll+filter test.

Fix list (all in wpf/tests/Tests.Tables.cs):
1. Lines 30 and 35-40: "ada" and "design" both give "2 of 6 projects", so the WaitFor is already satisfied by the stale "ada" state. The check only fails because the old code would produce 0. Clear the box before it (SetValue "" and wait for "6 of 6 projects"), or use a term with a different count ("operations" = 1 of 6).
2. Lines 213-219 (pin survives filter): the WaitFor at 214 is vacuous because Legacy archive is already first (Budget asc) before the filter. The pin is re-applied via Dispatcher.BeginInvoke at Loaded priority (ReapplyPinned), so the CellLeft read right after can race and flake. Replace it with WaitFor(() => cell "Legacy archive" found and |CellLeft - leftPinnedScrolled| <= 1, 2000), and do the Check on the final state. Also assert scrollAgain.Current.HorizontalScrollPercent > 0 after the filter, so a scroll reset cannot pass trivially.
3. Lines 96-99 and 193-196: the `pager == null ||` fallback turns the scoped check into the old vacuous root-wide one if the peer is missing. Add Check("... pager found", pager != null) and require the scoped match.
4. (Nit) No test covers item 5 (stable ties). Optional: sort Updated asc, so "Last week" ties Help center and Q3 campaign keep their prior order. Skip if it costs extra UIA actions.
