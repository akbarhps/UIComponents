// CDP regression checks for web/index.html. Node only, no dependencies.
// Usage: node plan/wpf/web-checks.mjs   (exit code 1 if any check fails)
import { spawn, spawnSync } from 'node:child_process';
import { mkdtempSync, rmSync } from 'node:fs';
import { tmpdir } from 'node:os';
import { join } from 'node:path';

const edge = 'C:/Program Files (x86)/Microsoft/Edge/Application/msedge.exe';
const pageUrl = new URL('../../web/index.html', import.meta.url).href;
const port = 9500 + Math.floor(Math.random() * 400);
const profile = mkdtempSync(join(tmpdir(), 'webchecks-'));
const child = spawn(edge, [
  '--headless=new', '--disable-gpu', '--hide-scrollbars', '--force-device-scale-factor=1',
  `--remote-debugging-port=${port}`, `--user-data-dir=${profile}`,
  '--window-size=1400,1100', 'about:blank'
], { stdio: 'ignore' });
const sleep = ms => new Promise(r => setTimeout(r, ms));
let ws;

async function connect(){
  let targets;
  for(let i = 0; i < 60; i++){
    try { targets = await (await fetch(`http://127.0.0.1:${port}/json`)).json(); break; }
    catch { await sleep(200); }
  }
  if(!targets) throw new Error('Edge CDP endpoint never came up');
  const t = targets.find(x => x.type === 'page');
  ws = new WebSocket(t.webSocketDebuggerUrl);
  await new Promise((res, rej) => { ws.onopen = res; ws.onerror = rej; });
}

let nextId = 0;
const pending = new Map();
const consoleErrors = [];
function onMessage(m){
  const d = JSON.parse(m.data);
  if(d.id && pending.has(d.id)){ pending.get(d.id)(d); pending.delete(d.id); return; }
  if(d.method === 'Runtime.exceptionThrown')
    consoleErrors.push('EXCEPTION: ' + (d.params.exceptionDetails.exception?.description || d.params.exceptionDetails.text));
  if(d.method === 'Runtime.consoleAPICalled' && d.params.type === 'error')
    consoleErrors.push('console.error: ' + d.params.args.map(a => a.value ?? a.description).join(' '));
  if(d.method === 'Log.entryAdded' && d.params.entry.level === 'error')
    consoleErrors.push('log: ' + d.params.entry.text + ' ' + (d.params.entry.url || ''));
}
const send = (method, params = {}) => new Promise(r => {
  const id = ++nextId; pending.set(id, r); ws.send(JSON.stringify({ id, method, params }));
});
async function ev(expr){
  const r = await send('Runtime.evaluate', { expression: expr, returnByValue: true, awaitPromise: true, userGesture: true });
  if(r.result.exceptionDetails) throw new Error(r.result.exceptionDetails.exception?.description || r.result.exceptionDetails.text);
  return r.result.result.value;
}
const click = async (x, y) => {
  await send('Input.dispatchMouseEvent', { type: 'mouseMoved', x, y });
  for(const type of ['mousePressed', 'mouseReleased'])
    await send('Input.dispatchMouseEvent', { type, x, y, button: 'left', clickCount: 1 });
};
const center = async expr => {
  const r = JSON.parse(await ev(`JSON.stringify(${expr}.getBoundingClientRect())`));
  return [r.x + r.width / 2, r.y + r.height / 2];
};
const tab = async () => {
  for(const type of ['rawKeyDown', 'keyUp'])
    await send('Input.dispatchKeyEvent', { type, key: 'Tab', code: 'Tab', windowsVirtualKeyCode: 9, nativeVirtualKeyCode: 9 });
};
const scroll = async id => { await ev(`document.getElementById('${id}').scrollIntoView({block:'center',behavior:'instant'})`); await sleep(80); };

let failures = 0;
async function check(name, fn){
  try {
    const { ok, detail } = await fn();
    console.log(`${ok ? 'PASS' : 'FAIL'} ${name} — ${detail}`);
    if(!ok) failures++;
  } catch(err){
    failures++;
    console.log(`FAIL ${name} — EXCEPTION: ${err.message}`);
  }
}

async function main(){
  await connect();
  ws.onmessage = onMessage;
  await send('Runtime.enable'); await send('Log.enable'); await send('Page.enable');
  await send('Emulation.setFocusEmulationEnabled', { enabled: true });
  await send('Page.navigate', { url: pageUrl });
  await sleep(1800);
  await ev(`document.documentElement.style.scrollBehavior='auto';
    window.__kd=(el,k,o={})=>el.dispatchEvent(new KeyboardEvent('keydown',{key:k,bubbles:true,cancelable:true,...o}));
    window.__ty=(el,v)=>{el.value=v;el.dispatchEvent(new Event('input',{bubbles:true}));};`);

  await check('A1', async () => {
    await ev(`document.getElementById('openLoading').click()`);
    await ev(`__kd(document.body,'k',{ctrlKey:true})`);
    const overLoading = await ev(`document.getElementById('cmdkOverlay').classList.contains('open')`);
    await ev(`closeOverlay(document.getElementById('loadingOverlay')); document.getElementById('cmdkOverlay').classList.remove('open')`);
    await ev(`document.getElementById('openCmdk').focus();
      __kd(document.body,'k',{ctrlKey:true}); __kd(document.body,'k',{ctrlKey:true}); __kd(document.body,'Escape')`);
    const focus = await ev(`document.activeElement.id`);
    return { ok: overLoading === false && focus === 'openCmdk',
      detail: `cmdk over loading modal=${overLoading}; focus after Ctrl+K x2 + Esc=${focus} (want openCmdk)` };
  });

  await check('A2', async () => {
    await scroll('tag-input');
    await click(...await center(`document.querySelector('.tag-input__field')`));
    await send('Input.insertText', { text: 'zeta' });
    await click(...await center(`document.querySelector('.tag-input-wrap .tag button')`));
    await sleep(120);
    const tags = await ev(`[...document.querySelector('.tag-input-wrap').querySelectorAll('.tag')].map(t=>t.textContent).join('|')`);
    return { ok: tags === 'research', detail: `tags after X with "zeta" pending: ${tags} (want research)` };
  });

  await check('A3', async () => {
    const r = await ev(`(() => {
      document.querySelector('#sheet thead [data-col="0"]').click();
      const p = document.querySelector('#tag-input .section__head p');
      const rg = document.createRange(); rg.selectNodeContents(p);
      getSelection().removeAllRanges(); getSelection().addRange(rg);
      const dt = new DataTransfer();
      const e = new ClipboardEvent('copy',{bubbles:true,cancelable:true,clipboardData:dt});
      p.dispatchEvent(e);
      const copy = { prevented: e.defaultPrevented, data: dt.getData('text/plain') };
      getSelection().removeAllRanges();
      const dt2 = new DataTransfer(); dt2.setData('text','X\\tY');
      const e2 = new ClipboardEvent('paste',{bubbles:true,cancelable:true,clipboardData:dt2});
      p.dispatchEvent(e2);
      return JSON.stringify({ copy, pastePrevented: e2.defaultPrevented });
    })()`);
    const o = JSON.parse(r);
    return { ok: o.copy.prevented === false && o.copy.data === '' && o.pastePrevented === false,
      detail: `copy prevented=${o.copy.prevented} data=${JSON.stringify(o.copy.data)}; paste on page text prevented=${o.pastePrevented} (all want false/empty)` };
  });

  await check('A4', async () => {
    const r = await ev(`(() => {
      const p = document.querySelector('.picker[data-mode="year"]');
      p.querySelector('.picker__trigger').click();
      const before = p.querySelector('.picker__title').textContent;
      p.querySelector('[data-next]').click();
      const after = p.querySelector('.picker__title').textContent;
      p.querySelector('.picker__trigger').click();
      return JSON.stringify({ before, after, delta: parseInt(after) - parseInt(before) });
    })()`);
    const o = JSON.parse(r);
    return { ok: o.delta === 20, detail: `${o.before} -> ${o.after}, delta=${o.delta} (want 20)` };
  });

  await check('A5', async () => {
    const r = await ev(`(() => {
      const p = document.querySelector('.picker');
      const t = p.querySelector('.picker__trigger'); t.click();
      __kd(p,'Escape');
      return JSON.stringify({ hidden: p.querySelector('.picker__panel').hidden,
        expanded: t.getAttribute('aria-expanded'), focus: document.activeElement === t });
    })()`);
    const o = JSON.parse(r);
    return { ok: o.hidden && o.expanded === 'false' && o.focus,
      detail: `panel hidden=${o.hidden}, aria-expanded=${o.expanded}, trigger focused=${o.focus}` };
  });

  await check('A6', async () => {
    const r = await ev(`(() => {
      const zone = document.querySelector('.file-input[data-multiple="true"] .file-drop');
      const dt = new DataTransfer();
      dt.items.add(new File(['x'],'notes.txt',{type:'text/plain'}));
      dt.items.add(new File([new Uint8Array([1,2])],'pic.png',{type:'image/png'}));
      zone.dispatchEvent(new DragEvent('drop',{bubbles:true,cancelable:true,dataTransfer:dt}));
      return JSON.stringify([...document.querySelectorAll('.file-input[data-multiple="true"] .file-item__name')].map(n=>n.textContent));
    })()`);
    const names = JSON.parse(r);
    return { ok: names.length === 1 && names[0] === 'pic.png',
      detail: `dropped text/plain + image/png -> ${names.join(',')} (want only pic.png)` };
  });

  await check('A7', async () => {
    const r = await ev(`(() => {
      const p = document.querySelector('.progress');
      const a = { role:p.getAttribute('role'), min:p.getAttribute('aria-valuemin'),
        max:p.getAttribute('aria-valuemax'), now:p.getAttribute('aria-valuenow'),
        labelled:p.getAttribute('aria-labelledby') };
      setProgress(42); a.after = p.getAttribute('aria-valuenow'); setProgress(0);
      return JSON.stringify(a);
    })()`);
    const o = JSON.parse(r);
    return { ok: o.role === 'progressbar' && o.min === '0' && o.max === '100' && o.now === '0'
        && o.labelled === 'progressLabel' && o.after === '42',
      detail: `role=${o.role} min=${o.min} max=${o.max} now=${o.now} labelledby=${o.labelled} after setProgress(42)=${o.after}` };
  });

  await check('A8', async () => {
    const r = await ev(`(() => {
      const btns = [...document.querySelectorAll('#sortTable [data-sort-key]')];
      const th = b => b.closest('th');
      btns[0].click(); const a = th(btns[0]).getAttribute('aria-sort');
      btns[0].click(); const b = th(btns[0]).getAttribute('aria-sort');
      btns[1].click();
      return JSON.stringify({ a, b, old: th(btns[0]).getAttribute('aria-sort'), next: th(btns[1]).getAttribute('aria-sort') });
    })()`);
    const o = JSON.parse(r);
    return { ok: o.a === 'ascending' && o.b === 'descending' && !o.old && o.next === 'ascending',
      detail: `first click=${o.a}, second=${o.b}, after other header: old=${o.old}, new=${o.next}` };
  });

  await check('A9', async () => {
    const r = await ev(`JSON.stringify([...document.querySelectorAll('.combo__input')].map(i => ({
      role:i.getAttribute('role'), controls:i.getAttribute('aria-controls'),
      expanded:i.getAttribute('aria-expanded'), autocomplete:i.getAttribute('aria-autocomplete') })))`);
    const o = JSON.parse(r);
    const seq = await ev(`(() => {
      const c = document.querySelector('.combo'), s = c.querySelector('.combo__input'), t = c.querySelector('.combo__trigger');
      const out = [];
      t.click(); out.push(s.getAttribute('aria-expanded'));
      __kd(s,'Escape'); out.push(s.getAttribute('aria-expanded'));
      t.click(); out.push(s.getAttribute('aria-expanded'));
      document.body.click(); out.push(s.getAttribute('aria-expanded'));
      t.click(); c.querySelectorAll('.combo__option')[0].click(); out.push(s.getAttribute('aria-expanded'));
      t.click(); s.dispatchEvent(new FocusEvent('focusout',{bubbles:true,relatedTarget:document.body})); out.push(s.getAttribute('aria-expanded'));
      return JSON.stringify(out);
    })()`);
    const states = JSON.parse(seq);
    const ok = o.length >= 2 && o[0].role === 'combobox' && o[0].controls === 'assigneeList'
        && o[0].expanded === 'false' && o[0].autocomplete === 'list'
        && o[1].controls === 'projectComboList' && o[1].expanded === 'false'
        && states.join(',') === 'true,false,true,false,false,false';
    return { ok, detail: `static=${JSON.stringify(o)}; expanded(open,Escape,outside,select,focusout sequence after reopen)=${states.join(',')}` };
  });

  await check('A10', async () => {
    const r = await ev(`JSON.stringify([...document.querySelectorAll('.multi__trigger')].map(t => t.getAttribute('aria-labelledby')))`);
    const o = JSON.parse(r);
    return { ok: o[0] === 'skillsLabel' && o[1] === 'tagsLabel', detail: JSON.stringify(o) };
  });

  await check('A11', async () => {
    const r = await ev(`(() => {
      const tabs = [...document.querySelector('[role=tablist]').querySelectorAll('.tab')];
      const snap = () => tabs.map(t => (t.getAttribute('aria-selected') === 'true' ? 'T' : '-') + t.tabIndex).join('');
      const initial = snap();
      __kd(tabs[0],'ArrowRight'); const right = snap() + '/' + document.activeElement.id;
      __kd(tabs[2],'Home'); const home = snap() + '/' + document.activeElement.id;
      __kd(tabs[0],'End'); const end = snap() + '/' + document.activeElement.id;
      __kd(tabs[2],'ArrowLeft'); const left = snap() + '/' + document.activeElement.id;
      return JSON.stringify({ initial, right, home, end, left });
    })()`);
    const o = JSON.parse(r);
    const ok = o.initial === 'T0--1--1' && o.right === '--1T0--1/t-2' && o.home === 'T0--1--1/t-1'
      && o.end === '--1--1T0/t-3' && o.left === '--1T0--1/t-2';
    return { ok, detail: JSON.stringify(o) + ' (T=selected, digit=tabindex)' };
  });

  await check('A12', async () => {
    const r = await ev(`(() => {
      const cfgs = [['openFormModal','fm-name','cancelFormModal'],['openDrawer','dr-email','cancelDrawer'],['openEditDrawer','ed-name','cancelEditDrawer']];
      return JSON.stringify(cfgs.map(([openBtn,id,cancelBtn]) => {
        document.getElementById(openBtn).click();
        const inp = document.getElementById(id), original = inp.value, field = inp.closest('.field');
        inp.value = 'TEMP-VALUE'; field.classList.add('has-error');
        document.getElementById(cancelBtn).click();
        document.getElementById(openBtn).click();
        const out = { id, restored: document.getElementById(id).value === original,
          clean: !document.getElementById(id).closest('.field').classList.contains('has-error') };
        document.getElementById(cancelBtn).click();
        return out;
      }));
    })()`);
    const o = JSON.parse(r);
    return { ok: o.every(x => x.restored && x.clean), detail: JSON.stringify(o) };
  });

  await check('B1', async () => {
    const r = await ev(`(() => {
      const t = document.querySelector('.picker__trigger'), v = t.querySelector('.picker__value');
      const a = t.getBoundingClientRect(), b = v.getBoundingClientRect();
      return JSON.stringify({ svg: t.querySelectorAll('svg').length, iconSvgs: t.querySelectorAll('svg use').length,
        triggerX: Math.round(a.x*10)/10, valueX: Math.round(b.x*10)/10, shift: Math.round((b.x-a.x)*10)/10 });
    })()`);
    const o = JSON.parse(r);
    return { ok: o.svg >= 1 && o.shift > 16,
      detail: `svg=${o.svg}, icon uses=${o.iconSvgs}, trigger x=${o.triggerX}, value x=${o.valueX}, shift=${o.shift}px (want icon drawn and shift>16)` };
  });

  await check('C1', async () => {
    await scroll('multi-select');
    await click(...await center(`document.querySelector('.multi__trigger')`));
    await sleep(80);
    await scroll('combobox');
    await click(...await center(`document.querySelector('.combo__trigger')`));
    await sleep(120);
    const r = await ev(`JSON.stringify({ active: document.activeElement.className,
      tag: document.activeElement.tagName, comboOpen: !document.querySelector('.combo__panel').hidden })`);
    const o = JSON.parse(r);
    return { ok: o.active.includes('combo__input') && o.comboOpen,
      detail: `real mouse: multi open then click combo trigger -> focus=${o.tag}.${o.active}, combo open=${o.comboOpen} (want INPUT.combo__input)` };
  });

  await check('C2', async () => {
    const r = await ev(`(() => {
      const ac = document.querySelector('.ac[data-multiple="true"]'), i = ac.querySelector('.ac__input');
      i.focus(); __ty(i,'React'); __kd(i,'Escape'); __kd(i,'Enter');
      __ty(i,'react'); __kd(i,'Escape'); __kd(i,'Enter');
      const out = [...ac.querySelectorAll('.tag')].map(t=>t.textContent);
      i.blur(); return JSON.stringify(out);
    })()`);
    const o = JSON.parse(r);
    return { ok: o.length === 1 && o[0] === 'React', detail: `tags=${o.join('|')} (want React)` };
  });

  await check('C3', async () => {
    const r = await ev(`(() => {
      const ac = document.querySelector('#autocomplete .ac[data-multiple]');
      ac.querySelectorAll('.tag button').forEach(b => b.click());
      const h = document.querySelector('#autocomplete .ac__hint'), f = h.closest('.field');
      const a = h.getBoundingClientRect(), b = f.getBoundingClientRect();
      return JSON.stringify({ text:h.textContent, h:Math.round(a.height*10)/10,
        top:Math.round((a.y-b.y)*10)/10, cardH:Math.round(b.height*10)/10 });
    })()`);
    const o = JSON.parse(r);
    return { ok: o.text === '0 selected' && o.h > 0,
      detail: `hint="${o.text}" height=${o.h}px top-in-card=${o.top}px card height=${o.cardH}px` };
  });

  await check('C4', async () => {
    const r = await ev(`(() => {
      const c = document.querySelector('.combo'); c.querySelector('.combo__trigger').click();
      const s = c.querySelector('.combo__input'); __kd(s,'ArrowDown'); __ty(s,'a');
      return JSON.stringify({ active: c.querySelectorAll('.combo__option.is-active').length,
        aad: s.getAttribute('aria-activedescendant') });
    })()`);
    const o = JSON.parse(r);
    return { ok: o.active === 0 && !o.aad, detail: `after filter: is-active=${o.active}, aria-activedescendant=${o.aad}` };
  });

  await check('C5', async () => {
    const r = await ev(`(() => {
      const c = document.querySelector('.combo'), s = c.querySelector('.combo__input');
      __kd(s,'Escape'); c.querySelector('.combo__trigger').click();
      __kd(s,'ArrowUp');
      const all = [...c.querySelectorAll('.combo__option')];
      return JSON.stringify({ index: all.findIndex(o => o.classList.contains('is-active')), total: all.length });
    })()`);
    const o = JSON.parse(r);
    return { ok: o.index === o.total - 1, detail: `ArrowUp from none -> index ${o.index} of ${o.total} (want last)` };
  });

  await check('C6', async () => {
    await scroll('combobox');
    const C = `document.querySelector('.combo')`;
    await ev(`__kd(${C}.querySelector('.combo__input'),'Escape')`);
    const closedAtStart = await ev(`${C}.querySelector('.combo__panel').hidden`);
    await click(...await center(`${C}.querySelector('.combo__trigger')`));
    await sleep(100);
    const valueBefore = await ev(`${C}.querySelector('.combo__value').textContent`);
    await click(...await center(`${C}.querySelectorAll('.combo__option')[1]`));
    await sleep(120);
    const valueAfter = await ev(`${C}.querySelector('.combo__value').textContent`);
    const closedAfterPick = await ev(`${C}.querySelector('.combo__panel').hidden`);
    await click(...await center(`${C}.querySelector('.combo__trigger')`));
    await sleep(80);
    await click(...await center(`${C}.querySelector('.combo__search svg')`));
    await sleep(80);
    const openAfterIcon = await ev(`!${C}.querySelector('.combo__panel').hidden`);
    let tabPresses = 0;
    while(tabPresses < 4 && !(await ev(`${C}.querySelector('.combo__panel').hidden`))){
      await tab(); tabPresses++; await sleep(100);
    }
    const closedAfterTab = await ev(`${C}.querySelector('.combo__panel').hidden`);

    await scroll('autocomplete');
    const AC = `document.querySelector('.ac[data-multiple="true"]')`;
    await click(...await center(`${AC}.querySelector('.ac__input')`));
    await sleep(80);
    const acWasOpen = await ev(`!${AC}.querySelector('.ac__list').hidden`);
    await click(...await center(`${AC}.querySelectorAll('.ac__option')[0]`));
    await sleep(120);
    const acTags = await ev(`[...${AC}.querySelectorAll('.tag')].map(t=>t.textContent).join('|')`);
    const acClosedAfterPick = await ev(`${AC}.querySelector('.ac__list').hidden`);
    await ev(`__ty(${AC}.querySelector('.ac__input'),'zzz')`);
    const acEmptyOpen = await ev(`!${AC}.querySelector('.ac__list').hidden`);
    await tab();
    await sleep(120);
    const acClosedAfterTab = await ev(`${AC}.querySelector('.ac__list').hidden`);

    const ok = closedAtStart && valueAfter === 'Jonas Silva' && valueAfter !== valueBefore && closedAfterPick
      && openAfterIcon && closedAfterTab && tabPresses <= 3 && acWasOpen && acTags === 'React'
      && acClosedAfterPick && acEmptyOpen && acClosedAfterTab;
    return { ok, detail: `combo "${valueBefore}" -> "${valueAfter}" (want Jonas Silva), pick closed=${closedAfterPick}, icon kept open=${openAfterIcon}, closed after ${tabPresses} real Tab(s); AC open=${acWasOpen}, real click selected ${acTags} + closed=${acClosedAfterPick}, empty list open=${acEmptyOpen}, real Tab closed=${acClosedAfterTab}` };
  });

  await check('C7', async () => {
    const r = await ev(`(() => {
      const orig = Element.prototype.scrollIntoView, calls = [];
      Element.prototype.scrollIntoView = function(o){ calls.push(String(this.className || this.tagName)); return orig.call(this,o); };
      const ac = document.querySelector('.ac[data-multiple="true"]'), ai = ac.querySelector('.ac__input');
      __ty(ai,''); ai.focus(); __kd(ai,'ArrowDown'); ai.blur();
      Element.prototype.scrollIntoView = orig;
      return JSON.stringify(calls);
    })()`);
    const calls = JSON.parse(r);
    return { ok: calls.some(c => c.includes('ac__option')), detail: `scrollIntoView calls: ${calls.join('|')} (want an ac__option)` };
  });

  await check('C8', async () => {
    const r = await ev(`(() => {
      const ts = document.getElementById('typedSheet');
      ts.querySelector('.sheet__control[data-r="0"][data-c="3"]').focus();
      ts._pasteData('Pro\\ttrue\\textra1\\textra2');
      return JSON.stringify([...ts.querySelectorAll('thead [data-col]')].map(t=>t.textContent));
    })()`);
    const cols = JSON.parse(r);
    return { ok: cols.length === 5, detail: `columns after wide paste: ${cols.join('|')} (want original 5)` };
  });

  await check('C9', async () => {
    const r = await ev(`(() => {
      const sh = document.getElementById('sheet');
      sh.querySelector('.sheet__control[data-r="0"][data-c="0"]').dispatchEvent(new MouseEvent('mousedown',{bubbles:true}));
      sh.querySelector('.sheet__control[data-r="2"][data-c="2"]').dispatchEvent(new MouseEvent('mousedown',{bubbles:true,ctrlKey:true}));
      document.dispatchEvent(new MouseEvent('mouseup'));
      return sh._copySelection();
    })()`);
    const lines = r.split('\n');
    return { ok: lines.length === 3 && lines[0] === 'Small\t\t' && lines[1] === '\t\t' && lines[2] === '\t\t30',
      detail: `non-rect copy: ${JSON.stringify(r)} (want only selected corners)` };
  });

  console.log(`--- console errors on load (${consoleErrors.length}) ---`);
  consoleErrors.forEach(e => console.log('  ' + e));
  console.log(`RESULT: ${22 - failures}/22 checks passed, ${failures} failed`);
  if(consoleErrors.length) failures++;
  process.exitCode = failures ? 1 : 0;
}

main().catch(e => { console.error('FATAL: ' + (e.stack || e.message)); process.exitCode = 1; })
  .finally(async () => {
    try { ws && ws.close(); } catch {}
    // Edge may relaunch itself under a new PID; match only our unique profile path, then kill by PID.
    const ps = `Get-CimInstance Win32_Process -Filter "Name='msedge.exe'" | Where-Object { $_.CommandLine -and $_.CommandLine.Contains('${profile}') } | ForEach-Object { Stop-Process -Id $_.ProcessId -Force -ErrorAction SilentlyContinue }`;
    try { spawnSync('powershell', ['-NoProfile', '-Command', ps], { stdio: 'ignore' }); } catch {}
    try { spawnSync('taskkill', ['/PID', String(child.pid), '/T', '/F'], { stdio: 'ignore' }); } catch {}
    try { child.kill(); } catch {}
    try { rmSync(profile, { recursive: true, force: true }); } catch {}
  });
