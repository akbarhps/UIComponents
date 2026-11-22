# nav: N1 verdict = fix-first, then N2
Review: plan/wpf/handoff/nav-n1-review.md (read it). Rules unchanged: agent "nav", build.bat nav, run-hidden.ps1 only, never commit, never kill by name. Minimise UIA actions (README test speed rule).

## N1 fixes
1. Navigation.xaml:30 ButtonSpinner `Rotor` Grid: add RenderTransformOrigin="0.5,0.5" (spins around top-left now).
2. B1 loading button (MainWindow.Navigation.cs:67-80 + test): the web now uses aria-disabled and keeps focus. Keep IsEnabled TRUE; add a `_working` flag, Click returns early while working; while working set Opacity .45, Cursor No, Tag null (hides shadow); restore all three in LoadingTimer_Tick. Remove the dead `!button.IsEnabled` guard. Test: expect `enabled|Working...` then `enabled|Loading`; SetFocus before Invoke and assert HasKeyboardFocus still true; a second Invoke while working leaves the name unchanged.
3. Controls.xaml disabled trigger: add Cursor="No" (web not-allowed). Stay inside lines 5-69.
4. Tests: assert BtnDisabled !IsEnabled; replace the bare catch(COMException) around Invoke with a re-find-and-retry that fails the check if Invoke still fails.
Nits (optional): idle Loading spinner static vs web always rotates (accept, note it); PrimaryButtonSm Effect on the button gives the focus ring a shadow (check, fix only if trivial).

## Web changes you must now follow (web approved, archived)
Web tooltip/stepper/tabs fallbacks W14-W18 are DROPPED: web fixed them (tooltip z-index above neighbours, one tab stop, Escape dismiss clears on blur/mouseleave, 9px hover bridge, tabs scope, stepper focus, aria-current on static stepper, description spans hidden). Follow the web as it is now and re-read web/index.html + plan/wpf/ref/web-nav-measurements.txt; web-nav-tip-right.png was regenerated (bubble now draws over Save).

## Heads-up
Your button restyle moved other sections' captures +2px (Dialogs "Command palette", Empty state "New project"); that is expected and told to the owners.

## Then N2 (Popover + Action menu) of plan/wpf/06-navigation.md
Do N2 only after the fixes; stop after N2 and report in README format. Q3 capture keys (~Left/~Right/~Home/~End) already exist in App.xaml.cs SendCaptureKey.

## Applies after task 08 (tests split) is done
Read handoff/tests-split.md first (tooling change).
Area `nav`, file `wpf/tests/Tests.Nav.cs`. Fixes 2 (B1 test) and 4 (BtnDisabled, COMException) are in its Buttons block. The legacy Slider/TabControl/Expander/ListBox/Breadcrumbs/Stepper/Tooltip/Action menu blocks are also in `Tests.Nav.cs`; replace them there in N2-N6 (06-navigation.md:151 line refs point to this file). You are the only writer of `MainWindow.xaml` until N7; tell the reviewer before any edit outside the nav sections.
