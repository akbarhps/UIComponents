// Builds web/dist/ui.css, web/dist/ui.js and web/dist/theme.js from web/index.html (the source of truth).
// Usage: node web/build-dist.mjs          write dist files
//        node web/build-dist.mjs --check  exit 1 if dist is stale, or a class used in SNIPPETS.md has no CSS selector
import { readFileSync, writeFileSync, mkdirSync, existsSync } from 'node:fs';
import { fileURLToPath } from 'node:url';
import { dirname, join } from 'node:path';

const here = dirname(fileURLToPath(import.meta.url));
const distDir = join(here, 'dist');
// LF on both sides: a Windows checkout with core.autocrlf turns index.html and dist/* into CRLF.
const lf = text => text.replace(/\r\n/g, '\n');
const html = lf(readFileSync(join(here, 'index.html'), 'utf8'));

const css = '/* GENERATED from web/index.html - run node web/build-dist.mjs */\n' +
  [...html.matchAll(/<style\b[^>]*>([\s\S]*?)<\/style>/gi)].map(m => m[1].trim()).join('\n\n') + '\n';

const scripts = [...html.matchAll(/<script\b([^>]*)>([\s\S]*?)<\/script>/gi)]
  .filter(m => !/\bsrc\s*=/i.test(m[1]) && !/\bdata-dist\s*=\s*"theme"/i.test(m[1]))
  .map(m => m[2].trim()).join('\n\n');

// theme.js: the tiny head script that applies a stored theme before first paint. Kept out of ui.js (it must run in <head>, not at end of body).
const themeScripts = [...html.matchAll(/<script\b[^>]*\bdata-dist\s*=\s*"theme"[^>]*>([\s\S]*?)<\/script>/gi)];
if (themeScripts.length !== 1) {
  console.error(`web/index.html must contain exactly one <script data-dist="theme"> (found ${themeScripts.length})`);
  process.exit(1);
}
const themeJs = '// GENERATED from web/index.html - run node web/build-dist.mjs\n' + themeScripts[0][1].trim() + '\n';

// Icon sprite: every <svg> block in index.html that defines <symbol id="i-..."> (main sprite and the #i-plus block).
const sprite = [...html.matchAll(/<svg\b[\s\S]*?<\/svg>/gi)].map(m => m[0])
  .filter(s => /<symbol\s+id="i-/.test(s)).join('\n');

// ui.js injects the sprite once, so consumers use <svg><use href="#i-name"/></svg> and never inline <symbol>s.
// Skipped when the page already has the sprite (index.html itself, or a page with its own copy).
const spriteLoader = `(function () {
  const sprite = ${JSON.stringify(sprite)};
  function inject() {
    if (!document.getElementById('i-check')) document.body.insertAdjacentHTML('afterbegin', sprite);
  }
  if (document.body) inject(); else document.addEventListener('DOMContentLoaded', inject);
})();`;

// One IIFE: no page globals leak in or out. Only the factories SNIPPETS.md names are exported on window.
// The export sits first. Function declarations are hoisted, and each auto-init runs inside safe() in index.html, so one failing init cannot stop the factories.
const exported = 'Object.assign(window, { openOverlay, closeOverlay, setFieldError, showToast, createGrid, bindSimpleFilters, bindColumnFilters, bindRowSelection, bindSortable, createCommandPalette, setTheme });';
const js = '// GENERATED from web/index.html - run node web/build-dist.mjs\n(function () {\n' +
  '  if (document.documentElement.dataset.uiLoaded) return; // second copy: would bind every component twice\n' +
  '  document.documentElement.dataset.uiLoaded = "1";\n' +
  exported + '\n' + spriteLoader + '\n\n' + scripts + '\n})();\n';

// Every class selector in the CSS: selector text is what sits between a '}' or ';' and the next '{'.
function cssClasses(text) {
  const selectors = [...text.replace(/\/\*[\s\S]*?\*\//g, '').matchAll(/([^{};]+)\{/g)].map(m => m[1]).join('\n');
  return new Set([...selectors.matchAll(/\.([A-Za-z_-][\w-]*)/g)].map(m => m[1]));
}

// Classes ui.js uses as JS hooks (quoted strings or .selectors inside quotes) but styles nowhere.
function jsHookClasses(text) {
  const hooks = new Set();
  for (const m of text.matchAll(/'([^'\n]*)'|"([^"\n]*)"|`([^`\n]*)`/g)) {
    const str = m[1] ?? m[2] ?? m[3];
    for (const tok of str.split(/[\s,]+/)) {
      if (/^[A-Za-z_][\w-]*$/.test(tok)) hooks.add(tok);
      for (const d of tok.matchAll(/\.([A-Za-z_][\w-]*)/g)) hooks.add(d[1]);
    }
  }
  return hooks;
}

const outputs = { 'ui.css': css, 'ui.js': js, 'theme.js': themeJs };

if (process.argv.includes('--check')) {
  let failed = false;
  for (const [name, text] of Object.entries(outputs)) {
    const file = join(distDir, name);
    if (!existsSync(file) || lf(readFileSync(file, 'utf8')) !== text) {
      console.error(`STALE: web/dist/${name} differs from web/index.html - run node web/build-dist.mjs`);
      failed = true;
    }
  }
  const snippetsFile = join(here, 'SNIPPETS.md');
  const known = cssClasses(css);
  const hooks = jsHookClasses(scripts);
  const unknown = new Set();
  for (const m of readFileSync(snippetsFile, 'utf8').matchAll(/class="([^"]*)"/g)) {
    for (const cls of m[1].split(/\s+/).filter(Boolean)) if (!known.has(cls) && !hooks.has(cls)) unknown.add(cls);
  }
  if (unknown.size) {
    console.error('UNKNOWN CLASSES in web/SNIPPETS.md (no selector in dist/ui.css and no JS hook in dist/ui.js): ' + [...unknown].join(', '));
    failed = true;
  }
  // Literal lint: every colour lives in the first :root{} block (it has no nested braces); anywhere else is a hard-coded colour.
  const bare = css.replace(/\/\*[\s\S]*?\*\//g, '');
  const rootStart = bare.indexOf(':root{');
  const rootEnd = bare.indexOf('}', rootStart);
  const rootBlock = bare.slice(rootStart, rootEnd);
  const rest = bare.slice(0, rootStart) + bare.slice(rootEnd + 1);
  const literal = /#[0-9a-fA-F]{3,8}\b|\brgba?\(|\bhsla?\(|(?<![\w-])(white|black)(?![\w-])/i;
  const hits = [...rest.matchAll(/:\s*([^;{}]+)(?=[;}])/g)].map(m => m[1].trim()).filter(v => literal.test(v));
  if (hits.length) {
    console.error('HARD-CODED COLOURS outside :root tokens: ' + hits.join(' | '));
    failed = true;
  }

  // Contrast: WCAG 2.x AA on the light/dark pairs of --name: light-dark(A, B) hex tokens.
  const tokens = {};
  for (const m of rootBlock.matchAll(/--([\w-]+)\s*:\s*light-dark\(\s*(#[0-9a-fA-F]{3}|#[0-9a-fA-F]{6})\s*,\s*(#[0-9a-fA-F]{3}|#[0-9a-fA-F]{6})\s*\)/g)) {
    tokens[m[1]] = { light: m[2], dark: m[3] };
  }
  const luminance = hex => {
    let h = hex.slice(1);
    if (h.length === 3) h = [...h].map(c => c + c).join('');
    const [r, g, b] = [0, 2, 4].map(i => parseInt(h.slice(i, i + 2), 16) / 255)
      .map(c => (c <= 0.03928 ? c / 12.92 : ((c + 0.055) / 1.055) ** 2.4));
    return 0.2126 * r + 0.7152 * g + 0.0722 * b;
  };
  const ratio = (a, b) => {
    const [hi, lo] = [luminance(a), luminance(b)].sort((x, y) => y - x);
    return (hi + 0.05) / (lo + 0.05);
  };
  // [foreground token, background token, minimum ratio]: 4.5 text, 3.0 focus indicator.
  const pairs = [
    ['ink', 'bg'], ['ink-2', 'bg-deep'], ['ink-3', 'surface'], ['ink-3', 'surface-2'], ['ink-3', 'bg-deep'],
    ['brand', 'surface'], ['brand', 'bg'], ['brand', 'bg-deep'], ['brand', 'brand-soft'],
    ['on-brand', 'brand'], ['on-ink', 'ink'], ['on-danger', 'danger'],
    ['success', 'success-soft'], ['warning', 'warning-soft'], ['danger', 'danger-soft'], ['info', 'info-soft'],
    ['focus', 'surface', 3.0],
  ];
  let passed = 0;
  for (const theme of ['light', 'dark']) {
    for (const [fg, bg, min = 4.5] of pairs) {
      if (!tokens[fg] || !tokens[bg]) {
        console.error(`LOW CONTRAST ${theme} ${fg}/${bg} missing token in :root`);
        failed = true;
        continue;
      }
      const r = ratio(tokens[fg][theme], tokens[bg][theme]);
      if (r < min) {
        console.error(`LOW CONTRAST ${theme} ${fg}/${bg} ${r.toFixed(2)} (min ${min})`);
        failed = true;
      } else passed++;
    }
  }

  if (failed) process.exit(1);
  console.log(`OK: dist is up to date; all SNIPPETS.md classes exist in ui.css or are JS hooks in ui.js (${known.size} CSS selectors), colours tokenized, ${passed} contrast pairs pass`);
} else {
  mkdirSync(distDir, { recursive: true });
  for (const [name, text] of Object.entries(outputs)) writeFileSync(join(distDir, name), text);
  console.log(`wrote web/dist/ui.css (${Buffer.byteLength(css)} bytes), web/dist/ui.js (${Buffer.byteLength(js)} bytes), web/dist/theme.js (${Buffer.byteLength(themeJs)} bytes)`);
}
