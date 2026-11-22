# Phase 2 review (csproj wildcards + MainWindow partials)

Reviewer: phase-reviewer. Method: read-only (diffs, file reads, handoff evidence). No builds or suites were run.
Verdict: **approved** (no blocking findings; one should-fix is a doc/worker-risk item that must land before workers resume).

## Blocking

none

## Should-fix

- `plan/wpf/README.md:74`: hard constraint 2 still tells workers "every new `.xaml` needs a `<Page Include>` entry and every new `.cs` a `<Compile Include>` entry". With the wildcards, an explicit entry duplicates the wildcard item. A duplicate `Compile` only warns (CS2002), but a duplicate `Page` can make MarkupCompile emit the same `.g.cs`/baml twice (I could not verify this without a build). Task files 03/05/06/07 repeat the instruction (`03-dropdown.md:65`, `05-display.md:48,428`, `06-navigation.md:150`, `07-overlays-pickers.md:165`). This is Phase 5 item 1 in the plan, but it should land (or the workers be told) before any worker resumes. Fix: do the README line now: "files in `wpf\` and `wpf\Themes\` are picked up by wildcard; do NOT add csproj entries; a new resource dictionary still needs its `App.xaml` merge line".

## Nit

- `wpf/UIComponents.Wpf.csproj:31-32`: the two `UIAutomationProvider`/`UIAutomationTypes` `<Reference>` lines are unindented. They are not Phase 2 work (they come from the earlier driver work, `driver-review.md:3`, and `App.xaml.cs:121-142` needs `IToggleProvider` etc.), so leave them, but fix the indentation next time the file is touched.
- `wpf/MainWindow.Forms.cs:1-7`: carries all seven `using` lines although it needs none of them (plan-mandated "same using lines"; csc does not warn). Harmless.
- `wpf/MainWindow.Display.cs:11`, `MainWindow.Forms.cs:11`: `public partial class MainWindow` without `: Window`. Matches `MainWindow.Navigation.cs:11`, fine.

## What I verified

### 1. csproj (`wpf/UIComponents.Wpf.csproj:34-43`)
- Patterns are exactly the plan's: `ApplicationDefinition App.xaml`; `Page *.xaml` Exclude `App.xaml`; `Page Themes\*.xaml`; `Compile *.cs`; `Compile Properties\AssemblyInfo.cs`. All are non-recursive, so `bin\`, `obj\<agent>\*.g.cs`, `tests\*.cs` are not picked up. `*.cs` does not overlap `Properties\AssemblyInfo.cs`.
- `App.xaml` is excluded from Page (it stays ApplicationDefinition only), so no duplicate `App.g.cs`. App.xaml.cs is compiled via `*.cs`.
- Nothing missing: old explicit list (HEAD) is a subset of the wildcard result, plus everything newer workers added (`Themes\Display/Dropdown/Navigation/Overlays/Pickers/Toasts.xaml`, `OverlayControls`, `ToastControls`, the `.cs` helpers). `split-p2-csc-before.txt` vs `split-p2-csc-after-csproj.txt` (captured 10:51:09, before the split at 10:51:45) are identical, and `split-p2-obj-*` (before / after-csproj / after) are identical (33 `.baml`/`.g.cs` entries), so the item set did not change with the csproj edit. `csc-after` differs only by `MainWindow.Display.cs` and `MainWindow.Forms.cs`, as expected.
- Deleted `DialogWindow*`/`LoadingWindow*` are not listed in the csc output (the old explicit entries would have broken the build; the wildcard handles it).
- Lost metadata: `Generator=MSBuild:Compile` and `SubType=Designer` are redundant for `Page` items (the WinFX targets compile every `Page` item). `DependentUpon` only affects Solution Explorer nesting (`Foo.xaml.cs` now shows as a sibling, not a child; no effect on the build, the XAML designer or `x:Class` resolution). `ApplicationDefinition` is kept. A VS/IDE edit may rewrite wildcards into explicit items; harmless.
- Per-agent builds: `build.bat` passes `/p:OutputPath=bin\<agent>\ /p:IntermediateOutputPath=obj\<agent>\`. The wildcards are relative to the project directory and non-recursive, so they are not affected by those paths (evidence: `split2` agent build in the csc lists).
- Stray files in `wpf\` (ls -a): no hidden, backup (`.bak`, `.orig`) or scratch `.cs`/`.xaml`. The only non-source entries are `README.md`, `wpf-preview.png`, `build.bat`, `.gitignore`. `Themes\` has only the 11 dictionaries, all loaded before.

### 2. MainWindow split
- `split-p2-move-proof.txt`: original working-tree file was 169 lines (not recoverable: HEAD is 96 lines, older). The moved ranges 13-67, 86-91, 121-127 (68 lines incl. 3 dropped blanks) match the plan; diff of moved bodies = 0 lines; kept lines equal; reconstruction equals the original; UTF-8 no BOM, LF (same as the original). I re-read the current files: Display holds the 3 fields, `ProgressStart_Click`, `ProgressTimer_Tick`, `ProgressReset_Click`, `SetProgress`, `ApplyReducedMotion`; Forms holds `MessageGrip_DragDelta` with both comments; Main keeps constructor, `ApplyStandaloneMode`, `ProjectRow`, `StandaloneHelper`.
- XAML handlers resolve: `MainWindow.xaml:196` `DragDelta="MessageGrip_DragDelta"` (Forms.cs:15), `:526`/`:528` `ProgressStart_Click`/`ProgressReset_Click` (Display.cs:18/44). The constructor's `ApplyReducedMotion()` (xaml.cs:27) resolves in Display.cs:70. No other file references the moved members (grep), and no member names collide with `MainWindow.Navigation.cs` (unchanged, own `IsCapture`, `_loadingTimer`, `_spinner`).
- Ownership matches the plan: display (progress, skeleton motion cue), forms (textarea grip). Skeleton cue `LoadingCue` is Display-owned, fine.

### 3. Full-run compare and remaining risk
- `split-p2-names-diff.txt`: 307 names in both, IDENTICAL; RESULT 306/1 in both. The one failure (`auto-dismiss resumes when focus leaves`) is also in `split-baseline-1/2.txt` and `split-phase1-full.txt`, so it is pre-existing, not a Phase 2 effect. DRIVER counts (fast=126, fallback=6) unchanged. 24 progress checks and `skeleton found by id`/`motion cue hidden` pass, covering the moved Display code. `MessageGrip_DragDelta` has no automated check (nit-level; moved verbatim).
- Other workers: no `wpf/` file other than the three MainWindow files and the csproj has an mtime after 10:15 (App.xaml.cs, last), nothing is mid-edit; no TODO/FIXME/conflict markers in `wpf/`. So nothing of theirs was lost in the split. Residual risks for them: (a) the stale README/task instructions above (explicit csproj entries now duplicate the wildcard); (b) the plan's line references into `MainWindow.xaml.cs` (e.g. `ApplyStandaloneMode` `:94-119`, `:100-115` in Phase 3) are shifted; Phase 3 will have to re-locate by name (current `ApplyStandaloneMode` is at 31-56, and still lists the hard-coded panel array without any `PickerControls`); (c) any worker with an editor buffer of the old 169-line `MainWindow.xaml.cs` will overwrite the split on save and cause CS0111 duplicates: tell them to reload.
