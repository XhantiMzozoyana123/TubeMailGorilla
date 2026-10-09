$r = @()
foreach ($n in 'TubeMailGorilla.Maui', 'TubeMailGorilla.Maui.Unlocked') {
    $p = Get-Process $n -ErrorAction SilentlyContinue
    if ($p) { $r += ($n + ' ALIVE pid=' + $p.Id + ' responding=' + $p.Responding + ' window=[' + $p.MainWindowTitle + ']') }
    else    { $r += ($n + ' DEAD') }
}
$r | Set-Content 'c:\Users\Shadow\Documents\TubeMailGorilla\bothstate_39.txt' -Encoding utf8
Write-Output ($r -join ' | ')
