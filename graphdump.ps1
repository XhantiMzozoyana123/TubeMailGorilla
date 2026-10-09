$f = 'C:\Users\Shadow\Documents\TubeMailGorilla\TubeMailGorilla.Maui.Unlocked\Services\ComfyUiImageGenerationService.cs'
$lines = ([System.IO.File]::ReadAllText($f)) -split "`r`n"
$seg = $lines[158..249] -join "`n"
$stamp = Get-Date -Format 'HHmmss_fff'
$seg | Set-Content ("C:\Users\Shadow\Documents\TubeMailGorilla\graph_" + $stamp + ".cs") -Encoding utf8
Write-Output ("GRAPH " + $stamp)
