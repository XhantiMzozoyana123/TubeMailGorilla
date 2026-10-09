$o = @()
$code = & curl.exe -s -o NUL -w '%{http_code}' --max-time 5 http://127.0.0.1:8188/system_stats 2>&1
$o += ('system_stats_http=' + $code)
$proc = Get-Process python* -ErrorAction SilentlyContinue | Select-Object -ExpandProperty Id
$o += ('python_pids=' + ($proc -join ','))
$o += '---SRV_ERR TAIL---'
if (Test-Path 'C:\Users\Shadow\ComfyUI\srv_err.log') { $o += (Get-Content 'C:\Users\Shadow\ComfyUI\srv_err.log' -Tail 25) } else { $o += 'no err log' }
$o | Set-Content 'C:\Users\Shadow\Documents\TubeMailGorilla\progress.txt'
Write-Output 'POLL6 DONE'
