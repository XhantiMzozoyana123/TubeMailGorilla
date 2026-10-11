param()
$p = 'C:/Users/Shadow/Documents/TubeMailGorilla/TubeMailGorilla.Mvc/Controllers/ContactsController.cs'
$lines = Get-Content $p
for ($i = 464; $i -le 540; $i++) {
    Write-Output ('LINE ' + $i + ' : ' + $lines[$i])
}
