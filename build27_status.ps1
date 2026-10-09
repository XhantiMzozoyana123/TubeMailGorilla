$r = @()
$running = Get-Process -Id 21280 -ErrorAction SilentlyContinue
$r += ('build running = ' + ($null -ne $running))
$log = Get-Content 'c:\Users\Shadow\Documents\TubeMailGorilla\build_27.log' -ErrorAction SilentlyContinue
if ($log) {
    $hits = $log | Select-String 'Error\(s\)|Warning\(s\)|error CS'
    foreach ($h in $hits) { $r += $h.Line }
} else { $r += 'log missing' }
$r | Set-Content 'c:\Users\Shadow\Documents\TubeMailGorilla\build27_status_54.txt' -Encoding utf8
Write-Output ($r -join ' | ')
