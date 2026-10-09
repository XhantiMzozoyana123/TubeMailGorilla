$r = @()
$f = 'c:\Users\Shadow\Documents\TubeMailGorilla\TubeMailGorilla.Maui.Unlocked\Views\SendEmailsPage.xaml'
$lines = Get-Content $f
for ($i = 0; $i -lt $lines.Count; $i++) {
    if ($lines[$i] -match 'SubjectTokens|BodyTokens|TokenHint|TokenBar|HorizontalItems|ScrollView') { $r += (($i + 1).ToString() + ': ' + $lines[$i].Trim()) }
}
$r | Set-Content 'c:\Users\Shadow\Documents\TubeMailGorilla\tokenxaml_61.txt' -Encoding utf8
Write-Output ($r -join ' | ')
