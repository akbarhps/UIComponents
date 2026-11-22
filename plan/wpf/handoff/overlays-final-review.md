# overlays final review (O2 fixes + O3-O6): FIX-FIRST

Suite not run (per instruction). Verified by reading the diff, the plan 07 tables, and the ref captures.

## Resolved from O1/O2
- Bar clip: `OverlayControls.xaml:523` has `ClipToBounds="True"`. OK.
- Proxy button: removed. `OverlayHost.cs:44` uses only the scrim `MouseLeftButtonUp`. No `OverlayScrimClick` left anywhere. OK.
- Timer lifecycle: stopped and replaced on open (`OverlayControls.xaml.cs:464`). Tick checks `IsOpen(LoadingDialog)` (488). `IsVisibleChanged` stops the timer and the motion on every close path (33-36). Under `--capture` the timer is not started (468). OK.
- `FocusVisualStyle={x:Null}`, `TabNavigation`, `HelpText` on LoadingDialog. IconPlus/IconButtonGlyph caps. OK.
- The four `*Window` pairs are deleted. Outside `obj/` there are no references to them or to `BtnCommandPalette`/`BtnDrawer`. The AdvancedControls "Dialogs" block and its handlers are removed. No probe code is left.
- Focus return: `Close()` restores `_previousFocus`. It is kept across dialog switches. Tested for the modal (Escape -> opener).

## Blocking
1. `wpf/OverlayControls.xaml:128`: bug: the palette search row has `BorderThickness="0,0,1,0"`, which is a RIGHT border. The web has `border-bottom`. The capture shows no separator, plus a stray 1px line at the card's right edge. Fix: use `0,0,0,1`.
2. `wpf/OverlayControls.xaml:148`: bug: the palette footer has `BorderThickness="1,0,0,0"`, which is a LEFT border. The web has `border-top`. Fix: use `0,1,0,0`.
3. `wpf/Themes/Overlays.xaml:112`: bug: `CmdkChip Padding="3,6"` is a WPF Thickness (L/R 3, T/B 6). The web `padding:3px 6px` means T/B 3, L/R 6. The "ESC" and "Ctrl N" chips render clipped in `wpf-overlays-cmdk-open.png`. Fix: use `6,3`.
4. `wpf/Themes/Overlays.xaml:58-66` + `OverlayControls.xaml:134`: missing-step: the `CmdkInput` template has no placeholder. `Tag="Search for an action…"` is never rendered, and the capture shows an empty input where the web shows the placeholder. Fix: add a Tag-bound placeholder TextBlock to the template, shown when Text is empty (reuse the pattern of the existing FieldInput placeholder).
5. `wpf/OverlayControls.xaml.cs:74-89`: bug (K6): `FindByTitle` returns the first TextBlock in the window with matching text. For "Go to Buttons" that is the sidebar label at `MainWindow.xaml:25`, so the command does not scroll. Fix: search only `SectionTitle`-styled TextBlocks (`text.Style == TryFindResource("SectionTitle")`), or search only inside `ContentScroll`.
6. `wpf/tests/Tests.Overlays.cs:173`: bug (fake pass): "L1 backdrop does not close loading" repeats the Escape assertion. The worker itself documents that `PostMouseClick` (line 166) is a no-op on the hidden desktop. Fix: drop the PostMouseClick and the Check, and print `CODE-ONLY  backdrop ignored while loading`. Fix 5 of overlays-next.md asked for exactly this.

## Should-fix
7. `wpf/tests/Tests.Overlays.cs:164-172`: L1 Escape cannot fail. Escape is posted right after Invoke, before the BeginInvoke focus moves into the layer. The key can route from `OpenLoading`, which is outside `_layer`, so dropping the `_blocking` guard in `OverlayLayer_KeyDown` would still pass. Fix: post Escape after "loading card focused" passes, then assert the dialog is still shown after a short WaitFor (for example, 300 ms of not-offscreen). The 3 s margin allows this.
8. `wpf/tests/Tests.Overlays.cs:328-336`: K3 Down is untested; only Up-wrap is. K4 (Enter runs the active command, plan "K") is untested. K6 (plan "U") is untested, and that is how bug 5 slipped through. Fix: post Down then Enter and assert the "Invite member" toast. Invoke a "Go to Date picker" item and assert the scroll offset changed (or that the target is on screen).
9. `wpf/tests/Tests.Overlays.cs:72`: F3 does not check the toast text `“Atlas”` (plan verify) or the reset after submit. Fix: assert the body text contains `“Atlas”`.
10. `wpf/tests/Tests.Overlays.cs`: no Tab-cycle test for the form modal (overlays-next.md asked to check the Team ComboBox Tab stops before the Tab test). No test for the D7 confirm's scoped Tab cycle (`SetTabScope`, new code at `OverlayHost.cs:160`). Fix: post Tab x N in the form modal and assert it wraps to FmName. Post Tab twice on the confirm and assert it wraps KeepMember -> ConfirmDelete -> KeepMember.

## Nits
11. `wpf/OverlayControls.xaml.cs:420`: duplicates the email regex. Fix: use `EmailOk(email)`.
12. `wpf/OverlayControls.xaml.cs:94-95`: `e.Handled = true` is written twice. Fix: set Handled once, then `if (!OverlayHost.IsAnyOpen) OpenCmdk();`.
13. `wpf/OverlayControls.xaml.cs:46`: the constructor hooks `Application.Current.MainWindow.PreviewKeyDown` and never unhooks. This only works because the only host is MainWindow. Acceptable; a comment would help.
14. `wpf/OverlayControls.xaml:522-523`: `ClipToBounds` clips to a rectangle, not to `CornerRadius="2"`, so the fill shows square ends at the track edges while it slides. Fix: use a `RectangleGeometry` clip with RadiusX/Y=2, as already done for `CmdkCard` (xaml.cs:37-41).
15. Report claims "sprite icons corrected to the web paths", but the Overlays.xaml diff only adds end caps. Fix: correct the report.

Out of scope, not the overlays agent's work: `AdvancedControls.xaml(.cs)` table edits (PagerId, sort-stable `_baseRows`, empty-state text), which belong to the tables round 5.
