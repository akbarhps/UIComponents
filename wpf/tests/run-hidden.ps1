# Runs the app plus the UI Automation tests (and optional captures) on a hidden Windows desktop.
# Nothing appears on your screen and nothing takes focus or keyboard input from you.
#
# Usage (from the repo root, after wpf\build.bat <agent> and wpf\tests\build-tests.bat):
#   pwsh wpf/tests/run-hidden.ps1 -Agent grid
#   pwsh wpf/tests/run-hidden.ps1 -Agent forms -Section "Forms" -Capture "FormsCard=plan/wpf/ref/wpf-forms-card1.png","FormsCard@FieldName=plan/wpf/ref/wpf-forms-focus.png"
#   pwsh wpf/tests/run-hidden.ps1 -Agent grid -NoTests -Capture "InputGrid=plan/wpf/ref/wpf-grid-input.png"
#
# -Capture entries: "<AutomationId>=<out.png>" or "<AutomationId>@<FocusAutomationId>=<out.png>".
# Each agent gets its own desktop (uitest-<agent>), so parallel agents don't collide either.
param([Parameter(Mandatory)][string]$Agent, [string]$Section = '', [string[]]$Capture = @(), [switch]$NoTests,
      [int]$TimeoutSec = 300)
$ErrorActionPreference = 'Stop'
$Capture = @($Capture | ForEach-Object { $_ -split ',' } | Where-Object { $_ })   # pwsh -File passes arrays as one comma-joined string
Add-Type @'
using System; using System.Runtime.InteropServices;
public static class HiddenDesktop {
  [DllImport("user32.dll", CharSet=CharSet.Unicode, SetLastError=true)]
  public static extern IntPtr CreateDesktop(string name, IntPtr dev, IntPtr mode, int flags, uint access, IntPtr sa);
  [DllImport("user32.dll")] public static extern bool CloseDesktop(IntPtr h);
  [StructLayout(LayoutKind.Sequential, CharSet=CharSet.Unicode)]
  public struct STARTUPINFO { public int cb; public string lpReserved; public string lpDesktop; public string lpTitle;
    public int dwX, dwY, dwXSize, dwYSize, dwXCountChars, dwYCountChars, dwFillAttribute, dwFlags; public short wShowWindow, cbReserved2;
    public IntPtr lpReserved2, hStdInput, hStdOutput, hStdError; }
  [StructLayout(LayoutKind.Sequential)]
  public struct PROCESS_INFORMATION { public IntPtr hProcess, hThread; public int dwProcessId, dwThreadId; }
  [DllImport("kernel32.dll", CharSet=CharSet.Unicode, SetLastError=true)]
  public static extern bool CreateProcess(string app, string cmd, IntPtr pa, IntPtr ta, bool inherit, uint flags,
    IntPtr env, string dir, ref STARTUPINFO si, out PROCESS_INFORMATION pi);
  [DllImport("kernel32.dll")] public static extern uint WaitForSingleObject(IntPtr h, uint ms);
  [DllImport("kernel32.dll")] public static extern bool GetExitCodeProcess(IntPtr h, out uint code);
  [DllImport("kernel32.dll")] public static extern bool CloseHandle(IntPtr h);
  [DllImport("kernel32.dll")] public static extern bool TerminateProcess(IntPtr h, uint code);
  public static PROCESS_INFORMATION Start(string desktop, string cmd, string dir) {
    var si = new STARTUPINFO(); si.cb = Marshal.SizeOf(si); si.lpDesktop = desktop;
    PROCESS_INFORMATION pi;
    if (!CreateProcess(null, cmd, IntPtr.Zero, IntPtr.Zero, false, 0x08000000 /*CREATE_NO_WINDOW*/, IntPtr.Zero, dir, ref si, out pi))
      throw new System.ComponentModel.Win32Exception(Marshal.GetLastWin32Error());
    return pi;
  }
}
'@
$root = (Resolve-Path "$PSScriptRoot\..\..").Path
$exe = Join-Path $root "wpf\bin\$Agent\UIComponents.Wpf.exe"
$tests = Join-Path $root "wpf\tests\bin\UIAutomationTests.exe"
if (-not (Test-Path $exe)) { throw "missing $exe - run wpf\build.bat $Agent first" }
$deskName = "uitest-$Agent"
$desk = [HiddenDesktop]::CreateDesktop($deskName, [IntPtr]::Zero, [IntPtr]::Zero, 0, 0x10000000 <# GENERIC_ALL #>, [IntPtr]::Zero)
if ($desk -eq [IntPtr]::Zero) { throw "CreateDesktop failed" }
$tmp = Join-Path ([IO.Path]::GetTempPath()) "uitest-$Agent"
New-Item -ItemType Directory -Force $tmp | Out-Null

function Invoke-OnDesktop([string]$cmd, [string]$logName) {
    $log = Join-Path $tmp $logName
    $pi = [HiddenDesktop]::Start($deskName, "cmd.exe /c `"$cmd > `"$log`" 2>&1`"", $root)
    if ([HiddenDesktop]::WaitForSingleObject($pi.hProcess, [uint32]($TimeoutSec * 1000)) -ne 0) {
        [void][HiddenDesktop]::TerminateProcess($pi.hProcess, 1); "TIMEOUT after $TimeoutSec s: $cmd"
    }
    $code = 0; [void][HiddenDesktop]::GetExitCodeProcess($pi.hProcess, [ref]$code)
    [void][HiddenDesktop]::CloseHandle($pi.hProcess); [void][HiddenDesktop]::CloseHandle($pi.hThread)
    if (Test-Path $log) { Get-Content $log }
    return $code
}

function Wait-Process([object]$pi) {
    if ([HiddenDesktop]::WaitForSingleObject($pi.hProcess, [uint32]($TimeoutSec * 1000)) -ne 0) {
        [void][HiddenDesktop]::TerminateProcess($pi.hProcess, 1); "TIMEOUT after $TimeoutSec s"
    }
    $code = 0; [void][HiddenDesktop]::GetExitCodeProcess($pi.hProcess, [ref]$code)
    [void][HiddenDesktop]::CloseHandle($pi.hProcess); [void][HiddenDesktop]::CloseHandle($pi.hThread)
    return $code
}

function Get-OutPath([string]$p) { if ([IO.Path]::IsPathRooted($p)) { $p } else { Join-Path $root $p } }
$sectionArg = if ($Section) { " `"$Section`"" } else { '' }
$exit = 0
try {
    # Captures: a separate app instance renders each element off-screen (App.xaml.cs --capture), then exits.
    if ($Capture.Count -gt 0) {
        $specs = ($Capture | ForEach-Object { $t, $o = $_ -split '=', 2; "`"$t=$(Get-OutPath $o)`"" }) -join ' '
        $code = Wait-Process ([HiddenDesktop]::Start($deskName, "`"$exe`"$sectionArg --capture $specs", $root))
        foreach ($c in $Capture) {
            $o = Get-OutPath (($c -split '=', 2)[1])
            if (Test-Path "$o.error.txt") { "CAPTURE FAIL  " + (Get-Content "$o.error.txt"); Remove-Item "$o.error.txt" }
            elseif (Test-Path $o) { "captured -> $o" }
        }
        if ($code -ne 0) { $exit = $code }
    }
    # Tests: the full app (all sections), tests pinned to its PID.
    if (-not $NoTests) {
        $app = [HiddenDesktop]::Start($deskName, "`"$exe`"", $root)
        "app PID $($app.dwProcessId) on hidden desktop '$deskName'"
        try {
            Start-Sleep -Seconds 5
            $result = Invoke-OnDesktop "`"$tests`" $($app.dwProcessId)" "tests.log"
            $result | Select-Object -SkipLast 1
            if ($result[-1] -ne 0) { $exit = $result[-1] }
        }
        finally {
            [void][HiddenDesktop]::TerminateProcess($app.hProcess, 0)   # only our own PID
            [void][HiddenDesktop]::CloseHandle($app.hProcess); [void][HiddenDesktop]::CloseHandle($app.hThread)
        }
    }
}
finally { [void][HiddenDesktop]::CloseDesktop($desk) }
exit $exit
