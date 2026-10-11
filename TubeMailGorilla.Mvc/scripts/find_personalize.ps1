param()
$p = 'C:/Users/Shadow/Documents/TubeMailGorilla/TubeMailGorilla.Mvc/Services/EmailService.cs'
$raw = Get-Content $p -Raw
$lines = $raw.Split([Environment]::NewLine)
for ($i = 0; $i -lt $lines.Count; $i++) {
    if ($lines[$i] -match 'Personalize\(string text, EmailContact') {
        Write-Output ('PERSONALIZE_AT_LINE ' + $i)
        # Print 8 lines before and 20 after
        $start = $i - 8
        $end = $i + 20
        if ($start -lt 0) { $start = 0 }
        for ($j = $start; $j -le $end -and $j -lt $lines.Count; $j++) {
            Write-Output ('LINE ' + $j + ' : ' + $lines[$j])
        }
        break
    }
}
