param()
# Resolve the MVC EmailService path and print its actual line count
$p = 'C:/Users/Shadow/Documents/TubeMailGorilla/TubeMailGorilla.Mvc/Services/EmailService.cs'
$lines = Get-Content $p
Write-Output ('MVC EMAILSVC TOTAL_LINES=' + $lines.Count)
for ($i = 0; $i -lt $lines.Count; $i++) {
    if ($lines[$i] -match 'Personalize') {
        Write-Output ('PERSONALIZE_AT_LINE ' + $i)
    }
}
# Print lines 300-370 (should show what's actually there)
for ($i = 300; $i -lt 370; $i++) {
    Write-Output ('LINE ' + $i + ' : ' + $lines[$i])
}
