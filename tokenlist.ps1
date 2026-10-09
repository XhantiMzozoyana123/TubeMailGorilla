$r = @()
$r += '=== EmailService Personalize fields ==='
$f1 = 'c:\Users\Shadow\Documents\TubeMailGorilla\TubeMailGorilla.Maui.Unlocked\Services\EmailService.cs'
$l1 = Get-Content $f1
for ($i = 0; $i -lt $l1.Count; $i++) {
    if ($l1[$i] -match '\["[a-z_\-]+"\]\s*=') { $r += (($i + 1).ToString() + ': ' + $l1[$i].Trim()) }
}
$r += '=== SendEmailsPage token seeds ==='
$f2 = 'c:\Users\Shadow\Documents\TubeMailGorilla\TubeMailGorilla.Maui.Unlocked\Views\SendEmailsPage.xaml.cs'
if (Test-Path $f2) {
    $l2 = Get-Content $f2
    for ($i = 0; $i -lt $l2.Count; $i++) {
        if ($l2[$i] -match 'AddToken|TokenOption\(|\[snapshot_|\[icebreaker\]|\[f_name\]') { $r += (($i + 1).ToString() + ': ' + $l2[$i].Trim()) }
    }
}
$r | Set-Content 'c:\Users\Shadow\Documents\TubeMailGorilla\tokenlist_60.txt' -Encoding utf8
Write-Output ('lines=' + $r.Count)
