$root = 'c:\Users\Shadow\Documents\TubeMailGorilla'
$o = @()
$o += '=== ContactDetailsPage.xaml.cs ==='
$o += (git -C $root diff -- 'TubeMailGorilla.Maui.Unlocked/Views/ContactDetailsPage.xaml.cs')
$o += '=== SendEmailsPage.xaml.cs ==='
$o += (git -C $root diff -- 'TubeMailGorilla.Maui.Unlocked/Views/SendEmailsPage.xaml.cs')
$o | Set-Content ($root + '\caller_diff_27.txt') -Encoding utf8
Write-Output 'DONE'
