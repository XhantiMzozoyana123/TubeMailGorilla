$o = @()
$port = (Test-NetConnection -ComputerName 127.0.0.1 -Port 8188 -InformationLevel Quiet -WarningAction SilentlyContinue) 2>$null
$o += ('port8188=' + $port)
$code = & curl.exe -s -o NUL -w '%{http_code}' --max-time 5 http://127.0.0.1:8188/system_stats 2>&1
$o += ('system_stats_http=' + $code)
$o += '---SERVER LOG TAIL---'
if (Test-Path 'C:\Users\Shadow\ComfyUI\server.log') { $o += (Get-Content 'C:\Users\Shadow\ComfyUI\server.log' -Tail 15) } else { $o += 'no server.log yet' }
$o | Set-Content 'C:\Users\Shadow\Documents\TubeMailGorilla\progress.txt'
Write-Output 'POLL5 DONE'
