$r = @()
$h = Get-Process -Id 22064 -ErrorAction SilentlyContinue
$r += ('harness pid22064 alive = ' + ($null -ne $h))
$procs = Get-Process ollama*, *llama* -ErrorAction SilentlyContinue
foreach ($p in $procs) {
    $cpu1 = $p.CPU
    Start-Sleep -Seconds 2
    $p.Refresh()
    $r += ($p.Name + ' pid=' + $p.Id + ' cpu_delta_2s=' + [Math]::Round($p.CPU - $cpu1, 2) + ' mem_mb=' + [int]($p.WorkingSet64 / 1MB))
}
$r += ('gpu=' + ((& nvidia-smi --query-gpu=utilization.gpu,memory.used --format=csv,noheader 2>&1) -join ' '))
$res = Get-Content 'c:\Users\Shadow\Documents\TubeMailGorilla\llmcheck_result.txt' -ErrorAction SilentlyContinue
$r += '--- llmcheck so far ---'
$r += $res
$r | Set-Content 'c:\Users\Shadow\Documents\TubeMailGorilla\diag2_53.txt' -Encoding utf8
Write-Output ($r -join ' | ')
