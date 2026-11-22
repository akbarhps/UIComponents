# nav follow-up report (nav-final-review2 should-fix 1–2)

## Changes
- `wpf/Popover.cs` — tooltip hide grace: `TipMaybeHide` now schedules a 150 ms one-shot `DispatcherTimer` instead of hiding immediately; `TipShow` and `TipHide` cancel it. This is the 9px hover bridge from `web/index.html:426-430`: pointer travel from host to bubble through the gap cancels the hide, and leaving the bubble schedules it again. Escape still hides immediately and clears dismissal on blur/mouseleave.
- `wpf/MainWindow.xaml` — kept `IsTabStop="False"` on `TabSecurity`/`TabBilling`; removed the now-unneeded `AutomationProperties.ItemStatus` mirror values from all three tabs.
- `wpf/MainWindow.Navigation.cs` — removed the "tabstop"/"notabstop" ItemStatus mirror from `Tab_Checked`.
- `wpf/tests/Tests.Nav.cs` — replaced the proxy X1 assertion with a real keyboard check: focus `ProgressReset` (the control before the tab strip), `PostKey(VK_TAB)` → expect `TabProfile` focused, `PostKey(VK_TAB)` again → expect neither `TabSecurity` nor `TabBilling` focused. Added `T1 hover bridge grace`: cache the bubble, move focus away, and assert the bubble is still visible before the grace expires; the existing hide-on-blur check still runs afterwards. Updated the accordion toggle-state comment.

No pixel-affecting edits, so no references were re-captured. No csproj edits, no commits, no mutation code left.

## Build / `-Only nav`
- `wpf\build.bat nav` OK; one build failed in `OverlayControls.xaml` (another agent's mid-edit) and was rebuilt after ~60 s.
- `wpf\tests\build-tests.bat nav nav` OK.
- `pwsh -NoProfile -File wpf/tests/run-hidden.ps1 -Agent nav -Only nav`: `AREAS compiled: nav`, 20.8 s, `PROFILE finds=112 findMs=15538`, `DRIVER fast=26 fallback=0`, no fallback lines.
- `RESULT  passed=73  failed=0`
- NODRIVE sign-off (`UITEST_NODRIVE=1`): `RESULT  passed=73  failed=0`.

## Mutation check
One combined temporary mutation run:
- Removed `IsTabStop="False"` from `TabSecurity` → **`X1 Tab skips other tabs` FAILED** (`X1 Tab enters first tab` still passed, as expected).
- Removed the hide-grace timer (`TipMaybeHide` hid immediately) → **`T1 hover bridge grace` FAILED**.
Run result: `passed=71 failed=2`. Both mutations reverted; the two clean runs above are green.

## Deviations / notes
- The grace check uses a cached `AutomationElement` and `Current.IsOffscreen` rather than a fresh `Find`, because a UIA find costs ~100 ms — longer than the 150 ms grace.
- The hover bridge is code-only (the hidden desktop cannot move the pointer through the gap); the `T1 hover bridge grace` assertion verifies the timing mechanism, not mouse travel.
- No blocking questions.

---

# Round 2 follow-up (nav-final-review3 should-fix 1)

## Change
- `wpf/tests/Tests.Nav.cs` — `"X1 Tab skips other tabs"` is no longer a bare negative check. After the second `PostKey(VK_TAB)`, a new `WaitFor(!tabProfile.Current.HasKeyboardFocus)` proves the async Tab was processed; only then are `TabSecurity`/`TabBilling` checked for focus (`X1 Tab leaves first tab` reports the wait). The T1 grace nit was left as-is (code-only path, noted above).

No product-code or pixel changes; no captures re-taken. No commits, no mutation code left.

## Build / `-Only nav`
- `wpf\build.bat nav` OK; `wpf\tests\build-tests.bat nav nav` OK.
- `-Only nav`: `AREAS compiled: nav`, 16.2 s, `PROFILE finds=116 findMs=10980`, `DRIVER fast=26 fallback=0`, no fallback lines.
- `RESULT  passed=74  failed=0`

## Mutation check
- Removed `IsTabStop="False"` from `TabSecurity`, rebuilt, reran: **`X1 Tab skips other tabs` FAILED** (`passed=73 failed=1`); `X1 Tab leaves first tab` still passed, proving the wait itself is sound and the failure comes from the real tab stop.
- Reverted; final clean run above is green.
- No blocking questions.
