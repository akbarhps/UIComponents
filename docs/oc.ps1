# Reviewer helper: drive worker sessions in OpenCode Desktop through its background server.
#   pwsh docs/oc.ps1 status                     # which workers are running, pending permission prompts
#   pwsh docs/oc.ps1 last grid                  # text of the worker's latest reply (since the last user prompt)
#   pwsh docs/oc.ps1 send grid -File prompt.md  # send a prompt (refuses if the worker is busy)
#   pwsh docs/oc.ps1 wait grid                  # block until the worker is idle, then print its reply
# Needs the OpenCode Desktop app running (its bundled CLI talks to the app's server).
param(
    [Parameter(Position = 0, Mandatory = $true)][ValidateSet('status', 'last', 'send', 'wait')][string]$Cmd,
    [Parameter(Position = 1)][string]$Agent,
    [string]$File,
    [int]$PollSec = 30
)
$ErrorActionPreference = 'Stop'
$cli = Join-Path $env:APPDATA 'ai.opencode.desktop\cli\2.3.2-x64\opencode-cli.exe'
# ponytail: hard-coded map; update when a worker gets a new session
$sessions = @{
    grid   = 'ses_e0238a9d0ffe1NkofzNcUvbWyi'
    forms  = 'ses_e023107ceffes9dzt57DNkx3dR'
    tables = 'ses_e04098a66ffeTvKHynZrtF4SOi'
}

function Invoke-Api([string[]]$ApiArgs) {
    $out = & $cli api @ApiArgs 2>&1 | Out-String
    if ($out -match '(?m)^HTTP [45]\d\d') { throw "opencode api $($ApiArgs[0]) $($ApiArgs[1]): $out" }
    return $out | ConvertFrom-Json
}
function Get-Busy { return (Invoke-Api @('GET', '/api/session/active')).data }
function Test-Busy([string]$sid) { return $null -ne (Get-Busy).PSObject.Properties[$sid] }

function Get-LastReply([string]$sid) {
    $msgs = (Invoke-Api @('GET', "/api/session/$sid/message", '--param', 'limit=60', '--param', 'order=desc')).data
    $texts = New-Object System.Collections.Generic.List[string]
    foreach ($m in $msgs) {
        if ($m.type -eq 'user') { break }
        if ($m.type -eq 'assistant') {
            $t = ($m.content | Where-Object { $_.type -eq 'text' } | ForEach-Object { $_.text }) -join "`n"
            if ($t) { $texts.Insert(0, $t) }
        }
    }
    return ($texts -join "`n`n")
}

if ($Cmd -ne 'status') {
    if (-not $sessions.ContainsKey($Agent)) { throw "unknown agent '$Agent' (known: $($sessions.Keys -join ', '))" }
    $sid = $sessions[$Agent]
}

switch ($Cmd) {
    'status' {
        $busy = Get-Busy
        foreach ($k in $sessions.Keys | Sort-Object) {
            $state = if ($busy.PSObject.Properties[$sessions[$k]]) { 'RUNNING' } else { 'idle' }
            '{0,-7} {1,-8} {2}' -f $k, $state, $sessions[$k]
        }
        $perm = (Invoke-Api @('GET', '/api/permission/request')).data
        if ($perm) { 'PENDING PERMISSIONS:'; $perm | ConvertTo-Json -Depth 5 }
    }
    'last' { Get-LastReply $sid }
    'send' {
        if (Test-Busy $sid) { throw "$Agent is running; wait first" }
        $text = Get-Content $File -Raw
        # Inline JSON body (command line limit ~32K chars; prompts are far smaller).
        $body = @{ text = $text } | ConvertTo-Json -Compress
        Invoke-Api @('POST', "/api/session/$sid/prompt", '--data', $body) | Out-Null
        "sent to $Agent ($sid)"
    }
    'wait' {
        Start-Sleep -Seconds 5
        while (Test-Busy $sid) { Start-Sleep -Seconds $PollSec }
        Get-LastReply $sid
    }
}
