$out = 'c:\Users\Shadow\Documents\TubeMailGorilla\Tools\app-status.txt'
Start-Sleep -Seconds 8
$proc = Get-Process -Id 15300 -ErrorAction SilentlyContinue
$lines = @(
    'time=' + (Get-Date).ToString('HH:mm:ss'),
    'alive=' + ($null -ne $proc)
)
$crashLog = Join-Path $env:LOCALAPPDATA 'TubeMailGorillaUnlocked\startup-crash.log'
if (Test-Path $crashLog) {
    $lines += '--- startup-crash.log tail ---'
    $lines += (Get-Content $crashLog -Tail 25)
} else {
    $lines += 'no startup-crash.log'
}
$lines | Set-Content $out
