$root = 'C:\Users\Shadow\ComfyUI\ComfyUI_windows_portable'
# Kill any stuck batch/pause
Get-Process python* -ErrorAction SilentlyContinue | Stop-Process -Force -ErrorAction SilentlyContinue
Get-Process cmd -ErrorAction SilentlyContinue | Where-Object { $_.Id -eq 17796 } | Stop-Process -Force -ErrorAction SilentlyContinue
Start-Sleep -Seconds 2

$psi = Start-Process -FilePath (Join-Path $root 'python_embeded\python.exe') `
  -ArgumentList '-s','ComfyUI\main.py','--windows-standalone-build' `
  -WorkingDirectory $root -NoNewWindow -Wait `
  -RedirectStandardOutput 'C:\Users\Shadow\ComfyUI\direct_stdout.log' `
  -RedirectStandardError 'C:\Users\Shadow\ComfyUI\direct_stderr.log'
Write-Output ("DIRECT RUN exit=" + $psi.ExitCode)
