$ErrorActionPreference = 'Continue'
$out = @()
$out += '=== CsvCheck dir ==='
Get-ChildItem 'c:\Users\Shadow\Documents\TubeMailGorilla\Tools\CsvCheck' | ForEach-Object { "$($_.Name)  len=$($_.Length)" } | Out-String | ForEach-Object { $out += $_ }
$out | Out-File -Encoding utf8 'c:\Users\Shadow\Documents\TubeMailGorilla\csvdir.txt'
'wrote csvdir.txt'
