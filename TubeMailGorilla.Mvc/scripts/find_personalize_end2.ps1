param()
$p = 'C:/Users/Shadow/Documents/TubeMailGorilla/TubeMailGorilla.Mvc/Services/EmailService.cs'
$lines = Get-Content $p
Write-Output ('TOTAL_LINES=' + $lines.Count)
# Find the end of Personalize
for ($i = 322; $i -lt $lines.Count; $i++) {
    $line = $lines[$i]
    if ($line -match '^\s*}$') {
        Write-Output ('PERSONALIZE_END_AT_LINE ' + $i)
        break
    }
}
# Print lines 330-360
for ($i = 330; $i -le 360; $i++) {
    Write-Output ('LINE ' + $i + ' : ' + $lines[$i])
}
