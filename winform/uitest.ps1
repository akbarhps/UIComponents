param([string]$Shot = "uitest.png", [int]$Wheel = 0, [int]$X = -1, [int]$Y = -1, [string]$Click = "", [string]$Keys = "")

Add-Type -AssemblyName System.Windows.Forms, System.Drawing, Microsoft.VisualBasic
Add-Type -TypeDefinition @'
using System;
using System.Runtime.InteropServices;
public class NativeUI {
  [DllImport("user32.dll")] public static extern bool SetCursorPos(int x, int y);
  [DllImport("user32.dll")] public static extern void mouse_event(uint f, uint dx, uint dy, int data, int extra);
  [DllImport("user32.dll")] public static extern IntPtr FindWindow(string cls, string name);
  [DllImport("user32.dll")] public static extern bool SetForegroundWindow(IntPtr h);
  [DllImport("user32.dll")] public static extern bool ShowWindow(IntPtr h, int cmd);
}
'@

$p = Get-Process -Name UIComponents -ErrorAction SilentlyContinue | Select-Object -First 1
if (-not $p) { throw "UIComponents is not running" }
$h = [NativeUI]::FindWindow($null, "Claude UI - WinForms components")
[NativeUI]::ShowWindow($h, 9) | Out-Null
[Microsoft.VisualBasic.Interaction]::AppActivate($p.Id)
[NativeUI]::SetForegroundWindow($h) | Out-Null
Start-Sleep -Milliseconds 300

if ($Wheel -ne 0) {
  [NativeUI]::SetCursorPos(1000, 500) | Out-Null
  $steps = [Math]::Abs($Wheel)
  for ($i = 0; $i -lt $steps; $i++) {
    [NativeUI]::mouse_event(0x0800, 0, 0, $(if ($Wheel -lt 0) { -120 } else { 120 }), 0)
    Start-Sleep -Milliseconds 40
  }
  Start-Sleep -Milliseconds 300
}

if ($X -ge 0 -and $Y -ge 0) {
  [NativeUI]::SetCursorPos($X, $Y) | Out-Null
  if ($Click -eq "double") {
    [NativeUI]::mouse_event(0x0002, 0, 0, 0, 0); [NativeUI]::mouse_event(0x0004, 0, 0, 0, 0)
    Start-Sleep -Milliseconds 80
    [NativeUI]::mouse_event(0x0002, 0, 0, 0, 0); [NativeUI]::mouse_event(0x0004, 0, 0, 0, 0)
  } elseif ($Click -eq "right") {
    [NativeUI]::mouse_event(0x0008, 0, 0, 0, 0); [NativeUI]::mouse_event(0x0010, 0, 0, 0, 0)
  } elseif ($Click -eq "down") {
    [NativeUI]::mouse_event(0x0002, 0, 0, 0, 0)
  } elseif ($Click -eq "up") {
    [NativeUI]::mouse_event(0x0004, 0, 0, 0, 0)
  } else {
    [NativeUI]::mouse_event(0x0002, 0, 0, 0, 0); [NativeUI]::mouse_event(0x0004, 0, 0, 0, 0)
  }
  Start-Sleep -Milliseconds 200
}

if ($Keys) { [System.Windows.Forms.SendKeys]::SendWait($Keys); Start-Sleep -Milliseconds 300 }

$b = [System.Windows.Forms.Screen]::PrimaryScreen.Bounds
$bmp = New-Object System.Drawing.Bitmap($b.Width, $b.Height)
$g = [System.Drawing.Graphics]::FromImage($bmp)
$g.CopyFromScreen($b.Location, [System.Drawing.Point]::Empty, $b.Size)
$bmp.Save((Join-Path $PSScriptRoot $Shot), [System.Drawing.Imaging.ImageFormat]::Png)
$g.Dispose(); $bmp.Dispose()
Write-Output "saved $Shot"
