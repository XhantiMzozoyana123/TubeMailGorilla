param()
$p = 'C:/Users/Shadow/Documents/TubeMailGorilla/TubeMailGorilla.Mvc/Services/EmailService.cs'
$lines = Get-Content $p
for ($i = 153; $i -le 165; $i++) {
    Write-Output ('LINE ' + $i + ' : ' + $lines[$i])
}
