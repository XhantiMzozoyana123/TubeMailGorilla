$f = 'c:\Users\Shadow\Documents\TubeMailGorilla\TubeMailGorilla.Maui.Unlocked\Services\AIService.cs'
$lines = Get-Content $f
$out = @()
for ($i = 79; $i -lt [Math]::Min(210, $lines.Count); $i++) { $out += (($i + 1).ToString() + ' | ' + $lines[$i]) }
$out | Set-Content 'c:\Users\Shadow\Documents\TubeMailGorilla\aisvc_dump_46.txt' -Encoding utf8
Write-Output 'DUMPED'
