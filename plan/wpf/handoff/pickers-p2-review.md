# pickers P1-fix + P2 phase review (diff-only; nothing built or run)

Scope read: wpf/CalendarPicker.cs (all), wpf/Themes/Pickers.xaml, InputControls.xaml Date picker card, UIAutomationTests.cs picker block (lines ~1064-1237), web/index.html 466-502 and 2543-2700, refs web-pickers-{date,month,year}-open.png vs wpf-pickers-*.png.

VERDICT: fix-first (one visual bug visible in all three P1/P2 captures, one selected-state bug in month/year, one keyboard regression, one unverified mouse-click risk, a vacuous test).

## Blocking

0. wpf/InputControls.xaml:140-141 - the Date picker card rows are swapped: `73.8` then `95.8`. Web: row 0 = 73.8 content + 22 gap = 95.8, row 1 = 73.8 (plan: label y 31 / 126.8, triggers 58.8 / 154.6). Total is still 231.6, which is why the card-height check passed. Captures confirm: WPF "Year only" label sits 12px under the first trigger (y 281 vs trigger bottom 269), web 32px (293 vs 261); the Year-only trigger is 22px too high. Fix: swap to `95.8` / `73.8`, re-capture the three refs and the idle card.

0b. wpf/CalendarPicker.cs:599-609 - promoted from item 4: handoff said restore focus "after the click"; the worker restores at Input priority from PreviewMouseLeftButtonDown, i.e. before the mouse-up. See item 4 for the risk and the test that must prove it.

1. wpf/CalendarPicker.cs:223-228 (shell/shadow) - the shadow Border (`DropdownPanelShadow`) sits in a `shell` Grid that DropdownAdorner arranges at the trigger width (450), while the panel is `Width=300, HorizontalAlignment=Left`. Result: a 450-wide white, shadowed slab sticks out 150px to the right of the 300px panel. Visible in wpf-pickers-date-open.png, -month-open.png, -year-open.png (web ref: panel is exactly 300 wide, nothing behind it). The slab is also inside `_panelRoot`, so it is a hit-test dead zone that DropdownOutsideClick treats as "inside" (a click on whatever is under it neither reaches that control nor closes the picker). The reported "panels 300 wide" measured the panel Border only. Fix: `shell.Width = 300; shell.HorizontalAlignment = HorizontalAlignment.Left;` (or give the shadow the same Width/Alignment as the panel), re-capture the three refs.

2. wpf/CalendarPicker.cs:195-198 (`IsSelected`) + 534-543 (`Today_Click`) - selection equality compares Year, Month and Day for every mode. In month mode cells carry Day=1 and in year mode Month=0, Day=1, but Today (shown in month and year mode, as in the web) stores today's month/day. After Today, reopening PickMonth/PickYear shows no selected cell (and no focus-on-reopen) unless today happens to be the 1st / Jan 1st. Web: year mode compares `y` only, month mode `y` and `m`. Fix: make IsSelected mode-aware (year: Year; month: Year+Month; date: all three). Add assertions: pick Today in PickMonth and PickYear, reopen, cell ItemStatus contains "selected".

3. wpf/CalendarPicker.cs:331-338 (`Render`) - the focus fix-up fires for focus anywhere in `_panelRoot`, including Prev/Next. A keyboard user on Next presses Enter: the body re-renders and focus jumps to the trigger, so Next cannot be pressed twice by keyboard (web keeps focus on Next; only the body is replaced). Fix: test `DropdownTree.IsWithin(focused, _bodyHost)` instead of `_panelRoot`. This also matters for P3: with the current condition every re-render would pull focus out of the Hour/Minute inputs.

## Should-fix

4. wpf/CalendarPicker.cs:599-609 (`Panel_PreviewMouseLeftButtonDown`) - UNVERIFIED RISK, needs a test before approval. The handler lets the pressed Button take focus, then restores the previous focus at Input priority, i.e. while the left button is still down. ButtonBase.OnLostKeyboardFocus releases capture and clears IsPressed (as I remember the framework code, I did not run it), so a real press/hold/release on a day cell, Prev/Next, Clear or Today can lose its Click. The suite never clicks a panel button with the mouse (all UIA Invoke), so 306/0 says nothing about it. Fix options: (a) cheaper and safer: in the handler, if the target is a Button set `Focusable=false` on it for the press and restore `Focusable=true` on PreviewMouseLeftButtonUp/LostMouseCapture (the button never takes focus, Click still fires; also covers Prev/Next/Clear/Today with one handler on the panel); (b) keep the restore but do it from PreviewMouseLeftButtonUp via BeginInvoke after the click. Add a test: PostMouseClick (UIAutomationTests.cs:156) on `PickDate_Day14` and assert the value commits and the trigger/cell focus did not change. If the timing of the posted down/up pair cannot reproduce the hold, say so.

5. wpf/CalendarPicker.cs (Tab path) - with no selection the panel opens with focus still on the trigger; Tab then leaves the picker (the adorner is not after the trigger in the tab order), `Picker_LostKeyboardFocus` closes it, and the panel is unreachable from the keyboard. Web: Tab from the trigger lands on Previous. Review fix 2 ("keyboard users can't pick a date") is therefore only met when a date is already selected. Fix: in the trigger KeyDown, when the panel is open and `Key.Tab` without Shift, `e.Handled = true; prevButton.Focus()`. Add an assertion (UIA `IsKeyboardFocusable` on Prev/Next/Clear/Today/Day cells at minimum; the reviewer-requested "tab stops" change has no test at all).

6. wpf/Themes/Pickers.xaml:79-105 (`PickerCell`) - `FocusVisualStyle` is null and the template has no focus trigger, so now that cells (and, check, IconButton) are tab stops the focused cell has no indicator; the web gets the global `:focus-visible` 2px ring (index.html:50). Fix: add an `IsKeyboardFocused` trigger with a 2px `Brand` (focus colour) ring, offset like the trigger ring. Mouse presses do not move focus (item 4), so a plain IsKeyboardFocused trigger matches `:focus-visible`.

7. wpf/CalendarPicker.cs:221 and 589-595 (`LayoutUpdated`) - the handler is never unhooked: `Panel_LayoutUpdated` runs on every layout pass of the app while the panel exists, returns early when there is no pending cell (the no-selection case never clears it), and `Close()` does not remove it. Fix: `_panelRoot.LayoutUpdated -= Panel_LayoutUpdated` after the focus is applied and in `Close()`. The 100 ms timer is stopped by its own Tick, so no timer leak, but the LayoutUpdated + timer pair is belt and braces; one `Dispatcher.BeginInvoke(DispatcherPriority.Loaded, ...)` that focuses the cell would do (ponytail). Also `_focusCellOnOpen` is nulled even if `cell.Focus()` returned false.

8. wpf/tests/UIAutomationTests.cs:1149-1152 ("P11 view month survives reopen") is vacuous: the flow goes Prev, Next (back to today's month), pick day 14, reopen, so "view reset to today" and "view survived" give the same title. Fix: navigate Next once, pick a day in that other month, reopen and expect that month's title (the Next-flow check at 1153-1160 can be folded in).

9. wpf/tests/UIAutomationTests.cs:1196-1237 (P2 block) - month/year coverage is thin: no Prev/Next in month (+-1 year) or year (+-20, title "2038-2057"), no selected-marker/focus-on-reopen for month and year, no Today in month/year (would have caught item 2), no Clear. Add the minimum: Next in month and year with title checks, reopen after a pick and assert ItemStatus "selected" on the picked cell. Keep UIA actions low (each ~2 s).

## Nits

10. wpf/CalendarPicker.cs:360 - month cell Name is "Jan"; web aria-label is the full month ("January"). Use `MonthsFull[i]` for AutomationProperties.Name (keep "Jan" as the visible text). Year cells have no label in the web, fine.

11. (SHOULD-FIX, fails P1 acceptance 3) wpf/CalendarPicker.cs:492-500 + Pickers.xaml:91 - the today ring is a Border inside a `Grid` placed in a ContentPresenter with HorizontalAlignment=Center, so it shrinks to the digits: wpf-pickers-date-open.png shows a ~16px circle around "22" where the web draws a 37.4x32 radius-7 ring. P1 acceptance 3. Fix: ContentPresenter `HorizontalAlignment/VerticalAlignment=Stretch` in PickerCell (labels already centre themselves) or set `content.HorizontalAlignment/VerticalAlignment=Stretch`. Re-capture.

12. wpf/CalendarPicker.cs:533-542 - `Today_Click` hardcodes Hour 9/Minute 30 and the `ponytail:` comment says P3 replaces them. Today only exists when `!WithTime` (web 2650-2663), and Format() ignores time then: the code is dead and the comment is wrong. Delete both.

13. (dropped) DateTime year-range clamp: unreachable in practice, not a finding.

14. wpf/CalendarPicker.cs:416, 505 - FontWeights.Medium: in the captures digits look as heavy as SemiBold next to the web's 500; confirm the Sans family has a Medium face, otherwise it silently falls back. Handoff nit 4 said "if the face exists".

15. W7 selected marker is ItemStatus "selected" on a Button (web: `aria-pressed`, a toggle state). The report states the deviation, which satisfies handoff item 4; no UIA TogglePattern means screen readers announce it as status text only. Accepted deviation, not blocking.

## Checked, no finding

- Web parity: titles ("November 2026", "2026", "2018-2037" with en dash), formats ("Nov 14, 2026", "Jan 2026", "2026"), Clear (placeholder, Ink3, close, no focus restore), Today/commit (close, no focus restore), Escape (close + focus trigger, handler on trigger and panel root), view not reset on reopen, uncommitted state survives, outside-click and focusout rules (null NewFocus ignored, non-null outside closes, Tab-out closes), month +-1 year, year +-20 start = viewYear-8, selected-cell focus on reopen, dialog names. Geometry in code matches the plan (288/274 inner widths, 88 and 64.8 columns).
- P1-fix items: 1 done (null guard + focus to trigger before Render, modulo item 3), 2 done in code but see items 4-6, 3 mostly done (P10 real, selected focus, P11 selection, ItemStatus marker, finding-1 flow) but item 8-9, 4 stated deviation + P3-only code annotated with `ponytail:` comments.
- C# 5: no `?.`, `$""`, `nameof`, expression bodies; no BOM in CalendarPicker.cs or Pickers.xaml; csproj has both Compile and Page entries; App.xaml merges Pickers.xaml.
- Timer: stops itself on Tick. Outside-click hook: unhooked in Close. Closure over `focused` is harmless.
- Product code hooks for tests: none found (no test-only proxies in CalendarPicker.cs; ids are real AutomationIds).
- Month/year panel heights (261/262, 299) and card 231.6 are layout numbers from the worker's report; not re-measured.

## Cross-worker finding: assert `element.Current.HasKeyboardFocus`, not `AutomationElement.FocusedElement`

Yes, `HasKeyboardFocus` on the specific element is the right assertion in this suite. The UIA peer reads `UIElement.IsKeyboardFocused` directly, i.e. WPF's own `Keyboard.FocusedElement == element`, which is exactly what our code controls. `AutomationElement.FocusedElement` reported the adorner Pane here (observed); the cause (likely the window focus provider not mapping adorner-hosted elements, or hidden-desktop activation) is not verified, but it is unreliable for anything inside an adorner (dropdown M7, picker panels, toasts if they use adorners). Caveats to write into REVIEWER.md: (a) always `WaitFor` (focus can arrive after the Checked handler, see the 100 ms fallback); (b) a false only says "not on this element", so for negative checks also assert the expected owner (e.g. trigger); (c) it does not prove a screen reader tracks focus into the adorner (UIA FocusedElement reporting the Pane suggests it will not): treat that as a separate accessibility follow-up for all adorner panels, not a test pattern; (d) `SetFocus()` + `HasKeyboardFocus` already works for the trigger in P9. Recommend the other workers (dropdown M7, toasts) switch their focus assertions to this pattern instead of dropping them.

## What P3 depends on

- Fix items 3 and 4 first: P3's time inputs need (a) Render's focus fix-up limited to the body, (b) the mouse-press handler to skip `TextBox` (web: `mousedown` is not prevented on `input`), and (c) Tab order Prev, Next, cells, Hour, Minute, (Second), Clear, Apply.
- BuildPanel has a hard-coded 5-row grid (head, 8, body, 10, foot 43): add the 71.4 time row (top border, padding-top 12, margin 10) between body and foot; panel height 398 for date+time.
- `WithTime` must be true when `WithSeconds` is (web: `time === 'true' || seconds === 'true'`), or the XAML sets both; `Format()` uses WithTime first.
- `Day_Click` time branch must copy Hour/Minute/Second (`int.TryParse` else 0, unclamped), then Render and stay open; `Apply_Click` must clamp 0-23/0-59/0-59 then Commit (currently commits an unclamped/zero time). `Selection.Hour/Minute/Second` already exist.
- `Icon == "Clock"` is wired; the date-only pickers in the date-time section also use the calendar icon (W4 fixed in the web).
- Clear keeps the time inputs (W6): the inputs are built per BuildPanel call, so they must live as fields on the control (not re-created from `_selected`) or reopen would reset them.
- IsSelected (item 2) keeps its meaning for time mode; keep the uncommitted selection highlighted after Close (already true).
