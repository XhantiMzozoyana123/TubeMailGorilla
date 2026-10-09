$f = 'c:\Users\Shadow\Documents\TubeMailGorilla\TubeMailGorilla.Maui.Unlocked\Services\EmailService.cs'
$o = @()
Select-String -Path $f -Pattern 'public (static|sealed|class)|private static readonly Regex|SnapshotIndex' | ForEach-Object {
  $o += ($_.LineNumber.ToString() + ': ' + $_.Line.Trim())
}
$o | Set-Content 'c:\Users\Shadow\Documents\TubeMailGorilla\emails_members_26.txt' -Encoding utf8
Write-Output 'DONE'
