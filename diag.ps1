$r = @()
$a = (Get-Process -Id 18088 -ErrorAction SilentlyContinue)
if ($a) {
    $cpu1 = $a.CPU
    Start-Sleep -Seconds 3
    $a.Refresh()
    $r += ('llama-server cpu_delta_3s=' + [Math]::Round($a.CPU - $cpu1, 2) + 's mem_mb=' + [int]($a.WorkingSet64 / 1MB))
} else { $r += 'llama-server GONE' }
$w = Get-Process -Id 20948 -ErrorAction SilentlyContinue
$r += ('test worker alive=' + ($null -ne $w))
$f = 'c:\Users\Shadow\Documents\TubeMailGorilla\ollama_test2_48.txt'
$r += ('result file=' + (Test-Path $f))
$r += ('gpu=' + ((& nvidia-smi --query-gpu=utilization.gpu,memory.used --format=csv,noheader 2>&1) -join ' '))
$r | Set-Content 'c:\Users\Shadow\Documents\TubeMailGorilla\diag_49.txt' -Encoding utf8
Write-Output ($r -join ' | ')
