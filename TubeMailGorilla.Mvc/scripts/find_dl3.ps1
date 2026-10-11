param()
$p = 'C:/Users/Shadow/Documents/TubeMailGorilla/TubeMailGorilla.Mvc/Controllers/ContactsController.cs'
$lines = Get-Content $p
$count = 0
foreach ($l in $lines) {
    $count++
    if ($l -match 'DownloadSnapshots') {
        Write-Output ('MATCH at line ' + $count + ' : ' + $l)
    }
    if ($count -gt 470) { break }
}
Write-Output ('TOTAL=' + $count)
