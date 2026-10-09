$root = 'c:\Users\Shadow\Documents\TubeMailGorilla'
$r = @()
$r += '=== git status --short ==='
$r += (git -C $root status --short 2>&1)
$r += '=== diff stat (tracked) ==='
$r += (git -C $root diff --stat HEAD 2>&1)
$r | Set-Content (Join-Path $root 'gitstatus_65.txt') -Encoding utf8
Write-Output ('LINES=' + $r.Count)
