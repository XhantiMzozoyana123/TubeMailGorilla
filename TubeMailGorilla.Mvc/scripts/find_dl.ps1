param()
$p = 'C:/Users/Shadow/Documents/TubeMailGorilla/TubeMailGorilla.Mvc/Controllers/ContactsController.cs'
$raw = Get-Content $p -Raw
$idx = 0
$raw.Split([Environment]::NewLine) | ForEach-Object {
    if ($_ -match 'DownloadSnapshots') {
        Write-Output ('LINE ' + $idx)
    }
    $idx++
}
