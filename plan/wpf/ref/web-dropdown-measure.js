(function(){
const st=document.createElement('style');st.textContent='*,*::before,*::after{transition:none!important;animation:none!important}';document.head.appendChild(st);
const kd=(el,k)=>el.dispatchEvent(new KeyboardEvent('keydown',{key:k,bubbles:true,cancelable:true}));
const ty=(el,v)=>{el.value=v;el.dispatchEvent(new Event('input',{bubbles:true}));};
const out=[];
const R=v=>Math.round(v*10)/10;
function rel(el,base){const b=el.getBoundingClientRect(),o=base.getBoundingClientRect();return [R(b.x-o.x),R(b.y-o.y),R(b.width),R(b.height)].join(',');}
function d(label,el,base){ if(!el){out.push(' '+label+' MISSING');return;}
  const c=getComputedStyle(el);
  out.push(' '+label+' rel '+rel(el,base)+' font='+c.fontWeight+' '+c.fontSize+'/'+c.lineHeight+' color='+c.color+' bg='+c.backgroundColor+' bd='+c.borderTopWidth+' '+c.borderTopColor+' rad='+c.borderTopLeftRadius+' pad='+c.padding+' shadow='+c.boxShadow+(el.children.length===0||el.tagName==='BUTTON'?' text="'+(el.textContent||el.value||el.placeholder||'').trim().slice(0,30)+'"':''));
}
function all(label,els,base){[...els].forEach((e,i)=>d(label+'['+i+']',e,base));}
function sec(id){const s=document.getElementById(id);const demo=s.querySelector('.demo');const b=s.getBoundingClientRect(),db=demo.getBoundingClientRect();
  out.push('== '+id+' section '+[b.x,b.y,b.width,b.height].map(R).join(',')+' demo '+[db.x,db.y,db.width,db.height].map(R).join(','));return demo;}

try{
// ---------- combobox
let demo=sec('combobox');
let combos=demo.querySelectorAll('.combo');
out.push('-- idle');
all('.field__label',demo.querySelectorAll('.field__label'),demo);
all('.combo__trigger',demo.querySelectorAll('.combo__trigger'),demo);
all('.combo__value',demo.querySelectorAll('.combo__value'),demo);
all('trigger svg',demo.querySelectorAll('.combo__trigger svg'),demo);
all('.field__hint',demo.querySelectorAll('.field__hint'),demo);
out.push('-- open: Assignee, Jonas selected, Mia active (ArrowDown on trigger, 2x ArrowDown in search)');
let c=combos[0];c.querySelectorAll('.combo__option')[1].click();kd(c.querySelector('.combo__trigger'),'ArrowDown');
let s=c.querySelector('.combo__input');kd(s,'ArrowDown');kd(s,'ArrowDown');
d('.combo__trigger(open)',c.querySelector('.combo__trigger'),demo);
d('.combo__value(selected)',c.querySelector('.combo__value'),demo);
d('.combo__panel',c.querySelector('.combo__panel'),demo);
d('.combo__search',c.querySelector('.combo__search'),demo);
d('search svg',c.querySelector('.combo__search svg'),demo);
d('.combo__input(focused)',s,demo);
let L=c.querySelector('.combo__list');d('.combo__list',L,demo);out.push('  list scrollHeight='+L.scrollHeight+' clientHeight='+L.clientHeight+' scrollTop='+L.scrollTop+' activedescendant='+s.getAttribute('aria-activedescendant'));
all('.combo__option',c.querySelectorAll('.combo__option'),demo);
all('.combo__check',c.querySelectorAll('.combo__check'),demo);
all('.combo__option-label',[...c.querySelectorAll('.combo__option-label')].slice(0,3),demo);
all('.combo__option-meta',[...c.querySelectorAll('.combo__option-meta')].slice(0,3),demo);
out.push('-- Project open, typed xyz (empty)');
let c2=combos[1];c2.querySelector('.combo__trigger').click();ty(c2.querySelector('.combo__input'),'xyz');
d('.combo__panel(empty)',c2.querySelector('.combo__panel'),demo);d('.combo__list(empty)',c2.querySelector('.combo__list'),demo);d('.combo__empty',c2.querySelector('.combo__empty'),demo);
// close both
document.body.click();

// ---------- multi
demo=sec('multi-select');
let ms=demo.querySelectorAll('.multi');
out.push('-- idle');
all('.multi__trigger',demo.querySelectorAll('.multi__trigger'),demo);
all('.multi__placeholder',demo.querySelectorAll('.multi__placeholder'),demo);
all('trigger svg',demo.querySelectorAll('.multi__trigger > svg'),demo);
out.push('-- Skills: React, TypeScript, Accessibility checked + open');
let m=ms[0];let o=m.querySelectorAll('.multi__option input');[0,1,3].forEach(i=>{o[i].checked=true;o[i].dispatchEvent(new Event('change'));});
m.querySelector('.multi__trigger').click();
d('.multi__trigger(open,3 chips)',m.querySelector('.multi__trigger'),demo);
d('.multi__chips',m.querySelector('.multi__chips'),demo);
all('.multi__chip',m.querySelectorAll('.multi__chip'),demo);
all('chip button',m.querySelectorAll('.multi__chip button'),demo);
all('chip svg',m.querySelectorAll('.multi__chip svg'),demo);
d('trigger svg(open)',m.querySelector('.multi__trigger > svg'),demo);
d('.multi__panel',m.querySelector('.multi__panel'),demo);
d('.multi__search',m.querySelector('.multi__search'),demo);
d('.multi__input',m.querySelector('.multi__input'),demo);
L=m.querySelector('.multi__list');d('.multi__list',L,demo);out.push('  list scrollHeight='+L.scrollHeight+' clientHeight='+L.clientHeight);
all('.multi__option',m.querySelectorAll('.multi__option'),demo);
all('.check__box',[...m.querySelectorAll('.check__box')].slice(0,3),demo);
all('.multi__option-label',[...m.querySelectorAll('.multi__option-label')].slice(0,2),demo);
all('.multi__option-meta',[...m.querySelectorAll('.multi__option-meta')].slice(0,2),demo);
d('.multi__foot',m.querySelector('.multi__foot'),demo);
d('select-all',m.querySelector('.multi__select-all'),demo);d('clear',m.querySelector('.multi__clear'),demo);
d('.multi__count',m.querySelector('.multi__count'),demo);d('done',m.querySelector('.multi__done'),demo);
document.body.click();
out.push('-- Skills select all (6 chips, closed)');
m.querySelector('.multi__select-all').click();
d('.multi__trigger(6 chips)',m.querySelector('.multi__trigger'),demo);
all('.multi__chip',m.querySelectorAll('.multi__chip'),demo);
out.push('-- Tags open, typed xyz');
let m2=ms[1];m2.querySelector('.multi__trigger').click();ty(m2.querySelector('.multi__input'),'xyz');
d('.multi__panel(empty)',m2.querySelector('.multi__panel'),demo);d('.multi__list(empty)',m2.querySelector('.multi__list'),demo);d('.multi__empty',m2.querySelector('.multi__empty'),demo);
document.body.click();

// ---------- tag input
demo=sec('tag-input');
out.push('-- idle');
all('.field__label',demo.querySelectorAll('.field__label'),demo);
all('.tag-input',demo.querySelectorAll('.tag-input'),demo);
all('.tag',demo.querySelectorAll('.tag'),demo);
all('tag button',demo.querySelectorAll('.tag button'),demo);
all('tag svg',demo.querySelectorAll('.tag svg'),demo);
all('.tag-input__field',demo.querySelectorAll('.tag-input__field'),demo);
all('.tag-input__hint',demo.querySelectorAll('.tag-input__hint'),demo);
out.push('-- Skills filled to max with vue,node,go then try "x" (rejected), 2 rows via long tags on Tags');
let w=demo.querySelectorAll('.tag-input-wrap');let f2=w[1].querySelector('input');['vue','node','go','x'].forEach(v=>{f2.value=v;kd(f2,'Enter');});
d('.tag-input(max)',w[1].querySelector('.tag-input'),demo);d('.tag-input__field(max)',f2,demo);d('hint(max)',w[1].querySelector('.tag-input__hint'),demo);
let f1=w[0].querySelector('input');['user interviews','accessibility audit'].forEach(v=>{f1.value=v;kd(f1,'Enter');});
d('.tag-input(wrap)',w[0].querySelector('.tag-input'),demo);all('.tag(wrap)',w[0].querySelectorAll('.tag'),demo);d('.tag-input__field(wrap)',f1,demo);

// ---------- autocomplete
demo=sec('autocomplete');
let acs=demo.querySelectorAll('.ac');
out.push('-- idle');
all('.field__label',demo.querySelectorAll('.field__label'),demo);
d('ac0 .tag-input',acs[0].querySelector('.tag-input'),demo);d('ac0 field',acs[0].querySelector('.ac__input'),demo);
d('.ac__hint',demo.querySelector('.ac__hint'),demo);
d('ac1 .input',acs[1].querySelector('.input'),demo);d('ac1 icon',acs[1].querySelector('.input-icon svg'),demo);d('ac1 .field__hint',demo.querySelector('.field__hint'),demo);
out.push('-- ac0: React, Vue chosen, typed o, 2x ArrowDown');
let i0=acs[0].querySelector('.ac__input');i0.focus();ty(i0,'react');kd(i0,'ArrowDown');kd(i0,'Enter');ty(i0,'vue');kd(i0,'ArrowDown');kd(i0,'Enter');ty(i0,'o');kd(i0,'ArrowDown');kd(i0,'ArrowDown');
d('ac0 .tag-input(focus)',acs[0].querySelector('.tag-input'),demo);all('ac0 .tag',acs[0].querySelectorAll('.tag'),demo);d('ac0 field',i0,demo);
d('.ac__list',acs[0].querySelector('.ac__list'),demo);all('.ac__option',acs[0].querySelectorAll('.ac__option'),demo);
d('.ac__hint(2)',demo.querySelector('.ac__hint'),demo);
out.push('-- ac1: focus (all 7), ArrowDown');
let i1=acs[1].querySelector('.ac__input');i1.focus();kd(i1,'ArrowDown');
L=acs[1].querySelector('.ac__list');d('.ac__list(7)',L,demo);out.push('  list scrollHeight='+L.scrollHeight+' clientHeight='+L.clientHeight);all('.ac__option',acs[1].querySelectorAll('.ac__option'),demo);
ty(i1,'zzz');d('.ac__list(empty)',L,demo);d('.ac__empty',L.querySelector('.ac__empty'),demo);
}catch(err){out.push('ERROR '+err.message+' '+err.stack);}
document.body.innerHTML='<pre id=o>'+out.join('\n').replace(/&/g,'&amp;').replace(/</g,'&lt;')+'</pre>';
})();
