param()
$p = 'C:/Users/Shadow/Documents/TubeMailGorilla/TubeMailGorilla.Mvc/Services/EmailService.cs'
$lines = Get-Content $p
for ($i = 210; $i -le 272; $i++) {
    Write-Output ('LINE ' + $i + ' : ' + $lines[$i])
}
