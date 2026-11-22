# Dropdown final review (Phase 3 fixes + Phases 4-6)

Scope: the dropdown diff only (MultiSelect.cs, DropdownPrimitives.cs, TagInput.cs, Autocomplete.cs, InputControls.xaml(.cs), Themes/Dropdown.xaml, tests/Tests.Dropdown.cs, wpf-dropdown-*.png). I reviewed the code, checked it against web/index.html (current: the autocomplete hint and duplicate check are web fixes, so the plan's A3/A6 "quirk" rows are stale and the worker was right to follow the web), and looked at the captures. I did not build or run anything.

Verdict: **fix-first**, 3 blocking.

## Blocking

1. `wpf/DropdownPrimitives.cs:71`: with `FillLast` set, `MeasureOverride` never wraps. The `!FillLast` guard turns off the wrap for every child, and the last child's "rest < MinFillWidth" wrap exists only in `ArrangeOverride` (91). Measure therefore always returns a single-row height, while Arrange puts the field (or overflowing chips) on a second row, outside the measured box. So after 4 tags the TagBox stays 46 tall and the field paints over the border and the hint, where acceptance 4.5 expects 78.2. The test at `Tests.Dropdown.cs` "tag field wraps to a second row" passes anyway, because it only checks the field's Top. Fix: in Measure, keep the normal wrap for non-last children, and for the last child copy Arrange's rule (`rest < MinFillWidth` → `y += rowH + GapY; rowH = 0`). Add a check that `TagTags_Hint`'s `BoundingRectangle.Top` also moves down by about 32 (the hint sits below the box and has a UIA peer). Re-capture with a wrapped state, or measure the box from a capture.
2. `wpf/Autocomplete.cs:288-294`: the option buttons can take focus. On a real mouse click, `ButtonBase` focuses the option on mouse down. That raises `Input_LostFocus` (200-204), which calls `Hide()` and removes the adorner before `Click` fires, so **choosing a suggestion with the mouse does nothing**. The web avoids this with `list mousedown preventDefault`. UIA `Invoke` in A5 skips the mouse path, so no test catches it. Fix: `option.Focusable = false`, as `SearchableSelect.cs:244` already does.
3. `wpf/Autocomplete.cs:339-340`: in Multiple mode, a duplicate returns before `_input.Text = ""` and `_input.Focus()`. The web (`choose`, index.html:2740) always clears the input. Typing "react" and pressing Enter leaves "react" in the field in WPF but clears it on the web. Fix: only skip the add; always clear, hide and focus. Test: "A3 duplicate on Enter ignored" checks the hint right after the asynchronous `PostKey` with no wait, so it can pass before Enter is handled. Change it to `WaitFor(GetValue(techInput) == "")`, then assert "1 selected".

## Should-fix

4. `wpf/TagInput.cs:140-148,186-192`: the chip-remove `Button` can take focus. A mouse click on remove while text is pending moves focus to the button, which triggers the blur-add. `Render()` then rebuilds the chips, the pressed button disappears, and the remove never happens. Even with an empty field, focus is left nowhere. The web uses `remove mousedown preventDefault`, so focus stays in the input. Fix: in `Input_LostKeyboardFocus`, return early when `e.NewFocus` is a `_Remove` button inside `_box`. In `Remove_Click`, refocus `_input` if it was focused before. This path is mouse-only (T5-adjacent), so write it in code and say it is code-only in the report.
5. `wpf/tests/Tests.Dropdown.cs` "T4 no comma split on blur" is vacuous: "zeta" has no comma. Use a value such as "x, y", then expect chip3 to be "x, y" and the hint to increase by 1.
6. `wpf/TagInput.cs:170` / `wpf/Autocomplete.cs:140`: `Children.Clear()` detaches the focused `_input` on every render, and the code then calls `Focus()` to recover. Removing only the chips (`RemoveRange(0, Count - 1)` and inserting before the input) avoids the focus churn and the blur handler firing during a render.

## Nits

7. `wpf/Autocomplete.cs:38` `SearchName` is set in XAML (`InputControls.xaml` AcFramework) but never read. Remove it. `ShowHint` is true on both instances. Drop it and derive the hint from `Multiple || HintText != null`.
8. `wpf/TagInput.cs:81` `_id + "_Box"` AutomationId: a `Border` has no UIA peer, so this is dead. Remove it.
9. `wpf/TagInput.cs:160`: `Max` defaults to 0, so a TagInput without `Max` can never add, and its hint reads "N of 0 tags". The web treats it as Infinity and shows "N tags". Both instances set Max, so this does not affect current behaviour.
10. `Tests.Dropdown.cs:1` header claims A1-A8. A8 outside-click and T5 are code-only (the report says so), so make the header say "A8 focusout". The Shift+Tab-stays-open nit from the p3 review was not added. Add the assertion or list it as unverified.

## Checked and fine

- Phase 3 fixes. M7 now really reopens, waits for `_Search`, sends Escape, and asserts that the panel closes and the trigger has focus (worker mutation-checked it). `_selected` is allocated at its declaration. `_building` covers the init loop. The redundant `UpdateTrigger` calls are gone: the call at 323 is still needed for `_count`, and the one in `Remove_Click` only runs while closed.
- InputControls.xaml.cs: the dead tag/autocomplete code and its usings are gone. The old ids are gone, and the new ids and section shells match the plan.
- Tag input: T1, T2 (split, trim, case-insensitive duplicates, always clear, comma via `PreviewTextInput`), T3, T4, T6 and T7 all match web 2770-2813. The T2, T3, T6 and T7 assertions can fail: T2 was mutation-checked, and T7 would read 5 without the cap.
- Autocomplete: filter, minus chosen tags, the 8-item cap, active index reset, clamped Down/Up, Enter, Escape, focusout hide and the "N selected" hint all match the current web (2703-2767). The list has `MaxHeight` 218 inside the border with a padding of 6.
- No test-only hooks in product code. No csproj edits. Everything is in owned files.
- Pixel checks: the tag-input focus capture matches the web (ring, chip positions, placeholder x). In autocomplete-open2, the panel sits at +6 with 37px rows and a scroll bar, and the card and field offsets are within 1px.
