$ErrorActionPreference = 'Continue'
$out = @()
$r = Select-String -Path 'c:\Users\Shadow\Documents\TubeMailGorilla\TubeMailGorilla.Maui.Unlocked\Views\ContactsPage.xaml' -Pattern 'Import|Export|RowDefinitions|OnAddContact'
$out += '=== XAML refs ==='
$out += (@($r | ForEach-Object { "$($_.LineNumber): $($_.Line.Trim())" }) -join "`r`n")
$c = Select-String -Path 'c:\Users\Shadow\Documents\TubeMailGorilla\TubeMailGorilla.Maui.Unlocked\Views\ContactsPage.xaml.cs' -Pattern 'OnImportCsv|OnExportCsv|ContactCsvService|_isImportingCsv|_isExportingCsv|private.*OnAddContact'
$out += ''
$out += '=== code-behind refs ==='
$out += (@($c | ForEach-Object { "$($_.LineNumber): $($_.Line.Trim())" }) -join "`r`n")
$s = Select-String -Path 'c:\Users\Shadow\Documents\TubeMailGorilla\TubeMailGorilla.Maui.Unlocked\Services\ContactCsvService.cs' -Pattern 'public static'
$out += ''
$out += '=== service methods ==='
$out += (@($s | ForEach-Object { "$($_.LineNumber): $($_.Line.Trim())" }) -join "`r`n")
$out += ''
$out += "=== service lines: $((Get-Content 'c:\Users\Shadow\Documents\TubeMailGorilla\TubeMailGorilla.Maui.Unlocked\Services\ContactCsvService.cs').Count) ==="
$out | Out-File -Encoding utf8 'c:\Users\Shadow\Documents\TubeMailGorilla\csvstate2.txt'
'wrote csvstate2.txt'
