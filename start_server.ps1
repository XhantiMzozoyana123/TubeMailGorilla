$root = 'C:\Users\Shadow\ComfyUI\ComfyUI_windows_portable'
$srvLog = 'C:\Users\Shadow\ComfyUI\server.log'
# Launch the NVIDIA launcher detached, inheriting its working dir so the
# embedded python + relative paths resolve exactly as they do when double-clicked.
$p = Start-Process -FilePath (Join-Path $root 'run_nvidia_gpu.bat') `
     -WorkingDirectory $root -WindowStyle Hidden -PassThru `
     -RedirectStandardOutput $srvLog
$p.Id | Set-Content 'C:\Users\Shadow\ComfyUI\server.pid'
Write-Output ("STARTED pid=" + $p.Id)
