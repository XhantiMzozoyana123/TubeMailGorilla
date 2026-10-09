$py = 'C:\Users\Shadow\ComfyUI\ComfyUI_windows_portable\python_embeded\python.exe'
Remove-Item 'C:\Users\Shadow\ComfyUI\cuda_result.txt' -ErrorAction SilentlyContinue
& $py 'C:\Users\Shadow\ComfyUI\cuda_test.py' 2>&1 | Out-Null
if (Test-Path 'C:\Users\Shadow\ComfyUI\cuda_result.txt') {
  Copy-Item 'C:\Users\Shadow\ComfyUI\cuda_result.txt' 'C:\Users\Shadow\Documents\TubeMailGorilla\progress.txt'
} else { 'NO CUDA RESULT' | Set-Content 'C:\Users\Shadow\Documents\TubeMailGorilla\progress.txt' }
Write-Output 'CUDATEST DONE'
