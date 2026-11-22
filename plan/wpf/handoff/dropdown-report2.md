# Dropdown — round-2 leftovers fix

## 1. Files changed

| File | Reason |
|---|---|
| `wpf/TagInput.cs` | Remove-button blur bypass is now mouse-only: `Box_PreviewMouseDown` sets `_removePress` only when a `_Remove` button is pressed (cleared on a background dispatch), and `Input_LostKeyboardFocus` bypasses only on that flag. Shift+Tab to a remove button now blur-adds pending text like the web. |
| `wpf/Autocomplete.cs` | Chip-remove buttons are `Focusable = false`, matching `SearchableSelect` and the suggestion options; a mouse click no longer pulls focus out of the field. |
| `wpf/tests/Tests.Dropdown.cs` | New "T4 keyboard focus to remove adds pending text" + "T4 keyboard blur keeps whole value" (focus-move proxy for Shift+Tab, with `WaitFor` on the hint); adjusted T4/T6/wrap chip indices and counts; new "A3 chip remove not focusable". |
| 7 captures in `plan/wpf/ref/` | All `wpf-dropdown-*.png` re-captured from the final clean binary (no visual change expected from these fixes). |

No csproj edits, no `Tests.Core.cs`, no commits. No other files touched.

## 2. Build and test summary

- `wpf\build.bat dropdown` and `cd wpf\tests && .\build-tests.bat dropdown dropdown`: clean, no nav interference this round.
- Final `-Only dropdown`:

```
DRIVER  in-process actions enabled
AREA  dropdown
AREA  dropdown done in 57,9 s
PROFILE finds=78 findMs=32920
DRIVER  fast=62 fallback=0
RESULT  passed=90  failed=0
```

No `DRIVER fallback` lines, no failures in this area.

**Mutation checks (both run together, then reverted):**

| Mutation | Failing assertions |
|---|---|
| A — restore keyboard bypass (skip blur-add whenever new focus is a `_Remove` button, ignoring the mouse flag) | `FAIL T4 keyboard focus to remove adds pending text - hint=3 of 6 tags`, `FAIL T4 keyboard blur keeps whole value - missing` (+ T4/T6 follow-on failures) |
| B — remove `Focusable = false` from the autocomplete chip-remove button | `FAIL A3 chip remove not focusable` |

Mutation run: `passed=84 failed=6`. After revert: `passed=90 failed=0`. Grep for `MUTATION|HACK|TEMP` in `wpf/*.cs` and `wpf/tests/*.cs`: no matches.

## 3. Captures and measured differences

All 7 refs re-captured from the final binary; measurements unchanged from the previous round:

- `wpf-dropdown-multi-open.png`: card 984x136, panel 450x332, divider y553 1px `#E6E2D7`, Done 56x32.
- `wpf-dropdown-multi-chips.png`: card 984x165, two 24.8 rows + 6 gap.
- `wpf-dropdown-tag-input.png`: card 984x162, container 450x46 at (31,67), chips x42 27.2 tall, placeholder ink x200.
- `wpf-dropdown-tag-input-focus.png`: Brand border x31-480, 3px `#F8ECE4` ring x28-483.
- `wpf-dropdown-autocomplete.png`: card 984x162 (web 161, +1), tech field 450x30, hint y117-128.
- `wpf-dropdown-autocomplete-open2.png`: panel 450x220 at +6, 37px rows, 5px scrollbar core.
- `wpf-dropdown-autocomplete-open.png`: 4-row list on tech focus.

Remaining difference from web: tag hint ink +2-3px (glyph metrics), no change this round.

## 4. Deviations / notes

- Keyboard test uses `Drv.Focus(removeButton)` as the focus-move proxy for Shift+Tab: `PostMessage` cannot set the Shift modifier, and keyboard focus movement reaches the same `LostKeyboardFocus` handler.
- Optional wrapped-state capture was not produced: the capture grammar applies `@focus` only after all `!` steps and `!` steps accept only `ButtonBase`, so it cannot focus the tag TextBox before typing/pressing Enter. `App.xaml.cs` is not an owned file. The "tag box grows so hint moves down on wrap" UIA assertion (hint Top delta ~32) still covers the 78.2px box growth.
- T5/M8/A8 outside-click remain code-only; A8 focusout is asserted.
- `wpf/README.md` coverage list intentionally untouched; the `// Covered:` header is authoritative.

## 5. Blocking questions

None.
