# Plan: web dark mode

Scope: `web/` only (`web/index.html` is the source of truth). WPF is out of scope; see "WPF parity note".
Tier: complex (new theming contract, public JS API change, consumer contract change). 4 phases.

## 0. Assumptions and open questions

Defaults below are what the plan builds if the user does not answer.

Assumptions:
- A1. Consumer browsers support CSS `light-dark()` (Baseline 2024: Chromium 123+, Safari 17.5+, Firefox 120+). If not, see Q2.
- A2. **Light mode must stay pixel-identical** (WPF matches it pixel for pixel). Every new token's light value equals the literal it replaces, character for character.
- A3. Light-mode `--line-strong` vs `--surface` is 1.55:1 (below the WCAG 3:1 non-text target). Fixing light is a non-goal. Dark borders only need to match or beat light.
- A4. Cross-tab sync (`storage` event), per-component theme overrides, and a high-contrast theme are non-goals.

Open questions (each one changes the build):
- **Q1. Primary/danger button text in dark mode.** Default (A): one `--brand` per theme. Dark `--brand` = `#e08a66`, and buttons use **dark text** (`--on-brand` = `#1c1a17`, 6.61:1). This changes how buttons look in dark mode. Option B keeps white button text: add a `--brand-fill` token (dark `#b85a33`, white text 4.62:1), with `--brand` (`#e08a66`) used only for links and text. B adds 1 token and changes ~6 more call sites (`.btn--primary`, check box, switch, step marker, picker selected cell, checkbox `accent-color`).
- **Q2. Browser floor.** Default: `light-dark()`, with one palette in one place. If older browsers matter, the fallback is two blocks with duplicated dark values: `:root[data-theme=dark]` plus `@media (prefers-color-scheme: dark){ :root:not([data-theme=light]) }`. Phase 3 would then also check that the two blocks are identical.
- **Q3. Toggle states.** Default: 3 states (System / Light / Dark) on the existing `.segment`. The alternative is a 2-state button, which loses the "back to system" option.
- **Q4. Toggle exposure.** Default: consumer-facing. `setTheme` is exported, the toggle gets a `### Theme toggle` SNIPPETS section, and `dist/theme.js` ships. The alternative is showcase-only: no export, no snippet, consumers get system-following only.

## 1. Contracts

### 1.1 CSS token contract (`web/index.html` `:root` block, lines 15-45)
```css
:root{
  color-scheme: light dark;
  --bg: light-dark(#f5f4ef, #1c1a17);
  /* ...every colour token is light-dark(<current light value unchanged>, <dark>) ... */
  --radius: 12px;  /* non-colour tokens unchanged */
}
:root[data-theme="light"]{ color-scheme: light; }
:root[data-theme="dark"]{ color-scheme: dark; }
```
- No `data-theme`: follows the OS (`prefers-color-scheme`) through `color-scheme: light dark`, with no JS.
- `data-theme="light|dark"` on `<html>`: forced.
- `color-scheme` also themes native controls: `<select>`, date inputs, `accent-color`, and Firefox scrollbars.
- `light-dark()` is a `<color>`, so it also works inside shadow values.

### 1.2 Palette (existing tokens). Light values are unchanged.
| token | light | dark |
|---|---|---|
| --bg | #f5f4ef | #1c1a17 |
| --bg-deep | #eeeae0 | #161412 |
| --surface | #ffffff | #24221e |
| --surface-2 | #faf9f5 | #2a2723 |
| --ink | #26241f | #ece8df |
| --ink-2 | #5b554a | #b8b0a2 |
| --ink-3 | #6b6659 | #9a9284 |
| --line | #e6e2d7 | #34302a |
| --line-strong | #d5cfc0 | #5a5449 |
| --brand | #a64e2a | #e08a66 |
| --brand-hover | #8f3f20 | #ea9c7b |
| --brand-bright | #d97757 | #d97757 |
| --brand-soft | #f8ece4 | #3a2820 |
| --success / -soft | #356b4c / #e8f2ec | #7cc39a / #1e2e25 |
| --warning / -soft | #855a16 / #f8f0dd | #d9a952 / #2f2616 |
| --danger / -soft | #b0433a / #f9eae7 | #ec8a80 / #3a201d |
| --info / -soft | #3f6485 / #eaf0f6 | #8fb4d6 / #1d2733 |
| --focus | #a64e2a | #e08a66 |
| --shadow-1 | `0 1px 2px rgba(40,33,22,.05), 0 4px 14px rgba(40,33,22,.06)` | `0 1px 2px rgba(0,0,0,.4), 0 4px 14px rgba(0,0,0,.3)` |
| --shadow-2 | `0 8px 30px rgba(40,33,22,.16), 0 2px 8px rgba(40,33,22,.08)` | `0 8px 30px rgba(0,0,0,.55), 0 2px 8px rgba(0,0,0,.35)` |

Shadows are written per colour stop: `0 1px 2px light-dark(rgba(40,33,22,.05), rgba(0,0,0,.4)), 0 4px 14px light-dark(rgba(40,33,22,.06), rgba(0,0,0,.3))`.

### 1.3 New tokens (Phase 2). Light value is the exact literal replaced.
| token | light | dark | replaces at (current line numbers) |
|---|---|---|---|
| --on-brand | #fff | #1c1a17 | 172 `.btn--primary` color, 276 `.step.is-done .step__marker` color, 330 `.check__box` color, 497 `.picker__cell.selected` color |
| --on-ink | #fff | #1c1a17 | 102 collapsed-nav tooltip color, 174 `.btn--dark` color, 395 `.avatar--dark` color, 419 `.tip::after` color, 736 `.page-btn.active` color |
| --ink-hover | #000 | #ffffff | 175 `.btn--dark:hover` bg |
| --on-danger | #fff | #1c1a17 | 180 `.btn--danger` color |
| --danger-hover | #982f27 | #f29d94 | 181 `.btn--danger:hover` bg |
| --brand-line | #ecd4c8 | #5a3a2c | 136, 391, 708 borders |
| --brand-soft-hover | #f0d9cd | #47302a | 643, 646 `.sheet__add` hover bg |
| --success-line | #d7e7dd | #2d4636 | 540 toast border |
| --info-line | #d9e4ee | #2e3f52 | 550 `.toast--info` border |
| --danger-line | #f0dcd8 | #5a2f2a | 553 `.toast--error` border |
| --sage-soft | #e6eee7 | #24302a | 396 `.avatar--sage` bg |
| --sage-line | #d3e3d6 | #33463a | 396 `.avatar--sage` border |
| --wash | rgba(255,255,255,.6) | rgba(255,255,255,.06) | 89, 93, 123, 719 hover bg |
| --wash-strong | rgba(255,255,255,.75) | rgba(255,255,255,.08) | 546 `.toast__icon` bg |
| --scrim | rgba(30,26,20,.42) | rgba(0,0,0,.6) | 442 overlay backdrop |
| --shimmer | rgba(255,255,255,.65) | rgba(255,255,255,.07) | 508 skeleton gradient middle stop |
| --focus-ring | #f8ece4 | rgba(224,138,102,.35) | `0 0 0 3px var(--brand-soft)` at 315, 475, 570, 600, 771, 789, 810, 832 (dark `--brand-soft` is 1.14:1 against surface, so the halo would be invisible) |
| --knob | #fff | #f3efe7 | 349 `.switch__track::after` bg |
| --knob-shadow | rgba(0,0,0,.2) | rgba(0,0,0,.5) | 349 `box-shadow:0 1px 3px rgba(0,0,0,.2)` becomes `0 1px 3px var(--knob-shadow)` |

Spinner (185), with no token: `border:2px solid color-mix(in srgb, currentColor 45%, transparent); border-top-color:currentColor;`. Inside `.btn--primary` this is the same pixels in light (currentColor = #fff), and it follows `--on-brand` in dark.

### 1.4 JS contract
- localStorage key `ui-theme`. Values `"light" | "dark"`. Absent means system. All storage access is wrapped in try/catch.
- Head script in index.html, `<script data-dist="theme">`. build-dist emits it as `dist/theme.js` and keeps it **out of** `ui.js`. Body: if `<html>` has no `data-theme` and storage holds `light|dark`, set `data-theme`. It never removes an author-set attribute.
- `setTheme(mode)` with `mode: 'light' | 'dark' | 'system'`. It is a top-level function declaration in the main `<script>` (hoisting is required: the export line runs first) and is added to the `exported` list in build-dist.
- Toggle hook: a container with attribute `data-theme-toggle`, buttons with `data-theme-value="system|light|dark"`. The active button gets class `active` and `aria-pressed="true"` (existing `.segment` styling, no new CSS class).

### 1.5 Consumer contract
`<head>`: `<link rel="stylesheet" href=".../dist/ui.css">` + `<script src=".../dist/theme.js"></script>`. theme.js is blocking and tiny; it is needed only when a manual toggle is used. End of `<body>`: `dist/ui.js` (unchanged).

### 1.6 Contrast targets (WCAG AA). Phase 3 enforces them.
4.5:1 for text pairs, 3:1 for focus indicator. Both themes pass with the palette above (computed values, light / dark):
ink/bg 14.08/14.20 · ink-2/bg-deep 6.15/8.55 · ink-3/surface 5.72/5.16 · ink-3/surface-2 5.43/4.83 · ink-3/bg-deep 4.76/5.97 · brand/surface 5.61/6.05 · brand/bg 5.09/6.61 · brand/bg-deep 4.67/7.00 · brand/brand-soft 4.84/5.32 · on-brand/brand 5.61/6.61 · on-ink/ink 15.50/14.20 · on-danger/danger 5.66/7.04 · success/-soft 5.46/6.88 · warning/-soft 5.33/6.91 · danger/-soft 4.84/6.06 · info/-soft 5.42/6.95 · focus/surface (3.0 min) 5.61/6.05.

## 2. Phases

Verify gate for every phase: `node web/build-dist.mjs && node web/build-dist.mjs --check` (must print `OK:`). Do not commit unless the user asks. If you do commit, include `web/dist/` (CLAUDE.md rule).

### Phase 1: walking skeleton (switching works end to end). Tag: `hard`

Files: `web/index.html`, `web/build-dist.mjs`.

1. `web/index.html`, between `<title>` (line 6) and `<style>` (line 7), insert:
   ```html
   <script data-dist="theme">
   (function () { try { var r = document.documentElement, t = localStorage.getItem('ui-theme'); if (!r.hasAttribute('data-theme') && (t === 'light' || t === 'dark')) r.setAttribute('data-theme', t); } catch (e) {} })();
   </script>
   ```
2. `:root` block: add `color-scheme: light dark;` as the first declaration. Rewrite every token in table 1.2 as `light-dark(<light>, <dark>)`. Keep the light value text exactly as it is now. After the `:root{...}` closing brace, add the two `:root[data-theme=...]` rules from 1.1.
3. Main `<script>` (starts line 2103). Add the top-level function declarations next to `function safe` (line 2159), **not** inside `safe()`:
   ```js
   function syncThemeToggles(mode){
     document.querySelectorAll('[data-theme-toggle] [data-theme-value]').forEach(b => {
       const on = b.dataset.themeValue === mode;
       b.classList.toggle('active', on); b.setAttribute('aria-pressed', String(on));
     });
   }
   function setTheme(mode){ // 'light' | 'dark' | 'system'
     const root = document.documentElement;
     if (mode !== 'light' && mode !== 'dark') mode = 'system';
     if (mode === 'system') root.removeAttribute('data-theme'); else root.setAttribute('data-theme', mode);
     try { if (mode === 'system') localStorage.removeItem('ui-theme'); else localStorage.setItem('ui-theme', mode); } catch (e) {}
     syncThemeToggles(mode);
   }
   ```
   Then add an auto-init with the existing pattern:
   ```js
   safe('theme toggle', () => {
     document.querySelectorAll('[data-theme-toggle] [data-theme-value]').forEach(b =>
       b.addEventListener('click', () => setTheme(b.dataset.themeValue)));
     syncThemeToggles(document.documentElement.getAttribute('data-theme') || 'system');
   });
   ```
4. Showcase toggle. In `<header class="masthead">` (line 982), right after the `.chip` span, insert:
   ```html
   <div class="segment" data-theme-toggle role="group" aria-label="Colour theme"><button type="button" data-theme-value="system">System</button><button type="button" data-theme-value="light">Light</button><button type="button" data-theme-value="dark">Dark</button></div>
   ```
5. `web/build-dist.mjs`:
   - `scripts` filter: also exclude `/\bdata-dist\s*=\s*"theme"/i.test(m[1])`.
   - New: `const themeScripts = [...html.matchAll(/<script\b[^>]*\bdata-dist\s*=\s*"theme"[^>]*>([\s\S]*?)<\/script>/gi)];` If `themeScripts.length !== 1`, call `console.error` and `process.exit(1)`. `const themeJs = '// GENERATED from web/index.html - run node web/build-dist.mjs\n' + themeScripts[0][1].trim() + '\n';`
   - `outputs` becomes `{ 'ui.css': css, 'ui.js': js, 'theme.js': themeJs }`. `--check` and `dist/* eol=lf` then cover it automatically. Update the write log line to list theme.js.
   - Add `setTheme` to the `exported` string.
   - Update the header comment (lines 1-3) to mention theme.js.

Verify:
```
node web/build-dist.mjs && node web/build-dist.mjs --check
grep -c "getItem('ui-theme')" web/dist/theme.js   # 1
grep -c "getItem('ui-theme')" web/dist/ui.js      # 0
grep -c "setTheme" web/dist/ui.js                 # >= 2
grep -c "light-dark(" web/dist/ui.css             # >= 23
```
Then run the visual check (section 3). Dark should mostly render. The leftover literals (white hover washes, white-on-light tooltips, etc.) are expected until Phase 2.

### Phase 2: tokenize every hard-coded colour. Tag: `mechanical`

Files: `web/index.html` (CSS only), plus rebuilt `web/dist/*`.

1. Add each token from table 1.3 to `:root` as `light-dark(<light>, <dark>)`, grouped after `--focus`.
2. At each listed line, replace the literal with `var(--token)`. Use exactly the replacements in table 1.3 and the spinner rule. Re-locate lines with grep because Phase 1 shifted them by about 5 lines. Grep for the literal string (for example `grep -n "#ecd4c8" web/index.html`).
3. Do not change any light value, selector, or non-colour property.

Verify:
```
node web/build-dist.mjs && node web/build-dist.mjs --check
grep -nE "#[0-9a-fA-F]{3,8}\b|rgba?\(" web/dist/ui.css | grep -v "light-dark("   # expect no output (only token lines carry literals)
```
Then run the visual check (section 3), both themes. Light must look identical to the pre-change screenshot.

### Phase 3: guards in `--check` (literal lint + contrast). Tag: `hard`

File: `web/build-dist.mjs` only (about 40 lines, inside the `--check` branch).

1. **Literal lint.** Strip comments from `css`. Remove the first `:root{...}` block (from `:root{` to the first `}` after it; it contains no nested braces). Over the rest, collect declaration values with `/:\s*([^;{}]+)(?=[;}])/g`. Selector pseudo-classes end at `{`, so they never match. Flag a value if it matches `/#[0-9a-fA-F]{3,8}\b|\brgba?\(|\bhsla?\(|(?<![\w-])(white|black)(?![\w-])/i`. On any hit: `console.error('HARD-CODED COLOURS outside :root tokens: ' + list)` and set `failed = true`.
2. **Contrast.** Parse the `:root` block. For each `--name: light-dark(A, B);` where A and B are hex (3 or 6 digits), store `{light: A, dark: B}`. Implement `luminance(hex)` and `ratio(a,b)` (WCAG 2.x formula: sRGB channel `c<=0.03928 ? c/12.92 : ((c+0.055)/1.055)**2.4`, weights .2126/.7152/.0722, `(L1+.05)/(L2+.05)`). Check every pair in section 1.6 for both themes: 4.5 minimum, 3.0 for focus/surface. Treat a missing token as a failure. On failure, print `LOW CONTRAST <theme> <fg>/<bg> <ratio>` and set `failed = true`.
3. Extend the final OK line with `, colours tokenized, N contrast pairs pass`.

Verify:
```
node web/build-dist.mjs --check                        # OK line with contrast count
```
Negative test (temporary, then revert): change dark `--ink-3` to `#5a5449` in index.html and run `node web/build-dist.mjs && node web/build-dist.mjs --check`. Expect exit 1 with `LOW CONTRAST dark ink-3/...`. Then add `.x{color:#123456}` to the CSS. Expect `HARD-CODED COLOURS`. Revert both and rebuild.

### Phase 4: consumer docs and snippet. Tag: `mechanical`

Files: `web/SNIPPETS.md`, `web/COMPONENTS.md`, `skills/ui-components/SKILL.md`.

1. `web/SNIPPETS.md` line 3 (usage line): add "`dist/theme.js` as a plain `<script src>` in `<head>` (blocking, before content) only if the page offers a manual theme toggle; without it the page follows the OS theme." Add a section at the end:
   ```
   ### Theme toggle
   ```html
   <div class="segment" data-theme-toggle role="group" aria-label="Colour theme"><button type="button" data-theme-value="system">System</button><button type="button" data-theme-value="light">Light</button><button type="button" data-theme-value="dark">Dark</button></div>
   ```
   init: auto (ui.js). Programmatic: `setTheme('light'|'dark'|'system')`. Requires dist/theme.js in <head> to avoid a flash on reload.
   ```
2. `web/COMPONENTS.md`:
   - Line 9: say colour tokens are `light-dark(light, dark)` pairs. Never hard-code a colour: `--check` fails on literals outside `:root`.
   - Line 11: add `setTheme` to the exported function list.
   - Quick picker: add the row `| Let the user pick light/dark/system | Theme toggle | A custom switch |`.
   - New `### Theme — light / dark` subsection under Foundations, after Sidebar. Cover: the `data-theme` attribute, the `ui-theme` storage key, `color-scheme`, `theme.js` in head, using `--on-brand`/`--on-ink` for text on filled backgrounds, and forcing a subtree with `style="color-scheme:dark"`. The last one works because `light-dark()` resolves per element.
   - List the new tokens from table 1.3 in one line.
3. `skills/ui-components/SKILL.md` Lookup step 3: add "`ui-design/web/dist/theme.js` as a `<script src>` in `<head>` when the page has a theme toggle". Also add "both themes come from the same tokens; never write a colour literal".

Verify:
```
node web/build-dist.mjs --check     # SNIPPETS class check covers the new section (only `segment`)
grep -n "theme.js" web/SNIPPETS.md web/COMPONENTS.md skills/ui-components/SKILL.md
```

## 3. Visual check (Phases 1, 2, final)

```
SP=<scratchpad dir>                     # Git Bash path, e.g. /c/Users/...
SPW=$(cygpath -m "$SP")                 # C:/Users/... form; Edge cannot open file:////c/...
EDGE="/c/Program Files (x86)/Microsoft/Edge/Application/msedge.exe"
cd /c/Workspace/Projects/others/ui-design
sed 's/<html lang="en">/<html lang="en" data-theme="dark">/'  web/index.html > "$SP/dark.html"
sed 's/<html lang="en">/<html lang="en" data-theme="light">/' web/index.html > "$SP/light.html"
git show <commit before Phase 1>:web/index.html > "$SP/before.html"   # light baseline (ba0a7f3 at plan time)
for f in dark light before; do "$EDGE" --headless=new --disable-gpu --hide-scrollbars --window-size=1440,9000 --screenshot="$SPW/$f.png" "file:///$SPW/$f.html"; done
ls -l "$SP"/*.png    # each must be well over 100 KB; a tiny PNG = Edge error page, the check did NOT run
```
Open each PNG with the Read tool. `dark.png` must visibly have a dark background.
index.html is self-contained, so the copies render fully. Compare `light.png` and `before.png` by eye: they must be identical, except that the masthead now has the toggle. Read `dark.png`.

Manual checklist (states a static shot misses; open `web/index.html` in Edge and click Dark):
- modal, drawer, and loading modal backdrop (`--scrim`)
- action menu, combo/multi panels, date picker selected/today cell
- command palette (Ctrl+K)
- each toast variant and its icon disc
- tooltip and collapsed-nav tooltip (`--on-ink`)
- sticky table column (`--surface` bg plus line shadow), row selection, sort icons, `.page-btn.active`
- focus rings via Tab on input/combo/picker (`--focus-ring` halo visible)
- switch knob, checkbox, radio
- skeleton shimmer
- native `<select>` dropdown and the grid checkbox `accent-color`
- scrollbars
- Reload while Dark is selected: no light flash. Select System and switch the OS theme: the page follows.

## 4. Risks / do not touch

- **Light pixel parity.** Never alter a light value. Review each `light-dark(` first argument against the pre-Phase-1 `web/index.html`. The only intended light-mode visual change is the masthead toggle (see section 5).
- `setTheme`/`syncThemeToggles` must be top-level function declarations. The `exported` line runs before the scripts, and a function defined inside `safe()` would throw ReferenceError and break every export.
- The theme head script must keep the `data-dist="theme"` attribute. Without it, build-dist bundles it into ui.js, where it runs too late and does nothing.
- Do not add classes to SNIPPETS.md that have no selector: `--check` fails.
- `--brand-bright` (spark accent) keeps the same value in both themes. Its contrast is decorative only.
- Do not touch `wpf/`, `plan/wpf/`, or the JS that sets `.style.*` (layout only, no colours).
- Do not use the Chromium `--force-dark-mode` flag (auto-darkening). Use the `data-theme` copies instead.

## 5. WPF parity note (not planned here)

Each token is one name with a light and a dark value, which maps to WPF as two dictionaries (`Tokens.Light.xaml` / `Tokens.Dark.xaml`) with identical keys swapped through MergedDictionaries. The new Phase 2 token names (`--on-brand`, `--wash`, `--scrim`, ...) should become WPF keys of the same name. Their light values equal today's literals, so the WPF light theme is unchanged.
Side effect: the masthead theme toggle (Phase 1 step 4) is a new visible element in the light showcase. Until WPF theming work starts, the WPF parity board should expect this one masthead difference.
