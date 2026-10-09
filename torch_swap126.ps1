$py = 'C:\Users\Shadow\ComfyUI\ComfyUI_windows_portable\python_embeded\python.exe'
$log = 'C:\Users\Shadow\ComfyUI\torch_swap126.log'
Set-Content $log 'TORCH SWAP 126 START'
function L($m){ Add-Content $log ("[{0}] {1}" -f (Get-Date -Format HH:mm:ss), $m) }

L 'uninstalling cu124 torch trio'
& $py -m pip uninstall -y torch torchvision torchaudio 2>&1 | Out-Null
L 'uninstall done, installing 2.14 cu126 trio'
& $py -m pip install --no-cache-dir --index-url https://download.pytorch.org/whl/cu126 `
     'torch==2.14.0' 'torchvision==0.29.0' 'torchaudio==2.11.0' 2>&1 |
     ForEach-Object { Add-Content $log $_ }
L ("INSTALL EXIT=" + $LASTEXITCODE)
L 'TORCH SWAP 126 COMPLETE'
