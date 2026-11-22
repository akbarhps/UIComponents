# pickers — final-review fix round

## Files changed
- `wpf/CalendarPicker.cs` — Tab routing scoped to the trigger; Prev→Next and Shift+Tab Prev→trigger explicit (the adorner is not in the window tab order); `IsKeyboardFocused` guard in the mouse-press focus handler; `_nextButton` field.
- `wpf/FileInput.cs` — 8px row gaps, multi list wrapped in a `ScrollViewer` (MaxHeight 220 + 12 gap, 4px right gutter), broad exception catch for `FileInfo.Length`, remove index via `Tag`.
- `wpf/tests/Tests.Pickers.cs` — updated `// Covered:` header; tab-order Prev→Next and Hour→Minute; focus-retained-after-re-render; P11 uncommitted selection survives Escape/reopen; hour clamp to 23.
- `plan/wpf/ref/wpf-file-input.png` — re-captured (984×237, unchanged idle geometry).

## Build / test
`wpf\build.bat pickers` green; `build-tests.bat pickers pickers` green; `run-hidden.ps1 -Agent pickers -Only pickers` passes alone:
`RESULT  passed=77  failed=0` (89.8 s; `PROFILE finds=140 findMs=89555`, `DRIVER fast=56 fallback=0`).

## Mutation checks (each: edit product code, rebuild, run `-Only pickers`, confirm failure, revert)
| Mutation | Failed assertion(s) | Result |
|---|---|---|
| `Render()` focus scope `_bodyHost` → `_panelRoot` | `FAIL  P5 prev month + focus kept - expected October 2026` | 76/1 |
| Trigger Tab handler `sender == _trigger` removed (routes every Tab to Prev) | `FAIL  tab order Prev -> Next`, `FAIL  tab order Hour -> Minute` | 75/2 |
| Escape clears `_selected` | `FAIL  P11 uncommitted selection survives reopen` | 65/3 |
| Apply hour clamp `Math.Min(23, …)` removed | `FAIL  P6 hour clamps to 23 - Nov 14, 2026, 30:05` | 76/1 |

All mutations reverted; final clean run is the 77/0 above.

## Fixes mapped to review items
1. (blocking) Tab no longer bubbles from panel controls to Prev; Prev→Next and Shift+Tab Prev→trigger are explicit. Verified by two tab-order assertions and their mutation.
2. (blocking) File rows have 8px gaps; multi list scrolls at 220 with a 4px right gutter (446px rows).
3. (blocking) `Invoke(prev)` while Prev is focused asserts focus retention + panel open; mutation fails.
4. (should) Pressing an already-focused button is skipped before toggling `Focusable`.
5. (should) `catch (Exception)` covers inaccessible paths.
6. (should) P11 uncommitted selection and hour clamp added and mutation-checked.
6b. (should) Shift+Tab from Prev returns focus to the trigger (code-only: PostMessage cannot set modifier state).
Nits applied: header, stale comment (already gone), `Tag` index for remove. Not applied: tabular figures on size labels and the Apply-button rasterisation check (shared style / glyph rasterisation, low impact).

## Deviations / questions
- Real mouse-down remains code-only (hidden desktop drops posted WM_LBUTTON*); P10 uses outside-focus close and passes alone.
- FI dialog/drop behaviour remains code-only per Q5.
- No blocking questions. Suggestion unchanged: add a driver `mouse` verb if real mouse presses need automated coverage.
