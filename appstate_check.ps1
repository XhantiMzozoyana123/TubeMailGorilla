$p = Get-Process -Id 39664 -ErrorAction SilentlyContinue
if ($p) {
    $msg = 'ALIVE=' + $p.Responding + ' WINDOW=[' + $p.MainWindowTitle + '] MEM_MB=' + [int]($p.WorkingSet64 / 1MB)
} else {
    $msg = 'DEAD - exited during startup'
}
$msg | Set-Content 'c:\Users\Shadow\Documents\TubeMailGorilla\appstate_36.txt' -Encoding utf8
Write-Output $msg
