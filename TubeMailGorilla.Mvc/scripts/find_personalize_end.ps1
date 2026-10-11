param()
$p = 'C:/Users/Shadow/Documents/TubeMailGorilla/TubeMailGorilla.Mvc/Services/EmailService.cs'
$lines = Get-Content $p
# Find the end of Personalize (the closing brace at line 322 + offset)
for ($i = 322; $i -lt $lines.Count; $i++) {
    $line = $lines[$i]
    if ($line -match '^\s*}$') {
        Write-Output ('PERSONALIZE_END_AT_LINE ' + $i)
        break
    }
}
# Also print lines 350-380 to understand the file structure
for ($i = 350; $i -le 380; $i++) {
    Write-Output ('LINE ' + $i + ' : ' + $lines[$i])
}
