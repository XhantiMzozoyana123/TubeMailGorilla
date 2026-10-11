param()
# Read the MAUI EmailService.cs and print lines 810-1160
$p = 'C:/Users/Shadow/Documents/TubeMailGorilla/TubeMailGorilla.Maui.Unlocked/Services/EmailService.cs'
$lines = Get-Content $p
for ($i = 810; $i -le 1160; $i++) {
    if ($i -le $lines.Count) {
        Write-Output ('LINE ' + $i + ' : ' + $lines[$i - 1])
    }
}
