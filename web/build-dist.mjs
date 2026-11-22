// Builds web/dist/ui.css and web/dist/ui.js from web/index.html (the source of truth).
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
  .filter(m => !/\bsrc\s*=/i.test(m[1]))
  .map(m => m[2].trim()).join('\n\n');

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
const exported = 'Object.assign(window, { openOverlay, closeOverlay, setFieldError, showToast, createGrid, bindSimpleFilters, bindColumnFilters, bindRowSelection, bindSortable, createCommandPalette });';
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

const outputs = { 'ui.css': css, 'ui.js': js };

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
  if (failed) process.exit(1);
  console.log(`OK: dist is up to date; all SNIPPETS.md classes exist in ui.css or are JS hooks in ui.js (${known.size} CSS selectors)`);
} else {
  mkdirSync(distDir, { recursive: true });
  for (const [name, text] of Object.entries(outputs)) writeFileSync(join(distDir, name), text);
  console.log(`wrote web/dist/ui.css (${Buffer.byteLength(css)} bytes), web/dist/ui.js (${Buffer.byteLength(js)} bytes)`);
}
