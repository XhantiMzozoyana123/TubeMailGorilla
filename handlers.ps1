$ErrorActionPreference = 'Continue'
$c = Get-Content 'c:\Users\Shadow\Documents\TubeMailGorilla\TubeMailGorilla.Maui.Unlocked\Views\ContactsPage.xaml.cs'
$out = @()
$out += "total lines: $($c.Count)"
for ($i = 0; $i -lt $c.Count; $i++) {
  if ($c[$i] -match 'OnImportCsvClicked|OnExportCsvClicked|OnAddContactClicked|ContactCsvService|private async void|private void|public partial') {
    $out += "$($i + 1): $($c[$i].Trim())"
  }
}
$out | Out-File -Encoding utf8 'c:\Users\Shadow\Documents\TubeMailGorilla\handlers.txt'
'wrote handlers.txt'
