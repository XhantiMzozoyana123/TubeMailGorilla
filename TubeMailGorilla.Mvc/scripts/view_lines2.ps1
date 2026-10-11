param()
$p = 'C:/Users/Shadow/Documents/TubeMailGorilla/TubeMailGorilla.Mvc/Controllers/ContactsController.cs'
$lines = Get-Content $p
for ($i = 320; $i -le 360; $i++) {
    Write-Output ('LINE ' + $i + ' : ' + $lines[$i])
}
