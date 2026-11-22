# Decisions taken while the user was away (2026-11-22)
User rule: for every open question take the recommended option, log it here, report to the user on return so they can review/reverse.
Not covered by this rule (still need the user): commits, permission prompts, interrupting workers.

| # | When | Question | Decision (recommended) | Why / how to reverse |
|---|---|---|---|---|
| 1 | 2026-11-22 | Start pickers P1 before overlays O1 reported (icons already in Overlays.xaml) | Sent P1 | Icons existed; if overlays changes icon styles pickers needs a small follow-up |
| 2 | 2026-11-22 | Toasts: send T1 fixes and T2 in one prompt | Combined | Same files, saves a round trip |
| 3 | 2026-11-22 | Start planner for group 06 navigation while workers run | Started | Idle workers existed; user asked for it |
| 4 | 2026-11-22 | Tables fix3: faint last-line row, accept as polish or fix? | Fix (reviewer: TableClip + SnapsToDevicePixels), ledger only if still faint | acceptance said no faint row; reviewer found cheap cause |
| 5 | 2026-11-22 | Profiler: pip install comtypes to test native UIA client? | NOT done (changes the user's Python env, needs OK) | ask user on return |
| 6 | 2026-11-22 | 06 Q1 workers | One worker 
av, started N0 | five workers already share 4 slots |
| 7 | 2026-11-22 | 06 Q2 web fixes first | Unarchived web, sent W14-W18 (W19 only if one-liner, W20 replicated) | recommended; before N3/N5/N6 |
| 8 | 2026-11-22 | 06 Q3 capture keys | I added Left/Right/Home/End to SendCaptureKey in wpf/App.xaml.cs (4 lines) | recommended; reviewer-owned file |
| 9 | 2026-11-22 | 06 Q4 delete Slider + multi-select ListBox (no web counterpart) | YES, nav does it in N7 | recommended |
| 10 | 2026-11-22 | 06 Q5 sidebar nav + masthead | Separate later task, not planned yet | collapsing sidebar changes the grid columns overlays/toasts span; revisit after they finish |
| 11 | 2026-11-22 | 06 Q6 small buttons ring/press shift | nav does Primary/Secondary/GhostButtonSm in N1; DangerButtonSm in polish | recommended |
| 12 | 2026-11-22 | 06 Q7 transitions | Skipped (spinner only) | same as 07 Q2 |
| 13 | 2026-11-22 | 06 Q8 breadcrumb links scroll to sections | YES (~10 lines) | recommended |
| 14 | 2026-11-22 | N1 button restyle shifts full-size button widths +2px in toasts/overlays/pickers/tables/progress captures | Tell those workers before N1 lands (between their phases) | plan authorises it; do NOT start N1 until the five are between phases |
| 15 | 2026-11-22 | README had Test speed rule twice | Removed one, added the 2 s per UIA action note | |
| 16 | 2026-11-22 | Tables round 4: approve with faint rows in polish ledger, or third review? | Approved, no third review, archived tables | reviewer's fixes all applied/verified in file; remaining nits are the shared fractional-border family (polish.md). Reverse: unarchive tables |
| 17 | 2026-11-22 | Start N1 (button restyle) now, though no worker is between phases (supersedes #14) | Sent N1; the running workers get a one-line heads-up in their next prompts (dropdown, toasts, overlays, pickers) | all are mid-phase most of the time; shifts are +2px toward the web and plan-authorised. Reverse: ask nav to revert Controls.xaml 5-69 |
| 18 | 2026-11-22 | Overlays O1 review: delete the 6 unused icons or keep? | Keep and fix them (nav reuses IconPlus/More/Download/Chevron/IconButton), combined O1 fixes + O2 in one prompt | nav plan Decision 10 reuses them |
| 19 | 2026-11-22 | Web nav fixes review (tooltip bugs 1-3, nits): fix all except mouse-only Escape | Sent web-fixes-nav2.md; web-nav-tip-right.png will be regenerated to show the fixed state | recommended by reviewer |
| 20 | 2026-11-22 | N1 B1 loading button: web now uses aria-disabled (focus kept) instead of disabled | TODO tell nav: keep IsEnabled true + ignore clicks while working + disabled look, so focus is not lost (replaces plan IsEnabled=false) | follows the fixed web; pending message to nav after its N1 report |
| 21 | 2026-11-22 | Web nav fix round 2 result | Verified 43/43 myself, approved, archived web (no extra reviewer: fixes are scoped CDP-tested) | Reverse: unarchive web |
| 22 | 2026-11-22 | User: let workers finish, improve test suite first | Reviews written to plan/wpf/handoff/<worker>-next.md, NOT sent; workers stay idle until user says | user instruction |
| 23 | 2026-11-22 | Tables round-4 diff review (reviewer, no suite) | fix-first (1 blocking test, 2 should-fix); prompt in handoff/tables-next.md, NOT sent, tables stays archived | send later: unarchive tables then tell it to read the file |
| 24 | 2026-11-22 | Nav N1 phase review | fix-first (spinner origin, loading button must keep IsEnabled true = row 20 applied in file); prompt incl. N2 + dropped W14-W18 in handoff/nav-next.md, NOT sent; nav idle at 20% | send later: tell nav to read the file, then start wait |
| 25 | 2026-11-22 | Dropdown Phase 3 fix-round review | fix-first (vacuous M7 test, stale captures, redundant UpdateTrigger); prompt + Phase 4 in handoff/dropdown-next2.md, NOT sent; dropdown idle 43% | send later |
| 26 | 2026-11-22 | Toasts T1/T2 phase review | fix-first (focus/resume tests vacuous, reviewer says focus claim wrong); prompt + T3 in handoff/toasts-next.md, NOT sent; toasts idle 29% | send later |
| 27 | 2026-11-22 | Overlays O1fix+O2 phase review | fix-first (clip on bar, remove test-only proxy button from product code, timer lifecycle); prompt + O3 in handoff/overlays-next.md, NOT sent. Overlays was accidentally archived by user, I unarchived it | send later |
| 28 | 2026-11-22 | Pickers P1fix+P2 phase review | fix-first (5 blocking: swapped card rows, shadow slab 450 wide, IsSelected mode-aware, Render focus scope, mouse-press focus restore); prompt + P3 in handoff/pickers-next.md, NOT sent | send later |
| 29 | 2026-11-22 | Test split Phase 1: P10 isolation failure; -Only order | Phase 1 stays a pure move; P10 fix recorded in pickers-next.md; -Only keeps fixed full-run order (accepted) | pickers owner fixes later |
| 30 | 2026-11-22 | Test split Phase 5: plan says the csproj instruction in task files is covered by one README line; the user prompt asks for every task-file mention | Fixed the csproj mentions in 01, 02, 03, 05, 06, 07 (no entries, wildcards). README hard constraint 2 and REVIEWER say the same | reviewer flagged the duplicate-Page risk before workers resume. Reverse: git diff of those task files |
| 31 | 2026-11-22 | Test split Phase 5: plan says workers run a full build and full run before their final report; user prompt says full run only by the reviewer | Wrote reviewer-only full runs (wpf README rules, Writing tests, tests-split.md). Workers use -Only per area | fewer full runs. Reverse: restore the plan line in Writing tests "Commands" and tests-split.md |
| 32 | 2026-11-22 | Test split Phase 5: `// Covered:` header content per area file | Old README Covered line split by area; added block names it lacked (searchable select, multi-select, loading modal). Added as comment-only lines, byte-checked | reverse: delete line 1 of each Tests.<Area>.cs |
