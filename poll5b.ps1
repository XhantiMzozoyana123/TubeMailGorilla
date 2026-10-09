$o = @()
$code = & curl.exe -s -o NUL -w '%{http_code}' --max-time 5 http://127.0.0.1:8188/system_stats 2>&1
$o += ('system_stats_http=' + $code)
$proc = Get-Process python* -ErrorAction SilentlyContinue | Select-Object -First 3 -ExpandProperty Id
$o += ('python_pids=' + ($proc -join ','))
$o += '---SERVER LOG TAIL---'
if (Test-Path 'C:\Users\Shadow\ComfyUI\server.log') { $o += (Get-Content 'C:\Users\Shadow\ComfyUI\server.log' -Tail 20) } else { $o += 'no server.log yet' }
$o | Set-Content 'C:\Users\Shadow\Documents\TubeMailGorilla\progress.txt'
Write-Output 'POLL5B DONE'
