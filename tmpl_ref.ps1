$r = @()
foreach ($pair in @(
    @('c:\Users\Shadow\Documents\TubeMailGorilla\TubeMailGorilla.Maui.Unlocked\Views\SendEmailsPage.xaml', 'XAML'),
    @('c:\Users\Shadow\Documents\TubeMailGorilla\TubeMailGorilla.Maui.Unlocked\Views\SendEmailsPage.xaml.cs', 'CS'))) {
    $lines = Get-Content $pair[0]
    $r += ('=== ' + $pair[1] + ' ===')
    for ($i = 0; $i -lt $lines.Count; $i++) {
        if ($lines[$i] -match '[Tt]emplate') { $r += (($i + 1).ToString() + ': ' + $lines[$i].Trim()) }
    }
}
$r | Set-Content 'c:\Users\Shadow\Documents\TubeMailGorilla\tmpl_ref_63.txt' -Encoding utf8
Write-Output ('lines=' + $r.Count)
