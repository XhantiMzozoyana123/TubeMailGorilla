$py = 'C:\Users\Shadow\ComfyUI\ComfyUI_windows_portable\python_embeded\python.exe'
Remove-Item 'C:\Users\Shadow\Documents\TubeMailGorilla\cuda_out_10.txt' -ErrorAction SilentlyContinue
$p = Start-Process -FilePath $py -ArgumentList 'C:\Users\Shadow\ComfyUI\cuda_test3.py' -WindowStyle Hidden -PassThru
$p.Id | Set-Content 'C:\Users\Shadow\ComfyUI\cudatest.pid'
Write-Output ("CUDA TEST LAUNCHED pid=" + $p.Id)
