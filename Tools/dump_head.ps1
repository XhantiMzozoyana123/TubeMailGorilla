$head = [IO.File]::ReadAllLines([IO.Path]::Combine($env:TEMP, 'llm_head.cs'))
$start = -1
for ($i = 0; $i -lt $head.Count; $i++) {
  if ($head[$i] -match 'private async Task<string> RunVisionInferenceAsync') { $start = $i; break }
}
Write-Output ('START=' + ($start + 1))
for ($i = $start; $i -lt $start + 60 -and $i -lt $head.Count; $i++) {
  Write-Output (('{0}: {1}' -f ($i + 1), $head[$i]))
}
