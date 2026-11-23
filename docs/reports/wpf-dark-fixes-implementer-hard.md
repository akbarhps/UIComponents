# WPF dark mode: final-review fixes (agent implementer)

Source: docs/reports/wpf-dark-final-reviewer.md. Not committed.

## Files changed
| file | change |
|---|---|
| wpf/ThemeManager.cs | MED1: `Apply(bool force)`; `Apply(false)` (System event) returns early when the resolved theme is unchanged; dictionary added/removed only when membership changes (`dark && !has` / `!dark && has`). `Initialize` and `SetTheme` call `Apply(true)` (always raise ThemeChanged, Mode may have changed). MED3: `LoadDark` builds an absolute sibling URI from `((IUriContext)d).BaseUri` (fallback `pack://application:,,,/`); `Initialize` calls `LoadDark` eagerly in every mode. LOW: after setting the DWM attribute, `SetWindowPos(FRAMECHANGED)` once dark has ever been used. |
| wpf/Themes/Forms.xaml, Dropdown.xaml, Controls.xaml | MED2: `CaretBrush={DynamicResource Brand}` on FieldInput, FieldInputError, FieldTextArea, DropdownSearch, TableFilterInput (FieldInputSearch inherits FieldInput) |
| wpf/Themes/Dropdown.xaml (BareInput), Overlays.xaml (CmdkInput), Sheet.xaml (SheetTextCell) | MED2: CaretBrush Ink -> Brand (web `*{ caret-color: var(--brand) }`, web/index.html:74, verified) |
| wpf/Shimmer.cs | LOW: `BandColor` uses `TryFindResource ... as SolidColorBrush`, null-guards `Application.Current`, falls back to light `#A6FFFFFF`; Shimmer transparent stops carry the allow marker |
| plan/wpf/theme-lint.mjs | LOW: C5 regex now `Color.From(Argb|Rgb|ScRgb)` and `Colors.<non-Transparent>` (the old `FromA?Rgb` never matched Argb); new C6 `thememanager` shape check |
| wpf/tests/run-hidden.ps1 | LOW: saves the caller's `UI_THEME`, restores it in finally (removes only if it was unset) |
| plan/wpf/REVIEWER.md, wpf/COMPONENTS.md | Record the two intentional light changes (caret black -> Brand, FileInput title black -> Ink), correct the wrong "web caret = ink" note, document the ThemeManager contract |

CaretBrush coverage: the 5 code-built TextBoxes (Autocomplete, CalendarPicker, MultiSelect, SearchableSelect, SheetGrid) all assign one of these styles. `ClaudeInput` (legacy) untouched.
Line endings: the sed/Edit-touched xaml/cs/mjs files are LF in the working copy (they were before); git autocrlf normalises, the diff is unaffected.

## Checks
| check | result |
|---|---|
| `wpf\build.bat implementer` | Built, 0 errors |
| `node plan/wpf/theme-lint.mjs` | C0-C6 violations=0, `RESULT theme-lint failed=0` |
| Light byte gate: `theme-captures -Theme light` (36 sections) vs `%TEMP%\theme\p4-light` | 36 captured, 0 files differ (byte-identical; run twice, before and after the IUriContext edit) |
| Dark startup (`-Theme dark`) vs runtime switch (`-Theme light` + `Sections!ThemeDark`) | 0 differing px; also 0 px vs earlier p3-dark and p3-rt-dark references |
| Full suite (final build) | `RESULT  passed=532  failed=2` (twice, same two) |

Full-suite failures, both environmental, not from this change:
- `FAIL grid copy column TSV - info=3 cells selected expected=[Small\n\nLarge] actual=[]` and `FAIL F1 grid Copy raises Copied toast`: clipboard read returns empty in this session. Proof: a worktree build of unmodified HEAD (195561f) fails the identical `grid copy column TSV` check on `-Only grid` (`passed=14 failed=1`), as does `-Only grid` on my build. Both are clipboard checks and the clipboard is shared with the user session.
- First full run on the pre-IUriContext build: `FAIL area dropdown threw InvalidOperationException: Unsupported Pattern.` (the known flake; area aborted, `passed=456 failed=1`). It did not recur in the next two runs.
- Remaining 532 pass. `DRIVER fast=281 fallback=4` (the same 4 fallbacks: `select:<no id> x2`, `toggle:RowCheck x2`; no new ones).

## Deviations
1. Title bar: still set on `Window.Loaded`; there is no routed SourceInitialized hook in WPF without a larger change. Mitigation instead: `SetWindowPos(FRAMECHANGED)` after the attribute, only once dark has been used (light-only sessions never touch the frame). Manual check only; not verifiable on the hidden desktop.
2. `TableFilterInput` (tables area) and `DropdownSearch` (dropdown area) also got the Brand caret, plus FieldInputError and FieldTextArea, for parity; all are shared styles, edit is one setter each.
3. C6 is a text-shape lint (it checks that Initialize calls LoadDark, LoadDark uses IsAbsoluteUri + IUriContext, Apply has the early return and the membership guard, the event handler calls Apply(false)), not a behavioural test. MED1 (no remove/re-add on a System event) and MED3 (missing Tokens.Dark.xaml fails at startup; consumer in a subfolder) are code-only: a UserPreferenceChanged burst and a missing/relocated dictionary cannot be driven hidden.
4. `Apply(true)` on SetTheme always raises ThemeChanged even when the resolved theme is unchanged (e.g. System -> Dark on a dark OS), because the showcase toggle syncs on Mode. Only System-driven applies are suppressed.
5. Skipped as instructed: OverlayHost.SetPageBlur effect restore (pre-existing). Not done: StorageKey "" treated as null, Initialize null guard parity (notes only in the review).
