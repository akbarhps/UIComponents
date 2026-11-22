# Regenerates every web-nav-* reference and web-nav-measurements.txt (06-navigation.md).
# Run: pwsh -NoProfile -File plan/wpf/ref/web-nav-capture.ps1
$root = 'C:\Workspace\Projects\others\ui-design'
Set-Location $root
$cap = "$root\plan\wpf\capture-web.ps1"
$R = 'plan/wpf/ref'
$na = "const st=document.createElement('style');st.textContent='*,*::before,*::after{transition:none!important;animation:none!important}';document.head.appendChild(st);"
$kd = "const kd=(el,k)=>el.dispatchEvent(new KeyboardEvent('keydown',{key:k,bubbles:true,cancelable:true}));const q=s=>document.querySelector(s);const qa=s=>[...document.querySelectorAll(s)];"
# One tooltip forced visible (the hover end state, no focus ring): n = 1..5 (Top, Bottom, Left, Right, Save).
function TipOn($n, $tf) { "const ts=document.createElement('style');ts.textContent='#tooltip-demo .row > .tip:nth-child($n){z-index:30}#tooltip-demo .row > .tip:nth-child($n)::after{opacity:1!important;transform:$tf!important}';document.head.appendChild(ts);" }
$shots = @(
  @('buttons','web-nav-buttons.png',''),
  @('buttons','web-nav-buttons-focus.png',"q('#buttons .btn').focus();"),
  @('buttons','web-nav-buttons-working.png',"q('#loadingBtn').click();"),
  @('action-menu','web-nav-menu.png',''),
  @('action-menu','web-nav-menu-open.png',"q('#action-menu .menu__trigger').click();"),
  @('action-menu','web-nav-menu-down2.png',"q('#action-menu .menu__trigger').click();kd(q('#projectMenu'),'ArrowDown');kd(q('#projectMenu'),'ArrowDown');"),
  @('action-menu','web-nav-menu-compact-open.png',"q('#action-menu .icon-btn').click();"),
  @('action-menu','web-nav-menu-picked.png',"q('#action-menu .icon-btn').click();qa('#compactMenu .menu__item')[1].click();"),
  @('accordion','web-nav-accordion.png',''),
  @('accordion','web-nav-accordion-toggled.png',"const t=qa('#accordion .accordion__trigger');t[1].click();t[4].click();"),
  @('accordion','web-nav-accordion-focus.png',"qa('#accordion .accordion__trigger')[1].focus();"),
  @('stepper','web-nav-stepper.png',''),
  @('stepper','web-nav-stepper-next.png',"q('#stepNext').click();"),
  @('stepper','web-nav-stepper-last.png',"q('#stepNext').click();q('#stepNext').click();q('#stepNext').click();"),
  @('breadcrumbs','web-nav-breadcrumbs.png',''),
  @('tooltip-demo','web-nav-tooltips.png',''),
  @('tooltip-demo','web-nav-tip-top.png',(TipOn 1 'translateX(-50%)')),
  @('tooltip-demo','web-nav-tip-bottom.png',(TipOn 2 'translateX(-50%)')),
  @('tooltip-demo','web-nav-tip-left.png',(TipOn 3 'translateY(-50%)')),
  @('tooltip-demo','web-nav-tip-right.png',(TipOn 4 'translateY(-50%)')),
  @('tooltip-demo','web-nav-tip-save.png',(TipOn 5 'translateX(-50%)')),
  @('tooltip-demo','web-nav-tooltips-focus-top.png',"qa('#tooltip-demo .tip .btn')[0].focus();"),
  @('tabs','web-nav-tabs.png',''),
  @('tabs','web-nav-tabs-security.png',"qa('#tabs .tab')[1].click();"),
  @('tabs','web-nav-tabs-focus.png',"qa('#tabs .tab')[0].focus();kd(qa('#tabs .tab')[0],'ArrowRight');")
)
foreach ($s in $shots) {
  $js = $na + $kd + $s[2]
  & pwsh -NoProfile -File $cap -Sections $s[0] -Out "$R/$($s[1])" -Js $js | Select-Object -Last 1
}
# Geometry dump (full page, Edge --dump-dom). Rel numbers are what the plan uses.
$edge = 'C:\Program Files (x86)\Microsoft\Edge\Application\msedge.exe'
$tmp = Join-Path ([IO.Path]::GetTempPath()) 'ui-design-nav-measure.html'
(Get-Content "$root/web/index.html" -Raw) + "<script>" + (Get-Content "$root/$R/web-nav-measure.js" -Raw) + "</script>" | Set-Content $tmp -Encoding utf8
# msedge is a GUI-subsystem exe: '&' does not capture its stdout, so redirect through Start-Process.
$dumpFile = Join-Path ([IO.Path]::GetTempPath()) 'ui-design-nav-dump.txt'
Start-Process -FilePath $edge -ArgumentList @('--headless=new','--disable-gpu','--hide-scrollbars','--force-device-scale-factor=1','--window-size=1400,1100','--dump-dom',([Uri]$tmp).AbsoluteUri) -RedirectStandardOutput $dumpFile -Wait -NoNewWindow
$text = [Regex]::Match((Get-Content $dumpFile -Raw), '(?s)<pre id="o">(.*)</pre>').Groups[1].Value
$text = $text.Replace('&lt;','<').Replace('&gt;','>').Replace('&quot;','"').Replace('&amp;','&')
$hdr = "# 06 navigation: web geometry (generated $(Get-Date -Format yyyy-MM-dd) by web-nav-measure.js, Edge --dump-dom --hide-scrollbars). Use the 'rel' numbers (relative to the .demo card or the named base)."
Set-Content "$R/web-nav-measurements.txt" ($hdr + "`n" + $text) -Encoding utf8
"measurements -> $R/web-nav-measurements.txt"
