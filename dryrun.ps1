$py = 'C:\Users\Shadow\ComfyUI\ComfyUI_windows_portable\python_embeded\python.exe'
$o = @()
$o += ('PIP: ' + (& $py -m pip --version 2>&1))
$o += '---DRYRUN torch 2.6.0 cu124---'
$o += (& $py -m pip install --dry-run 'torch==2.6.0' 'torchvision==0.21.0' 'torchaudio==2.6.0' --index-url https://download.pytorch.org/whl/cu124 2>&1 | Select-Object -Last 25)
$o | Set-Content 'C:\Users\Shadow\Documents\TubeMailGorilla\progress.txt'
Write-Output 'DRYRUN DONE'
