$r = @()
$running = Get-Process -Id 20796 -ErrorAction SilentlyContinue
$r += ('build running = ' + ($null -ne $running))
$log = Get-Content 'c:\Users\Shadow\Documents\TubeMailGorilla\build_26.log' -ErrorAction SilentlyContinue
if ($log) {
    $hits = $log | Select-String 'Error\(s\)|Warning\(s\)|error CS'
    foreach ($h in $hits) { $r += $h.Line }
} else { $r += 'log missing' }
$r | Set-Content 'c:\Users\Shadow\Documents\TubeMailGorilla\build26_status_52.txt' -Encoding utf8
Write-Output ($r -join ' | ')
