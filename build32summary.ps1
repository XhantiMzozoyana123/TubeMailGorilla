$ErrorActionPreference = 'Continue'
$lines = Get-Content 'c:\Users\Shadow\Documents\TubeMailGorilla\build32_out.txt'
$summary = $lines | Where-Object { $_ -match 'error|Build succeeded|Warning\(s\)|Error\(s\)' } | Select-Object -Last 6
$out = @()
$out += "total lines now: $($lines.Count)"
$out += '=== summary lines ==='
$out += ($summary -join "`r`n")
$out | Out-File -Encoding utf8 'c:\Users\Shadow\Documents\TubeMailGorilla\build32summary.txt'
'wrote build32summary.txt'
