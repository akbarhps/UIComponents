# Test-slot limiter: at most $env:UITEST_SLOTS (default 4) UI test suites run at once, machine-wide.
# Named Mutexes (not a counter semaphore): the OS frees a slot when its owner dies, so a killed run cannot leak one.
# Usage: . .\test-slot.ps1; $slot = Enter-TestSlot; try { ... } finally { Exit-TestSlot $slot }
function Enter-TestSlot {
    $n = if ($env:UITEST_SLOTS) { [int]$env:UITEST_SLOTS } else { 4 }
    $t0 = Get-Date; $said = $false
    while ($true) {
        for ($i = 0; $i -lt $n; $i++) {
            $m = New-Object System.Threading.Mutex($false, "ui-design-uitests-slot$i")
            $got = $false
            try { $got = $m.WaitOne(0) }
            catch { if ($_.Exception.InnerException -is [System.Threading.AbandonedMutexException] -or $_.Exception -is [System.Threading.AbandonedMutexException]) { $got = $true } else { throw } }
            if ($got) {
                if ($said) { Write-Host ("got test slot $i after {0:N0}s" -f ((Get-Date) - $t0).TotalSeconds) }
                return $m
            }
            $m.Dispose()
        }
        if (-not $said) { Write-Host "waiting for a free test slot (max $n suites at once)..."; $said = $true }
        Start-Sleep -Seconds 2
    }
}
function Exit-TestSlot($m) { if ($m) { try { $m.ReleaseMutex() } catch {}; $m.Dispose() } }
