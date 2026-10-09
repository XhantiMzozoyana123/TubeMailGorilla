$f = 'C:\Users\Shadow\ComfyUI\dl\ComfyUI_windows_portable_nvidia.7z'
$m = 'C:\Users\Shadow\ComfyUI\dl\v1-5-pruned-emaonly.safetensors'
$o = @()
if (Test-Path $f) { $o += ('comfyui_7z_MB=' + [math]::Round((Get-Item $f).Length/1MB,1)) } else { $o += 'comfyui_7z=none' }
if (Test-Path $m) { $o += ('model_MB=' + [math]::Round((Get-Item $m).Length/1MB,1)) } else { $o += 'model=none' }
$o += '---LOG TAIL---'
if (Test-Path 'C:\Users\Shadow\ComfyUI\setup.log') { $o += (Get-Content 'C:\Users\Shadow\ComfyUI\setup.log' -Tail 6) }
$o | Set-Content 'C:\Users\Shadow\Documents\TubeMailGorilla\progress.txt'
Write-Output 'POLL DONE'
