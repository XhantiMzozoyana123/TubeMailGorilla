$p = Get-Process 7zr -ErrorAction SilentlyContinue
$o = @()
$o += ('7zr_running=' + [bool]$p)
if ($p) { $o += ('7zr_cpu=' + [math]::Round($p.CPU,1)) }
$ex = 'C:\Users\Shadow\ComfyUI\ComfyUI_windows_portable'
if (Test-Path $ex) {
  $cnt = (Get-ChildItem $ex -Recurse -File -ErrorAction SilentlyContinue).Count
  $o += ('extracted_files=' + $cnt)
  $runBat = Join-Path $ex 'run_nvidia_gpu.bat'
  $o += ('run_bat_exists=' + (Test-Path $runBat))
} else { $o += 'extracted_dir=missing' }
$o | Set-Content 'C:\Users\Shadow\Documents\TubeMailGorilla\progress.txt'
Write-Output 'POLL2 DONE'
