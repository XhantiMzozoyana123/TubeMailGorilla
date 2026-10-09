$root = 'C:\Users\Shadow\ComfyUI\ComfyUI_windows_portable'
$py = Join-Path $root 'python_embeded\python.exe'
Remove-Item 'C:\Users\Shadow\ComfyUI\torch_diag_result.txt' -ErrorAction SilentlyContinue
& $py 'C:\Users\Shadow\ComfyUI\torch_diag2.py' 2>&1 | Out-Null
if (Test-Path 'C:\Users\Shadow\ComfyUI\torch_diag_result.txt') {
  Copy-Item 'C:\Users\Shadow\ComfyUI\torch_diag_result.txt' 'C:\Users\Shadow\Documents\TubeMailGorilla\progress.txt'
} else {
  'NO RESULT FILE' | Set-Content 'C:\Users\Shadow\Documents\TubeMailGorilla\progress.txt'
}
Write-Output 'TORCHDIAG2 DONE'
