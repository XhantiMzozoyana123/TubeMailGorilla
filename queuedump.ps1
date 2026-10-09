$f = 'C:\Users\Shadow\Documents\TubeMailGorilla\TubeMailGorilla.Maui.Unlocked\Services\ComfyUiImageGenerationService.cs'
$lines = ([System.IO.File]::ReadAllText($f)) -split "`r`n"
$stamp = Get-Date -Format 'HHmmss_fff'
$o = @()
$o += '=== lines 96-158 (upload/queue head) ==='
$o += ($lines[95..157] -join "`n")
$o += '=== lines 250-280 (queue tail) ==='
$o += ($lines[249..279] -join "`n")
$o -join "`n" | Set-Content ("C:\Users\Shadow\Documents\TubeMailGorilla\queue_" + $stamp + ".cs") -Encoding utf8
Write-Output ("QUEUE " + $stamp)
