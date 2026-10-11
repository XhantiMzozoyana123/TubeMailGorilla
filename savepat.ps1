$ErrorActionPreference = 'Continue'
$hits = Select-String -Path 'c:\Users\Shadow\Documents\TubeMailGorilla\TubeMailGorilla.Maui.Unlocked\Views\ContactDetailsPage.xaml.cs' -Pattern 'OnDownloadSnap|FileSavePicker|PickSaveFileAsync|FilePicker|Share\.Default|InitializeWithWindow|GetWindowHandle' | Select-Object LineNumber, Line
$out = @($hits | ForEach-Object { "$($_.LineNumber): $($_.Line)" }) -join "`r`n"
$out | Out-File -Encoding utf8 'c:\Users\Shadow\Documents\TubeMailGorilla\savepat.txt'
"wrote savepat.txt ($($hits.Count) hits)"
