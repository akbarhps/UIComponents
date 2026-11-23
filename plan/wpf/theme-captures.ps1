# Captures every standalone section of the WPF showcase to <Out>\<slug>.png, one hidden run per section.
# Usage: pwsh -NoProfile -File plan/wpf/theme-captures.ps1 -Agent theme -Out $env:TEMP\theme\base [-Theme light]
# -Theme is forwarded to wpf/tests/run-hidden.ps1 (UI_THEME for the app), so captures never depend on the OS theme.
# Ceiling: one app launch per section (about 36 per run). Upgrade path: one launch that iterates sections in-process.
param([Parameter(Mandatory)][string]$Agent, [Parameter(Mandatory)][string]$Out,
      [ValidateSet('light', 'dark', 'system')][string]$Theme = 'light')
$ErrorActionPreference = 'Stop'
$root = (Resolve-Path "$PSScriptRoot\..\..").Path
$runHidden = Join-Path $root 'wpf\tests\run-hidden.ps1'
$outDir = if ([IO.Path]::IsPathRooted($Out)) { $Out } else { Join-Path $root $Out }
New-Item -ItemType Directory -Force $outDir | Out-Null

# Section titles: every SectionTitle TextBlock in wpf\*.xaml (non-recursive), XML-decoded to match the runtime TextBlock.Text.
$titles = @(Select-String -Path (Join-Path $root 'wpf\*.xaml') -Pattern 'SectionTitle' | ForEach-Object {
    if ($_.Line -notmatch '\bText="([^"]*)"') { throw "no Text= on $($_.Filename):$($_.LineNumber)" }
    [System.Net.WebUtility]::HtmlDecode($Matches[1]) } | Select-Object -Unique)
$slugs = @($titles | ForEach-Object { ($_ -replace '[^A-Za-z0-9]+', '-').Trim('-') })
$collisions = @($slugs | Group-Object | Where-Object { $_.Count -gt 1 })
if ($collisions.Count -gt 0) { throw "slug collision: $($collisions[0].Name)" }
"titles=$($titles.Count)"

$captured = 0; $failed = 0
for ($i = 0; $i -lt $titles.Count; $i++) {
    $png = Join-Path $outDir ($slugs[$i] + '.png')
    if (Test-Path $png) { Remove-Item $png }   # so "exists" means written by this run
    $runArgs = @('-NoProfile', '-File', $runHidden, '-Agent', $Agent, '-NoTests', '-Theme', $Theme, '-Section', $titles[$i], '-Capture', "Sections=$png")
    $log = (& pwsh @runArgs 2>&1) | Out-String
    if ($LASTEXITCODE -eq 0 -and $log -notmatch 'CAPTURE FAIL' -and (Test-Path $png)) { $captured++ }
    else { $failed++; "FAIL $($titles[$i]): $($log.Trim())" }
}
"captured $captured, failed $failed"
