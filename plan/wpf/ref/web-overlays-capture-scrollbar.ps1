# Captures the command-palette refs WITH the real 11px scroll bar (no --hide-scrollbars). Run: pwsh -NoProfile -File plan/wpf/ref/web-overlays-capture-scrollbar.ps1
$root='C:\Workspace\Projects\others\ui-design'
$edge='C:\Program Files (x86)\Microsoft\Edge\Application\msedge.exe'
$na="const st=document.createElement('style');st.textContent='*,*::before,*::after{transition:none!important;animation:none!important}';document.head.appendChild(st);const q=s=>document.querySelector(s);const kd=(el,k)=>el.dispatchEvent(new KeyboardEvent('keydown',{key:k,bubbles:true,cancelable:true}));"
$shots=@(@('web-overlays-cmdk-open-scrollbar.png',"q('#openCmdk').click();"),@('web-overlays-cmdk-down2-scrollbar.png',"q('#openCmdk').click();kd(q('#cmdkInput'),'ArrowDown');kd(q('#cmdkInput'),'ArrowDown');"),@('web-overlays-cmdk-up-scrollbar.png',"q('#openCmdk').click();kd(q('#cmdkInput'),'ArrowUp');"))
foreach($s in $shots){
 $inject="<script>const keep=['command-palette'];document.querySelectorAll('.section').forEach(x=>{if(!keep.includes(x.id))x.style.display='none';});const host=document.getElementById(keep[0]).parentElement;[...host.children].forEach(c=>{if(!c.classList.contains('section'))c.style.display='none';});$na$($s[1])</script>"
 $tmp=Join-Path ([IO.Path]::GetTempPath()) 'ui-design-capture-sb.html'
 (Get-Content "$root/web/index.html" -Raw)+$inject | Set-Content $tmp -Encoding utf8
 & $edge --headless=new --disable-gpu --force-device-scale-factor=1 --window-size=1400,1100 "--screenshot=$root\plan\wpf\ref\$($s[0])" ([Uri]$tmp).AbsoluteUri 2>&1 | Out-Null
}
