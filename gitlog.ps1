$root = 'c:\Users\Shadow\Documents\TubeMailGorilla'
$o = @()
$o += '=== log ==='
$o += (git -C $root log --oneline -8 2>&1)
$o += '=== status -sb ==='
$o += (git -C $root status -sb 2>&1 | Select-Object -First 5)
$o | Set-Content ($root + '\gitlog_29.txt') -Encoding utf8
Write-Output 'DONE'
