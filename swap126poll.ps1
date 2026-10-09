$log = 'C:\Users\Shadow\ComfyUI\torch_swap126.log'
$o = @()
$raw = Get-Content $log -Raw -ErrorAction SilentlyContinue
$o += ('complete=' + ($raw -match 'TORCH SWAP 126 COMPLETE'))
$o += (Get-Content $log -Tail 6 -ErrorAction SilentlyContinue)
$o | Set-Content 'C:\Users\Shadow\Documents\TubeMailGorilla\swap126_status_16.txt'
Write-Output 'SWAP126POLL DONE'
