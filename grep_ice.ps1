$root = 'c:\Users\Shadow\Documents\TubeMailGorilla'
$r = @()
$r += '=== error string ==='
Get-ChildItem $root -Recurse -Include *.cs,*.xaml -File -ErrorAction SilentlyContinue |
    Where-Object { $_.FullName -notlike '*\bin\*' -and $_.FullName -notlike '*\obj\*' } |
    Select-String -Pattern 'timed out or could not be reached' -SimpleMatch |
    ForEach-Object { $r += ($_.Path + ':' + $_.LineNumber + ': ' + $_.Line.Trim()) }

$r += '=== icebreaker refs ==='
Get-ChildItem $root -Recurse -Include *.cs -File -ErrorAction SilentlyContinue |
    Where-Object { $_.FullName -notlike '*\bin\*' -and $_.FullName -notlike '*\obj\*' -and $_.FullName -notlike '*\Tools\*' } |
    Select-String -Pattern 'icebreaker' |
    ForEach-Object { $r += ($_.Path + ':' + $_.LineNumber + ': ' + $_.Line.Trim()) }

$r | Set-Content (Join-Path $root 'grep_ice_44.txt') -Encoding utf8
Write-Output ('lines=' + $r.Count)
