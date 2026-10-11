$p = Get-Process -Id 15456 -ErrorAction SilentlyContinue
$r = @()
if ($p) { $r += "pid=$($p.Id) responding=$($p.Responding) title=[$($p.MainWindowTitle)]" } else { $r += 'NOT RUNNING' }
$r | Out-File -Encoding utf8 'c:\Users\Shadow\Documents\TubeMailGorilla\apprun_check.txt'
'checked'
