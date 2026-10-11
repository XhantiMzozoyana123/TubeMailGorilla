param()
$p = 'C:/Users/Shadow/Documents/TubeMailGorilla/TubeMailGorilla.Mvc/Controllers/ContactsController.cs'
$lines = Get-Content $p
for ($i = 225; $i -le 240; $i++) {
    Write-Output ('LINE ' + $i + ' : ' + $lines[$i])
}
Write-Output '---'
for ($i = 300; $i -le 320; $i++) {
    Write-Output ('LINE ' + $i + ' : ' + $lines[$i])
}
