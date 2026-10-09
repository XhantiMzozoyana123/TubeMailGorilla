# Restart ComfyUI (it is already down - no need to kill python processes first).
# Direct python launch (same as start_server2.ps1): skips the .bat's `pause`,
# detached, both streams to files, no -Wait (server runs forever).
$root = 'C:\Users\Shadow\ComfyUI\ComfyUI_windows_portable'
$p = Start-Process -FilePath (Join-Path $root 'python_embeded\python.exe') `
    -ArgumentList '-s','ComfyUI\main.py','--windows-standalone-build' `
    -WorkingDirectory $root -WindowStyle Hidden -PassThru `
    -RedirectStandardOutput 'C:\Users\Shadow\ComfyUI\srv_out.log' `
    -RedirectStandardError  'C:\Users\Shadow\ComfyUI\srv_err.log'
$p.Id | Set-Content 'C:\Users\Shadow\ComfyUI\server.pid'
Write-Output ("SERVER LAUNCHED pid=" + $p.Id)