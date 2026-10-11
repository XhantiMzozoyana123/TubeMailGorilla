param()
$p = 'C:/Users/Shadow/Documents/TubeMailGorilla/TubeMailGorilla.Maui.Unlocked/Services/EmailService.cs'
$raw = Get-Content $p -Raw
$lines = $raw.Split([Environment]::NewLine)
$idx = 0
foreach ($l in $lines) {
    if ($l -match 'ContainsSnapshotRandomToken|PersonalizeSnapshotRandom|ContainsSnapshotIndexToken|PersonalizeSnapshotIndexed|ContainsSnapshotAiToken|PersonalizeSnapshotAiAsync') {
        Write-Output ('LINE ' + $idx + ' : ' + $l)
    }
    $idx++
}
