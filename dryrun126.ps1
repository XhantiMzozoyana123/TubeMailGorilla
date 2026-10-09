$py = 'C:\Users\Shadow\ComfyUI\ComfyUI_windows_portable\python_embeded\python.exe'
$o = @()
$o += '--- DRYRUN torch 2.14 cu126 trio ---'
$o += (& $py -m pip install --dry-run 'torch==2.14.0' 'torchvision==0.29.0' 'torchaudio==2.11.0' --index-url https://download.pytorch.org/whl/cu126 2>&1 | Select-Object -Last 20)
$o | Set-Content 'C:\Users\Shadow\Documents\TubeMailGorilla\dryrun126_15.txt'
Write-Output 'DRYRUN126 DONE'
