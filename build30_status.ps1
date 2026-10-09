$r = @()
$pid30 = (Get-Content 'c:\Users\Shadow\Documents\TubeMailGorilla\build_30.pid' -ErrorAction SilentlyContinue)
if ($pid30) {
    $running = Get-Process -Id ([int]$pid30) -ErrorAction SilentlyContinue
    $r += ('build pid=' + $pid30 + ' running = ' + ($null -ne $running))
} else { $r += 'pid file missing' }
$log = Get-Content 'c:\Users\Shadow\Documents\TubeMailGorilla\build_30.log' -ErrorAction SilentlyContinue
if ($log) {
    $hits = $log | Select-String 'Error\(s\)|Warning\(s\)|error CS'
    foreach ($h in $hits) { $r += $h.Line }
} else { $r += 'log missing' }
$r | Set-Content 'c:\Users\Shadow\Documents\TubeMailGorilla\build30_status_64.txt' -Encoding utf8
Write-Output ($r -join ' | ')
