$py = 'C:\Users\Shadow\ComfyUI\ComfyUI_windows_portable\python_embeded\python.exe'
$log = 'C:\Users\Shadow\ComfyUI\torch_swap.log'
Set-Content $log 'TORCH SWAP START'
function L($m){ Add-Content $log ("[{0}] {1}" -f (Get-Date -Format HH:mm:ss), $m) }

# Remove the CUDA 13.0 build first so cu124 libs don't collide.
L 'uninstalling cu130 torch trio'
& $py -m pip uninstall -y torch torchvision torchaudio 2>&1 | Out-Null
L 'uninstall done, installing cu124 trio'

# Force cu124 index; --no-cache-dir avoids stale wheels; --upgrade handles any residue.
& $py -m pip install --no-cache-dir --index-url https://download.pytorch.org/whl/cu124 `
     'torch==2.6.0' 'torchvision==0.21.0' 'torchaudio==2.6.0' 2>&1 |
     ForEach-Object { Add-Content $log $_ }
L ("INSTALL EXIT=" + $LASTEXITCODE)
L 'TORCH SWAP COMPLETE'
