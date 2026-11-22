# Dropdown final review, round 2 (fix round)

Scope: DropdownPrimitives.cs, Autocomplete.cs, TagInput.cs, MultiSelect.cs, tests/Tests.Dropdown.cs, wpf-dropdown-*.png. I reviewed the code only and did not run the suite.

Verdict: **clean** (0 blocking). One should-fix is optional and can be folded into a later round.

## Prior items

1. Blocking 1 (Measure did not wrap): fixed. `DropdownPrimitives.cs:65-75`: Measure now wraps non-last children and applies Arrange's `rest < MinFillWidth` rule to the fill child. Measure's height and Arrange's layout now agree. The new assertion "tag box grows so hint moves down on wrap" (Tests.Dropdown.cs:88-90) checks the hint, which sits outside the box, so it can fail (the worker's mutation gave delta=-4).
2. Blocking 2 (option took focus): fixed. `Autocomplete.cs:290` sets `option.Focusable = false`. The A5 check is a proxy (`IsKeyboardFocusable`) and does not exercise a real mouse press, but it does fail if the line is removed.
3. Blocking 3 (duplicate did not clear): fixed. `Autocomplete.cs:338-353`: only the add is skipped, and clear, Hide and Focus always run. The test now uses `WaitFor(GetValue == "")` and asserts both the hint and the empty field (167-170).
4. Should-fix 4 (remove button blur-add): fixed for the mouse path (`TagInput.cs:142-153`, `221-224`), but this adds a keyboard regression (see below).
5. Should-fix 5 (vacuous T4): fixed. It uses "x, y" and asserts that chip3's text is "x, y" (49-56).
6. Should-fix 6 (Children.Clear churn): fixed in both files. Only the chips are removed and new chips are inserted before `_input`.
7. Nit 10: the header now says "A8 focusout", and the report lists Shift+Tab-stays-open as unverified. Nits 7-9 were not done, which is allowed because they were optional.

## Findings

- wpf/TagInput.cs:142-153: should-fix: the remove-button bypass also triggers on the keyboard. Remove buttons are tab stops before the field, so Shift+Tab from a field with pending text lands on `TagTags_Remove{n}`, the blur-add is skipped, and the text stays pending (it is never added when focus later leaves the button). On the web, Shift+Tab fires focusout, so the text gets added. Fix: set a `_removePress` flag in `Box_PreviewMouseDown` where it already detects a `_Remove` target, and bypass in `Input_LostKeyboardFocus` only when that flag is set (clear it there), or check `Mouse.LeftButton == MouseButtonState.Pressed`.
- wpf/Autocomplete.cs:158-164: nit: the autocomplete chip-remove buttons still take focus on a mouse click, so the list hides and focus leaves the input. The web keeps focus in the field. Low impact, so apply the same treatment only if cheap.
- plan/wpf/ref: nit: there is still no wrapped-state capture. The hint-Top UIA assertion stands in for it, which is acceptable, but the 78.2px box height is not pixel-verified.

## Hygiene and captures

- No mutation leftovers: a grep for MUTATION/HACK/TEMP/mutat in wpf/*.cs and wpf/tests/*.cs found nothing relevant.
- Timestamps: the last source edit was at 12:50:17 (TagInput.cs), `bin/dropdown` was built at 12:53:13, and the 7 captures were taken between 12:53:21 and 12:53:27. The captures therefore come from the final, clean build, not the mutated one.
- The captures look plausible: tag-input-focus shows the ring, 2 chips and the placeholder; autocomplete-open2 shows the panel at +6 with 37px rows and a scroll bar. None of the 7 captures is in a wrapped state, so the Measure fix cannot show up in them, which is expected.
