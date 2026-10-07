$p = 'c:\Users\Shadow\Documents\TubeMailGorilla\TubeMailGorilla.Maui.Unlocked\Services\LLMService.cs'
$lines = [IO.File]::ReadAllLines($p)
for ($i = 505; $i -lt 600 -and $i -lt $lines.Count; $i++) {
  Write-Output ('{0}: {1}' -f ($i + 1), $lines[$i])
}
