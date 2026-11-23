---
name: ui-components
description: Use when building NEW UI (forms, tables, dropdowns, pickers, dialogs, nav, toasts, cards) in a hub project marked 'yes' in the scope table, using the ui-design library (web = HTML/CSS/JS, desktop = WPF). Not for CLI projects, not for Manual Logger, not for any project marked NO.
---

If the task is in a project marked NO, stop: do not use this library.

The library is assumed at `ui-design/` in the hub root (a clone of the ui-design repo).

## Scope

Maintainer: replace each `<...>` placeholder with the hub's real project name before use.

| Project | Stack | Use this library? |
|---|---|---|
| Manual Logger | web | NO: legacy UI, users cannot re-learn; match existing styles, never import ui-design |
| `<desktop app>` | WPF | yes, `wpf/` |
| `<other web project>` | web | yes, `web/` |
| `<CLI projects>` | n/a | n/a |

## Rules

- Use library components. Do not invent styles, classes or controls.
- Tokens (colours, radii, shadows, fonts) are CSS custom properties in `:root` (web) and theme dictionaries in `wpf/Themes/Tokens.xaml` (WPF). Use them; never hard-code values.
- On any conflict, web wins: `web/index.html` is the source of truth.
- Never edit `ui-design/` from a consumer project. Changes belong in the ui-design repo.
- If a component is missing, say so. Do not invent one.

## Lookup protocol (read as little as possible)

Never read `ui-design/web/index.html` (212 KB) or `ui-design/wpf/MainWindow.xaml` (103 KB) whole.

**Web**
1. `grep -n "## Quick picker" ui-design/web/COMPONENTS.md`, then read about 60 lines from that line.
2. `grep -n "### <name>" ui-design/web/SNIPPETS.md`, then read that section (markup, plus the `init` line).
3. Link the library, never copy it: `ui-design/web/dist/ui.css` in `<head>` and `ui-design/web/dist/ui.js` at the end of `<body>`. These paths are relative to the consumer page (for example `../ui-design/web/dist/ui.css`); state the path you used. Copying tokens, CSS or JS into the consumer is NEVER allowed. Icons: ui.js injects the sprite; use `<svg><use href="#i-name"/></svg>`; never inline `<symbol>`s. Also link `ui-design/web/dist/theme.js` as a `<script src>` in `<head>` when the page has a theme toggle. Both themes come from the same tokens; never write a colour literal.
4. Only if a component has no snippet: `grep -n "<name>" ui-design/web/index.html`, read about 40 lines around the match.

**WPF**
1. Read `ui-design/wpf/COMPONENTS.md`. Find the component row.
2. Open only the named file, or the style key: `grep -n 'x:Key="KEY"' ui-design/wpf/Themes/*.xaml`.

## Delegating UI work

When you hand UI work to a subagent, either tell it to load this skill, or paste the scope table and the line "never read index.html whole".
