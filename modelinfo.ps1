$ErrorActionPreference = 'Continue'
$out = @()
$out += '=== Models dir ==='
$out += (Get-ChildItem 'c:\Users\Shadow\Documents\TubeMailGorilla\TubeMailGorilla.Maui.Unlocked\Models' -Name | Out-String)
$out += '=== which file defines EmailContact ==='
$hits = Select-String -Path 'c:\Users\Shadow\Documents\TubeMailGorilla\TubeMailGorilla.Maui.Unlocked\Models\*.cs' -Pattern 'class EmailContact'
$out += (@($hits | ForEach-Object { "$($_.Filename):$($_.LineNumber): $($_.Line.Trim())" }) -join "`r`n")
$out | Out-File -Encoding utf8 'c:\Users\Shadow\Documents\TubeMailGorilla\modelinfo.txt'
'wrote modelinfo.txt'
