$ErrorActionPreference = 'Continue'
$svc = 'c:\Users\Shadow\Documents\TubeMailGorilla\TubeMailGorilla.Maui.Unlocked\Services\ContactCsvService.cs'
$pg = 'c:\Users\Shadow\Documents\TubeMailGorilla\TubeMailGorilla.Maui.Unlocked\Views\ContactsPage.xaml.cs'
$out = @()
$out += "=== CsvService lines: $((Get-Content $svc).Count) ==="
$out += "open braces: $(([regex]::Matches([IO.File]::ReadAllText($svc), '\{')).Count)  close braces: $(([regex]::Matches([IO.File]::ReadAllText($svc), '\}')).Count)"
$out += "methods: " + ((Select-String -Path $svc -Pattern 'public static .*\(|private static .*\\(' | ForEach-Object { $_.Line.Trim() }) -join "`r`n")
$out += ''
$out += "=== XAML grid rows ==="
$out += ((Select-String -Path 'c:\Users\Shadow\Documents\TubeMailGorilla\TubeMailGorilla.Maui.Unlocked\Views\ContactsPage.xaml' -Pattern 'RowDefinitions|Grid\.Row=|ImportCsv|ExportCsv') | ForEach-Object { "$($_.LineNumber): $($_.Line.Trim())" } -join "`r`n")
$out | Out-File -Encoding utf8 'c:\Users\Shadow\Documents\TubeMailGorilla\csvverify.txt'
'wrote csvverify.txt'
