$o = @()
$log = 'C:\Users\Shadow\ComfyUI\torch_swap.log'
$raw = Get-Content $log -Raw
$o += ('swap_complete=' + ($raw -match 'TORCH SWAP COMPLETE'))
$o += ('swap_tail:')
$o += (Get-Content $log -Tail 4)
$proc = Get-Process python* -ErrorAction SilentlyContinue
$o += ('python_running=' + [bool]$proc)
$cuda = Test-Path 'C:\Users\Shadow\ComfyUI\cuda_result.txt'
$o += ('cuda_result_exists=' + $cuda)
$o | Set-Content 'C:\Users\Shadow\Documents\TubeMailGorilla\progress.txt'
Write-Output 'STATUS DONE'
