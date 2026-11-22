# Dropdown — final review fix round

## 1. Files changed

| File | Reason |
|---|---|
| `wpf/DropdownPrimitives.cs` | Blocking 1: `ChipWrapPanel.MeasureOverride` now wraps non-last children with `FillLast` set and applies Arrange's `rest < MinFillWidth` wrap to the fill child, so the TagBox grows to 78.2 instead of staying 46. |
| `wpf/Autocomplete.cs` | Blocking 2: suggestion buttons `Focusable = false` (mouse-down no longer hides the adorner before Click). Blocking 3: duplicate `Choose` skips only the add, then always clears input, hides and focuses. Should-fix 6: `RenderTags` removes only chips, keeping `_input` attached. Input added explicitly when the chip host is built. |
| `wpf/TagInput.cs` | Should-fix 4: `Input_LostKeyboardFocus` ignores focus moves to a `_Remove` button; `Remove_Click` refocuses the input when focus was inside the box. Should-fix 6: `Render` removes only chips, keeping `_input` attached. |
| `wpf/tests/Tests.Dropdown.cs` | Vacuous T4 now uses "x, y" and asserts the exact chip text; new wrap assertion checks `TagTags_Hint` Top also moves down one row; A5 asserts the option is not keyboard focusable; A3 duplicate now waits for the field to clear and asserts it; header says "A8 focusout". |
| 7 captures in `plan/wpf/ref/` | All `wpf-dropdown-*.png` re-captured from the final, reverted, clean binary. |

No csproj entries (wildcards), no `Tests.Core.cs` edits, no commits. Nits 7-9 (SearchName/ShowHint, dead `_Box` id, Max default) were not changed: optional, no current behaviour effect.

## 2. Build and test summary

- `wpf\build.bat dropdown` and `cd wpf\tests && .\build-tests.bat dropdown dropdown`: clean. One intermediate build failure in nav's `MainWindow.xaml` (`Navigation_Loaded` missing) was waited out (~145 s total) and rebuilt; that file was not edited.
- Final `-Only dropdown` run:

```
DRIVER  in-process actions enabled
AREA  dropdown
AREA  dropdown done in 71,9 s
PROFILE finds=76 findMs=42098
DRIVER  fast=58 fallback=0
RESULT  passed=87  failed=0
```

- No `DRIVER fallback` lines. No failures in this area.
- **Mutation check** (all four new/changed assertions temporarily broken, then reverted):

| Mutation | Result |
|---|---|
| `MeasureOverride` wrap disabled (old `!FillLast` guard + no fill-wrap) | `FAIL tag box grows so hint moves down on wrap - delta=-4` |
| `option.Focusable = false` removed | `FAIL A5 option not focusable` |
| Duplicate early-return restored | `FAIL A3 duplicate clears field - react` |
| Blur uses comma split (`AddFromField`) | `FAIL T4 no comma split on blur - x` (plus T4 hint and T6 follow-on) |

Mutation run: `passed=81 failed=6`. After revert: `passed=87 failed=0`. No `MUTATION` markers remain in the source.

## 3. Captures and measured differences

All re-captured from the final binary.

| Capture | Measurement (web reference in parentheses) | Result |
|---|---|---|
| `plan/wpf/ref/wpf-dropdown-multi-open.png` | card 984x136 (135.8); panel 450x332 (332.2); divider y553 1px `#E6E2D7` 445px; Done 56x32; chip column 402 + 8 gap | within ±1 |
| `plan/wpf/ref/wpf-dropdown-multi-chips.png` | card 984x165; two 24.8 chip rows + 6 gap = 55.6 | ✓ |
| `plan/wpf/ref/wpf-dropdown-tag-input.png` | card 984x162 (162); container 450x46 at x31 y67; chips x42, 27.2 tall, widths 67/75 (67.2/76.5); placeholder ink x200 (199.7) | chips/field ±1 |
| `plan/wpf/ref/wpf-dropdown-tag-input-focus.png` | Brand border x31-480 y67-96; 3px `#F8ECE4` ring x28-483 y56-106 | ✓ |
| `plan/wpf/ref/wpf-dropdown-autocomplete.png` | card 984x162 (161, +1); tech field 450x30; "0 selected" hint y117-128 | ✓ (+1 card) |
| `plan/wpf/ref/wpf-dropdown-autocomplete-open2.png` | panel 450x220 at +6 (y274-481); 37px row pitch; 5px `#D5CFC0` scrollbar core | ✓ |
| `plan/wpf/ref/wpf-dropdown-autocomplete-open.png` | 4-row list (~146) on tech focus | ✓ |

Remaining measured difference: tag hint ink y118-129 vs web ~115-127 (+2-3 px, glyph/line metrics); box geometry matches. No wrapped-state capture was added: the hint-Top UIA assertion proves the ~32 px box growth, which the capture grammar cannot set up (it cannot focus a TextBox before `^text` steps).

## 4. Deviations / notes

- T5 container click, M8/A8 outside click are mouse-only: ported in code, not UIA-asserted. A8 focusout is asserted.
- Shift+Tab-stays-open (p3 nit) was not added as an assertion and is unverified.
- `~Key` capture grammar cannot combine chips + typed text + highlight; `wpf-dropdown-autocomplete-open.png` shows the list with no chips, as the plan anticipated.
- `wpf/README.md` coverage list was intentionally not edited; the `// Covered:` header is authoritative under tests-split.
- nav's button restyle was not touched; no capture section here contains full-size buttons.

## 5. Blocking questions

None.
