$py = 'C:\Users\Shadow\ComfyUI\ComfyUI_windows_portable\python_embeded\python.exe'
$o = @()
$o += '--- ComfyUI requirements.txt (torch-related) ---'
$req = 'C:\Users\Shadow\ComfyUI\ComfyUI_windows_portable\ComfyUI\requirements.txt'
if (Test-Path $req) {
  $o += (Select-String -Path $req -Pattern 'torch|comfy' | ForEach-Object { $_.Line })
} else { $o += 'no requirements.txt' }
$o += '--- installed comfy_kitchen Requires-Dist (all) ---'
$mfile = Get-Item 'C:\Users\Shadow\ComfyUI\ComfyUI_windows_portable\python_embeded\Lib\site-packages\comfy_kitchen-*.dist-info\METADATA' -ErrorAction SilentlyContinue | Select-Object -First 1
if ($mfile) { $o += (Select-String -Path $mfile.FullName -Pattern 'Requires-Dist' | ForEach-Object { $_.Line }) }
$o += '--- torch 2.7 cu126 availability for py3.13 ---'
$o += (& $py -m pip index versions torch --index-url https://download.pytorch.org/whl/cu126 2>&1 | Select-Object -First 3)
$o | Set-Content 'C:\Users\Shadow\Documents\TubeMailGorilla\comfy_req_14.txt'
Write-Output 'REQ DONE'
