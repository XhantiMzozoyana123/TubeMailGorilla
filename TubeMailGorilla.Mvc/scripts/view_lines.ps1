param()
$p = 'C:/Users/Shadow/Documents/TubeMailGorilla/TubeMailGorilla.Mvc/Controllers/ContactsController.cs'
$lines = Get-Content $p
for ($i = 233; $i -le 245; $i++) {
    Write-Output ('LINE ' + $i + ' : ' + $lines[$i])
}
