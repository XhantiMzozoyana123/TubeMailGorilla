$ErrorActionPreference = 'Continue'
$out = @()
$out += '=== CsvCheck files ==='
Get-ChildItem 'c:\Users\Shadow\Documents\TubeMailGorilla\Tools\CsvCheck' | ForEach-Object { "$($_.Name) len=$($_.Length)" } | ForEach-Object { $out += $_ }
$out += ''
$out += '=== ContactCsvService head (first 40 lines) ==='
$out += ((Get-Content 'c:\Users\Shadow\Documents\TubeMailGorilla\TubeMailGorilla.Maui.Unlocked\Services\ContactCsvService.cs' -TotalCount 40) -join "`r`n")
$out | Out-File -Encoding utf8 'c:\Users\Shadow\Documents\TubeMailGorilla\csvinv.txt'
'wrote csvinv.txt'
