# 06 — Navigation and buttons: buttons, action menu, accordion, stepper, breadcrumbs, tooltips, tabs (agent "nav")

A fix-up audit, close to a rewrite. The full-size buttons are the shared primitive every other group uses, so they come first. The other six sections are rough stock-WPF ports: a `ContextMenu`, a `ToolTip` popup, a native `TabControl` and `Expander`, a 4-column `Grid` stepper, and `›` text crumbs. None of them match the web, and the popups can't be captured.

**Web group vs this list.** The web has exactly these seven sections. The nav label of `tooltip-demo` is "Tooltips", and that is also its `h2`. Each `h2` is the WPF `SectionTitle`.

| Web id | h2 (= WPF section title) | Markup | JS | CSS |
|---|---|---|---|---|
| `buttons` | Buttons | 980–1008 | loading button 3482–3490 | `.btn*`, `.spinner` 159–186 |
| `action-menu` | Action menu | 1010–1040 | 3100–3158 | `.menu*` 188–212, `.icon-btn` 449–455 |
| `accordion` | Accordion | 1456–1497 | 3341–3358 | 247–259 |
| `stepper` | Stepper | 1499–1523 | 3320–3339 | 261–285 |
| `breadcrumbs` | Breadcrumbs | 1525–1544 | none (plain `href="#…"` links) | 287–293 |
| `tooltip-demo` | Tooltips | 1546–1558 | none (CSS only) | 413–428 |
| `tabs` | Tabs | 1560–1573 | 3360–3380 | 401–411 |

- **Shared CSS:**
  - tokens 14–45; global `:focus-visible` 50
  - section shell 141–157 (`.demo`, `.demo__label`, `.row`, `.grid-2`, `.divider`)
  - `.field__hint` 298; reduced motion 870–874
- **Icons** (sprite 880–907, plus `i-plus` 3529; all 24-unit boxes):
  - stroke 2: `i-chevron` 898, `i-arrow` 893, `i-plus` 3529
  - stroke 1.8: `i-more` 889, `i-edit` 904, `i-copy` 905, `i-download` 906, `i-pin` 899, `i-trash` 907
  - stroke 1.9: `i-search` 892, `i-info` 894
  - stroke 2.4: `i-check` 890
- **Not in this group** (named in the open questions): the sidebar nav (active-on-scroll, collapse, collapsed-mode tips; markup 912–969, CSS 74–126, JS 3492–3524) and the masthead ("version 2.0", 54px). See **Q5**.

**References** in `plan/wpf/ref/` (2026-11-22, 1400×1100, scale 1, transitions and animations off). `web-nav-capture.ps1` regenerates all of them plus the measurements.
- **Idle sections:** `web-nav-<buttons|menu|accordion|stepper|breadcrumbs|tooltips|tabs>.png`.
- **States:**
  - **Buttons:** `web-nav-buttons-focus.png` (Primary action keyboard-focused) and `web-nav-buttons-working.png` (Loading clicked: "Working…", disabled).
  - **Action menu:**
    - `web-nav-menu-open.png` (project menu, first item focused)
    - `-menu-down2.png` (2× Down, "Export as CSV" focused)
    - `-menu-compact-open.png`
    - `-menu-picked.png` ("Make a copy" chosen, so the hint reads "Make a copy")
  - **Accordion:** `web-nav-accordion-toggled.png` (Team members opened in the multi accordion, Notifications in the single one, so Account closes) and `-accordion-focus.png` (Team members focused).
  - **Stepper:** `web-nav-stepper-next.png` (after one Next) and `-stepper-last.png` (step 4: Next disabled).
  - **Tooltips:**
    - `web-nav-tip-<top|bottom|left|right|save>.png`, one tooltip forced visible each (the hover end state)
    - `-tooltips-focus-top.png` (the Top button focused: tooltip plus ring)
    - `-tip-right.png` shows web bug **W14**: the Save button covers the Right tooltip.
  - **Tabs:** `web-nav-tabs-security.png` (Security selected) and `-tabs-focus.png` (ArrowRight from Profile: Security selected and focused).
- **Measurements:** `web-nav-measurements.txt`, produced by `web-nav-measure.js`. It holds every rect, font, colour, radius, padding, opacity, transform and shadow, plus the key-handling traces. The dump viewport is 1376×1008: use the `rel` numbers (relative to the `.demo` card, or to the named base).
- **WPF "before" captures:** none yet. Phase N0 produces `wpf-before-nav-*.png`.

Read `plan/wpf/README.md` first. Rules 4 (hidden desktop, your agent name) and 5 (never commit) are mandatory.

---

## Review of the current port (audit, 2026-11-22)

All seven old blocks sit inline in `MainWindow.xaml`. They use the shadowed `ClaudeCard` with a 16px gap and the old `SectionDesc`, not `SheetSectionDesc` + `DemoCard`.

| Web section | WPF now (`MainWindow.xaml`) | Problems |
|---|---|---|
| Buttons | ~47–60: one row of 5 buttons | **Missing:** the "With icons" and "Sizes & states" rows, the labels and the dividers. The description is shortened. **`ClaudeButtonBase` (`Controls.xaml` 6–42) problems:** see the list below this table |
| Action menu | Part of "Tooltips &amp; action menu" ~582–597: a "More actions" `SecondaryButton` with a right-click `ContextMenu` of 3 plain items | **Wrong control:** a `ContextMenu` is a `Popup`, so it can't be captured, and it opens on right-click. **Missing:** the "Actions ▾" trigger, the icon-btn menu, icons, shortcuts, dividers, the disabled item, the danger item, arrow/Home/End/Escape/Tab handling and the result hint |
| Accordion | "Accordion &amp; list" ~490–506: two stock `Expander`s + a multi-select `ListBox` | **Wrong look:** stock Aero chrome, a single column, no "One at a time" variant, shortened texts. **Extra:** the `ListBox` (`MultiList`) has no web counterpart (**Q4**) |
| Stepper | ~569–580: a 4-column `Grid` of `Ellipse`s with "1 Account" labels | **Missing:** numbers, titles and descriptions, the per-step lines, the done/current/todo states, the Previous / Next / status row, the vertical stepper. The geometry is wrong (centred columns, not flex) |
| Breadcrumbs | ~557–567: `TextBlock`s with a `›` glyph | **Wrong:** only one trail with 3 crumbs (the web has 2 trails with 4); the separator is a glyph, not the 13px chevron. **Missing:** "Settings" current crumb, the collapsed "…" trail, focusable links and navigation |
| Tooltips | Part of "Tooltips &amp; action menu": one "Hover me" button with a WPF `ToolTip` (a `Popup`) | **Not capturable**, and the show delay and placement don't match. **Missing:** the four sides, the Save variant, show on keyboard focus |
| Tabs | ~471–488: a `Slider` + a stock `TabControl` (Height 140) | **Wrong look:** stock tab chrome. **Extra:** the `Slider` has no web counterpart (**Q4**) |

**`ClaudeButtonBase` problems** (`Controls.xaml` 6–42):
- **Hover** is opacity .86 instead of a colour change.
- **Pressed** is opacity .72 instead of a 1px down shift.
- **No `Shadow1`** on Primary.
- **Focus** shows only a brand border instead of the 2px ring 2px outside.
- **2px too narrow** (Primary, Dark, Ghost, Danger) — the root cause of the polish item "Start upload 142 vs 144": every web `.btn` has a **1px transparent border** (CSS 165), but these four styles set `BorderThickness 0` with padding `17,11`. Secondary has the 1px border and is correct.

`Themes/ExtraControls.xaml` holds `CrumbLink`/`CrumbCurrent`/`CrumbSep` plus **implicit** `ToolTip`, `ContextMenu` and `MenuItem` styles. The implicit ones also style the default TextBox context menu (Cut/Copy/Paste) in every input, so they stay (Decision 9).

---

## Decisions (made in this plan; reviewer confirms)

1. **One worker, `nav`**, buttons first (**Q1**). The phases are small and sequential. Five workers already share 4 test slots.
2. **This task explicitly authorises restyling `ClaudeButtonBase` and its five variants** (`Controls.xaml` 5–69). That is README hard constraint 3's exception.
   - The change moves widths (+2 on Primary/Dark/Ghost/Danger) and text ink in the captures of toasts, overlays, pickers, tables and display. Every one of them moves toward the web, and their plans already accept ±2 on full-size buttons.
   - **The reviewer announces N1 to the running workers and lands it between their phases.** It does not block them.
3. **The sections stay in `MainWindow.xaml`, edited in place, in the current WPF order** (precedent: 05 Q3). There is no new UserControl.
   - `display` is archived, so these blocks have no live owner. Standalone `-Section` keeps working through `Sections` unchanged.
   - Split "Tooltips &amp; action menu" into **"Action menu"** (moved to directly after "Buttons", as in the web) and **"Tooltips"** (left where the old block is).
   - Rename "Accordion &amp; list" to **"Accordion"**.
4. **The code-behind goes in a new partial file `wpf/MainWindow.Navigation.cs`** (`public partial class MainWindow`). It holds the loading button, the menus, accordion single mode, the stepper, the tabs and the crumb navigation.
   - `MainWindow.xaml.cs` is **not** edited.
   - Initialise the stepper in the XAML start state (step 1 current, Previous disabled), so the constructor needs nothing.
5. **Popover hosting = a lazily created in-window layer, not an adorner and not a `Popup`.** This hosts the menu panels and the tooltip bubbles.
   - **Why not `DropdownAdorner`:**
     - It arranges the panel at the anchor's width, and the menu panel is 210 wide, right-aligned to a 107 trigger.
     - It uses the `ScrollContentPresenter`'s adorner layer. That layer is **clipped at `ContentScroll`'s 48px padding**, while the menu panel sticks out **71.7px left of the card**. At the 1400 capture width the card sits only ~19–36px inside that clip, so ~40–50px of the panel would be cut off.
     - The window's own adorner layer is outside `MainWindow.Content`, so `--capture` (which renders only `MainWindow.Content`) wouldn't show it.
   - **`Popover.cs`** (new, `nav`) creates the layer on first use:
     - a `Canvas` named **`PopoverLayer`** added to the root `Grid` (`MainWindow.Content`)
     - **`Grid.ColumnSpan=2`** (the whole window, like `OverlayLayer`). The web viewport (`innerWidth`/`innerHeight`) is the whole window, sidebar included.
     - `Panel.ZIndex=50` (below `OverlayLayer` 100 and `ToastLayer` 200), `ClipToBounds=True`, `Background=null` (clicks pass through)
     - Because it is created lazily, there's no `Install` line in the shared constructor.
   - **Why the whole window:** at the 1400 capture width the card sits only ~67–76px inside column 1 (67 with a scroll bar, 76 without), so the panel's left edge lands ~4px inside column 1. A column-1 layer with the 8px clamp would shift the panel right, and in a full run it would clip it.
   - **API:** `Popover.Show(FrameworkElement anchor, FrameworkElement content, Placement)` and `Popover.Hide(FrameworkElement content)`.
     - **Position** comes from `anchor.TransformToVisual(layer)` (or `anchor.TranslatePoint(new Point(), layer)`). **Not** `TransformToAncestor`: the layer is a sibling of the anchor's ancestors, so that throws `InvalidOperationException`.
     - Flip and clamp against the layer (= root `Grid`) bounds.
     - While anything is shown, it re-positions on the anchor's ancestor `ScrollViewer.ScrollChanged` and on the layer's `SizeChanged`.
   - **Captures:** the layer doesn't exist until the first open, and `--capture` resolves its target **before** any `!` step, so **never use `PopoverLayer` as a capture target**.
     - For menu states, crop to **`OverlayLayer`**: it always exists, is full-window and is never collapsed.
     - For tooltips, `ContentScroll` is enough.
6. **Menus reuse `DropdownOutsideClick` and `DropdownTree` from `DropdownPrimitives.cs` unmodified** (owned by `dropdown`; never edit it). Use them for outside-click close and the "is the focus inside" checks.
7. **Controls are `ButtonBase`, so UIA Invoke and the `!Id` capture chain reach them:**
   - **menu trigger and icon-btn:** `Button`; Click toggles open/close, as the web does
   - **menu items:** `Button`s
   - **accordion triggers:** `ToggleButton`s; the panel `Visibility` is bound to `IsChecked`
   - **tabs:** `RadioButton`s in one group; `SegmentButton` is the precedent. `!TabSecurity` checks it, and UIA gets the `SelectionItem` pattern
   - **crumb links:** `Button`s with a link template
   - **No `Expander`, `TabControl`, `ContextMenu` or `ToolTip`:** their headers aren't addressable by the `!Id` chain, or they are `Popup`s.
8. **No transitions** (**Q7**, same answer as 07 Q2): no menu fade-in (.14s), no chevron rotation animation (.15s/.18s), no tooltip fade and 4px slide (.15s), no button background/shadow transitions. Set the end state directly.
   - **Exception:** the button spinner (0.7s linear spin) is functional, so port it.
   - **Guard it like `Shimmer.cs`:** static, no animation when the command line has `--capture` or `SystemParameters.ClientAreaAnimation` is false. When static, the arc sits at the top.
   - If overlays O2 has landed with a reusable guard, reuse it. Otherwise use a private `IsCapture` with a `ponytail:` note naming the lift.
9. **Leave the implicit `ToolTip`/`ContextMenu`/`MenuItem` styles in `ExtraControls.xaml`** (they style TextBox context menus). Delete only the `Crumb*` styles once breadcrumbs move to `Navigation.xaml`. `nav` takes ownership of `ExtraControls.xaml`.
10. **New icons** (`IconArrow`, `IconEdit`, `IconCopy`, `IconPin`, `IconTrash`) go in the new `Themes/Navigation.xaml`, in the `Icon*` pattern (Viewbox, 24 Canvas, `Path` stroke bound to `Foreground`).
    - **Reuse:**
      - from Display: `IconSearch`, `IconInfo`, `IconCheck`
      - from Overlays: `IconPlus`, `IconMore`, `IconDownload`, `IconChevron`, `IconButton`
    - Path data (converted from the sprite, all round caps and joins):
      - Arrow (2): `M5,12 L19,12 M13,6 L19,12 L13,18`
      - Edit (1.8): `M12,20 L21,20 M16.5,3.5 A2.1,2.1 0 0 1 19.5,6.5 L7,19 L3,20 L4,16 Z`
      - Copy (1.8): `M10,8 L19,8 A2,2 0 0 1 21,10 L21,19 A2,2 0 0 1 19,21 L10,21 A2,2 0 0 1 8,19 L8,10 A2,2 0 0 1 10,8 Z M16,8 L16,5 A2,2 0 0 0 14,3 L5,3 A2,2 0 0 0 3,5 L3,14 A2,2 0 0 0 5,16 L8,16`
      - Pin (1.8): `M12,17 L12,22 M9,3 L15,3 L14,9 L17,12 L17,14 L7,14 L7,12 L10,9 Z`
      - Trash (1.8): `M4,7 L20,7 M9,7 L9,4 L15,4 L15,7 M6,7 L7,20 L17,20 L18,7`
11. **Focus rings:** a 2px `Brand` ring 2px outside, on `IsKeyboardFocused`, with `FocusVisualStyle {x:Null}`. The forms quirk is accepted: it also shows after programmatic focus.
    - **Outer radius = element radius + 4**, drawn as a `Border` with `Margin -4`:
      - **12** for `.btn` (radius 8)
      - **11** for `.btn--sm` (radius 7)
      - **8** for the tab, accordion trigger, crumb link and tooltip host. The global `:focus-visible` gives an element without its own radius `border-radius:4px`.
    - Menu items have **no** ring (`outline:none`); keyboard focus paints the `BgDeep` background instead.

---

## Files and ownership

| File | Change |
|---|---|
| `wpf/Themes/Controls.xaml` (**shared with `tables`, which is running**) | **Only** lines 5–69 (`ClaudeButtonBase`, `PrimaryButton`, `SecondaryButton`, `GhostButton`, `DangerButton`, `DarkButton`). Re-read the file before every `Edit`; never touch the `Table*`/`Segment*`/`Pin*`/`Sort*`/`WebSelect`/`DangerButtonSm` styles |
| `wpf/Themes/Navigation.xaml` (**new**) | icons, spinner, menu panel/item, tooltip bubble, accordion, stepper, crumb, tab styles. Append it as the **last** merged dictionary in `App.xaml` (it references Display and Overlays keys) |
| `wpf/Popover.cs` (**new**) | `PopoverLayer`, `Show`/`Hide`/placement, and the `Tip.Text`/`Tip.Side` attached properties (Decision 5) |
| `wpf/MainWindow.Navigation.cs` (**new**, partial `MainWindow`) | the handlers (Decision 4) |
| `wpf/MainWindow.xaml` (**shared**) | **Only** these blocks: Buttons (~47–60), Tabs (~471–488), Accordion &amp; list (~490–506), Breadcrumbs (~557–567), Stepper (~569–580), Tooltips &amp; action menu (~582–597). The new "Action menu" block goes directly after Buttons. Never touch the Forms, Cards, Badges, Data table, Alerts, Progress, Avatars, Empty state or Skeleton blocks, or the `<local:…>` lines |
| `wpf/Themes/ExtraControls.xaml` | delete `CrumbLink`/`CrumbCurrent`/`CrumbSep` in N6. Keep the implicit styles |
| `wpf/App.xaml` (**shared**) | append `Themes/Navigation.xaml` **last**; never reorder |
| `wpf/UIComponents.Wpf.csproj` | none: wildcards pick up `Themes\Navigation.xaml`, `Popover.cs` and `MainWindow.Navigation.cs` (no entries) |
| `wpf/tests/UIAutomationTests.cs` (**shared**) | replace only the old nav assertions, found by their comments (line numbers drift): `// Buttons` (~245–248), `// Slider`, `// TabControl`, `// Expander`, `// ListBox with multiple selection` (~528–580), `// Breadcrumbs`, `// Stepper`, `// Tooltip button`, `// Action menu button` (~670–684) |
| `wpf/README.md` | N7: covered list and assertion count |
| `wpf/Themes/Display.xaml`, `wpf/Themes/Sheet.xaml` | **only if Q6 = yes**: add the ring and press triggers to `PrimaryButtonSm`, `SecondaryButtonSm` and `GhostButtonSm` (owners archived). Nothing else |

**Never edit:**
- `DropdownPrimitives.cs`, `SearchableSelect.cs`, `MultiSelect.cs`, `Themes/Dropdown.xaml`
- `Forms.xaml`, `Overlays.xaml`, `Toasts.xaml`, `Pickers.xaml`, `Tokens.xaml`
- `OverlayHost.cs`, `Toasts.cs`, `CalendarPicker.cs`, `Shimmer.cs`
- `MainWindow.xaml.cs`, `App.xaml.cs`, `run-hidden.ps1`
- `InputControls`, `DataControls`, `AdvancedControls`, `ToastControls`, `OverlayControls`, `TablePagination`

If one of them blocks you, ask.

**Shared files:** re-read each one immediately before editing, make small `Edit`s, add only your own lines, and never reorder other people's entries.

**Proposed README ownership row** (the reviewer adds it):

| Area | Owner |
|---|---|
| `Popover.cs`, `MainWindow.Navigation.cs`, `Themes/Navigation.xaml`, `Themes/ExtraControls.xaml`; `ClaudeButtonBase` + the five full-size button styles in `Controls.xaml`; the Buttons, Action menu, Accordion, Stepper, Breadcrumbs, Tooltips and Tabs blocks of `MainWindow.xaml` | `nav` |

---

## Dependencies

| On | What | Blocks |
|---|---|---|
| `tables` (running) | shares `Controls.xaml` | nothing; re-read before every edit, button block only |
| `toasts`, `overlays`, `pickers` (running) | their captures use `PrimaryButton`/`SecondaryButton`/`DangerButton` | nothing; N1 shifts their widths +2 toward the web (Decision 2). The reviewer tells them |
| `overlays` | `IconButton`, `IconChevron`, `IconMore`, `IconPlus`, `IconDownload` (exist); maybe a spinner guard (O2) | none; reuse only |
| `dropdown` | `DropdownOutsideClick`, `DropdownTree` (exist) | none; reuse only |
| reviewer | **Q3**: `~Left`/`~Right`/`~Home`/`~End` capture steps in `App.xaml.cs` `SendCaptureKey` (4 `case` lines) | N2 and N6 key captures. Without them those states are K-tested only |

---

## Visual spec (DIPs; `rel` to the `.demo` card unless stated)

### Section shell (all seven)

- **Title:** `SectionTitle`, Margin `0,46,0,0`.
- **Description:** `SheetSectionDesc`, Margin `0,8,0,0`, verbatim text:
  - Buttons: "Primary, dark, secondary, ghost and danger variants, with size options and loading / disabled states." (2 lines)
  - Action menu: "A popover of actions anchored to a button. Navigate with the arrow keys, activate with Enter, and close with Escape or an outside click." (2 lines)
  - Accordion: "Collapsible sections. Click a title to open or close it; several can stay open at once." (2 lines)
  - Stepper: "Progress through a multi-step flow. Completed steps show a check, the current step is highlighted, and the controls below move between them." (2 lines)
  - Breadcrumbs: "Path navigation with the current page marked as the final crumb." (1 line)
  - Tooltips: "Hover or focus a control to reveal its label. Tooltips appear on all four sides." (1 line)
  - Tabs: "Switch between related views without leaving the page." (1 line)
- **Card:** a chrome-free `GroupBox` with an id, Margin `0,22,0,0`, around a `Border Style=DemoCard`. Reuse `DemoLabel` (13/600 `Ink3`, 20.8, +16) and `Divider` (1px `Line`, 26 above and below) from `Forms.xaml`.

| Section | Card id | Demo height |
|---|---|---|
| Buttons | `ButtonsCard` | 412.4 |
| Action menu | `MenuCard` | 104 |
| Accordion | `AccordionCard` | 307.2 (370 with Team members open) |
| Stepper | `StepperCard` | 506.1 |
| Breadcrumbs | `CrumbsCard` | 82.8 |
| Tooltips | `TipsCard` | 104 |
| Tabs | `TabsCard` | 143.4 |

### Buttons (`Controls.xaml` 5–69 + the Buttons section)

**`.btn`:**
- 600 14px, line-height 1 (14), `min-height 42`, radius 8, 8px gap between the icon and the text, icon 16.
- **Width = text advance + 36** for every variant (padding 17 + border 1 on each side).

| Style | Rest | Hover | Notes |
|---|---|---|---|
| `PrimaryButton` | `Brand` bg, white, **`Shadow1`** | `BrandHover` `#8F3F20` | `BorderThickness 0`, **Padding `18,12`** (the fill spans the whole box, as in `PrimaryButtonSm`) |
| `DarkButton` | `Ink` bg, white | `#000000` | same box as Primary, no shadow |
| `SecondaryButton` | `Surface`, `Ink`, 1px `LineStrong` | `Surface2` bg, `Ink3` border | `BorderThickness 1`, Padding `17,11` (unchanged box) |
| `GhostButton` | transparent, `Ink2` | `BgDeep` bg, `Ink` text | same box as Primary |
| `DangerButton` | `Danger` bg, white | `#982F27` | same box as Primary |

**All variants:**
- **Pressed:** `RenderTransform TranslateY 1` on the template root (C).
- **Disabled:** opacity .45, no shadow, no press shift, `Cursor No`.
- **Keyboard focus:** a ring `Border` (`Margin -4`, `CornerRadius 12`, 2px `Brand`) behind the button.
- **Shadow1:** on a sibling background `Border` in the template, never on the button's `Effect` (an Effect rasterises the text; 07 rule).
- Delete the opacity hover/press triggers and the focus-border trigger.

**Template mechanics** (to avoid five template copies):
- **One template in `ClaudeButtonBase`.** It holds:
  - the ring `Border`
  - a collapsed shadow `Border` carrying `Shadow1`
  - the `Bd` border, with `Background`, `BorderBrush`, `BorderThickness` and `Padding` TemplateBound
  - the press, disabled and ring triggers
- **Each variant puts its own hover** in its own `Style.Triggers` (`IsMouseOver` → `Background`/`BorderBrush`/`Foreground` setters). That works because the template TemplateBinds those properties.
- **Primary's shadow:** Primary sets `Tag="Shadow"`. A template trigger on `Tag` shows the shadow `Border` unless `IsEnabled` is false. That's one `Tag` use, with a comment. No second template.

**UIA Names:** a `Button` whose content is a panel has an empty UIA Name, so set `AutomationProperties.Name` explicitly on:
- `BtnNewItem`/`BtnSearch`/`BtnContinue`
- the menu triggers and items
- the crumb links
- `BtnLoading`: B1's handler updates the Name to "Working…" and back to "Loading"

**Text position:** the ink must land where the web's does (±1). The ±0.5 WPF/Chromium line-height difference may need asymmetric padding (precedent: `PrimaryButtonSm` 13,8,13,6). If you change it, keep the outer box exactly the same.

**Section layout:**
- Label "Variants" at y 31; row 1 at y 67.8; divider at 135.8.
- Label "With icons" at 162.8; row 2 at 199.6; divider at 267.6.
- Label "Sizes &amp; states" at 294.6; row 3 at 331.4 (50 tall; items centred vertically).
- 12px gaps.

| Id | Content | Style | x, y | w×h |
|---|---|---|---|---|
| `BtnPrimary` | Primary action | Primary | 31, 67.8 | 128.6×42 |
| `BtnDark` | Dark | Dark | 171.6 | 65.9 |
| `BtnSecondary` | Secondary | Secondary | 249.5 | 102.8 |
| `BtnGhost` | Ghost | Ghost | 364.3 | 73.4 |
| `BtnDanger` | Danger | Danger | 449.6 | 82.6 |
| `BtnNewItem` | `IconPlus` + New item | Primary | 31, 199.6 | 121.1 (icon at +18,+13) |
| `BtnSearch` | `IconSearch` + Search | Secondary | 164.1 | 102.2 (icon +18,+13) |
| `BtnContinue` | Continue + `IconArrow` | Secondary | 278.3 | 117.7 (icon at +83.7,+13) |
| `BtnSmall` | Small | `PrimaryButtonSm` (exists) | 31, 340.4 | 58.2×32 |
| `BtnDefault` | Default | Primary | 101.2, 335.4 | 82.5×42 |
| `BtnLarge` | Large | Primary + local `FontSize 15`, `Padding 23,15`, `MinHeight 50` (one use: no new style) | 195.7, 331.4 | 83.6×50 |
| `BtnDisabled` | Disabled | Primary, `IsEnabled False` | 291.3, 335.4 | 91×42, opacity .45, no shadow |
| `BtnLoading` | spinner + Loading | Primary | 394.3, 335.4 | 110×42; "Working…" state 124.7 |

**Spinner** (`ButtonSpinner` in `Navigation.xaml`):
- 15×15 at (+18, +13.5): a 2px ring `#73FFFFFF` (rgba(255,255,255,.45)) plus the white top quarter. The `border-top` covers −45°…+45° around 12 o'clock; draw it as a `Path` arc, as in the 07 loading spinner.
- Spins 360° every 0.7s, linear (Decision 8 guard).

**Polish item (from `handoff/polish.md`):** after N1, the Progress section's "Start upload" right edge must be **144** and "Reset" must start at **157** (±1). Measure both in the Progress capture.

### Action menu (`web-nav-menu*.png`)

- **Row** at (31,31), 12 gaps, items centred on the 42 row:
  - **"Actions" trigger** (`MenuActions`): `SecondaryButton`, 107.3×42, content "Actions" + `IconChevron` 16 at +73.3, +13. The chevron is rotated 180° while open.
  - **More** (`MenuMore`, Name "More actions"): `IconButton` 32 at (150.3,36) with `IconMore` 18 at +7,+7.
  - **Hint** (`MenuResult`): `FieldHint` with `Margin 0` (12px `Ink3`, 19.2), at (194.3, 42.4). Text "Choose an action…".
- **Panel** (`ProjectMenu` / `CompactMenu`):
  - A chrome-free `GroupBox` root (`Name` "Actions" / "More actions") hosted in `PopoverLayer`.
  - **Box:** 210 wide (content never exceeds it), `Surface`, 1px `Line`, radius 12, padding 6. `Shadow2` on a sibling `Border`.
  - **Placement:** right edge = trigger right edge, top = trigger bottom + 6.
    - Project panel: rel trigger (−102.7, 48) = rel card (−71.7, 79); 210×**258**.
    - Compact panel: rel icon-btn (−178, 38); 210×**136**.
  - **Flip and clamp** (JS 3106–3124), against the full-window `PopoverLayer` bounds (= `innerWidth`/`innerHeight`), margin 8:
    - if it overflows the bottom and there's room above, open above (top = trigger top − 6 − height)
    - then clamp top into [8, H − h − 8] and left into [8, W − w − 8]
- **Item** (`<Panel>_Item<i>`, Name = label): a `Button`, 196×**37**, radius 7, Padding `10,0`, 10px gaps.
  - Icon 16 `Ink3` at (10, 10.5).
  - Label 14px `Ink`, starting at x 36.
  - Shortcut ("Ctrl E", "Ctrl D"): 12px `Ink3`, right-aligned, ending at x 186 (item right − 10).
  - **Hover or keyboard focus:** `BgDeep`.
  - **Danger** (`Delete project`, `Remove`): text and icon `Danger`; hover `DangerSoft`, but **keyboard focus stays `BgDeep`** (CSS specificity).
  - **Disabled** (`Archive`, `i-info`): opacity .45, no hover, not focusable.
- **Divider:** 1px `Line`, Margin `4,5`, 188 wide.
  - Project item tops: 0, 37, 74 | divider | 122, 159 | divider | 207 (panel-content relative, +7 for the panel rel).
- **Items:**
  - Project: Edit project (`IconEdit`, Ctrl E), Duplicate (`IconCopy`, Ctrl D), Export as CSV (`IconDownload`), —, Pin to top (`IconPin`), Archive (`IconInfo`, disabled), —, Delete project (`IconTrash`, danger).
  - Compact: Rename (`IconEdit`), Make a copy (`IconCopy`), —, Remove (`IconTrash`, danger).

### Accordion (`web-nav-accordion*.png`)

- **grid-2:** two 450 columns, 22 gap. Labels "Multiple open" / "One at a time" at y 31; accordions at y 67.8.
- **`.accordion`:** 450×208.4, `Surface`, 1px `Line`, radius 12, padding 4.
- **Items:** 440 wide. Items 2 and 3 have a 1px `Line` top border (an item is 53 = 1 + 52; the first is 52 + its panel).
- **Trigger** (`ToggleButton`, `AccordionTrigger`): 440×**52**, Padding `18,0`, 600 15px `Ink`, text at x 18. Transparent; hover `Surface2` (square corners).
  - `IconChevron` 16 `Ink3` at (406, 18). Rotated 180° when checked.
  - Ring radius 8 (Decision 11).
- **Panel:** 14px `Ink2`, `LineHeight 22.4`, wrap, Padding `18,0,18,18`.
  - 1 line = 40.4 tall. "Invite people, assign roles and manage their access to the workspace." wraps to 2 lines at 404 = 62.8.
  - Collapsed when unchecked.
- **Start state:** item 1 open in both accordions. Texts verbatim from markup 1465–1491.
- **Ids:** `AccMulti_0..2`, `AccSingle_0..2` (Name = title); panels `AccMulti_Panel0..2`, `AccSingle_Panel0..2`.

### Stepper (`web-nav-stepper*.png`)

- **Horizontal** (`StepperDemo`): at (31,31), 922×82.6.
  - **Columns:** `*,*,*,Auto`. The last step is auto = 50 (text 34 + 16 right padding). The first three are 290.7.
  - **Each step:**
    - marker 30×30 at its column's left
    - title at y +39 (marker 30 + 9): 600 14px, 22.4
    - desc at +2 below: 12px `Ink3`, 19.2
    - text right padding 16
  - **Line** (steps 1–3): 2px tall, from x +30 to column right − 10 (250.7 wide), at y +14. `Line` colour; `Brand` when that step is done. It sits under the marker (marker `ZIndex` 1).
- **Marker:** a circle, 1px border (CSS 1.5px measures 1px in Chromium at scale 1: use 1), 600 13px, centred.

  | State | Background | Border | Text | Title |
  |---|---|---|---|---|
  | done | `Brand` | `Brand` | `IconCheck` 15 white (at +7.5) | `Ink` |
  | current | `BrandSoft` | `Brand` | the number in `Brand` | `Ink` |
  | todo | `Surface` | `LineStrong` | the number in `Ink3` | `Ink3` |

- **Divider** at 139.6.
- **Controls row** at 166.6:
  - **Previous** (`StepPrev`): `SecondaryButtonSm` 76.2×32.
  - **Next step** (`StepNext`): `PrimaryButtonSm` 82.6×32 at x 119.2.
  - **Status** (`StepStatus`): `FieldHint` (Margin 0), at (213.8, 173), "Step 1 of 4".
- **Divider** at 224.6. **Label** "Vertical" at 251.6.
- **Vertical stepper** at 288.4, 922×186.7. It is static: done / current / todo.
  - **Rows:** 69.6, 69.6, 47.6 (text 4 + 22.4 + 2 + 19.2 = 47.6, plus 22 bottom padding except on the last row).
  - **Marker** at x 0; text at x 44 (marker + 14 gap), title at y +4.
  - **Line:** 2px wide at x 14, from y 30 down to the row bottom + 8 (47.6 tall). `Brand` after a done step, else `Line`.
  - **Texts:** "Account created" / "Your details are saved.", "Set up workspace" / "Invite your team and pick a plan.", "Review and finish" / "Confirm everything looks right."
- **Ids:**
  - `StepperCard`, `StepPrev`, `StepNext`, `StepStatus`
  - `Step<i>_Num` (the number TextBlock, collapsed when done) and `Step<i>_Check`, for i = 0..3

### Breadcrumbs (`web-nav-breadcrumbs.png`)

- **grid-2** at y 31, 20.8 tall rows. 8px gaps between every child.
- **Link** (`Button`, `CrumbLink` style): 13px `Ink2`, `LineHeight 20.8`; hover `Brand` + underline; ring radius 8.
- **Separator:** `IconChevron` 13×13 `Ink3`, rotated −90° (pointing right), at y +3.9. Not focusable, no UIA name.
- **Current:** 600 13px `Ink`.
- **Ellipsis:** "…" `Ink3`. The web uses `.08em` letter-spacing, which WPF lacks: give it `Width 10.6`.
- **Positions:**
  - Trail 1: Workspace 31 (62.8 w), sep 101.8, Projects 122.8 (45.3), sep 176.1, Website refresh 197.1 (89.1), sep 294.2, Settings 315.2 (47.8).
  - Trail 2: Workspace 503, sep 573.8, … 594.8 (10.6), sep 613.4, Website refresh 634.4, sep 731.5, Settings 752.5.
- **Ids:**
  - Trail 1: `CrumbWorkspace`, `CrumbProjects`, `CrumbRefresh`, `CrumbCurrent`
  - Trail 2: `Crumb2Workspace`, `Crumb2Refresh`, `Crumb2Current`
  - Trail roots: chrome-free `GroupBox`es `Crumbs1` / `Crumbs2`, Names "Breadcrumb" / "Breadcrumb with collapsed path"

### Tooltips (`web-nav-tip-*.png`)

- **Row** at (31,31), 12 gaps:
  - "Top" `TipTop` 59.2
  - "Bottom" `TipBottom` 83.6 at 102.2
  - "Left" `TipLeft` 60.4 at 197.8
  - "Right" `TipRight` 70 at 270.2 (all Secondary)
  - "Save" `TipSave` Primary 65.4 at 352.3
- **Behaviour:** `Tip.Text` / `Tip.Side` attached properties on each button. The bubble shows in `PopoverLayer` while the button `IsMouseOver` or `IsKeyboardFocusWithin`.
- **Bubble** (`TipBubble`, Name = its text): `Ink` bg, radius 6, Padding `9,6`, 12px white text, `LineHeight 19.2` → **31.2** tall. `Shadow1` on a sibling `Border`. `IsHitTestVisible False` (`pointer-events:none`).
- **Placement** (relative to the button; w = bubble width):
  - Top: (bw − w)/2, −40.2
  - Bottom: y +51
  - Left: x −(w + 9), y (42 − 31.2)/2 = 5.4
  - Right: x bw + 9, y 5.4
- **Bubble widths:** Top 75.6, Bottom 95.8, Left 76.1, Right 84.4, Save 116.5 (±2 text advance).
- The WPF bubble always draws above the page. The web hides the Right tooltip under Save: that is **W14**, and we don't replicate it.

### Tabs (`web-nav-tabs*.png`)

- **Tab row** at (31,31): 922×41, with a 1px `Line` bottom border at y 71. 4px gaps.
- **Tab** (`RadioButton`, `TabButton` style, `GroupName` "AccountTabs"): Padding `14,10`, 600 14px, **41** tall, including a 2px bottom border that overlaps the row line (`Margin 0,0,0,-1`).
  - **Unselected:** `Ink3`, transparent underline; hover `Ink`.
  - **Selected:** `Brand` text + `Brand` underline.
  - Ring radius 8.
  - `IsTabStop` bound to `IsChecked` (the roving tabindex).
- **Tabs:** Profile `TabProfile` 69 at x 31; Security `TabSecurity` 78.8 at 104; Billing `TabBilling` 67.7 at 186.8.
- **Panel** (`TabPanel`, one `TextBlock` whose text follows the selection): at y 72 with 18 top padding, so the text is at y 90. 14px `Ink2`, 22.4.
  - Texts: "Update your name, photo and public profile details." / "Manage your password and two-factor authentication." / "View invoices and update your payment method."

---

## Behaviour spec (every web branch)

**Verify column:**
- **U:** UIA assertion.
- **K:** UIA plus `PostKey` (`PostMessage` to the main HWND; VK_TAB works per the overlays O0 probe).
- **P:** capture or pixel check.
- **C:** code-only; the user checks it by hand.
- Never use `SendKeys`, `SendInput` or `keybd_event`. Shift+Tab is C.

### Buttons (3482–3490)

| # | Branch | Verify |
|---|---|---|
| B1 | Loading click: if already disabled, do nothing; otherwise `IsEnabled=false` and the content becomes spinner + "Working…". After **1600ms** (`DispatcherTimer`, one-shot) restore spinner + "Loading" and re-enable | U (Name "Working…" then, poll ≤ 2.5s, "Loading" and enabled) |
| B2 | The other buttons have no handler (the old `BtnPrimary` Invoke check stays as a smoke test) | U |
| B3 | Hover colours, the press shift, the spinner rotation | C (P for the static frame) |

### Action menu (3100–3158); one instance per trigger

| # | Branch | Verify |
|---|---|---|
| M1 | Trigger click: if closed, **open** (show the panel, position it, `ItemStatus` "expanded", rotate the chevron, focus the first **enabled** item with `BeginInvoke(Input)`); if open, **close and focus the trigger** | U |
| M2 | ArrowDown on the trigger → open and focus the first item | K |
| M3 | In the panel: Down/Up move among enabled items, **wrapping** (Archive is skipped). Focus on nothing goes to the first. Home → first, End → last | K (`~Down` P with Q3 off; Home/End K only unless Q3) |
| M4 | Escape in the panel → close, focus the trigger, `Handled` | K, P (`!MenuActions!~Escape`) |
| M5 | Tab in the panel → close without moving focus back; focus continues **after the trigger** (`trigger.MoveFocus(Next)`). Shift+Tab → close and focus the trigger (the browser lands on the trigger, since the hidden panel is skipped) | K (Tab), C (Shift+Tab) |
| M6 | Item click → `MenuResult.Text = label` (only items with an action; Archive is disabled), then close and focus the trigger | U |
| M7 | Outside click (`DropdownOutsideClick`, hooked on open, unhooked on close) → close **without** refocusing. A real click on the other trigger closes this menu through the same rule | C |
| M8 | Placement flip and clamp (spec above) | C (P for the default below-right placement) |

### Accordion (3341–3358)

| # | Branch | Verify |
|---|---|---|
| A1 | Toggle: the panel shows while checked; the chevron rotates | U (TogglePattern, then the panel is found / gone) |
| A2 | **Single** accordion: opening an item first closes every other item in that accordion. Clicking the open item closes it, leaving all closed (allowed) | U |
| A3 | Multiple accordion: items are independent | U |

### Stepper (3320–3339)

| # | Branch | Verify |
|---|---|---|
| S1 | `Render(current)`: steps before it are done (check, `Brand` line), current is current, later ones todo. Status "Step N of 4". Previous disabled at 0, Next disabled at 3 | U |
| S2 | Previous / Next move ±1 within 0..3 | U |
| S3 | A disabled focused button loses focus (WPF does this by itself; web **W17**) | C |
| S4 | The vertical stepper is static | P |

### Breadcrumbs

| # | Branch | Verify |
|---|---|---|
| C1 | Link click scrolls `ContentScroll` so the target section's title shows near the top: Workspace → "Buttons", Projects → "Cards", Website refresh → "Data table". The web uses `href="#id"` with `scroll-margin-top 24`, so put the title's section top 24 below the viewport top (title y − 46 − 24). In standalone mode the target is collapsed: do nothing (**Q8**) | U (full run: `ContentScroll` `VerticalScrollPercent` changes after Invoke on `CrumbProjects`; scroll back to 0 after) |
| C2 | Hover colour and underline | C |

### Tooltips (CSS 413–428)

| # | Branch | Verify |
|---|---|---|
| T1 | Show the bubble while the host is hovered **or** keyboard focus is within it; hide when neither holds. No delay, no fade | U (`SetFocus` on `TipTop` → `TipBubble` "Top tooltip" found; focus elsewhere → gone), P (`ContentScroll@TipTop`) |
| T2 | Four placements and the Save variant | P |
| T3 | One tab stop per tooltip button. The web's extra `tabindex=0` wrapper is **W15**: don't replicate it | K |
| T4 | Expose the tip text as `AutomationProperties.HelpText` on the button (`aria-describedby` equivalent; web lacks it, **W15**) | U |

### Tabs (3360–3380)

| # | Branch | Verify |
|---|---|---|
| X1 | Click → check that tab (the others uncheck through the group), show its text in `TabPanel`, and make it the only tab stop | U (`SelectionItemPattern.Select` on `TabSecurity` → `TabPanel` text) |
| X2 | Right/Left → next/previous tab, **wrapping**; Home/End → first/last. Each selects **and** focuses (automatic activation), `Handled` | K (VK_RIGHT/LEFT/HOME/END), P if Q3 |

---

## AutomationIds (summary)

| Area | Ids |
|---|---|
| Cards (chrome-free `GroupBox`) | `ButtonsCard`, `MenuCard`, `AccordionCard`, `StepperCard`, `CrumbsCard`, `TipsCard`, `TabsCard` |
| Buttons | `BtnPrimary`, `BtnDark`, `BtnSecondary`, `BtnGhost`, `BtnDanger`, `BtnNewItem`, `BtnSearch`, `BtnContinue`, `BtnSmall`, `BtnDefault`, `BtnLarge`, `BtnDisabled`, `BtnLoading` |
| Menu | `MenuActions`, `MenuMore`, `MenuResult`, `ProjectMenu`, `ProjectMenu_Item0..5`, `CompactMenu`, `CompactMenu_Item0..2` |
| Accordion | `AccMulti_0..2`, `AccMulti_Panel0..2`, `AccSingle_0..2`, `AccSingle_Panel0..2` |
| Stepper | `StepperDemo`, `StepPrev`, `StepNext`, `StepStatus`, `Step<i>_Num`, `Step<i>_Check` |
| Crumbs | `Crumbs1`, `Crumbs2`, `CrumbWorkspace`, `CrumbProjects`, `CrumbRefresh`, `CrumbCurrent`, `Crumb2Workspace`, `Crumb2Refresh`, `Crumb2Current` |
| Tooltips | `TipTop`, `TipBottom`, `TipLeft`, `TipRight`, `TipSave`, `TipBubble` |
| Tabs | `TabProfile`, `TabSecurity`, `TabBilling`, `TabPanel` |
| Layer | `PopoverLayer` (Name only). A `Canvas` has no UIA peer, and it is **not a capture target** because it is created lazily (Decision 5) |

**aria → UIA:**
- `aria-expanded` → `ItemStatus` "expanded"/"collapsed" on the menu triggers; `TogglePattern` on accordion triggers
- `aria-label` "More actions" → Name
- `aria-selected` → `SelectionItemPattern`
- `aria-current="page"` → `ItemStatus` "current" on `CrumbCurrent`
- tooltip → `HelpText`

**Old ids that go away:** `Slider`, `Tabs`, `Expander`, `MultiList` (per Q4), `Breadcrumbs`, `Stepper`, `BtnTooltip`, `BtnMenu`, `ActionMenu`. Replace their assertions in the phase that removes them.

---

## Capture recipe (hidden)

```powershell
wpf\build.bat nav
cd wpf\tests; .\build-tests.bat nav; cd ..\..
pwsh wpf/tests/run-hidden.ps1 -Agent nav
pwsh wpf/tests/run-hidden.ps1 -Agent nav -NoTests -Section "Buttons" -Capture "ButtonsCard=plan/wpf/ref/wpf-nav-buttons.png,ButtonsCard@BtnPrimary=plan/wpf/ref/wpf-nav-buttons-focus.png,ButtonsCard!BtnLoading=plan/wpf/ref/wpf-nav-buttons-working.png"
# popovers overflow their card: crop menu states to OverlayLayer (always exists, full window), tooltips to ContentScroll
pwsh wpf/tests/run-hidden.ps1 -Agent nav -NoTests -Section "Action menu" -Capture "MenuCard=plan/wpf/ref/wpf-nav-menu.png,OverlayLayer!MenuActions=plan/wpf/ref/wpf-nav-menu-open.png,OverlayLayer!MenuActions!~Down!~Down=plan/wpf/ref/wpf-nav-menu-down2.png,OverlayLayer!MenuMore=plan/wpf/ref/wpf-nav-menu-compact-open.png,MenuCard!MenuMore!CompactMenu_Item1=plan/wpf/ref/wpf-nav-menu-picked.png"
pwsh wpf/tests/run-hidden.ps1 -Agent nav -NoTests -Section "Tooltips" -Capture "TipsCard=plan/wpf/ref/wpf-nav-tooltips.png,ContentScroll@TipTop=plan/wpf/ref/wpf-nav-tip-top.png,ContentScroll@TipBottom=plan/wpf/ref/wpf-nav-tip-bottom.png,ContentScroll@TipLeft=plan/wpf/ref/wpf-nav-tip-left.png,ContentScroll@TipRight=plan/wpf/ref/wpf-nav-tip-right.png,ContentScroll@TipSave=plan/wpf/ref/wpf-nav-tip-save.png"
```

- `@Id` focus shows the ring as well as the tip. Compare the bubble against `web-nav-tip-*.png` and the ring against `web-nav-tooltips-focus-top.png`.
- `!MenuMore!CompactMenu_Item1` works because each step runs after the previous one settled, and the item exists once the panel is open.

| State | Capture | How |
|---|---|---|
| Every idle section | P | the `*Card` id |
| Button focus, Working… | P | `@BtnPrimary`, `!BtnLoading` (the capture happens before the 1600ms timer fires) |
| Menu open, 2× Down, Escape, compact, picked | P | `OverlayLayer!MenuActions…`, `~Down`, `~Escape` |
| Menu Home/End, tabs Left/Right/Home/End | P **only with Q3**, else K | `~Home`/`~End`/`~Left`/`~Right` |
| Accordion toggled | P | `AccordionCard!AccMulti_1!AccSingle_1` (a `ToggleButton` flips `IsChecked`; the single-mode handler must react to `Checked`, not `Click`) |
| Accordion focus | P | `AccordionCard@AccMulti_1` |
| Stepper next / last | P | `StepperCard!StepNext`, `StepperCard!StepNext!StepNext!StepNext` |
| Tabs Security | P | `TabsCard!TabSecurity` (`RadioButton` → `IsChecked = true`) |
| Tooltips | P | `ContentScroll@Tip…` |
| **Hover states, press shift, outside click, the crumb scroll animation, Shift+Tab** | **no** | no mouse and no modifier keys on the hidden desktop. C |
| **Spinner rotation** | **no** (static frame) | code review |

**Comparing:**
- Align on the card's top-left: scan for its `#E6E2D7` border.
- Popovers (`OverlayLayer` and `ContentScroll` crops): align on the card, then compare the panel/bubble `rel` numbers.
- The menu panel's left edge must be at card x − 71.7 ±1 and **not clipped**.

---

## Phases: one at a time, capture, stop for review

Agent `nav`: N0 → N1 → N2 → N3 → N4 → N5 → N6 → N7. **Buttons (N1) come first** because N2, N3 and N5 build on them. Each phase is self-contained: stop and report after it.

**N0 (audit, no code):**
- Build `nav`, run the tests (counts and wall time).
- Capture `wpf-before-nav-<buttons|menu|accordion|stepper|breadcrumbs|tooltips|tabs>.png`. Use `-Section` with the **current** titles, e.g. "Tooltips &amp; action menu", "Accordion &amp; list".
- Measure in the current Progress capture: "Start upload" right edge, "Reset" left edge.
- Measure the card's left offset inside `ContentScroll` at 1400. It confirms the Decision 5 clip numbers.
- Grep every use of the five full-size button styles.
- Re-run `plan/wpf/ref/web-nav-capture.ps1` if `web/index.html` changed since 2026-11-22.
- K probe: `PostKey` VK_HOME/VK_END/VK_LEFT/VK_RIGHT to the current `TabControl`, to learn what the native handling does. Report only.

**N1: full-size buttons + "Buttons" section.**
- `Controls.xaml` 5–69 restyle; new `Themes/Navigation.xaml` with `IconArrow` and `ButtonSpinner`; App entry only (no csproj entries).
- `MainWindow.Navigation.cs` with B1.
- The Buttons block rewrite (shell, 3 rows, labels, dividers). Rewrite the `// Buttons` assertions: scoped to `ButtonsCard`, 13 ids found, B1 with `WaitFor`.
- Q6 triggers if approved.
- **Captures:** `wpf-nav-buttons.png`, `-buttons-focus.png`, `-buttons-working.png`, plus `wpf-nav-progress.png` (`-Section "Progress bar"`, the progress card id) for the polish check.
- **Acceptance:**
  1. Card 412.4 ±1. Every button rect in the table ±1 (widths ±2).
  2. Primary action 128.6, Dark 65.9, Ghost 73.4, Danger 82.6 (the +2 fix).
  3. Colours exact (`#A64E2A`, `#26241F`, `#B0433A`, `#D5CFC0`), and the disabled button at .45.
  4. Focus ring: 2px `#A64E2A`, outer edge 4px outside the button, outer radius 12.
  5. Icons 16 at +18,+13 (Continue +83.7); spinner 15 at +18,+13.5.
  6. Text ink row ±1 of `web-nav-buttons.png`.
  7. "Start upload" right edge 144, "Reset" left edge 157 (±1).
  8. Tests pass. Report every other section whose capture moved.

**N2: `Popover.cs` + "Action menu" section** (move it directly after Buttons).
- M1–M8; `IconEdit`, `IconCopy`, `IconPin`, `IconTrash`, the menu styles.
- Delete the `ContextMenu` button and its assertion (`// Action menu button`).
- Tests: Invoke `MenuActions` → `WaitFor` focus = "Edit project". `PostKey` Down ×2 → "Export as CSV". End → "Delete project". Down → wraps to "Edit project". Escape → panel gone, focus `MenuActions`. Invoke `MenuMore`, Invoke `CompactMenu_Item1` → `MenuResult` "Make a copy". Tab (VK_TAB) closes.
  - **Every menu test closes its menu in a `finally`.**
- **Captures:** `wpf-nav-menu.png`, `-menu-open.png`, `-menu-down2.png`, `-menu-compact-open.png`, `-menu-picked.png`.
- **Acceptance:**
  1. Card 104 ±1; trigger 107.3×42, icon-btn 32 at (150.3,36), hint at (194.3,42.4).
  2. Panel 210×258 at card (−71.7, 79) ±1, **unclipped**; compact 210×136 at icon-btn (−178, 38).
  3. Items 196×37, dividers at 123/208 (panel rel), icon x 17, label x 43, shortcut right edge 193 (panel rel).
  4. Item 0 `BgDeep` when the panel opens; Archive at .45; danger `#B0433A`.
  5. Radius 12, `#E6E2D7` border, shadow visible.

**N3: "Tooltips" section** (the rest of the old "Tooltips &amp; action menu" block).
- `Tip` attached properties in `Popover.cs`, the bubble style; T1–T4.
- Delete the `ToolTip` button and its assertion.
- **Captures:** `wpf-nav-tooltips.png`, `-tip-top|bottom|left|right|save.png`.
- **Acceptance:**
  1. Card 104 ±1.
  2. Button x positions per the spec.
  3. Bubble 31.2 tall, radius 6, `#26241F`, white 12px text, at the four placements ±1 (widths ±2).
  4. The Right bubble is fully visible (W14 not replicated).
  5. T1/T4 U tests pass.

**N4: "Accordion" section** (rename it, drop the `ListBox` per Q4).
- A1–A3.
- Replace the `// Expander` and `// ListBox` assertions.
- **Captures:** `wpf-nav-accordion.png`, `-accordion-toggled.png`, `-accordion-focus.png`.
- **Acceptance:**
  1. Card 307.2 → 370 after the toggle ±1.
  2. Accordion 450×208.4, radius 12, padding 4.
  3. Trigger 52 tall, text x 18, chevron at (406,18) rotated when open.
  4. Panel 40.4 / 62.8; separators 1px `#E6E2D7`.
  5. In single mode Account closes when Notifications opens (U).

**N5: "Stepper" section.**
- S1–S4.
- Replace the `// Stepper` assertion.
- **Captures:** `wpf-nav-stepper.png`, `-stepper-next.png`, `-stepper-last.png`.
- **Acceptance:**
  1. Card 506.1 ±1.
  2. Markers 30 at x 31 / 321.7 / 612.3 / 903; lines 250.7×2 at y 45.
  3. Titles at y 70, descs at 94.4.
  4. Controls row at 166.6; status "Step 1 of 4" at 213.8.
  5. Vertical rows 69.6/69.6/47.6; lines 2×47.6 at x 45 (card rel).
  6. Colours per the state table.
  7. U: Next ×3 → "Step 4 of 4", Next disabled; Previous ×3 → Previous disabled.

**N6: "Breadcrumbs" + "Tabs" sections** (drop the `Slider` per Q4).
- C1–C2, X1–X2.
- Move the crumb styles into `Navigation.xaml` and delete `Crumb*` from `ExtraControls.xaml`.
- Replace the `// Breadcrumbs`, `// Slider` and `// TabControl` assertions.
- **Captures:** `wpf-nav-breadcrumbs.png`, `-tabs.png`, `-tabs-security.png` (+ `-tabs-focus.png` via `@TabSecurity`).
- **Acceptance:**
  1. Crumbs card 82.8; every crumb x ±1 (widths ±2); separators 13 at y +3.9, pointing right.
  2. Tabs card 143.4; tabs 69/78.8/67.7 at 31/104/186.8, 41 tall.
  3. The selected underline is 2px `#A64E2A` over the 1px line at y 71.
  4. Panel text at y 90.
  5. X2 K tests pass (wrap both ways, Home/End).

**N7: tests, cleanup, README.**
- Full run green; record the wall-time delta.
- No old nav ids, no dead handlers. `ExtraControls.xaml` keeps only the implicit styles. csproj clean.
- Update the `wpf/README.md` covered list and count.

---

## WPF quirks that apply

- **Never `Popup`/`ContextMenu`/`ToolTip`/`Window`** for these. They aren't captured, and they live outside the UIA root.
- **`PopoverLayer` content is not a visual child of the card.** Key events from a menu item don't bubble through the trigger, so handle `KeyDown` on the panel root. `DropdownTree.IsWithin` must check the panel as well as the trigger.
- **Focus on open:** `Dispatcher.BeginInvoke(DispatcherPriority.Input, …)` after adding the panel.
- **`ToggleButton`/`RadioButton`:** the `!Id` capture step sets `IsChecked` without a `Click`, so drive accordion single mode and tab selection from `Checked`/`Unchecked`.
- **`DropShadowEffect`** blurs whatever it's applied to: put it on a sibling background `Border` (buttons, menu panel, bubble).
- **Line heights:** set the measured heights explicitly (item 37, trigger 52, tab 41, step text 22.4/19.2). Don't trust font metrics (±0.5px between WPF and Chromium).
- **Tests share one app run with every other agent's tests:**
  - close every menu in a `finally`
  - move focus off a tooltip host before you continue
  - put the stepper back at step 1
  - scroll `ContentScroll` back to 0 after C1
- **Test speed rule (README):**
  - Find each card **once** and search inside it; cache the elements you reuse.
  - Menu panels and tooltip bubbles live in `PopoverLayer`, not in the card. Find them from the root **once per open**, then search inside the found `GroupBox`.
  - Use the existing `WaitFor` (`UIAutomationTests.cs` ~27), never fixed sleeps.
- **Timers:** `DispatcherTimer` only (B1).
- **C# 5:** no `$""`, `?.`, `nameof`, `=>` members or auto-property initialisers.

## Reviewer checklist

- [ ] Only the files in the ownership table changed. `Controls.xaml` changed only in lines 5–69. `MainWindow.xaml` changed only in the seven nav blocks. `MainWindow.xaml.cs`, `DropdownPrimitives.cs` and `App.xaml.cs` are untouched.
- [ ] No `Popup`/`ContextMenu`/`ToolTip`/`Expander`/`TabControl` left in the nav blocks. Shadows are on sibling Borders. The spinner has the capture and reduced-motion guard.
- [ ] The menu panel is unclipped at card x − 71.7. `PopoverLayer` is created lazily and never collapsed.
- [ ] Every B/M/A/S/C/T/X branch is implemented as written. U/K tests exist where the table says so; menus close in `finally`.
- [ ] I captured every P state myself, hidden, and compared it to the `web-nav-*` refs (±1px, exact hex). I also re-checked one capture each from toasts, overlays and progress after N1.
- [ ] C# 5. Build clean, tests pass, no commits.

---

## Web issues to decide (fix in `web/index.html` before the port, or replicate)

W1–W13 are taken (07, `polish.md`, `web-checks.mjs`), so these continue at W14. For every fix accepted, add a CDP check to `plan/wpf/web-checks.mjs` and re-run `ref/web-nav-capture.ps1`.

| # | Where | Problem | Suggested fix |
|---|---|---|---|
| **W14** | CSS 414–421 | `.tip::after` has no `z-index`, and each `.tip` is `position:relative` with no stacking order. Later siblings therefore paint over an earlier tooltip. **The Right tooltip is hidden under the Save button** (`web-nav-tip-right.png`), and a Bottom tooltip can sit under content that follows | `.tip:hover,.tip:focus-within{z-index:30}` (or `z-index` on `::after`) |
| **W15** | markup 1551–1555, CSS 414–428 | Tooltip a11y: the `.tip` span has `tabindex=0` around an already focusable button, so there are **two Tab stops per tooltip** and the first is a nameless span with its own focus ring. The tip text isn't exposed (no `role=tooltip` / `aria-describedby`). There's no Escape to dismiss, and `pointer-events:none` means you can't hover the tip (WCAG 1.4.13) | Drop the `tabindex`. Add `aria-describedby` pointing at a hidden text node. Add an Escape key handler that hides the tip until blur |
| **W16** | JS 3364 | The tab click handler hides **every** `.tabpanel` in the document, not just its own tablist's panels (latent: a second tab set on the page would break). The panels aren't focusable (APG: `tabindex=0` when the panel holds no focusables) | Hide only the panels named by the sibling tabs' `aria-controls`; add `tabindex=0` to the panels |
| **W17** | JS 3334–3335, 3486 | Focus loss: at the stepper ends the focused Previous/Next becomes `disabled`, and the Loading button disables itself on click. Focus drops to `body`, so keyboard users lose their place | Move focus to the other stepper button when the focused one disables. For Loading, use `aria-disabled="true"` (ignore clicks) instead of `disabled` |
| **W18** | markup 1503–1513, 1037 | Status not announced: `#stepStatus` and `#menuResult` aren't live regions; the current step has no `aria-current="step"`; a done step is shown only by colour and a check icon (no text) | `role="status"` on both; `aria-current="step"`; a visually hidden "completed" on done steps |
| **W19** | markup 1530–1541 | Breadcrumbs: the separator `svg`s aren't `aria-hidden`. The collapsed "…" is `aria-hidden` and can't be expanded, so the hidden levels are unreachable for everyone (low; it's a demo of the look) | `aria-hidden="true"` on the separators; leave the ellipsis as is |
| **W20** | CSS 50 | The global `:focus-visible{border-radius:4px}` changes the **element's** radius (not just the outline) on elements without their own radius: tab, accordion trigger, crumb link, `.tip` span. The accordion trigger's hover background gets rounded corners only while focused | Low; replicate (ring radius 8 here). An optional fix is to move the radius into `outline` only (it can't be done in CSS today, so leave it) |

**Recommendation (Q2):** fix W14–W18 before N3/N5/N6, as one `web` round. W19 is optional. Replicate W20.
- **If W14 is not fixed:** the WPF tooltip still draws on top (the popover layer has no sibling stacking to imitate). Accept that difference.
- **If W15 is not fixed:** WPF still gets one tab stop (a nameless focus stop is a bug, not a look).
- **If W16 is not fixed:** WPF behaves the same either way (one tab set).
- **If W17 is not fixed:** WPF loses focus the same way (S3).
- **If W18 is not fixed:** skip `ItemStatus` "current" on the step.

---

## Open questions (user / reviewer)

1. **Q1 Workers:** one `nav` worker, N0–N7 in order (recommended: five workers already share 4 test slots, and the phases are short), or two? If two, split exactly at "N1 (`Controls.xaml` button block + Buttons section) + N2 + N3" (`buttons`: `Popover.cs` too) vs "N4–N6" (`nav`). Each then owns its own blocks and its own App/csproj lines, and `Navigation.xaml` must be split into two dictionaries. *Recommend one worker.*
2. **Q2 Web fixes first:** which of W14–W20 should a `web` worker fix before the port? *Recommend W14, W15, W16, W17, W18 (CDP checks each). W19 optional; W20 replicate.*
3. **Q3 Capture keys:** the reviewer adds `~Left`, `~Right`, `~Home`, `~End` to `SendCaptureKey` in `App.xaml.cs` (4 `case` lines, reviewer-owned) so the menu Home/End and tab-arrow states can be captured. *Recommend yes.* Without them those states are K-tested only.
4. **Q4 Extras with no web counterpart:** delete the `Slider` (Tabs block) and the multi-select `ListBox` (Accordion block), with their 4 assertions? The web's multi-select is the dropdown one (task 03). *Recommend delete.*
5. **Q5 Sidebar nav and masthead:** not one of the seven sections, but they are "navigation". The WPF sidebar is a stub: six plain TextBlocks, padding 28,24 vs the web's 28,18, no icons, links or active-on-scroll, no collapse to 72px with tips, no toggle. The masthead says "version 1.0" at 44px vs "version 2.0" at 54px. *Recommend a separate later task (08 shell)*, after overlays and toasts finish: collapsing changes the root `Grid` columns that `OverlayLayer` and `ToastLayer` span. Wire the palette's "Toggle sidebar" no-op (07 Q3) then.
6. **Q6 Small buttons:** `PrimaryButtonSm`, `SecondaryButtonSm` (Display, owner archived), `GhostButtonSm` (Sheet, owner archived) and `DangerButtonSm` (Controls, `tables`) have no focus ring and no press shift. Every web `.btn` has both. *Recommend:* `nav` adds the ring (radius 11) and the press triggers to the first three in N1. `DangerButtonSm` goes in the polish round after `tables` is archived.
7. **Q7 Transitions:** skip the menu fade-in, chevron rotation animation, tooltip fade and slide, and button transitions, keeping only the spinner (as in 07 Q2)? *Recommend skip.*
8. **Q8 Breadcrumb links:** the web links jump to the Buttons / Cards / Data table sections. Port that as a `ContentScroll` scroll (C1, ~10 lines), or leave the links inert? *Recommend port it* (it is the component's behaviour, and a link that does nothing is a bug).

---

## Found in passing (not this task)

- `Themes/Overlays.xaml` `IconTable` and `IconPanel` (owned by `overlays`): the path draws the rect from x 1 to 23 (`A2,2 0 0,1 23,6`), but the sprite's `rect x=3 width=18` spans 3–21. Both icons come out 4 units too wide. Route it to `overlays`.

---

## Worker kickoff

```
You are the worker agent "nav" on the WPF pixel-parity effort (repo C:\Workspace\Projects\others\ui-design).
Read plan/wpf/README.md fully (rules, workflow, ownership, gotchas), then plan/wpf/06-navigation.md
(Decisions, Files, Visual spec, B/M/A/S/C/T/X tables, phases) and the web lines it cites
(index.html CSS 141-212, 247-298, 401-428, 449-455; markup 980-1040, 1456-1573; JS 3100-3158, 3320-3380, 3482-3490).
Also read wpf/DropdownPrimitives.cs (reuse DropdownOutsideClick/DropdownTree, never edit).
Rules: build with wpf\build.bat nav; test and capture only with wpf/tests/run-hidden.ps1 -Agent nav; never commit;
never kill processes by name; Controls.xaml is shared with "tables": re-read it before every edit and touch only lines 5-69;
in MainWindow.xaml touch only the seven nav blocks; never edit MainWindow.xaml.cs.
Do Phase N0 only (audit, no code changes), then stop and report in the README format.
```
