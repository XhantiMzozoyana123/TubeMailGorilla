$ErrorActionPreference = 'Continue'
$root = 'c:\Users\Shadow\Documents\TubeMailGorilla'
Set-Location $root
$out = @()
$out += '=== branch ==='
$out += (git branch --show-current 2>&1 | Out-String)
$out += '=== remote ==='
$out += (git remote -v 2>&1 | Out-String)
$out += '=== log ==='
$out += (git log --oneline -5 2>&1 | Out-String)
$out | Out-File -Encoding utf8 "$root\push_info.txt"
'wrote push_info.txt'
