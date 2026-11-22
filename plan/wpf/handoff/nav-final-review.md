# nav final review (N1 fixes + N2-N7)

I reviewed the diff and the captures only. I did not run the full suite or the nav area.

**Verdict: fix-first.** There are 4 blocking findings. The N1 fix-first items are resolved: the Rotor pivot, B1 aria-disabled with `_working`, Cursor=No, the BtnDisabled assert, and the Invoke retry.

## Blocking
1. wpf/MainWindow.xaml:690 and :728 (both `Accordion` Borders): bug. A fixed `Height="208.4"` clips the open accordion. In `wpf-nav-accordion-toggled.png`, "Billing and plan" is cut off and the card stays 307 tall, but N4 acceptance 1 wants 370. In single mode with every item closed (A2 allows that), the fixed height also leaves blank space. Fix: drop both fixed heights, re-capture the toggled state, and check the card is 370 ±1.
2. wpf/MainWindow.xaml:982, :994, :1006 (vertical stepper marker Borders): bug. The markers have no `VerticalAlignment="Top"`, so each one is centred in its 69.6 row (top at about 20). The 2px line starts at y 30 and is declared after the marker, so it paints over the marker. In `wpf-nav-stepper*.png` the check on row 1 and the "2" on row 2 are cut by the line. Fix: add `VerticalAlignment="Top"` to the three vertical markers, re-capture, and check rows and lines against N5 acceptance 5.
3. wpf/MainWindow.Navigation.cs:252-268: bug. The M2 ArrowDown `KeyDown` handlers on `MenuActions`/`MenuMore` and the panel `KeyDown` are wired only in `EnsureMenus()`, which only runs from Click. ArrowDown on a trigger that has never been clicked does nothing. Fix: wire `KeyDown` in XAML on both triggers and both panels, and create the two `DropdownOutsideClick` objects lazily in `OpenMenu`. Add an M2 K test as the first check in the menu block: focus `MenuActions`, PostKey Down, then expect "expanded" and `ProjectMenu_Item0` focused.
4. wpf/MainWindow.xaml:161, 169, 177, 185, 192, 214, 221 (menu item icons): bug, parity. The icons render in Ink: I sampled the pin at (38,36,31) in wpf-nav-menu-open.png. Web `.menu__item svg{color:var(--ink-3)}` gives (107,102,89). Fix: set `Foreground="{StaticResource Ink3}"` on the non-danger icon ContentControls (danger icons stay Danger), then re-capture the menu states.

## Should-fix
1. wpf/MainWindow.xaml:6: missed-step / ruling (a). Rejected: the Window-root `Loaded="Navigation_Loaded"` sits outside the nav blocks, and an in-block alternative exists. Add `Checked="Accordion_Changed" Unchecked="Accordion_Changed"` on the six accordion ToggleButtons; `Accordion_Changed` already null-guards `FindName` during init. Then delete `Navigation_Loaded`, `EnsureAccordions` and `_accordionsWired` (wpf/MainWindow.Navigation.cs:104-107 and 211-225).
2. wpf/MainWindow.xaml:659-667 (tabs): bug. `TabProfile IsChecked="True"` fires `Tab_Checked` during init. The guard at wpf/MainWindow.Navigation.cs:113 returns early, so all three tabs stay tab stops until the first click, which breaks X1's single tab stop. Fix: set `IsTabStop="False"` in XAML on TabSecurity and TabBilling.
3. wpf/MainWindow.Navigation.cs:343-350: bug. Shift+Tab takes the Tab path and moves focus Next from the trigger. M5 says Shift+Tab closes the menu and focuses the trigger. Fix: if `Keyboard.Modifiers` has Shift, call `CloseMenu(true)`.
4. wpf/MainWindow.Navigation.cs:353: bug. Up with nothing focused goes to the last item. The web (index.html:3154) and M3 send it to the first. Fix: `index < 0 ? 0 : ...` for Up as well.
5. wpf/MainWindow.Navigation.cs:194-198: bug (W18 is now fixed in the web). ItemStatus "current"/"todo" is set only on `Step<i>_Check`, which is Collapsed for those states, so UIA can never see "current". XAML also has no initial status. Fix: put the status on `Step<i>_Num` too, or on the step container, with initial values in XAML.
6. wpf/MainWindow.Navigation.cs:203-207: bug. Focus is read after `IsEnabled=false`. The web reads `activeElement` before disabling (index.html:3347). Fix: capture `StepPrev.IsKeyboardFocused`/`StepNext.IsKeyboardFocused` before setting IsEnabled.
7. wpf/Popover.cs:128-131 and wpf/Themes/Navigation.xaml:307: bug, parity. MouseLeave hides the bubble while the host still has focus, and LostKeyboardFocus hides it while the host is still hovered. T1 says hide only when neither holds. The current web also has a hoverable bubble with a 9px bridge (index.html:423, 426-430), but here `TipBubble` is `IsHitTestVisible=False` and is hidden on leaving the host. Fix: hide only if `!host.IsKeyboardFocusWithin && !host.IsMouseOver && !bubble.IsMouseOver`. Make the bubble hit-testable and add a MouseLeave on it. This is C-level, so no test is needed.
8. wpf/tests/Tests.Nav.cs:140-162: missed-step (vacuous test). A1/A3 assert only ToggleState, which a ToggleButton flips on its own. `AccMulti_Panel0` is never collapsed, so its check passes regardless. Because of this, blocking 1 shipped green. Fix: assert `AccMulti_Panel1` is found after the toggle and gone after the `finally`. Restore `AccSingle_0` to On in the `finally` (A2 leaves it off). Mutation-check this by commenting out the Visibility line at Navigation.cs:234.
9. wpf/tests/Tests.Nav.cs:256-289: missed-step. N2 lists "Tab (VK_TAB) closes", and that check is missing; M2 is also untested (see blocking 3). The `// Covered: action menu (M1-M6)` header (line 1) overclaims: correct it to the branches that are actually tested once these are added.
10. wpf/tests/Tests.Nav.cs:72-73: missed-step (vacuous test). "B1 second click ignored" cannot fail: without the `_working` guard, a second click rebuilds the same "Working…" name. Either assert something the guard changes, or drop the check and note the guard as C.
11. wpf/MainWindow.Navigation.cs:45-69, 95: ruling (c). Do it. The web spinner always rotates, and Decision 8 ports it as functional; captures are unaffected because of `IsCapture`. Name the XAML spinner (MainWindow.xaml:116), start it from a `Loaded` on `BtnLoading` (inside the block), and call `Spin(true)` after the Tick rebuild. Then the stop branch can go.

## Nits
1. wpf/MainWindow.xaml:1032-1044 and wpf/Popover.cs:127: nit. HelpText is set twice, in XAML and in `OnTipChanged`. Keep one.
2. wpf/Popover.cs:125: nit. The handlers are attached inside `Loaded`, which re-fires if the element is re-parented, so they could be added twice. Attach them directly in `OnTipChanged`.
3. wpf/MainWindow.Navigation.cs:97-99: nit. Use `ClearValue` for Opacity, Cursor and Tag instead of hard-coding the style values.
4. wpf/tests/Tests.Nav.cs:39: nit. `Check("primary button invokes", true)` is a pure smoke test. That is acceptable under B2, but it cannot fail.

## Rulings
- (a) Window-root `Loaded` hook: rejected (should-fix 1).
- (b) The ~1.3px menu-panel left-edge delta is accepted. The panel's right edge equals the trigger's right edge (both at x 461 in wpf-nav-menu-open.png), and the trigger is 106 wide against the web's 107.3, which is inside the ±2 button tolerance. The delta is inherited from the button width, not a positioning bug.
- (c) Idle spinner rotation: do it (should-fix 11).

## Checked and fine
- N1: Rotor `RenderTransformOrigin` is set, B1 is aria-disabled with focus kept and the dead guard removed, `Cursor=No` is on the disabled trigger, BtnDisabled is asserted, and the COMException is replaced by a retry plus a "delivered" check.
- Popover is lazy, spans the window columns, sits at Z 50, uses `TransformToVisual`, and flips and clamps like web position(). Menus reuse DropdownOutsideClick, and Escape/End/wrap/item-click match the web. Old Crumb* styles and old nav ids are gone, and no Popup/ToolTip/Expander/TabControl remains in the nav blocks.
- The MainWindow.xaml hunk at old line 401 (`PagerId="DataPageSize"`) is not nav's (tables), so it was ignored.
