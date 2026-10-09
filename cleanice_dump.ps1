$root = 'c:\Users\Shadow\Documents\TubeMailGorilla'
$r = @()
# CleanIcebreaker definition + surrounding lines
$f = Join-Path $root 'TubeMailGorilla.Maui.Unlocked\Services\AIService.cs'
$lines = Get-Content $f
for ($i = 0; $i -lt $lines.Count; $i++) {
    if ($lines[$i] -match 'CleanIcebreaker\(string') {
        for ($j = $i; $j -lt [Math]::Min($i + 45, $lines.Count); $j++) { $r += (($j + 1).ToString() + ' | ' + $lines[$j]) }
    }
}
$r += '=== ollama process ==='
$op = Get-Process ollama -ErrorAction SilentlyContinue
if ($op) { $r += ('pid=' + $op.Id + ' path=' + $op.Path) } else { $r += 'ollama process NOT found' }
$r | Set-Content (Join-Path $root 'cleanice_47.txt') -Encoding utf8
Write-Output 'DONE'
