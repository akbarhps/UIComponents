# nav N1 review (diff-only; nothing built or run)

Verdict: fix-first (2 blocking, both small).

## Blocking

1. wpf/Themes/Navigation.xaml:30 (ButtonSpinner `Rotor` Grid) + wpf/MainWindow.Navigation.cs:43-67 (Spin): the RotateTransform has no CenterX/CenterY and the Grid has no RenderTransformOrigin, so the live animation rotates around the top-left corner (0,0), not the centre. Captures never see it (IsCapture skips Spin). Fix: add `RenderTransformOrigin="0.5,0.5"` to `Rotor` (it also covers the replacement transform made in Spin). Compare OverlayControls.xaml, which sets CenterX/CenterY=18 on its spinner.
2. wpf/MainWindow.Navigation.cs:67-80 (BtnLoading_Click) and the test at wpf/tests/UIAutomationTests.cs ("B1 loading disabled + Working..."): the worker followed the old plan row B1 (`IsEnabled=false`). The web now uses aria-disabled (W17): ignore clicks, keep focus, show the disabled look. Setting IsEnabled=false on a focused WPF button drops keyboard focus, and re-enabling does not restore it. The `!button.IsEnabled` guard is also dead code (a disabled button never raises Click), so there is no real double-click guard. Fix: keep IsEnabled true. Add a `bool _working` field; return from Click if it is set. While working set `button.Opacity = 0.45`, `button.Cursor = Cursors.No` and `button.Tag = null` (hides the shadow sibling; restore `"Shadow"` in the tick). Reset Opacity, Cursor and Tag in LoadingTimer_Tick and clear `_working`. Opacity on the whole button also dims the focus ring, which matches web (opacity applies to the outline). Update the test: state becomes `"enabled|Working..."` then `"enabled|Loading"`. Add `loading.SetFocus()` before Invoke and assert `HasKeyboardFocus` is still true at "Working...". Add a second Invoke during "Working..." and assert the name is unchanged.

## Should-fix

1. wpf/Themes/Controls.xaml:49-52 (IsEnabled=False trigger): no `Cursor="No"`. Web uses `cursor:not-allowed`, and PrimaryButtonSm/SecondaryButtonSm in Display.xaml already set it. Fix: add `<Setter Property="Cursor" Value="No"/>` (no TargetName) to that trigger.
2. wpf/tests/UIAutomationTests.cs (Buttons block): `BtnDisabled` is only counted among the 13 ids. Assert `!Current.IsEnabled` on it (cheap, no UIA action). The Invoke on `loading` is wrapped in `catch (COMException) { }`, which hides a failed Invoke. The WaitFor catches the symptom, so this is tolerable, but comment why or narrow it.

## Nit

1. wpf/MainWindow.Navigation.cs:14-24: this is a third copy of the capture / reduced-motion guard (Shimmer.cs:31, OverlayControls.xaml.cs:67). The ponytail comment admits it. Fine for now; fold into one helper in the final polish.
2. wpf/MainWindow.Navigation.cs: the idle "Loading" spinner is static, but the web spinner rotates all the time (`.spinner{animation:spin .7s linear infinite}`). Optional: start Spin for the XAML spinner on Loaded, unless IsCapture. The restored content after the timer should do the same.
3. wpf/MainWindow.Navigation.cs:78-86: no Unloaded/Closed cleanup for `_loadingTimer`. It is one-shot and stopped in Tick, so there is no real leak. Ticks are hooked once, so there are no duplicate handlers. Acceptable.
4. wpf/Themes/Display.xaml:97-100 (PrimaryButtonSm, a Q6 file): `Effect=Shadow1` sits on the button itself, so the focus ring also gets a shadow. Not in the N1 diff proper; use a Shadow sibling like Controls.xaml if the ring looks off.
5. wpf/MainWindow.xaml (BtnDefault `MinWidth="82.5"`): acceptable. It is demo-only, commented, and confined to one button. Without it the width is 3.5 under the web, outside the +-2 tolerance.

## Checked and fine

- Controls.xaml 5-69: single template with a shadow sibling, ring margin -4 / border 2 / radius 12, press shift on Root, disabled .45 on Bd with the shadow collapsed, and per-variant hover colours matching web lines 172-181. Large padding 23,15 equals web 22/14 plus the 1px border. I could not isolate other agents' hunks in the shared dirty Controls.xaml. The nav hunks sit in the old lines 5-69 and I saw no edit near DangerButtonSm (line 420, which is untouched, as intended).
- Q6: ring + press shift present on PrimaryButtonSm/SecondaryButtonSm (Display.xaml) and GhostButtonSm (Sheet.xaml). DangerButtonSm untouched, as agreed.
- App.xaml: Navigation.xaml is the last merged dictionary. csproj: the Navigation.xaml Page and the `MainWindow.Navigation.cs` Compile entry (DependentUpon MainWindow.xaml) are present. C# 5 has no violations (no `?.`, `$""`, `nameof` or expression bodies). No BOM.
- 13 ids present, ButtonsCard GroupBox pattern reused. The loading content has the spinner and an 8px gap like web. `AutomationProperties.Name` is kept live. No scope creep seen in nav-owned files. MainWindow.xaml.cs and the Sheet/Display extras in the diff belong to other agents.
- wpf-nav-buttons-working.png matches web-nav-buttons-working.png visually (disabled look, no shadow, spinner ring).
