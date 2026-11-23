# Web snippets

usage = link `dist/ui.css` in `<head>` (path relative to your page), `dist/ui.js` at END of `<body>`, `dist/theme.js` as a plain `<script src>` in `<head>` (blocking, before content) only if the page offers a manual theme toggle; without it the page follows the OS theme, never copy CSS/JS, never read index.html whole (grep -n, then read ranges), ids below are neutral and not wired by ui.js (except `#toastStack`, which ui.js requires). Never reuse index.html ids (sheet, overlay, filterTable, ...): ui.js wires those by name.
icons: ui.js injects the sprite; use <svg><use href="#i-name"/></svg>; never inline <symbol>s
icon ids: spark grid button form card modal badge alert more check close search arrow info table filter panel chevron pin calendar clock upload file edit copy download trash plus
Markup is copied from index.html: whitespace condensed, repeated rows and options trimmed, showcase ids renamed to neutral ones; otherwise unchanged. Class hooks that ui.js requires are never trimmed. Section names match the Quick picker in COMPONENTS.md.

### Button
when: Trigger a single action
```html
<button class="btn btn--primary">Primary action</button>
<button class="btn btn--secondary">Secondary</button>
<button class="btn btn--primary btn--sm">Small</button>
<button class="btn btn--primary" disabled>Disabled</button>
```
init: static, no JS.

### Action menu
when: Choose from 3+ actions on one object
```html
<div class="menu">
  <button type="button" class="btn btn--secondary menu__trigger" aria-haspopup="menu" aria-expanded="false" aria-controls="projectMenu">Actions <svg><use href="#i-chevron"/></svg></button>
  <div class="menu__panel" id="projectMenu" role="menu" hidden>
    <button type="button" class="menu__item" role="menuitem" data-action="Edit project"><svg><use href="#i-edit"/></svg>Edit project<span class="menu__shortcut">Ctrl E</span></button>
    <button type="button" class="menu__item" role="menuitem" data-action="Export as CSV"><svg><use href="#i-download"/></svg>Export as CSV</button>
    <div class="menu__divider" role="separator"></div>
    <button type="button" class="menu__item menu__item--danger" role="menuitem" data-action="Delete project"><svg><use href="#i-trash"/></svg>Delete project</button>
  </div>
</div>
```
init: auto-initialised by ui.js at load (every `.menu`).

### Native select
when: Pick one option from a short, known list
```html
<label class="field__label" for="f-role">Role</label>
<select class="select" id="f-role">
  <option>Designer</option><option>Engineer</option><option>Product manager</option>
</select>
```
init: static, no JS.

### Searchable select
when: Pick one option from a long list, with typing
```html
<label class="field__label" id="assigneeLabel">Assignee</label>
<div class="combo">
  <button type="button" class="combo__trigger" role="combobox" aria-expanded="false" aria-controls="assigneeList" aria-labelledby="assigneeLabel" aria-haspopup="listbox">
    <span class="combo__value is-placeholder">Select a person…</span><svg><use href="#i-chevron"/></svg>
  </button>
  <div class="combo__panel" hidden>
    <div class="combo__search"><svg><use href="#i-search"/></svg><input class="combo__input" type="text" placeholder="Search people…" aria-label="Search people" autocomplete="off" role="combobox" aria-controls="assigneeList" aria-expanded="false" aria-autocomplete="list"/></div>
    <div class="combo__list" id="assigneeList" role="listbox" aria-labelledby="assigneeLabel">
      <div class="combo__option" role="option" aria-selected="false"><svg class="combo__check"><use href="#i-check"/></svg><span class="combo__option-label">Ada Lovelace</span><span class="combo__option-meta">Design</span></div>
      <div class="combo__option" role="option" aria-selected="false"><svg class="combo__check"><use href="#i-check"/></svg><span class="combo__option-label">Jonas Silva</span><span class="combo__option-meta">Engineering</span></div>
      <div class="combo__empty" hidden>No matches</div>
    </div>
  </div>
</div>
```
init: auto-initialised by ui.js at load (every `.combo`). Not re-initialised if added later.

### Multi-select
when: Pick several values from a known list
```html
<label class="field__label" id="skillsLabel">Skills</label>
<div class="multi" data-placeholder="Select skills…">
  <button type="button" class="multi__trigger" aria-expanded="false" aria-haspopup="listbox" aria-labelledby="skillsLabel">
    <span class="multi__chips"></span><svg><use href="#i-chevron"/></svg>
  </button>
  <div class="multi__panel" hidden>
    <div class="multi__search"><svg><use href="#i-search"/></svg><input class="multi__input" type="text" placeholder="Search skills…" aria-label="Search skills" autocomplete="off"/></div>
    <div class="multi__list">
      <label class="check multi__option"><input type="checkbox"/><span class="check__box"><svg><use href="#i-check"/></svg></span><span class="multi__option-label">React</span><span class="multi__option-meta">Frontend</span></label>
    <div class="multi__empty" hidden>No matches</div>
    </div>
    <div class="multi__foot">
      <div class="multi__foot-left"> <button type="button" class="btn btn--ghost btn--sm multi__select-all">Select all</button> <button type="button" class="btn btn--ghost btn--sm multi__clear">Clear all</button> </div>
      <div class="multi__foot-right"> <span class="multi__count">0 selected</span> <button type="button" class="btn btn--primary btn--sm multi__done">Done</button> </div>
    </div>
  </div>
</div>
```
init: auto-initialised by ui.js at load (every `.multi`). Add more options by copying the `multi__option` label line.

### Tag input
when: Enter freeform values not in a fixed list
```html
<div class="field tag-input-wrap" data-tags="design,research" data-max="6">
  <label class="field__label">Tags</label>
  <div class="tag-input"><input class="tag-input__field" placeholder="Add a tag…" aria-label="Tags"/></div>
  <span class="tag-input__hint"></span>
</div>
```
init: auto-initialised by ui.js at load (every `.tag-input-wrap`). `data-tags` = preset chips, `data-max` = limit.

### Autocomplete
when: Type and get filtered suggestions
```html
<div class="field">
  <label class="field__label">Framework</label>
  <div class="ac" data-options="React,Vue,Angular,Svelte,Solid,Next.js,Nuxt">
    <div class="input-icon"><svg><use href="#i-search"/></svg><input class="input ac__input" placeholder="Search or type your own…" aria-label="Framework"/></div>
    <div class="ac__list" hidden></div>
  </div>
  <span class="field__hint">Pick a suggestion or keep your own text.</span>
</div>
```
init: auto-initialised by ui.js at load (every `.ac`). `data-options` = comma-separated suggestions.

### Date picker
when: Pick a date / month / year / time
```html
<div class="picker" data-mode="date" data-time="false">
  <button type="button" class="picker__trigger" aria-expanded="false" aria-haspopup="dialog"><svg><use href="#i-calendar"/></svg><span class="picker__value is-placeholder">Pick a date…</span></button>
</div>
```
init: auto-initialised by ui.js at load (every `.picker`). `data-mode` = date | month | year; `data-time="true"` adds hour/minute; `data-seconds="true"` adds seconds.

### File input
when: Upload files
```html
<div class="file-input" data-multiple="false">
  <label class="file-drop">
    <input class="sr-only" type="file"/>
    <svg><use href="#i-upload"/></svg>
    <strong>Drop a file here</strong>
    <span>or click to browse</span>
  </label>
  <div class="file-list"></div>
  <div class="file-count" hidden></div>
</div>
```
init: auto-initialised by ui.js at load (every `.file-input`). `data-multiple="true"` plus `multiple` on the input for several files.

### Grid input / Typed grid
when: Edit many cells as a grid
```html
<div class="sheet" id="productGrid">
  <div class="sheet__toolbar">
    <div class="sheet__actions">
      <button type="button" class="btn btn--ghost btn--sm" data-sheet-copy>Copy selected</button>
      <button type="button" class="btn btn--ghost btn--sm" data-sheet-clear>Clear selected</button>
    </div>
    <span class="sheet__info" data-sheet-info>0 cells selected</span>
  </div>
  <div class="sheet__scroll"><table class="sheet__table"><thead></thead><tbody></tbody></table></div>
</div>
```
init: NOT auto. After the markup is in the DOM, call `createGrid('productGrid', [{ name: 'Width', type: 'text' }, { name: 'Category', type: 'select', options: ['Standard', 'Pro'] }, { name: 'IsActive', type: 'checkbox' }], [['Small', 'Standard', 'true']])`. Typed grid = same call with the same column types (the showcase's `typedSheet`, found by grep). Signature: `createGrid(rootId, columns, rows, options?)`, rootId is the element id as a string. Optional 4th arg `{ allowAddColumn: false }`.

### Badge
when: Show a short status
```html
<span class="badge badge--neutral">Draft</span>
<span class="badge badge--brand">In review</span>
<span class="badge badge--success">Active</span>
<span class="badge badge--warning">Pending</span>
<span class="badge badge--danger">Overdue</span>
```
init: static, no JS.

### Alert
when: Show inline feedback in a form
```html
<div class="alert alert--success"><svg><use href="#i-check"/></svg><div><div class="alert__title">Saved</div><div class="alert__text">Your settings have been updated.</div></div></div>
<div class="alert alert--danger"><svg><use href="#i-close"/></svg><div><div class="alert__title">Something went wrong</div><div class="alert__text">We couldn't complete that request. Try again.</div></div></div>
```
init: static, no JS. Variants: `alert--info`, `alert--success`, `alert--warning`, `alert--danger`.

### Toast
when: Show transient feedback after an action
```html
<div class="toast-stack" id="toastStack" aria-live="polite"></div>
<button class="btn btn--secondary" data-toast="success">Success toast</button>
<button class="btn btn--secondary" data-toast="error">Error toast</button>
```
init: `[data-toast]` buttons are auto-wired at load. For any other toast call the factory `showToast('success', 'Saved', 'Your changes have been saved.')` (types: success | info | error; options `{ dismissible: true }`, `{ action: 'Undo' }`, `{ persistent: true }`). Needs `#toastStack` in the DOM.

### Modal / drawer confirmation
when: Confirm a destructive action
```html
<button class="btn btn--primary" id="openConfirm">Open dialog</button>
<div class="overlay" id="confirmDialog" role="dialog" aria-modal="true" aria-labelledby="modalTitle">
  <div class="modal">
    <div class="modal__head">
      <h3 class="modal__title" id="modalTitle">Delete this project?</h3>
      <button class="icon-btn" id="closeConfirm" aria-label="Close dialog"><svg><use href="#i-close"/></svg></button>
    </div>
    <p class="modal__body">This will permanently remove the project and all of its contents. This action can't be undone.</p>
    <div class="modal__foot">
      <button class="btn btn--secondary" id="cancelConfirm">Cancel</button>
      <button class="btn btn--danger" id="confirmAction">Delete project</button>
    </div>
  </div>
</div>
```
init: NOT auto. Open with `openOverlay(el, focusEl)`, close with `closeOverlay(el)` (global). Backdrop click and Escape close every `.overlay` automatically. Wire the buttons yourself, e.g. `document.getElementById('openConfirm').addEventListener('click', () => openOverlay(document.getElementById('confirmDialog'), document.getElementById('cancelConfirm')))`.

### Loading modal
when: Block the UI while work finishes
```html
<button class="btn btn--primary" id="startLoading">Start loading</button>
<div class="overlay overlay--blocking" id="loadingDialog" role="dialog" aria-modal="true" aria-busy="true" aria-labelledby="loadingTitle" aria-describedby="loadingText">
  <div class="modal loading-modal" tabindex="-1">
    <div class="loading-modal__spinner" aria-hidden="true"></div>
    <h3 class="loading-modal__title" id="loadingTitle">Preparing your workspace</h3>
    <p class="loading-modal__text" id="loadingText">Please wait a moment while we set things up for you.</p>
  </div>
</div>
```
init: NOT auto. Show with `openOverlay(el, el.querySelector('.loading-modal'))`, hide with `closeOverlay(el)` when the work ends. `overlay--blocking` ignores backdrop and Escape.

### Progress bar
when: Show known-duration work
```html
<div class="progress__meta"><span id="uploadLabel">Uploading files…</span><span class="progress__value" id="uploadValue">0%</span></div>
<div class="progress" role="progressbar" aria-valuemin="0" aria-valuemax="100" aria-valuenow="0" aria-labelledby="uploadLabel"><div class="progress__fill" id="uploadFill"></div></div>
```
init: NOT auto (the demo's upload simulation needs the start/reset buttons, so the snippet has none). Set `style="width:NN%"` on `.progress__fill`, `aria-valuenow` on `.progress`, and the value text yourself.

### Skeleton
when: Hold layout while data loads
```html
<div class="sk-card">
  <div class="sk-row"><div class="skeleton sk-circle"></div><div style="flex:1"><div class="skeleton sk-line" style="width:55%"></div><div class="skeleton sk-line" style="width:35%; margin-top:8px"></div></div></div>
  <div class="skeleton sk-line" style="width:100%; margin-top:22px"></div>
  <div class="skeleton sk-line" style="width:82%; margin-top:9px"></div>
</div>
```
init: static, no JS. Remove the skeleton when data arrives.

### Empty state
when: Show "nothing here yet"
```html
<div class="card">
  <div class="empty-state">
    <div class="empty-state__head"><div class="empty-state__icon"><svg><use href="#i-card"/></svg></div><h3 class="empty-state__title">No projects yet</h3></div>
    <p class="empty-state__text">Projects you create will appear here. Start with a blank project to see how it works.</p>
    <div class="empty-state__actions"><button class="btn btn--primary">New project</button><button class="btn btn--secondary">View guide</button></div>
  </div>
</div>
```
init: static, no JS.

### Modal form
when: Ask for input without leaving the page
```html
<button class="btn btn--primary" id="openNewProject">Create project</button>
<div class="overlay" id="newProjectDialog" role="dialog" aria-modal="true" aria-labelledby="formModalTitle">
  <div class="modal modal--form">
    <div class="modal__head"> <h3 class="modal__title" id="formModalTitle">Create a project</h3> <button class="icon-btn" id="closeNewProject" aria-label="Close dialog"><svg><use href="#i-close"/></svg></button> </div>
    <form id="newProjectForm" novalidate>
      <div class="field">
        <label class="field__label" for="newProjectName">Project name</label>
        <input class="input" id="newProjectName" placeholder="Website refresh"/>
        <span class="field__error" id="newProjectName-error" role="alert"><svg><use href="#i-alert"/></svg>Give the project a name.</span>
      </div>
      <div class="field">
        <label class="field__label" for="newProjectEmail">Owner email</label>
        <input class="input" id="newProjectEmail" type="email" placeholder="you@example.com"/>
        <span class="field__error" id="newProjectEmail-error" role="alert"><svg><use href="#i-alert"/></svg>Enter a valid email address.</span>
      </div>
      <div class="modal__foot">
        <button type="button" class="btn btn--secondary" id="cancelNewProject">Cancel</button>
        <button type="submit" class="btn btn--primary">Create project</button>
      </div>
    </form>
  </div>
</div>
```
init: NOT auto. Open with `openOverlay(el, focusEl)`, close with `closeOverlay(el)`, mark errors with global `setFieldError(fieldEl, true|false)`. Wire the buttons and submit yourself, e.g. `document.getElementById('openNewProject').addEventListener('click', () => openOverlay(document.getElementById('newProjectDialog'), document.getElementById('newProjectName')))`.

### Drawer form
when: Longer form, side panel
```html
<button class="btn btn--secondary" id="openInvite">Invite a member</button>
<div class="overlay drawer-overlay" id="inviteDrawer" role="dialog" aria-modal="true" aria-labelledby="drawerTitle">
  <div class="drawer">
    <div class="drawer__head"> <h3 class="modal__title" id="drawerTitle">Invite a member</h3> <button class="icon-btn" id="closeInvite" aria-label="Close panel"><svg><use href="#i-close"/></svg></button> </div>
    <form class="drawer__form" id="inviteMemberForm" novalidate>
      <div class="drawer__body">
        <div class="field">
          <label class="field__label" for="inviteEmail">Email address</label>
          <input class="input" id="inviteEmail" type="email" placeholder="teammate@example.com"/>
          <span class="field__error" id="inviteEmail-error" role="alert"><svg><use href="#i-alert"/></svg>Enter a valid email address.</span>
        </div>
      </div>
      <div class="drawer__foot">
        <button type="button" class="btn btn--secondary" id="cancelInvite">Cancel</button>
        <button type="submit" class="btn btn--primary">Send invite</button>
      </div>
    </form>
  </div>
</div>
```
init: NOT auto. Open with `openOverlay(el, focusEl)`, close with `closeOverlay(el)`, mark errors with `setFieldError`. Wire the buttons and submit yourself, e.g. `document.getElementById('openInvite').addEventListener('click', () => openOverlay(document.getElementById('inviteDrawer'), document.getElementById('inviteEmail')))`.

### Breadcrumbs
when: Navigate a site path
```html
<nav class="crumbs" aria-label="Breadcrumb">
  <a href="#buttons">Workspace</a><svg class="crumbs__sep" aria-hidden="true"><use href="#i-chevron"/></svg>
  <a href="#cards">Projects</a><svg class="crumbs__sep" aria-hidden="true"><use href="#i-chevron"/></svg>
  <span class="crumbs__current" aria-current="page">Settings</span>
</nav>
```
init: static, no JS. Replace the hrefs.

### Tabs
when: Switch views on the same page
```html
<div class="tabs" role="tablist" aria-label="Account sections">
  <button class="tab" role="tab" aria-selected="true" aria-controls="p-1" id="t-1" tabindex="0">Profile</button>
  <button class="tab" role="tab" aria-selected="false" aria-controls="p-2" id="t-2" tabindex="-1">Security</button>
</div>
<div class="tabpanel" role="tabpanel" id="p-1" aria-labelledby="t-1" tabindex="0">Update your name, photo and public profile details.</div>
<div class="tabpanel" role="tabpanel" id="p-2" aria-labelledby="t-2" tabindex="0" hidden>Manage your password and two-factor authentication.</div>
```
init: auto-initialised by ui.js at load (every `.tab`, keyboard included). Ids `t-N` and `p-N` must pair up.

### Accordion
when: Reveal extra detail in place
```html
<div class="accordion">
  <div class="accordion__item">
    <button type="button" class="accordion__trigger" aria-expanded="true" aria-controls="acc-1">Project overview <svg><use href="#i-chevron"/></svg></button>
    <div class="accordion__panel" id="acc-1">Track scope, timeline and ownership for this project in one place.</div>
  </div>
  <div class="accordion__item">
    <button type="button" class="accordion__trigger" aria-expanded="false" aria-controls="acc-2">Team members <svg><use href="#i-chevron"/></svg></button>
    <div class="accordion__panel" id="acc-2" hidden>Invite people, assign roles and manage their access to the workspace.</div>
  </div>
</div>
```
init: auto-initialised by ui.js at load (every `.accordion`). Add `data-single` to the `.accordion` for one-open-at-a-time.

### Stepper
when: Show a wizard's position
```html
<div class="stepper" id="wizard">
  <div class="step is-current"><span class="step__line"></span><span class="step__marker">1</span><div class="step__text"><div class="step__title">Account</div><div class="step__desc">Your details</div></div></div>
  <div class="step"><span class="step__line"></span><span class="step__marker">2</span><div class="step__text"><div class="step__title">Workspace</div><div class="step__desc">Set it up</div></div></div>
  <div class="step"><span class="step__line"></span><span class="step__marker">3</span><div class="step__text"><div class="step__title">Review</div><div class="step__desc">Confirm</div></div></div>
  <div class="step"><span class="step__marker">4</span><div class="step__text"><div class="step__title">Done</div><div class="step__desc">All set</div></div></div>
</div>
<div class="row">
  <button class="btn btn--secondary btn--sm" id="wizardPrev">Previous</button>
  <button class="btn btn--primary btn--sm" id="wizardNext">Next step</button>
  <span class="field__hint" id="wizardStatus" role="status">Step 1 of 4</span>
</div>
```
init: static. ui.js wires Previous/Next only to the showcase ids, so the buttons do nothing on your page. Steps are 1-based by position. Move `is-current` / `is-done` and `aria-current="step"` yourself, and update `#wizardStatus`.

### Data table
when: Show tabular data
```html
<div class="table-wrap">
    <table class="table">
      <thead><tr><th scope="col">Project</th><th scope="col">Owner</th><th scope="col">Status</th><th scope="col">Updated</th><th scope="col" class="table__num">Budget</th><th scope="col"></th></tr></thead>
      <tbody>
        <tr>
          <th scope="row"><span class="table__name"><span class="avatar avatar--sm">AL</span><span>Website refresh<span class="table__sub">Design team</span></span></span></th>
          <td>Ada Lovelace</td><td><span class="badge badge--success">Active</span></td><td>Today</td><td class="table__num">$12,400</td><td><button class="table__action">Open</button></td>
        </tr>
      </tbody>
    </table>
  </div>
  <div class="pagination">
    <label class="pagination__limit">Show <select class="select select--sm" data-page-limit aria-label="Rows per page"><option value="10">10</option><option value="25" selected>25</option><option value="50">50</option><option value="100">100</option></select> per page</label>
    <span class="pagination__info" data-page-info></span>
    <div class="pagination__controls"> <button type="button" class="btn btn--secondary btn--sm" data-page-prev>Previous</button> <div class="pagination__pages" data-page-list></div> <button type="button" class="btn btn--secondary btn--sm" data-page-next>Next</button> </div>
  </div>
```
init: auto-initialised by ui.js at load for every `.table-wrap` whose parent element also holds a pagination block (`[data-page-limit]`). Put one table-wrap and its pagination block in the same parent element (one parent per table). No `.demo` wrapper is needed (`.demo` is the showcase card style). Without the pagination block nothing happens.

### Table with filters
when: Narrow rows by search/status
```html
<div class="filters">
  <div class="field filters__search">
    <div class="input-icon">
      <svg><use href="#i-search"/></svg>
      <input class="input" id="projectSearch" type="search" placeholder="Search projects…" aria-label="Search projects"/>
    </div>
  </div>
  <div class="segment" id="projectFilters" role="group" aria-label="Filter by status">
    <button type="button" class="active" data-status="all">All</button>
    <button type="button" data-status="active">Active</button>
  </div>
  <span class="filter-count" id="projectCount">6 of 6 projects</span>
</div>
<table class="table" id="projectTable">
  <thead><tr><th>Project</th><th>Owner</th><th>Status</th><th>Updated</th></tr></thead>
  <tbody>
    <tr data-status="active"><td><span class="table__name">Website refresh<span class="table__sub">Design team</span></span></td><td>Ada Lovelace</td><td><span class="badge badge--success">Active</span></td><td>Today</td></tr>
  </tbody>
</table>
```
init: NOT auto. Call `bindSimpleFilters('projectTable', 'projectSearch', 'projectFilters', 'projectCount')` (global, after load). Rows and filter buttons need `data-status`. Pagination is optional: if the table sits in a `.table-wrap` whose parent also holds a Data table pagination block, filtering also resets its page.

### Column-filters table
when: Filter each column independently
```html
<div class="table-wrap">
  <table class="table" id="budgetTable">
    <thead> <tr><th scope="col">Project</th><th scope="col">Owner</th><th scope="col">Status</th><th scope="col">Updated</th><th scope="col" class="table__num">Budget</th><th scope="col"></th></tr>
      <tr>
        <th scope="col" class="table__filter"><input class="input input--sm" id="colProject" type="search" placeholder="Filter projects…" aria-label="Filter by project"/></th>
        <th scope="col" class="table__filter"><input class="input input--sm" id="colOwner" type="search" placeholder="Filter owners…" aria-label="Filter by owner"/></th>
        <th scope="col" class="table__filter"> <select class="select select--sm" id="colStatus" aria-label="Filter by status">
          <option value="">All statuses</option><option value="active">Active</option><option value="pending">Pending</option><option value="archived">Archived</option> </select> </th>
        <th scope="col" class="table__filter"><input class="input input--sm" id="colUpdated" type="search" placeholder="Any date" aria-label="Filter by updated date"/></th>
        <th scope="col" class="table__filter"><input class="input input--sm" id="colBudget" type="number" min="0" placeholder="Min amount" aria-label="Filter by minimum budget"/></th>
      </tr>
    </thead>
    <tbody>
      <tr data-status="active" data-budget="12400"><th scope="row">Website refresh<span class="table__sub">Design team</span></th><td>Ada Lovelace</td><td><span class="badge badge--success">Active</span></td><td>Today</td><td class="table__num">$12,400</td><td><button class="table__action">Open</button></td></tr>
    </tbody>
  </table>
</div>
```
init: NOT auto. Call `bindColumnFilters('budgetTable')` (global, after load; optional 2nd argument is a count element id). Filters are `thead input, thead select`: keep one filter cell per column. Pagination is optional, as in Table with filters (it needs the table in a `.table-wrap` with a pagination block in the same parent).

### Row selection table
when: Select several rows and act on them
```html
<div class="bulkbar" id="myBulkbar" hidden>
  <strong>0 selected</strong>
  <div class="bulkbar__actions">
    <button type="button" class="btn btn--secondary btn--sm" data-bulk="export">Export</button>
    <button type="button" class="btn btn--danger btn--sm" data-bulk="delete">Delete</button>
  </div>
</div>
<div class="table-wrap">
  <table class="table" id="mySelectTable">
    <thead><tr>
      <th class="select-all-check"><label class="check"><input type="checkbox" aria-label="Select all rows"/><span class="check__box"><svg><use href="#i-check"/></svg></span></label></th>
      <th>Project</th><th>Owner</th><th>Status</th>
    </tr></thead>
    <tbody>
      <tr><td><label class="check"><input type="checkbox" class="row-check" aria-label="Select row"/><span class="check__box"><svg><use href="#i-check"/></svg></span></label></td><td>Website refresh</td><td>Ada Lovelace</td><td><span class="badge badge--success">Active</span></td></tr>
      <tr><td><label class="check"><input type="checkbox" class="row-check" aria-label="Select row"/><span class="check__box"><svg><use href="#i-check"/></svg></span></label></td><td>Mobile app</td><td>Jonas Silva</td><td><span class="badge badge--success">Active</span></td></tr>
    </tbody>
  </table>
</div>
```
init: NOT auto. Call once after load: `bindRowSelection(document.getElementById('mySelectTable'), document.getElementById('myBulkbar'), (action, rows) => { ... })`. The header checkbox (`.select-all-check input`) and each `.row-check` drive the bulk bar; its `<strong>` shows the count. The callback gets the `data-bulk` value and the checked `<tr>` elements; the function does not remove rows, so do that in the callback (the showcase deletes them there).

### Sortable table
when: Click-to-sort headers, optional pinned column
```html
<div class="table-wrap">
  <table class="table table--wide" id="mySortTable">
    <thead>
      <tr>
        <th scope="col"><span class="th-inner"><button type="button" class="pin-btn" aria-pressed="false" aria-label="Pin project column" title="Pin project column"><svg><use href="#i-pin"/></svg></button><button type="button" class="sort-btn" data-sort-key="project">Project <svg class="sort-icon"><use href="#i-chevron"/></svg></button></span></th>
        <th scope="col"><button type="button" class="sort-btn" data-sort-key="owner">Owner <svg class="sort-icon"><use href="#i-chevron"/></svg></button></th>
        <th scope="col" class="table__num"><button type="button" class="sort-btn" data-sort-key="budget">Budget <svg class="sort-icon"><use href="#i-chevron"/></svg></button></th>
      </tr>
    </thead>
    <tbody>
      <tr><th scope="row">Mobile app</th><td>Jonas Silva</td><td class="table__num" data-sort-value="28900">$28,900</td></tr>
      <tr><th scope="row">Website refresh</th><td>Ada Lovelace</td><td class="table__num" data-sort-value="12400">$12,400</td></tr>
    </tbody>
  </table>
</div>
```
init: NOT auto. Call once after load: `bindSortable(document.getElementById('mySortTable'))`. Header buttons with `data-sort-key` sort the rows (click again to reverse); `data-sort-value` on a cell sorts by that value instead of its text. A `.pin-btn` in the header toggles `.sticky-column` on the table.

### Command palette
when: Run a command by search
```html
<button type="button" class="btn btn--secondary" id="openPalette">Open command palette <kbd class="cmdk__esc">Ctrl K</kbd></button>
<div class="overlay cmdk-overlay" id="myPalette" role="dialog" aria-modal="true" aria-label="Command palette">
  <div class="cmdk">
    <div class="cmdk__search">
      <svg><use href="#i-search"/></svg>
      <input class="cmdk__input" type="text" placeholder="Search for an action…" aria-label="Search commands" autocomplete="off" role="combobox" aria-controls="myPaletteList" aria-expanded="false"/>
      <span class="cmdk__esc">ESC</span>
    </div>
    <div class="cmdk__list" id="myPaletteList" role="listbox" aria-label="Commands"></div>
    <div class="cmdk__foot"><span><b>↑↓</b> to navigate</span><span><b>↵</b> to select</span><span><b>esc</b> to close</span></div>
  </div>
</div>
```
init: NOT auto. Call once after load (one palette per page): `const palette = createCommandPalette(document.getElementById('myPalette'), [{ group: 'Actions', label: 'Export workspace', icon: 'i-download', shortcut: '', run: () => exportWorkspace() }]);`. Ctrl/Cmd+K opens it (ignored while another overlay is open); `icon` is an id from the icon list above. Optional: `document.getElementById('openPalette').addEventListener('click', palette.open)`.

### Theme toggle
```html
<div class="segment" data-theme-toggle role="group" aria-label="Colour theme"><button type="button" data-theme-value="system">System</button><button type="button" data-theme-value="light">Light</button><button type="button" data-theme-value="dark">Dark</button></div>
```
init: auto (ui.js). Programmatic: `setTheme('light'|'dark'|'system')`. Requires dist/theme.js in <head> to avoid a flash on reload.
