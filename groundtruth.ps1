$f = 'C:\Users\Shadow\Documents\TubeMailGorilla\TubeMailGorilla.Maui.Unlocked\Services\ComfyUiImageGenerationService.cs'
$raw = [System.IO.File]::ReadAllText($f)
$lines = $raw -split "`r`n"
$o = @()
$o += ('FILE=' + $f)
$o += ('len=' + $raw.Length)
$o += ('contains LoraZero=' + $raw.Contains('LoraZero'))
$o += ('contains CLIPTextEncode=' + $raw.Contains('CLIPTextEncode'))
$o += ('contains strength_model=' + $raw.Contains('strength_model'))
$o += '--- lines matching class_type ---'
for ($i = 0; $i -lt $lines.Length; $i++) {
  if ($lines[$i] -match 'class_type') { $o += (($i+1).ToString() + ': ' + $lines[$i].Trim()) }
}
$o += '--- lines matching positive|negative|model\" ---'
for ($i = 0; $i -lt $lines.Length; $i++) {
  if ($lines[$i] -match '\["(positive|negative|model|clip)"\]') { $o += (($i+1).ToString() + ': ' + $lines[$i].Trim()) }
}
$stamp = Get-Date -Format 'HHmmss_fff'
$o | Set-Content ("C:\Users\Shadow\Documents\TubeMailGorilla\groundtruth_" + $stamp + ".txt") -Encoding utf8
Write-Output ("GROUNDTRUTH " + $stamp)
