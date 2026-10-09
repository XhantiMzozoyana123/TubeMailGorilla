$root = 'C:\Users\Shadow\ComfyUI\ComfyUI_windows_portable'
Get-Process python* -ErrorAction SilentlyContinue | Stop-Process -Force -ErrorAction SilentlyContinue
Start-Sleep -Seconds 2

# Launch python.exe DIRECTLY (skip the .bat's pause), detached, capturing BOTH
# streams to files. Do NOT use -Wait (server runs forever). No stdout-to-console
# redirection that can crash the portable build.
$p = Start-Process -FilePath (Join-Path $root 'python_embeded\python.exe') `
  -ArgumentList '-s','ComfyUI\main.py','--windows-standalone-build' `
  -WorkingDirectory $root -WindowStyle Hidden -PassThru `
  -RedirectStandardOutput 'C:\Users\Shadow\ComfyUI\srv_out.log' `
  -RedirectStandardError  'C:\Users\Shadow\ComfyUI\srv_err.log'
$p.Id | Set-Content 'C:\Users\Shadow\ComfyUI\server.pid'
Write-Output ("SERVER LAUNCHED pid=" + $p.Id)
