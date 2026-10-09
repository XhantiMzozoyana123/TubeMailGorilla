$r = @()
$r += '=== Views ==='
$r += (Get-ChildItem 'c:\Users\Shadow\Documents\TubeMailGorilla\TubeMailGorilla.Maui.Unlocked\Views' -Filter '*.xaml' | Select-Object -ExpandProperty Name)
$r += '=== ContactDetailsPage.xaml token lines ==='
$hits = Select-String -Path 'c:\Users\Shadow\Documents\TubeMailGorilla\TubeMailGorilla.Maui.Unlocked\Views\ContactDetailsPage.xaml' -Pattern 'Token'
foreach ($h in $hits) { $r += ($h.LineNumber.ToString() + ': ' + $h.Line.Trim()) }
$r | Set-Content 'c:\Users\Shadow\Documents\TubeMailGorilla\tokeninfo_58.txt' -Encoding utf8
Write-Output ('lines=' + $r.Count)
