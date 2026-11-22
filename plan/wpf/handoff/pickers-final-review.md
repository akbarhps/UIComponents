# pickers final review (P1/P2 fixes, P10 isolation, P3-P5)
Verdict: **fix-first** (1 blocking bug, 2 blocking missed steps). Suite not run (per instructions); verified by reading diff + web/index.html 2543-2670, 3169-3224, CSS 606-613.

Prior blocking 1-5: resolved in code (rows swapped; shell 300/Left; IsSelected mode-aware; Render scope = _bodyHost; Focusable=false on press). Fix 5's PostMouseClick test not added (hidden desktop can't deliver WM_LBUTTON*, documented) - accepted as code-only. P10 self-contained via focusout - accepted.

## Blocking
1. wpf/CalendarPicker.cs:230,682-687: bug: Picker_KeyDown is hooked on _panelRoot too, so Tab pressed on ANY panel control (Next, cells, Hour/Minute/Second, Clear, Apply) bubbles up and is redirected to Prev. Keyboard users are trapped on Prev forever; the P3 tab order (Prev, Next, cells, time inputs, Clear, Apply) is unreachable. Fix: only redirect when `sender == _trigger` (or `e.OriginalSource == _trigger`). Test: focus Prev, PostKey Tab, assert Next HasKeyboardFocus; focus Hour, Tab, assert Minute.
2. wpf/FileInput.cs:96-99 + Themes/Pickers.xaml PickerFileItem: missed-step: web .file-list has `gap 8px`; multi mode `max-height 220; overflow-y auto; padding-right 4px` (items 446 wide). WPF stacks rows with no gap and the multi list is a bare StackPanel with MaxHeight, so rows past 220 are clipped with no scroll. Fix: rows Margin 0,0,0,8 except last (or 0,8,0,0 except first); wrap multi list in a ScrollViewer (VerticalScrollBarVisibility Auto, MaxHeight 220) with list Margin/Padding right 4.
3. wpf/tests/Tests.Pickers.cs:81-97: missed-step: prior fix 4 (Render must not pull focus off Prev/Next) has no failing-capable assertion. Mutation `_bodyHost` -> `_panelRoot` at CalendarPicker.cs:428 would pass every assertion (by reading them; suite not run). Fix: after the Tab-to-Prev check, Invoke(prev) and assert `PickDate_Prev` HasKeyboardFocus && panel open.

## Should-fix
4. wpf/CalendarPicker.cs:698-707: setting Focusable=false on the element that already has keyboard focus (e.g. Tab to Next, then mouse-click Next; or click the auto-focused selected cell on reopen) makes KeyboardDevice re-evaluate focus asynchronously, likely moving it out of the panel -> Picker_LostKeyboardFocus closes the picker before Click. Fix: `if (button.IsKeyboardFocused) return;` before disabling Focusable (a press on the focused button can't move focus anyway).
5. wpf/FileInput.cs:145-146: only IOException is caught; FileInfo.Length throws UnauthorizedAccessException / NotSupportedException / ArgumentException -> unhandled crash on drop. Also dropped folders get added as 0 B files (web drops them as files too, so leave that). Fix: `catch (Exception)` or catch those types too.
6. wpf/tests/Tests.Pickers.cs:302-357: W6/P11 "uncommitted time-mode selection stays highlighted on reopen" (P11) not asserted; Apply clamp only tests minutes (hour 0-23 clamp and negative-to-0 untested). Add: pick Day14, Escape, reopen -> Day14 ItemStatus selected while value is placeholder; SetValue Hour "30" -> Apply -> ", 23:".

6b. wpf/CalendarPicker.cs:682: Shift+Tab from Prev follows the visual tree out of the adorner (not back to the trigger as on the web, where the panel follows the trigger in DOM order) and focusout closes the picker. Fix: on Shift+Tab with `e.OriginalSource == _prevButton`, focus `_trigger` and mark Handled.

## Nits
6c. wpf/tests/Tests.Pickers.cs:1: `// Covered:` header omits the Date & time picker (P3/P4/P6/W6) section.
7. wpf/CalendarPicker.cs:524: stale ponytail comment "P3 uses this branch to copy the time inputs" on Cell_Click (month/year never have time; P3 lives in Day_Click). Delete.
8. wpf/FileInput.cs:194-197: Remove_Click finds the index by string-matching AutomationIds; set `remove.Tag = i` and read it.
9. wpf/FileInput.cs:238-245: size label lacks tabular figures (web `.file-item__size` tabular). Code review only; low impact.
10. plan/wpf/ref/wpf-pickers-datetime-open.png: Apply button looks soft/blurred vs crisp web ref (likely the shared primary-sm style or a hover state at capture). Check it isn't a pickers-owned effect; day digits also render heavier than web 500 (earlier "confirm FontWeights.Medium" nit still open).

Mutation coverage check (reading assertions): mode-aware selection - can fail (month/year reopen markers after Today, except on the 1st / Jan); W6 retention - can fail (hour "12" after Clear); Apply clamp - can fail (12:59); focus fix-up - CANNOT fail (item 3); file de-dupe/size labels - code-only per Q5, no test (expected).
No test-only hooks in product code; Browse_Click/ListBox removed; no debug/temp code found.
