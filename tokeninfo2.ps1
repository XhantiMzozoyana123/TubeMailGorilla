$r = @()
$f = 'c:\Users\Shadow\Documents\TubeMailGorilla\TubeMailGorilla.Maui.Unlocked\Views\ContactDetailsPage.xaml.cs'
$lines = Get-Content $f
for ($i = 0; $i -lt $lines.Count; $i++) {
    if ($lines[$i] -match 'AddDirectToken\(|OnDirectTokenClicked\(|MergeDirectTokens|custom parameters|GetParametersAsync|OnAppearing') {
        $r += (($i + 1).ToString() + ': ' + $lines[$i].Trim())
    }
}
$r | Set-Content 'c:\Users\Shadow\Documents\TubeMailGorilla\tokeninfo2_59.txt' -Encoding utf8
Write-Output ($r -join ' | ')
