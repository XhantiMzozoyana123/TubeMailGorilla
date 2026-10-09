$py = 'C:\Users\Shadow\ComfyUI\ComfyUI_windows_portable\python_embeded\python.exe'
$o = @()
$o += '---CURRENT---'
$o += (& $py -m pip show torch torchvision torchaudio 2>&1 | Select-String '^(Name|Version):')
$o | Set-Content 'C:\Users\Shadow\ComfyUI\pip_before.txt'
Write-Output 'SNAP DONE'
