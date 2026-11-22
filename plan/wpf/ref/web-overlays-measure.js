(function(){
// Geometry dump for 07-overlays-pickers. Append to index.html and run Edge --dump-dom (README recipe, add --hide-scrollbars).
// Overlay numbers are viewport-absolute (vp) and relative to the dialog box (rel). Section numbers are relative to the .demo card.
const st=document.createElement('style');st.textContent='*,*::before,*::after{transition:none!important;animation:none!important}';document.head.appendChild(st);
const kd=(el,k)=>el.dispatchEvent(new KeyboardEvent('keydown',{key:k,bubbles:true,cancelable:true}));
const ty=(el,v)=>{el.value=v;el.dispatchEvent(new Event('input',{bubbles:true}));};
const q=s=>document.querySelector(s), qa=(s,r)=>[...(r||document).querySelectorAll(s)];
const out=[]; const R=v=>Math.round(v*10)/10;
function rect(el,base){const b=el.getBoundingClientRect();if(!base)return [R(b.x),R(b.y),R(b.width),R(b.height)].join(',');const o=base.getBoundingClientRect();return [R(b.x-o.x),R(b.y-o.y),R(b.width),R(b.height)].join(',');}
function d(label,el,base){ if(!el){out.push(' '+label+' MISSING');return;}
  const c=getComputedStyle(el);
  out.push(' '+label+(base?' rel ':' vp ')+rect(el,base)+' font='+c.fontWeight+' '+c.fontSize+'/'+c.lineHeight+' '+c.fontFamily.split(',')[0]+' color='+c.color+' bg='+c.backgroundColor+' bd='+c.borderTopWidth+' '+c.borderTopStyle+' '+c.borderTopColor+' rad='+c.borderTopLeftRadius+' pad='+c.padding+' shadow='+c.boxShadow+(el.children.length===0||el.tagName==='BUTTON'?' text="'+(el.textContent||el.value||el.placeholder||'').trim().slice(0,40)+'"':''));
}
function all(label,els,base){els.forEach((e,i)=>d(label+'['+i+']',e,base));}
function sec(id){const s=document.getElementById(id);const demo=s.querySelector('.demo');
  out.push('== '+id+' section '+rect(s)+' demo '+rect(demo));
  d('h2',s.querySelector('h2'),demo);d('p',s.querySelector('.section__head p'),demo);
  all('btn',qa('.demo .btn',s),demo);return demo;}
function vp(){out.push(' viewport '+innerWidth+'x'+innerHeight);}
try{
vp();
// ---------- modal
sec('modal');
q('#openModal').click(); let ov=q('#overlay'), m=ov.querySelector('.modal');
out.push('-- modal open (focus='+document.activeElement.id+')');
d('.overlay',ov);d('.modal',m);d('.modal__head',m.querySelector('.modal__head'),m);d('.modal__title',m.querySelector('.modal__title'),m);
d('.icon-btn',q('#closeModal'),m);d('icon svg',q('#closeModal svg'),m);d('.modal__body',m.querySelector('.modal__body'),m);
d('.modal__foot',m.querySelector('.modal__foot'),m);d('#cancelModal',q('#cancelModal'),m);d('#confirmModal',q('#confirmModal'),m);
out.push(' overlay backdrop-filter='+getComputedStyle(ov).backdropFilter);
q('#cancelModal').click();
// ---------- form modal
sec('form-modal');
q('#openFormModal').click(); ov=q('#formOverlay'); m=ov.querySelector('.modal');
out.push('-- form modal open (focus='+document.activeElement.id+')');
d('.modal--form',m);d('.modal__title',m.querySelector('.modal__title'),m);d('.icon-btn',q('#closeFormModal'),m);d('.modal__body',m.querySelector('.modal__body'),m);
d('.form-grid',m.querySelector('.form-grid'),m);all('.field__label',qa('.field__label',m),m);
all('.input',qa('.input',m),m);d('.select',q('#fm-role'),m);d('.textarea',q('#fm-desc'),m);
d('.modal__foot',m.querySelector('.modal__foot'),m);d('#cancelFormModal',q('#cancelFormModal'),m);d('submit',m.querySelector('button[type=submit]'),m);
out.push('-- form modal errors (empty submit)');
m.querySelector('button[type=submit]').click();
d('.modal--form(errors)',m);all('.field__error',qa('.field__error',m).filter(e=>e.offsetParent),m);all('error svg',qa('.field__error svg',m).filter(e=>e.offsetParent),m);
all('.input(error)',qa('.input',m),m);d('.textarea(errors)',q('#fm-desc'),m);d('submit(errors)',m.querySelector('button[type=submit]'),m);
q('#cancelFormModal').click();
// ---------- drawer invite
sec('drawer');
q('#openDrawer').click(); ov=q('#drawerOverlay'); m=ov.querySelector('.drawer');
out.push('-- drawer invite (focus='+document.activeElement.id+')');
d('.drawer',m);d('.drawer__head',m.querySelector('.drawer__head'),m);d('.modal__title',m.querySelector('.modal__title'),m);d('.icon-btn',q('#closeDrawer'),m);
d('.drawer__body',m.querySelector('.drawer__body'),m);d('.drawer__intro',m.querySelector('.drawer__intro'),m);
all('.field',qa('.field',m),m);all('.field__label',qa('.field__label',m),m);d('.input',q('#dr-email'),m);d('.field__hint',m.querySelector('.field__hint'),m);
d('.select',q('#dr-role'),m);d('.textarea',q('#dr-note'),m);d('.switch',m.querySelector('.switch'),m);d('.switch__track',m.querySelector('.switch__track'),m);
d('.drawer__foot',m.querySelector('.drawer__foot'),m);d('#cancelDrawer',q('#cancelDrawer'),m);d('submit',m.querySelector('button[type=submit]'),m);
out.push('-- drawer invite errors');
m.querySelector('button[type=submit]').click();
d('.field__error',qa('.field__error',m).find(e=>e.offsetParent),m);d('.field__hint(err)',m.querySelector('.field__hint'),m);d('.textarea(err)',q('#dr-note'),m);d('.switch(err)',m.querySelector('.switch'),m);
q('#cancelDrawer').click();
// ---------- drawer edit
q('#openEditDrawer').click(); ov=q('#editDrawerOverlay'); m=ov.querySelector('.drawer');
out.push('-- drawer edit (focus='+document.activeElement.id+')');
d('.drawer',m);d('.modal__title',m.querySelector('.modal__title'),m);d('.drawer__intro',m.querySelector('.drawer__intro'),m);
all('.field__label',qa('.field__label',m),m);all('.input',qa('.input',m),m);d('.select',q('#ed-role'),m);d('.switch',m.querySelector('.switch'),m);
d('.drawer__foot--update',m.querySelector('.drawer__foot'),m);d('#editDelete',q('#editDelete'),m);d('#cancelEditDrawer',q('#cancelEditDrawer'),m);d('save',m.querySelector('button[type=submit]'),m);
out.push('-- drawer edit confirm');
q('#editDelete').click();
d('.drawer__confirm',q('#editConfirm'),m);d('.drawer__confirm-card',m.querySelector('.drawer__confirm-card'),m);d('.confirm-icon',m.querySelector('.confirm-icon'),m);d('confirm svg',m.querySelector('.confirm-icon svg'),m);
d('h4',m.querySelector('.drawer__confirm h4'),m);d('confirm intro',m.querySelector('.drawer__confirm .drawer__intro'),m);d('#keepMember',q('#keepMember'),m);d('#confirmDelete',q('#confirmDelete'),m);
out.push(' focus='+document.activeElement.id);
q('#keepMember').click();q('#cancelEditDrawer').click();
// ---------- loading
sec('loading');
q('#openLoading').click(); ov=q('#loadingOverlay'); m=ov.querySelector('.modal');
out.push('-- loading open (focus='+document.activeElement.className+')');
d('.loading-modal',m);d('.loading-modal__spinner',m.querySelector('.loading-modal__spinner'),m);d('.loading-modal__title',m.querySelector('.loading-modal__title'),m);
d('.loading-modal__text',m.querySelector('.loading-modal__text'),m);d('.loading-bar',m.querySelector('.loading-bar'),m);
const ab=getComputedStyle(m.querySelector('.loading-bar'),'::after');out.push(' loading-bar::after width='+ab.width+' bg='+ab.backgroundColor);
out.push(' loading-modal outline when focused='+getComputedStyle(m).outlineStyle+' '+getComputedStyle(m).outlineWidth);
ov.classList.remove('open');document.body.style.overflow='';
// ---------- command palette
sec('command-palette');d('kbd in button',q('#openCmdk .cmdk__esc'),q('#command-palette .demo'));
q('#openCmdk').click(); ov=q('#cmdkOverlay'); m=ov.querySelector('.cmdk');
out.push('-- cmdk open (focus='+document.activeElement.id+')');
d('.cmdk',m);d('.cmdk__search',m.querySelector('.cmdk__search'),m);d('search svg',m.querySelector('.cmdk__search svg'),m);d('.cmdk__input',q('#cmdkInput'),m);d('.cmdk__esc',m.querySelector('.cmdk__search .cmdk__esc'),m);
let L=q('#cmdkList');d('.cmdk__list',L,m);out.push('  list scrollHeight='+L.scrollHeight+' clientHeight='+L.clientHeight);
all('.cmdk__group',qa('.cmdk__group',m),m);all('.cmdk__item',qa('.cmdk__item',m),m);all('item svg',qa('.cmdk__item svg',m).slice(0,2),m);all('.cmdk__item-label',qa('.cmdk__item-label',m).slice(0,2),m);all('.cmdk__shortcut',qa('.cmdk__shortcut',m),m);
d('.cmdk__foot',m.querySelector('.cmdk__foot'),m);all('foot span',qa('.cmdk__foot span',m),m);all('foot b',qa('.cmdk__foot b',m),m);
kd(q('#cmdkInput'),'ArrowUp');out.push('-- ArrowUp from 0 -> active '+qa('.cmdk__item',m).findIndex(e=>e.classList.contains('is-active'))+' scrollTop='+L.scrollTop);
ty(q('#cmdkInput'),'go');out.push('-- typed go');d('.cmdk(go)',m);all('.cmdk__item(go)',qa('.cmdk__item',m),m);all('.cmdk__group(go)',qa('.cmdk__group',m),m);
ty(q('#cmdkInput'),'zzz');out.push('-- typed zzz');d('.cmdk(empty)',m);d('.cmdk__empty',m.querySelector('.cmdk__empty'),m);
kd(document.body,'Escape');
// ---------- toasts
sec('toasts');
qa('[data-toast]').forEach(b=>b.click());
out.push('-- toasts: success, info, error');
d('.toast-stack',q('#toastStack'));let T=qa('.toast');all('.toast',T);
T.forEach((t,i)=>{d(' t'+i+' icon',t.querySelector('.toast__icon'),t);d(' t'+i+' icon svg',t.querySelector('.toast__icon svg'),t);d(' t'+i+' strong',t.querySelector('strong'),t);d(' t'+i+' span',t.querySelector('span'),t);});
T.forEach(t=>t.remove());
sec('toast-variants');
q('#toastDismissible').click();q('#toastWithAction').click();q('#toastPersistent').click();
out.push('-- toast variants: dismissible, with action, persistent');
T=qa('.toast');all('.toast',T);
T.forEach((t,i)=>{d(' t'+i+' body',t.querySelector('.toast__body'),t);d(' t'+i+' strong',t.querySelector('strong'),t);d(' t'+i+' span',t.querySelector('span'),t);d(' t'+i+' close',t.querySelector('.toast__close'),t);d(' t'+i+' close svg',t.querySelector('.toast__close svg'),t);d(' t'+i+' action',t.querySelector('.toast__action'),t);});
T.forEach(t=>t.remove());
// ---------- pickers
let demo=sec('date-picker');
all('.field__label',qa('.field__label',demo),demo);all('.picker__trigger',qa('.picker__trigger',demo),demo);all('trigger svg',qa('.picker__trigger svg',demo),demo);all('.picker__value',qa('.picker__value',demo),demo);
let P=qa('.picker',demo);
function openP(p,label){p.querySelector('.picker__trigger').click();const pn=p.querySelector('.picker__panel');out.push('-- '+label+' (focus='+(document.activeElement.className||document.activeElement.tagName)+')');
  d('.picker__trigger(open)',p.querySelector('.picker__trigger'),demo);d('.picker__panel',pn,demo);d('.picker__head',pn.querySelector('.picker__head'),pn);
  d('[data-prev]',pn.querySelector('[data-prev]'),pn);d('prev svg',pn.querySelector('[data-prev] svg'),pn);d('.picker__title',pn.querySelector('.picker__title'),pn);d('[data-next]',pn.querySelector('[data-next]'),pn);
  d('.picker__body',pn.querySelector('.picker__body'),pn);return pn;}
let pn=openP(P[0],'date open');
all('.picker__dow',qa('.picker__dow',pn),pn);const cells=qa('.picker__cell',pn);all('.picker__cell',cells.slice(0,8),pn);d('cell.today',pn.querySelector('.picker__cell.today'),pn);
out.push(' last cell '+rect(cells[cells.length-1],pn)+' count='+cells.length);
d('.picker__foot',pn.querySelector('.picker__foot'),pn);d('[data-clear]',pn.querySelector('[data-clear]'),pn);d('[data-today]',pn.querySelector('[data-today]'),pn);
cells[13].click(); out.push('-- picked day 14: value="'+P[0].querySelector('.picker__value').textContent+'"');d('.picker__value(set)',P[0].querySelector('.picker__value'),demo);
pn=openP(P[0],'date reopen with selection');d('cell.selected',pn.querySelector('.picker__cell.selected'),pn);
pn=openP(P[1],'month open');all('.picker__months cell',qa('.picker__cell',pn).slice(0,4),pn);d('.picker__months',pn.querySelector('.picker__months'),pn);d('.picker__foot',pn.querySelector('.picker__foot'),pn);
pn=openP(P[2],'year open');const yl=pn.querySelector('.picker__years');d('.picker__years',yl,pn);out.push('  years scrollHeight='+yl.scrollHeight+' clientHeight='+yl.clientHeight);all('year cell',qa('.picker__cell',pn).slice(0,5),pn);d('.picker__foot',pn.querySelector('.picker__foot'),pn);
document.body.click();
demo=sec('datetime-picker');
all('.field__label',qa('.field__label',demo),demo);all('.picker__trigger',qa('.picker__trigger',demo),demo);
P=qa('.picker',demo);
pn=openP(P[0],'datetime open');d('.picker__time',pn.querySelector('.picker__time'),pn);all('time label',qa('.picker__time label',pn),pn);all('time input',qa('.picker__time input',pn),pn);
d('.picker__foot',pn.querySelector('.picker__foot'),pn);d('[data-apply]',pn.querySelector('[data-apply]'),pn);
qa('.picker__cell',pn)[13].click();out.push(' after day click (time mode) panel hidden='+pn.hidden+' value="'+P[0].querySelector('.picker__value').textContent+'"');
pn.querySelector('[data-apply]').click();out.push(' after Apply value="'+P[0].querySelector('.picker__value').textContent+'"');d('.picker__value(set)',P[0].querySelector('.picker__value'),demo);
pn=openP(P[1],'datetime seconds open');all('time input',qa('.picker__time input',pn),pn);d('.picker__panel',pn,demo);
document.body.click();
// ---------- file input
demo=sec('file-input');
all('.field__label',qa('.field__label',demo),demo);all('.file-drop',qa('.file-drop',demo),demo);all('drop svg',qa('.file-drop > svg',demo),demo);all('drop strong',qa('.file-drop strong',demo),demo);all('drop span',qa('.file-drop span',demo),demo);
const addF=(inp,list)=>{const dt=new DataTransfer();list.forEach(f=>dt.items.add(new File([new Uint8Array(f[1])],f[0],{type:f[2]})));inp.files=dt.files;inp.dispatchEvent(new Event('change'));};
const fi=qa('input[type=file]',demo);
addF(fi[0],[['quarterly-report.pdf',2457600,'application/pdf']]);
addF(fi[1],[['cover.png',348160,'image/png'],['team-photo.jpg',1887437,'image/jpeg'],['logo.svg',812,'image/svg+xml']]);
out.push('-- with files');out.push(' demo '+rect(demo));
all('.file-list',qa('.file-list',demo),demo);all('.file-item',qa('.file-item',demo),demo);all('item svg',qa('.file-item > svg',demo).slice(0,1),demo);
all('.file-item__name',qa('.file-item__name',demo).slice(0,2),demo);all('.file-item__size',qa('.file-item__size',demo),demo);all('item .icon-btn',qa('.file-item .icon-btn',demo).slice(0,2),demo);all('btn svg',qa('.file-item .icon-btn svg',demo).slice(0,1),demo);
all('.file-count',qa('.file-count',demo),demo);
}catch(err){out.push('ERROR '+err.message+' '+err.stack);}
document.body.innerHTML='<pre id=o>'+out.join('\n').replace(/&/g,'&amp;').replace(/</g,'&lt;')+'</pre>';
})();
