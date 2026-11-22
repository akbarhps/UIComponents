# toasts final review (T1-T3 + F1-F3): FIX-FIRST

Scope reviewed: uncommitted diff in wpf/Toasts.cs, wpf/tests/Tests.Toasts.cs, wpf/SheetGrid.cs, wpf/MainWindow.Display.cs,
wpf/DataControls.xaml(.cs) against web/index.html (3026-3082, 3317, 3428-3439, 3443+). Suite not run (per brief).
C# 5 check: clean. Toasts.cs Schedule IsEnabled guard (fix 5): done. F2 (MainWindow.Display.cs:40): matches web 3318.

## Blocking

1. wpf/tests/Tests.Toasts.cs:228-233: bug: the pause test is still vacuous. It waits `remaining - 400` while focused,
   which is BEFORE the original expiry, so the toast is there even with no pause. The resume check
   (`gone within remaining + 800`) also passes with no pause. A build with Pause() deleted passes both checks.
   Fix: while focused, wait `remaining + 600` (past the original expiry) and assert present ("focus pauses").
   Then `Drv.Focus(bDismiss)`, record `leftAt`, assert still present at `leftAt + remaining - 400` (lower bound, driver-review #7),
   then assert gone by `leftAt + remaining + 800`.

2. wpf/SheetGrid.cs:602-620: bug: the Ctrl+C texts are swapped against the web, and there is a single-cell copy the web does not have.
   - Web keyboard copy (document `copy` handler, 3072-3082) uses `selectionText()`, which returns '' when selection.size < 2 (3027).
     So one cell gives no custom copy and no toast. With 2+ cells, the toast is "Cells copied. Paste into Excel." (no count).
   - The "N cells copied." text belongs only to the Copy button (3057).
   - WPF now: Ctrl+C on 2+ cells goes through CopySelected() and toasts "N cells copied." (wrong text). Ctrl+C on one cell copies
     that cell and toasts "Cells copied." The web does neither, and the comment at :603 misstates what the web does.
   Fix: delete the single-cell block (603-620). Give CopySelected a toast-text parameter (or return bool and toast at the call sites).
   The button (:561) toasts `N cells copied.`, Ctrl+C (:602) toasts `Cells copied. Paste into Excel.`

## Non-blocking

3. wpf/tests/Tests.Toasts.cs:2: missed-step: the header says "F1-F3 call sites" are covered, but no assertion anywhere raises or
   finds a "Copied" / "Upload complete" / "Rows deleted" / "Export started" toast. Either add cheap checks or drop the claim.
   - Grid copy button -> ToastsFind("Copied").
   - Tables bulk bar: tick one row, Invoke BulkExport -> "Export started"; Invoke BulkDelete -> "Rows deleted", and the row count drops by 1.
4. wpf/DataControls.xaml.cs:140-148: nit: after Delete the web's updateBulkbar recomputes selectAll (3420-3421), so it ends unchecked
   when every row is deleted. WPF leaves the SelectAll header checkbox checked. Tables-owned, so route it to tables if not fixed here.
5. wpf/tests/Tests.Toasts.cs:276-280: nit: the focus-restore check does not assert that the first toast was removed. Add
   ToastsWaitGone on dups[0]'s id so a no-op Invoke cannot leave focus where the test expects it.

## Accepted / recorded
- No focus ring on close/action (FocusVisualStyle=null), recorded in the test header as an accepted difference (review fix 4 allowed this).
- Stack margin 19 + toast 5/5 is equivalent to the plan's 0,0,24,24 (recorded, Toasts.cs:42).
- Cross-area: toast layer/stack have no Background, so they are not hit-testable outside the cards. Tests.Grid copy and Tests.Display
  progress now raise 3.6 s toasts at bottom-right, but no other area test asserts their absence. No other-area regression found.
- DataControls.xaml FilterPagination PagerId / filter search columns and MainWindow.xaml PagerId belong to the tables round, not toasts.
