$ErrorActionPreference = 'Continue'
$root = 'c:\Users\Shadow\Documents\TubeMailGorilla'
Set-Location $root
$out = @()
$out += '=== tracked root ps1 (already in repo?) ==='
$out += (git ls-files '*.ps1' 2>&1 | Out-String)
$out += '=== tracked root txt? ==='
$out += (git ls-files '*.txt' 2>&1 | Out-String)
$out += '=== Tools tracked? ==='
$out += (git ls-files 'Tools/*' 2>&1 | Out-String)
$out += '=== full status porcelain ==='
$out += (git status --porcelain=v1 --untracked-files=all 2>&1 | Out-String)
$out | Out-File -Encoding utf8 "$root\push_info2.txt"
'wrote push_info2.txt'
