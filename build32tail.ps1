$ErrorActionPreference = 'Continue'
$lines = Get-Content 'c:\Users\Shadow\Documents\TubeMailGorilla\build32_out.txt'
$tail = $lines | Select-Object -Last 6
$out = @()
$out += "total lines: $($lines.Count)"
$out += '=== last 6 ==='
$out += ($tail -join "`r`n")
$err = ''
if (Test-Path 'c:\Users\Shadow\Documents\TubeMailGorilla\build32_err.txt') { $err = Get-Content 'c:\Users\Shadow\Documents\TubeMailGorilla\build32_err.txt' -Raw }
$out += '=== err file ==='
$out += if ([string]::IsNullOrWhiteSpace($err)) { '(empty)' } else { $err }
$out | Out-File -Encoding utf8 'c:\Users\Shadow\Documents\TubeMailGorilla\build32tail.txt'
'wrote build32tail.txt'
