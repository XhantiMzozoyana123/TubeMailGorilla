param()
$p = 'C:/Users/Shadow/Documents/TubeMailGorilla/TubeMailGorilla.Mvc/Services/EmailService.cs'
$lines = Get-Content $p
for ($i = 243; $i -lt 300; $i++) {
    Write-Output ('LINE ' + $i + ' : ' + $lines[$i])
}
