# Screenshot one WPF element by AutomationId. No cursor movement, and it works even when another window covers the app.
# Usage (app must be running): pwsh plan/wpf/capture-wpf.ps1 -Id InputGrid -Out plan/wpf/ref/wpf-grid-input.png -ProcessId $p.Id [-Pad 0] [-Focus <AutomationId>]
#   - resizes the app window to 1400x1100 (same as the web references) unless -NoResize
#   - scrolls the main ScrollViewer via ScrollPattern until the element is fully inside the viewport
#   - renders the window with PrintWindow (PW_RENDERFULLCONTENT), so always-on-top windows don't matter, then crops the element
#   - -Focus sets UIA focus on another element first (e.g. a cell), for focus-state captures
# Without -ProcessId it uses the newest UIComponents.Wpf process, which may belong to another agent.
param([Parameter(Mandatory)][string]$Id, [Parameter(Mandatory)][string]$Out, [int]$Pad = 0, [int]$ProcessId = 0,
      [string]$Focus = '', [switch]$NoResize, [int]$Width = 1400, [int]$Height = 1100)
Add-Type -AssemblyName UIAutomationClient, UIAutomationTypes, System.Drawing
Add-Type @'
using System; using System.Runtime.InteropServices;
public static class Win32Capture {
  [DllImport("user32.dll")] public static extern bool PrintWindow(IntPtr hwnd, IntPtr hdc, uint flags);
  [StructLayout(LayoutKind.Sequential)] public struct RECT { public int L, T, R, B; }
  [DllImport("user32.dll")] public static extern bool GetWindowRect(IntPtr hwnd, out RECT r);
}
'@
$ae = [System.Windows.Automation.AutomationElement]
$proc = if ($ProcessId) { Get-Process -Id $ProcessId } else { Get-Process UIComponents.Wpf -ErrorAction Stop | Sort-Object StartTime -Descending | Select-Object -First 1 }
$cond = New-Object System.Windows.Automation.AndCondition(
    (New-Object System.Windows.Automation.PropertyCondition($ae::ProcessIdProperty, $proc.Id)),
    (New-Object System.Windows.Automation.PropertyCondition($ae::ControlTypeProperty, [System.Windows.Automation.ControlType]::Window)))
$win = $ae::RootElement.FindFirst('Children', $cond)
if (-not $win) { throw "window not found for PID $($proc.Id)" }

if (-not $NoResize) {
    $tp = $win.GetCurrentPattern([System.Windows.Automation.TransformPattern]::Pattern)
    $tp.Resize($Width, $Height)
    Start-Sleep -Milliseconds 300
}
$el = $win.FindFirst('Descendants', (New-Object System.Windows.Automation.PropertyCondition($ae::AutomationIdProperty, $Id)))
if (-not $el) { throw "AutomationId '$Id' not found (wrong PID? section hidden by standalone mode?)" }
if ($Focus) {
    $f = $win.FindFirst('Descendants', (New-Object System.Windows.Automation.PropertyCondition($ae::AutomationIdProperty, $Focus)))
    if (-not $f) { throw "Focus target '$Focus' not found" }
    $f.SetFocus()
}

# Largest scrollable pane = main content ScrollViewer.
$scrollCond = New-Object System.Windows.Automation.PropertyCondition($ae::IsScrollPatternAvailableProperty, $true)
$scroller = $win.FindAll('Descendants', $scrollCond) | Sort-Object { $_.Current.BoundingRectangle.Height } -Descending | Select-Object -First 1
if ($scroller) {
    $view = $scroller.Current.BoundingRectangle
    $sp = $scroller.GetCurrentPattern([System.Windows.Automation.ScrollPattern]::Pattern)
    for ($i = 0; $i -lt 80; $i++) {
        $r = $el.Current.BoundingRectangle
        if (-not $r.IsEmpty -and $r.Top -ge $view.Top -and $r.Bottom -le $view.Bottom) { break }
        $amount = if ($r.IsEmpty -or $r.Bottom -gt $view.Bottom) { 'SmallIncrement' } else { 'SmallDecrement' }
        $sp.ScrollVertical([System.Windows.Automation.ScrollAmount]::$amount)
        Start-Sleep -Milliseconds 40
    }
}
Start-Sleep -Milliseconds 250

$hwnd = [IntPtr]$win.Current.NativeWindowHandle
$wr = New-Object Win32Capture+RECT
[void][Win32Capture]::GetWindowRect($hwnd, [ref]$wr)
$full = New-Object System.Drawing.Bitmap ($wr.R - $wr.L), ($wr.B - $wr.T)
$g = [System.Drawing.Graphics]::FromImage($full)
$hdc = $g.GetHdc()
[void][Win32Capture]::PrintWindow($hwnd, $hdc, 2)   # 2 = PW_RENDERFULLCONTENT
$g.ReleaseHdc($hdc); $g.Dispose()

$r = $el.Current.BoundingRectangle
if ($r.IsEmpty) { throw "element has no on-screen rect" }
$crop = New-Object System.Drawing.Rectangle ([int]$r.X - $wr.L - $Pad), ([int]$r.Y - $wr.T - $Pad), ([int]$r.Width + 2 * $Pad), ([int]$r.Height + 2 * $Pad)
$part = $full.Clone($crop, $full.PixelFormat)
$outAbs = Join-Path (Resolve-Path (Split-Path $Out -Parent)).Path (Split-Path $Out -Leaf)
$part.Save($outAbs, [System.Drawing.Imaging.ImageFormat]::Png)
$full.Dispose(); $part.Dispose()
"captured $Id $($crop.Width)x$($crop.Height) from PID $($proc.Id) -> $Out"
