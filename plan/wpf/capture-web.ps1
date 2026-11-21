# Screenshot web/index.html showing only the listed sections (sidebar kept), at 100% scale.
# Usage: pwsh plan/wpf/capture-web.ps1 -Sections sheet-input,typed-sheet -Out plan/wpf/ref/web-grid-input.png [-Js "extra JS run after load"]
param([Parameter(Mandatory)][string[]]$Sections, [Parameter(Mandatory)][string]$Out, [string]$Js = '', [int]$Width = 1400, [int]$Height = 1100)
$root = Resolve-Path "$PSScriptRoot/../.."
$edge = 'C:\Program Files (x86)\Microsoft\Edge\Application\msedge.exe'
$tmp = Join-Path ([IO.Path]::GetTempPath()) 'ui-design-capture.html'
$ids = ($Sections -split "," | ForEach-Object { "'$_'" }) -join ','
$inject = @"
<script>
const keep=[$ids];
document.querySelectorAll('.section').forEach(s=>{ if(!keep.includes(s.id)) s.style.display='none'; });
const host=document.getElementById(keep[0]).parentElement;
[...host.children].forEach(c=>{ if(!c.classList.contains('section')) c.style.display='none'; });
$Js
</script>
"@
(Get-Content "$root/web/index.html" -Raw) + $inject | Set-Content $tmp -Encoding utf8
$outAbs = Join-Path (Resolve-Path (Split-Path $Out -Parent)) (Split-Path $Out -Leaf)
& $edge --headless=new --disable-gpu --hide-scrollbars --force-device-scale-factor=1 "--window-size=$Width,$Height" "--screenshot=$outAbs" ([Uri]$tmp).AbsoluteUri 2>&1 | Select-String "bytes written"
"web capture -> $outAbs"
