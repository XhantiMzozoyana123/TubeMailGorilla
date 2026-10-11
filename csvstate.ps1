$ErrorActionPreference = 'Continue'
$out = @()
$out += '=== CsvCheck dir ==='
$out += (Get-ChildItem 'c:\Users\Shadow\Documents\TubeMailGorilla\Tools\CsvCheck' -Name | Out-String)
$out += '=== service Parse head ==='
$out += ((Get-Content 'c:\Users\Shadow\Documents\TubeMailGorilla\TubeMailGorilla.Maui.Unlocked\Services\ContactCsvService.cs' | Select-Object -Skip 118 -First 10) -join "`r`n")
$out | Out-File -Encoding utf8 'c:\Users\Shadow\Documents\TubeMailGorilla\csvstate.txt'
'wrote csvstate.txt'
