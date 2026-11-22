// CDP regression checks for web/index.html. Node only, no dependencies.
// Usage: node plan/wpf/web-checks.mjs   (exit code 1 if any check fails)
import { spawn, spawnSync } from 'node:child_process';
import { mkdtempSync, rmSync, readFileSync } from 'node:fs';
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
const press = async (key, code, vk) => {
  for(const type of ['rawKeyDown', 'keyUp'])
    await send('Input.dispatchKeyEvent', { type, key, code, windowsVirtualKeyCode: vk, nativeVirtualKeyCode: vk });
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

  await check('W1', async () => {
    const r = await ev(`(() => {
      const ov = document.getElementById('cmdkOverlay'), card = ov.querySelector('.cmdk'), i = document.getElementById('cmdkInput');
      const cs = getComputedStyle(ov), R = v => Math.round(v*10)/10, top = () => R(card.getBoundingClientRect().top);
      i.value = ''; openCmdk();
      const out = { z:cs.zIndex, align:cs.alignItems, pt:cs.paddingTop, want:R(innerHeight*0.12), open:top() };
      i.value = 'go'; i.dispatchEvent(new Event('input',{bubbles:true})); out.go = top();
      i.value = 'zzz'; i.dispatchEvent(new Event('input',{bubbles:true})); out.empty = top();
      i.value = ''; i.dispatchEvent(new Event('input',{bubbles:true})); out.clear = top();
      closeCmdk(); return JSON.stringify(out);
    })()`);
    const o = JSON.parse(r);
    return { ok: o.z === '120' && o.align.includes('start') && Math.abs(parseFloat(o.pt) - o.want) <= 1
        && o.open === o.go && o.go === o.empty && o.empty === o.clear && Math.abs(o.open - o.want) <= 1,
      detail: `z=${o.z} align=${o.align} padding-top=${o.pt} (12vh=${o.want}); card top open=${o.open}, "go"=${o.go}, empty=${o.empty}, cleared=${o.clear}` };
  });

  await check('W2', async () => {
    const r = await ev(`(() => {
      const ov = document.getElementById('loadingOverlay'), card = ov.querySelector('.loading-modal');
      openOverlay(ov, card);
      const cs = getComputedStyle(card);
      const out = { focused: document.activeElement === card, outline: cs.outlineStyle + ' ' + cs.outlineWidth, tabIndex: card.tabIndex };
      closeOverlay(ov); return JSON.stringify(out);
    })()`);
    const o = JSON.parse(r);
    return { ok: o.focused && o.outline.startsWith('none') && o.tabIndex === -1,
      detail: `focused=${o.focused}, outline=${o.outline}, tabIndex=${o.tabIndex} (want focused, no outline, still focusable)` };
  });

  await check('W4', async () => {
    const o = JSON.parse(await ev(`JSON.stringify({
      time: [...document.querySelectorAll('#datetime-picker .picker__trigger')].slice(0,2).map(t => t.querySelector('use').getAttribute('href')),
      dateOnly: [...document.querySelectorAll('#datetime-picker .picker__trigger')].slice(2).map(t => t.querySelector('use').getAttribute('href')),
      dateSection: [...document.querySelectorAll('#date-picker .picker__trigger')].map(t => t.querySelector('use').getAttribute('href'))
    })`));
    return { ok: o.time.length === 2 && o.time.every(h => h === '#i-clock')
        && o.dateOnly.length === 3 && o.dateOnly.every(h => h === '#i-calendar')
        && o.dateSection.every(h => h === '#i-calendar'),
      detail: `datetime: time=${o.time.join(',')} date-only=${o.dateOnly.join(',')}; date-picker=${o.dateSection.join(',')}` };
  });

  await check('W5', async () => {
    await scroll('date-picker');
    const el = sel => `document.querySelector('#date-picker .picker ${sel}')`;
    await click(...await center(el('.picker__trigger'))); await sleep(100);
    const opened = await ev(`!${el('.picker__panel')}.hidden`);
    const titleBefore = await ev(`${el('.picker__title')}.textContent`);
    await click(...await center(el('.picker__panel [data-next]'))); await sleep(100);
    const titleAfter = await ev(`${el('.picker__title')}.textContent`);
    const openAfterInsideClick = await ev(`!${el('.picker__panel')}.hidden`);
    await ev(`(() => { const r=document.querySelector('#date-picker .picker'); const f=[...r.querySelectorAll('button, input, select, textarea')].filter(x => x.offsetParent !== null); f[f.length-1].focus(); })()`);
    await tab(); await sleep(120);
    const closedAfterTab = await ev(`${el('.picker__panel')}.hidden`);
    await click(...await center(el('.picker__trigger'))); await sleep(80);
    const valueBefore = await ev(`${el('.picker__value')}.textContent`);
    await click(...await center(el('.picker__panel [data-day="14"]'))); await sleep(120);
    const valueAfter = await ev(`${el('.picker__value')}.textContent`);
    const closedAfterPick = await ev(`${el('.picker__panel')}.hidden`);

    // reopened with a value: the selected cell has focus, a real Next click must not close the panel
    await click(...await center(el('.picker__trigger'))); await sleep(80);
    const reopenFocus = await ev(`document.activeElement.className`);
    await click(...await center(el('.picker__panel [data-next]'))); await sleep(120);
    const openAfterReopenNext = await ev(`!${el('.picker__panel')}.hidden`);

    // date+time picker: hour input focus and typing, day click with a focused cell
    await scroll('datetime-picker');
    const dt = sel => `document.querySelector('#datetime-picker .picker ${sel}')`;
    await click(...await center(dt('.picker__trigger'))); await sleep(100);
    await click(...await center(dt('[data-hour]'))); await sleep(100);
    const hourFocused = await ev(`document.activeElement === ${dt('[data-hour]')}`);
    await send('Input.insertText', { text: '7' }); await sleep(60);
    const hourValue = await ev(`${dt('[data-hour]')}.value`);
    await ev(`${dt('.picker__panel [data-day="10"]')}.focus()`);
    await click(...await center(dt('.picker__panel [data-day="12"]'))); await sleep(120);
    const dtOpenAfterDay = await ev(`!${dt('.picker__panel')}.hidden`);
    await ev(`${dt('.picker__panel [data-clear]')}.click()`);

    const ok = opened && titleAfter !== titleBefore && openAfterInsideClick && closedAfterTab
      && valueAfter !== valueBefore && valueAfter.includes('14') && closedAfterPick
      && reopenFocus.includes('picker__cell') && openAfterReopenNext
      && hourFocused && hourValue !== '09' && dtOpenAfterDay;
    return { ok, detail: `date-only: next kept open=${openAfterInsideClick}, Tab closed=${closedAfterTab}, day selected="${valueAfter}", reopen focus=${reopenFocus}, next after reopen kept open=${openAfterReopenNext}; datetime: hour focused=${hourFocused} value="${hourValue}", day click kept open=${dtOpenAfterDay}` };
  });

  await check('W7', async () => {
    const r = await ev(`(() => {
      const p = document.querySelector('#date-picker .picker'), t = p.querySelector('.picker__trigger'), d = new Date();
      if(p.querySelector('.picker__panel').hidden) t.click();
      const wantTitle = d.toLocaleString('en-US',{month:'long'}) + ' ' + d.getFullYear();
      for(let i = 0; i < 12 && p.querySelector('.picker__title').textContent !== wantTitle; i++) p.querySelector('[data-prev]').click();
      p.querySelector('[data-clear]').click(); t.click();
      const panel = p.querySelector('.picker__panel');
      const c14 = panel.querySelector('[data-day="14"]'), todayCell = panel.querySelector('.picker__cell.today');
      const month = new Date(d.getFullYear(), d.getMonth(), 14).toLocaleString('en-US', {month:'long'});
      const out = { role:panel.getAttribute('role'), label:panel.getAttribute('aria-label'),
        controls: t.getAttribute('aria-controls') === panel.id, hasId: !!panel.id,
        dayLabel: c14.getAttribute('aria-label'), wantLabel: month + ' 14, ' + d.getFullYear(),
        todayCurrent: todayCell && todayCell.getAttribute('aria-current'), pressedBefore: c14.getAttribute('aria-pressed') };
      c14.click(); t.click();
      out.pressedAfter = p.querySelector('[data-day="14"]').getAttribute('aria-pressed');
      t.click(); return JSON.stringify(out);
    })()`);
    const o = JSON.parse(r);
    const extra = JSON.parse(await ev(`(() => {
      const ps = document.querySelectorAll('#date-picker .picker'), m = ps[1], y = ps[2];
      [m,y].forEach(p => { if(p.querySelector('.picker__panel').hidden) p.querySelector('.picker__trigger').click(); });
      const yCell = y.querySelector('.picker__cell'), yr = yCell.dataset.year;
      const out = { monthLabel: m.querySelector('[data-month="4"]').getAttribute('aria-label') };
      m.querySelector('[data-month="4"]').click(); yCell.click();
      out.monthPressed = m.querySelector('[data-month="4"]').getAttribute('aria-pressed');
      out.yearPressed = y.querySelector('[data-year="' + yr + '"]').getAttribute('aria-pressed');
      return JSON.stringify(out);
    })()`));
    return { ok: o.role === 'dialog' && !!o.label && o.controls && o.hasId && o.dayLabel === o.wantLabel
        && o.todayCurrent === 'date' && o.pressedBefore === null && o.pressedAfter === 'true'
        && extra.monthLabel === 'May' && extra.monthPressed === 'true' && extra.yearPressed === 'true',
      detail: `panel role=${o.role} label="${o.label}" controls=${o.controls}; day14 label="${o.dayLabel}" (want "${o.wantLabel}"), today=${o.todayCurrent}, pressed ${o.pressedBefore} -> ${o.pressedAfter}; month label="${extra.monthLabel}" pressed=${extra.monthPressed}, year pressed=${extra.yearPressed}` };
  });

  await check('W8', async () => {
    const r = await ev(`(() => {
      openCmdk();
      const i = document.getElementById('cmdkInput'), list = document.getElementById('cmdkList'), item = list.querySelector('.cmdk__item');
      const out = { role:i.getAttribute('role'), controls:i.getAttribute('aria-controls'), expanded:i.getAttribute('aria-expanded'),
        aad:i.getAttribute('aria-activedescendant'), itemId:item && item.id, listRole:list.getAttribute('role'),
        itemRole:item && item.getAttribute('role'), selected:item && item.getAttribute('aria-selected') };
      out.groupRole = list.querySelector('.cmdk__group') && list.querySelector('.cmdk__group').getAttribute('role');
      __kd(i,'ArrowDown');
      out.aadAfter = i.getAttribute('aria-activedescendant');
      out.selectedCount = list.querySelectorAll('[aria-selected="true"]').length;
      i.value='zzz'; i.dispatchEvent(new Event('input',{bubbles:true}));
      out.aadEmpty = i.getAttribute('aria-activedescendant');
      out.emptyRole = list.querySelector('.cmdk__empty') && list.querySelector('.cmdk__empty').getAttribute('role');
      closeCmdk(); out.expandedClosed = i.getAttribute('aria-expanded'); out.aadClosed = i.getAttribute('aria-activedescendant');
      openCmdk(); const item2 = list.querySelector('.cmdk__item'); item2.focus(); __kd(item2,'Escape');
      out.escClosed = { expanded:i.getAttribute('aria-expanded'), aad:i.getAttribute('aria-activedescendant') };
      openCmdk(); document.getElementById('cmdkOverlay').click();
      out.backdropClosed = { expanded:i.getAttribute('aria-expanded'), aad:i.getAttribute('aria-activedescendant') };
      closeCmdk();
      return JSON.stringify(out);
    })()`);
    const o = JSON.parse(r);
    return { ok: o.role === 'combobox' && o.controls === 'cmdkList' && o.expanded === 'true' && o.aad === o.itemId
        && o.listRole === 'listbox' && o.itemRole === 'option' && o.selected === 'true'
        && o.aadAfter && o.aadAfter !== o.itemId && o.selectedCount === 1 && !o.aadEmpty
        && o.expandedClosed === 'false' && !o.aadClosed
        && o.groupRole === 'presentation' && o.emptyRole === 'presentation'
        && o.escClosed.expanded === 'false' && !o.escClosed.aad
        && o.backdropClosed.expanded === 'false' && !o.backdropClosed.aad,
      detail: `input role=${o.role} controls=${o.controls} expanded=${o.expanded}; list=${o.listRole}, item=${o.itemRole}#${o.itemId} selected=${o.selected}; aad ${o.aad} -> ${o.aadAfter} -> empty ${o.aadEmpty}; groups/empty roles=${o.groupRole}/${o.emptyRole}; closeCmdk/Escape/backdrop expanded=${o.expandedClosed}/${o.escClosed.expanded}/${o.backdropClosed.expanded}` };
  });

  await check('W9', async () => {
    const r = await ev(`(() => {
      const cancel = { openFormModal:'cancelFormModal', openDrawer:'cancelDrawer', openEditDrawer:'cancelEditDrawer' };
      return JSON.stringify([['openFormModal','projectForm','fm-name'],['openDrawer','inviteForm','dr-email'],['openEditDrawer','editForm','ed-email']].map(([openBtn, formId, id]) => {
        document.getElementById(openBtn).click();
        const form = document.getElementById(formId), field = document.getElementById(id);
        field.value = ''; form.querySelector('button[type=submit]').click();
        const error = field.closest('.field').querySelector('.field__error');
        const out = { id, focused: document.activeElement === field, invalid: field.getAttribute('aria-invalid'),
          described: field.getAttribute('aria-describedby') === error.id, alert: error.getAttribute('role') };
        document.getElementById(cancel[openBtn]).click(); return out;
      }));
    })()`);
    const o = JSON.parse(r);
    return { ok: o.every(x => x.focused && x.invalid === 'true' && x.described && x.alert === 'alert'), detail: JSON.stringify(o) };
  });

  await check('W10', async () => {
    const role = JSON.parse(await ev(`(() => { const el = showToast('error','Boom','Failed.',{persistent:true,dismissible:true}); const r = el.getAttribute('role'); el.remove(); return JSON.stringify({r}); })()`)).r;
    await ev(`(() => { window.__pauseToast = showToast('info','Pause test','Hovering.',{dismissible:true}); window.__pauseToast.dispatchEvent(new MouseEvent('mouseenter')); })()`);
    await sleep(3800);
    const survivedHover = await ev(`window.__pauseToast.isConnected`);
    await ev(`window.__pauseToast.dispatchEvent(new MouseEvent('mouseleave'))`);
    const hoverAtLeave = await ev(`window.__pauseToast.matches(':hover')`);
    let goneAfterLeave = false;
    for(let i = 0; i < 14 && !goneAfterLeave; i++){ await sleep(400); goneAfterLeave = await ev(`!window.__pauseToast.isConnected`); }
    await ev(`document.getElementById('toastWithAction').click()`);
    await sleep(3800);
    const actionSurvived = await ev(`!!document.querySelector('.toast__action')`);
    const focusRestored = JSON.parse(await ev(`(async () => {
      document.getElementById('toastDismissible').focus();
      const el = showToast('info','Focus test','Dismiss me.',{dismissible:true});
      const btn = el.querySelector('.toast__close'); btn.focus(); btn.click();
      await new Promise(r => setTimeout(r, 350));
      return JSON.stringify({ restored: document.activeElement.id === 'toastDismissible' });
    })()`)).restored;
    const stray = JSON.parse(await ev(`(async () => {
      const orig = window.setTimeout; let count = 0;
      window.setTimeout = function(fn, ms){ if(ms === 3600) count++; return orig.apply(window, arguments); };
      document.getElementById('toastDismissible').focus();
      const el = showToast('info','Stray','x',{dismissible:true});
      const btn = el.querySelector('.toast__close'); btn.focus();
      const before = count; btn.click(); const after = count;
      window.setTimeout = orig;
      await new Promise(r => orig(r, 300));
      return JSON.stringify({ before, after });
    })()`));
    return { ok: role === 'alert' && survivedHover && goneAfterLeave && actionSurvived && focusRestored
        && stray.after === stray.before,
      detail: `error role=${role}, hover survived=${survivedHover}, left gone=${goneAfterLeave}, action toast kept=${actionSurvived}, focus restored=${focusRestored}, stray timers ${stray.before} -> ${stray.after}` };
  });

  await check('W11', async () => {
    const r = await ev(`(() => {
      const boxes = document.querySelectorAll('#file-input .file-input'), single = boxes[0], multi = boxes[1];
      const mk = files => { const dt = new DataTransfer(); files.forEach(f => dt.items.add(new File([new Uint8Array(f[1])],f[0],{type:f[2]}))); return dt; };
      const change = (inp, files) => { inp.files = mk(files).files; inp.dispatchEvent(new Event('change')); };
      const drop = (box, files) => box.querySelector('.file-drop').dispatchEvent(new DragEvent('drop',{bubbles:true,cancelable:true,dataTransfer:mk(files)}));
      const names = box => [...box.querySelectorAll('.file-item__name')].map(n=>n.textContent);
      document.querySelectorAll('#file-input .file-item button').forEach(b => b.click());
      change(single.querySelector('input'), [['report.pdf',100,'application/pdf']]);
      drop(single, []);
      const out = { singleAfterEmptyDrop: names(single).join('|'), removeLabel: single.querySelector('.file-item button').getAttribute('aria-label') };
      change(multi.querySelector('input'), [['notes.txt',10,'text/plain']]);
      out.browseRejected = names(multi).length === 0;
      drop(multi, [['a.png',512,'image/png'],['a.png',512,'image/png'],['b.jpg',256,'image/jpeg']]);
      out.deduped = names(multi).join('|'); return JSON.stringify(out);
    })()`);
    const o = JSON.parse(r);
    return { ok: o.singleAfterEmptyDrop === 'report.pdf' && o.removeLabel === 'Remove report.pdf'
        && o.browseRejected && o.deduped === 'a.png|b.jpg',
      detail: `single after empty drop=${o.singleAfterEmptyDrop}, remove label="${o.removeLabel}", browse text rejected=${o.browseRejected}, multiple de-duped=${o.deduped}` };
  });

  await check('W14', async () => {
    const o = JSON.parse(await ev(`(() => {
      const t = [...document.querySelectorAll('#tooltip-demo .tip')].find(x => x.classList.contains('tip--right'));
      const btn = t.querySelector('button'); btn.focus();
      const out = { z: getComputedStyle(t).zIndex, pointer: getComputedStyle(t, '::after').pointerEvents };
      btn.blur(); return JSON.stringify(out);
    })()`));
    return { ok: o.z === '30' && o.pointer === 'auto',
      detail: `Right tip on focus: z-index=${o.z}, bubble pointer-events=${o.pointer} (want 30/auto)` };
  });

  await check('W15', async () => {
    const o = JSON.parse(await ev(`(() => {
      const t = [...document.querySelectorAll('#tooltip-demo .tip')].find(x => x.classList.contains('tip--right'));
      const btn = t.querySelector('button'), id = btn.getAttribute('aria-describedby'), desc = id && document.getElementById(id);
      btn.focus();
      const shown = t.matches(':focus-within') && !t.classList.contains('is-dismissed');
      btn.dispatchEvent(new KeyboardEvent('keydown',{key:'Escape',bubbles:true,cancelable:true}));
      const escaped = t.classList.contains('is-dismissed');
      btn.blur(); btn.focus();
      const reshown = !t.classList.contains('is-dismissed');
      btn.blur();
      return JSON.stringify({ wrapperTab: t.getAttribute('tabindex'), descText: desc && desc.textContent, shown, escaped, reshown });
    })()`));
    return { ok: o.wrapperTab === null && o.descText === 'Right tooltip' && o.shown && o.escaped && o.reshown,
      detail: `wrapper tabindex=${o.wrapperTab}, describedby text="${o.descText}", shown=${o.shown}, Escape dismissed=${o.escaped}, refocus restored=${o.reshown}` };
  });

  await check('W16', async () => {
    const o = JSON.parse(await ev(`(() => {
      const list = document.querySelector('[role=tablist]');
      const panels = [...list.querySelectorAll('.tab')].map(t => document.getElementById(t.getAttribute('aria-controls')));
      const demo = document.getElementById('tabs').querySelector('.demo');
      const list2 = document.createElement('div'); list2.setAttribute('role','tablist'); demo.appendChild(list2);
      const b = document.createElement('button'); b.setAttribute('role','tab'); b.setAttribute('aria-controls','w16-panel'); list2.appendChild(b);
      const p2 = document.createElement('div'); p2.className = 'tabpanel'; p2.id = 'w16-panel'; demo.appendChild(p2);
      list.querySelectorAll('.tab')[1].click();
      const out = { ownHidden: panels.map(p => p.hidden), otherHidden: p2.hidden, panelTabIndex: panels[0].tabIndex };
      b.remove(); list2.remove(); p2.remove(); list.querySelectorAll('.tab')[0].click();
      return JSON.stringify(out);
    })()`));
    return { ok: o.ownHidden.join(',') === 'true,false,true' && o.otherHidden === false && o.panelTabIndex === 0,
      detail: `own panels hidden=${o.ownHidden.join(',')}, unrelated panel hidden=${o.otherHidden}, panel tabindex=${o.panelTabIndex}` };
  });

  await check('W17', async () => {
    await scroll('stepper');
    for(let i = 0; i < 3; i++){ await click(...await center(`document.getElementById('stepNext')`)); await sleep(90); }
    const step = JSON.parse(await ev(`JSON.stringify({ active:document.activeElement.id, nextDisabled:document.getElementById('stepNext').disabled, prevDisabled:document.getElementById('stepPrev').disabled })`));
    const load = JSON.parse(await ev(`(() => { const b = document.getElementById('loadingBtn'); b.focus(); b.click();
      return JSON.stringify({ active:document.activeElement.id, ariaDisabled:b.getAttribute('aria-disabled'), disabled:b.disabled }); })()`));
    await sleep(1700);
    return { ok: step.active === 'stepPrev' && step.nextDisabled && !step.prevDisabled
        && load.active === 'loadingBtn' && load.ariaDisabled === 'true' && !load.disabled,
      detail: `stepper at end: focus=${step.active}, next disabled=${step.nextDisabled}; loading: focus=${load.active}, aria-disabled=${load.ariaDisabled}, disabled=${load.disabled}` };
  });

  await check('W18', async () => {
    const o = JSON.parse(await ev(`(() => {
      const first = document.querySelector('#stepperDemo .step'), status = document.getElementById('stepStatus'), menu = document.getElementById('menuResult');
      for(let i = 0; i < 3; i++) document.getElementById('stepPrev').click();
      const out = { statusRole: status.getAttribute('role'), menuRole: menu.getAttribute('role'), current: first.getAttribute('aria-current') };
      document.getElementById('stepNext').click();
      const done = document.querySelector('#stepperDemo .step');
      out.doneText = [...done.querySelectorAll('.sr-only')].map(s => s.textContent).join('|');
      document.getElementById('stepPrev').click();
      return JSON.stringify(out);
    })()`));
    return { ok: o.statusRole === 'status' && o.menuRole === 'status' && o.current === 'step' && o.doneText.includes('Completed'),
      detail: `stepStatus role=${o.statusRole}, menuResult role=${o.menuRole}, current step aria-current=${o.current}, done text="${o.doneText}"` };
  });

  await check('W14b', async () => {
    await scroll('tooltip-demo');
    const T = `[...document.querySelectorAll('#tooltip-demo .tip')]`;
    await ev(`document.activeElement.blur()`);
    await click(...await center(`${T}[0].querySelector('button')`)); await sleep(80);
    await press('Escape', 'Escape', 27); await sleep(80);
    const dismissed = await ev(`${T}[0].classList.contains('is-dismissed')`);
    await press('Tab', 'Tab', 9); await sleep(80);
    const focus = await ev(`document.activeElement.textContent.trim()`);
    const [x, y] = await center(`${T}[0].querySelector('button')`);
    await send('Input.dispatchMouseEvent', { type: 'mouseMoved', x, y }); await sleep(80);
    const shown = await ev(`(() => { const t=${T}[0]; return !t.classList.contains('is-dismissed') && t.matches(':hover'); })()`);
    return { ok: dismissed && focus === 'Bottom' && shown,
      detail: `Escape dismissed=${dismissed}, Tab focus=${focus}, hover after Tab shows tip=${shown}` };
  });

  await check('W14c', async () => {
    await scroll('tooltip-demo');
    const T = `[...document.querySelectorAll('#tooltip-demo .tip')]`;
    await ev(`document.activeElement.blur()`);
    await click(...await center(`${T}[3].querySelector('button')`)); await sleep(80);
    await press('Escape', 'Escape', 27); await sleep(80);
    const o = JSON.parse(await ev(`(() => { const t=${T}[3], s=document.querySelector('#tooltip-demo .btn--primary'), b=s.getBoundingClientRect();
      const el=document.elementFromPoint(b.left+15,b.top+b.height/2);
      return JSON.stringify({ dismissed:t.classList.contains('is-dismissed'), hitSave:el===s, hit:el.className }); })()`));
    return { ok: o.dismissed && o.hitSave,
      detail: `dismissed=${o.dismissed}, elementFromPoint at Save+15px=${o.hit} (want the Save button)` };
  });

  await check('W14d', async () => {
    await scroll('tooltip-demo');
    const T = `[...document.querySelectorAll('#tooltip-demo .tip')]`;
    await ev(`document.activeElement.blur()`);
    const g = JSON.parse(await ev(`(() => { const t=${T}[3], r=t.getBoundingClientRect(), cs=getComputedStyle(t,'::after');
      return JSON.stringify({ right:r.right, midY:r.top+r.height/2, bx:r.right+9, bw:parseFloat(cs.width) }); })()`));
    const [sx, sy] = await center(`${T}[3].querySelector('button')`);
    await send('Input.dispatchMouseEvent', { type: 'mouseMoved', x: sx, y: sy }); await sleep(80);
    for(let x = Math.round(g.right) - 1; x <= Math.round(g.bx + 10); x++)
      await send('Input.dispatchMouseEvent', { type: 'mouseMoved', x, y: g.midY });
    await sleep(120);
    const o = JSON.parse(await ev(`(() => { const t=${T}[3], x=${Math.round(g.bx + 10)}, y=${g.midY}, el=document.elementFromPoint(x,y);
      return JSON.stringify({ hover:t.matches(':hover'), hitTip:el===t, hit:el.className }); })()`));
    return { ok: o.hover && o.hitTip,
      detail: `1px mouse path across the 9px gap: tip hovered=${o.hover}, bubble hit target=${o.hitTip} (${o.hit})` };
  });

  await check('W15b', async () => {
    const o = JSON.parse(await ev(`JSON.stringify([...document.querySelectorAll('#tooltip-demo .tip')].map(t => {
      const b=t.querySelector('button'), d=document.getElementById(b.getAttribute('aria-describedby'));
      return { hidden:d.hasAttribute('hidden'), text:d.textContent, want:t.dataset.tip }; }))`));
    return { ok: o.length === 5 && o.every(x => x.hidden && x.text === x.want),
      detail: `descriptions hidden=${o.map(x => x.hidden).join(',')}; aria-describedby still resolves text=${o.every(x => x.text === x.want)}` };
  });

  await check('W18b', async () => {
    const cur = await ev(`document.querySelector('.step--vertical .step.is-current').getAttribute('aria-current')`);
    return { ok: cur === 'step', detail: `vertical current step aria-current=${cur}` };
  });

  await check('NAVMEASURE', async () => {
    const src = readFileSync(new URL('./ref/web-nav-measure.js', import.meta.url), 'utf8');
    const txt = readFileSync(new URL('./ref/web-nav-measurements.txt', import.meta.url), 'utf8');
    const ok = src.includes("getAttribute('aria-disabled')") && /disabled=false aria-disabled=true/.test(txt);
    return { ok, detail: `measure script logs aria-disabled=${src.includes("getAttribute('aria-disabled')")}, measurements line updated=${/disabled=false aria-disabled=true/.test(txt)}` };
  });

  await check('NAVCAP', async () => {
    const src = readFileSync(new URL('./ref/web-nav-capture.ps1', import.meta.url), 'utf8');
    const ok = /function TipOn[\s\S]{0,300}z-index:30/.test(src);
    return { ok, detail: `TipOn forces z-index:30=${ok}` };
  });

  console.log(`--- console errors on load (${consoleErrors.length}) ---`);
  consoleErrors.forEach(e => console.log('  ' + e));
  console.log(`RESULT: ${43 - failures}/43 checks passed, ${failures} failed`);
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
