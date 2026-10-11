param()
$p = 'C:/Users/Shadow/Documents/TubeMailGorilla/TubeMailGorilla.Mvc/Services/EmailService.cs'
$lines = Get-Content $p
for ($i = 145; $i -le 175; $i++) {
    Write-Output ('LINE ' + $i + ' : ' + $lines[$i])
}
