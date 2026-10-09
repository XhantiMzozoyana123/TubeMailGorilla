$py = 'C:\Users\Shadow\ComfyUI\ComfyUI_windows_portable\python_embeded\python.exe'
$o = @()
$o += '--- comfy_kitchen metadata ---'
$o += (& $py -m pip show comfy_kitchen 2>&1 | Select-String '^(Name|Version|Requires):')
$meta = 'C:\Users\Shadow\ComfyUI\ComfyUI_windows_portable\python_embeded\Lib\site-packages\comfy_kitchen-*.dist-info\METADATA'
$mfile = Get-Item $meta -ErrorAction SilentlyContinue | Select-Object -First 1
if ($mfile) {
  $o += '--- Requires-Dist (torch-related) ---'
  $o += (Select-String -Path $mfile.FullName -Pattern 'Requires-Dist' | ForEach-Object { $_.Line } | Where-Object { $_ -match 'torch' })
}
$o | Set-Content 'C:\Users\Shadow\Documents\TubeMailGorilla\ck_meta_13.txt'
Write-Output 'CKMETA DONE'
