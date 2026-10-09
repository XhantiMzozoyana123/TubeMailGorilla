$p = Get-Process 'TubeMailGorilla.Maui.Unlocked' -ErrorAction SilentlyContinue
if ($p) {
    $msg = 'Unlocked ALIVE pid=' + $p.Id + ' responding=' + $p.Responding + ' window=[' + $p.MainWindowTitle + '] mem_mb=' + [int]($p.WorkingSet64 / 1MB)
} else {
    $msg = 'Unlocked DEAD - exited during startup'
}
$msg | Set-Content 'c:\Users\Shadow\Documents\TubeMailGorilla\ensure_43.txt' -Encoding utf8
Write-Output $msg
