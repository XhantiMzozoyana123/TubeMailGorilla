$py = 'C:\Users\Shadow\ComfyUI\ComfyUI_windows_portable\python_embeded\python.exe'
$res = 'C:\Users\Shadow\Documents\TubeMailGorilla\cuda_out_9.txt'
$o = @()
$o += ('torch ver: ' + (& $py -c "import torch;print(torch.__version__)" 2>&1))
$o += ('torch cuda: ' + (& $py -c "import torch;print(torch.version.cuda)" 2>&1))
$o += '--- device_count ---'
$o += (& $py -c "import torch;print(torch.cuda.device_count())" 2>&1)
$o += '--- is_available ---'
$o += (& $py -c "import torch;print(torch.cuda.is_available())" 2>&1)
$o | Set-Content $res
Write-Output 'CUDA3 DONE'
