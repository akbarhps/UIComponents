// Theme lint for the WPF dark-mode work (plan/wpf/09-dark-mode.md, C5). No dependencies.
// Usage: node plan/wpf/theme-lint.mjs [--only C0,C1,...]
// Prints "Cn <name> violations=N" (up to 10 file:line each), then "RESULT theme-lint failed=K"; exit 1 if any selected check fails.
// Ceiling: regex/text based (no XAML or C# parser), so a colour in a comment or string can false-positive;
// silence a C# line with "theme-lint: allow". Upgrade path: a Roslyn/XML based walk.
import fs from 'fs';
import path from 'path';
import { fileURLToPath } from 'url';

const root = path.resolve(path.dirname(fileURLToPath(import.meta.url)), '..', '..');
const rel = (p) => path.relative(root, p).split(path.sep).join('/');
const read = (p) => fs.readFileSync(p, 'utf8');
const ls = (dir, ext) => {
  const d = path.join(root, dir);
  return fs.existsSync(d) ? fs.readdirSync(d).filter((f) => f.endsWith(ext)).map((f) => path.join(d, f)) : [];
};

const onlyArg = process.argv.indexOf('--only');
const only = onlyArg > 0 ? new Set(process.argv[onlyArg + 1].split(',').map((s) => s.trim().toUpperCase())) : null;

const tokensLight = path.join(root, 'wpf/Themes/Tokens.xaml');
const tokensDark = path.join(root, 'wpf/Themes/Tokens.Dark.xaml');
const webIndex = path.join(root, 'web/index.html');
const xamlFiles = [...ls('wpf', '.xaml'), ...ls('wpf/Themes', '.xaml')];
const xamlNoTokens = xamlFiles.filter((f) => f !== tokensLight && f !== tokensDark);
const csFiles = ls('wpf', '.cs');

// --- helpers ---------------------------------------------------------------
const pascal = (kebab) => kebab.split('-').map((s) => s.charAt(0).toUpperCase() + s.slice(1)).join('');
const lineOf = (text, index) => text.slice(0, index).split('\n').length;

// "#rgb" | "#rrggbb" | "rgba(r,g,b,a)" -> "AARRGGBB" (upper case), or null.
function toArgb(c) {
  c = c.trim();
  let m = /^#([0-9a-f]{3})$/i.exec(c);
  if (m) return ('FF' + m[1].split('').map((h) => h + h).join('')).toUpperCase();
  m = /^#([0-9a-f]{6})$/i.exec(c);
  if (m) return ('FF' + m[1]).toUpperCase();
  m = /^#([0-9a-f]{8})$/i.exec(c);
  if (m) return m[1].toUpperCase();
  m = /^rgba?\(\s*(\d+)\s*,\s*(\d+)\s*,\s*(\d+)\s*(?:,\s*([\d.]+)\s*)?\)$/i.exec(c);
  if (m) {
    const h = (n) => Number(n).toString(16).padStart(2, '0');
    const a = m[4] === undefined ? 255 : Math.round(parseFloat(m[4]) * 255);
    return (h(a) + h(m[1]) + h(m[2]) + h(m[3])).toUpperCase();
  }
  return null;
}

// Web :root tokens whose whole value is light-dark(a, b): [{ name, key, line, light, dark }].
function webTokens() {
  const text = read(webIndex);
  const rootBlock = /:root\s*\{/.exec(text);
  const end = text.indexOf('\n}', rootBlock.index);
  const block = text.slice(rootBlock.index, end);
  const out = [];
  const re = /^\s*--([a-z0-9-]+)\s*:\s*light-dark\(/gm;
  let m;
  while ((m = re.exec(block))) {
    let depth = 1;
    let i = m.index + m[0].length;
    const start = i;
    let comma = -1;
    for (; i < block.length && depth > 0; i++) {
      const ch = block[i];
      if (ch === '(') depth++;
      else if (ch === ')') depth--;
      else if (ch === ',' && depth === 1 && comma < 0) comma = i;
    }
    const inner = block.slice(start, i - 1);
    const after = block.slice(i).match(/^\s*;/);   // whole value must be this light-dark(...)
    if (comma < 0 || !after) continue;
    out.push({
      name: m[1], key: pascal(m[1]), line: lineOf(text, rootBlock.index + m.index + m[0].indexOf('--')),
      light: toArgb(inner.slice(0, comma - start)), dark: toArgb(inner.slice(comma - start + 1)),
    });
  }
  return out;
}

// Tokens*.xaml: { Key: { kind, argb } } for SolidColorBrush and DropShadowEffect (argb = effect colour RGB + round(Opacity*255)).
function xamlTokens(file) {
  const text = read(file);
  const out = {};
  const re = /<(SolidColorBrush|DropShadowEffect)\b([^>]*)>/g;
  let m;
  while ((m = re.exec(text))) {
    const key = /x:Key="([^"]+)"/.exec(m[2]);
    const color = /\bColor="([^"]+)"/.exec(m[2]);
    if (!key || !color) continue;
    const argb = toArgb(color[1]);
    const entry = { kind: m[1], line: lineOf(text, m.index), argb };
    if (m[1] === 'DropShadowEffect') {
      const op = /\bOpacity="([^"]+)"/.exec(m[2]);
      entry.rgb = argb ? argb.slice(2) : null;
      entry.alpha = Math.round(parseFloat(op ? op[1] : '1') * 255);
    }
    out[key[1]] = entry;
  }
  return out;
}

function sameColour(entry, argb) {
  if (!argb) return false;
  if (entry.kind === 'SolidColorBrush') return entry.argb === argb;
  return entry.rgb === argb.slice(2) && Math.abs(entry.alpha - parseInt(argb.slice(0, 2), 16)) <= 1;
}

// --- checks ----------------------------------------------------------------
const checks = {
  C0: {
    name: 'missing',   // web light-dark tokens without a PascalCase key in Tokens.xaml
    run() {
      const have = xamlTokens(tokensLight);
      const missing = webTokens().filter((t) => !(t.key in have));
      this.extra = missing.length ? '  missing: ' + missing.map((t) => t.key).join(', ') : '';
      return missing.map((t) => `web/index.html:${t.line} --${t.name} -> ${t.key}`);
    },
  },
  C1: {
    name: 'palette',   // light/dark values of keys present on both sides equal the web
    run() {
      const light = xamlTokens(tokensLight);
      const dark = fs.existsSync(tokensDark) ? xamlTokens(tokensDark) : {};
      const v = [];
      for (const t of webTokens()) {
        if (t.key in light && !sameColour(light[t.key], t.light))
          v.push(`wpf/Themes/Tokens.xaml:${light[t.key].line} ${t.key} light ${light[t.key].argb || '?'} != web ${t.light}`);
        if (t.key in light && t.key in dark && !sameColour(dark[t.key], t.dark))
          v.push(`wpf/Themes/Tokens.Dark.xaml:${dark[t.key].line} ${t.key} dark ${dark[t.key].argb || '?'} != web ${t.dark}`);
      }
      return v;
    },
  },
  C2: {
    name: 'keyset',   // brush + effect keys identical in both dictionaries
    run() {
      const a = Object.keys(xamlTokens(tokensLight));
      const b = fs.existsSync(tokensDark) ? Object.keys(xamlTokens(tokensDark)) : [];
      return [
        ...a.filter((k) => !b.includes(k)).map((k) => `wpf/Themes/Tokens.Dark.xaml: missing ${k}`),
        ...b.filter((k) => !a.includes(k)).map((k) => `wpf/Themes/Tokens.xaml: missing ${k} (only in Tokens.Dark.xaml)`),
      ];
    },
  },
  C3: {
    name: 'static',   // no {StaticResource <colour key>}
    run() {
      const keys = Object.keys(xamlTokens(tokensLight));
      if (!keys.length) return [];
      const re = new RegExp('\\{StaticResource (' + keys.join('|') + ')\\}', 'g');
      const v = [];
      for (const f of xamlFiles) {
        const text = read(f);
        let m;
        while ((m = re.exec(text))) v.push(`${rel(f)}:${lineOf(text, m.index)} ${m[0]}`);
      }
      return v;
    },
  },
  C4: {
    name: 'literals',   // no colour literals in XAML outside the token files (Transparent allowed)
    run() {
      const re = /="(#[0-9A-Fa-f]{3,8}|White|Black)"/g;
      const v = [];
      for (const f of xamlNoTokens) {
        const text = read(f);
        let m;
        while ((m = re.exec(text))) v.push(`${rel(f)}:${lineOf(text, m.index)} ${m[0]}`);
      }
      return v;
    },
  },
  C5: {
    name: 'snapshots',   // code-behind colours that read a brush once
    run() {
      const pats = [
        /Brushes\.(?!Transparent\b)\w+/,
        /Color\.From(Argb|Rgb|ScRgb)/,
        /\bColors\.(?!Transparent\b)\w+/,
        /ColorConverter/,
        /\(Brush\)\s*(Res|FindResource|TryFindResource|res\[)/,
        /\b(Res|FindResource|TryFindResource)\([^)]*\)\s*as\s+Brush/,
        /\(Brush\)\s*\w*[Rr]esources?\[/,
      ];
      const v = [];
      for (const f of csFiles) {
        read(f).split('\n').forEach((line, i) => {
          if (line.includes('theme-lint: allow')) return;
          if (pats.some((p) => p.test(line))) v.push(`${rel(f)}:${i + 1} ${line.trim().slice(0, 100)}`);
        });
      }
      return v;
    },
  },
  C6: {
    name: 'thememanager',   // ThemeManager contract: eager absolute dark load; no remove/re-add or event when the theme is unchanged
    run() {
      const text = read(path.join(root, 'wpf/ThemeManager.cs')).replace(/\r\n/g, '\n');
      const body = (re) => { const m = re.exec(text); return m ? text.slice(m.index, text.indexOf('\n        }\n', m.index)) : ''; };
      const init = body(/public static void Initialize\(\)/);
      const load = body(/private static ResourceDictionary LoadDark\(/);
      const apply = body(/private static void Apply\(bool force\)/);
      const v = [];
      if (!/LoadDark\(/.test(init)) v.push('ThemeManager.Initialize must call LoadDark eagerly (missing Tokens.Dark.xaml must fail at startup in every mode)');
      if (!/IsAbsoluteUri/.test(load) || !/IUriContext/.test(load)) v.push('ThemeManager.LoadDark must build an absolute sibling URI against the declaring dictionary (IUriContext.BaseUri)');
      if (!/if \(!force && _applied && dark == _isDark\) return;/.test(apply)) v.push('ThemeManager.Apply must return early when a System-driven apply leaves the resolved theme unchanged');
      if (!/dark && !has/.test(apply) || !/!dark && has/.test(apply)) v.push('ThemeManager.Apply must change the dictionary only when membership changes (dark && !has / !dark && has)');
      if (!/Apply\(false\)/.test(text)) v.push('ThemeManager.OnUserPreferenceChanged must call Apply(false)');
      return v;
    },
  },
};

let failed = 0;
for (const id of Object.keys(checks)) {
  if (only && !only.has(id)) continue;
  const c = checks[id];
  const v = c.run();
  console.log(`${id} ${c.name} violations=${v.length}`);
  v.slice(0, 10).forEach((l) => console.log('  ' + l));
  if (v.length > 10) console.log(`  ... ${v.length - 10} more`);
  if (c.extra) console.log(c.extra);
  if (v.length) failed++;
}
console.log(`RESULT theme-lint failed=${failed}`);
process.exit(failed ? 1 : 0);
