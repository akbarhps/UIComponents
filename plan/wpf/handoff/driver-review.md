# In-process UIA test driver review

Scope: `wpf/App.xaml.cs` (InstallDriver / DriverHook / FindAllById), `wpf/UIComponents.Wpf.csproj` (UIAutomationProvider ref),
`wpf/tests/run-hidden.ps1` (UITEST_DRIVE=1), `wpf/tests/UIAutomationTests.cs` (Drv, Fast*, rewritten call sites, PROFILE).

Reviewer run (own build `rvdrv`, `run-hidden.ps1 -Agent rvdrv`): `RESULT passed=306 failed=1` (only
"auto-dismiss resumes when focus leaves"), `DRIVER fast=123 fallback=9` (select:<no id> x4, toggle:RowCheck x2,
setvalue:SortProject x3), `PROFILE finds=313 findMs=82065`.

Verdict: **keep the driver; today's results are trustworthy.** I found no current wrong result: no fast action hits a
blocking case below. Every Popup in the app is a ComboBox dropdown (`Themes/Controls.xaml:614`, `Forms.xaml:168`,
`Sheet.xaml:295`) whose items have no id, so they all fall back. No sync handler opens a modal. The 3 blocking items are
**latent** and should be fixed before more tests are written on top: the fallback can *repeat an action that already ran*
(#1, #2), or the driver can act on a *different element* than the one the test holds (#3). The design is sound otherwise:
it calls the same peer providers UIA calls, is inert without the env var, and falls back safely on "not resolved".

## Blocking

1. `wpf/tests/UIAutomationTests.cs:1789` + `:1801-1806`: **double action on timeout.** `SendMessageTimeout` returns 0 on
   timeout / hung (`SMTO_ABORTIFHUNG`, 15 s), `Send` maps that to 0, `Do` treats anything != 1 as "not resolved" and runs
   the real UIA call. But the provider call was already delivered and may have run (or still be running): Toggle,
   SetValue, Select, Expand, Collapse are **synchronous** in the hook, so any sync handler that pumps a nested loop
   (a `ShowDialog` in a Checked/TextChanged/SelectionChanged handler, a slow refresh) ends in a second toggle / select.
   Today the three `ShowDialog` sites (`AdvancedControls.xaml.cs:222,228`, `InputControls.xaml.cs:75`) are Button
   `Click` handlers, and `ButtonAutomationPeer.Invoke` queues the click with `Dispatcher.BeginInvoke(Input)`, so invoke
   returns 1 before the dialog opens and nothing blocks now. It is latent, not hypothetical: the next toggle-opens-dialog
   control hits it. Fix: only fall back on result 2. On 0, throw
   (`throw new TimeoutException("driver " + verb + " " + id + " did not return")`), or check
   `Marshal.GetLastWin32Error() == 1460` (declare `SetLastError = true`) and throw; never re-run the action.

2. `wpf/App.xaml.cs:148`: **exceptions after the side effect also fall back and re-run.** `catch (Exception) { }` →
   result 2 → the client re-runs the same action over real UIA. For provider pre-checks
   (`ElementNotEnabledException`, `InvalidOperationException` thrown *before* anything changed) that is right and keeps
   the semantics (real UIA then throws the same exception, ~2 s). For an exception thrown by an *app handler* after the
   state changed (e.g. `RowCheck_Changed` throws after `IsChecked` flipped), the action runs twice and the app bug is
   hidden behind whatever the second run does. Fix: split the result codes. Return 2 only for "could not resolve"
   (no unique id, no peer, no pattern) **and** for `ElementNotEnabledException`. Return 3 for any other exception from
   the provider call, write `ex.GetType().Name + ": " + ex.Message` to `Console.Error` / `Trace` (or keep it in a static
   and add a `lasterror` verb), and in `Drv.Do` throw `InvalidOperationException("driver " + verb + " " + id + " threw in app")`
   on 3 instead of falling back.

3. `wpf/App.xaml.cs:113-114, 152-157`: **identity check is weaker than UIA's, so the driver can act on the wrong element.**
   `FindAllById` searches only `Application.Current.MainWindow`'s visual tree and matches only
   `AutomationProperties.AutomationId`. UIA's AutomationId is `AutomationProperties.AutomationId`, **falling back to
   `x:Name`** (`FrameworkElementAutomationPeer.GetAutomationIdCore`), and the test's element can live in a Popup
   (combo/picker dropdowns, own HwndSource, not reachable by `VisualTreeHelper` from the window). So if the target is in a
   popup, or is identified by x:Name, and *one* other element in the main window carries that same AutomationId, `Count == 1`
   resolves to that other element and the call returns 1. The test then reads state from the element it holds and passes
   or fails for the wrong reason. Fix (about 10 lines):
   - search every `PresentationSource.CurrentSources` root (`HwndSource.RootVisual`), not only MainWindow. That also makes
     popup items fast instead of falling back;
   - match `string aid = AutomationProperties.GetAutomationId(fe); if (aid.Length == 0) aid = fe.Name;`. This makes
     `SortProject` (x:Name only, `AdvancedControls.xaml:172`) fast too (3 of the 9 fallbacks);
   - the client also sends the element's `BoundingRectangle` centre (`el.Current.BoundingRectangle`, screen px), and the
     hook keeps only the matches whose `PointToScreen` rect contains it. That disambiguates template duplicates
     (`RowCheck`, `DataControls.xaml:149`, 2 fallbacks) and confirms identity. Offscreen or empty rect: send no point and
     keep the "exactly one" rule.

## Should-fix

4. `wpf/App.xaml.cs:132-135` (select): item containers never resolve. `ListBoxItem`, `ComboBoxItem` and `TabItem` create
   *wrapper* peers (`ListBoxItemWrapperAutomationPeer`, `TabItemWrapperAutomationPeer`) with no SelectionItem pattern.
   UIA's SelectionItem lives on the `ItemAutomationPeer` that the parent `ItemsControlAutomationPeer` creates. So giving
   the 4 `select` fallbacks (`:432` combo Engineer, `:609` tab, `:636` MultiList item, `:1328` grid Custom) AutomationIds is
   **not enough**: they still return 2. Fix: when `GetPattern(SelectionItem)` is null and
   `ItemsControl.ItemsControlFromItemContainer(fe)` is not null, take that control's peer, call `GetChildren()`, and use the
   `ItemAutomationPeer` whose `Item == ic.ItemContainerGenerator.ItemFromContainer(fe)`. Then the docs rule "give it an id" works.

5. `wpf/App.xaml.cs:101-146`: provider calls run **inside an inter-process SendMessage** (`InSendMessage()` is true). Real UIA
   runs them from a normal dispatcher operation. Inside a sent message, outgoing cross-apartment / out-of-proc COM calls
   fail with `RPC_E_CANTCALLOUT_ININPUTSYNCCALL` (TSF/IME on TextBox focus, clipboard, OLE drag), and `SetForegroundWindow`
   rules differ. Nothing in the current suite hits it (all pass), but it's a new failure mode a real AT can't hit. Cheapest fix,
   which also defuses #1 for good: resolve and validate synchronously (unique element, peer, pattern, `peer.IsEnabled()`
   → else return 2), then `ReplyMessage(new IntPtr(1))` (user32), then run the provider call. Exceptions after the reply
   go to the log/`lasterror` from #2. The client's next UIA read is serialized behind the UI thread anyway.

6. `wpf/App.xaml.cs:142-145` (focus): `win.Activate()` + `FrameworkElement.Focus()` differs from UIA `SetFocus` in two ways that
   matter for tests:
   (a) It succeeds on the hidden desktop, where real UIA SetFocus could not focus. That is why the toast focus tests at
   `UIAutomationTests.cs:1679-1691` and `:1714-1729` now **run instead of printing CODE-ONLY**, and why 2 of the 3 new
   failures appeared. They are new coverage, not regressions;
   (b) the precondition is `Focusable` only. UIA checks `IsKeyboardFocusable` (Focusable && IsEnabled && IsVisible).
   `Focus()` returns false for disabled or invisible elements, so the result is still right, but it then falls back for
   ~2 s and the real call throws, as before. That's fine, just say so in a comment.
   Also: the `Activate()` would steal the foreground on a visible desktop. That is only reachable with UITEST_DRIVE set
   outside run-hidden. Comment it at `:144`.

7. Toast failures are **test timing assumptions the 2 s latency used to hide**, not driver bugs:
   - "auto-dismiss resumes when focus leaves" (`UIAutomationTests.cs:1690`): `Toasts.cs:183` (and web 3465) resumes with
     the *remaining* time. Before, ~4 s of UIA latency between show and focus had used up the 3.6 s budget. Now focus lands
     ~50 ms after show, so ~3.5 s remain and the 1600 ms window fails. Fix: record `shownAt` after `Invoke(bDismiss)` and
     `focusAt` after `Drv.Focus(c)`, then `waitGone(..., 3600 - (focusAt - shownAt) + 800)` and also check it is *not* gone
     after `remaining - 400` ms, so the resume really is a resume.
   - "newest toast at bottom" / "toasts 10 apart" gap=NaN (`:1698-1710`) are a **cascade from the failure above**. When the
     resume check fails, the focus-test toast ("New update available") is still alive with ~1.5-2 s left. The dups block
     selects toasts by Name only (`:1702`), so that leftover becomes `dups[0]`. If it auto-dismisses while it is being
     measured, its peer reports `Rect.Empty` and the gap is `inf - inf` = NaN. If it dies before or after the measurement,
     the check passes, possibly against the wrong pair. That explains 1 fail in one run and 3 in the other. My run had the
     resume failure without NaN (it survived the measurement). Fixes:
     - primary: the resume fix above makes the leftover die inside the focus test;
     - also make the dups block independent of earlier blocks: remember the highest `ToastN` id before
       `Invoke(bDismiss); Invoke(bDismiss);` and keep only toasts with a higher N;
     - secondary hardening: `Invoke` is async (the click is queued at Input priority, for real UIA too). `WaitFor` up to
       1000 ms until both new toasts have non-empty rects (`rectPart(e,"Height") > 0`) before measuring.
     General rule for the docs: after a fast `Invoke`, never read state without `WaitFor`.
   - Audit the other "Invoke then immediately read" sites for the same race. They pass today only because the provider's
     `BeginInvoke(Input)` usually runs before the next cross-process read: `:1563-1565` (`findToast` right after Invoke),
     `:1592-1593`, `:1610-1611`, `:1633-1634`, `:1661-1662`, `:1673-1674`, `:1296-1297` (add row then Find),
     `:1299-1300`, `:543-544` (progress restart). Wrap each first read in `WaitFor(..., 1000)`.

8. Calls that still pay ~2 s each (not routed through Drv):
   - `UIAutomationTests.cs:595` `RangeValuePattern.SetValue` (slider). Add verb `setrange` →
     `IRangeValueProvider.SetValue(double.Parse(arg, CultureInfo.InvariantCulture))`.
   - `:1427`, `:1440` `ScrollPattern.SetScrollPercent`. Add verb `scroll` → `IScrollProvider.SetScrollPercent(h, -1)`.
   - That's 3 actions ≈ 6 s. Nothing else is left: no remaining `.SetFocus()`, `InvokePattern`, `TogglePattern`,
     `SelectionItemPattern` or `ExpandCollapsePattern` action outside the helpers. The remaining `GetCurrentPattern` calls
     (`:95,102,434,497,515,530,544,555,572,578,593,1330,1426`) are reads.
   - The 9 fallbacks at ~2 s each ≈ 18 s: fixed by #3 (SortProject ×3, RowCheck ×2) and #4 (select ×4).

9. `wpf/tests/run-hidden.ps1:49`: `$env:UITEST_DRIVE` is set in the *calling* process. Under `pwsh -File` that's a child
   process (fine). But `.\run-hidden.ps1` or `& run-hidden.ps1` from an interactive pwsh leaves it set for the session, and
   any later manual launch of the exe from that shell installs the driver and its `Activate()`. Fix: set it inside the
   `try` and `Remove-Item Env:UITEST_DRIVE` in the outer `finally`.

## Nit

10. `wpf/App.xaml.cs:109`, `UIAutomationTests.cs:1782`: the separator is a literal TAB inside `'	'` / `"	"`. An editor or
    formatter that converts tabs breaks the protocol silently. Use `'\t'`. Also `Split('\t')` truncates a SetValue
    argument that contains a TAB, and still returns 1 (silently wrong value). Use `Split(new[] { '\t' }, 3)`.
11. `wpf/App.xaml.cs:103-104`: every WM_COPYDATA is claimed (`handled = true`), including foreign ones. Tag the protocol:
    the client sets `dwData = (IntPtr)0x55494454` ("UIDT"), and the hook returns early unless it matches.
12. COPYDATASTRUCT parsing is correct on both bitnesses: `cbData` at offset `IntPtr.Size`, `lpData` at 8 (x86) or 16 (x64,
    4 bytes padding). The system marshals WM_COPYDATA across a 32/64 mismatch. `cbData = text.Length * 2` excludes the
    terminator, and `PtrToStringUni(data, cb / 2)` matches it. Optional hardening: `if (lParam == IntPtr.Zero) return IntPtr.Zero;`.
    No change needed.
13. Leaving the driver in the product exe is acceptable. It is inert without `UITEST_DRIVE=1`: one env read in `OnStartup`
    and no hook, so normal launches are unaffected. It grants nothing a same-desktop, same-integrity process can't already
    do via UIA (UIPI still blocks lower-integrity senders). If you want it out of release builds, wrap `InstallDriver` /
    `DriverHook` in `#if UITEST` and add the define in `build.bat`. Not needed for a demo gallery.
14. `wpf/App.xaml.cs:32`: the hook is installed at `DispatcherPriority.Loaded` after StartupUri creates the window. If
    `PresentationSource` is null at that moment, it silently doesn't install, and the suite runs ~2 s per action with only
    the `DRIVER off` line as the hint. Cheap guard: in `Drv.Init`, retry `ping` for up to 2 s before declaring off.
15. `UIAutomationTests.cs:1684`, `:1720`: the CODE-ONLY messages say "UIA SetFocus cannot focus a control on the hidden desktop".
    With the driver on, these branches now run, and the message only appears when the driver is off. Reword to
    "...(driver off)". The `SetForegroundWindow` at `:1680`/`:1716` is redundant when the driver is on (`Activate()` does it) and harmless.
16. `UIAutomationTests.cs:41-56`: PROFILE only counts `Find`/`FindByName`/`CountByName`. The heavier walks are not
    counted: `CountComboOptions` (`:76`, `TrueCondition` over the whole window + 2 property RPCs per element, 6 calls),
    `FindActiveComboOption` (`:61`, 7 calls), `findToast`/`collectToasts` polling (`:1500`, `:1516`), `FirstGridCell`
    (`:127-130`, 8 calls) and the `FindFirst` calls at `:427,1324,1639,1675,1711`. So the 80 s figure understates search time.
    Wrap those in `FindStart`/`FindStop` too, or time them separately.

## Speedups (estimated, on top of the current 174 s)

| Change | Gain |
|---|---|
| #3 + #4 (9 fallbacks → fast) | ~18 s |
| #8 `setrange` + `scroll` verbs (3 actions) | ~6 s |
| `CountComboOptions` / `FindActiveComboOption`: scope to the combo's list (`Find(root,"ComboAssignee_List")` or the trigger's parent), and use a `CacheRequest` with `AutomationIdProperty`, `IsOffscreenProperty` and `ItemStatusProperty` so `FindAll` returns all values in one RPC instead of 2 per element | ~10-20 s (13 full-window walks with per-element reads) |
| **A or B, not additive (both come out of the same 82 s of profiled finds; combined ceiling about 30-40 s):** | |
| A. In-process `count`/`exists` verb (`count<TAB>idPrefix`, `exists<TAB>id`, answered from the visual tree in <5 ms) for the "is X gone / present" polls (`BulkBar`, `ColEmpty`, `SortEmpty`, `*_Panel`, `waitGone`/`waitPresent`). Caveat: `FindAllById` walks `Visibility=Collapsed` elements too, so the verb must require `IsVisible` (and, for popups, an open source) or it disagrees with the UIA `Find == null` it replaces. Keep at least one UIA `Find` per element so the a11y tree is still checked. | ~15-25 s |
| B. Scope `Find(root, …)` to the section card that was already found (`Find(card, id)`), as done for `ButtonsCard`. Descendant searches cost about the size of the subtree. | ~10-20 s |
| `run-hidden.ps1:99` `Start-Sleep -Seconds 5` → poll for the window (or `Drv.Init` retrying `ping`) | ~3-4 s |
| Toasts: raise persistent + action toasts together and do the single `waitMs(4500)` survival wait once (`:1625`, `:1652`) | ~4.5 s |

What's left in the other ~95 s besides the profiled finds: the **unprofiled** `FindAll` walks (`CountComboOptions`
×6 full-window `TrueCondition` walks, `FindActiveComboOption` ×7, `findToast`/`collectToasts` polling, `FirstGridCell` ×8,
5 direct `FindFirst`, see #16), probably the largest bucket and the cheapest to shrink with CacheRequest and scoping; fixed `Thread.Sleep`s ≈ 10.8 s (71 sites, mostly 100-200 ms after PostKey
or Toggle, many replaceable by `WaitFor`), toast `waitMs` 13 s + partial 3 s + `waitGone` (~4.6 s × 4) that track real 3.6 s
timers, progress run to 100 % (up to ~12 s of real ticks), 5 s startup sleep, ~18 s in the 9 fallbacks, ~6 s in the 3
un-routed actions, and the remaining per-element `.Current.*` RPC reads. The toast and progress waits (~35 s) are product
timing and the floor unless the timers become configurable in test mode (which would be scope creep).

## Docs (plan/wpf/README.md, REVIEWER.md)

- README `:136` is now wrong ("every UIA action … ~95 s of a suite"). Replace it with:
  - `run-hidden.ps1` sets `UITEST_DRIVE=1`. The app then installs an in-process driver (`App.xaml.cs DriverHook`), and test
    actions go through it (~ms instead of ~2 s). Reads (`Current.*`, `Find*`) still go through real UIA.
  - **Rule:** in tests, never call pattern actions directly. Use `Invoke(el)`, `SetValue(el, v)`, `FastToggle.Of(el)`,
    `FastSelectionItem.Of(el)`, `FastExpandCollapse.Of(el)`, `Drv.Focus(el)`. Never `el.SetFocus()` or
    `((XPattern)el.GetCurrentPattern(...)).Action()`.
  - **Rule:** every element a test acts on needs an AutomationId (or x:Name, after #3) that is unique across the window and its popups.
    Duplicates and missing ids silently fall back to the 2 s path.
  - **Rule:** `Invoke` is asynchronous (it is for real UIA too). Never read the result right after it. Use `WaitFor(..., 1000-2000)`.
  - Output: the suite prints `DRIVER  in-process actions enabled` (or `off`) at the start, and `PROFILE finds=N findMs=M`,
    `DRIVER  fast=N fallback=M` and one `DRIVER  fallback <verb>:<id> xN` line per fallback at the end. These are in the
    run-hidden output and in `%TEMP%\uitest-<agent>\tests.log`. A worker must not add new `fallback` lines. Check them before
    reporting done.
  - `UITEST_NODRIVE=1` in the test process forces real UIA for everything (to confirm a result against true UIA).
    Run it once per task before sign-off: the driver is not an accessibility check.
- README `:92` ("Don't use … UIA SetFocus against a visible window"): add "and never set UITEST_DRIVE outside run-hidden: `Drv.Focus` calls `Window.Activate()`".
- README "Who owns what" `:38` and REVIEWER `:141`: add `App.xaml.cs` driver section and the `Drv`/`Fast*` block in
  `UIAutomationTests.cs` to the reviewer's area.
- REVIEWER: add a section "In-process driver" with the design (WM_COPYDATA `verb\tid\targ`, results 1/2/3), the known
  semantic gaps (#5 sent-message context, #6 focus on the hidden desktop), the open fallback list (select ×4 → #4,
  SortProject ×3 / RowCheck ×2 → #3, slider/scroll → #8), and a review checklist item: "new test actions use the helpers,
  no new DRIVER fallback lines, an UITEST_NODRIVE=1 run passes".
