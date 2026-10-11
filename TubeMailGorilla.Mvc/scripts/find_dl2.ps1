param()
$p = 'C:/Users/Shadow/Documents/TubeMailGorilla/TubeMailGorilla.Mvc/Controllers/ContactsController.cs'
$raw = Get-Content $p -Raw
$lines = $raw.Split([Environment]::NewLine)
for ($i = 0; $i -lt $lines.Count; $i++) {
    if ($lines[$i] -match 'DownloadSnapshots') {
        Write-Output ('LINE ' + $i + ' : ' + $lines[$i])
    }
}
Write-Output ('TOTAL_LINES=' + $lines.Count)
