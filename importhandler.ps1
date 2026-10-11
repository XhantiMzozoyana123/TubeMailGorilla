$ErrorActionPreference = 'Continue'
$c = Get-Content 'c:\Users\Shadow\Documents\TubeMailGorilla\TubeMailGorilla.Maui.Unlocked\Views\ContactsPage.xaml.cs'
$out = @()
$out += '=== lines 327-427 ==='
for ($i = 326; $i -lt 427; $i++) { $out += "$($i + 1): $($c[$i])" }
$out | Out-File -Encoding utf8 'c:\Users\Shadow\Documents\TubeMailGorilla\importhandler.txt'
'wrote importhandler.txt'
