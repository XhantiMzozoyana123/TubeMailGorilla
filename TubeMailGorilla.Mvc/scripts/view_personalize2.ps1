param()
$p = 'C:/Users/Shadow/Documents/TubeMailGorilla/TubeMailGorilla.Mvc/Services/EmailService.cs'
$lines = Get-Content $p
for ($i = 175; $i -le 215; $i++) {
    Write-Output ('LINE ' + $i + ' : ' + $lines[$i])
}
