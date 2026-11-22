# pickers final review 2 (fix round)
Verdict: **fix-first** (1 blocking, one-number fix). Suite not run (per instructions); verified by reading CalendarPicker.cs, FileInput.cs, Tests.Pickers.cs, Tests.Core.cs driver helpers, web/index.html 592/606-607.

## Prior items
1. Tab trap - resolved. CalendarPicker.cs:687-704: trigger Tab -> Prev only when `sender == _trigger`; panel redirects only when `OriginalSource == _prevButton` (Tab -> Next, Shift+Tab -> trigger). Other panel controls tab naturally.
2. FileInput gap/scroll - partly resolved. Row gap 8 (FileInput.cs:222) and ScrollViewer + 4px right gutter are right; MaxHeight is wrong (see Blocking 1).
3. Focus fix-up test - resolved. Tests.Pickers.cs:95-103 focuses Prev, invokes, waits for title AND Prev HasKeyboardFocus; `_bodyHost -> _panelRoot` mutation would fail it (Drv.Focus/Invoke are synchronous in-process driver calls, so there's no ordering race).
4. Focusable=false on the focused button - resolved (CalendarPicker.cs:721).
5. FileInfo exceptions - resolved (`catch (Exception)`, FileInput.cs:158).
6. P11 + hour clamp - resolved (Tests.Pickers.cs:350-366). Escape is awaited (panel null) before reopen; Day14 `selected` + placeholder asserted; hour 30 -> `, 23:05`. Negative-to-0 still untested (accepted, same Math.Max line).
6b. Shift+Tab from Prev -> trigger - resolved in code (CalendarPicker.cs:699-704); code-only (PostMessage has no modifier state). Accepted.
Nits: header updated, stale comment gone, Remove uses Tag (FileInput.cs:207-208, 261). Tabular size figures and Apply-button raster not applied (acceptable, low impact).

New assertions: every PostKey (Tab x3, Escape) is followed by a WaitFor on its effect before the next action/assert; each can fail (mutations in the report line up with the code). No leftover mutation/debug/temp code in the pickers files.
Capture plan/wpf/ref/wpf-file-input.png (14:12) is newer than FileInput.cs (13:55) and shows the idle state as expected. List gap/scroll can't be seen at idle (no seeding hook per Q5).

## Blocking
1. wpf/FileInput.cs:104: bug: `scroller.MaxHeight = 232; // 220 list + 12 column gap`. In WPF the Margin (0,12,0,0, line 107) sits outside MaxHeight, so the scroll viewport is 232 tall, not 220 (web .file-input[data-multiple] .file-list max-height 220; the 12 is the parent flex gap). The multi list shows 12px more than the web before it scrolls. Fix: `scroller.MaxHeight = 220;` and fix the comment.

## Nits
2. wpf/tests/Tests.Pickers.cs:83-92: nit: the report says Tab from Prev "leaves the adorner without help", so natural tab navigation inside the adorner is suspect. Only Prev->Next and Hour->Minute are asserted; Next -> first focusable day cell isn't. Optional: focus Next, Tab, assert a `PickDate_Day*` cell HasKeyboardFocus and the panel is still open.
