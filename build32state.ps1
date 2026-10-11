$ErrorActionPreference = 'Continue'
$out = @()
$procs = Get-Process -Name 'dotnet' -ErrorAction SilentlyContinue | Select-Object Id, StartTime, @{N='Elapsed';E={(Get-Date) - $_.StartTime}}
$out += "dotnet procs: $($procs.Count)"
$out += (@($procs | ForEach-Object { "pid=$($_.Id) started=$($_.StartTime) elapsed=$($_.Elapsed)" }) -join "`r`n")
$out += ''
$out += '=== build32_out tail ==='
if (Test-Path 'c:\Users\Shadow\Documents\TubeMailGorilla\build32_out.txt') {
  $out += ((Get-Content 'c:\Users\Shadow\Documents\TubeMailGorilla\build32_out.txt' -Tail 8) -join "`r`n")
} else { $out += '(no build32_out.txt yet)' }
$out | Out-File -Encoding utf8 'c:\Users\Shadow\Documents\TubeMailGorilla\build32state.txt'
'wrote build32state.txt'
