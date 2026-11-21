# 05 — Display: cards, badges, alerts, avatars, skeleton, progress, empty state (agent "display")

There are early, unplanned WPF versions of most of these sections in `MainWindow.xaml`. They use the shadowed `ClaudeCard`, merge sections the web keeps apart ("Cards & badges", "Progress & tabs"), are missing variants, and draw icons as text glyphs. This is a **fix-up audit**: each section is rebuilt in place against the web, using new shared styles in a new `Themes/Display.xaml`.

**Source of truth:** `web/index.html`, line numbers as of 2026-11-21.
- **CSS:** shell 142–157, `.btn`/`--sm` 160–184, `.empty-state` 215–221, `.card` 357–360, `.badge` 363–374, `.alert` 377–385, `.avatar` 388–399, `.skeleton`/`.sk-*` 496–509, `.progress` 512–515, `.motion-cue` + reduced motion 868–873.
- **Icons** (sprite, 24 viewBox, stroke currentColor, width in brackets): `i-card` 884 (1.8), `i-alert` 887 (1.8), `i-check` 889 (2.4), `i-close` 890 (2), `i-search` 891 (1.9), `i-info` 893 (1.9).
- **Markup:** `#cards` 1344, `#badges` 1363, `#alerts` 1378, `#skeleton` 1391, `#progress` 1419, `#empty-state` 1433, `#avatar` 1575.
- **JS:** progress only, 3234–3263. The other six sections have **no JS**.

**References** in `plan/wpf/ref/` (2026-11-21, 1400×1100, scale 1):
- `web-display-<id>.png`, for `cards`, `badges`, `alerts`, `avatar`, `skeleton`, `progress` and `empty-state`, idle. The skeleton shot has the shimmer overlay hidden, so it shows the static base.
- `web-display-progress-64.png`: the fill forced to 64%. Reference for colour and radius only.
- `web-display-skeleton-reduced.png`: Edge with `--force-prefers-reduced-motion`. Use it only for the motion cue and the line colour; the gradient frozen mid-line is a Chromium `.01ms`-animation artefact.
- `wpf-before-display-<cards-badges|alerts|progress|avatar|empty-state|skeleton>.png`: the current WPF state (`Sections`, standalone).
- `web-display-measurements.txt`: every rect, font, colour, radius and padding. `rel` means x,y,w,h relative to the `.demo` card's outer top-left. All numbers below come from it, and the full skeleton line list is only there.

Read `plan/wpf/README.md` first. Rules 4 (hidden desktop, agent name `display`) and 5 (never commit) are mandatory.

---

## Review of the current port

| Section | WPF now (`MainWindow.xaml`) | Web |
|---|---|---|
| Shell (all) | `SectionTitle` + `ClaudeCard` (radius 12, **shadow**, 16px gap). No description, or the 14px `SectionDesc` | Title, then `SheetSectionDesc` (verbatim, 8px gap), then `DemoCard` (radius 16, no shadow) 22px below |
| Cards | One "Cards & badges" section: a single plain card plus 3 badges. The title isn't 600 and the text is shortened | Its own section. `.grid-2` with **two** `.card`s (radius 12, padding 22, 1px `Line`, no shadow): 17px/600 serif title, 14px `Ink2` text, `.btn--sm` foot |
| Badges | Border + TextBlock with **no dot**, 3 variants | Its own section. 6 badges: neutral, brand, success, warning, danger, plain neutral. 24 tall, 6px dot |
| Alerts | 2 stacked alerts in a `ClaudeCard`, with text glyph icons ("i", "✓") and padding 16 | **4** alerts in a 2×2 `.grid-2` (info, success, warning, danger). SVG icons 18px, padding 15/16 |
| Avatars | 3 × 40px, **no border**, 10px gaps. No sm/lg, no avatar-row | Sizes 30/40/52, dark and sage tones, 1px border, 12px gaps, then a divider and an avatar-row (name 14 **700**, role 12 `Ink3`) |
| Skeleton | 3 bars, 14px tall, fixed widths, static gradient, no shimmer | Two `.sk-card`s (circle plus 2 lines, then 3 or 2 lines), a divider, a "Table rows" label, and a table card with 3 rows × 4 lines. Lines 12px, radius 6, animated shimmer |
| Progress | Default-template `ProgressBar` at a **static 64**, sharing a card with `Slider` and `TabControl` | Meta row (label + %), 8px bar, divider, and Start / Reset buttons driving a simulated upload |
| Empty state | One card, the icon in a 42px `BrandSoft` box, shortened text, one button | **Two** `.card`s in `.grid-2`. Bare 22px icon, 20px/600 serif title, text capped at 317px, 2 or 1 buttons |

**Old styles:**
- `Avatar`, `AvatarText`, `ShimmerBrush` and `EmptyIcon` (`Themes/ExtraControls.xaml`) are used **only** by these MainWindow blocks (grepped all of `wpf/`). This task owns them: replace them in `Display.xaml` and delete them from ExtraControls.
- `BadgeBase`, `BadgeSuccess` and `BadgeText` (`Controls.xaml`) are used by `StatusBadgeTemplate` (`tables`). **Don't touch them.**
- `BadgeBrand` and `BadgeNeutral` become unused after Phase 1. Report it; don't delete them.

---

## Files and ownership

| File | What you may do |
|---|---|
| `wpf/Themes/Display.xaml` (**new**) | All new display styles. Owned by `display` |
| `wpf/Shimmer.cs` (**new**) | The shimmer attached property (Phase 4). Owned by `display` |
| `wpf/UIComponents.Wpf.csproj` | Add `<Page Include="Themes\Display.xaml">` (copy the `Themes\Forms.xaml` entry) and `<Compile Include="Shimmer.cs" />` (like `TableClip.cs`). Nothing else |
| `wpf/App.xaml` | Add `<ResourceDictionary Source="Themes/Display.xaml"/>` as the **last** merged dictionary. Nothing else |
| `wpf/MainWindow.xaml` | **Only** the blocks `<!-- Cards & badges -->`, `<!-- Alerts -->`, `<!-- Progress -->` (the "Progress & tabs" title), `<!-- Avatars -->`, `<!-- Empty state -->` and `<!-- Skeleton -->`. Find them by comment or title; the line numbers drift. Forms belongs to `forms`, Data table to `tables` |
| `wpf/MainWindow.xaml.cs` | Add the progress handlers in their own `// Progress bar (05-display)` block. Don't touch the grip handler or `StandaloneHelper` |
| `wpf/Themes/ExtraControls.xaml` | Delete only `Avatar`, `AvatarText`, `ShimmerBrush` and `EmptyIcon`, each in the phase that replaces it |
| `wpf/tests/UIAutomationTests.cs` | Only the display assertions: progress, avatars, empty state, skeleton (find them by their `// ProgressBar`, `// Avatars`, `// Empty state` and `// Skeleton` comments), plus the new ones below |
| `wpf/README.md` | The covered list and the assertion count (Phase 5) |

**Don't touch:**
- `Controls.xaml`. No new tokens are needed: `#ECD4C8`, `#E6EEE7` and `#D3E3D6` live as brushes in Display.xaml.
- `Forms.xaml`, `Sheet.xaml`, `SheetGrid.cs`, `Table*`, `DataControls`, `AdvancedControls`, `InputControls`.

**Reuse, don't copy:** `SheetSectionDesc` and `DemoCard` (Sheet.xaml), `Divider` and `DemoLabel` (Forms.xaml), and `PrimaryButton` / `SecondaryButton` for the 42px buttons.

**Other agents edit MainWindow.xaml, the csproj and App.xaml right now.** Re-read each one immediately before every edit, and make small `Edit`s. Never rewrite a whole file.

**Section titles.** Keep each WPF block where it is; don't reorder, because the blocks in between are other agents'. Titles become the web `h2`: "Cards", "Badges", "Alerts", "Skeleton loader", "Progress bar", "Empty state", "Avatars".
- "Cards & badges" splits into "Cards" and "Badges".
- "Progress & tabs" splits into "Progress bar" and "Tabs". The "Tabs" block keeps the `Slider` and `TabControl` **unchanged** in its `ClaudeCard`: that's a later task, and the slider has no web counterpart.
- Nothing else references the old titles (grepped plan/, docs/, tests, scripts).

## Capture recipe (hidden)

```powershell
wpf\build.bat display
cd wpf\tests; .\build-tests.bat; cd ..\..
pwsh wpf/tests/run-hidden.ps1 -Agent display                       # tests
pwsh wpf/tests/run-hidden.ps1 -Agent display -NoTests -Section "Cards" -Capture "Sections=plan/wpf/ref/wpf-display-cards.png"
# one run per section; output wpf-display-<web id>.png (cards, badges, alerts, avatar, skeleton, progress, empty-state)
```

**Comparing:**
- `Sections` starts at web x≈314.5. Align on the `DemoCard` top-left (the first #E6E2D7 row and column in both images), then compare against the `rel` numbers.
- Pixel scans use System.Drawing (REVIEWER.md): a column scan through an element's centre gives top, bottom and height; a row scan gives left, right and width; `GetPixel` gives fills.
- Report every difference greater than 1px.

---

## Visual spec

All positions are `rel` to the demo card. Demo padding 30 + 1 border puts content at x 31, y 31, width 922.

**Shell (every section):**
- `SectionTitle` with Margin `0,46,0,0`.
- `SheetSectionDesc` with Margin `0,8,0,0` and the verbatim web `<p>` text.
- `DemoCard` with Margin `0,22,0,0`, wrapped in a chrome-free `GroupBox` (the `FormsCard` pattern) with an id: `DisplayCards`, `DisplayBadges`, `DisplayAlerts`, `Skeleton`, `DisplayProgress`, `EmptyState`, `DisplayAvatars`.
- `.grid-2` is a `Grid` with columns `* | 22 | *` (450 + 22 + 450).

### Cards (`web-display-cards.png`; demo height 232.9)

- **`DisplayCard`** (`.card`; also used by the skeleton and the empty state): Border, `Surface` background, 1px `Line`, `CornerRadius` 12, `Padding` 22, **no Effect**. Both cards are 450×170.9, at (31,31) and (503,31).
- **Title:** Serif 17px 600 (see Palatino below), `Ink`, line height 22.1, at (54,54).
- **Text:** 14px `Ink2`, line height 22.4, at y 84.1. Two lines (44.8) at 404 wide.
- **Foot** at y 146.9:
  - card 1: "Open settings", `SecondaryButtonSm`, 108.1×32
  - card 2: "Upgrade plan", `PrimaryButtonSm`, 106.3×32
- **New small buttons in Display.xaml** (Q1). Copy `GhostButtonSm`'s template pattern from Sheet.xaml. Common geometry: padding `12,7`, MinHeight 32, 13px 600, `CornerRadius` 7, `FocusVisualStyle={x:Null}`. Hover changes the background, **never the opacity**.
  - **`SecondaryButtonSm`:** `Surface` background, 1px `LineStrong` border, `Ink` text. Hover: `Surface2` background, `Ink3` border.
  - **`PrimaryButtonSm`:** 1px transparent border, `Brand` background, white text, `Effect=Shadow1`. Hover: `BrandHover`.

**Palatino 600** (checked in Phase 1). The family has only Regular and Bold faces, and Chromium renders 600 as Bold.
- Capture with `SemiBold`, then compare the vertical-stroke width of the "W" in "Workspace" against the web.
- If WPF renders it lighter, use `Bold`. Report which one you used.
- This applies to the card and empty-state titles.

### Badges (`web-display-badges.png`; demo height 86)

**`Badge` style** (ContentControl template):
- A Border with Height **24**, `CornerRadius` 12, padding `10,0`, Background = `TemplateBinding Background`.
- Inside, a horizontal StackPanel: an Ellipse 6×6 (Fill bound to `Foreground`, right margin 6), then the content at 12px 600.
- `Tag="Plain"` collapses the dot.

**Variants** (`BasedOn="Badge"`), as background / foreground:
- `BadgeNeutralDot`: `BgDeep` / `Ink2`
- `BadgeBrandDot`: `BrandSoft` / `Brand`
- `BadgeSuccessDot`: `SuccessSoft` / `Success`
- `BadgeWarningDot`: `WarningSoft` / `Warning`
- `BadgeDangerDot`: `DangerSoft` / `Danger`

Any naming is fine as long as it doesn't collide with the existing `Badge*` keys.

**Row:** horizontal, 12px gaps, at y 31.

| Badge | Draft | In review | Active | Pending | Overdue | v1.0 (plain) |
|---|---|---|---|---|---|---|
| x | 31 | 103 | 196.2 | 273.8 | 362.3 | 452.7 |
| width | 60 | 81.2 | 65.5 | 76.6 | 78.4 | 40.5 |

Tolerances: ±2 on width and x (text advance), ±1 on height and padding.

### Alerts (`web-display-alerts.png`; demo height 237.6)

**Layout:** 2×2 `.grid-2` with a 22px row gap. Each alert is 450×**76.8**, at (31,31), (503,31), (31,129.8) and (503,129.8).

**`Alert` style:**
- Border, `CornerRadius` 12, `Padding` `16,15`.
- Background = the variant's `*Soft`. Foreground = the variant colour.

**Inside:**
- **Icon:** 18×18 at (+16,+16).
- **Column:** starts 12px after the icon, at x +46.
  - Title: 14px 600, variant colour, line height 22.4, at y +15.
  - Text: 14px `Ink2`, line height 22.4, at y +39.4.

| Variant | Icon | Title | Text |
|---|---|---|---|
| info | `i-info` | Good to know | Your changes save automatically as you type. |
| success | `i-check` | Saved | Your settings have been updated. |
| warning | `i-alert` | Check your input | Some fields still need your attention. |
| danger | `i-close` | Something went wrong | We couldn't complete that request. Try again. |

**Icon styles:** `IconInfo`, `IconCheck`, `IconAlert`, `IconClose`, `IconCard` and `IconSearch`.
- Each is a ContentControl, then a Viewbox, then a 24×24 Canvas. Copy the sprite's path data, stroke widths and round caps/joins.
- **Stroke = `{Binding Foreground, RelativeSource={RelativeSource TemplatedParent}}`.** The size (18 or 22) is set on the instance.
- Forms' `SearchIcon` and `ErrorIcon` hard-code their colour. Flag the duplication as non-blocking.

### Avatars (`web-display-avatar.png`; demo height 213.4)

**`AvatarCircle` style:** a Border plus a centred TextBlock, `CornerRadius` = size/2. `BrandSoft` background, **1px `#ECD4C8` border**, 600 `Brand` text.

| Avatar | Size | Font | rel x,y | Tone |
|---|---|---|---|---|
| sm "AL" | 30 | 11 | 31,42 | brand |
| md "AL" | 40 | 13 | 73,37 | brand |
| lg "AL" | 52 | 16 | 125,31 | brand |
| "JS" | 40 | 13 | 189,37 | `Ink` background and border, white text |
| "MK" | 40 | 13 | 241,37 | `#E6EEE7` background, `#D3E3D6` border, `Success` text |

**Layout:**
- **Row:** 12px gaps, vertically centred.
- **Divider:** y 109.
- **Avatar-row** at y 136, height 46.4: a 40px "AL" at (31,139.2), a 10px gap, then the meta at x 81:
  - "Ada Lovelace": 14px **Bold**, `Ink`, line height 22.4, at y 136
  - "Workspace owner": 12px `Ink3`, at y ≈162

Centre the initials optically: the glyph box centre matches the web ±1.

### Skeleton (`web-display-skeleton.png`; demo height 469.8)

**Styles:**
- **`SkLine`:** Height 12, `CornerRadius` 6, `BgDeep`.
- **`SkCircle`:** 40×40, `CornerRadius` 20.
- Both carry the shimmer overlay (B7). The skeleton cards use `DisplayCard`.

**Top `.grid-2`** at y 31. Both cards are 450×**162**; card 2 stretches.
- **Header row:** the circle at (54,54). The lines start at x 106 and fill a 352 flex column:
  - card 1: 55% (193.6) at y 58, then 35% (123.2) at y 78
  - card 2: 48% / 30%
- **Body lines** at y 116 / 137 / 158:
  - card 1: 100% / 82% / 64% of 404
  - card 2: 90% / 72%
- **Use percentages** (star-column Grids, for example `55*|45*`), not fixed px.

**Below the cards:**
- A divider at y 219.
- `DemoLabel` "Table rows" at y 246.
- The table card: 922×**156**, at y 282.8.

**Table card rows:**
- 3 rows, 876 wide at x 54, with padding 12/0 and a 1px `Line` bottom border. The last row has none.
- Heights 37/37/36, starting at y 305.8, 342.8 and 379.8.
- 4 lines per row with 16px gaps. The 4th is right-aligned, ending at x 930.
- Widths as % of 876: row 1 28/20/14/10, row 2 34/18/12/9, row 3 24/22/15/11.

**Motion cue** "Loading content…": `Collapsed`, except under reduced motion (B6). Then it's a pill above the grid: 12px 600 `Brand` on `BrandSoft`, radius 99, padding `10,4`, and no gap to the grid (a web quirk).

### Progress bar (`web-display-progress.png`; demo height 193.8)

**Meta row** at y 31, height 20.8, full width:
- **Left:** "Uploading files…", 13px `Ink2`, id `ProgressLabel`.
- **Right:** "0%", 13px 600 `Ink`, `Typography.NumeralAlignment="Tabular"`, right edge at x 953, id `ProgressValue`.

**Bar** at y **59.8**, 922×8:
- Keep the `ProgressBar` (id `ProgressBar`, Maximum 100) with a new template, `DisplayProgress`.
- `PART_Track`: Border, `BgDeep`, `CornerRadius` 4.
- `PART_Indicator`: Border, `Brand`, `CornerRadius` 4, left-aligned.
- The smallest non-zero value is 4% (≈37px), so the fill never needs a clip.

**Below the bar:**
- A `Divider` at y 93.8.
- Buttons at y 120.8: "Start upload" (`PrimaryButton`, 114.3×42, id `ProgressStart`), then a 12px gap, then "Reset" (`SecondaryButton`, 70.3×42, x 157.3, id `ProgressReset`).

### Empty state (`web-display-empty-state.png`; demo height 343.8)

**Cards:** two `DisplayCard`s, 450×**281.8**, content centred, with `.empty-state` padding `20,44`.

**Head** at y 98, height 27: a centred horizontal StackPanel.
- Icon: 22×22, `Brand`, at y 100.5.
- A 9px gap.
- Title: Serif 20px 600, `Ink`, fixed Height 27.
- Card 1 head spans x 171.8–340.2; card 2 spans 635.3–820.7.

**Text** at y **137**:
- 14px `Ink2`, line height 22.4, `TextAlignment=Center`, **MaxWidth 317** (42ch).
- Two lines that must break as on the web: "…Start with a / blank project…" and "…Try a / different term…".
- Adjust MaxWidth ±3 if WPF breaks differently, and report the value.

**Actions** at y 203.8: centred, 10px gap, 42 tall.
- **Card 1:** "New project" (`PrimaryButton`, 113.2, keep id `EmptyAction`) and "View guide" (`SecondaryButton`, 106.7, `EmptyGuide`). Together they span x 141.1–371.
- **Card 2:** "Clear filters" (`SecondaryButton`, 108, `EmptyClear`).

**Icons:** `IconCard` (card 1) and `IconSearch` (card 2).

### Out of scope (accepted differences; don't chase them into Controls.xaml)

- **The full-size `PrimaryButton` / `SecondaryButton`:** opacity hover (the web changes colour), no Shadow1, no `:active` 1px shift. That's a later buttons task; measure their geometry only.
- **Button focus rings.**
- **Section order.**
- **The sidebar.**
- **The "Tabs" block.**

---

## Behaviour spec (every web branch)

| # | Branch (web) | WPF | Verified by |
|---|---|---|---|
| B1 | Cards, badges, alerts, avatars and empty state have **no JS**. The danger alert's ✕ is a decorative icon, **not a dismiss**. The empty-state buttons do nothing | Static, no handlers | UIA presence |
| B2 | **Start while idle:** label → "Uploading files…", value = 0. Every 350ms: `value = min(100, value + round(rand*12) + 4)` (+4…+16), which sets the width and "N%". No visual reset before the first tick | `DispatcherTimer` (350ms) and a `Random` field. Text = the target **immediately**. The width animates over **250ms** (B9) | UIA: Start, then poll `RangeValuePattern` and `ProgressValue` until 100 (timeout 12s; worst case 25 ticks ≈ 8.75s) |
| B3 | **Start while running:** `if(progressTimer) return` | Return if the timer is enabled | UIA: Start twice; the value never drops between polls |
| B4 | **Complete** (≥ 100): stop the timer, label → "Upload complete", success toast "Upload complete / All files were uploaded." | Stop the timer and set the label. **Toast deferred**: the WPF toast is private to `AdvancedControls`, the same decision as the grid copy toast. Add a `ponytail:` note | UIA: label "Upload complete", value 100 |
| B5 | **Reset** at any time: stop the timer, label → "Uploading files…", value 0 (the width animates) | The same | UIA: Reset mid-run and after completion. Within 300ms the value is 0, "0%" |
| B6 | **Reduced motion:** animations and transitions ≈ 0, the cue shows, `.skeleton` background → `Line` | When `SystemParameters.ClientAreaAnimation == false`: no shimmer (overlay `Collapsed`), `SkLine`/`SkCircle` background `Line`, cue `Visible`, progress set with a 0 duration | Code-only. Compare by eye to `web-display-skeleton-reduced.png` (cue and colour only) |
| B7 | **Shimmer:** an `::after` gradient `90deg transparent → rgba(255,255,255,.65) → transparent`, `translateX(-100%)` → `100%`, **1.4s ease, infinite**, relative to each element's width, clipped to its rounded shape, all elements in phase | See the B7 implementation notes below | Code review for timing. The capture shows the static base |
| B8 | Hover: **none**, except buttons | The small-button hovers as specified | Code-only (no mouse on the hidden desktop) |
| B9 | `.progress__fill{transition:width .25s ease}` | **One** `SetProgress(int)` does every change, Reset and B6 included: `BeginAnimation(ValueProperty, …)` with a `SplineDoubleKeyFrame KeySpline="0.25,0.1 0.25,1"`, 250ms (0ms under reduced motion). **Never assign `Value` directly**: with the default `HoldEnd`, the held animation masks a direct set | Code review, plus the B5 UIA check |

**B7 implementation notes:**
- **Overlay:** `Shimmer.Enabled` (`Shimmer.cs`) adds the overlay as a child Border with the same `CornerRadius`. A rounded Border draws its background rounded, so **no clip is needed**.
- **Brush:** the overlay's Background is a `LinearGradientBrush` with stops `#00FFFFFF` / `#A6FFFFFF` / `#00FFFFFF`.
  - Use `#00FFFFFF`, not `Transparent`: WPF doesn't premultiply, so a black-transparent stop gives grey fringes.
  - Create the brush **in code, unfrozen**. A brush from a ResourceDictionary is frozen, and animating it throws.
  - One shared, unfrozen brush for all overlays keeps them in phase.
- **Animation:** the brush's `RelativeTransform` is a `TranslateTransform` whose X animates -1 → 1 over 1.4s, with the KeySpline above and `RepeatBehavior=Forever`.
- **When not animating** (`--capture` or reduced motion): the overlay is **`Collapsed`**. A transform left at X=0 would park the white band across every line.

**The animation must not hang the hidden runner.**
- `Shimmer.cs` must not start the storyboard when `Environment.GetCommandLineArgs()` contains `--capture`. Read the args there; `App.xaml.cs` belongs to the reviewer.
- A Forever animation can starve the `ApplicationIdle` point where `--capture` fires, and it makes captures non-deterministic.
- The progress timer stops at 100 and on Reset.
- No `DropShadowEffect` on skeleton elements or alerts. `Effect` is used only on `PrimaryButtonSm`.

---

## Phases: one at a time, stop for review

Phase 0 runs first. Phase 1 creates `Display.xaml`, which everything else needs. **After Phase 1, phases 2, 3 and 4 are independent:** each touches only its own MainWindow blocks and styles, so the worker can stop and report after any of them, and they can be reordered. Phase 5 runs last.

### Phase 0: audit and measure (no code changes)

1. Build `display` and the tests. Run `run-hidden.ps1 -Agent display`, and record the pass/fail counts and the **test wall time**.
2. Re-capture the six current sections if the build changed since the `wpf-before-display-*.png` captures.
3. Re-run the README geometry dump for one section, and confirm `web-display-measurements.txt` still matches `web/index.html`.
4. Grep `wpf/` for `ButtonSm` (Q1).

**Acceptance:**
- 0.1 Counts and wall time are reported.
- 0.2 Measurement drift is reported (or "none").
- 0.3 The `ButtonSm` grep result is reported.
- 0.4 `git status` is unchanged.

### Phase 1: Display.xaml, shell, cards, badges

**Infrastructure:**
- `Themes/Display.xaml`, with the same header as Forms.xaml.
- The csproj `Page` entry, and the App.xaml merge (last).
- The brushes `AvatarBorder` #ECD4C8, `SageBg` #E6EEE7 and `SageBorder` #D3E3D6.

**Styles:** `DisplayCard`, `SecondaryButtonSm`, `PrimaryButtonSm`, `Badge` and the 5 variants.

**MainWindow:** replace "Cards & badges" with a "Cards" section and a "Badges" section, with the shell and the verbatim descriptions. Do the Palatino weight check here.

**Tests:** "Workspace settings", "Your plan", "Open settings", "Upgrade plan" and all 6 badge texts are found by Name.

**Captures:** `wpf-display-cards.png` and `wpf-display-badges.png`.

**Acceptance:**
- 1.1 The demo card is 984 wide. Its height is ±1 of 232.9 (cards) and 86 (badges). Radius 16, no shadow: the pixel 3px outside the edge is `Bg` #F5F4EF.
- 1.2 The card rects are ±1 of (31,31,450,170.9) and (503,31,450,170.9). The border is #E6E2D7, the radius 12.
- 1.3 Title, text and foot y are ±1 of 54 / 84.1 / 146.9. The small buttons are 32 ±1 tall, with widths ±2 of 108.1 and 106.3.
- 1.4 The Palatino weight is reported with stroke-width numbers (web vs SemiBold vs Bold).
- 1.5 The badges are 24 ±1 tall, with x ±2 of the table. The dot is 6, and the plain badge has none. Fills are exact hex.
- 1.6 The build is clean, all tests pass, and only the listed files changed.

### Phase 2: alerts and empty state (icons, grid-2 cards)

**Styles:** the 6 `Icon*` styles and `Alert`.

**Blocks:**
- The alerts as a 2×2 grid.
- The empty state: two cards, a bare icon with the serif title, text at MaxWidth 317, and the actions.

**Cleanup:** delete `EmptyIcon` from ExtraControls.

**Tests:** the 4 alert titles are found; `EmptyState`, `EmptyAction`, `EmptyGuide` and `EmptyClear` exist; "No results found" is found.

**Captures:** `wpf-display-alerts.png` and `wpf-display-empty-state.png`.

**Acceptance:**
- 2.1 The alert rects are ±1 of the four listed. Backgrounds are exact `*Soft` hex. The radius is 12.
- 2.2 The icon box is 18 ±1 at (+16,+16), in the variant colour. Text x is +46 ±1, and y is +15 / +39.4 ±1.
- 2.3 The empty cards are 281.8 ±1. The head is at y 98, the text at y 137 and the actions at y 203.8, all ±1.
- 2.4 The head is centred ±2. Line breaks match the web in both cards.
- 2.5 The build is clean and all tests pass.

### Phase 3: avatars and progress bar

**Avatars:**
- `AvatarCircle` with the sizes, the row, the divider and the avatar-row.
- Delete `Avatar` and `AvatarText` from ExtraControls.

**Progress:**
- The `DisplayProgress` template, the meta row and the buttons.
- B2–B5 and B9 in `MainWindow.xaml.cs`.
- Split off "Tabs", unchanged.

**Tests** (replace the now-wrong "progress has value > 0"):
- The initial value is 0, "0%", and the label is "Uploading files…".
- Start, then poll until 100 (≤ 12s). The value is non-decreasing even with a second Start in between. Then the label reads "Upload complete" and the value "100%".
- Reset gives 0 within 300ms, "0%" and "Uploading files…".
- Avatars: "AL" is found ≥ 4 times, plus "JS", "MK" and "Ada Lovelace". This replaces the old `Avatar` id check.

**Captures:**
- `wpf-display-avatar.png`, and `wpf-display-progress.png` at idle (0%).
- **No pixel acceptance for the filled bar.** `!ProgressStart` would capture at about 0%, because the ticks are random and async. The fill colour and radius are checked by code review against `web-display-progress-64.png`.

**Acceptance:**
- 3.1 Circles are 30 / 40 / 52 ±1 at the listed x,y ±1. Borders and fills are exact (#ECD4C8, #F8ECE4, #26241F, #E6EEE7, #D3E3D6). Initials are centred ±1.
- 3.2 The divider is at y 109 ±1. The avatar-row name is at (81,136) ±1, and Bold.
- 3.3 The meta row is at y 31. The track is at y 59.8 ±1, 922×8, #EEEAE0, radius 4. The divider is at 93.8 and the buttons at 120.8, both ±1. "0%" ends at x 953 ±1.
- 3.4 The B2–B5 UIA checks pass. **Record the new test wall time:** this is the baseline for Phase 4. Expect about +10s.
- 3.5 The build is clean and all tests pass.

### Phase 4: skeleton

**Styles:**
- `SkLine` and `SkCircle`.
- `Shimmer.cs` (B6, B7), with its csproj `Compile` entry.
- Delete `ShimmerBrush` from ExtraControls.

**Block:**
- The cue, the two sk-cards with percentage widths, the divider, the "Table rows" label and the table card.
- The outer `GroupBox` gets id `Skeleton`, so the existing test passes on the id rather than the fallback.

**Tests:**
- `Skeleton` is found by id.
- "Table rows" is found.
- "Loading content…" is offscreen or absent, assuming animations are on in the test environment.

**Captures:** `wpf-display-skeleton.png`. It must show the static base, with no band.

**Acceptance:**
- 4.1 The sk-cards are 450×162 ±1 (card 2 stretched). The table card is 922×156 ±1.
- 4.2 Lines are 12 ±1 tall with radius 6. Positions are ±2 in x and width, ±1 in y, of `web-display-measurements.txt` sk-line[0..20]. The circle is 40.
- 4.3 Table rows are 37/37/36 ±1, with 1px `Line` under rows 1–2 only. The 4th line ends at x 930 ±1.
- 4.4 The line fill is exactly #EEEAE0 everywhere in the capture (the overlay is collapsed).
- 4.5 Captures exit 0. The test wall time is within +10% of the **Phase 3** run.
- 4.6 Code review: KeySpline `0.25,0.1 0.25,1`, 1.4s, Forever, `#00FFFFFF` stops, an unfrozen brush created in code, and both the `--capture` and the `ClientAreaAnimation` guards.

### Phase 5: tests and README

- All display assertions are green, and nobody else's assertions were edited.
- `wpf/README.md`: update the covered list and the assertion count.
- Re-capture all 7 `wpf-display-*.png`.
- Report `BadgeBrand` / `BadgeNeutral` as unused.

**Acceptance:**
- 5.1 The full run passes.
- 5.2 All 7 captures are present.
- 5.3 The README is updated.
- 5.4 `git status` shows only the files in the ownership table.

---

## Reminders

- **C# 5:**
  - no `$""`, `?.`, `nameof`, `=>` members, auto-property initialisers, `out var` or tuples
  - use `string.Format`
  - `Random` is a field, not created per tick
- **Project entries:** every new `.cs` needs a `<Compile Include>`; every new `.xaml` needs a `<Page Include>`, plus the App.xaml merge.
- **WPF quirks:**
  - `ClipToBounds` on a rounded Border clips to a rectangle. If a real rounded clip is ever needed, use a `RectangleGeometry` updated on `SizeChanged`; `TableClip.cs` shows the pattern (it's `tables`', so don't edit it). The rounded-overlay approach (B7) avoids the clip.
  - `Effect` forces software rendering of its subtree. Keep it off the skeleton and the alerts.
  - Use fixed heights from the measurements (badge 24, buttons 32/42, line 12, head 27), not padding plus font metrics.
  - Use `UseLayoutRounding` and `SnapsToDevicePixels` for 1px lines.
  - A `Border` or `StackPanel` has no UIA peer, so put ids on a `GroupBox` wrapper.

## Reviewer checklist

- [ ] Only the ownership-table files and blocks changed. `Controls.xaml` is untouched.
- [ ] New keys don't collide (badge, icon, button-sm). No mechanism is duplicated beyond the flagged icons.
- [ ] `Avatar`, `AvatarText`, `ShimmerBrush` and `EmptyIcon` are each deleted in the phase that replaced them.
- [ ] Captures were taken hidden and pixel-scanned against `web-display-measurements.txt`.
- [ ] The shimmer and the timer can't hang the runner. `SetProgress` is the only writer of `Value`. The toast deferral carries a `ponytail:` note.
- [ ] C# 5, the build is clean, the tests pass, and there are no commits.

## Decisions (reviewer, final)

- **Q1 yes.** Display defines `SecondaryButtonSm` and `PrimaryButtonSm` in Display.xaml (merged last) in Phase 1. 04-tables Phase 3 is amended to reuse them and add only `DangerButtonSm`. If tables lands its own first, display reuses theirs and does not define a duplicate.
- **Q2 yes.** Upload-complete toast deferred (with the `ponytail:` note).
- **Q3 yes.** Keep the WPF section order.

---

## Worker kickoff

```
You are the worker agent "display" on the WPF pixel-parity effort (repo C:\Workspace\Projects\others\ui-design).
Read plan/wpf/README.md fully (rules, worker workflow, ownership, gotchas), then plan/wpf/05-display.md
and the web lines it cites (CSS 142-157, 160-184, 215-221, 357-399, 496-515, 868-873; markup 1344-1453, 1575-1591; JS 3234-3263).
Rules: build only with wpf\build.bat display; test and capture only via pwsh wpf/tests/run-hidden.ps1 -Agent display;
never launch the exe on the user's desktop, never kill processes by name, never git commit.
Edit only the files and MainWindow blocks in the 05 ownership table; re-read MainWindow.xaml, the csproj and App.xaml
right before each edit (other agents are editing them).
Do Phase 0 only (audit and measure, NO code changes), then stop and report in the README format, including:
test pass/fail counts and wall time, measurement drift (or none), and the `ButtonSm` grep result.
```
