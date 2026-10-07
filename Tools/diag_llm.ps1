$p = 'c:\Users\Shadow\Documents\TubeMailGorilla\TubeMailGorilla.Maui.Unlocked\Services\LLMService.cs'
$t = [IO.File]::ReadAllText($p)
Write-Output ('HAS_MTMD=' + $t.Contains('_mtmdParameters'))
Write-Output ('HAS_MARKER=' + $t.Contains('mediaMarker'))
Write-Output ('HAS_LOADMEDIA=' + $t.Contains('LoadMediaAsync'))
Write-Output ('HAS_ORIG_IF=' + $t.Contains('if (base64Images is { Count: > 0 }'))
Write-Output ('LINES=' + ($t -split "`n").Count)
