# Regenerates every web-overlays-*, web-pickers-* and web-file-input* reference (07-overlays-pickers.md).
# Run: pwsh -NoProfile -File plan/wpf/ref/web-overlays-capture.ps1   (picker refs depend on today's date: re-run on the day you compare)
# The cmdk *-scrollbar.png refs (real 11px scroll bar) come from web-overlays-capture-scrollbar.ps1 (no --hide-scrollbars).
$root = 'C:\Workspace\Projects\others\ui-design'
Set-Location $root
$cap = "$root\plan\wpf\capture-web.ps1"
$R = 'plan/wpf/ref'
$na = "const st=document.createElement('style');st.textContent='*,*::before,*::after{transition:none!important;animation:none!important}';document.head.appendChild(st);"
$kd = "const kd=(el,k)=>el.dispatchEvent(new KeyboardEvent('keydown',{key:k,bubbles:true,cancelable:true}));const ty=(el,v)=>{el.value=v;el.dispatchEvent(new Event('input',{bubbles:true}));};const q=s=>document.querySelector(s);"
$cell = "const cell=(p,k,v)=>[...p.querySelectorAll('[data-'+k+']')].find(b=>b.dataset[k]===String(v));";
$file = "const addF=(inp,list)=>{const dt=new DataTransfer();list.forEach(f=>dt.items.add(new File([new Uint8Array(f[1])],f[0],{type:f[2]})));inp.files=dt.files;inp.dispatchEvent(new Event('change'));};"
$shots = @(
  @('modal','web-overlays-modal.png',''),
  @('modal','web-overlays-modal-open.png',"q('#openModal').click();"),
  @('form-modal','web-overlays-form-modal.png',''),
  @('form-modal','web-overlays-form-modal-open.png',"q('#openFormModal').click();"),
  @('form-modal','web-overlays-form-modal-errors.png',"q('#openFormModal').click();q('#projectForm button[type=submit]').click();"),
  @('drawer','web-overlays-drawer.png',''),
  @('drawer','web-overlays-drawer-invite.png',"q('#openDrawer').click();"),
  @('drawer','web-overlays-drawer-invite-errors.png',"q('#openDrawer').click();q('#inviteForm button[type=submit]').click();"),
  @('drawer','web-overlays-drawer-edit.png',"q('#openEditDrawer').click();"),
  @('drawer','web-overlays-drawer-edit-confirm.png',"q('#openEditDrawer').click();q('#editDelete').click();"),
  @('loading','web-overlays-loading.png',''),
  @('loading','web-overlays-loading-open.png',"q('#openLoading').click();"),
  @('command-palette','web-overlays-cmdk.png',''),
  @('command-palette','web-overlays-cmdk-open.png',"q('#openCmdk').click();"),
  @('command-palette','web-overlays-cmdk-down2.png',"q('#openCmdk').click();kd(q('#cmdkInput'),'ArrowDown');kd(q('#cmdkInput'),'ArrowDown');"),
  @('command-palette','web-overlays-cmdk-filter.png',"q('#openCmdk').click();ty(q('#cmdkInput'),'go');"),
  @('command-palette','web-overlays-cmdk-empty.png',"q('#openCmdk').click();ty(q('#cmdkInput'),'zzz');"),
  @('toasts','web-overlays-toasts.png',''),
  @('toasts','web-overlays-toasts-shown.png',"document.querySelectorAll('[data-toast]').forEach(b=>b.click());"),
  @('toast-variants','web-overlays-toast-variants.png',''),
  @('toast-variants','web-overlays-toast-variants-shown.png',"q('#toastDismissible').click();q('#toastWithAction').click();q('#toastPersistent').click();"),
  @('date-picker','web-pickers-date.png',''),
  @('date-picker','web-pickers-date-open.png',"document.querySelectorAll('#date-picker .picker__trigger')[0].click();"),
  @('date-picker','web-pickers-date-selected.png',"const t=document.querySelectorAll('#date-picker .picker__trigger');t[0].click();cell(document,'day',14).click();t[0].click();"),
  @('date-picker','web-pickers-month-open.png',"document.querySelectorAll('#date-picker .picker__trigger')[1].click();"),
  @('date-picker','web-pickers-year-open.png',"document.querySelectorAll('#date-picker .picker__trigger')[2].click();"),
  @('date-picker','web-pickers-date-values.png',"const p=document.querySelectorAll('#date-picker .picker');p[0].querySelector('.picker__trigger').click();cell(p[0],'day',14).click();p[1].querySelector('.picker__trigger').click();cell(p[1],'month',4).click();p[2].querySelector('.picker__trigger').click();cell(p[2],'year',2030).click();"),
  @('datetime-picker','web-pickers-datetime.png',''),
  @('datetime-picker','web-pickers-datetime-open.png',"const p=document.querySelectorAll('#datetime-picker .picker');p[0].querySelector('.picker__trigger').click();cell(p[0],'day',14).click();"),
  @('datetime-picker','web-pickers-datetime-seconds-open.png',"document.querySelectorAll('#datetime-picker .picker__trigger')[1].click();"),
  @('datetime-picker','web-pickers-datetime-values.png',"const p=document.querySelectorAll('#datetime-picker .picker');p[0].querySelector('.picker__trigger').click();cell(p[0],'day',14).click();p[0].querySelector('[data-apply]').click();p[1].querySelector('.picker__trigger').click();cell(p[1],'day',3).click();p[1].querySelector('[data-apply]').click();"),
  @('file-input','web-file-input.png',''),
  @('file-input','web-file-input-files.png',"const i=document.querySelectorAll('#file-input input[type=file]');addF(i[0],[['quarterly-report.pdf',2457600,'application/pdf']]);addF(i[1],[['cover.png',348160,'image/png'],['team-photo.jpg',1887437,'image/jpeg'],['logo.svg',812,'image/svg+xml']]);")
)
foreach ($s in $shots) {
  $js = $na + $kd + $cell + $file + $s[2]
  & pwsh -NoProfile -File $cap -Sections $s[0] -Out "$R/$($s[1])" -Js $js | Select-Object -Last 1
}
