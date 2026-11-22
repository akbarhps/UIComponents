(function(){
// Geometry dump for 06-navigation. Append to index.html and run Edge --dump-dom --hide-scrollbars (see web-nav-capture.ps1 header).
// 'rel' numbers are relative to the section's .demo card (or the named base); 'vp' = viewport-absolute (dump viewport only).
const st=document.createElement('style');st.textContent='*,*::before,*::after{transition:none!important;animation:none!important}html{scroll-behavior:auto!important}';document.head.appendChild(st);
const kd=(el,k)=>el.dispatchEvent(new KeyboardEvent('keydown',{key:k,bubbles:true,cancelable:true}));
const q=s=>document.querySelector(s), qa=(s,r)=>[...(r||document).querySelectorAll(s)];
const out=[]; const R=v=>Math.round(v*10)/10;
function rect(el,base){const b=el.getBoundingClientRect();if(!base)return [R(b.x),R(b.y),R(b.width),R(b.height)].join(',');const o=base.getBoundingClientRect();return [R(b.x-o.x),R(b.y-o.y),R(b.width),R(b.height)].join(',');}
function d(label,el,base){ if(!el){out.push(' '+label+' MISSING');return;}
  const c=getComputedStyle(el);
  out.push(' '+label+(base?' rel ':' vp ')+rect(el,base)+' font='+c.fontWeight+' '+c.fontSize+'/'+c.lineHeight+' '+c.fontFamily.split(',')[0]+' color='+c.color+' bg='+c.backgroundColor+' bd='+c.borderTopWidth+' '+c.borderTopStyle+' '+c.borderTopColor+' bdb='+c.borderBottomWidth+' '+c.borderBottomColor+' rad='+c.borderTopLeftRadius+' pad='+c.padding+' op='+c.opacity+' tf='+c.transform+' shadow='+c.boxShadow+(el.children.length===0||el.tagName==='BUTTON'||el.tagName==='A'?' text="'+(el.textContent||'').trim().replace(/\s+/g,' ').slice(0,40)+'"':''));
}
function all(label,els,base){els.forEach((e,i)=>d(label+'['+i+']',e,base));}
function sec(id){const s=document.getElementById(id);const demo=s.querySelector('.demo');
  out.push('== '+id+' section '+rect(s)+' demo '+rect(demo));
  d('h2',s.querySelector('h2'),demo);d('p',s.querySelector('.section__head p'),demo);return demo;}
function pseudo(label,el,base){const c=getComputedStyle(el,'::after');
  out.push(' '+label+'::after w='+c.width+' h='+c.height+' top='+c.top+' bottom='+c.bottom+' left='+c.left+' right='+c.right+' font='+c.fontWeight+' '+c.fontSize+'/'+c.lineHeight+' color='+c.color+' bg='+c.backgroundColor+' rad='+c.borderTopLeftRadius+' pad='+c.padding+' op='+c.opacity+' tf='+c.transform+' shadow='+c.boxShadow+' content='+c.content+' host rel '+rect(el,base));}
try{
out.push(' viewport '+innerWidth+'x'+innerHeight);
// ---------- buttons
let demo=sec('buttons');
all('.demo__label',qa('.demo__label',demo),demo);all('.divider',qa('.divider',demo),demo);all('.row',qa('.row',demo),demo);
all('.btn',qa('.btn',demo),demo);all('btn svg',qa('.btn svg',demo),demo);d('.spinner',q('#loadingBtn .spinner'),demo);
const sp=getComputedStyle(q('#loadingBtn .spinner'));out.push(' spinner border-top='+sp.borderTopColor+' border-right='+sp.borderRightColor+' width='+sp.borderTopWidth);
q('#buttons .btn').focus();const fc=getComputedStyle(q('#buttons .btn'));out.push(' focused .btn outline='+fc.outlineStyle+' '+fc.outlineWidth+' '+fc.outlineColor+' offset='+fc.outlineOffset+' rad='+fc.borderTopLeftRadius+' focus-visible='+q('#buttons .btn').matches(':focus-visible'));
q('#loadingBtn').click();out.push('-- loading button clicked');d('#loadingBtn(working)',q('#loadingBtn'),demo);out.push(' disabled='+q('#loadingBtn').disabled+' aria-disabled='+q('#loadingBtn').getAttribute('aria-disabled'));
// ---------- action menu
demo=sec('action-menu');
const trig=q('#action-menu .menu__trigger'), ib=q('#action-menu .icon-btn');
trig.scrollIntoView({block:'start'});window.scrollBy(0,-120); // needs scroll-behavior:auto (above), else position() runs before the scroll and flips the panel
out.push(' trigger vp after scroll '+rect(trig));
d('.menu__trigger',trig,demo);d('trigger svg',trig.querySelector('svg'),demo);d('.icon-btn',ib,demo);d('icon svg',ib.querySelector('svg'),demo);d('#menuResult',q('#menuResult'),demo);
trig.click();let pn=q('#projectMenu');
out.push('-- project menu open (focus="'+document.activeElement.textContent.trim()+'") expanded='+trig.getAttribute('aria-expanded'));
d('.menu__panel',pn,demo);d('.menu__panel(rel trigger)',pn,trig);all('.menu__item',qa('.menu__item',pn),pn);all('item svg',qa('.menu__item svg',pn),pn);
all('.menu__shortcut',qa('.menu__shortcut',pn),pn);all('.menu__divider',qa('.menu__divider',pn),pn);d('trigger svg(open)',trig.querySelector('svg'),demo);
kd(pn,'ArrowDown');kd(pn,'ArrowDown');out.push(' after 2x Down focus="'+document.activeElement.textContent.trim()+'"');
kd(pn,'End');out.push(' End focus="'+document.activeElement.textContent.trim()+'"');kd(pn,'ArrowDown');out.push(' Down from last focus="'+document.activeElement.textContent.trim()+'"');
kd(pn,'ArrowUp');kd(pn,'ArrowUp');out.push(' 2x Up focus="'+document.activeElement.textContent.trim()+'" (Archive disabled is skipped)');
kd(pn,'Escape');out.push(' Escape: hidden='+pn.hidden+' focus="'+document.activeElement.textContent.trim()+'"');
ib.click();pn=q('#compactMenu');
out.push('-- compact menu open (focus="'+document.activeElement.textContent.trim()+'")');
d('.menu__panel',pn,demo);d('.menu__panel(rel icon-btn)',pn,ib);all('.menu__item',qa('.menu__item',pn),pn);
qa('.menu__item',pn)[1].click();out.push(' picked: result="'+q('#menuResult').textContent+'" hidden='+pn.hidden+' focus='+(document.activeElement===ib?'icon-btn':document.activeElement.tagName));
d('#menuResult(after)',q('#menuResult'),demo);
// ---------- accordion
demo=sec('accordion');
all('.demo__label',qa('.demo__label',demo),demo);all('.accordion',qa('.accordion',demo),demo);all('.accordion__item',qa('.accordion__item',demo),demo);
all('.accordion__trigger',qa('.accordion__trigger',demo),demo);all('trigger svg',qa('.accordion__trigger svg',demo),demo);
all('.accordion__panel',qa('.accordion__panel',demo).filter(p=>!p.hidden),demo);
const at=qa('.accordion__trigger',demo);
at[1].click();out.push('-- multi: Team members opened');d('demo',demo);all('.accordion__trigger',at.slice(0,3),demo);all('.accordion__panel',qa('.accordion__panel',demo).filter(p=>!p.hidden),demo);
at[4].click();out.push('-- single: Notifications opened (Account closes)');all('.accordion__trigger',at.slice(3),demo);all('.accordion__panel',qa('.accordion__panel',demo).filter(p=>!p.hidden),demo);
out.push(' expanded states: '+at.map(t=>t.getAttribute('aria-expanded')).join(','));
at[4].click();out.push('-- single: Notifications clicked again -> all closed: '+at.slice(3).map(t=>t.getAttribute('aria-expanded')).join(','));
d('demo(after)',demo);
// ---------- stepper
demo=sec('stepper');
const sd=q('#stepperDemo');d('.stepper',sd,demo);all('.step',qa('.step',sd),demo);all('.step__marker',qa('.step__marker',sd),demo);all('.step__line',qa('.step__line',sd),demo);
all('.step__title',qa('.step__title',sd),demo);all('.step__desc',qa('.step__desc',sd),demo);all('.divider',qa('.divider',demo),demo);
d('#stepPrev',q('#stepPrev'),demo);d('#stepNext',q('#stepNext'),demo);d('#stepStatus',q('#stepStatus'),demo);out.push(' prev disabled='+q('#stepPrev').disabled);
d('.demo__label',demo.querySelector('.demo__label'),demo);
const sv=demo.querySelector('.step--vertical');d('.step--vertical',sv,demo);all('v .step',qa('.step',sv),demo);all('v .step__marker',qa('.step__marker',sv),demo);all('v marker svg',qa('.step__marker svg',sv),demo);
all('v .step__line',qa('.step__line',sv),demo);all('v .step__text',qa('.step__text',sv),demo);all('v .step__title',qa('.step__title',sv),demo);all('v .step__desc',qa('.step__desc',sv),demo);
q('#stepNext').click();out.push('-- Next once');all('.step__marker',qa('.step__marker',sd),demo);d('marker svg',sd.querySelector('.step__marker svg'),demo);all('.step__line',qa('.step__line',sd).slice(0,1),demo);d('#stepStatus',q('#stepStatus'),demo);
q('#stepNext').click();q('#stepNext').click();out.push('-- at last step: next disabled='+q('#stepNext').disabled+' status="'+q('#stepStatus').textContent+'"');d('#stepNext(disabled)',q('#stepNext'),demo);all('.step__marker',qa('.step__marker',sd),demo);
// ---------- breadcrumbs
demo=sec('breadcrumbs');
all('.crumbs',qa('.crumbs',demo),demo);qa('.crumbs',demo).forEach((c,i)=>{all('crumbs'+i+' child',[...c.children],demo);});
// ---------- tooltips
demo=sec('tooltip-demo');
qa('.tip',demo).forEach((t,i)=>{d('.tip['+i+']',t,demo);d(' btn['+i+']',t.querySelector('.btn'),demo);pseudo('.tip['+i+']',t,demo);});
// ---------- tabs
demo=sec('tabs');
d('.tabs',demo.querySelector('.tabs'),demo);all('.tab',qa('.tab',demo),demo);all('.tabpanel',qa('.tabpanel',demo).filter(p=>!p.hidden),demo);
qa('.tab',demo)[1].click();out.push('-- Security clicked');all('.tab',qa('.tab',demo),demo);all('.tabpanel',qa('.tabpanel',demo).filter(p=>!p.hidden),demo);
qa('.tab',demo)[1].focus();kd(qa('.tab',demo)[1],'ArrowRight');out.push(' ArrowRight -> selected="'+qa('.tab',demo).find(t=>t.getAttribute('aria-selected')==='true').textContent+'" focus="'+document.activeElement.textContent+'"');
kd(document.activeElement,'ArrowRight');out.push(' ArrowRight wraps -> "'+document.activeElement.textContent+'"');kd(document.activeElement,'End');out.push(' End -> "'+document.activeElement.textContent+'"');kd(document.activeElement,'Home');out.push(' Home -> "'+document.activeElement.textContent+'"');
const tf=getComputedStyle(document.activeElement);out.push(' focused tab outline='+tf.outlineStyle+' '+tf.outlineWidth+' offset='+tf.outlineOffset+' rad='+tf.borderTopLeftRadius);
// ---------- sidebar (context for Q on sidebar nav)
out.push('== sidebar');d('.sidebar',q('.sidebar'));d('.brand',q('.brand'));d('.brand__mark',q('.brand__mark'));d('.brand__name',q('.brand__name'));d('.brand__sub',q('.brand__sub'));d('#sidebarToggle',q('#sidebarToggle'));
all('.nav-label',qa('.nav-label'));all('.nav a',qa('.nav a').slice(0,3));d('.nav a.active',q('.nav a.active'));d('.sidebar__foot',q('.sidebar__foot'));
}catch(err){out.push('ERROR '+err.message+' '+err.stack);}
document.body.innerHTML='<pre id=o>'+out.join('\n').replace(/&/g,'&amp;').replace(/</g,'&lt;')+'</pre>';
})();
