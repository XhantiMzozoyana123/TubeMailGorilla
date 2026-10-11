param()
$p = 'C:/Users/Shadow/Documents/TubeMailGorilla/TubeMailGorilla.Mvc/Services/EmailService.cs'
$raw = Get-Content $p -Raw
$lines = $raw.Split([Environment]::NewLine)
for ($i = 0; $i -lt $lines.Count; $i++) {
    if ($lines[$i] -match 'Personalize|ExtractBy|Snapshot|Contains') {
        Write-Output ('LINE ' + $i + ' : ' + $lines[$i])
    }
}
