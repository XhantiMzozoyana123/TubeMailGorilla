param()
$p = 'C:/Users/Shadow/Documents/TubeMailGorilla/TubeMailGorilla.Mvc/Controllers/ContactsController.cs'
$lines = Get-Content $p
for ($i = 400; $i -le 480; $i++) {
    Write-Output ('LINE ' + $i + ' : ' + $lines[$i])
}
