# 02 — Forms (text input, search, select, textarea, error, checkbox, radio, switch)

**Start only after task 01 (grid input) is finished and committed.** This task edits `MainWindow.xaml` and reuses the `DemoCard` and `SheetSectionDesc` styles that 01 created in `Themes/Sheet.xaml`.

Source of truth: `web/index.html`
- markup: section `#forms`, lines ~1042–1101
- CSS: `.field*`, `.input`, `.textarea`, `.select`, `.input-icon` at ~295–324; `.check*`, `.switch*` at ~325–354; `.grid-2`, `.divider`, `.demo__label` at ~153–157
- icons: `i-search`, `i-alert`, `i-check` at ~887–891

References in `plan/wpf/ref/`:
- `web-forms.png` is the idle state.
- `web-forms-focus.png` has "Full name" focused and shows the focus ring.
- `web-forms-measurements.txt` holds the measured geometry.

Read `plan/wpf/README.md` first. Its constraints apply here too.

## Why this group

Everything in this task is a primitive in the web's single **Forms** section:
- label, input, hint, error
- select, textarea
- checkbox, radio, switch

These primitives share one focus and error language. The later dropdown-family tasks (searchable select, multi-select, tag input, autocomplete) build on them, so they come first.

## Current WPF state (what's wrong)

The `Forms` section in `MainWindow.xaml` (~line 62) has only Full name and Email. Those two fields are also broken:
- "Ada Lovelace" and "you@example.com" are set as **values**, but on the web they are **placeholders**.
- There's no focus ring.
- It uses `ClaudeCard` (radius 12, with a shadow) instead of the web's `.demo` look.

The web has these, and WPF is missing them:
- Search (with icon), Role select, Message textarea, and Username (error state)
- dividers and the two-column grid
- the second "Selection controls" card: a disabled checkbox, radios, and a switch

A separate `Selection controls` **section** (~line 203) holds two checkboxes, an unstyled `ComboBox` and an unstyled `DatePicker`. The web has no such section. Its checkboxes and select belong inside Forms, and the date picker belongs to the web's own Date picker component (a later task).

## Files

| File | Change |
|---|---|
| `wpf/Themes/Forms.xaml` | **new.** `FieldInput`, `FieldInputError`, `FieldSelect`, `FieldTextArea`, `ClaudeRadio`, `ClaudeSwitch`, `DemoLabel`, `Divider` |
| `wpf/Themes/Controls.xaml` | fix `FieldLabel`, `FieldHint` and `ClaudeCheckBox` **in place**. They mirror `.field__label`, `.field__hint` and `.check` exactly, and `ClaudeCheckBox` is only used in Forms |
| `wpf/App.xaml` and `wpf/UIComponents.Wpf.csproj` | merge and register `Themes/Forms.xaml` |
| `wpf/MainWindow.xaml` | rewrite the `Forms` section; **delete** the `Selection controls` section, DatePicker included |
| `wpf/tests/UIAutomationTests.cs` | Phase 4 |
| `wpf/README.md` | Phase 4: update the covered list and the assertion count |

**Don't modify `ClaudeInput`.** Tag input, autocomplete, filter search, the drawer and the date-time field all use it, and some of them restyle it (for example `BorderThickness=0`). Adding a focus ring there would break them. `FieldInput` is its web-accurate replacement; later tasks migrate those callers and then delete `ClaudeInput`.

---

## Visual spec

All values are in DIPs at 100% scale. Colours are `Tokens.xaml` keys. The web content width is 984 (see the "Open item" in task 01, Phase 0), and the measurements below assume it.

### Section shell

| Part | Spec |
|---|---|
| Title | "Forms", `SectionTitle`, `Margin="0,46,0,0"` |
| Description | `SheetSectionDesc` (15px, line-height 24, `Ink2`, max width 501): "Labels, hints, validation, select, textarea and selection controls. All fields share the same focus and error language." It wraps to 2 lines (48px tall) |
| Card 1 (fields) | `DemoCard`, 22px below the description. Measured height **649.8** |
| Card 2 (selection) | `DemoCard`, **18px** below card 1. Measured height **191.6** |

Give the cards `AutomationProperties.AutomationId` values of `FormsCard` and `SelectionCard` so they can be captured.

### Card 1 layout, top to bottom (inner width 922)

1. **Two columns** (`.grid-2`): equal star columns with a **22px** gap, so each column is 450 wide. Use a `Grid` with a 22px middle `ColumnDefinition`.
   - "Full name": an input with the placeholder "Ada Lovelace", then the hint "As it should appear on your account."
   - "Email address": an input with the placeholder "you@example.com". No hint.
2. **Divider:** 1px `Line`, with **26px** margin above and below.
3. **Two columns:**
   - "Search": a search input with an icon and the placeholder "Find something…"
   - "Role": a select with the options Designer, Engineer and Product manager. "Designer" is selected.
4. **Divider.**
5. **Full width:** "Message", a textarea with the placeholder "Write a short note…", then the hint "Keep it under a few sentences."
6. **Divider.**
7. **Full width, error state:** "Username", an input with the **value** "ada#", then the error "Use letters and numbers only."

**Field** (`.field`) is a vertical stack with a **7px** gap: label, then control, then hint or error. For example, the Full name label is at y=0 (20.8 tall), the input at y=27.8 (45 tall) and the hint at y=79.8 (19.2 tall), for a field height of 99.

### Text styles (fix in place in `Controls.xaml`)

| Style | Spec |
|---|---|
| `FieldLabel` | 13px, SemiBold, `Ink`, `LineHeight=20.8`, `LineStackingStrategy=BlockLineHeight`, `Margin=0,0,0,7` |
| `FieldHint` | 12px, `Ink3`, `LineHeight=19.2`, BlockLineHeight, `Margin=0,7,0,0` |
| `DemoLabel` (new) | 13px, SemiBold, **`Ink3`**, `LineHeight=20.8`, `Margin=0,0,0,16` |

### `FieldInput` (TextBox): mirrors `.input`

- **Size:** height **45** (15px font, line-height 21, padding 11/13, 1px border). Set `Height=45`; don't rely on padding.
- **Colours:** font Sans 15px, `Ink`. Background `Surface`. Border 1px `LineStrong`, corner radius **8**.
- **Text position:** the text starts **14px** from the outer left edge (1px border plus 13px padding). WPF's TextBox host adds about 2px of inset, so expect a content margin of about `11,0`, then verify against the screenshot.
- **Placeholder:** carry the text in `Tag`. Show it in a TextBlock, coloured `Ink3`, at the same position as the text, visible only when `Text` is empty. It doesn't hide on focus (see "Ada Lovelace" in `web-forms-focus.png`).
- **Hover:** the border becomes `Ink3`.
- **Focus:**
  - The border becomes `Brand`.
  - A **3px `BrandSoft` ring sits outside** the border. Draw it as a `Border` behind the main one with `Margin=-3` and `CornerRadius=11`, visible only on focus.
  - The ring takes no layout space.
- `FocusVisualStyle={x:Null}`.

**`FieldInputError`** is `BasedOn FieldInput`. The border is `Danger` in **every** state: idle, hover and focus. The web's error selector outranks `:hover`/`:focus`, so focus still shows the `BrandSoft` ring but keeps the `Danger` border.

**Search variant:**
- Draw the icon over the input: 16×16 at **left 13px** (from the outer edge), vertically centred, `Ink3`.
  - Paths, in a 24-unit box: `<Ellipse>` circle cx 11, cy 11, r 7, plus the line `M20,20 L16.5,16.5`.
  - Stroke about 1.27 (1.9 scaled by 16/24), round caps.
- The text and placeholder start **39px** from the outer edge (`padding-left:38` plus the border).
- Prefer a `Grid` overlay (icon on top of a `FieldInput` with a larger left padding) over a third template.

### `FieldTextArea` (TextBox, `AcceptsReturn`, `TextWrapping=Wrap`)

- The same box, border, focus ring and placeholder as `FieldInput`, but with `MinHeight=104` and the content top-aligned. The text and placeholder start at the 11px top padding.
- Scroll bar: vertical, auto.
- **Resize grip:** the web textarea shows Chromium's grip at the bottom right and resizes **vertically**. Add a small `Thumb` styled as two short diagonal `Ink3` lines, and on `DragDelta` change `Height` (never below 104).
  - Width doesn't resize.
  - Compare the grip against `web-forms.png`; a close match is enough.

### `FieldSelect` (ComboBox)

- Closed state: the same box as `FieldInput` (45 tall, radius 8, `LineStrong` border, hover, focus ring). The selected text sits at 15px `Ink`, 14px from the left. `padding-right:38`.
- **Chevron:** 16×16 at **right 12px**, vertically centred. `Data="M6,9 L12,15 L18,9"` in a 24-unit box. `Stroke=Ink3`, about 1.33, round caps and joins.
  - This is the same chevron as task 01's `SheetSelect`. Reuse that geometry and popup style if it exists by then.
- Popup (native on the web, so it can't match exactly):
  - `Surface` background, 1px `LineStrong`, radius 8, `Shadow2`
  - items 15px, padding `13,8`
  - hover and current item `BrandSoft`

### Error line (`.field__error`)

A horizontal stack with a **5px** gap:
- **Alert icon:** 13×13, `Danger`. In a 24-unit box, paths `M12,3 L2,20 L22,20 Z` and `M12,9 L12,14 M12,17.5 L12,18`. Stroke about 0.98 (1.8 × 13/24), round caps and joins.
- **Text:** 12px, `Danger`, `LineHeight=19.2`.

The icon sits about 3px below the line top, so it's vertically centred on the text. The line is 7px below the input.

### Card 2: selection controls

1. `DemoLabel` "Selection controls".
2. **Two columns** (22px gap). Each column is a vertical stack with a **12px** gap.
   - Left: checkbox "Enable notifications" (checked), checkbox "Weekly digest", and checkbox "Unavailable option" (**disabled**).
   - Right: radio "Starter" (checked), radio "Pro" (same group), and switch "Dark mode" (on).

**`ClaudeCheckBox` (fix in place to match `.check`):**
- **Row:** 22.4 tall (14px text, line-height 22.4), `Ink`. The box is aligned to the **top** with a 1px top margin, and there's a 10px gap to the label.
- **Box:** 19×19, radius 5, border 1.5 `LineStrong`, background `Surface`.
- **Checked:** `Brand` fill and border. The check is 13×13, centred, white. In a 24-unit box it's `M5,12 L10,17 L19,7`, stroke about 1.3 (2.4 × 13/24), round caps and joins.
- **Disabled:** **only the box** gets opacity 0.45. The label stays `Ink` at full opacity, as in `web-forms.png`.
- **Keyboard focus:** a 2px `Brand` outline, 2px outside the box (a `Margin=-4` border, radius 9 = box radius 5 + 2 offset + 2 outline). Show it on `IsKeyboardFocused` only. Set `FocusVisualStyle={x:Null}`.

**`ClaudeRadio`** (new, a RadioButton based on the same template):
- The box is a 19px circle with a 1.5 `LineStrong` border.
- **Checked:** the border becomes `Brand`, the fill stays `Surface`, and a 9px `Brand` dot is centred inside. There is **no** check glyph.

**`ClaudeSwitch`** (new, a CheckBox):
- **Track:** 42×24, radius 12. `LineStrong` when off, `Brand` when on.
- **Knob:** an 18px white circle. Off, it sits at (3,3); on, at (21,3).
  - Shadow `0 1 3 rgba(0,0,0,.2)`: use a `DropShadowEffect` with BlurRadius 3, ShadowDepth 1, Direction 270, Opacity 0.2, Colour black.
- The row is 24 tall, the label is 14px `Ink`, and there's an 11px gap.
- An optional 0.18s slide is nice to have but not required.
- Same keyboard-focus outline as the checkbox, around the track.

Measured row positions inside card 2: label y=0, first row y=36.7, then rows every 34.4 (22.4 + 12). The switch row is 24 tall.

---

## AutomationIds

Keep these existing IDs:
- `FieldName`, `FieldEmail`
- `ChkNotifications`, `ChkDigest`
- `CmbRole` (now the Role select)

Add these:
- `FieldSearch`, `FieldMessage`, `FieldUsername`, `FieldUsernameError` (the error text)
- `ChkUnavailable`
- `RadStarter`, `RadPro`
- `SwDarkMode`
- `FormsCard`, `SelectionCard`

`DatePick` goes away together with the `Selection controls` section.

---

## Phases: do one, capture, stop for review

Capture the WPF side of each phase in standalone mode, so only the Forms section shows:

```powershell
Start-Process wpf\bin\Debug\UIComponents.Wpf.exe -ArgumentList '"Forms"'
pwsh plan/wpf/capture-wpf.ps1 -Id FormsCard -Out plan/wpf/ref/wpf-forms-card1.png
```

### Phase 1: field primitives and the text inputs

- Create `Forms.xaml` with `FieldInput`, `FieldInputError`, the search overlay and `Divider`. Fix `FieldLabel` and `FieldHint`.
- Rebuild the Forms section shell and card 1 with Full name, Email, Search, and Username (error). Use placeholders, not values.
- Leave the Role and Message slots as empty placeholders for now.

Captures:
- `wpf-forms-card1.png`, idle
- `wpf-forms-focus.png`, with Full name focused (use UIA `SetFocus`)

Acceptance, against `web-forms.png` and `web-forms-focus.png`:
- the input boxes are 45 tall, radius 8, with exact colours
- the text and placeholder x-offset is 14 (39 for search)
- the focus ring is 3px `BrandSoft` outside a `Brand` border
- the error border is `Danger`, and so are the icon and text

### Phase 2: select and textarea

- Add `FieldSelect` (Role, `CmbRole`) and `FieldTextArea` with its grip (Message).

Capture `wpf-forms-card1.png` again.

Acceptance:
- card 1 height is **649.8 ±1**
- every row's y position matches `web-forms-measurements.txt` within ±1
- the chevron is 12px from the right

### Phase 3: selection card

- Fix `ClaudeCheckBox`. Add `ClaudeRadio`, `ClaudeSwitch` and `DemoLabel`. Build card 2.
- **Delete** the old `Selection controls` section, DatePicker included.

Capture: `-Id SelectionCard -Out plan/wpf/ref/wpf-forms-card2.png`.

Acceptance:
- card 2 height is **191.6 ±1**
- the box, radio and switch geometry and colours match
- the disabled checkbox dims only its box

### Phase 4: tests

- Keep the existing name, email, checkbox and combobox checks. Remove the `DatePick` check.
- Add:
  1. `FieldSearch` SetValue/GetValue round-trips.
  2. `FieldMessage` accepts a value containing `\n`.
  3. `FieldUsername` has the value "ada#", and `FieldUsernameError` exists.
  4. `ChkUnavailable` has `IsEnabled == false`.
  5. `RadPro` `SelectionItemPattern.Select()` makes `RadStarter.IsSelected` false.
  6. `SwDarkMode` Toggle goes On, then Off.
  7. Selecting "Engineer" in `CmbRole` through `ExpandCollapse` and `SelectionItem` changes its value.
- Update the covered list and the assertion total in `wpf/README.md`.

Acceptance: every test passes.

---

## Reviewer checklist

- [ ] Only the listed files changed. `ClaudeInput` is untouched.
- [ ] C# 5 compliant, and the build is clean.
- [ ] Placeholders are real placeholders, not values. "ada#" is a value.
- [ ] Every captured card matches its web reference: lines and boxes ±1px, exact hex colours, text offsets, and card heights 649.8 / 191.6.
- [ ] The error border stays `Danger` on hover and focus.
- [ ] Nothing of the old `Selection controls` section or `DatePicker` is left, and no dead styles or code.
- [ ] Tests pass.
