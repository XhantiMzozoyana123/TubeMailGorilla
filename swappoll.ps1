$log = 'C:\Users\Shadow\ComfyUI\torch_swap.log'
$o = @()
if (Test-Path $log) { $o += (Get-Content $log -Tail 15) } else { $o += 'no swap log yet' }
$o | Set-Content 'C:\Users\Shadow\Documents\TubeMailGorilla\progress.txt'
Write-Output 'SWAPPOLL DONE'
