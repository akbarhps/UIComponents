# Screenshot one WPF element by AutomationId — no cursor movement.
# Usage (app must be running): pwsh plan/wpf/capture-wpf.ps1 -Id InputGrid -Out plan/wpf/ref/wpf-grid-input.png [-Pad 0] [-ProcessId N]
# Without -ProcessId it uses the newest UIComponents.Wpf process.
# Scrolls the main ScrollViewer via ScrollPattern until the element is fully on screen, then copies that screen rect.
param([Parameter(Mandatory)][string]$Id, [Parameter(Mandatory)][string]$Out, [int]$Pad = 0, [int]$ProcessId = 0)
Add-Type -AssemblyName UIAutomationClient, UIAutomationTypes, System.Drawing
$ae = [System.Windows.Automation.AutomationElement]
$proc = if ($ProcessId) { Get-Process -Id $ProcessId } else { Get-Process UIComponents.Wpf -ErrorAction Stop | Sort-Object StartTime -Descending | Select-Object -First 1 }
$win = $ae::RootElement.FindFirst('Children', (New-Object System.Windows.Automation.PropertyCondition($ae::ProcessIdProperty, $proc.Id)))
if (-not $win) { throw "window not found" }
$win.SetFocus()
$el = $win.FindFirst('Descendants', (New-Object System.Windows.Automation.PropertyCondition($ae::AutomationIdProperty, $Id)))
if (-not $el) { throw "AutomationId '$Id' not found" }

# Largest scrollable pane = main content ScrollViewer.
$scrollCond = New-Object System.Windows.Automation.PropertyCondition($ae::IsScrollPatternAvailableProperty, $true)
$scroller = $win.FindAll('Descendants', $scrollCond) | Sort-Object { $_.Current.BoundingRectangle.Height } -Descending | Select-Object -First 1
$view = $scroller.Current.BoundingRectangle
if ($scroller) {
    $sp = $scroller.GetCurrentPattern([System.Windows.Automation.ScrollPattern]::Pattern)
    for ($i = 0; $i -lt 60; $i++) {
        $r = $el.Current.BoundingRectangle
        if (-not $r.IsEmpty -and $r.Top -ge $view.Top -and $r.Bottom -le $view.Bottom) { break }
        $delta = if ($r.IsEmpty -or $r.Top -gt $view.Top) { 'SmallIncrement' } else { 'SmallDecrement' }
        if ($r.IsEmpty -or $r.Bottom -gt $view.Bottom) { $delta = 'LargeIncrement' }
        if (-not $r.IsEmpty -and $r.Top -lt $view.Top) { $delta = 'SmallDecrement' }
        $sp.ScrollVertical([System.Windows.Automation.ScrollAmount]::$delta)
        Start-Sleep -Milliseconds 60
    }
}
Start-Sleep -Milliseconds 200
$r = $el.Current.BoundingRectangle
if ($r.IsEmpty) { throw "element has no on-screen rect" }
$x = [int]$r.X - $Pad; $y = [int]$r.Y - $Pad; $w = [int]$r.Width + 2 * $Pad; $h = [int]$r.Height + 2 * $Pad
$bmp = New-Object System.Drawing.Bitmap $w, $h
$g = [System.Drawing.Graphics]::FromImage($bmp)
$g.CopyFromScreen($x, $y, 0, 0, $bmp.Size)
$bmp.Save((Resolve-Path -LiteralPath (Split-Path $Out -Parent)).Path + '\' + (Split-Path $Out -Leaf), [System.Drawing.Imaging.ImageFormat]::Png)
"captured $Id ${w}x${h} at $x,$y -> $Out"
