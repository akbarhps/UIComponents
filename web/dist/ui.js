// GENERATED from web/index.html - run node web/build-dist.mjs
(function () {
  if (document.documentElement.dataset.uiLoaded) return; // second copy: would bind every component twice
  document.documentElement.dataset.uiLoaded = "1";
Object.assign(window, { openOverlay, closeOverlay, setFieldError, showToast, createGrid, bindSimpleFilters, bindColumnFilters, bindRowSelection, bindSortable, createCommandPalette });
(function () {
  const sprite = "<svg width=\"0\" height=\"0\" style=\"position:absolute\" aria-hidden=\"true\">\n  <symbol id=\"i-spark\" viewBox=\"0 0 24 24\"><path fill=\"currentColor\" d=\"M12 1.6c.5 4.9 2.5 7 7.4 7.4-4.9.5-6.9 2.5-7.4 7.4-.5-4.9-2.5-6.9-7.4-7.4 4.9-.4 6.9-2.5 7.4-7.4Z\"/><path fill=\"currentColor\" d=\"M19.4 15.2c.25 2.4 1.2 3.4 3.6 3.6-2.4.25-3.35 1.2-3.6 3.6-.25-2.4-1.2-3.35-3.6-3.6 2.4-.2 3.35-1.2 3.6-3.6Z\"/></symbol>\n  <symbol id=\"i-grid\" viewBox=\"0 0 24 24\" fill=\"none\" stroke=\"currentColor\" stroke-width=\"1.8\" stroke-linecap=\"round\"><rect x=\"3\" y=\"3\" width=\"7\" height=\"7\" rx=\"1.5\"/><rect x=\"14\" y=\"3\" width=\"7\" height=\"7\" rx=\"1.5\"/><rect x=\"3\" y=\"14\" width=\"7\" height=\"7\" rx=\"1.5\"/><rect x=\"14\" y=\"14\" width=\"7\" height=\"7\" rx=\"1.5\"/></symbol>\n  <symbol id=\"i-button\" viewBox=\"0 0 24 24\" fill=\"none\" stroke=\"currentColor\" stroke-width=\"1.8\" stroke-linecap=\"round\"><rect x=\"3\" y=\"7\" width=\"18\" height=\"10\" rx=\"3\"/><path d=\"M9.5 12h5\"/></symbol>\n  <symbol id=\"i-form\" viewBox=\"0 0 24 24\" fill=\"none\" stroke=\"currentColor\" stroke-width=\"1.8\" stroke-linecap=\"round\" stroke-linejoin=\"round\"><rect x=\"3\" y=\"3\" width=\"18\" height=\"18\" rx=\"2\"/><path d=\"M7 9h10M7 13h10M7 17h6\"/></symbol>\n  <symbol id=\"i-card\" viewBox=\"0 0 24 24\" fill=\"none\" stroke=\"currentColor\" stroke-width=\"1.8\" stroke-linecap=\"round\"><rect x=\"3\" y=\"4\" width=\"18\" height=\"16\" rx=\"2\"/><path d=\"M3 9h18\"/></symbol>\n  <symbol id=\"i-modal\" viewBox=\"0 0 24 24\" fill=\"none\" stroke=\"currentColor\" stroke-width=\"1.8\" stroke-linecap=\"round\" stroke-linejoin=\"round\"><rect x=\"3\" y=\"4\" width=\"18\" height=\"14\" rx=\"2\"/><path d=\"M8 21h8M12 18v3\"/></symbol>\n  <symbol id=\"i-badge\" viewBox=\"0 0 24 24\" fill=\"none\" stroke=\"currentColor\" stroke-width=\"1.8\" stroke-linecap=\"round\"><path d=\"M12 3l2.5 2.5L18 4l-.5 3.5L21 9l-2.5 2.5L21 14l-3.5.5L18 18l-3.5-1.5L12 19l-2.5-2.5L6 18l.5-3.5L3 14l2.5-2.5L3 9l3.5-1.5L6 4l3.5 1.5L12 3Z\"/></symbol>\n  <symbol id=\"i-alert\" viewBox=\"0 0 24 24\" fill=\"none\" stroke=\"currentColor\" stroke-width=\"1.8\" stroke-linecap=\"round\" stroke-linejoin=\"round\"><path d=\"M12 3 2 20h20L12 3Z\"/><path d=\"M12 9v5M12 17.5v.5\"/></symbol>\n  <symbol id=\"i-more\" viewBox=\"0 0 24 24\" fill=\"none\" stroke=\"currentColor\" stroke-width=\"1.8\" stroke-linecap=\"round\"><circle cx=\"5\" cy=\"12\" r=\"1\"/><circle cx=\"12\" cy=\"12\" r=\"1\"/><circle cx=\"19\" cy=\"12\" r=\"1\"/></symbol>\n  <symbol id=\"i-check\" viewBox=\"0 0 24 24\" fill=\"none\" stroke=\"currentColor\" stroke-width=\"2.4\" stroke-linecap=\"round\" stroke-linejoin=\"round\"><path d=\"m5 12 5 5L19 7\"/></symbol>\n  <symbol id=\"i-close\" viewBox=\"0 0 24 24\" fill=\"none\" stroke=\"currentColor\" stroke-width=\"2\" stroke-linecap=\"round\"><path d=\"M6 6l12 12M18 6 6 18\"/></symbol>\n  <symbol id=\"i-search\" viewBox=\"0 0 24 24\" fill=\"none\" stroke=\"currentColor\" stroke-width=\"1.9\" stroke-linecap=\"round\"><circle cx=\"11\" cy=\"11\" r=\"7\"/><path d=\"m20 20-3.5-3.5\"/></symbol>\n  <symbol id=\"i-arrow\" viewBox=\"0 0 24 24\" fill=\"none\" stroke=\"currentColor\" stroke-width=\"2\" stroke-linecap=\"round\" stroke-linejoin=\"round\"><path d=\"M5 12h14M13 6l6 6-6 6\"/></symbol>\n  <symbol id=\"i-info\" viewBox=\"0 0 24 24\" fill=\"none\" stroke=\"currentColor\" stroke-width=\"1.9\" stroke-linecap=\"round\"><circle cx=\"12\" cy=\"12\" r=\"9\"/><path d=\"M12 11v5M12 8v.5\"/></symbol>\n  <symbol id=\"i-table\" viewBox=\"0 0 24 24\" fill=\"none\" stroke=\"currentColor\" stroke-width=\"1.8\" stroke-linejoin=\"round\"><rect x=\"3\" y=\"4\" width=\"18\" height=\"16\" rx=\"2\"/><path d=\"M3 9h18M3 14h18M9 9v11\"/></symbol>\n  <symbol id=\"i-filter\" viewBox=\"0 0 24 24\" fill=\"none\" stroke=\"currentColor\" stroke-width=\"1.8\" stroke-linecap=\"round\" stroke-linejoin=\"round\"><path d=\"M3 5h18l-7 8v6l-4 2v-8L3 5Z\"/></symbol>\n  <symbol id=\"i-panel\" viewBox=\"0 0 24 24\" fill=\"none\" stroke=\"currentColor\" stroke-width=\"1.8\" stroke-linejoin=\"round\"><rect x=\"3\" y=\"4\" width=\"18\" height=\"16\" rx=\"2\"/><path d=\"M15 4v16\"/></symbol>\n  <symbol id=\"i-chevron\" viewBox=\"0 0 24 24\" fill=\"none\" stroke=\"currentColor\" stroke-width=\"2\" stroke-linecap=\"round\" stroke-linejoin=\"round\"><path d=\"m6 9 6 6 6-6\"/></symbol>\n  <symbol id=\"i-pin\" viewBox=\"0 0 24 24\" fill=\"none\" stroke=\"currentColor\" stroke-width=\"1.8\" stroke-linecap=\"round\" stroke-linejoin=\"round\"><path d=\"M12 17v5M9 3h6l-1 6 3 3v2H7v-2l3-3-1-6Z\"/></symbol>\n  <symbol id=\"i-calendar\" viewBox=\"0 0 24 24\" fill=\"none\" stroke=\"currentColor\" stroke-width=\"1.8\" stroke-linejoin=\"round\"><rect x=\"3\" y=\"5\" width=\"18\" height=\"16\" rx=\"2\"/><path d=\"M3 10h18M8 3v4M16 3v4\"/></symbol>\n  <symbol id=\"i-clock\" viewBox=\"0 0 24 24\" fill=\"none\" stroke=\"currentColor\" stroke-width=\"1.8\" stroke-linecap=\"round\"><circle cx=\"12\" cy=\"12\" r=\"9\"/><path d=\"M12 7v5l3 2\"/></symbol>\n  <symbol id=\"i-upload\" viewBox=\"0 0 24 24\" fill=\"none\" stroke=\"currentColor\" stroke-width=\"1.8\" stroke-linecap=\"round\" stroke-linejoin=\"round\"><path d=\"M12 16V4m0 0L7 9m5-5 5 5\"/><path d=\"M4 16v3a2 2 0 0 0 2 2h12a2 2 0 0 0 2-2v-3\"/></symbol>\n  <symbol id=\"i-file\" viewBox=\"0 0 24 24\" fill=\"none\" stroke=\"currentColor\" stroke-width=\"1.8\" stroke-linejoin=\"round\"><path d=\"M6 3h8l4 4v14H6z\"/><path d=\"M14 3v4h4\"/></symbol>\n  <symbol id=\"i-edit\" viewBox=\"0 0 24 24\" fill=\"none\" stroke=\"currentColor\" stroke-width=\"1.8\" stroke-linecap=\"round\" stroke-linejoin=\"round\"><path d=\"M12 20h9\"/><path d=\"M16.5 3.5a2.1 2.1 0 0 1 3 3L7 19l-4 1 1-4 12.5-12.5Z\"/></symbol>\n  <symbol id=\"i-copy\" viewBox=\"0 0 24 24\" fill=\"none\" stroke=\"currentColor\" stroke-width=\"1.8\" stroke-linejoin=\"round\"><rect x=\"8\" y=\"8\" width=\"13\" height=\"13\" rx=\"2\"/><path d=\"M16 8V5a2 2 0 0 0-2-2H5a2 2 0 0 0-2 2v9a2 2 0 0 0 2 2h3\"/></symbol>\n  <symbol id=\"i-download\" viewBox=\"0 0 24 24\" fill=\"none\" stroke=\"currentColor\" stroke-width=\"1.8\" stroke-linecap=\"round\" stroke-linejoin=\"round\"><path d=\"M12 4v12m0 0 5-5m-5 5-5-5\"/><path d=\"M4 17v2a2 2 0 0 0 2 2h12a2 2 0 0 0 2-2v-2\"/></symbol>\n  <symbol id=\"i-trash\" viewBox=\"0 0 24 24\" fill=\"none\" stroke=\"currentColor\" stroke-width=\"1.8\" stroke-linecap=\"round\" stroke-linejoin=\"round\"><path d=\"M4 7h16M9 7V4h6v3m-9 0 1 13h10l1-13\"/></symbol>\n</svg>\n<svg width=\"0\" height=\"0\" style=\"position:absolute\" aria-hidden=\"true\">\n  <symbol id=\"i-plus\" viewBox=\"0 0 24 24\" fill=\"none\" stroke=\"currentColor\" stroke-width=\"2\" stroke-linecap=\"round\"><path d=\"M12 5v14M5 12h14\"/></symbol>\n</svg>";
  function inject() {
    if (!document.getElementById('i-check')) document.body.insertAdjacentHTML('afterbegin', sprite);
  }
  if (document.body) inject(); else document.addEventListener('DOMContentLoaded', inject);
})();

// Declared before any auto-init: a throw in one block must not leave these in the TDZ for createGrid/showToast.
const toastIcons = { success:'i-check', info:'i-info', error:'i-close' };
let activeGrid = null;
// Set by the demo's blocks below; web-checks calls these by name, so they stay page globals.
let openCmdk = () => {}, closeCmdk = () => {}, setProgress = () => {};
/* Overlays */
const overlays = [...document.querySelectorAll('.overlay')];
const lastFocusByOverlay = new Map();
function openOverlay(el, focusEl){
  lastFocusByOverlay.set(el, document.activeElement);
  el.classList.add('open');
  document.body.style.overflow = 'hidden';
  (focusEl || el.querySelector('input, button')).focus();
}
function closeOverlay(el){
  el.classList.remove('open');
  el._onClose?.(); // a component that resets on close (the command palette) registers this on its overlay
  if(!overlays.some(o => o.classList.contains('open'))) document.body.style.overflow = '';
  const previous = lastFocusByOverlay.get(el);
  lastFocusByOverlay.delete(el);
  if(previous && document.contains(previous)) previous.focus();
}
overlays.forEach(el => el.addEventListener('click', e => {
  if(e.target === el && !el.classList.contains('overlay--blocking')) closeOverlay(el);
}));
document.addEventListener('keydown', e => {
  if(e.key !== 'Escape') return;
  const open = overlays.find(el => el.classList.contains('open'));
  if(open && !open.classList.contains('overlay--blocking')) closeOverlay(open);
});
document.addEventListener('keydown', e => {
  if(e.key !== 'Tab') return;
  const open = overlays.find(el => el.classList.contains('open'));
  if(!open) return;
  const scope = open.querySelector('.drawer__confirm:not([hidden])') || open;
  const focusable = [...scope.querySelectorAll('a[href], button:not([disabled]), input:not([disabled]), select:not([disabled]), textarea:not([disabled]), [tabindex]:not([tabindex="-1"])')]
    .filter(el => el.offsetParent !== null);
  if(!focusable.length){ e.preventDefault(); return; }
  const first = focusable[0], last = focusable[focusable.length - 1];
  const active = document.activeElement;
  if(!scope.contains(active)){
    e.preventDefault();
    (e.shiftKey ? last : first).focus();
  } else if(e.shiftKey && active === first){
    e.preventDefault();
    last.focus();
  } else if(!e.shiftKey && active === last){
    e.preventDefault();
    first.focus();
  }
});

/* Confirmation modal */
const hasIds = (...ids) => ids.every(id => document.getElementById(id));
// Each auto-init runs alone: a throw is logged with its block name and the next block still runs.
function safe(name, fn){ try { fn(); } catch(err){ console.error('ui.js: ' + name + ' failed', err); } }
const overlay = document.getElementById('overlay');
safe('confirm modal', () => {
if(hasIds('overlay','openModal','closeModal','cancelModal','confirmModal')){ // demo-only wiring: skipped when the demo markup is absent
  document.getElementById('openModal').addEventListener('click', () => openOverlay(overlay, document.getElementById('closeModal')));
  document.getElementById('closeModal').addEventListener('click', () => closeOverlay(overlay));
  document.getElementById('cancelModal').addEventListener('click', () => closeOverlay(overlay));
  document.getElementById('confirmModal').addEventListener('click', () => { closeOverlay(overlay); showToast('success','Deleted','The project was removed.'); });
}
});

/* Loading modal */
const loadingOverlay = document.getElementById('loadingOverlay');
safe('loading modal', () => {
if(hasIds('loadingOverlay','openLoading')){ // demo-only wiring
  document.getElementById('openLoading').addEventListener('click', () => {
    openOverlay(loadingOverlay, loadingOverlay.querySelector('.loading-modal'));
    setTimeout(() => {
      closeOverlay(loadingOverlay);
      showToast('success', 'Ready', 'Your workspace is ready.');
    }, 3000);
  });
}
});

/* Modal form */
const formOverlay = document.getElementById('formOverlay');
const emailOk = value => /^[^\s@]+@[^\s@]+\.[^\s@]+$/.test(value.trim());
function setFieldError(field, on){
  field.classList.toggle('has-error', on);
  const control = field.querySelector('input, select, textarea');
  const error = field.querySelector('.field__error');
  if(control) on ? control.setAttribute('aria-invalid','true') : control.removeAttribute('aria-invalid');
  if(control && error) on ? control.setAttribute('aria-describedby', error.id) : control.removeAttribute('aria-describedby');
}
function resetForm(form){ form.reset(); form.querySelectorAll('.field.has-error').forEach(f => setFieldError(f, false)); }
safe('modal form', () => {
if(hasIds('formOverlay','openFormModal','closeFormModal','cancelFormModal','projectForm','fm-name','fm-email')){ // demo-only wiring
document.getElementById('openFormModal').addEventListener('click', () => { resetForm(document.getElementById('projectForm')); openOverlay(formOverlay, document.getElementById('fm-name')); });
document.getElementById('closeFormModal').addEventListener('click', () => closeOverlay(formOverlay));
document.getElementById('cancelFormModal').addEventListener('click', () => closeOverlay(formOverlay));
document.getElementById('projectForm').addEventListener('submit', e => {
  e.preventDefault();
  const name = document.getElementById('fm-name');
  const email = document.getElementById('fm-email');
  const validName = name.value.trim().length > 0;
  const validEmail = emailOk(email.value);
  setFieldError(name.closest('.field'), !validName);
  setFieldError(email.closest('.field'), !validEmail);
  if(!validName || !validEmail){ const first = e.target.querySelector('[aria-invalid="true"]'); if(first) first.focus(); return; }
  closeOverlay(formOverlay);
  showToast('success','Project created','“' + name.value.trim() + '” is ready.');
  e.target.reset();
});
['fm-name','fm-email'].forEach(id => document.getElementById(id)?.addEventListener('input', e => setFieldError(e.target.closest('.field'), false)));
}
});

/* Sidebar drawer form */
const drawerOverlay = document.getElementById('drawerOverlay');
safe('drawer form', () => {
if(hasIds('drawerOverlay','openDrawer','closeDrawer','cancelDrawer','inviteForm','dr-email')){ // demo-only wiring
document.getElementById('openDrawer').addEventListener('click', () => { resetForm(document.getElementById('inviteForm')); openOverlay(drawerOverlay, document.getElementById('dr-email')); });
document.getElementById('closeDrawer').addEventListener('click', () => closeOverlay(drawerOverlay));
document.getElementById('cancelDrawer').addEventListener('click', () => closeOverlay(drawerOverlay));
document.getElementById('inviteForm').addEventListener('submit', e => {
  e.preventDefault();
  const email = document.getElementById('dr-email');
  const valid = emailOk(email.value);
  setFieldError(email.closest('.field'), !valid);
  if(!valid){ const first = e.target.querySelector('[aria-invalid="true"]'); if(first) first.focus(); return; }
  closeOverlay(drawerOverlay);
  showToast('success','Invite sent','An invitation is on its way to ' + email.value.trim() + '.');
  e.target.reset();
});
document.getElementById('dr-email').addEventListener('input', e => setFieldError(e.target.closest('.field'), false));
}
});

/* Update drawer */
const editOverlay = document.getElementById('editDrawerOverlay');
const editConfirm = document.getElementById('editConfirm');
const editEmail = document.getElementById('ed-email');
safe('edit drawer', () => {
if(hasIds('editDrawerOverlay','openEditDrawer','closeEditDrawer','cancelEditDrawer','editForm','ed-name','ed-email','editConfirm','editDelete','keepMember','confirmDelete')){ // demo-only wiring
document.getElementById('openEditDrawer').addEventListener('click', () => {
  editConfirm.hidden = true;
  resetForm(document.getElementById('editForm'));
  openOverlay(editOverlay, document.getElementById('ed-name'));
});
document.getElementById('closeEditDrawer').addEventListener('click', () => closeOverlay(editOverlay));
document.getElementById('cancelEditDrawer').addEventListener('click', () => closeOverlay(editOverlay));
document.getElementById('editForm').addEventListener('submit', e => {
  e.preventDefault();
  const valid = emailOk(editEmail.value);
  setFieldError(editEmail.closest('.field'), !valid);
  if(!valid){ const first = e.target.querySelector('[aria-invalid="true"]'); if(first) first.focus(); return; }
  closeOverlay(editOverlay);
  showToast('success', 'Changes saved', "Jamie's details have been updated.");
});
editEmail.addEventListener('input', () => setFieldError(editEmail.closest('.field'), false));
document.getElementById('editDelete').addEventListener('click', () => {
  editConfirm.hidden = false;
  document.getElementById('keepMember').focus();
});
document.getElementById('keepMember').addEventListener('click', () => {
  editConfirm.hidden = true;
  document.getElementById('editDelete').focus();
});
document.getElementById('confirmDelete').addEventListener('click', () => {
  editConfirm.hidden = true;
  closeOverlay(editOverlay);
  showToast('success', 'Member deleted', 'Jamie no longer has access.');
});
editOverlay.addEventListener('keydown', e => {
  if(e.key === 'Escape' && !editConfirm.hidden){
    e.stopPropagation();
    editConfirm.hidden = true;
    document.getElementById('editDelete').focus();
  }
});
}
});

/* Table pagination */
// A .table-wrap is paginated when its parent element holds a pagination block (the parent scopes the controls; no .demo needed).
safe('pagination', () => {
document.querySelectorAll('.table-wrap').forEach(wrap => {
  const scope = wrap.parentElement;
  const table = wrap.querySelector('table');
  const getRows = () => [...table.tBodies[0].rows].filter(r => !r.id);
  const emptyRow = table.querySelector('tbody tr[id]');
  const info = scope.querySelector('[data-page-info]');
  const pageList = scope.querySelector('[data-page-list]');
  const prev = scope.querySelector('[data-page-prev]');
  const next = scope.querySelector('[data-page-next]');
  const limit = scope.querySelector('[data-page-limit]');
  if(!limit) return;
  let page = 1;
  let pageSize = Number(limit.value);

  function render(){
    const rows = getRows();
    const matched = rows.filter(r => r.dataset.match !== 'false');
    const pages = Math.max(1, Math.ceil(matched.length / pageSize));
    page = Math.min(page, pages);
    const start = (page - 1) * pageSize;
    const visible = matched.slice(start, start + pageSize);
    rows.forEach(r => r.hidden = !visible.includes(r));
    if(emptyRow) emptyRow.hidden = matched.length !== 0;
    info.textContent = matched.length
      ? 'Showing ' + (start + 1) + '–' + Math.min(start + pageSize, matched.length) + ' of ' + matched.length
      : 'No projects to show';
    pageList.innerHTML = '';
    for(let i = 1; i <= pages; i++){
      const btn = document.createElement('button');
      btn.type = 'button';
      btn.className = 'page-btn' + (i === page ? ' active' : '');
      btn.textContent = i;
      btn.setAttribute('aria-label', 'Page ' + i);
      btn.setAttribute('aria-current', i === page ? 'page' : 'false');
      btn.addEventListener('click', () => { page = i; render(); });
      pageList.appendChild(btn);
    }
    prev.disabled = page === 1;
    next.disabled = page === pages;
  }
  prev.addEventListener('click', () => { if(page > 1){ page--; render(); } });
  next.addEventListener('click', () => { page++; render(); });
  limit.addEventListener('change', () => { pageSize = Number(limit.value); page = 1; render(); });
  table._resetPage = () => { page = 1; render(); };
  render();
});
});

/* Search + status filters */
function bindSimpleFilters(tableId, searchId, filterId, countId){
  const rows = [...document.querySelectorAll('#' + tableId + ' tbody tr[data-status]')];
  const search = document.getElementById(searchId);
  const count = document.getElementById(countId);
  const buttons = [...document.querySelectorAll('#' + filterId + ' button')];
  let status = 'all';
  function apply(){
    const query = search.value.trim().toLowerCase();
    let shown = 0;
    rows.forEach(row => {
      const match = row.textContent.toLowerCase().includes(query) &&
        (status === 'all' || row.dataset.status === status);
      row.dataset.match = String(match);
      row.hidden = !match;
      if(match) shown++;
    });
    count.textContent = shown + ' of ' + rows.length + ' projects';
    document.getElementById(tableId)._resetPage?.();
  }
  search.addEventListener('input', apply);
  buttons.forEach(b => b.addEventListener('click', () => {
    status = b.dataset.status;
    buttons.forEach(x => x.classList.toggle('active', x === b));
    apply();
  }));
}
safe('table filters', () => {
if(document.getElementById('filterTable')) bindSimpleFilters('filterTable', 'tableSearch', 'tableFilters', 'tableCount');
});

/* Sortable table: a [data-sort-key] header button sorts the rows; a .pin-btn toggles .sticky-column. Call once per table. */
function bindSortable(tableEl){
  const sortBody = tableEl.tBodies[0];
  const sortButtons = [...tableEl.querySelectorAll('[data-sort-key]')];
  let sortKey = '', sortDir = 1;
  sortButtons.forEach(btn => btn.addEventListener('click', () => {
    const key = btn.dataset.sortKey;
    if(sortKey === key) sortDir *= -1;
    else { sortKey = key; sortDir = 1; }
    sortButtons.forEach(b => { b.classList.remove('asc', 'desc'); b.closest('th').removeAttribute('aria-sort'); });
    btn.classList.add(sortDir === 1 ? 'asc' : 'desc');
    btn.closest('th').setAttribute('aria-sort', sortDir === 1 ? 'ascending' : 'descending');
    const index = btn.closest('th').cellIndex;
    [...sortBody.rows].filter(r => !r.id)
      .sort((a, b) => {
        const av = a.cells[index].dataset.sortValue || a.cells[index].textContent.trim();
        const bv = b.cells[index].dataset.sortValue || b.cells[index].textContent.trim();
        const na = Number(av), nb = Number(bv);
        const cmp = !Number.isNaN(na) && !Number.isNaN(nb) ? na - nb : av.localeCompare(bv);
        return cmp * sortDir;
      })
      .forEach(row => sortBody.appendChild(row));
    tableEl._resetPage?.();
  }));
  const pin = tableEl.querySelector('.pin-btn');
  pin?.addEventListener('click', () => {
    const pinned = tableEl.classList.toggle('sticky-column');
    pin.setAttribute('aria-pressed', String(pinned));
  });
}
const sortTable = document.getElementById('sortTable');
safe('sortable table', () => {
if(hasIds('sortTable','stickyToggle')){ // demo-only wiring
  bindSortable(sortTable);
}
});

/* Column filters */
function bindColumnFilters(tableId, countId){
  const table = document.getElementById(tableId);
  const controls = [...table.querySelectorAll('thead input, thead select')];
  const count = document.getElementById(countId);
  const rows = () => [...table.tBodies[0].rows].filter(r => !r.id);
  function apply(){
    const values = controls.map(el => el.value.trim().toLowerCase());
    let shown = 0;
    rows().forEach(row => {
      const cells = [...row.cells];
      const match = controls.every((el, i) => {
        const value = values[i];
        if(!value || !cells[i]) return true;
        if(el.type === 'number'){
          const raw = cells[i].dataset.sortValue || cells[i].textContent.replace(/[^0-9.-]/g, '');
          return Number(raw) >= Number(value);
        }
        const text = cells[i].textContent.trim().toLowerCase();
        return el.tagName === 'SELECT' ? text === value : text.includes(value);
      });
      row.dataset.match = String(match);
      row.hidden = !match;
      if(match) shown++;
    });
    if(count) count.textContent = shown + ' of ' + rows().length + ' projects';
    table._resetPage?.();
  }
  controls.forEach(el => el.addEventListener(el.tagName === 'SELECT' ? 'change' : 'input', apply));
}
safe('column filters', () => {
if(document.getElementById('columnTable')) bindColumnFilters('columnTable', 'colCount');
});
safe('sortable column filters', () => {
if(sortTable) bindColumnFilters('sortTable');
});

/* Searchable selects */
safe('combo', () => {
document.querySelectorAll('.combo').forEach((combo, index) => {
  const trigger = combo.querySelector('.combo__trigger');
  const panel = combo.querySelector('.combo__panel');
  const search = combo.querySelector('.combo__input');
  const list = combo.querySelector('.combo__list');
  const options = [...list.querySelectorAll('.combo__option')];
  const empty = list.querySelector('.combo__empty');
  const valueEl = combo.querySelector('.combo__value');
  let activeIndex = -1;
  options.forEach((o, i) => o.id = 'combo-' + index + '-option-' + i);
  const visible = () => options.filter(o => !o.hidden);

  function setActive(i){
    const items = visible();
    if(!items.length) return;
    activeIndex = (i + items.length) % items.length;
    items.forEach((o, n) => o.classList.toggle('is-active', n === activeIndex));
    search.setAttribute('aria-activedescendant', items[activeIndex].id);
    items[activeIndex].scrollIntoView({ block: 'nearest' });
  }
  function open(){
    panel.hidden = false;
    trigger.setAttribute('aria-expanded', 'true');
    search.setAttribute('aria-expanded', 'true');
    search.value = '';
    options.forEach(o => { o.hidden = false; o.classList.remove('is-active'); });
    empty.hidden = true;
    activeIndex = -1;
    search.removeAttribute('aria-activedescendant');
    search.focus();
  }
  function close(restoreFocus = true){
    panel.hidden = true;
    trigger.setAttribute('aria-expanded', 'false');
    search.setAttribute('aria-expanded', 'false');
    search.removeAttribute('aria-activedescendant');
    if(restoreFocus) trigger.focus();
  }
  function choose(option){
    options.forEach(o => o.setAttribute('aria-selected', 'false'));
    option.setAttribute('aria-selected', 'true');
    valueEl.textContent = option.querySelector('.combo__option-label').textContent;
    valueEl.classList.remove('is-placeholder');
    close();
  }
  trigger.addEventListener('click', () => panel.hidden ? open() : close());
  trigger.addEventListener('keydown', e => {
    if((e.key === 'ArrowDown' || e.key === 'Enter' || e.key === ' ') && panel.hidden){
      e.preventDefault(); open(); setActive(0);
    }
  });
  search.addEventListener('input', () => {
    const q = search.value.trim().toLowerCase();
    options.forEach(o => o.hidden = !o.textContent.toLowerCase().includes(q));
    empty.hidden = visible().length > 0;
    options.forEach(o => o.classList.remove('is-active'));
    search.removeAttribute('aria-activedescendant');
    activeIndex = -1;
  });
  search.addEventListener('keydown', e => {
    const items = visible();
    if(e.key === 'ArrowDown'){ e.preventDefault(); setActive(activeIndex + 1); }
    else if(e.key === 'ArrowUp'){ e.preventDefault(); setActive(activeIndex < 0 ? -1 : activeIndex - 1); }
    else if(e.key === 'Enter' && items[activeIndex]){ e.preventDefault(); choose(items[activeIndex]); }
    else if(e.key === 'Escape'){ e.preventDefault(); close(); }
  });
  options.forEach(o => o.addEventListener('click', () => choose(o)));
  document.addEventListener('click', e => { if(!combo.contains(e.target) && !panel.hidden) close(false); });
  panel.addEventListener('mousedown', e => { if(e.target !== search) e.preventDefault(); });
  combo.addEventListener('focusout', e => { if(!combo.contains(e.relatedTarget)) close(false); });
});
});

/* Multi-selects */
safe('multi', () => {
document.querySelectorAll('.multi').forEach(multi => {
  const trigger = multi.querySelector('.multi__trigger');
  const panel = multi.querySelector('.multi__panel');
  const search = multi.querySelector('.multi__input');
  const options = [...multi.querySelectorAll('.multi__option')];
  const empty = multi.querySelector('.multi__empty');
  const chips = multi.querySelector('.multi__chips');
  const count = multi.querySelector('.multi__count');
  const selected = () => options.filter(o => o.querySelector('input').checked);

  function render(){
    chips.innerHTML = '';
    const chosen = selected();
    if(!chosen.length){
      const placeholder = document.createElement('span');
      placeholder.className = 'multi__placeholder';
      placeholder.textContent = multi.dataset.placeholder;
      chips.appendChild(placeholder);
    } else {
      chosen.forEach(option => {
        const label = option.querySelector('.multi__option-label').textContent;
        const chip = document.createElement('span');
        chip.className = 'multi__chip';
        chip.append(document.createTextNode(label));
        const remove = document.createElement('button');
        remove.type = 'button';
        remove.setAttribute('aria-label', 'Remove ' + label);
        remove.innerHTML = '<svg><use href="#i-close"/></svg>';
        remove.addEventListener('click', e => {
          e.stopPropagation();
          option.querySelector('input').checked = false;
          render();
        });
        chip.appendChild(remove);
        chips.appendChild(chip);
      });
    }
    count.textContent = chosen.length + ' selected';
    trigger.classList.toggle('multi__trigger--empty', !chosen.length);
  }
  function close(restoreFocus = true){
    panel.hidden = true;
    trigger.setAttribute('aria-expanded', 'false');
    if(restoreFocus) trigger.focus();
  }
  trigger.addEventListener('click', () => {
    const open = panel.hidden;
    panel.hidden = !open;
    trigger.setAttribute('aria-expanded', String(open));
    if(open){
      search.value = '';
      options.forEach(o => o.hidden = false);
      empty.hidden = true;
      search.focus();
    }
  });
  search.addEventListener('input', () => {
    const q = search.value.trim().toLowerCase();
    options.forEach(o => o.hidden = !o.textContent.toLowerCase().includes(q));
    empty.hidden = options.some(o => !o.hidden);
  });
  options.forEach(o => o.querySelector('input').addEventListener('change', render));
  multi.querySelector('.multi__select-all').addEventListener('click', () => {
    options.filter(o => !o.hidden).forEach(o => o.querySelector('input').checked = true);
    render();
  });
  multi.querySelector('.multi__clear').addEventListener('click', () => {
    options.forEach(o => o.querySelector('input').checked = false);
    render();
  });
  multi.querySelector('.multi__done').addEventListener('click', close);
  document.addEventListener('keydown', e => { if(e.key === 'Escape' && !panel.hidden) close(); });
  document.addEventListener('click', e => { if(!multi.contains(e.target) && !panel.hidden) close(false); });
  render();
});
});

/* Date & time pickers */
const MONTHS = ['Jan','Feb','Mar','Apr','May','Jun','Jul','Aug','Sep','Oct','Nov','Dec'];
const MONTHS_FULL = ['January','February','March','April','May','June','July','August','September','October','November','December'];
function initPicker(root, index){
  const mode = root.dataset.mode || 'date';
  const withSeconds = root.dataset.seconds === 'true';
  const withTime = root.dataset.time === 'true' || withSeconds;
  const trigger = root.querySelector('.picker__trigger');
  const valueEl = root.querySelector('.picker__value');
  const placeholder = valueEl.textContent;
  const today = new Date();
  let viewYear = today.getFullYear();
  let viewMonth = today.getMonth();
  let selected = null;

  const panelId = 'picker-panel-' + index;
  const dialogLabel = mode === 'date' ? (withTime ? 'Choose a date and time' : 'Choose a date') : mode === 'month' ? 'Choose a month' : 'Choose a year';
  trigger.setAttribute('aria-controls', panelId);
  root.insertAdjacentHTML('beforeend', `
    <div class="picker__panel" id="${panelId}" role="dialog" aria-label="${dialogLabel}" hidden>
      <div class="picker__head">
        <button type="button" class="icon-btn" data-prev aria-label="Previous"><svg><use href="#i-chevron"/></svg></button>
        <span class="picker__title"></span>
        <button type="button" class="icon-btn" data-next aria-label="Next"><svg><use href="#i-chevron"/></svg></button>
      </div>
      <div class="picker__body"></div>
      ${withTime ? '<div class="picker__time"><label>Hour<input class="input input--sm" type="number" min="0" max="23" data-hour value="09"/></label><label>Minute<input class="input input--sm" type="number" min="0" max="59" data-minute value="30"/></label>' + (withSeconds ? '<label>Second<input class="input input--sm" type="number" min="0" max="59" data-second value="00"/></label>' : '') + '</div>' : ''}
      <div class="picker__foot">
        <button type="button" class="btn btn--ghost btn--sm" data-clear>Clear</button>
        ${withTime ? '<button type="button" class="btn btn--primary btn--sm" data-apply>Apply</button>' : '<button type="button" class="btn btn--primary btn--sm" data-today>Today</button>'}
      </div>
    </div>`);
  const panel = root.querySelector('.picker__panel');
  const title = panel.querySelector('.picker__title');
  const body = panel.querySelector('.picker__body');
  const hour = panel.querySelector('[data-hour]');
  const minute = panel.querySelector('[data-minute]');
  const second = panel.querySelector('[data-second]');

  function format(){
    if(!selected) return '';
    if(mode === 'year') return String(selected.y);
    if(mode === 'month') return MONTHS[selected.m] + ' ' + selected.y;
    const date = MONTHS[selected.m] + ' ' + selected.d + ', ' + selected.y;
    const time = String(selected.h).padStart(2, '0') + ':' + String(selected.min).padStart(2, '0') + (withSeconds ? ':' + String(selected.s).padStart(2, '0') : '');
    return withTime ? date + ', ' + time : date;
  }
  function commit(){
    if(!selected) return;
    valueEl.textContent = format();
    valueEl.classList.remove('is-placeholder');
    panel.hidden = true;
    trigger.setAttribute('aria-expanded', 'false');
  }
  function render(){
    if(mode === 'date'){
      title.textContent = MONTHS_FULL[viewMonth] + ' ' + viewYear;
      const start = new Date(viewYear, viewMonth, 1).getDay();
      const days = new Date(viewYear, viewMonth + 1, 0).getDate();
      let html = '<div class="picker__grid">' + ['S','M','T','W','T','F','S'].map(d => '<span class="picker__dow">' + d + '</span>').join('');
      for(let i = 0; i < start; i++) html += '<span></span>';
      for(let d = 1; d <= days; d++){
        const isSelected = !!(selected && selected.y === viewYear && selected.m === viewMonth && selected.d === d);
        const isToday = today.getFullYear() === viewYear && today.getMonth() === viewMonth && today.getDate() === d;
        html += '<button type="button" class="picker__cell' + (isSelected ? ' selected' : '') + (isToday ? ' today' : '') + '" data-day="' + d + '" aria-label="' + MONTHS_FULL[viewMonth] + ' ' + d + ', ' + viewYear + '"' + (isSelected ? ' aria-pressed="true"' : '') + (isToday ? ' aria-current="date"' : '') + '>' + d + '</button>';
      }
      body.innerHTML = html + '</div>';
    } else if(mode === 'month'){
      title.textContent = viewYear;
      body.innerHTML = '<div class="picker__months">' + MONTHS_FULL.map((m, i) => {
        const isSelected = !!(selected && selected.y === viewYear && selected.m === i);
        return '<button type="button" class="picker__cell' + (isSelected ? ' selected' : '') + '" data-month="' + i + '" aria-label="' + m + '"' + (isSelected ? ' aria-pressed="true"' : '') + '>' + m.slice(0, 3) + '</button>';
      }).join('') + '</div>';
    } else {
      const start = viewYear - 8;
      title.textContent = start + '–' + (start + 19);
      let html = '<div class="picker__years">';
      for(let y = start; y < start + 20; y++) html += '<button type="button" class="picker__cell' + (selected && selected.y === y ? ' selected' : '') + '" data-year="' + y + '"' + (selected && selected.y === y ? ' aria-pressed="true"' : '') + '>' + y + '</button>';
      body.innerHTML = html + '</div>';
    }
  }
  function navigate(step){
    if(mode === 'date'){
      viewMonth += step;
      if(viewMonth < 0){ viewMonth = 11; viewYear--; }
      if(viewMonth > 11){ viewMonth = 0; viewYear++; }
    } else if(mode === 'month') viewYear += step;
    else viewYear += step * 20;
    render();
  }
  panel.querySelector('[data-prev]').addEventListener('click', () => navigate(-1));
  panel.querySelector('[data-next]').addEventListener('click', () => navigate(1));
  body.addEventListener('click', e => {
    e.stopPropagation();
    const cell = e.target.closest('.picker__cell');
    if(!cell) return;
    if(cell.dataset.day) selected = { y:viewYear, m:viewMonth, d:Number(cell.dataset.day) };
    else if(cell.dataset.month !== undefined) selected = { y:viewYear, m:Number(cell.dataset.month), d:1 };
    else selected = { y:Number(cell.dataset.year), m:0, d:1 };
    if(withTime){
      selected.h = Number(hour.value) || 0;
      selected.min = Number(minute.value) || 0;
      if(withSeconds) selected.s = Number(second.value) || 0;
    }
    render();
    if(!withTime) commit();
  });
  if(withTime){
    panel.querySelector('[data-apply]').addEventListener('click', () => {
      if(!selected) return;
      selected.h = Math.min(23, Math.max(0, Number(hour.value) || 0));
      selected.min = Math.min(59, Math.max(0, Number(minute.value) || 0));
      if(withSeconds) selected.s = Math.min(59, Math.max(0, Number(second.value) || 0));
      commit();
    });
  } else {
    panel.querySelector('[data-today]').addEventListener('click', () => {
      selected = { y:today.getFullYear(), m:today.getMonth(), d:today.getDate() };
      commit();
    });
  }
  panel.querySelector('[data-clear]').addEventListener('click', () => {
    selected = null;
    valueEl.textContent = placeholder;
    valueEl.classList.add('is-placeholder');
    panel.hidden = true;
    trigger.setAttribute('aria-expanded', 'false');
  });
  trigger.addEventListener('click', () => {
    const open = panel.hidden;
    panel.hidden = !open;
    trigger.setAttribute('aria-expanded', String(open));
    if(open){
      render();
      const cell = body.querySelector('.picker__cell.selected');
      if(cell) cell.focus();
    }
  });
  root.addEventListener('keydown', e => { if(e.key === 'Escape' && !panel.hidden){ panel.hidden = true; trigger.setAttribute('aria-expanded','false'); trigger.focus(); } });
  panel.addEventListener('mousedown', e => { if(!e.target.closest('input')) e.preventDefault(); });
  root.addEventListener('focusout', () => {
    setTimeout(() => {
      const a = document.activeElement;
      if(!panel.hidden && a !== document.body && !root.contains(a)){
        panel.hidden = true;
        trigger.setAttribute('aria-expanded', 'false');
      }
    });
  });
  document.addEventListener('click', e => {
    if(!root.contains(e.target) && !panel.hidden){
      panel.hidden = true;
      trigger.setAttribute('aria-expanded', 'false');
    }
  });
  render();
}
safe('picker', () => {
document.querySelectorAll('.picker').forEach((root, index) => initPicker(root, index));
});

/* Autocomplete */
safe('autocomplete', () => {
document.querySelectorAll('.ac').forEach(root => {
  const options = (root.dataset.options || '').split(',').map(s => s.trim()).filter(Boolean);
  const multiple = root.dataset.multiple === 'true';
  const input = root.querySelector('.ac__input');
  const list = root.querySelector('.ac__list');
  const hint = root.parentElement.querySelector('.ac__hint');
  const esc = s => String(s).replace(/&/g, '&amp;').replace(/</g, '&lt;').replace(/"/g, '&quot;');
  let tags = [], matches = [], active = -1;

  function renderTags(){
    root.querySelectorAll('.tag').forEach(el => el.remove());
    tags.forEach((tag, i) => {
      const chip = document.createElement('span');
      chip.className = 'tag';
      chip.append(document.createTextNode(tag));
      const remove = document.createElement('button');
      remove.type = 'button';
      remove.setAttribute('aria-label', 'Remove ' + tag);
      remove.innerHTML = '<svg><use href="#i-close"/></svg>';
      remove.addEventListener('click', e => { e.stopPropagation(); tags.splice(i, 1); renderTags(); updateHint(); });
      chip.appendChild(remove);
      input.parentElement.insertBefore(chip, input);
    });
  }
  function updateHint(){ if(hint) hint.textContent = tags.length + ' selected'; }
  function show(){
    const q = input.value.trim().toLowerCase();
    matches = options.filter(o => o.toLowerCase().includes(q) && !(multiple && tags.some(t => t.toLowerCase() === o.toLowerCase()))).slice(0, 8);
    active = -1;
    list.innerHTML = matches.length
      ? matches.map((o, i) => '<button type="button" class="ac__option" data-i="' + i + '">' + esc(o) + '</button>').join('')
      : '<div class="ac__empty">No suggestions</div>';
    list.hidden = false;
  }
  function hide(){ list.hidden = true; }
  function highlight(){ list.querySelectorAll('.ac__option').forEach((o, i) => o.classList.toggle('is-active', i === active)); list.querySelectorAll('.ac__option')[active]?.scrollIntoView({block:'nearest'}); }
  function choose(value){
    if(multiple){ if(!tags.some(t => t.toLowerCase() === value.toLowerCase())){ tags.push(value); renderTags(); updateHint(); } input.value = ''; }
    else input.value = value;
    hide();
    input.focus();
  }
  input.addEventListener('input', show);
  input.addEventListener('focus', show);
  input.addEventListener('keydown', e => {
    if(e.key === 'ArrowDown' || e.key === 'ArrowUp'){
      e.preventDefault();
      if(list.hidden || !matches.length){ show(); return; }
      active = e.key === 'ArrowDown' ? Math.min(active + 1, matches.length - 1) : Math.max(active - 1, 0);
      highlight();
    } else if(e.key === 'Enter'){
      if(!list.hidden && active >= 0 && matches[active]){ e.preventDefault(); choose(matches[active]); }
      else if(multiple && input.value.trim()){ e.preventDefault(); choose(input.value.trim()); }
    } else if(e.key === 'Escape') hide();
  });
  list.addEventListener('mousedown', e => e.preventDefault());
  list.addEventListener('click', e => {
    const option = e.target.closest('.ac__option');
    if(option) choose(matches[+option.dataset.i]);
  });
  document.addEventListener('click', e => { if(!root.contains(e.target)) hide(); });
  root.addEventListener('focusout', e => { if(!root.contains(e.relatedTarget)) hide(); });
  renderTags();
  updateHint();
});
});

/* Tag inputs */
safe('tag input', () => {
document.querySelectorAll('.tag-input-wrap').forEach(root => {
  const container = root.querySelector('.tag-input');
  const input = root.querySelector('.tag-input__field');
  const hint = root.querySelector('.tag-input__hint');
  const max = Number(root.dataset.max) || Infinity;
  let tags = (root.dataset.tags || '').split(',').map(t => t.trim()).filter(Boolean);
  function render(){
    container.querySelectorAll('.tag').forEach(el => el.remove());
    tags.forEach((tag, i) => {
      const chip = document.createElement('span');
      chip.className = 'tag';
      chip.append(document.createTextNode(tag));
      const remove = document.createElement('button');
      remove.type = 'button';
      remove.setAttribute('aria-label', 'Remove ' + tag);
      remove.innerHTML = '<svg><use href="#i-close"/></svg>';
      remove.addEventListener('mousedown', e => e.preventDefault());
      remove.addEventListener('click', e => { e.stopPropagation(); tags.splice(i, 1); render(); });
      chip.appendChild(remove);
      container.insertBefore(chip, input);
    });
    hint.textContent = tags.length + (max !== Infinity ? ' of ' + max : '') + ' tags';
  }
  function add(value){
    const tag = value.trim();
    if(!tag || tags.length >= max) return;
    if(tags.some(t => t.toLowerCase() === tag.toLowerCase())) return;
    tags.push(tag);
    render();
  }
  input.addEventListener('keydown', e => {
    if(e.key === 'Enter' || e.key === ','){
      e.preventDefault();
      input.value.split(',').forEach(add);
      input.value = '';
    } else if(e.key === 'Backspace' && !input.value && tags.length){
      tags.pop();
      render();
    }
  });
  input.addEventListener('blur', () => { if(input.value.trim()){ add(input.value); input.value = ''; } });
  container.addEventListener('click', () => input.focus());
  render();
});
});

/* Spreadsheet inputs */
function createGrid(rootId, initialColumns, initialData, options = {}){
  const allowAddColumn = options.allowAddColumn !== false;
  const root = document.getElementById(rootId);
  let columns = initialColumns.map(col => ({ ...col }));
  let data = initialData.map(row => [...row]);
  const selection = new Set();
  let anchor = null, dragging = false, dragAdditive = false, lastDrag = '';
  const head = root.querySelector('thead');
  const body = root.querySelector('tbody');
  const info = root.querySelector('[data-sheet-info]');
  const cellKey = (r, c) => r + ':' + c;
  const control = el => el.closest('.sheet__control');
  const escapeAttr = s => String(s).replace(/&/g, '&amp;').replace(/"/g, '&quot;').replace(/</g, '&lt;');
  const typeOf = c => (columns[c] && columns[c].type) || 'text';

  function ensure(r, c){
    while(data.length <= r) data.push(Array(columns.length).fill(''));
    while(columns.length <= c){
      columns.push({ name: 'Column ' + (columns.length + 1), type: 'text' });
      data.forEach(row => row.push(''));
    }
  }
  function rowSelected(r){ return Array.from({ length: columns.length }, (_, c) => selection.has(cellKey(r, c))).every(Boolean); }
  function colSelected(c){ return data.every((_, r) => selection.has(cellKey(r, c))); }
  function paint(){
    activeGrid = root;
    root._copySelection = selectionText;
    root._pasteData = pasteData;
    body.querySelectorAll('.sheet__cell').forEach(td => {
      const el = td.querySelector('.sheet__control');
      td.classList.toggle('selected', selection.has(cellKey(+el.dataset.r, +el.dataset.c)));
    });
    body.querySelectorAll('[data-row]').forEach(h => h.classList.toggle('selected', rowSelected(+h.dataset.row)));
    head.querySelectorAll('[data-col]').forEach(h => h.classList.toggle('selected', colSelected(+h.dataset.col)));
    info.textContent = selection.size + (selection.size === 1 ? ' cell selected' : ' cells selected');
  }
  function cellHTML(r, c, value){
    const col = columns[c];
    const type = col.type || 'text';
    const sel = selection.has(cellKey(r, c)) ? ' selected' : '';
    const label = escapeAttr(col.name + ', row ' + (r + 1));
    const attrs = 'data-r="' + r + '" data-c="' + c + '" aria-label="' + label + '"';
    if(type === 'checkbox')
      return '<td class="sheet__cell sheet__cell--center' + sel + '"><input type="checkbox" class="sheet__control sheet__checkbox" ' + attrs + (value === 'true' || value === true ? ' checked' : '') + '/></td>';
    if(type === 'select')
      return '<td class="sheet__cell' + sel + '"><select class="sheet__control sheet__input sheet__select" ' + attrs + '><option value=""></option>' +
        col.options.map(o => '<option value="' + escapeAttr(o) + '"' + (value === o ? ' selected' : '') + '>' + escapeAttr(o) + '</option>').join('') + '</select></td>';
    return '<td class="sheet__cell' + sel + '"><input class="sheet__control sheet__input" ' + attrs + ' value="' + escapeAttr(value) + '"/></td>';
  }
  function render(){
    const addColHead = allowAddColumn ? '<th class="sheet__add" data-add-col title="Add column">+</th>' : '';
    const addColCell = allowAddColumn ? '<td class="sheet__add-cell"></td>' : '';
    head.innerHTML = '<tr><th class="sheet__head"></th>' + columns.map((col, c) =>
      '<th scope="col" class="sheet__head sheet__head--col' + (colSelected(c) ? ' selected' : '') + '" data-col="' + c + '">' + escapeAttr(col.name) + '</th>').join('') + addColHead + '</tr>';
    body.innerHTML = data.map((row, r) =>
      '<tr><th scope="row" class="sheet__head' + (rowSelected(r) ? ' selected' : '') + '" data-row="' + r + '">' + (r + 1) + '</th>' +
      row.map((value, c) => cellHTML(r, c, value)).join('') + addColCell + '</tr>').join('') +
      '<tr class="sheet__add-row"><th class="sheet__add" data-add-row title="Add row">+</th><td class="sheet__add-cell" colspan="' + (columns.length + (allowAddColumn ? 1 : 0)) + '" data-add-row>+ Add row</td></tr>';
    paint();
  }
  function selectCell(r, c, event){
    if(event.shiftKey && anchor){
      const [ar, ac] = anchor;
      for(let i = Math.min(ar, r); i <= Math.max(ar, r); i++)
        for(let j = Math.min(ac, c); j <= Math.max(ac, c); j++) selection.add(cellKey(i, j));
    } else if(event.ctrlKey || event.metaKey){
      const k = cellKey(r, c);
      if(selection.has(k)) selection.delete(k); else selection.add(k);
      anchor = [r, c];
    } else {
      selection.clear();
      selection.add(cellKey(r, c));
      anchor = [r, c];
    }
    paint();
  }
  function selectRange(r, c, additive){
    if(!anchor) return;
    const [ar, ac] = anchor;
    if(!additive) selection.clear();
    for(let i = Math.min(ar, r); i <= Math.max(ar, r); i++)
      for(let j = Math.min(ac, c); j <= Math.max(ac, c); j++) selection.add(cellKey(i, j));
    paint();
  }
  function clearSelected(){
    selection.forEach(k => {
      const [r, c] = k.split(':').map(Number);
      if(data[r]) data[r][c] = '';
    });
    body.querySelectorAll('.sheet__control').forEach(el => {
      if(!selection.has(cellKey(+el.dataset.r, +el.dataset.c))) return;
      if(el.type === 'checkbox') el.checked = false; else el.value = '';
    });
  }
  body.addEventListener('click', e => {
    const row = e.target.closest('[data-row]');
    if(!row) return;
    const r = +row.dataset.row;
    if(e.shiftKey && anchor){
      const start = Math.min(anchor[0], r), end = Math.max(anchor[0], r);
      if(!e.ctrlKey && !e.metaKey) selection.clear();
      for(let i = start; i <= end; i++)
        for(let c = 0; c < columns.length; c++) selection.add(cellKey(i, c));
    } else if(e.ctrlKey || e.metaKey){
      for(let c = 0; c < columns.length; c++){
        const k = cellKey(r, c);
        if(selection.has(k)) selection.delete(k); else selection.add(k);
      }
      anchor = [r, 0];
    } else {
      selection.clear();
      for(let c = 0; c < columns.length; c++) selection.add(cellKey(r, c));
      anchor = [r, 0];
    }
    paint();
    if(document.activeElement && document.activeElement.blur) document.activeElement.blur();
  });
  body.addEventListener('mousedown', e => {
    const el = control(e.target);
    if(!el) return;
    dragging = true;
    dragAdditive = e.ctrlKey || e.metaKey;
    lastDrag = el.dataset.r + ':' + el.dataset.c;
    selectCell(+el.dataset.r, +el.dataset.c, e);
  });
  body.addEventListener('mouseover', e => {
    if(!dragging) return;
    const el = control(e.target);
    if(!el) return;
    const key = el.dataset.r + ':' + el.dataset.c;
    if(key === lastDrag) return;
    lastDrag = key;
    selectRange(+el.dataset.r, +el.dataset.c, dragAdditive);
  });
  document.addEventListener('mouseup', () => { dragging = false; lastDrag = ''; });
  body.addEventListener('focusin', e => {
    const el = control(e.target);
    if(!el) return;
    const r = +el.dataset.r, c = +el.dataset.c;
    if(!selection.has(cellKey(r, c))){
      selection.clear();
      selection.add(cellKey(r, c));
      anchor = [r, c];
      paint();
    }
  });
  body.addEventListener('input', e => {
    const el = control(e.target);
    if(!el) return;
    const r = +el.dataset.r, c = +el.dataset.c;
    const value = el.type === 'checkbox' ? String(el.checked) : el.value;
    data[r][c] = value;
    if(selection.size > 1){
      selection.forEach(k => {
        const [sr, sc] = k.split(':').map(Number);
        if((sr === r && sc === c) || !data[sr]) return;
        if(typeOf(sc) !== typeOf(c)) return;
        if(typeOf(c) !== 'text' && sc !== c) return;
        data[sr][sc] = value;
        const target = body.querySelector('.sheet__control[data-r="' + sr + '"][data-c="' + sc + '"]');
        if(target){ if(target.type === 'checkbox') target.checked = value === 'true'; else target.value = value; }
      });
    }
  });
  body.addEventListener('keydown', e => {
    const el = control(e.target);
    if(!el) return;
    if((e.key === 'Delete' || e.key === 'Backspace') && selection.size > 1){
      e.preventDefault();
      clearSelected();
    } else if(e.key === 'Enter'){
      e.preventDefault();
      const r = +el.dataset.r, c = +el.dataset.c;
      ensure(r + 1, c);
      if(body.querySelectorAll('.sheet__control').length !== data.length * columns.length) render();
      const next = body.querySelector('.sheet__control[data-r="' + (r + 1) + '"][data-c="' + c + '"]');
      if(next) next.focus();
    }
  });
  head.addEventListener('click', e => {
    const col = e.target.closest('[data-col]');
    if(!col) return;
    const c = +col.dataset.col;
    selection.clear();
    data.forEach((_, r) => selection.add(cellKey(r, c)));
    anchor = [0, c];
    paint();
    if(document.activeElement && document.activeElement.blur) document.activeElement.blur();
  });
  function pasteData(text){
    if(!text || (!text.includes('\t') && !text.includes('\n'))) return false;
    const active = control(document.activeElement);
    const [r, c] = active ? [+active.dataset.r, +active.dataset.c] : (anchor || [0, 0]);
    const rows = text.replace(/\r/g, '').split('\n');
    if(rows[rows.length - 1] === '') rows.pop();
    rows.forEach((line, ri) => line.split('\t').forEach((value, ci) => {
      if(!allowAddColumn && c + ci >= columns.length) return;
      ensure(r + ri, c + ci);
      const type = typeOf(c + ci);
      let stored = value;
      if(type === 'checkbox') stored = /^(true|1|yes)$/i.test(value.trim()) ? 'true' : 'false';
      else if(type === 'select' && !columns[c + ci].options.includes(value)) stored = '';
      data[r + ri][c + ci] = stored;
    }));
    render();
    const focusCell = body.querySelector('.sheet__control[data-r="' + r + '"][data-c="' + c + '"]');
    if(focusCell) focusCell.focus();
    return true;
  }
  function selectionText(){
    if(selection.size < 2) return '';
    const rows = [...selection].map(k => k.split(':').map(Number));
    const minR = Math.min(...rows.map(x => x[0])), maxR = Math.max(...rows.map(x => x[0]));
    const minC = Math.min(...rows.map(x => x[1])), maxC = Math.max(...rows.map(x => x[1]));
    const lines = [];
    for(let r = minR; r <= maxR; r++){
      const cells = [];
      for(let c = minC; c <= maxC; c++){
        const el = body.querySelector('.sheet__control[data-r="' + r + '"][data-c="' + c + '"]');
        cells.push(selection.has(cellKey(r, c)) && el ? (el.type === 'checkbox' ? String(el.checked) : el.value) : '');
      }
      lines.push(cells.join('\t'));
    }
    return lines.join('\n');
  }
  root.querySelector('[data-sheet-copy]').addEventListener('click', async () => {
    const text = selectionText();
    if(!text) return;
    try {
      await navigator.clipboard.writeText(text);
    } catch {
      const temp = document.createElement('textarea');
      temp.value = text;
      temp.style.position = 'fixed';
      temp.style.opacity = '0';
      document.body.appendChild(temp);
      temp.select();
      document.execCommand('copy');
      temp.remove();
    }
    showToast('success', 'Copied', selection.size + ' cells copied. Paste into Excel.');
  });
  root.addEventListener('click', e => {
    if(e.target.closest('[data-add-row]')){
      data.push(Array(columns.length).fill(''));
      render();
    } else if(e.target.closest('[data-add-col]')){
      columns.push({ name: 'Column ' + (columns.length + 1), type: 'text' });
      data.forEach(row => row.push(''));
      render();
    }
  });
  root.querySelector('[data-sheet-clear]').addEventListener('click', clearSelected);
  render();
}
document.addEventListener('copy', e => {
  const target = e.target;
  if(target && (target.tagName === 'INPUT' || target.tagName === 'TEXTAREA') && target.selectionStart !== target.selectionEnd) return;
  if(!activeGrid || !activeGrid._copySelection) return;
  const s = getSelection(); if(s && !s.isCollapsed && !activeGrid.contains(s.anchorNode)) return;
  const text = activeGrid._copySelection();
  if(!text) return;
  e.preventDefault();
  (e.clipboardData || window.clipboardData).setData('text/plain', text);
  showToast('success', 'Copied', 'Cells copied. Paste into Excel.');
});
document.addEventListener('paste', e => {
  const target = e.target;
  if(target && /^(INPUT|TEXTAREA|SELECT)$/.test(target.tagName) && target.closest('.sheet') === null) return;
  if(!activeGrid || !activeGrid._pasteData) return;
  if(!activeGrid.contains(e.target) && e.target !== document.body) return;
  const text = (e.clipboardData || window.clipboardData).getData('text');
  if(activeGrid._pasteData(text)) e.preventDefault();
});
safe('grid sheet', () => {
if(document.getElementById('sheet')) createGrid('sheet', [
  { name: 'Width', type: 'text' }, { name: 'Height', type: 'text' },
  { name: 'Depth', type: 'text' }, { name: 'Weight', type: 'text' }
], [
  ['Small', '10', '20', '5'], ['Medium', '14', '24', '8'], ['Large', '18', '30', '12']
]);
});
safe('typed grid', () => {
if(document.getElementById('typedSheet')) createGrid('typedSheet', [
  { name: 'Variant', type: 'text' },
  { name: 'Width', type: 'text' },
  { name: 'Height', type: 'text' },
  { name: 'Category', type: 'select', options: ['Standard', 'Pro', 'Custom'] },
  { name: 'IsActive', type: 'checkbox' }
], [
  ['Small', '10', '20', 'Standard', 'true'],
  ['Medium', '14', '24', 'Pro', 'true'],
  ['Large', '18', '30', 'Custom', 'false']
], { allowAddColumn: false });
});

/* Action menus */
safe('action menu', () => {
document.querySelectorAll('.menu').forEach(menu => {
  const trigger = menu.querySelector('[aria-haspopup="menu"]');
  const panel = menu.querySelector('.menu__panel');
  const items = [...panel.querySelectorAll('.menu__item')];
  const enabled = () => items.filter(item => !item.disabled);
  function position(){
    panel.style.top = '';
    panel.style.bottom = '';
    panel.style.left = '';
    panel.style.right = '';
    const t = trigger.getBoundingClientRect();
    const p = panel.getBoundingClientRect();
    const host = menu.getBoundingClientRect();
    const margin = 8, gap = 6;
    let top = t.bottom + gap;
    if(top + p.height > innerHeight - margin && t.top - gap - p.height > margin) top = t.top - p.height - gap;
    top = Math.max(margin, Math.min(top, innerHeight - p.height - margin));
    let left = t.right - p.width;
    if(left < margin) left = margin;
    if(left + p.width > innerWidth - margin) left = innerWidth - p.width - margin;
    panel.style.top = (top - host.top) + 'px';
    panel.style.left = (left - host.left) + 'px';
    panel.style.right = 'auto';
  }
  function open(focusFirst){
    panel.hidden = false;
    position();
    trigger.setAttribute('aria-expanded', 'true');
    if(focusFirst && enabled().length) enabled()[0].focus();
  }
  function close(restoreFocus){
    panel.hidden = true;
    trigger.setAttribute('aria-expanded', 'false');
    if(restoreFocus) trigger.focus();
  }
  trigger.addEventListener('click', () => panel.hidden ? open(true) : close(true));
  trigger.addEventListener('keydown', e => {
    if(e.key === 'ArrowDown'){ e.preventDefault(); open(true); }
  });
  panel.addEventListener('keydown', e => {
    const list = enabled();
    const index = list.indexOf(document.activeElement);
    if(e.key === 'ArrowDown' || e.key === 'ArrowUp'){
      e.preventDefault();
      if(index === -1) list[0].focus();
      else list[(index + (e.key === 'ArrowDown' ? 1 : -1) + list.length) % list.length].focus();
    } else if(e.key === 'Home'){ e.preventDefault(); list[0].focus(); }
    else if(e.key === 'End'){ e.preventDefault(); list[list.length - 1].focus(); }
    else if(e.key === 'Escape'){ e.preventDefault(); e.stopPropagation(); close(true); }
    else if(e.key === 'Tab') close(false);
  });
  items.forEach(item => item.addEventListener('click', () => {
    const result = document.getElementById('menuResult');
    if(result && item.dataset.action) result.textContent = item.dataset.action;
    close(true);
  }));
  document.addEventListener('click', e => { if(!menu.contains(e.target) && !panel.hidden) close(false); });
});
});

/* File inputs */
safe('file input', () => {
document.querySelectorAll('.file-input').forEach(box => {
  const input = box.querySelector('input[type=file]');
  const drop = box.querySelector('.file-drop');
  const list = box.querySelector('.file-list');
  const count = box.querySelector('.file-count');
  const multiple = box.dataset.multiple === 'true';
  let files = [];
  function sizeLabel(bytes){
    if(bytes < 1024) return bytes + ' B';
    if(bytes < 1024 * 1024) return Math.round(bytes / 1024) + ' KB';
    return (bytes / (1024 * 1024)).toFixed(1) + ' MB';
  }
  function render(){
    count.hidden = files.length === 0;
    count.textContent = files.length + (files.length === 1 ? ' file selected' : ' files selected');
    list.innerHTML = '';
    files.forEach((file, i) => {
      const row = document.createElement('div');
      row.className = 'file-item';
      row.innerHTML = '<svg><use href="#i-file"/></svg><span class="file-item__name"></span><span class="file-item__size"></span><button type="button" class="icon-btn"><svg><use href="#i-close"/></svg></button>';
      row.querySelector('button').setAttribute('aria-label', 'Remove ' + file.name);
      row.querySelector('.file-item__name').textContent = file.name;
      row.querySelector('.file-item__size').textContent = sizeLabel(file.size);
      row.querySelector('button').addEventListener('click', () => {
        files.splice(i, 1);
        input.value = '';
        render();
      });
      list.appendChild(row);
    });
  }
  const accepted = f => !input.accept || f.type.startsWith('image/');
  function add(chosen){
    const fresh = [];
    chosen.forEach(f => {
      if(!accepted(f)) return;
      if(multiple && [...files, ...fresh].some(x => x.name === f.name && x.size === f.size)) return;
      fresh.push(f);
    });
    if(!fresh.length) return;
    files = multiple ? [...files, ...fresh] : fresh.slice(0, 1);
    render();
  }
  input.addEventListener('change', () => add([...input.files]));
  ['dragenter','dragover'].forEach(type => drop.addEventListener(type, e => {
    e.preventDefault();
    drop.classList.add('dragover');
  }));
  ['dragleave','drop'].forEach(type => drop.addEventListener(type, e => {
    e.preventDefault();
    drop.classList.remove('dragover');
  }));
  drop.addEventListener('drop', e => add([...e.dataTransfer.files]));
  render();
});
});

/* Command palette: Ctrl/Cmd+K opens it. commands: [{ group, label, icon, shortcut, run }]. One palette per page. */
function createCommandPalette(overlayEl, commands){
  const input = overlayEl.querySelector('.cmdk__input');
  const list = overlayEl.querySelector('.cmdk__list');
  let active = 0, matches = [];
  overlayEl._onClose = () => { input.setAttribute('aria-expanded', 'false'); input.removeAttribute('aria-activedescendant'); };
  function render(){
    const q = input.value.trim().toLowerCase();
    matches = commands.filter(c => c.label.toLowerCase().includes(q));
    active = 0;
    if(!matches.length){ list.innerHTML = '<div class="cmdk__empty" role="presentation">No matching commands</div>'; input.removeAttribute('aria-activedescendant'); return; }
    let html = '', lastGroup = '';
    matches.forEach((c, i) => {
      if(c.group !== lastGroup){ html += '<div class="cmdk__group" role="presentation">' + c.group + '</div>'; lastGroup = c.group; }
      html += '<button type="button" role="option" id="cmdk-option-' + i + '" aria-selected="' + (i === 0) + '" class="cmdk__item' + (i === 0 ? ' is-active' : '') + '" data-i="' + i + '"><svg><use href="#' + c.icon + '"/></svg><span class="cmdk__item-label">' + c.label + '</span>' + (c.shortcut ? '<span class="cmdk__shortcut">' + c.shortcut + '</span>' : '') + '</button>';
    });
    list.innerHTML = html;
    input.setAttribute('aria-activedescendant', list.querySelector('.cmdk__item').id);
  }
  function setActive(i){
    const items = list.querySelectorAll('.cmdk__item');
    if(!items.length) return;
    active = (i + items.length) % items.length;
    items.forEach((el, n) => { el.classList.toggle('is-active', n === active); el.setAttribute('aria-selected', String(n === active)); });
    input.setAttribute('aria-activedescendant', items[active].id);
    items[active].scrollIntoView({ block:'nearest' });
  }
  function open(){
    openOverlay(overlayEl, input);
    input.value = '';
    input.setAttribute('aria-expanded', 'true');
    render();
  }
  function close(){ closeOverlay(overlayEl); }
  input.addEventListener('input', render);
  input.addEventListener('keydown', e => {
    if(e.key === 'ArrowDown'){ e.preventDefault(); setActive(active + 1); }
    else if(e.key === 'ArrowUp'){ e.preventDefault(); setActive(active - 1); }
    else if(e.key === 'Enter' && matches[active]){
      e.preventDefault();
      const command = matches[active];
      close();
      command.run();
    }
  });
  list.addEventListener('click', e => {
    const item = e.target.closest('.cmdk__item');
    if(!item) return;
    const command = matches[+item.dataset.i];
    close();
    command.run();
  });
  document.addEventListener('keydown', e => {
    if((e.ctrlKey || e.metaKey) && e.key.toLowerCase() === 'k'){
      if(overlays.some(o => o.classList.contains('open'))){ e.preventDefault(); return; }
      e.preventDefault();
      open();
    }
  });
  return { open, close };
}
safe('command palette', () => {
if(hasIds('cmdkOverlay','cmdkInput','cmdkList','openCmdk')){ // demo-only wiring: skipped when the demo markup is absent
function goTo(id){ document.getElementById(id)?.scrollIntoView({ behavior:'smooth', block:'start' }); }
const cmdkCommands = [
  { group:'Actions', label:'Create a project', icon:'i-plus', shortcut:'Ctrl N', run:() => showToast('success','Create project','A new project draft was started.') },
  { group:'Actions', label:'Invite a member', icon:'i-more', shortcut:'', run:() => showToast('info','Invite member','Opening the invite form.') },
  { group:'Actions', label:'Export workspace', icon:'i-download', shortcut:'', run:() => showToast('info','Export','Preparing your export.') },
  { group:'Navigation', label:'Go to Buttons', icon:'i-button', shortcut:'', run:() => goTo('buttons') },
  { group:'Navigation', label:'Go to Data table', icon:'i-table', shortcut:'', run:() => goTo('data-table') },
  { group:'Navigation', label:'Go to Grid input', icon:'i-table', shortcut:'', run:() => goTo('sheet-input') },
  { group:'Navigation', label:'Go to Date picker', icon:'i-calendar', shortcut:'', run:() => goTo('date-picker') },
  { group:'Appearance', label:'Toggle sidebar', icon:'i-panel', shortcut:'Ctrl \\', run:() => sidebarToggle?.click() }
];
const palette = createCommandPalette(document.getElementById('cmdkOverlay'), cmdkCommands);
openCmdk = palette.open;
closeCmdk = palette.close;
document.getElementById('openCmdk').addEventListener('click', palette.open);
}
});

/* Progress bar */
const progressFill = document.getElementById('progressFill');
safe('progress', () => {
if(hasIds('progressFill','progressValue','progressLabel')){ // demo-only wiring

const progressValue = document.getElementById('progressValue');
const progressLabel = document.getElementById('progressLabel');
let progressTimer = null;
setProgress = function(value){
  progressFill.style.width = value + '%';
  progressValue.textContent = value + '%';
  progressFill.parentElement.setAttribute('aria-valuenow', value);
};
document.getElementById('progressStart')?.addEventListener('click', () => {
  if(progressTimer) return;
  let value = 0;
  progressLabel.textContent = 'Uploading files…';
  progressTimer = setInterval(() => {
    value = Math.min(100, value + Math.round(Math.random() * 12) + 4);
    setProgress(value);
    if(value >= 100){
      clearInterval(progressTimer);
      progressTimer = null;
      progressLabel.textContent = 'Upload complete';
      showToast('success', 'Upload complete', 'All files were uploaded.');
    }
  }, 350);
});
document.getElementById('progressReset')?.addEventListener('click', () => {
  clearInterval(progressTimer);
  progressTimer = null;
  progressLabel.textContent = 'Uploading files…';
  setProgress(0);
});
}
});

/* Stepper */
const stepperDemo = document.getElementById('stepperDemo');
safe('stepper', () => {
if(hasIds('stepperDemo','stepStatus','stepPrev','stepNext')){ // demo-only wiring
const stepEls = [...stepperDemo.querySelectorAll('.step')];
const stepStatus = document.getElementById('stepStatus');
let currentStep = 0;
const CHECK = '<svg><use href="#i-check"/></svg>';
function renderStepper(){
  stepEls.forEach((step, i) => {
    const marker = step.querySelector('.step__marker');
    step.classList.toggle('is-done', i < currentStep);
    step.classList.toggle('is-current', i === currentStep);
    if(i === currentStep) step.setAttribute('aria-current','step'); else step.removeAttribute('aria-current');
    marker.innerHTML = i < currentStep ? CHECK : String(i + 1);
    let done = step.querySelector('.step__text .sr-only');
    if(i < currentStep && !done){ done = document.createElement('span'); done.className = 'sr-only'; done.textContent = 'Completed'; step.querySelector('.step__text').appendChild(done); }
    else if(i >= currentStep && done) done.remove();
  });
  stepStatus.textContent = 'Step ' + (currentStep + 1) + ' of ' + stepEls.length;
  const prev = document.getElementById('stepPrev'), next = document.getElementById('stepNext'), active = document.activeElement;
  prev.disabled = currentStep === 0;
  next.disabled = currentStep === stepEls.length - 1;
  if(active === prev && prev.disabled) next.focus();
  else if(active === next && next.disabled) prev.focus();
}
document.getElementById('stepPrev').addEventListener('click', () => { if(currentStep > 0){ currentStep--; renderStepper(); } });
document.getElementById('stepNext').addEventListener('click', () => { if(currentStep < stepEls.length - 1){ currentStep++; renderStepper(); } });
renderStepper();
}
});

/* Accordions */
safe('accordion', () => {
document.querySelectorAll('.accordion').forEach(accordion => {
  const single = accordion.hasAttribute('data-single');
  accordion.querySelectorAll('.accordion__trigger').forEach(trigger => {
    trigger.addEventListener('click', () => {
      const panel = document.getElementById(trigger.getAttribute('aria-controls'));
      const open = trigger.getAttribute('aria-expanded') === 'true';
      if(single && !open){
        accordion.querySelectorAll('.accordion__trigger').forEach(other => {
          other.setAttribute('aria-expanded', 'false');
          document.getElementById(other.getAttribute('aria-controls')).hidden = true;
        });
      }
      trigger.setAttribute('aria-expanded', String(!open));
      panel.hidden = open;
    });
  });
});
});

/* Tabs */
safe('tabs', () => {
document.querySelectorAll('.tab').forEach(tab => {
  tab.addEventListener('click', () => {
    tab.closest('[role=tablist]').querySelectorAll('.tab').forEach(t => { t.setAttribute('aria-selected','false'); t.tabIndex = -1; });
    tab.closest('[role=tablist]').querySelectorAll('.tab').forEach(t => {
      const panel = document.getElementById(t.getAttribute('aria-controls'));
      if(panel) panel.hidden = true;
    });
    tab.setAttribute('aria-selected','true');
    tab.tabIndex = 0;
    document.getElementById(tab.getAttribute('aria-controls')).hidden = false;
  });
  tab.addEventListener('keydown', e => {
    const tabs = [...tab.closest('[role=tablist]').querySelectorAll('.tab')];
    const i = tabs.indexOf(tab);
    const n = e.key === 'ArrowRight' ? (i + 1) % tabs.length
      : e.key === 'ArrowLeft' ? (i - 1 + tabs.length) % tabs.length
      : e.key === 'Home' ? 0 : e.key === 'End' ? tabs.length - 1 : -1;
    if(n < 0) return;
    e.preventDefault();
    tabs[n].click();
    tabs[n].focus();
  });
});
});

/* Tooltips */
safe('tooltips', () => {
document.querySelectorAll('#tooltip-demo .tip').forEach(tip => {
  const btn = tip.querySelector('button');
  btn.addEventListener('keydown', e => { if(e.key === 'Escape'){ e.preventDefault(); tip.classList.add('is-dismissed'); } });
  btn.addEventListener('focus', () => tip.classList.remove('is-dismissed'));
  btn.addEventListener('blur', () => tip.classList.remove('is-dismissed'));
  tip.addEventListener('mouseleave', () => tip.classList.remove('is-dismissed'));
});
});

/* Row selection */
/* Row selection: .row-check boxes in tableEl drive the bulk bar (its <strong> shows the count; its [data-bulk] buttons call onBulk(action, checkedRows)). Call once per table. */
function bindRowSelection(tableEl, bulkbarEl, onBulk){
  const selectAll = tableEl.querySelector('.select-all-check input');
  const count = bulkbarEl.querySelector('strong');
  const boxes = () => [...tableEl.querySelectorAll('.row-check')];
  function update(){
    const all = boxes(), selected = all.filter(c => c.checked).length;
    bulkbarEl.hidden = selected === 0;
    count.textContent = selected + (selected === 1 ? ' row selected' : ' rows selected');
    selectAll.checked = selected === all.length && selected > 0;
    selectAll.indeterminate = selected > 0 && selected < all.length;
  }
  tableEl.addEventListener('change', e => { if(e.target.classList.contains('row-check')) update(); });
  selectAll.addEventListener('change', () => {
    boxes().forEach(c => c.checked = selectAll.checked);
    update();
  });
  bulkbarEl.querySelectorAll('[data-bulk]').forEach(b => b.addEventListener('click', () => {
    onBulk?.(b.dataset.bulk, boxes().filter(c => c.checked).map(c => c.closest('tr')));
    update();
  }));
}
const selectionTable = document.getElementById('selectionTable');
safe('row selection', () => {
if(hasIds('selectionTable','bulkbar')){ // demo-only wiring
bindRowSelection(selectionTable, document.getElementById('bulkbar'), (action, rows) => {
  if(action === 'delete'){
    rows.forEach(row => row.remove());
    showToast('success', 'Rows deleted', rows.length + (rows.length === 1 ? ' row' : ' rows') + ' removed.');
  } else {
    showToast('info', 'Export started', 'Preparing ' + rows.length + (rows.length === 1 ? ' row' : ' rows') + ' for export.');
  }
});
}
});

/* Toasts */
function showToast(type, title, text, options = {}){
  const el = document.createElement('div');
  el.className = 'toast' + (type !== 'success' ? ' toast--' + type : '');
  if(type === 'error') el.setAttribute('role', 'alert');
  const previousFocus = document.activeElement;
  const body = '<div class="toast__body"><strong></strong><span></span>' +
    (options.action ? '<button type="button" class="toast__action"></button>' : '') + '</div>';
  el.innerHTML = '<div class="toast__icon"><svg><use href="#' + toastIcons[type] + '"/></svg></div>' + body +
    (options.dismissible || options.persistent ? '<button type="button" class="toast__close" aria-label="Dismiss"><svg><use href="#i-close"/></svg></button>' : '');
  el.querySelector('strong').textContent = title;
  el.querySelector('span').textContent = text;
  if(options.action) el.querySelector('.toast__action').textContent = options.action;
  let timer = null, started = 0, remaining = 3600, dismissed = false;
  function schedule(){
    if(dismissed || timer || options.persistent || options.action) return;
    if(el.matches(':hover') || el.contains(document.activeElement)) return;
    started = Date.now();
    timer = setTimeout(dismiss, remaining);
  }
  function pause(){
    if(!timer) return;
    clearTimeout(timer); timer = null;
    remaining = Math.max(0, remaining - (Date.now() - started));
  }
  function dismiss(){
    if(dismissed) return;
    dismissed = true;
    clearTimeout(timer); timer = null;
    if(el.contains(document.activeElement)){
      const next = el.nextElementSibling && el.nextElementSibling.querySelector('.toast__close');
      const target = next || (previousFocus && previousFocus !== document.body && document.contains(previousFocus) ? previousFocus : null);
      if(target) target.focus();
    }
    el.classList.add('out');
    setTimeout(() => el.remove(), 260);
  }
  el.addEventListener('mouseenter', pause);
  el.addEventListener('focusin', pause);
  el.addEventListener('mouseleave', schedule);
  el.addEventListener('focusout', e => { if(!el.contains(e.relatedTarget)) schedule(); });
  const closeBtn = el.querySelector('.toast__close');
  if(closeBtn) closeBtn.addEventListener('click', dismiss);
  const actionBtn = el.querySelector('.toast__action');
  if(actionBtn) actionBtn.addEventListener('click', () => { showToast('success', 'Done', 'The action was completed.'); dismiss(); });
  document.getElementById('toastStack').appendChild(el);
  schedule();
  return el;
}
safe('toast buttons', () => {
document.querySelectorAll('[data-toast]').forEach(b => {
  b.addEventListener('click', () => {
    const t = b.dataset.toast;
    const msg = {
      success:['Saved','Your changes have been saved.'],
      info:['Heads up','A new version is available.'],
      error:['Upload failed','The file could not be uploaded.']
    }[t];
    showToast(t, msg[0], msg[1]);
  });
});
});

safe('toast demo', () => {
if(hasIds('toastDismissible','toastWithAction','toastPersistent')){ // demo-only wiring
document.getElementById('toastDismissible').addEventListener('click', () =>
  showToast('info', 'New update available', 'Restart to apply the latest changes.', { dismissible: true }));
document.getElementById('toastWithAction').addEventListener('click', () =>
  showToast('success', 'Message sent', 'Delivered to the team.', { action: 'Undo', dismissible: true }));
document.getElementById('toastPersistent').addEventListener('click', () =>
  showToast('error', 'Connection lost', 'We could not reach the server. Check your network.', { persistent: true, dismissible: true }));
}
});

/* Loading button demo */
const loadingBtn = document.getElementById('loadingBtn');
safe('loading button', () => {
if(loadingBtn){ // demo-only wiring
loadingBtn.addEventListener('click', () => {
  if(loadingBtn.getAttribute('aria-disabled') === 'true') return;
  loadingBtn.setAttribute('aria-disabled', 'true');
  const original = loadingBtn.innerHTML;
  loadingBtn.innerHTML = '<span class="spinner"></span>Working…';
  setTimeout(() => { loadingBtn.innerHTML = original; loadingBtn.removeAttribute('aria-disabled'); }, 1600);
});
}
});

/* Active nav highlight on scroll */
const sections = [...document.querySelectorAll('.section[id]')];
const navLinks = [...document.querySelectorAll('#nav a')];
safe('active nav', () => {
const observer = new IntersectionObserver(entries => {
  entries.forEach(entry => {
    if(entry.isIntersecting){
      navLinks.forEach(a => a.classList.toggle('active', a.getAttribute('href') === '#' + entry.target.id));
    }
  });
}, { rootMargin: '-15% 0px -75% 0px' });
sections.forEach(s => observer.observe(s));
});

/* Collapsible sidebar */
const app = document.querySelector('.app');
const sidebar = document.querySelector('.sidebar');
const sidebarToggle = document.getElementById('sidebarToggle');
const brandToggle = document.getElementById('brandToggle');
safe('sidebar nav links', () => {
navLinks.forEach(a => {
  const label = a.textContent.trim();
  if(!a.dataset.tip) a.dataset.tip = label;
  a.setAttribute('aria-label', label);
});
});
function setCollapsed(collapsed){
  app.classList.toggle('app--collapsed', collapsed);
  sidebar.classList.toggle('sidebar--collapsed', collapsed);
  const label = collapsed ? 'Expand sidebar' : 'Collapse sidebar';
  sidebarToggle.setAttribute('aria-expanded', String(!collapsed));
  sidebarToggle.setAttribute('aria-label', label);
  sidebarToggle.title = label;
  brandToggle.setAttribute('aria-label', label);
}
safe('sidebar toggle', () => {
if(sidebarToggle) sidebarToggle.addEventListener('click', () => setCollapsed(!app.classList.contains('app--collapsed')));
});
safe('brand toggle', () => {
if(brandToggle) brandToggle.addEventListener('click', () => setCollapsed(!app.classList.contains('app--collapsed')));
});
})();
