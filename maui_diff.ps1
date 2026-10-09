$root = 'c:\Users\Shadow\Documents\TubeMailGorilla'
$o = @()
$o += '=== Maui/Services/EmailService.cs diff ==='
$d = git -C $root diff -- 'TubeMailGorilla.Maui/Services/EmailService.cs'
if (-not $d) { $o += '(no diff)' } else { $o += $d }
$o += '=== does Unlocked EmailService have f_name in HEAD? ==='
$h = git -C $root show 'HEAD:TubeMailGorilla.Maui.Unlocked/Services/EmailService.cs'
$o += ('head f_name count=' + (($h | Select-String 'f_name').Count))
$o += ('head has PersonalizeSnapshotIndexed=' + (($h | Select-String 'PersonalizeSnapshotIndexed').Count))
$o += ('head has ContainsSnapshotIndexToken=' + (($h | Select-String 'ContainsSnapshotIndexToken').Count))
$o | Set-Content ($root + '\maui_diff_28.txt') -Encoding utf8
Write-Output 'DONE'
