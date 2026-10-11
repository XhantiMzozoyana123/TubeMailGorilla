$ErrorActionPreference = 'Continue'
$lines = Get-Content 'c:\Users\Shadow\Documents\TubeMailGorilla\Tools\CsvCheck\Program.cs' -Raw
$out = @()
$out += '=== lines 20-30 raw ==='
$split = $lines -split "`r?`n"
for ($i = 19; $i -lt 30; $i++) { $out += "$($i+1): $($split[$i])" }
$out += '=== lines 40-70 raw ==='
for ($i = 39; $i -lt 70; $i++) { $out += "$($i+1): $($split[$i])" }
$out | Out-File -Encoding utf8 'c:\Users\Shadow\Documents\TubeMailGorilla\rawlines.txt'
'wrote rawlines.txt'
