$log = 'C:\Users\Shadow\ComfyUI\torch_swap.log'
$o = @()
$o += '---SWAP TAIL---'
$o += (Get-Content $log -Tail 6)
$o += '---SWAP DONE?---'
$o += ('contains_complete=' + ((Get-Content $log -Raw) -match 'TORCH SWAP COMPLETE'))
$o | Set-Content 'C:\Users\Shadow\Documents\TubeMailGorilla\progress.txt'
Write-Output 'CHECK DONE'
