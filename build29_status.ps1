$r = @()
$pid29 = (Get-Content 'c:\Users\Shadow\Documents\TubeMailGorilla\build_29.pid' -ErrorAction SilentlyContinue)
if ($pid29) {
    $running = Get-Process -Id ([int]$pid29) -ErrorAction SilentlyContinue
    $r += ('build pid=' + $pid29 + ' running = ' + ($null -ne $running))
} else { $r += 'pid file missing' }
$log = Get-Content 'c:\Users\Shadow\Documents\TubeMailGorilla\build_29.log' -ErrorAction SilentlyContinue
if ($log) {
    $hits = $log | Select-String 'Error\(s\)|Warning\(s\)|error CS'
    foreach ($h in $hits) { $r += $h.Line }
} else { $r += 'log missing' }
$r | Set-Content 'c:\Users\Shadow\Documents\TubeMailGorilla\build29_status_62.txt' -Encoding utf8
Write-Output ($r -join ' | ')
