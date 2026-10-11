$ErrorActionPreference = 'Continue'
$out = @()
$out += "=== EmailContact.cs lines: $((Get-Content 'c:\Users\Shadow\Documents\TubeMailGorilla\TubeMailGorilla.Maui.Unlocked\Models\EmailContact.cs').Count) ==="
$out += ((Select-String -Path 'c:\Users\Shadow\Documents\TubeMailGorilla\TubeMailGorilla.Maui.Unlocked\Models\EmailContact.cs' -Pattern 'public .*\{ get;') | ForEach-Object { "$($_.LineNumber): $($_.Line.Trim())" } -join "`r`n")
$out += ''
$out += "=== DatabaseService contact methods ==="
$out += ((Select-String -Path 'c:\Users\Shadow\Documents\TubeMailGorilla\TubeMailGorilla.Maui.Unlocked\Services\DatabaseService.cs' -Pattern 'public async Task.*Contact') | ForEach-Object { "$($_.LineNumber): $($_.Line.Trim())" } -join "`r`n")
$out | Out-File -Encoding utf8 'c:\Users\Shadow\Documents\TubeMailGorilla\modelcheck.txt'
'wrote modelcheck.txt'
