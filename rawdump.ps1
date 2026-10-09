$f = 'C:\Users\Shadow\Documents\TubeMailGorilla\TubeMailGorilla.Maui.Unlocked\Services\ComfyUiImageGenerationService.cs'
$raw = [System.IO.File]::ReadAllText($f)
$o = @()
$o += ('len=' + $raw.Length)
$o += ('has LoraZero=' + $raw.Contains('LoraZero'))
$i = $raw.IndexOf('class_type')
$o += ('first class_type at=' + $i)
if ($i -ge 0) {
  $seg = $raw.Substring([Math]::Max(0,$i-40), 160)
  $o += '--- raw segment (escaped) ---'
  $o += ($seg -replace "`r",'\r' -replace "`n",'\n' -replace "`t",'\t')
}
$o += '--- has CRLF=' + ($raw -match "`r`n")
$o | Set-Content 'C:\Users\Shadow\Documents\TubeMailGorilla\rawdump_21.txt' -Encoding utf8
Write-Output 'RAW DONE'
