$r = @()
$p = Get-Process -Id 8648 -ErrorAction SilentlyContinue
if ($p) { $r += ('HARNESS RUNNING name=' + $p.Name) } else { $r += 'HARNESS GONE' }
$r += (Get-Content 'c:\Users\Shadow\Documents\TubeMailGorilla\llmcheck_result.txt' -ErrorAction SilentlyContinue)
$r | Set-Content 'c:\Users\Shadow\Documents\TubeMailGorilla\llmpoll_56.txt' -Encoding utf8
Write-Output ($r -join ' | ')
