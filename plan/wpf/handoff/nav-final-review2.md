# nav final review 2 (re-review of the final-review fix round)

I reviewed the code and captures only. I did not run the suite.

**Verdict: clean (0 blocking).** Two should-fix items can go in a follow-up, or with the next nav touch.

## Prior items verified resolved
- Blocking 1: both accordion Borders have no fixed height (MainWindow.xaml:695). In `wpf-nav-accordion-toggled.png` the card is 984x370 and "Billing and plan" is fully visible.
- Blocking 2: the vertical markers have `VerticalAlignment="Top"` and `Panel.ZIndex="1"` (xaml:999/1012/1025). In `wpf-nav-stepper.png` the check, the "2" and the "3" are not cut by the line.
- Blocking 3: `KeyDown` is set in XAML on both triggers and both panels (xaml:138/148/159/212), `DropdownOutsideClick` is created lazily in `OpenMenu`, `EnsureMenus` is gone, and the M2 test runs first in the menu block.
- Blocking 4: the non-danger icons use Ink3 (xaml:165-226). The recaptured `wpf-nav-menu-open.png` shows grey icons and a red Delete.
- Should-fix 1: grep finds no `Navigation_Loaded`, `EnsureAccordions` or `_accordionsWired`. All six toggles have XAML `Checked`/`Unchecked`.
- Should-fixes 2-6, 8, 10 and 11 are resolved as the report says: tab IsTabStop, Shift+Tab -> `CloseMenu(true)`, Up -> 0, Num ItemStatus with XAML initial values, focus captured before IsEnabled, the accordion panels are asserted with an AccSingle_0 restore, the vacuous B1 check is gone, and the idle spinner is named with `Loaded` and `Spin()` called after the Tick rebuild.
- Should-fix 9: the M5 Tab-close test is added and the header is corrected.
- Nits 1-3 are applied.
- The grep finds no mutation or temp code. The captures are at 13:01. Navigation.cs and xaml were last edited at 13:03-13:04, which matches the reported mutation-revert run. The current source matches what the captures show.

## Should-fix
1. wpf/Popover.cs:95-98, 126, 148 + wpf/Themes/Navigation.xaml:306-319: bug, parity (C-level). The bubble sits 9px from the host with no bridge. The host's `MouseLeave` fires in the gap while `bubble.IsMouseOver` is still false, so the bubble hides before the pointer can reach it. That makes the "hoverable bubble" from should-fix 7 unreachable, and web index.html:426-430 has a 9px `::before` bridge. Fix: add a transparent hit-testable 9px strip on the host-facing side of the TipBubble template, sized per side or a uniform transparent `Margin`/`Padding` with Popover offsetting by -9. Another option is to defer `TipMaybeHide` by one dispatcher tick or about 100ms.
2. wpf/tests/Tests.Nav.cs:96-99 + wpf/MainWindow.xaml:666-672: missed-step (proxy assertion). "X1 single tab stop" asserts the ItemStatus strings that are hard-coded in XAML. It does not check `IsTabStop` itself, so if someone deleted `IsTabStop="False"` on TabSecurity/TabBilling the test would still pass. The harness already sends VK_TAB (M5). Fix: focus the element before the tab strip, PostKey Tab, and expect TabProfile focused. PostKey Tab again and expect TabSecurity *not* focused. After that, the ItemStatus mirror (MainWindow.Navigation.cs:116-119) and its XAML values can be removed.

## Nits
1. wpf/tests/Tests.Nav.cs:137-138: nit. The comment says the test asserts toggle state only, but the checks now also assert that the panel peer is present or absent. Update the comment.
2. wpf/MainWindow.Navigation.cs:116-119: nit. "tabstop"/"notabstop" ItemStatus is test-only state that screen readers will announce. If should-fix 2 lands, drop it.
