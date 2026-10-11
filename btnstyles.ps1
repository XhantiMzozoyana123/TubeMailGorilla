$ErrorActionPreference = 'Continue'
$hits = Select-String -Path 'c:\Users\Shadow\Documents\TubeMailGorilla\TubeMailGorilla.Maui.Unlocked\Resources\Styles\*.xaml', 'c:\Users\Shadow\Documents\TubeMailGorilla\TubeMailGorilla.Maui.Unlocked\App.xaml' -Pattern 'SecondaryButtonStyle|PrimaryButtonStyle|CompactButtonStyle' | Select-Object Filename, LineNumber, Line
$out = @($hits | ForEach-Object { "$($_.Filename):$($_.LineNumber): $($_.Line)" }) -join "`r`n"
if ([string]::IsNullOrWhiteSpace($out)) { $out = 'NO HITS - check App.xaml resources' }
$out | Out-File -Encoding utf8 'c:\Users\Shadow\Documents\TubeMailGorilla\btnstyles.txt'
'wrote btnstyles.txt'
