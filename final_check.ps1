$root = 'c:\Users\Shadow\Documents\TubeMailGorilla'
$o = @()
$o += '=== latest build result ==='
$c = Get-Content (Join-Path $root 'build_25.log') -ErrorAction SilentlyContinue
if ($c) { $o += ($c | Select-String 'Error\(s\)|Warning\(s\)' | ForEach-Object { $_.ToString().Trim() }) }
$o += '=== comfyui server ==='
$code = & curl.exe -s -o NUL -w '%{http_code}' --max-time 5 http://127.0.0.1:8188/system_stats
$o += ('system_stats http=' + $code)
$o += '=== git status (tracked changes) ==='
$o += (git -C $root status --short 2>&1 | Where-Object { $_ -notlike '??*' })
$o | Set-Content (Join-Path $root 'final_33.txt') -Encoding utf8
Write-Output 'FINAL DONE'
