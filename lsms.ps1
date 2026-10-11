$ErrorActionPreference = 'Continue'
$out = @()
$out += '=== Models ==='
$out += (Get-ChildItem 'c:\Users\Shadow\Documents\TubeMailGorilla\TubeMailGorilla.Maui.Unlocked\Models' | Select-Object -ExpandProperty Name | Out-String)
$out += '=== Services ==='
$out += (Get-ChildItem 'c:\Users\Shadow\Documents\TubeMailGorilla\TubeMailGorilla.Maui.Unlocked\Services' | Select-Object -ExpandProperty Name | Out-String)
$out | Out-File -Encoding utf8 'c:\Users\Shadow\Documents\TubeMailGorilla\ls_ms.txt'
'wrote ls_ms.txt'
