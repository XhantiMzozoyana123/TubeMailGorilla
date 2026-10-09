$root = 'C:\Users\Shadow\ComfyUI\ComfyUI_windows_portable'
$o = @()
$o += ('root_exists=' + (Test-Path $root))
$bat = Get-ChildItem $root -Filter '*.bat' -ErrorAction SilentlyContinue | Select-Object -ExpandProperty FullName
$o += ('BATS: ' + ($bat -join ' | '))
$main = Join-Path $root 'ComfyUI\main.py'
$o += ('main_py=' + (Test-Path $main))
$py = Join-Path $root 'python_embeded\python.exe'
$o += ('embedded_py=' + (Test-Path $py))
$o | Set-Content 'C:\Users\Shadow\Documents\TubeMailGorilla\progress.txt'
Write-Output 'POLL4 DONE'
