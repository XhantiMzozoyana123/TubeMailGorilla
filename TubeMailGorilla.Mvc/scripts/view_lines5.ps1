param()
$p = 'C:/Users/Shadow/Documents/TubeMailGorilla/TubeMailGorilla.Mvc/Controllers/ContactsController.cs'
$lines = Get-Content $p
for ($i = 455; $i -le 520; $i++) {
    Write-Output ('LINE ' + $i + ' : ' + $lines[$i])
}
