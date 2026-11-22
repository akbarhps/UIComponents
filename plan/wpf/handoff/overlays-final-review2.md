# overlays final review 2 (fix round): CLEAN

I did not run the suite (per instruction). I checked the code, the tests and the capture mtimes.

## Blocking items from review 1: all resolved
1. `OverlayControls.xaml:128`: the search row now has `BorderThickness="0,0,0,1"` (bottom). OK.
2. `OverlayControls.xaml:148`: the footer now has `BorderThickness="0,1,0,0"` (top). OK.
3. `Themes/Overlays.xaml:124`: `CmdkChip Padding="6,3"`. OK.
4. `Themes/Overlays.xaml:58-77`: the `CmdkInput` template has a Tag-bound `Placeholder` TextBlock and a `Text=""` trigger. The test prints CODE-ONLY for it and the capture verifies it. OK.
5. `OverlayControls.xaml.cs:75-96`: `GoToSection` searches from `ContentScroll` (`MainWindow.xaml:38`) and matches only TextBlocks with `Style == SectionTitle`. The sidebar label at `MainWindow.xaml:25` is excluded. All 4 nav titles exist as SectionTitles. OK.
6. `Tests.Overlays.cs:197`: the L1 backdrop check is now CODE-ONLY. The PostMouseClick and the fake Check are gone. OK.

## Should-fix items from review 1: all resolved
7. L1 Escape (`Tests.Overlays.cs:190-196`) is posted after "loading card focused". It then asserts `!WaitFor(IsOffscreen, 300)`, so it can fail. The report says the `_blocking` mutation was run and the test failed. OK.
8. K3 Down (362), K4 Enter (368-375: palette closes and the "Invite member" toast appears), K6 (391-407: scroll percent changes by more than 5 after "Go to Buttons" (item 3)). Each waits on the posted key. The old sidebar-label bug would leave the scroll unchanged, so K6 would fail. OK.
9. F3 (`Tests.Overlays.cs:90-94`) asserts `“Atlas” is ready.`, which matches the source at `OverlayControls.xaml.cs:436`, and checks the reset on reopen. OK.
10. Form Tab cycle (57-69, 7 stops, ComboBox counted as one stop) and confirm scoped cycle (279-285). OK.
Nits 11-14 are done: `EmailOk` is reused (429), Handled is set once (98-103), and `LoadingBar` gets a rounded clip (xaml.cs:37-42).

## Hygiene
- No `Debug.`/`Trace.`/`File.Append`/`File.Write`/TEMP/mutant text in `wpf/**/*.cs` outside `obj/`. The only `Console.WriteLine` calls are the 3 intended CODE-ONLY lines. The temporary GoToSection instrumentation is gone.
- Captures: the `wpf-overlays-cmdk*.png` files (14:59) are newer than `OverlayControls.xaml` and `Themes/Overlays.xaml` (14:20). Plausible.

## Nits (non-blocking)
- `wpf/tests/Tests.Overlays.cs:405`: K6 passes on any scroll change over 5%, in either direction. Buttons is the first section, so assert `scrollAfter < scrollBefore` to make the check point at the target.
- `wpf/OverlayControls.xaml:523`: `ClipToBounds="True"` on `LoadingBar` is now redundant with the code-set rounded `Clip`. Drop it.
- `wpf-overlays-loading-open.png` (14:07) is older than the LoadingBar rounded-clip change. The difference is a 2px corner, so a re-capture is optional.
- `wpf/OverlayControls.xaml.cs:78,84`: the fallbacks (`scroll ?? MainWindow`, `titleStyle == null ||`) would silently bring back the sidebar-label match if either lookup fails. Acceptable.
