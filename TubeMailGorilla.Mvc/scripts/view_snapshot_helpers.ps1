param()
$p = 'C:/Users/Shadow/Documents/TubeMailGorilla/TubeMailGorilla.Mvc/Services/EmailService.cs'
$lines = Get-Content $p
for ($i = 215; $i -le 305; $i++) {
    Write-Output ('LINE ' + $i + ' : ' + $lines[$i])
}
