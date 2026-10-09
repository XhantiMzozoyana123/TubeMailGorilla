$root = 'C:\Users\Shadow\ComfyUI\ComfyUI_windows_portable'
$py = Join-Path $root 'python_embeded\python.exe'
$script = @'
import sys
print("py", sys.version)
try:
    import torch
    print("torch", torch.__version__)
    print("torch.version.cuda", torch.version.cuda)
    print("cudnn", torch.backends.cudnn.version())
    print("device_count", torch.cuda.device_count())
    print("is_available", torch.cuda.is_available())
    try:
        torch.cuda.init()
        print("init OK")
    except Exception as e:
        print("INIT ERR:", repr(e))
except Exception as e:
    import traceback; traceback.print_exc()
'@
$tmp = 'C:\Users\Shadow\ComfyUI\torch_diag.py'
$script | Set-Content -Path $tmp -Encoding UTF8
$out = & $py $tmp 2>&1
$out | Set-Content 'C:\Users\Shadow\Documents\TubeMailGorilla\progress.txt'
Write-Output 'TORCHDIAG DONE'
